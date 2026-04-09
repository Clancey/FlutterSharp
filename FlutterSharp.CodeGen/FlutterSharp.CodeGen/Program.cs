using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlutterSharp.CodeGen.Analysis;
using FlutterSharp.CodeGen.Config;
using FlutterSharp.CodeGen.Generators.CSharp;
using FlutterSharp.CodeGen.Generators.Dart;
using FlutterSharp.CodeGen.Models;
using FlutterSharp.CodeGen.Sources;
using FlutterSharp.CodeGen.TypeMapping;
using SdkConfig = FlutterSharp.CodeGen.Sources.FlutterSdkConfig;

namespace FlutterSharp.CodeGen;

/// <summary>
/// FlutterSharp Code Generator - Command-line tool for generating C# and Dart FFI code from Flutter SDK and packages.
/// </summary>
internal class Program
{
	private static bool _verbose;
	private static int _generatedFileCount;
	private static readonly object _consoleLock = new();

	static async Task<int> Main(string[] args)
	{
		var rootCommand = new RootCommand("FlutterSharp Code Generator - Generate C# and Dart FFI code from Flutter SDK and packages")
		{
			CreateGenerateCommand(),
			CreateValidateCommand(),
			CreateListWidgetsCommand()
		};

		return await rootCommand.InvokeAsync(args);
	}

	/// <summary>
	/// Creates the 'generate' command.
	/// </summary>
	private static Command CreateGenerateCommand()
	{
		var command = new Command("generate", "Generate code from Flutter SDK or packages");

		// Source options
		var sourceOption = new Option<string>(
			name: "--source",
			description: "Source type: 'sdk', 'package', or 'local'",
			getDefaultValue: () => "sdk");

		var flutterSdkPathOption = new Option<string?>(
			name: "--flutter-sdk-path",
			description: "Flutter SDK path (auto-detect if not provided)");

		var packageNameOption = new Option<string?>(
			name: "--package-name",
			description: "Package name for pub packages (e.g., 'provider')");

		var packageVersionOption = new Option<string?>(
			name: "--package-version",
			description: "Package version (e.g., '^6.0.0')");

		var packagePathOption = new Option<string?>(
			name: "--package-path",
			description: "Local package path");

		// Output options
		var outputCSharpOption = new Option<string?>(
			name: "--output-csharp",
			description: "C# output directory",
			getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "Generated", "CSharp"));

		var outputDartOption = new Option<string?>(
			name: "--output-dart",
			description: "Dart output directory",
			getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "flutter_module", "lib"));

		// Filtering options
		var includeOption = new Option<string?>(
			name: "--include",
			description: "Comma-separated list of types to include");

		var excludeOption = new Option<string?>(
			name: "--exclude",
			description: "Comma-separated list of types to exclude");

		// General options
		var verboseOption = new Option<bool>(
			name: "--verbose",
			description: "Verbose logging",
			getDefaultValue: () => false);

		var updateSdkOption = new Option<bool>(
			name: "--update-sdk",
			description: "Update Flutter SDK to latest release",
			getDefaultValue: () => false);

		command.AddOption(sourceOption);
		command.AddOption(flutterSdkPathOption);
		command.AddOption(packageNameOption);
		command.AddOption(packageVersionOption);
		command.AddOption(packagePathOption);
		command.AddOption(outputCSharpOption);
		command.AddOption(outputDartOption);
		command.AddOption(includeOption);
		command.AddOption(excludeOption);
		command.AddOption(verboseOption);
		command.AddOption(updateSdkOption);

		command.SetHandler(async (context) =>
		{
			var source = context.ParseResult.GetValueForOption(sourceOption)!;
			var flutterSdkPath = context.ParseResult.GetValueForOption(flutterSdkPathOption);
			var packageName = context.ParseResult.GetValueForOption(packageNameOption);
			var packageVersion = context.ParseResult.GetValueForOption(packageVersionOption);
			var packagePath = context.ParseResult.GetValueForOption(packagePathOption);
			var outputCSharp = context.ParseResult.GetValueForOption(outputCSharpOption);
			var outputDart = context.ParseResult.GetValueForOption(outputDartOption);
			var include = context.ParseResult.GetValueForOption(includeOption);
			var exclude = context.ParseResult.GetValueForOption(excludeOption);
			var verbose = context.ParseResult.GetValueForOption(verboseOption);
			var updateSdk = context.ParseResult.GetValueForOption(updateSdkOption);

			_verbose = verbose;

			try
			{
				await GenerateCodeAsync(
					source,
					flutterSdkPath,
					packageName,
					packageVersion,
					packagePath,
					outputCSharp!,
					outputDart!,
					include,
					exclude,
					updateSdk,
					context.GetCancellationToken());

				context.ExitCode = 0;
			}
			catch (Exception ex)
			{
				LogError($"Generation failed: {ex.Message}");
				if (verbose)
				{
					LogError(ex.ToString());
				}
				context.ExitCode = 1;
			}
		});

		return command;
	}

	/// <summary>
	/// Creates the 'validate' command.
	/// </summary>
	private static Command CreateValidateCommand()
	{
		var command = new Command("validate", "Validate setup (Dart tools, Flutter SDK)");

		var flutterSdkPathOption = new Option<string?>(
			name: "--flutter-sdk-path",
			description: "Flutter SDK path (auto-detect if not provided)");

		var verboseOption = new Option<bool>(
			name: "--verbose",
			description: "Verbose logging",
			getDefaultValue: () => false);

		command.AddOption(flutterSdkPathOption);
		command.AddOption(verboseOption);

		command.SetHandler(async (context) =>
		{
			var flutterSdkPath = context.ParseResult.GetValueForOption(flutterSdkPathOption);
			var verbose = context.ParseResult.GetValueForOption(verboseOption);

			_verbose = verbose;

			try
			{
				await ValidateSetupAsync(flutterSdkPath, context.GetCancellationToken());
				context.ExitCode = 0;
			}
			catch (Exception ex)
			{
				LogError($"Validation failed: {ex.Message}");
				if (verbose)
				{
					LogError(ex.ToString());
				}
				context.ExitCode = 1;
			}
		});

		return command;
	}

	/// <summary>
	/// Creates the 'list-widgets' command.
	/// </summary>
	private static Command CreateListWidgetsCommand()
	{
		var command = new Command("list-widgets", "List widgets that would be generated (dry run)");

		var sourceOption = new Option<string>(
			name: "--source",
			description: "Source type: 'sdk', 'package', or 'local'",
			getDefaultValue: () => "sdk");

		var flutterSdkPathOption = new Option<string?>(
			name: "--flutter-sdk-path",
			description: "Flutter SDK path (auto-detect if not provided)");

		var packageNameOption = new Option<string?>(
			name: "--package-name",
			description: "Package name for pub packages (e.g., 'provider')");

		var packagePathOption = new Option<string?>(
			name: "--package-path",
			description: "Local package path");

		var includeOption = new Option<string?>(
			name: "--include",
			description: "Comma-separated list of types to include");

		var excludeOption = new Option<string?>(
			name: "--exclude",
			description: "Comma-separated list of types to exclude");

		var verboseOption = new Option<bool>(
			name: "--verbose",
			description: "Verbose logging",
			getDefaultValue: () => false);

		command.AddOption(sourceOption);
		command.AddOption(flutterSdkPathOption);
		command.AddOption(packageNameOption);
		command.AddOption(packagePathOption);
		command.AddOption(includeOption);
		command.AddOption(excludeOption);
		command.AddOption(verboseOption);

		command.SetHandler(async (context) =>
		{
			var source = context.ParseResult.GetValueForOption(sourceOption)!;
			var flutterSdkPath = context.ParseResult.GetValueForOption(flutterSdkPathOption);
			var packageName = context.ParseResult.GetValueForOption(packageNameOption);
			var packagePath = context.ParseResult.GetValueForOption(packagePathOption);
			var include = context.ParseResult.GetValueForOption(includeOption);
			var exclude = context.ParseResult.GetValueForOption(excludeOption);
			var verbose = context.ParseResult.GetValueForOption(verboseOption);

			_verbose = verbose;

			try
			{
				await ListWidgetsAsync(
					source,
					flutterSdkPath,
					packageName,
					packagePath,
					include,
					exclude,
					context.GetCancellationToken());

				context.ExitCode = 0;
			}
			catch (Exception ex)
			{
				LogError($"Failed to list widgets: {ex.Message}");
				if (verbose)
				{
					LogError(ex.ToString());
				}
				context.ExitCode = 1;
			}
		});

		return command;
	}

	/// <summary>
	/// Generates code from Flutter SDK or packages.
	/// </summary>
	private static async Task GenerateCodeAsync(
		string source,
		string? flutterSdkPath,
		string? packageName,
		string? packageVersion,
		string? packagePath,
		string outputCSharp,
		string outputDart,
		string? include,
		string? exclude,
		bool updateSdk,
		CancellationToken cancellationToken)
	{
		LogInfo("=".PadRight(80, '='));
		LogInfo("FlutterSharp Code Generator");
		LogInfo("=".PadRight(80, '='));

		// Initialize Flutter SDK Manager
		var sdkConfig = new SdkConfig
		{
			Mode = flutterSdkPath != null ? "local" : "auto",
			LocalPath = flutterSdkPath
		};

		var sdkManager = new FlutterSdkManager(sdkConfig, LogVerbose);
		var sdkPath = await sdkManager.EnsureFlutterSdkAsync(cancellationToken);

		// Update SDK if requested
		if (updateSdk)
		{
			LogInfo("Updating Flutter SDK to latest release...");
			await sdkManager.UpdateFlutterSdkAsync(sdkPath, cancellationToken);
		}

		// Determine package path to analyze
		string analysisPath = source.ToLowerInvariant() switch
		{
			"sdk" => sdkManager.GetFlutterPackageRoot(sdkPath),
			"package" => await ResolvePackagePathAsync(packageName, packageVersion, cancellationToken),
			"local" => packagePath ?? throw new InvalidOperationException("--package-path is required for local source"),
			_ => throw new InvalidOperationException($"Invalid source type: {source}")
		};

		LogInfo($"Analyzing package at: {analysisPath}");

		// Initialize Dart Analyzer Host
		var analyzerScriptPath = FindAnalyzerScript();
		var analyzerHost = new DartAnalyzerHost(analyzerScriptPath);

		// Validate Dart tools
		LogInfo("Validating Dart tools...");
		await analyzerHost.ValidateDartToolsAsync(cancellationToken);

		// Parse include/exclude lists
		var includeList = ParseList(include);
		var excludeList = ParseList(exclude);

		// Analyze package
		LogInfo("Analyzing package...");
		var packageDefinition = await analyzerHost.AnalyzePackageAsync(
			analysisPath,
			includeList,
			excludeList,
			cancellationToken);

		LogInfo($"Analysis complete:");
		LogInfo($"  Package: {packageDefinition.Name} v{packageDefinition.Version}");
		LogInfo($"  Widgets: {packageDefinition.Widgets.Count}");
		LogInfo($"  Types: {packageDefinition.Types.Count}");
		LogInfo($"  Enums: {packageDefinition.Enums.Count}");
		LogInfo($"  Typedefs: {packageDefinition.Typedefs.Count}");

		// Initialize type mappers early for validation
		var registry = new TypeMappingRegistry();
		var dartToCSharpMapper = new DartToCSharpMapper(registry);

		// Validate package before generation
		LogInfo("");
		LogInfo("Validating package data...");
		var validationService = new ValidationService(dartToCSharpMapper, LogWarning, LogVerbose);

		// Validate output paths are writable
		var pathValidation = validationService.ValidateOutputPaths(outputCSharp, outputDart);
		if (!pathValidation.IsValid)
		{
			foreach (var error in pathValidation.Errors)
			{
				LogError(error.ToString());
			}
			throw new ValidationException("Output path validation failed", pathValidation);
		}

		// Validate package definition
		var packageValidation = validationService.ValidatePackage(packageDefinition);

		// Log warnings
		if (packageValidation.HasWarnings)
		{
			LogWarning($"Validation completed with {packageValidation.Warnings.Count} warning(s):");
			foreach (var warning in packageValidation.Warnings.Take(20))
			{
				LogVerbose($"  {warning}");
			}
			if (packageValidation.Warnings.Count > 20)
			{
				LogVerbose($"  ... and {packageValidation.Warnings.Count - 20} more warnings");
			}
		}

		// Check for blocking errors
		if (!packageValidation.IsValid)
		{
			foreach (var error in packageValidation.Errors)
			{
				LogError(error.ToString());
			}
			throw new ValidationException($"Package validation failed with {packageValidation.Errors.Count} error(s)", packageValidation);
		}

		LogSuccess($"Validation passed ({packageValidation.Warnings.Count} warnings)");

		LogInfo("");
		LogInfo("Building UI surface manifest...");
		var uiSurfaceManifest = BuildUiSurfaceManifest(packageDefinition);
		LogInfo($"  Widgets: {uiSurfaceManifest.Widgets.Count}");
		LogInfo($"  Types: {uiSurfaceManifest.Types.Count}");
		LogInfo($"  Enums: {uiSurfaceManifest.Enums.Count}");
		LogInfo($"  Typedefs: {uiSurfaceManifest.Typedefs.Count}");
		LogInfo($"  Exclusions: {uiSurfaceManifest.Exclusions.Count}");

		var persistedManifest = await PersistAndReloadUiSurfaceManifestAsync(uiSurfaceManifest, outputCSharp, cancellationToken);
		uiSurfaceManifest = persistedManifest.Manifest;
		LogInfo($"Persisted UI surface manifest: {persistedManifest.Path}");

		// Initialize remaining mappers (registry and dartToCSharpMapper already created for validation)
		var csharpToDartMapper = new CSharpToDartFfiMapper(registry);
		var generatedDartStructNames = new HashSet<string>(
			uiSurfaceManifest.Widgets.Where(widget => widget.GenerateDartStruct).Select(widget => widget.Name),
			StringComparer.OrdinalIgnoreCase);
		foreach (var typeName in uiSurfaceManifest.Types.Where(type => type.GenerateDartStruct).Select(type => type.Name))
		{
			generatedDartStructNames.Add(typeName);
		}
		generatedDartStructNames.Add("Widget");

		// Initialize enricher (new architecture)
		var enricher = new WidgetAnalysisEnricher(dartToCSharpMapper, csharpToDartMapper, LogWarning, generatedDartStructNames);

		var csharpWidgetsDir = Path.Combine(outputCSharp, "Widgets");
		var csharpStructsDir = Path.Combine(outputCSharp, "Structs");
		var csharpEnumsDir = Path.Combine(outputCSharp, "Enums");
		var dartStructsDir = Path.Combine(outputDart, "structs");
		var dartParsersDir = Path.Combine(outputDart, "parsers");
		var dartEnumsDir = Path.Combine(outputDart, "enums");

		// Pass template paths to generators
		var csharpWidgetTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "CSharpWidget.scriban");
		var csharpStructTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "CSharpStruct.scriban");
		var csharpWidgetGenerator = new CSharpWidgetGenerator(dartToCSharpMapper, csharpWidgetTemplatePath, LogWarning);
		var csharpStructGenerator = new CSharpStructGenerator(dartToCSharpMapper, csharpStructTemplatePath, LogWarning);
		var csharpEnumGenerator = new CSharpEnumGenerator(dartToCSharpMapper);
		var dartStructGenerator = new DartStructGenerator(csharpToDartMapper, generatedDartStructNames);
		var dartParserGenerator = new DartParserGenerator(csharpToDartMapper, outputDart);
		var dartParserImportsGenerator = new DartParserImportsGenerator();
		var dartUtilityParserGenerator = new DartUtilityParserGenerator(csharpToDartMapper);
		var dartEnumGenerator = new DartEnumGenerator();

		// Create output directories
		Directory.CreateDirectory(outputCSharp);
		Directory.CreateDirectory(outputDart);

		Directory.CreateDirectory(csharpWidgetsDir);
		Directory.CreateDirectory(csharpStructsDir);
		Directory.CreateDirectory(csharpEnumsDir);
		Directory.CreateDirectory(dartStructsDir);
		Directory.CreateDirectory(dartParsersDir);
		Directory.CreateDirectory(dartEnumsDir);

		_generatedFileCount = 0;

		// Generate widgets
		LogInfo("");
		LogInfo("Generating widget code...");
		var widgetsToGenerate = uiSurfaceManifest.Widgets
			.OrderBy(w => w.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();

		foreach (var widget in widgetsToGenerate)
		{
			LogVerbose($"  Generating {widget.Name}...");

			// Enrich widget with all generation metadata (new architecture)
			var enrichedWidget = enricher.Enrich(widget);

			if (widget.GenerateCSharpWidget)
			{
				// C# Widget - use enriched data
				var csharpWidgetCode = csharpWidgetGenerator.Generate(enrichedWidget);
				await File.WriteAllTextAsync(
					Path.Combine(csharpWidgetsDir, $"{widget.Name}.cs"),
					csharpWidgetCode,
					cancellationToken);
				_generatedFileCount++;
			}

			if (widget.GenerateCSharpStruct)
			{
				// C# Struct
				var csharpStructCode = csharpStructGenerator.Generate(widget);
				await File.WriteAllTextAsync(
					Path.Combine(csharpStructsDir, $"{widget.Name}Struct.cs"),
					csharpStructCode,
					cancellationToken);
				_generatedFileCount++;
			}

			if (widget.GenerateDartStruct)
			{
				// Dart Struct - use enriched data
				var dartStructCode = dartStructGenerator.Generate(enrichedWidget);
				await File.WriteAllTextAsync(
					Path.Combine(dartStructsDir, $"{enrichedWidget.Name.ToLowerInvariant()}_struct.dart"),
					dartStructCode,
					cancellationToken);
				_generatedFileCount++;
			}

			if (widget.GenerateDartParser)
			{
				var dartParserCode = dartParserGenerator.Generate(enrichedWidget);
				await File.WriteAllTextAsync(
					Path.Combine(dartParsersDir, $"{enrichedWidget.Name.ToLowerInvariant()}_parser.dart"),
					dartParserCode,
					cancellationToken);
				_generatedFileCount++;
			}
		}

		LogInfo("");
		LogInfo("Generating type struct code...");
		foreach (var type in uiSurfaceManifest.Types)
		{
			LogVerbose($"  Generating {type.Name}...");

			var widgetDef = ConvertTypeToWidget(type);

			if (type.GenerateCSharpStruct)
			{
				var csharpStructCode = csharpStructGenerator.Generate(widgetDef);
				await File.WriteAllTextAsync(
					Path.Combine(csharpStructsDir, $"{type.Name}Struct.cs"),
					csharpStructCode,
					cancellationToken);
				_generatedFileCount++;
			}

			if (type.GenerateDartStruct)
			{
				var dartStructCode = dartStructGenerator.Generate(widgetDef);
				await File.WriteAllTextAsync(
					Path.Combine(dartStructsDir, $"{type.Name.ToLowerInvariant()}_struct.dart"),
					dartStructCode,
					cancellationToken);
				_generatedFileCount++;
			}
		}

		// Generate enums
		LogInfo("");
		LogInfo("Generating enum code...");
		foreach (var enumDef in uiSurfaceManifest.Enums)
		{
			LogVerbose($"  Generating {enumDef.Name}...");

			// C# Enum
			var csharpEnumCode = csharpEnumGenerator.Generate(enumDef);
			await File.WriteAllTextAsync(
				Path.Combine(csharpEnumsDir, $"{enumDef.Name}.cs"),
				csharpEnumCode,
				cancellationToken);
			_generatedFileCount++;

			// Skip private enums for Dart code (starting with _)
			if (!enumDef.Name.StartsWith("_"))
			{
				// Dart Enum
				var dartEnumCode = dartEnumGenerator.Generate(enumDef);
				await File.WriteAllTextAsync(
					Path.Combine(dartEnumsDir, $"{enumDef.Name.ToLowerInvariant()}.dart"),
					dartEnumCode,
					cancellationToken);
				_generatedFileCount++;
			}
		}

		// Generate parser registration file (imports + list)
		LogInfo("");
		LogInfo("Generating parser registration file...");
		var allExcludedParsers = new HashSet<string>(
			uiSurfaceManifest.Widgets
				.Where(widget => !widget.GenerateDartParser)
				.Select(widget => widget.Name),
			StringComparer.OrdinalIgnoreCase);
		var parserRegistrationCode = dartParserImportsGenerator.Generate(uiSurfaceManifest.Widgets, allExcludedParsers);
		await File.WriteAllTextAsync(
			Path.Combine(outputDart, "generated_parsers.dart"),
			parserRegistrationCode,
			cancellationToken);
		_generatedFileCount++;
		LogVerbose($"  Generated generated_parsers.dart");

		// Generate utility parser functions
		LogInfo("");
		LogInfo("Generating utility parser functions...");

		// Read existing parsers from utils.dart
		var utilsDartPath = ResolveFlutterModuleLibFile(outputDart, "utils.dart");
		var manualUtilityParsers = new HashSet<string>(StringComparer.Ordinal)
		{
			"parseBlendMode",
			"parseBoxFit",
			"parseBrightness",
			"parseClip",
			"parseFlexFit",
			"parseHitTestBehavior",
			"parseMaterialTapTargetSize",
			"parseScrollViewKeyboardDismissBehavior",
			"parseTextCapitalization"
		};
		var existingParsers = new HashSet<string>(StringComparer.Ordinal);

		if (utilsDartPath != null)
		{
			var utilsContent = await File.ReadAllTextAsync(utilsDartPath, cancellationToken);
			var lines = utilsContent.Split('\n');
			foreach (var line in lines)
			{
				// Match single-argument manual parser definitions like:
				// BlendMode? parseBlendMode(dynamic value) {
				var match = System.Text.RegularExpressions.Regex.Match(
					line,
					@"^\s*[\w<>,\?\s]+\s+(\w+)\s*\(([^)]*)\)");
				if (!match.Success)
				{
					continue;
				}

				var parserName = match.Groups[1].Value;
				if (!manualUtilityParsers.Contains(parserName))
				{
					continue;
				}

				var parameters = match.Groups[2].Value;
				if (parameters.Contains(',') || parameters.Contains("Map<", StringComparison.Ordinal))
				{
					continue;
				}

				existingParsers.Add(parserName);
			}
			LogInfo($"Found {existingParsers.Count} existing parser functions in utils.dart");
		}
		else
		{
			LogVerbose($"utils.dart not found relative to {outputDart}, generating all parsers");
		}

		// Generate missing parsers
		var utilityParserCode = dartUtilityParserGenerator.GenerateAll(uiSurfaceManifest.Enums, uiSurfaceManifest.Types, existingParsers);
		await File.WriteAllTextAsync(
			Path.Combine(outputDart, "generated_utility_parsers.dart"),
			utilityParserCode,
			cancellationToken);
		_generatedFileCount++;
		LogInfo($"Generated utility parsers (generated_utility_parsers.dart)");

		// Generate summary
		LogInfo("");
		LogInfo("=".PadRight(80, '='));
		LogInfo("Generation Complete!");
		LogInfo("=".PadRight(80, '='));
		var generatedCSharpWidgetCount = uiSurfaceManifest.Widgets.Count(widget => widget.GenerateCSharpWidget);
		var generatedCSharpStructCount = uiSurfaceManifest.Widgets.Count(widget => widget.GenerateCSharpStruct)
			+ uiSurfaceManifest.Types.Count(type => type.GenerateCSharpStruct);
		var generatedDartStructCount = uiSurfaceManifest.Widgets.Count(widget => widget.GenerateDartStruct)
			+ uiSurfaceManifest.Types.Count(type => type.GenerateDartStruct);
		var generatedDartParserCount = uiSurfaceManifest.Widgets.Count(widget => widget.GenerateDartParser);
		var generatedDartEnumCount = uiSurfaceManifest.Enums.Count(enumDefinition => !enumDefinition.Name.StartsWith("_", StringComparison.Ordinal));
		LogInfo($"Generated files: {_generatedFileCount}");
		LogInfo($"C# output: {outputCSharp}");
		LogInfo($"  - Widgets: {generatedCSharpWidgetCount} files");
		LogInfo($"  - Structs: {generatedCSharpStructCount} files");
		LogInfo($"  - Enums: {uiSurfaceManifest.Enums.Count} files");
		LogInfo($"Dart output: {outputDart}");
		LogInfo($"  - Structs: {generatedDartStructCount} files");
		LogInfo($"  - Parsers: {generatedDartParserCount} files");
		LogInfo($"  - Enums: {generatedDartEnumCount} files");
		LogInfo($"UI surface exclusions: {uiSurfaceManifest.Exclusions.Count}");
		LogInfo("=".PadRight(80, '='));

		// Report unmapped types if any
		var unmappedReport = dartToCSharpMapper.GetUnmappedTypesReport();
		if (unmappedReport.TotalCount > 0)
		{
			LogInfo("");
			LogWarning($"Type Mapping Summary: {unmappedReport.TotalCount} type(s) required fallback handling");
			LogInfo($"  - Inferred from parameter names: {unmappedReport.InferredCount}");
			LogInfo($"  - Failed (IntPtr fallback): {unmappedReport.FailedCount}");

			if (unmappedReport.FailedCount > 0 && _verbose)
			{
				LogInfo("");
				LogInfo(unmappedReport.ToString());
			}
			else if (unmappedReport.FailedCount > 0)
			{
				LogInfo("  Run with --verbose for detailed unmapped type report");
			}
		}
	}

	private static UiSurfaceManifest BuildUiSurfaceManifest(PackageDefinition packageDefinition)
	{
		var (policy, policyPath) = UiSurfacePolicyLoader.LoadDefault();
		var manifestBuilder = new UiSurfaceManifestBuilder(LogWarning);
		return manifestBuilder.Build(packageDefinition, policy, policyPath);
	}

	private static async Task<(UiSurfaceManifest Manifest, string Path)> PersistAndReloadUiSurfaceManifestAsync(
		UiSurfaceManifest manifest,
		string outputCSharp,
		CancellationToken cancellationToken)
	{
		var manifestPath = ResolveUiSurfaceManifestPath(outputCSharp, manifest.Name);
		await UiSurfaceManifestStore.SaveAsync(manifest, manifestPath, cancellationToken);
		var persistedManifest = await UiSurfaceManifestStore.LoadAsync(manifestPath, cancellationToken);
		return (persistedManifest, manifestPath);
	}

	private static string ResolveUiSurfaceManifestPath(string outputCSharp, string packageName)
	{
		var outputDirectory = Path.GetFullPath(outputCSharp);
		var generatedDirectory = Directory.GetParent(outputDirectory)?.FullName ?? outputDirectory;
		var analysisDirectory = Path.Combine(generatedDirectory, "Analysis");
		return Path.Combine(analysisDirectory, $"{SanitizeFileName(packageName)}.ui-surface-manifest.json");
	}

	private static string SanitizeFileName(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "package";
		}

		var invalidChars = Path.GetInvalidFileNameChars();
		var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());
		sanitized = sanitized.Trim();
		return string.IsNullOrWhiteSpace(sanitized) ? "package" : sanitized;
	}

	private static string? ResolveFlutterModuleLibFile(string outputDart, string fileName)
	{
		var outputDirectory = Path.GetFullPath(outputDart);
		var directCandidate = Path.Combine(outputDirectory, fileName);
		if (File.Exists(directCandidate))
		{
			return directCandidate;
		}

		for (var current = new DirectoryInfo(outputDirectory); current != null; current = current.Parent)
		{
			var flutterModuleCandidate = Path.Combine(current.FullName, "flutter_module", "lib", fileName);
			if (File.Exists(flutterModuleCandidate))
			{
				return flutterModuleCandidate;
			}
		}

		return null;
	}

	/// <summary>
	/// Validates the setup (Dart tools, Flutter SDK).
	/// </summary>
	private static async Task ValidateSetupAsync(string? flutterSdkPath, CancellationToken cancellationToken)
	{
		LogInfo("=".PadRight(80, '='));
		LogInfo("Validating Setup");
		LogInfo("=".PadRight(80, '='));

		// Validate Dart tools
		LogInfo("Checking Dart installation...");
		var analyzerScriptPath = FindAnalyzerScript();
		var analyzerHost = new DartAnalyzerHost(analyzerScriptPath);

		try
		{
			await analyzerHost.ValidateDartToolsAsync(cancellationToken);
			LogSuccess("Dart tools validated successfully");
		}
		catch (Exception ex)
		{
			LogError($"Dart validation failed: {ex.Message}");
			throw;
		}

		// Validate Flutter SDK
		LogInfo("");
		LogInfo("Checking Flutter SDK...");
		var sdkConfig = new SdkConfig
		{
			Mode = flutterSdkPath != null ? "local" : "auto",
			LocalPath = flutterSdkPath
		};

		var sdkManager = new FlutterSdkManager(sdkConfig, LogVerbose);

		try
		{
			var sdkPath = await sdkManager.EnsureFlutterSdkAsync(cancellationToken);
			LogSuccess($"Flutter SDK found at: {sdkPath}");

			var packageRoot = sdkManager.GetFlutterPackageRoot(sdkPath);
			LogSuccess($"Flutter package root: {packageRoot}");
		}
		catch (Exception ex)
		{
			LogError($"Flutter SDK validation failed: {ex.Message}");
			throw;
		}

		LogInfo("");
		LogInfo("=".PadRight(80, '='));
		LogSuccess("All validations passed!");
		LogInfo("=".PadRight(80, '='));
	}

	/// <summary>
	/// Lists widgets that would be generated (dry run).
	/// </summary>
	private static async Task ListWidgetsAsync(
		string source,
		string? flutterSdkPath,
		string? packageName,
		string? packagePath,
		string? include,
		string? exclude,
		CancellationToken cancellationToken)
	{
		LogInfo("=".PadRight(80, '='));
		LogInfo("Widget List (Dry Run)");
		LogInfo("=".PadRight(80, '='));

		// Initialize Flutter SDK Manager
		var sdkConfig = new SdkConfig
		{
			Mode = flutterSdkPath != null ? "local" : "auto",
			LocalPath = flutterSdkPath
		};

		var sdkManager = new FlutterSdkManager(sdkConfig, LogVerbose);
		var sdkPath = await sdkManager.EnsureFlutterSdkAsync(cancellationToken);

		// Determine package path to analyze
		string analysisPath = source.ToLowerInvariant() switch
		{
			"sdk" => sdkManager.GetFlutterPackageRoot(sdkPath),
			"package" => await ResolvePackagePathAsync(packageName, null, cancellationToken),
			"local" => packagePath ?? throw new InvalidOperationException("--package-path is required for local source"),
			_ => throw new InvalidOperationException($"Invalid source type: {source}")
		};

		LogInfo($"Analyzing package at: {analysisPath}");

		// Initialize Dart Analyzer Host
		var analyzerScriptPath = FindAnalyzerScript();
		var analyzerHost = new DartAnalyzerHost(analyzerScriptPath);

		// Parse include/exclude lists
		var includeList = ParseList(include);
		var excludeList = ParseList(exclude);

		// Analyze package
		var packageDefinition = await analyzerHost.AnalyzePackageAsync(
			analysisPath,
			includeList,
			excludeList,
			cancellationToken);

		var uiSurfaceManifest = BuildUiSurfaceManifest(packageDefinition);

		LogInfo("");
		LogInfo($"Package: {packageDefinition.Name} v{packageDefinition.Version}");
		if (!string.IsNullOrEmpty(packageDefinition.Description))
		{
			LogInfo($"Description: {packageDefinition.Description}");
		}

		LogInfo("");
		LogInfo($"UI Surface Widgets ({uiSurfaceManifest.Widgets.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var widget in uiSurfaceManifest.Widgets.OrderBy(w => w.Name))
		{
			var typeInfo = widget.Type.ToString();
			var childInfo = widget.HasSingleChild ? " [single child]" :
							widget.HasMultipleChildren ? " [multiple children]" : "";
			var abstractInfo = widget.IsAbstract ? " [abstract]" : "";
			var deprecatedInfo = widget.IsDeprecated ? " [DEPRECATED]" : "";

			LogInfo($"  {widget.Name} ({typeInfo}){childInfo}{abstractInfo}{deprecatedInfo}");
			LogInfo($"    Properties: {widget.Properties.Count}");

			if (_verbose && widget.Properties.Any())
			{
				foreach (var prop in widget.Properties.Take(5))
				{
					var requiredMark = prop.IsRequired ? "[required] " : "";
					LogVerbose($"      - {requiredMark}{prop.Name}: {prop.DartType}");
				}

				if (widget.Properties.Count > 5)
				{
					LogVerbose($"      ... and {widget.Properties.Count - 5} more");
				}
			}
		}

		LogInfo("");
		LogInfo($"UI Support Types ({uiSurfaceManifest.Types.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var type in uiSurfaceManifest.Types.OrderBy(t => t.Name).Take(10))
		{
			LogInfo($"  {type.Name} ({type.Properties.Count} properties)");
		}

		if (uiSurfaceManifest.Types.Count > 10)
		{
			LogInfo($"  ... and {uiSurfaceManifest.Types.Count - 10} more");
		}

		LogInfo("");
		LogInfo($"Enums ({uiSurfaceManifest.Enums.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var enumDef in uiSurfaceManifest.Enums.OrderBy(e => e.Name))
		{
			LogInfo($"  {enumDef.Name} ({enumDef.Values.Count} values)");
			if (_verbose && enumDef.Values.Any())
			{
				foreach (var value in enumDef.Values.Take(5))
				{
					LogVerbose($"      - {value.Name}");
				}

				if (enumDef.Values.Count > 5)
				{
					LogVerbose($"      ... and {enumDef.Values.Count - 5} more");
				}
			}
		}

		LogInfo("");
		LogInfo($"Typedefs ({uiSurfaceManifest.Typedefs.Count}):");
		LogInfo("-".PadRight(80, '-'));
		foreach (var typedefDefinition in uiSurfaceManifest.Typedefs.OrderBy(t => t.Name).Take(10))
		{
			var kind = typedefDefinition.IsFunction ? "function" : "alias";
			LogInfo($"  {typedefDefinition.Name} ({kind})");
		}

		if (uiSurfaceManifest.Typedefs.Count > 10)
		{
			LogInfo($"  ... and {uiSurfaceManifest.Typedefs.Count - 10} more");
		}

		LogInfo("");
		LogInfo($"Exclusions ({uiSurfaceManifest.Exclusions.Count}):");
		LogInfo("-".PadRight(80, '-'));
		foreach (var exclusion in uiSurfaceManifest.Exclusions.Take(_verbose ? uiSurfaceManifest.Exclusions.Count : 15))
		{
			var referenceInfo = string.IsNullOrWhiteSpace(exclusion.ReferencedBy)
				? string.Empty
				: $" [referenced by {exclusion.ReferencedBy}]";
			LogInfo($"  {exclusion.Kind}: {exclusion.Name} - {exclusion.Reason}{referenceInfo}");
		}

		if (!_verbose && uiSurfaceManifest.Exclusions.Count > 15)
		{
			LogInfo($"  ... and {uiSurfaceManifest.Exclusions.Count - 15} more");
		}

		LogInfo("");
		LogInfo("=".PadRight(80, '='));
		LogInfo($"Included items: {uiSurfaceManifest.Widgets.Count + uiSurfaceManifest.Types.Count + uiSurfaceManifest.Enums.Count + uiSurfaceManifest.Typedefs.Count}");
		LogInfo("=".PadRight(80, '='));
	}

	/// <summary>
	/// Resolves a package path from a package name and version.
	/// </summary>
	private static Task<string> ResolvePackagePathAsync(string? packageName, string? packageVersion, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(packageName))
		{
			throw new InvalidOperationException("--package-name is required for package source");
		}

		// For now, assume the package is already in pub cache
		// In a real implementation, you would use 'pub get' or similar
		var pubCache = Environment.GetEnvironmentVariable("PUB_CACHE")
			?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".pub-cache");

		var packagePath = Path.Combine(pubCache, "hosted", "pub.dev", packageName);

		if (!Directory.Exists(packagePath))
		{
			throw new DirectoryNotFoundException(
				$"Package '{packageName}' not found in pub cache. Please run 'dart pub get' or 'flutter pub get' first.");
		}

		return Task.FromResult(packagePath);
	}

	/// <summary>
	/// Finds the Dart analyzer script.
	/// </summary>
	private static string FindAnalyzerScript()
	{
		// Try to find the analyzer script relative to the executable
		var baseDir = AppContext.BaseDirectory;
		var scriptPath = Path.Combine(baseDir, "Tools", "analyzer", "package_scanner.dart");

		if (File.Exists(scriptPath))
		{
			return scriptPath;
		}

		// Try relative to current directory
		scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Tools", "analyzer", "package_scanner.dart");

		if (File.Exists(scriptPath))
		{
			return scriptPath;
		}

		// Try going up directories (for development scenarios)
		var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (currentDir != null)
		{
			scriptPath = Path.Combine(currentDir.FullName, "Tools", "analyzer", "package_scanner.dart");
			if (File.Exists(scriptPath))
			{
				return scriptPath;
			}

			currentDir = currentDir.Parent;
		}

		throw new FileNotFoundException(
			"Could not find Dart analyzer script (package_scanner.dart). " +
			"Please ensure the Tools/analyzer directory is present.");
	}

	/// <summary>
	/// Parses a comma-separated list.
	/// </summary>
	private static List<string>? ParseList(string? input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return null;
		}

		return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
	}

	/// <summary>
	/// Logs an informational message.
	/// </summary>
	private static void LogInfo(string message)
	{
		lock (_consoleLock)
		{
			Console.WriteLine(message);
		}
	}

	/// <summary>
	/// Logs a verbose message (only if verbose mode is enabled).
	/// </summary>
	private static void LogVerbose(string message)
	{
		if (!_verbose)
		{
			return;
		}

		lock (_consoleLock)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
		}
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	private static void LogWarning(string message)
	{
		lock (_consoleLock)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"WARNING: {message}");
			Console.ForegroundColor = oldColor;
		}
	}

	/// <summary>
	/// Logs a success message.
	/// </summary>
	private static void LogSuccess(string message)
	{
		lock (_consoleLock)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
		}
	}

	/// <summary>
	/// Logs an error message.
	/// </summary>
	private static void LogError(string message)
	{
		lock (_consoleLock)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine($"ERROR: {message}");
			Console.ForegroundColor = oldColor;
		}
	}


	/// <summary>
	/// Converts a TypeDefinition to a WidgetDefinition for struct generation purposes.
	/// </summary>
	private static WidgetDefinition ConvertTypeToWidget(TypeDefinition type)
	{
		return new WidgetDefinition
		{
			Name = type.Name,
			Namespace = type.Namespace,
			BaseClass = type.BaseClass,
			Type = WidgetType.Stateless, // Default type for non-widget types
			Properties = type.Properties,
			Constructors = type.Constructors,
			Documentation = type.Documentation,
			SourceLibrary = type.SourceLibrary,
			HasSingleChild = false,
			HasMultipleChildren = false,
			IsAbstract = type.IsAbstract,
			IsDeprecated = type.IsDeprecated,
			DeprecationMessage = type.DeprecationMessage
		};
	}

}
