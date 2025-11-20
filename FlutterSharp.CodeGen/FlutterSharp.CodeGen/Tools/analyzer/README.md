# Dart Package Scanner

A Dart analyzer tool that extracts widget, type, and enum definitions from Dart source files for code generation purposes.

## Features

- **Widget Extraction**: Identifies and extracts all Flutter widget classes including:
  - StatelessWidget
  - StatefulWidget
  - RenderObjectWidget variants
  - Custom widgets extending Widget base class

- **Type Extraction**: Extracts custom class definitions with:
  - Properties and their types
  - Constructors with parameters
  - Generic type parameters
  - Documentation comments

- **Enum Extraction**: Extracts enum definitions with values and documentation

- **Comprehensive Metadata**: Captures:
  - Nullability information
  - Required vs optional parameters
  - Default values
  - Documentation comments
  - Deprecation status
  - Type arguments for generics

## Installation

1. Navigate to the analyzer directory:
   ```bash
   cd /Users/clancey/Projects/FlutterSharp/FlutterSharp.CodeGen/FlutterSharp.CodeGen/Tools/analyzer
   ```

2. Install dependencies:
   ```bash
   dart pub get
   ```

## Usage

### Basic Usage

```bash
dart run package_scanner.dart <package-path>
```

### With Include Filter

Only extract specific types by name:

```bash
dart run package_scanner.dart /path/to/package Text,Container,Row
```

### With Include and Exclude Filters

Extract specific types but exclude others:

```bash
dart run package_scanner.dart /path/to/package "Widget,Text,Container" "PrivateWidget,InternalClass"
```

### Command Line Arguments

1. **package-path** (required): Path to the Dart package to analyze
2. **include-list** (optional): Comma-separated list of type names to include
3. **exclude-list** (optional): Comma-separated list of type names to exclude

## Output Format

The scanner outputs JSON matching the C# model classes used in FlutterSharp.CodeGen:

```json
{
  "packagePath": "/path/to/package",
  "name": "package_name",
  "version": "1.0.0",
  "description": "Package description",
  "widgets": [
    {
      "name": "Text",
      "namespace": "widgets.text",
      "baseClass": "StatelessWidget",
      "type": "Stateless",
      "properties": [
        {
          "name": "data",
          "dartType": "String",
          "isRequired": true,
          "isNullable": false,
          "isNamed": false,
          "documentation": "The text to display"
        }
      ],
      "constructors": [
        {
          "name": "",
          "isConst": true,
          "isFactory": false,
          "parameters": [...],
          "fullName": "Text"
        }
      ],
      "hasSingleChild": false,
      "hasMultipleChildren": false,
      "isAbstract": false,
      "isDeprecated": false,
      "typeParameters": []
    }
  ],
  "types": [
    {
      "name": "TextStyle",
      "namespace": "painting.text_style",
      "baseClass": "Diagnosticable",
      "interfaces": [],
      "isAbstract": false,
      "isImmutable": true,
      "properties": [...],
      "constructors": [...]
    }
  ],
  "enums": [
    {
      "name": "TextAlign",
      "namespace": "painting.text_align",
      "values": [
        {
          "name": "left",
          "documentation": "Align text to the left edge"
        },
        {
          "name": "right",
          "documentation": "Align text to the right edge"
        }
      ]
    }
  ],
  "analysisTimestamp": "2025-11-19T12:00:00.000Z"
}
```

## Integration with C#

The output JSON can be deserialized directly into the C# model classes:

```csharp
var json = await Process.Start(new ProcessStartInfo
{
    FileName = "dart",
    Arguments = $"run package_scanner.dart {packagePath}",
    RedirectStandardOutput = true
}).StandardOutput.ReadToEndAsync();

var package = JsonSerializer.Deserialize<PackageDefinition>(json);
```

## Error Handling

- Invalid package paths will result in an exit code of 1
- Parse errors for individual files are logged to stderr but don't stop the analysis
- Missing pubspec.yaml will use default package information

## Examples

### Scan Flutter SDK Widgets

```bash
dart run package_scanner.dart /path/to/flutter/packages/flutter
```

### Scan Material Widgets Only

```bash
dart run package_scanner.dart /path/to/flutter/packages/flutter "AppBar,Scaffold,Button"
```

### Scan Custom Package

```bash
dart run package_scanner.dart ~/my_flutter_package
```

## Limitations

- Only analyzes files in the `lib/` directory
- Skips private classes and members (prefixed with `_`)
- Test files are automatically excluded
- Generated files in `.dart_tool/` are excluded

## Development

### Running Tests

```bash
dart test
```

### Debugging

For verbose output, redirect stderr to see warnings:

```bash
dart run package_scanner.dart /path/to/package 2> errors.log
```
