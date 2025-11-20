# Example Usage

## From Command Line

### Basic scan of a package
```bash
dart run package_scanner.dart /path/to/flutter_package
```

### Scan with include filter (only specific types)
```bash
dart run package_scanner.dart /path/to/flutter_package "Text,Container,Row,Column"
```

### Scan with both include and exclude filters
```bash
dart run package_scanner.dart /path/to/flutter_package "Text,Container" "PrivateWidget"
```

## From C#

### Basic Integration

```csharp
using System.Diagnostics;
using System.Text.Json;
using FlutterSharp.CodeGen.Models;

public async Task<PackageDefinition> ScanPackageAsync(string packagePath)
{
    var analyzerPath = Path.Combine(
        AppContext.BaseDirectory,
        "Tools",
        "analyzer",
        "package_scanner.dart"
    );

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "dart",
        Arguments = $"run {analyzerPath} {packagePath}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(processStartInfo);
    if (process == null)
    {
        throw new Exception("Failed to start dart process");
    }

    var output = await process.StandardOutput.ReadToEndAsync();
    var errors = await process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        throw new Exception($"Scanner failed: {errors}");
    }

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    return JsonSerializer.Deserialize<PackageDefinition>(output, options)
        ?? throw new Exception("Failed to deserialize package definition");
}
```

### With Include/Exclude Filters

```csharp
public async Task<PackageDefinition> ScanPackageAsync(
    string packagePath,
    IEnumerable<string>? includeTypes = null,
    IEnumerable<string>? excludeTypes = null)
{
    var analyzerPath = Path.Combine(
        AppContext.BaseDirectory,
        "Tools",
        "analyzer",
        "package_scanner.dart"
    );

    var includeArg = includeTypes != null && includeTypes.Any()
        ? string.Join(",", includeTypes)
        : "";

    var excludeArg = excludeTypes != null && excludeTypes.Any()
        ? string.Join(",", excludeTypes)
        : "";

    var arguments = $"run {analyzerPath} {packagePath} \"{includeArg}\" \"{excludeArg}\"";

    var processStartInfo = new ProcessStartInfo
    {
        FileName = "dart",
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WorkingDirectory = Path.GetDirectoryName(analyzerPath)
    };

    using var process = Process.Start(processStartInfo);
    if (process == null)
    {
        throw new Exception("Failed to start dart process");
    }

    var output = await process.StandardOutput.ReadToEndAsync();
    var errors = await process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        throw new Exception($"Scanner failed: {errors}");
    }

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    return JsonSerializer.Deserialize<PackageDefinition>(output, options)
        ?? throw new Exception("Failed to deserialize package definition");
}
```

### Usage Example

```csharp
// Scan entire package
var package = await ScanPackageAsync("/path/to/flutter/packages/flutter");

Console.WriteLine($"Found {package.Widgets.Count} widgets");
Console.WriteLine($"Found {package.Types.Count} types");
Console.WriteLine($"Found {package.Enums.Count} enums");

// Scan only specific widgets
var materialPackage = await ScanPackageAsync(
    "/path/to/flutter/packages/flutter",
    includeTypes: new[] { "AppBar", "Scaffold", "FloatingActionButton" }
);

// Process widgets
foreach (var widget in package.Widgets)
{
    Console.WriteLine($"Widget: {widget.Name}");
    Console.WriteLine($"  Base: {widget.BaseClass}");
    Console.WriteLine($"  Properties: {widget.Properties.Count}");

    foreach (var prop in widget.Properties)
    {
        var required = prop.IsRequired ? "required " : "";
        var nullable = prop.IsNullable ? "?" : "";
        Console.WriteLine($"    {required}{prop.DartType}{nullable} {prop.Name}");
    }
}
```

### Error Handling

```csharp
public async Task<PackageDefinition?> SafeScanPackageAsync(string packagePath)
{
    try
    {
        return await ScanPackageAsync(packagePath);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to scan package {packagePath}: {ex.Message}");
        return null;
    }
}
```

## Output Sample

The scanner produces JSON output like this:

```json
{
  "packagePath": "/tmp/test_flutter_package",
  "name": "test_flutter_package",
  "version": "1.0.0",
  "description": "A test package for scanner validation",
  "widgets": [],
  "types": [
    {
      "name": "TextStyle",
      "namespace": "painting.text_style",
      "baseClass": "Object",
      "interfaces": [],
      "isAbstract": false,
      "isImmutable": true,
      "properties": [
        {
          "name": "fontSize",
          "dartType": "double?",
          "isRequired": false,
          "isNullable": true,
          "isNamed": true,
          "documentation": "The size of the font",
          "isList": false,
          "isCallback": false,
          "typeArguments": null
        }
      ],
      "constructors": [
        {
          "name": "",
          "isConst": true,
          "isFactory": false,
          "parameters": [...],
          "documentation": "Creates a text style",
          "fullName": "TextStyle"
        }
      ]
    }
  ],
  "enums": [
    {
      "name": "TextAlign",
      "namespace": "painting.text_align",
      "values": [
        {
          "name": "left",
          "documentation": "Align text to the left"
        }
      ]
    }
  ],
  "analysisTimestamp": "2025-11-19T16:34:11.926174"
}
```

## Notes

- The scanner requires Dart SDK to be installed
- Run `dart pub get` in the analyzer directory before first use
- The scanner automatically excludes test files and private members
- Widget detection requires the Flutter SDK to be properly resolved
- For best results, ensure the target package has a valid `pubspec.yaml`
