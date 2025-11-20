# DartAnalyzerHost

The `DartAnalyzerHost` class provides a C# API for analyzing Dart packages using the Dart analyzer script. It wraps the `package_scanner.dart` script and provides strongly-typed results using the `PackageDefinition` model.

## Prerequisites

- Dart SDK installed and available in PATH
- The `package_scanner.dart` script located at `Tools/analyzer/package_scanner.dart`
- Required Dart analyzer dependencies installed (via `dart pub get` in the analyzer directory)

## Basic Usage

### Validate Dart Tools

Before analyzing packages, validate that Dart tools are available:

```csharp
var analyzer = new DartAnalyzerHost();

try
{
    await analyzer.ValidateDartToolsAsync();
    Console.WriteLine("Dart tools are available!");
}
catch (DartToolsNotFoundException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Analyze a Package

```csharp
var analyzer = new DartAnalyzerHost();
var packagePath = "/path/to/flutter/package";

var result = await analyzer.AnalyzePackageAsync(packagePath);

Console.WriteLine($"Package: {result.Name} v{result.Version}");
Console.WriteLine($"Widgets: {result.Widgets.Count}");
Console.WriteLine($"Types: {result.Types.Count}");
Console.WriteLine($"Enums: {result.Enums.Count}");
```

### Analyze with Filters

Use include/exclude filters to analyze only specific types:

```csharp
var includeTypes = new List<string> { "Container", "Padding", "Center" };
var excludeTypes = new List<string> { "DeprecatedWidget" };

var result = await analyzer.AnalyzePackageAsync(
    packagePath,
    includeTypes,
    excludeTypes);
```

### Analyze a Single File

```csharp
var filePath = "/path/to/flutter/package/lib/widgets/my_widget.dart";
var result = await analyzer.AnalyzeFileAsync(filePath);

// Note: This analyzes the entire package but is a convenience method
// for when you want to analyze a file that's part of a package
```

## Advanced Usage

### Custom Configuration

```csharp
// Custom analyzer script path and timeout
var analyzer = new DartAnalyzerHost(
    analyzerScriptPath: "custom/path/to/script.dart",
    timeoutSeconds: 600); // 10 minutes

var result = await analyzer.AnalyzePackageAsync(packagePath);
```

### Custom Logging

Extend `DartAnalyzerHost` to integrate with your logging framework:

```csharp
public class CustomDartAnalyzer : DartAnalyzerHost
{
    private readonly ILogger<CustomDartAnalyzer> _logger;

    public CustomDartAnalyzer(ILogger<CustomDartAnalyzer> logger)
    {
        _logger = logger;
    }

    protected override void LogDebug(string message)
    {
        _logger.LogDebug(message);
    }

    protected override void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }
}
```

### Cancellation Support

All async methods support cancellation tokens:

```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    var result = await analyzer.AnalyzePackageAsync(
        packagePath,
        cancellationToken: cts.Token);
}
catch (DartAnalyzerException ex) when (ex.Message.Contains("cancelled"))
{
    Console.WriteLine("Analysis was cancelled");
}
```

## Error Handling

The class throws specific exceptions for different error scenarios:

### DartToolsNotFoundException

Thrown when Dart SDK or the analyzer script is not found:

```csharp
try
{
    await analyzer.ValidateDartToolsAsync();
}
catch (DartToolsNotFoundException ex)
{
    // Dart is not installed or analyzer script not found
    Console.WriteLine($"Setup error: {ex.Message}");
}
```

### DartAnalyzerException

Thrown when analysis fails:

```csharp
try
{
    var result = await analyzer.AnalyzePackageAsync(packagePath);
}
catch (DartAnalyzerException ex)
{
    // Analysis failed - check inner exception for details
    Console.WriteLine($"Analysis error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Caused by: {ex.InnerException.Message}");
    }
}
```

### Standard Exceptions

- `ArgumentNullException`: When required parameters are null
- `DirectoryNotFoundException`: When package directory doesn't exist
- `FileNotFoundException`: When analyzing a file that doesn't exist

## Processing Results

### Working with Widgets

```csharp
var result = await analyzer.AnalyzePackageAsync(packagePath);

// Find stateless widgets
var statelessWidgets = result.Widgets
    .Where(w => w.Type == WidgetType.Stateless)
    .ToList();

// Find widgets with children
var containerWidgets = result.Widgets
    .Where(w => w.HasSingleChild || w.HasMultipleChildren)
    .ToList();

// Find deprecated widgets
var deprecated = result.Widgets
    .Where(w => w.IsDeprecated)
    .ToList();

// Examine widget properties
foreach (var widget in result.Widgets)
{
    var requiredProps = widget.Properties
        .Where(p => p.IsRequired)
        .ToList();

    var callbacks = widget.Properties
        .Where(p => p.IsCallback)
        .ToList();

    Console.WriteLine($"{widget.Name}:");
    Console.WriteLine($"  Required: {requiredProps.Count}");
    Console.WriteLine($"  Callbacks: {callbacks.Count}");
}
```

### Working with Types

```csharp
// Find abstract types
var abstractTypes = result.Types
    .Where(t => t.IsAbstract)
    .ToList();

// Find immutable types
var immutableTypes = result.Types
    .Where(t => t.IsImmutable)
    .ToList();

// Find types implementing specific interfaces
var specificInterface = result.Types
    .Where(t => t.Interfaces?.Contains("Comparable") ?? false)
    .ToList();
```

### Working with Enums

```csharp
foreach (var enumDef in result.Enums)
{
    Console.WriteLine($"Enum: {enumDef.Name}");

    foreach (var value in enumDef.Values)
    {
        var deprecated = value.IsDeprecated ? " [DEPRECATED]" : "";
        Console.WriteLine($"  {value.Name}{deprecated}");

        if (!string.IsNullOrEmpty(value.Documentation))
        {
            Console.WriteLine($"    {value.Documentation}");
        }
    }
}
```

## Performance Considerations

1. **Timeout Configuration**: Large packages may require longer timeouts. The default is 5 minutes.

2. **Caching**: Consider caching `PackageDefinition` results if analyzing the same package multiple times.

3. **Parallel Analysis**: The analyzer can analyze multiple packages in parallel:

```csharp
var packages = new[] { "/path/to/package1", "/path/to/package2" };
var tasks = packages.Select(p => analyzer.AnalyzePackageAsync(p));
var results = await Task.WhenAll(tasks);
```

4. **Memory**: Large packages with many widgets/types will consume more memory. Consider processing results in batches if memory is a concern.

## Troubleshooting

### "Dart is not installed or not found in PATH"

Ensure Dart SDK is installed and available in your system PATH:
```bash
dart --version
```

### "Analyzer script not found"

Verify the script exists at the specified path:
- Default: `Tools/analyzer/package_scanner.dart`
- Custom: Pass the correct path to the constructor

### "Process exited with non-zero exit code"

The Dart script failed. Common causes:
- Missing Dart dependencies (run `dart pub get` in analyzer directory)
- Invalid package structure (missing `pubspec.yaml`)
- Dart syntax errors in the package being analyzed

### Timeout Issues

Increase the timeout for large packages:
```csharp
var analyzer = new DartAnalyzerHost(timeoutSeconds: 1200); // 20 minutes
```

## Integration Examples

### ASP.NET Core Dependency Injection

```csharp
services.AddSingleton<DartAnalyzerHost>();
services.AddTransient<IPackageAnalyzer, PackageAnalyzerService>();
```

### Console Application

```csharp
static async Task Main(string[] args)
{
    var analyzer = new DartAnalyzerHost();

    if (args.Length == 0)
    {
        Console.WriteLine("Usage: analyzer <package-path>");
        return;
    }

    try
    {
        await analyzer.ValidateDartToolsAsync();
        var result = await analyzer.AnalyzePackageAsync(args[0]);

        Console.WriteLine($"Analysis complete: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}
```

## API Reference

### Constructor

```csharp
public DartAnalyzerHost(
    string? analyzerScriptPath = null,
    int timeoutSeconds = 300)
```

### Methods

#### ValidateDartToolsAsync
```csharp
public async Task<bool> ValidateDartToolsAsync(
    CancellationToken cancellationToken = default)
```

Validates that Dart tools are available and properly configured.

#### AnalyzePackageAsync
```csharp
public async Task<PackageDefinition> AnalyzePackageAsync(
    string packagePath,
    List<string>? includeTypes = null,
    List<string>? excludeTypes = null,
    CancellationToken cancellationToken = default)
```

Analyzes a complete Dart package and returns all discovered definitions.

#### AnalyzeFileAsync
```csharp
public async Task<PackageDefinition> AnalyzeFileAsync(
    string filePath,
    CancellationToken cancellationToken = default)
```

Analyzes a single Dart file by finding and analyzing its containing package.

### Virtual Methods (Override for Custom Logging)

#### LogDebug
```csharp
protected virtual void LogDebug(string message)
```

#### LogWarning
```csharp
protected virtual void LogWarning(string message)
```

## See Also

- `PackageDefinition` - The main result model
- `WidgetDefinition` - Widget-specific information
- `TypeDefinition` - Class/type information
- `EnumDefinition` - Enum information
- `PropertyDefinition` - Property/parameter information
- `ConstructorDefinition` - Constructor information
