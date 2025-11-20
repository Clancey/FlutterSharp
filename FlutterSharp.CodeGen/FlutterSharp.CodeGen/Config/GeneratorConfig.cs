using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Main configuration class for the FlutterSharp code generator.
	/// </summary>
	public record GeneratorConfig
	{
		/// <summary>
		/// Gets the Flutter SDK configuration.
		/// </summary>
		public FlutterSdkConfig FlutterSdk { get; init; } = new();

		/// <summary>
		/// Gets the output paths configuration.
		/// </summary>
		public OutputPathsConfig OutputPaths { get; init; } = new();

		/// <summary>
		/// Gets the filter configuration for what to include/exclude.
		/// </summary>
		public FiltersConfig Filters { get; init; } = new();

		/// <summary>
		/// Gets the third-party package sources configuration.
		/// </summary>
		public ThirdPartyPackagesConfig ThirdPartyPackages { get; init; } = new();

		/// <summary>
		/// Gets the code generation options.
		/// </summary>
		public GenerationOptionsConfig GenerationOptions { get; init; } = new();

		/// <summary>
		/// Gets the type mapping overrides.
		/// </summary>
		public TypeMappingConfig TypeMapping { get; init; } = new();

		/// <summary>
		/// Gets the advanced generator options.
		/// </summary>
		public AdvancedOptionsConfig AdvancedOptions { get; init; } = new();
	}

	/// <summary>
	/// Configuration for Flutter SDK management.
	/// </summary>
	public record FlutterSdkConfig
	{
		/// <summary>
		/// Gets the Flutter SDK mode: "auto", "local", or "clone".
		/// </summary>
		public string Mode { get; init; } = "auto";

		/// <summary>
		/// Gets the path to a local Flutter SDK installation (when Mode is "local").
		/// </summary>
		public string? Path { get; init; }

		/// <summary>
		/// Gets the specific Flutter version to use or clone (e.g., "3.16.0", "stable", "master").
		/// </summary>
		public string? Version { get; init; }

		/// <summary>
		/// Gets the directory where Flutter SDK should be cloned (when Mode is "clone").
		/// </summary>
		public string? CloneDirectory { get; init; }

		/// <summary>
		/// Gets the Git repository URL for cloning Flutter SDK.
		/// </summary>
		public string GitRepository { get; init; } = "https://github.com/flutter/flutter.git";

		/// <summary>
		/// Gets whether to use Flutter from PATH environment variable.
		/// </summary>
		public bool UseSystemFlutter { get; init; } = true;
	}

	/// <summary>
	/// Configuration for output file paths.
	/// </summary>
	public record OutputPathsConfig
	{
		/// <summary>
		/// Gets the base output directory for generated files.
		/// </summary>
		public string BaseDirectory { get; init; } = "./Generated";

		/// <summary>
		/// Gets the output directory for C# bindings.
		/// </summary>
		public string CSharpOutput { get; init; } = "./Generated/CSharp";

		/// <summary>
		/// Gets the output directory for Dart interop code.
		/// </summary>
		public string DartOutput { get; init; } = "./Generated/Dart";

		/// <summary>
		/// Gets the output directory for documentation files.
		/// </summary>
		public string? DocumentationOutput { get; init; }

		/// <summary>
		/// Gets the output directory for analysis artifacts.
		/// </summary>
		public string? AnalysisOutput { get; init; }

		/// <summary>
		/// Gets whether to organize output by package.
		/// </summary>
		public bool OrganizeByPackage { get; init; } = true;

		/// <summary>
		/// Gets whether to clean output directories before generation.
		/// </summary>
		public bool CleanBeforeGenerate { get; init; } = false;
	}

	/// <summary>
	/// Configuration for filtering what gets generated.
	/// </summary>
	public record FiltersConfig
	{
		/// <summary>
		/// Gets the list of packages to include (empty means all).
		/// </summary>
		public List<string> IncludePackages { get; init; } = new();

		/// <summary>
		/// Gets the list of packages to exclude.
		/// </summary>
		public List<string> ExcludePackages { get; init; } = new();

		/// <summary>
		/// Gets the list of widget name patterns to include (supports wildcards).
		/// </summary>
		public List<string> IncludeWidgets { get; init; } = new();

		/// <summary>
		/// Gets the list of widget name patterns to exclude (supports wildcards).
		/// </summary>
		public List<string> ExcludeWidgets { get; init; } = new();

		/// <summary>
		/// Gets the list of type name patterns to include (supports wildcards).
		/// </summary>
		public List<string> IncludeTypes { get; init; } = new();

		/// <summary>
		/// Gets the list of type name patterns to exclude (supports wildcards).
		/// </summary>
		public List<string> ExcludeTypes { get; init; } = new();

		/// <summary>
		/// Gets the list of namespaces to include.
		/// </summary>
		public List<string> IncludeNamespaces { get; init; } = new();

		/// <summary>
		/// Gets the list of namespaces to exclude.
		/// </summary>
		public List<string> ExcludeNamespaces { get; init; } = new();

		/// <summary>
		/// Gets whether to include internal/private APIs.
		/// </summary>
		public bool IncludeInternalApis { get; init; } = false;

		/// <summary>
		/// Gets whether to include deprecated APIs.
		/// </summary>
		public bool IncludeDeprecated { get; init; } = true;

		/// <summary>
		/// Gets whether to include experimental/preview APIs.
		/// </summary>
		public bool IncludeExperimental { get; init; } = false;

		/// <summary>
		/// Gets the minimum stability level to include (alpha, beta, stable).
		/// </summary>
		public string MinimumStability { get; init; } = "stable";
	}

	/// <summary>
	/// Configuration for third-party package sources.
	/// </summary>
	public record ThirdPartyPackagesConfig
	{
		/// <summary>
		/// Gets whether to enable third-party package discovery.
		/// </summary>
		public bool Enabled { get; init; } = false;

		/// <summary>
		/// Gets the list of pub.dev packages to include.
		/// </summary>
		public List<PubPackageSource> PubDevPackages { get; init; } = new();

		/// <summary>
		/// Gets the list of Git repository packages to include.
		/// </summary>
		public List<GitPackageSource> GitPackages { get; init; } = new();

		/// <summary>
		/// Gets the list of local file system packages to include.
		/// </summary>
		public List<LocalPackageSource> LocalPackages { get; init; } = new();

		/// <summary>
		/// Gets the cache directory for downloaded packages.
		/// </summary>
		public string CacheDirectory { get; init; } = "./.cache/packages";

		/// <summary>
		/// Gets whether to cache downloaded packages.
		/// </summary>
		public bool EnableCaching { get; init; } = true;

		/// <summary>
		/// Gets the cache expiration time in hours.
		/// </summary>
		public int CacheExpirationHours { get; init; } = 24;
	}

	/// <summary>
	/// Represents a package from pub.dev.
	/// </summary>
	public record PubPackageSource
	{
		/// <summary>
		/// Gets the package name on pub.dev.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the package version (or version constraint).
		/// </summary>
		public string? Version { get; init; }

		/// <summary>
		/// Gets whether this package is enabled.
		/// </summary>
		public bool Enabled { get; init; } = true;
	}

	/// <summary>
	/// Represents a package from a Git repository.
	/// </summary>
	public record GitPackageSource
	{
		/// <summary>
		/// Gets the Git repository URL.
		/// </summary>
		public string Url { get; init; } = string.Empty;

		/// <summary>
		/// Gets the Git reference (branch, tag, or commit).
		/// </summary>
		public string? Ref { get; init; }

		/// <summary>
		/// Gets the subdirectory path within the repository.
		/// </summary>
		public string? Path { get; init; }

		/// <summary>
		/// Gets the package name (defaults to repository name).
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// Gets whether this package is enabled.
		/// </summary>
		public bool Enabled { get; init; } = true;
	}

	/// <summary>
	/// Represents a package from the local file system.
	/// </summary>
	public record LocalPackageSource
	{
		/// <summary>
		/// Gets the path to the package directory.
		/// </summary>
		public string Path { get; init; } = string.Empty;

		/// <summary>
		/// Gets the package name (defaults to directory name).
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// Gets whether this package is enabled.
		/// </summary>
		public bool Enabled { get; init; } = true;
	}

	/// <summary>
	/// Configuration for code generation options.
	/// </summary>
	public record GenerationOptionsConfig
	{
		/// <summary>
		/// Gets the namespace prefix for generated C# code.
		/// </summary>
		public string NamespacePrefix { get; init; } = "FlutterSharp";

		/// <summary>
		/// Gets whether to generate XML documentation comments.
		/// </summary>
		public bool GenerateDocumentation { get; init; } = true;

		/// <summary>
		/// Gets whether to generate nullable reference type annotations.
		/// </summary>
		public bool GenerateNullableAnnotations { get; init; } = true;

		/// <summary>
		/// Gets whether to generate async/await patterns.
		/// </summary>
		public bool GenerateAsyncPatterns { get; init; } = true;

		/// <summary>
		/// Gets whether to generate extension methods.
		/// </summary>
		public bool GenerateExtensionMethods { get; init; } = true;

		/// <summary>
		/// Gets whether to generate builder patterns for complex objects.
		/// </summary>
		public bool GenerateBuilderPatterns { get; init; } = false;

		/// <summary>
		/// Gets whether to generate immutable types (records).
		/// </summary>
		public bool GenerateImmutableTypes { get; init; } = true;

		/// <summary>
		/// Gets whether to generate ToString() overrides.
		/// </summary>
		public bool GenerateToString { get; init; } = true;

		/// <summary>
		/// Gets whether to generate Equals/GetHashCode overrides.
		/// </summary>
		public bool GenerateEquality { get; init; } = false;

		/// <summary>
		/// Gets whether to generate debug attributes.
		/// </summary>
		public bool GenerateDebuggerAttributes { get; init; } = true;

		/// <summary>
		/// Gets the C# language version to target (e.g., "10", "11", "12").
		/// </summary>
		public string CSharpVersion { get; init; } = "12";

		/// <summary>
		/// Gets whether to use file-scoped namespaces.
		/// </summary>
		public bool UseFileScopedNamespaces { get; init; } = true;

		/// <summary>
		/// Gets whether to use global usings.
		/// </summary>
		public bool UseGlobalUsings { get; init; } = false;

		/// <summary>
		/// Gets the line ending style ("lf", "crlf", "auto").
		/// </summary>
		public string LineEnding { get; init; } = "auto";

		/// <summary>
		/// Gets the indentation style ("tabs" or "spaces").
		/// </summary>
		public string IndentationStyle { get; init; } = "tabs";

		/// <summary>
		/// Gets the number of spaces per indentation level (when using spaces).
		/// </summary>
		public int IndentationSize { get; init; } = 4;
	}

	/// <summary>
	/// Configuration for type mapping overrides.
	/// </summary>
	public record TypeMappingConfig
	{
		/// <summary>
		/// Gets custom Dart to C# type mappings.
		/// </summary>
		public Dictionary<string, string> CustomMappings { get; init; } = new();

		/// <summary>
		/// Gets whether to use C# collection types (List, Dictionary) instead of interfaces.
		/// </summary>
		public bool UseConcreteCollections { get; init; } = true;

		/// <summary>
		/// Gets whether to map Dart dynamic to C# object.
		/// </summary>
		public bool MapDynamicToObject { get; init; } = true;

		/// <summary>
		/// Gets whether to use C# tuples for Dart tuples.
		/// </summary>
		public bool UseTuples { get; init; } = true;

		/// <summary>
		/// Gets whether to use C# records for Dart data classes.
		/// </summary>
		public bool UseRecords { get; init; } = true;

		/// <summary>
		/// Gets the default numeric type for untyped numbers (int, long, double).
		/// </summary>
		public string DefaultNumericType { get; init; } = "double";

		/// <summary>
		/// Gets whether to use System.Text.Json attributes.
		/// </summary>
		public bool UseSystemTextJson { get; init; } = true;

		/// <summary>
		/// Gets whether to generate JSON converters.
		/// </summary>
		public bool GenerateJsonConverters { get; init; } = false;
	}

	/// <summary>
	/// Configuration for advanced generator options.
	/// </summary>
	public record AdvancedOptionsConfig
	{
		/// <summary>
		/// Gets whether to enable parallel generation.
		/// </summary>
		public bool ParallelGeneration { get; init; } = true;

		/// <summary>
		/// Gets the maximum degree of parallelism.
		/// </summary>
		public int MaxDegreeOfParallelism { get; init; } = -1; // -1 means use default

		/// <summary>
		/// Gets whether to enable incremental generation (skip unchanged files).
		/// </summary>
		public bool IncrementalGeneration { get; init; } = false;

		/// <summary>
		/// Gets whether to enable caching of analysis results.
		/// </summary>
		public bool EnableCaching { get; init; } = true;

		/// <summary>
		/// Gets the cache directory for analysis results.
		/// </summary>
		public string CacheDirectory { get; init; } = "./.cache/analysis";

		/// <summary>
		/// Gets whether to validate generated code.
		/// </summary>
		public bool ValidateGeneratedCode { get; init; } = false;

		/// <summary>
		/// Gets whether to format generated code.
		/// </summary>
		public bool FormatGeneratedCode { get; init; } = true;

		/// <summary>
		/// Gets the path to dotnet format tool (or null to use default).
		/// </summary>
		public string? DotNetFormatPath { get; init; }

		/// <summary>
		/// Gets the path to dart format tool (or null to use default).
		/// </summary>
		public string? DartFormatPath { get; init; }

		/// <summary>
		/// Gets whether to generate source maps for debugging.
		/// </summary>
		public bool GenerateSourceMaps { get; init; } = false;

		/// <summary>
		/// Gets the log level (Trace, Debug, Information, Warning, Error, Critical).
		/// </summary>
		public string LogLevel { get; init; } = "Information";

		/// <summary>
		/// Gets the log output path (null for console only).
		/// </summary>
		public string? LogOutputPath { get; init; }

		/// <summary>
		/// Gets whether to emit performance diagnostics.
		/// </summary>
		public bool EmitPerformanceDiagnostics { get; init; } = false;

		/// <summary>
		/// Gets the timeout for external tool execution in seconds.
		/// </summary>
		public int ToolExecutionTimeout { get; init; } = 300;

		/// <summary>
		/// Gets whether to continue on errors.
		/// </summary>
		public bool ContinueOnError { get; init; } = false;

		/// <summary>
		/// Gets the maximum number of errors before aborting.
		/// </summary>
		public int MaxErrors { get; init; } = 100;
	}
}
