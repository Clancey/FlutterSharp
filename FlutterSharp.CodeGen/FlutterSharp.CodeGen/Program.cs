using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlutterSharp.CodeGen.Analysis;
using FlutterSharp.CodeGen.Generators.CSharp;
using FlutterSharp.CodeGen.Generators.Dart;
using FlutterSharp.CodeGen.Models;
using FlutterSharp.CodeGen.Sources;
using FlutterSharp.CodeGen.TypeMapping;

namespace FlutterSharp.CodeGen;

/// <summary>
/// FlutterSharp Code Generator - Command-line tool for generating C# and Dart FFI code from Flutter SDK and packages.
/// </summary>
internal class Program
{
	private static bool _verbose;
	private static int _generatedFileCount;
	private static readonly object _consoleLock = new();
	private static readonly HashSet<string> _supportedParserCallbackTypes = new(StringComparer.Ordinal)
	{
		"VoidCallback",
		"GestureTapCallback",
		"GestureTapDownCallback",
		"GestureTapUpCallback",
		"GestureTapCancelCallback",
		"GestureTapMoveCallback",
		"GestureLongPressCallback",
		"GestureLongPressDownCallback",
		"GestureLongPressUpCallback",
		"GestureLongPressStartCallback",
		"GestureLongPressEndCallback",
		"GestureLongPressCancelCallback",
		"GestureLongPressMoveUpdateCallback",
		"GestureDragStartCallback",
		"GestureDragUpdateCallback",
		"GestureDragEndCallback",
		"GestureDragDownCallback",
		"GestureDragCancelCallback",
		"GestureScaleStartCallback",
		"GestureScaleUpdateCallback",
		"GestureScaleEndCallback",
		"GestureForcePressStartCallback",
		"GestureForcePressPeakCallback",
		"GestureForcePressUpdateCallback",
		"GestureForcePressEndCallback",
		"PointerDownEventListener",
		"PointerMoveEventListener",
		"PointerUpEventListener",
		"PointerCancelEventListener",
		"PointerEnterEventListener",
		"PointerExitEventListener",
		"PointerHoverEventListener",
		"PointerSignalEventListener",
		"PointerPanZoomStartEventListener",
		"PointerPanZoomUpdateEventListener",
		"PointerPanZoomEndEventListener",
		"PlatformViewCreatedCallback",
		"ShaderCallback"
	};

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
			getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "Generated", "Dart"));

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
		var sdkConfig = new FlutterSdkConfig
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

		// Initialize remaining mappers (registry and dartToCSharpMapper already created for validation)
		var csharpToDartMapper = new CSharpToDartFfiMapper(registry);

		// Initialize enricher (new architecture)
		var enricher = new WidgetAnalysisEnricher(dartToCSharpMapper, csharpToDartMapper, LogWarning);

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
		var dartStructGenerator = new DartStructGenerator(csharpToDartMapper);
		var dartParserGenerator = new DartParserGenerator(csharpToDartMapper, dartStructsDir);
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

		// Widgets that require Animation<T> parameters or special handling
		// These generate invalid parsers because FFI can't serialize Animation controllers
		var skipParserGeneration = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			// Transition widgets (require Animation<T> parameters)
			"AlignTransition", "DecoratedBoxTransition", "DefaultTextStyleTransition",
			"FadeTransition", "MatrixTransition", "PositionedTransition",
			"RelativePositionedTransition", "RotationTransition", "ScaleTransition",
			"SizeTransition", "SlideTransition", "SliverFadeTransition",
			// Animated builders (require Listenable/Animation parameters)
			"AnimatedBuilder", "AnimatedModalBarrier", "DualTransitionBuilder",
			"TweenAnimationBuilder", "ValueListenableBuilder",
			// Widgets with complex required parameters that can't be FFI serialized
			"EditableText", "FadeInImage", "FlutterLogo", "Image", "RawImage",
			"KeyboardListener", "RawKeyboardListener", "UndoHistory",
			"ListWheelViewport", "ShrinkWrappingViewport", "Viewport",
			"PrimaryScrollController", "WidgetsApp", "Table",
			// Platform-specific widgets
			"ImgElementPlatformView", "RawWebImage", "SystemContextMenu",
			"AndroidView", "HtmlElementView",
			// Widgets with delegate parameters
			"AnnotatedRegion", "SelectionContainer",
			// Widgets with positional-only arguments or special constructors
			"Icon", "ImageIcon", "Text", "RichText", "Dismissible",
			"LayoutId", "ListWheelScrollView", "PageView", "Semantics",
			"SliverConstrainedCrossAxis", "SliverSafeArea", "SliverVisibility", "Wrap",
			// Widgets with complex callback types that can't be FFI serialized
			"ActionListener", "AnimatedGrid", "AnimatedList", "AnimatedSwitcher",
			"Builder", "ConstraintsTransformBox", "DraggableScrollableSheet",
			"Expansible", "Focus", "FocusScope", "LayoutBuilder", "ListenableBuilder",
			"MouseRegion", "NavigatorPopHandler", "NotificationListener",
			"OrientationBuilder", "OverlayPortal", "PlatformViewLink", "PopScope",
			"RawMenuAnchor", "ReorderableList", "ShaderMask",
			"SliverAnimatedGrid", "SliverAnimatedList", "SliverLayoutBuilder",
			"SliverReorderableList", "SliverVariedExtentList", "StatefulBuilder",
			"TapRegion", "TextFieldTapRegion", "TreeSliver", "WillPopScope",
			// Widgets with HitTestBehavior type mismatch
			"SingleChildScrollView"
		};

		// Generate widgets
		LogInfo("");
		LogInfo("Generating widget code...");
		// Widgets that should be completely excluded from generation
		// (platform-specific, problematic struct mappings, or edge-case widgets)
		var excludeWidgets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			// Platform-specific widgets that don't exist on all platforms
			"UiKitView", "AppKitView",
			// Widgets with struct mapping issues (child property not matching)
			"SliverCrossAxisExpanded", "NestedScrollViewViewport",
			// Widgets with routing/navigation types that aren't defined in C#
			"WidgetsApp"
		};

		var runtimeSkippedParsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Deduplicate widgets by choosing the most complete definition for each name.
		var widgetsToGenerate = packageDefinition.Widgets
			.Where(w => !w.Name.StartsWith("_") && !excludeWidgets.Contains(w.Name))
			.GroupBy(w => w.Name, StringComparer.OrdinalIgnoreCase)
			.Select(SelectPreferredWidgetDefinition)
			.ToList();

		foreach (var widget in widgetsToGenerate)
		{
			LogVerbose($"  Generating {widget.Name}...");

			// Enrich widget with all generation metadata (new architecture)
			var enrichedWidget = enricher.Enrich(widget);

			// C# Widget - use enriched data
			var csharpWidgetCode = csharpWidgetGenerator.Generate(enrichedWidget);
			await File.WriteAllTextAsync(
				Path.Combine(csharpWidgetsDir, $"{widget.Name}.cs"),
				csharpWidgetCode,
				cancellationToken);
			_generatedFileCount++;

			// C# Struct
			var csharpStructCode = csharpStructGenerator.Generate(widget);
			await File.WriteAllTextAsync(
				Path.Combine(csharpStructsDir, $"{widget.Name}Struct.cs"),
				csharpStructCode,
				cancellationToken);
			_generatedFileCount++;

			// Skip abstract widgets for Dart code
			if (!enrichedWidget.IsAbstract)
			{
				// Dart Struct - use enriched data
				var dartStructCode = dartStructGenerator.Generate(enrichedWidget);
				await File.WriteAllTextAsync(
					Path.Combine(dartStructsDir, $"{enrichedWidget.Name.ToLowerInvariant()}_struct.dart"),
					dartStructCode,
					cancellationToken);
				_generatedFileCount++;

				// Dart Parser - skip known unsupported widgets and callback signatures.
				var shouldSkipParser = skipParserGeneration.Contains(enrichedWidget.Name)
					|| ShouldSkipParserGeneration(widget, enrichedWidget);
				if (!shouldSkipParser)
				{
					var dartParserCode = dartParserGenerator.Generate(enrichedWidget);
					await File.WriteAllTextAsync(
						Path.Combine(dartParsersDir, $"{enrichedWidget.Name.ToLowerInvariant()}_parser.dart"),
						dartParserCode,
						cancellationToken);
					_generatedFileCount++;
				}
				else
				{
					runtimeSkippedParsers.Add(enrichedWidget.Name);
					LogVerbose($"  Skipping parser generation for {enrichedWidget.Name} (unsupported parser surface)");
				}
			}
		}

		// Generate important non-widget type structs
		LogInfo("");
		LogInfo("Generating type struct code...");

		// Whitelist of important Flutter types that need struct generation
		var importantTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			// Alignment types
			"Alignment", "AlignmentGeometry", "AlignmentDirectional",

			// Padding/Insets types
			"EdgeInsets", "EdgeInsetsGeometry", "EdgeInsetsDirectional",

			// Color types
			"Color", "MaterialColor", "ColorSwatch",

			// Decoration types
			"BoxDecoration", "Decoration", "ShapeDecoration",

			// Constraints
			"BoxConstraints", "Constraints",

			// Text styling
			"TextStyle", "TextAlign", "TextDirection", "FontWeight", "FontStyle",

			// Geometry types
			"Size", "Offset", "Rect", "Radius",

			// Border types
			"BorderRadius", "BorderRadiusGeometry", "BorderRadiusDirectional",
			"Border", "BorderSide",

			// Shadow types
			"BoxShadow", "Shadow",

			// Gradient types
			"Gradient", "LinearGradient", "RadialGradient", "SweepGradient",

			// Matrix types
			"Matrix4", "Matrix3",

			// Time/Animation types
			"Duration", "DateTime",
			"Animation", "AnimationController", "Curve", "Curves",

			// Image types
			"ImageProvider",

			// Keys and State
			"Key", "GlobalKey", "State",

			// Theme and Material
			"ThemeData", "IconData", "MaterialStateProperty",

			// Controllers and Focus
			"FocusNode", "ScrollController", "ScrollPhysics",
			"TextEditingController",

			// BuildContext (special case - may not generate correctly but included for completeness)
			"BuildContext"
		};

		var typesToGenerate = packageDefinition.Types
			.Where(t => importantTypes.Contains(t.Name))
			.ToList();

		LogInfo($"Found {typesToGenerate.Count} important types to generate structs for");

		foreach (var type in typesToGenerate)
		{
			LogVerbose($"  Generating {type.Name}...");

			// Convert TypeDefinition to WidgetDefinition for struct generation
			var widgetDef = ConvertTypeToWidget(type);

			// C# Struct
			var csharpStructCode = csharpStructGenerator.Generate(widgetDef);
			await File.WriteAllTextAsync(
				Path.Combine(csharpStructsDir, $"{type.Name}Struct.cs"),
				csharpStructCode,
				cancellationToken);
			_generatedFileCount++;

			// Define truly abstract types that are handled separately in AbstractInterfaceTypes.json
			var abstractInterfaceTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"AlignmentGeometry", "EdgeInsetsGeometry", "BorderRadiusGeometry",
				"Gradient", "Animation", "Decoration", "Constraints", "BoxBorder", "ParametricCurve"
			};

			// Skip private types and truly abstract types for Dart code
			// Important: We generate Dart structs for all important types EXCEPT the ones in abstractInterfaceTypes
			if (!type.Name.StartsWith("_") && !abstractInterfaceTypes.Contains(type.Name))
			{
				// Dart Struct
				var dartStructCode = dartStructGenerator.Generate(widgetDef);
				await File.WriteAllTextAsync(
					Path.Combine(dartStructsDir, $"{type.Name.ToLowerInvariant()}_struct.dart"),
					dartStructCode,
					cancellationToken);
				_generatedFileCount++;
			}
		}

		// Generate Dart core type structs (Duration, DateTime, etc.)
		LogInfo("");
		LogInfo("Generating Dart core type structs...");

		var dartCoreTypesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ManualTypes", "DartCoreTypes.json");
		if (File.Exists(dartCoreTypesPath))
		{
			var dartCoreTypesJson = await File.ReadAllTextAsync(dartCoreTypesPath, cancellationToken);
			var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var dartCoreTypesData = System.Text.Json.JsonSerializer.Deserialize<DartCoreTypesDefinition>(dartCoreTypesJson, options);

			if (dartCoreTypesData?.Types != null)
			{
				LogInfo($"Found {dartCoreTypesData.Types.Count} Dart core types to generate");

				foreach (var coreType in dartCoreTypesData.Types)
				{
					LogVerbose($"  Generating {coreType.Name}...");

					// Convert to WidgetDefinition for struct generation
					var widgetDef = ConvertCoreTypeToWidget(coreType);

					// C# Struct
					var csharpStructCode = csharpStructGenerator.Generate(widgetDef);
					await File.WriteAllTextAsync(
						Path.Combine(csharpStructsDir, $"{coreType.Name}Struct.cs"),
						csharpStructCode,
						cancellationToken);
					_generatedFileCount++;

					// Dart Struct
					var dartStructCode = dartStructGenerator.Generate(widgetDef);
					await File.WriteAllTextAsync(
						Path.Combine(dartStructsDir, $"{coreType.Name.ToLowerInvariant()}_struct.dart"),
						dartStructCode,
						cancellationToken);
					_generatedFileCount++;
				}
			}
		}
		else
		{
			LogVerbose($"No Dart core types file found at {dartCoreTypesPath}");
		}

		// Generate Dart UI types (Size, Offset, Rect, Radius, etc.)
		LogInfo("");
		LogInfo("Generating Dart UI types...");

		var dartUITypesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ManualTypes", "DartUITypes.json");
		if (File.Exists(dartUITypesPath))
		{
			var dartUITypesJson = await File.ReadAllTextAsync(dartUITypesPath, cancellationToken);
			var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var dartUITypesData = System.Text.Json.JsonSerializer.Deserialize<DartCoreTypesDefinition>(dartUITypesJson, options);

			if (dartUITypesData?.Types != null)
			{
				LogInfo($"Found {dartUITypesData.Types.Count} Dart UI types to generate");

				foreach (var uiType in dartUITypesData.Types)
				{
					LogVerbose($"  Generating {uiType.Name}...");

					// Convert to WidgetDefinition for struct generation
					var widgetDef = ConvertCoreTypeToWidget(uiType);

					// C# Struct
					var csharpStructCode = csharpStructGenerator.Generate(widgetDef);
					await File.WriteAllTextAsync(
						Path.Combine(csharpStructsDir, $"{uiType.Name}Struct.cs"),
						csharpStructCode,
						cancellationToken);
					_generatedFileCount++;

					// Dart Struct
					var dartStructCode = dartStructGenerator.Generate(widgetDef);
					await File.WriteAllTextAsync(
						Path.Combine(dartStructsDir, $"{uiType.Name.ToLowerInvariant()}_struct.dart"),
						dartStructCode,
						cancellationToken);
					_generatedFileCount++;
				}
			}
		}
		else
		{
			LogVerbose($"No Dart UI types file found at {dartUITypesPath}");
		}

		// Generate abstract interface type structs (empty structs for abstract base classes)
		LogInfo("");
		LogInfo("Generating abstract interface type structs...");

		var abstractInterfaceTypesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ManualTypes", "AbstractInterfaceTypes.json");
		if (File.Exists(abstractInterfaceTypesPath))
		{
			var abstractInterfaceTypesJson = await File.ReadAllTextAsync(abstractInterfaceTypesPath, cancellationToken);
			var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var abstractInterfaceTypesData = System.Text.Json.JsonSerializer.Deserialize<DartCoreTypesDefinition>(abstractInterfaceTypesJson, options);

			if (abstractInterfaceTypesData?.Types != null)
			{
				LogInfo($"Found {abstractInterfaceTypesData.Types.Count} abstract interface types to generate");

				foreach (var interfaceType in abstractInterfaceTypesData.Types)
				{
					LogVerbose($"  Generating {interfaceType.Name}...");

					// Convert to WidgetDefinition for struct generation
					var widgetDef = ConvertCoreTypeToWidget(interfaceType);

					// Dart Struct only (no C# struct needed for abstract interfaces)
					var dartStructCode = dartStructGenerator.Generate(widgetDef);
					await File.WriteAllTextAsync(
						Path.Combine(dartStructsDir, $"{interfaceType.Name.ToLowerInvariant()}_struct.dart"),
						dartStructCode,
						cancellationToken);
					_generatedFileCount++;
				}
			}
		}
		else
		{
			LogVerbose($"No abstract interface types file found at {abstractInterfaceTypesPath}");
		}

		// Generate enums
		LogInfo("");
		LogInfo("Generating enum code...");
		foreach (var enumDef in packageDefinition.Enums)
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
		// Combine excluded widgets and skip parser generation sets for parser registration
		var allExcludedParsers = new HashSet<string>(skipParserGeneration, StringComparer.OrdinalIgnoreCase);
		foreach (var excluded in excludeWidgets)
		{
			allExcludedParsers.Add(excluded);
		}
		foreach (var runtimeSkipped in runtimeSkippedParsers)
		{
			allExcludedParsers.Add(runtimeSkipped);
		}
		var parserRegistrationCode = dartParserImportsGenerator.Generate(packageDefinition.Widgets, allExcludedParsers);
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
		var utilsDartPath = Path.Combine(outputDart, "utils.dart");
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

		if (File.Exists(utilsDartPath))
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
			LogVerbose($"utils.dart not found at {utilsDartPath}, generating all parsers");
		}

		// Generate missing parsers
		var utilityParserCode = dartUtilityParserGenerator.GenerateAll(packageDefinition.Enums, packageDefinition.Types, existingParsers);
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
		LogInfo($"Generated files: {_generatedFileCount}");
		LogInfo($"C# output: {outputCSharp}");
		LogInfo($"  - Widgets: {packageDefinition.Widgets.Count} files");
		LogInfo($"  - Structs: {packageDefinition.Widgets.Count} files");
		LogInfo($"  - Enums: {packageDefinition.Enums.Count} files");
		LogInfo($"Dart output: {outputDart}");
		LogInfo($"  - Structs: {packageDefinition.Widgets.Count} files");
		LogInfo($"  - Parsers: {packageDefinition.Widgets.Count} files");
		LogInfo($"  - Enums: {packageDefinition.Enums.Count} files");
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
		var sdkConfig = new FlutterSdkConfig
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
		var sdkConfig = new FlutterSdkConfig
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

		LogInfo("");
		LogInfo($"Package: {packageDefinition.Name} v{packageDefinition.Version}");
		if (!string.IsNullOrEmpty(packageDefinition.Description))
		{
			LogInfo($"Description: {packageDefinition.Description}");
		}

		LogInfo("");
		LogInfo($"Widgets ({packageDefinition.Widgets.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var widget in packageDefinition.Widgets.OrderBy(w => w.Name))
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
		LogInfo($"Types ({packageDefinition.Types.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var type in packageDefinition.Types.OrderBy(t => t.Name).Take(10))
		{
			LogInfo($"  {type.Name} ({type.Properties.Count} properties)");
		}

		if (packageDefinition.Types.Count > 10)
		{
			LogInfo($"  ... and {packageDefinition.Types.Count - 10} more");
		}

		LogInfo("");
		LogInfo($"Enums ({packageDefinition.Enums.Count}):");
		LogInfo("-".PadRight(80, '-'));

		foreach (var enumDef in packageDefinition.Enums.OrderBy(e => e.Name))
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
		LogInfo("=".PadRight(80, '='));
		LogInfo($"Total items: {packageDefinition.Widgets.Count + packageDefinition.Types.Count + packageDefinition.Enums.Count}");
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

	private static bool ShouldSkipParserGeneration(WidgetDefinition widget, EnrichedWidgetDefinition enrichedWidget)
	{
		var sourceLibrary = widget.SourceLibrary ?? enrichedWidget.SourceLibrary ?? string.Empty;

		// Material/Cupertino widgets introduce large callback/typedef surfaces not yet mapped safely.
		if (sourceLibrary.Contains("material", StringComparison.OrdinalIgnoreCase)
			|| sourceLibrary.Contains("cupertino", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		return HasUnsupportedCallbackSignature(enrichedWidget);
	}

	private static bool HasUnsupportedCallbackSignature(EnrichedWidgetDefinition enrichedWidget)
	{
		foreach (var property in enrichedWidget.AllProperties)
		{
			if (!property.IsCallback)
			{
				continue;
			}

			var callbackType = (property.DartType ?? string.Empty).Trim();
			if (callbackType.EndsWith("?", StringComparison.Ordinal))
			{
				callbackType = callbackType.Substring(0, callbackType.Length - 1).Trim();
			}

			if (string.IsNullOrEmpty(callbackType))
			{
				return true;
			}

			// Literal function signatures are not fully mapped yet; avoid generating invalid closures.
			if (callbackType.Contains("Function(", StringComparison.Ordinal))
			{
				return true;
			}

			if (callbackType.StartsWith("ValueChanged<", StringComparison.Ordinal)
				|| callbackType.StartsWith("FormFieldValidator", StringComparison.Ordinal))
			{
				continue;
			}

			if (!_supportedParserCallbackTypes.Contains(callbackType))
			{
				return true;
			}
		}

		return false;
	}

	private static WidgetDefinition SelectPreferredWidgetDefinition(IGrouping<string, WidgetDefinition> definitions)
	{
		// Prefer concrete definitions with richer metadata when duplicate widget names exist
		// across Flutter libraries.
		return definitions
			.OrderBy(w => w.IsAbstract ? 1 : 0)
			.ThenBy(w => string.IsNullOrWhiteSpace(w.SourceLibrary) ? 1 : 0)
			.ThenByDescending(w => w.Properties?.Count ?? 0)
			.First();
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

	/// <summary>
	/// Converts a DartCoreType to a WidgetDefinition for struct generation purposes.
	/// </summary>
	private static WidgetDefinition ConvertCoreTypeToWidget(DartCoreType coreType)
	{
		return new WidgetDefinition
		{
			Name = coreType.Name,
			Namespace = coreType.Namespace,
			BaseClass = null, // Core types don't have base classes in our model
			Type = WidgetType.Stateless,
			Properties = coreType.Properties,
			Constructors = new List<ConstructorDefinition>(),
			Documentation = coreType.Documentation,
			SourceLibrary = coreType.SourceLibrary,
			HasSingleChild = false,
			HasMultipleChildren = false,
			IsAbstract = coreType.IsAbstract,
			IsDeprecated = false,
			DeprecationMessage = null
		};
	}
}

/// <summary>
/// Definition for manually defined Dart core types that need struct generation.
/// </summary>
internal class DartCoreTypesDefinition
{
	public List<DartCoreType> Types { get; set; } = new();
}

/// <summary>
/// Represents a Dart core type (like Duration, DateTime) that needs struct generation.
/// </summary>
internal class DartCoreType
{
	public string Name { get; set; } = string.Empty;
	public string Namespace { get; set; } = string.Empty;
	public string? Documentation { get; set; }
	public List<PropertyDefinition> Properties { get; set; } = new();
	public bool IsAbstract { get; set; }
	public string? SourceLibrary { get; set; }
}
