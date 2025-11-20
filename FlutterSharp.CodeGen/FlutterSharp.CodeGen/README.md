# FlutterSharp Code Generator

A comprehensive command-line tool for generating C# and Dart FFI interoperability code from Flutter SDK and third-party packages. This tool automates the process of creating C# wrapper classes, structs, and Dart parser code needed to bridge the gap between .NET applications and Flutter-based UIs.

**GitHub Repository:** [github.com/Clancey/FlutterSharp](https://github.com/Clancey/FlutterSharp)

## Table of Contents

- [Features](#features)
- [Project Overview](#project-overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Generated Code](#generated-code)
- [Examples](#examples)
- [Advanced Usage](#advanced-usage)
- [Architecture](#architecture)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Automated Code Generation**: Generates C# widget wrappers, structs, enums, and corresponding Dart FFI code automatically from Flutter packages
- **Multiple Source Support**: Generate from Flutter SDK, public pub.dev packages, or local packages
- **Smart Type Mapping**: Comprehensive type mapping system that translates Dart types to C# types and Dart FFI struct types
- **Flexible Filtering**: Include/exclude specific widgets and types using wildcard patterns
- **FFI Interoperability**: Generates complete FFI bindings for seamless C#/.NET to Dart communication
- **Configuration File Support**: Highly configurable via `generator-config.json` with sensible defaults
- **CLI Commands**: Three main commands: `generate`, `validate`, and `list-widgets` (dry run)
- **Verbose Logging**: Detailed logging to help debug issues and understand the generation process
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Performance Optimized**: Parallel generation and caching support for large codebases
- **Template-Based**: Uses Scriban templates for extensible and maintainable code generation

## Project Overview

FlutterSharp.CodeGen bridges the .NET and Flutter ecosystems by automating code generation that allows C# applications to interact with Flutter's widget system through Dart FFI. This is particularly useful when:

- Building .NET applications that need to display Flutter UIs
- Creating cross-platform applications that leverage Flutter's UI capabilities
- Integrating Flutter packages into .NET/C# projects
- Maintaining type-safe C# bindings to Flutter APIs

### What Gets Generated

The code generator creates three main categories of files:

1. **C# Widget Wrappers** - Object-oriented C# classes that represent Flutter widgets with property bindings
2. **C# Structs** - FFI struct definitions for type-safe data marshalling between C# and Dart
3. **Dart Parsers and Structs** - Corresponding Dart code for parsing C# data and creating Flutter widgets

## Prerequisites

### Required

- **.NET 10.0 or higher** - The project targets .NET 10.0
  ```bash
  dotnet --version
  ```

- **Dart SDK 3.0.0 or higher** - Required for package analysis
  ```bash
  dart --version
  ```

### Optional

- **Flutter SDK 3.0.0 or higher** - Only needed if generating from Flutter SDK
  - Auto-detection from PATH or FLUTTER_ROOT environment variable
  - Can be automatically downloaded if not present
  ```bash
  flutter --version
  ```

### Environment Variables

| Variable | Purpose | Required |
|----------|---------|----------|
| `FLUTTER_ROOT` | Points to Flutter SDK root for auto-detection | Optional |
| `PUB_CACHE` | Pub package cache directory (default: `~/.pub-cache`) | Optional |
| `PATH` | Must include Dart SDK bin directory | Required |

## Quick Start

### 1. Clone and Build the Project

```bash
cd /path/to/FlutterSharp.CodeGen
dotnet build --configuration Release
```

### 2. Validate Your Setup

```bash
dotnet run -- validate
```

This checks that Dart and Flutter SDK are properly configured.

### 3. Generate Code from Flutter SDK

```bash
dotnet run -- generate
```

This generates code from the default Flutter SDK and outputs to `./Generated/CSharp` and `./Generated/Dart`.

### 4. Generate Specific Widgets Only

```bash
dotnet run -- generate --include "Container,Text,Row,Column"
```

### 5. Generate from a Local Package

```bash
dotnet run -- generate --source local --package-path /path/to/my_package
```

## Installation

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Clancey/FlutterSharp.git
cd FlutterSharp/FlutterSharp.CodeGen/FlutterSharp.CodeGen

# Build in Release mode
dotnet build --configuration Release

# Run directly
dotnet run -- --help

# Or use the compiled executable
dotnet bin/Release/net10.0/FlutterSharp.CodeGen.dll --help
```

### Prerequisites Installation

#### On macOS (using Homebrew)

```bash
# Install .NET
brew install dotnet

# Install Dart SDK
brew install dart

# Install Flutter SDK (optional)
brew install flutter
```

#### On Ubuntu/Debian

```bash
# Install .NET (adjust version as needed)
apt-get install -y dotnet-sdk-10.0

# Install Dart SDK
apt-get install -y dart

# Install Flutter SDK (optional)
apt-get install -y flutter
```

#### On Windows (using Chocolatey)

```powershell
# Install .NET
choco install dotnet-sdk

# Install Dart SDK
choco install dart

# Install Flutter SDK (optional)
choco install flutter
```

## Usage

The tool provides three main commands accessible via the CLI.

### Command: `generate`

Generate C# and Dart FFI code from Flutter packages.

#### Basic Usage

```bash
dotnet run -- generate [options]
```

#### Options

**Source Options:**
- `--source <type>` - Source type: `sdk` (default), `package`, or `local`
- `--flutter-sdk-path <path>` - Custom Flutter SDK path (auto-detected if not provided)
- `--package-name <name>` - Package name for pub packages (e.g., "provider", "get")
- `--package-version <version>` - Package version constraint (e.g., "^6.0.0")
- `--package-path <path>` - Absolute path to local package

**Output Options:**
- `--output-csharp <directory>` - C# output directory (default: `./Generated/CSharp`)
- `--output-dart <directory>` - Dart output directory (default: `./Generated/Dart`)

**Filtering Options:**
- `--include <types>` - Comma-separated types to include (e.g., "Container,Text,Row")
- `--exclude <types>` - Comma-separated types to exclude (supports wildcards: `*`, `?`)

**General Options:**
- `--verbose` - Enable verbose logging for detailed output
- `--update-sdk` - Update Flutter SDK to latest stable release before generating

#### Examples

```bash
# Generate from Flutter SDK (default)
dotnet run -- generate

# Generate from Flutter SDK with verbose output
dotnet run -- generate --verbose

# Generate only core material widgets
dotnet run -- generate --include "Scaffold,AppBar,FloatingActionButton,Card"

# Exclude internal/debug widgets
dotnet run -- generate --exclude "*Internal*,*Debug*,*Test*"

# Generate from specific Flutter SDK path
dotnet run -- generate --flutter-sdk-path ~/flutter --output-csharp ./src/Generated/CSharp

# Generate from pub.dev package
dotnet run -- generate --source package --package-name provider --package-version "^6.0.0"

# Generate from local Flutter package
dotnet run -- generate --source local --package-path ./packages/my_custom_widgets

# Update SDK and generate with custom output
dotnet run -- generate --update-sdk --output-csharp ./Output/CSharp --output-dart ./Output/Dart

# Complex filtering with multiple packages
dotnet run -- generate --include "Widget,StatelessWidget,StatefulWidget" --exclude "*Deprecated*"
```

#### Output Structure

```
Generated/
├── CSharp/
│   ├── Widgets/
│   │   ├── Container.cs           # C# widget wrapper class
│   │   ├── Text.cs
│   │   ├── Row.cs
│   │   └── ...
│   ├── Structs/
│   │   ├── ContainerStruct.cs     # FFI struct for marshalling
│   │   ├── TextStruct.cs
│   │   └── ...
│   └── Enums/
│       ├── TextAlign.cs           # C# enum with extension methods
│       ├── MainAxisAlignment.cs
│       └── ...
└── Dart/
    ├── structs/
    │   ├── container_struct.dart  # Dart FFI struct definitions
    │   ├── text_struct.dart
    │   └── ...
    ├── parsers/
    │   ├── container_parser.dart  # Dart widget parser/builder
    │   ├── text_parser.dart
    │   └── ...
    └── enums/
        ├── text_align.dart        # Dart enum definitions
        ├── main_axis_alignment.dart
        └── ...
```

### Command: `validate`

Validate that required tools are properly configured.

#### Usage

```bash
dotnet run -- validate [options]
```

#### Options

- `--flutter-sdk-path <path>` - Validate specific Flutter SDK path
- `--verbose` - Show detailed validation information

#### Examples

```bash
# Validate default setup
dotnet run -- validate

# Validate specific Flutter SDK
dotnet run -- validate --flutter-sdk-path ~/custom/flutter --verbose

# In CI/CD pipeline
dotnet run -- validate || exit 1
```

#### What Gets Validated

- Dart SDK is installed and accessible
- Dart analyzer script exists at `Tools/analyzer/package_scanner.dart`
- Flutter SDK is available (if source is SDK)
- Flutter SDK structure is valid
- All required dependencies are present

### Command: `list-widgets`

List all widgets and types that would be generated (dry run).

#### Usage

```bash
dotnet run -- list-widgets [options]
```

#### Options

Same as `generate` command (excluding output and update options):
- `--source <type>`
- `--flutter-sdk-path <path>`
- `--package-name <name>`
- `--package-version <version>`
- `--package-path <path>`
- `--include <types>`
- `--exclude <types>`
- `--verbose`

#### Examples

```bash
# List all widgets from Flutter SDK
dotnet run -- list-widgets

# List only specific widgets
dotnet run -- list-widgets --include "Container,Text,Row,Column"

# List with detailed information
dotnet run -- list-widgets --verbose

# List from third-party package
dotnet run -- list-widgets --source package --package-name provider
```

#### Output Example

```
================================================================================
Widget List (Dry Run)
================================================================================
Analyzing package at: /path/to/flutter/packages/flutter

Package: flutter v3.24.0
Description: Flutter SDK

Widgets (150):
--------------------------------------------------------------------------------
  Container (Stateless) [single child]
    Properties: 12
  Text (Stateless)
    Properties: 8
  Row (MultiChildRenderObject) [multiple children]
    Properties: 6
  Column (MultiChildRenderObject) [multiple children]
    Properties: 6
  ...

Types (45):
--------------------------------------------------------------------------------
  EdgeInsets (4 properties)
  TextStyle (15 properties)
  Color (2 properties)
  ...

Enums (20):
--------------------------------------------------------------------------------
  TextAlign (7 values)
  MainAxisAlignment (6 values)
  Axis (2 values)
  ...

================================================================================
Total items: 215
================================================================================
```

## Configuration

The code generator can be configured via a `generator-config.json` file for more complex scenarios and production use.

### Creating a Configuration File

```bash
# The tool automatically looks for generator-config.json in the current directory
# Copy the example configuration as a starting point
cp Config/generator-config.json ./generator-config.json
```

### Configuration Structure

The configuration file supports the following sections:

```json
{
  "flutterSdk": { ... },          // Flutter SDK configuration
  "outputPaths": { ... },         // Output directory paths
  "filters": { ... },             // Include/exclude patterns
  "thirdPartyPackages": { ... },  // Third-party package configuration
  "generationOptions": { ... },   // Code generation options
  "typeMappingConfig": { ... },   // Custom type mappings
  "advancedOptions": { ... }      // Performance and advanced options
}
```

### Configuration Examples

#### Minimal Configuration (Auto-Detect Everything)

```json
{
  "flutterSdk": {
    "mode": "auto"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart"
  }
}
```

#### Using Local Flutter SDK

```json
{
  "flutterSdk": {
    "mode": "local",
    "path": "/Users/username/flutter"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart"
  }
}
```

#### Cloning Specific Flutter Version

```json
{
  "flutterSdk": {
    "mode": "clone",
    "version": "3.16.0",
    "cloneDirectory": "./.cache/flutter-sdk"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart"
  }
}
```

#### Development Configuration with Logging

```json
{
  "flutterSdk": {
    "mode": "auto"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart"
  },
  "generationOptions": {
    "generateDocumentation": true,
    "generateDebuggerAttributes": true
  },
  "advancedOptions": {
    "logLevel": "Debug",
    "emitPerformanceDiagnostics": true,
    "validateGeneratedCode": true
  }
}
```

#### Production/Performance Configuration

```json
{
  "flutterSdk": {
    "mode": "local",
    "path": "/opt/flutter"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart",
    "cleanBeforeGenerate": true
  },
  "filters": {
    "excludeWidgets": ["*Internal*", "*Debug*", "*Test*"]
  },
  "advancedOptions": {
    "parallelGeneration": true,
    "maxDegreeOfParallelism": 8,
    "incrementalGeneration": true,
    "enableCaching": true,
    "formatGeneratedCode": false,
    "logLevel": "Warning"
  }
}
```

#### With Third-Party Packages

```json
{
  "flutterSdk": {
    "mode": "auto"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp",
    "dartOutput": "./Generated/Dart"
  },
  "thirdPartyPackages": {
    "enabled": true,
    "pubDevPackages": [
      {
        "name": "provider",
        "version": "^6.0.0",
        "enabled": true
      },
      {
        "name": "get",
        "version": "^4.6.0",
        "enabled": true
      }
    ]
  }
}
```

For comprehensive configuration documentation, see [Config/README.md](Config/README.md).

## Generated Code

### C# Widget Wrappers

Generated C# widget classes provide a strongly-typed object-oriented interface to Flutter widgets:

```csharp
namespace FlutterSharp.Generated.Widgets;

/// <summary>
/// A Flutter Container widget wrapping the layout and decoration properties.
/// </summary>
public partial class Container : StatelessWidget
{
    public Alignment? Alignment { get; set; }
    public EdgeInsets? Padding { get; set; }
    public EdgeInsets? Margin { get; set; }
    public Decoration? Decoration { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }

    public Container() { }

    public Container WithChild(Widget child)
    {
        // Implementation
    }

    public override string ToJson() { /* ... */ }
}
```

### C# Structs for FFI

FFI struct definitions for type-safe data marshalling:

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ContainerStruct
{
    public IntPtr alignment;
    public IntPtr padding;
    public IntPtr margin;
    public IntPtr decoration;
    public double width;
    public double height;
}
```

### Dart Parsers

Dart code that converts C# data structures into Flutter widgets:

```dart
import 'package:flutter/widgets.dart';

class ContainerParser {
  static Widget parse(Map<String, dynamic> json) {
    return Container(
      alignment: _parseAlignment(json['alignment']),
      padding: _parseEdgeInsets(json['padding']),
      margin: _parseEdgeInsets(json['margin']),
      child: _parseWidget(json['child']),
    );
  }

  // Helper methods...
}
```

## Examples

### Example 1: Generate Core Material Widgets

Generate only the most commonly used Material widgets:

```bash
dotnet run -- generate \
  --include "Scaffold,AppBar,FloatingActionButton,Card,ListView,GridView" \
  --output-csharp ./src/Generated/CSharp \
  --output-dart ./flutter_app/lib/generated
```

### Example 2: Generate from Third-Party Package

Generate wrappers for the `provider` state management package:

```bash
dotnet run -- generate \
  --source package \
  --package-name provider \
  --package-version "^6.0.0" \
  --output-csharp ./src/Generated/CSharp
```

### Example 3: Generate Specific Package but Exclude Private Types

Generate from a package but exclude internal/private implementations:

```bash
dotnet run -- generate \
  --source local \
  --package-path ./packages/my_widgets \
  --exclude "*Internal*,*Private*,_*" \
  --verbose
```

### Example 4: CI/CD Pipeline

Validate and generate in an automated pipeline:

```bash
#!/bin/bash
set -e

# Validate environment
dotnet run -- validate --verbose

# Generate from Flutter SDK with update
dotnet run -- generate \
  --update-sdk \
  --output-csharp ./src/Generated/CSharp \
  --output-dart ./flutter/lib/generated

# Check for changes
if git diff --exit-code Generated/ > /dev/null; then
  echo "No code generation changes"
else
  echo "Generated code updated"
  git add Generated/
  git commit -m "chore: update generated code"
fi
```

### Example 5: Development vs Production Configuration

Use different configurations for different environments:

```bash
# Development - include everything with verbose logging
dotnet run -- generate --verbose --include "*"

# Production - exclude internal types, optimize performance
dotnet run -- generate \
  --exclude "*Internal*,*Debug*" \
  --output-csharp ./src/Generated/CSharp
```

## Advanced Usage

### Third-Party Packages

Generate bindings for third-party Flutter packages from pub.dev:

```bash
# Generate from a single package
dotnet run -- generate --source package --package-name get --package-version "^4.6.0"

# Generate from multiple packages (requires configuration file)
# Edit generator-config.json to include multiple packages:
```

```json
{
  "thirdPartyPackages": {
    "enabled": true,
    "pubDevPackages": [
      { "name": "provider", "version": "^6.0.0" },
      { "name": "get", "version": "^4.6.0" },
      { "name": "riverpod", "version": "^2.0.0" }
    ]
  }
}
```

### Filtering and Customization

#### Wildcard Patterns

The include/exclude filters support Unix-style wildcards:

- `*` - Matches any sequence of characters
- `?` - Matches a single character
- Examples:
  - `*Internal*` - Excludes anything with "Internal" in the name
  - `Test*` - Excludes anything starting with "Test"
  - `*_private` - Excludes anything ending with "_private"

#### Include-Only Generation

Generate only specific widgets:

```bash
dotnet run -- generate --include "Container,Text,Row,Column,Padding,Center" --exclude ""
```

#### Namespace Filtering

Control which packages and namespaces are included (via configuration):

```json
{
  "filters": {
    "includePackages": ["flutter", "material"],
    "excludeNamespaces": ["*internal*", "*test*"]
  }
}
```

### Custom Type Mapping

For advanced scenarios, extend type mappings (requires code modification):

```csharp
var registry = new TypeMappingRegistry();

// Register custom type mapping
registry.RegisterMapping(new TypeMapping
{
    DartType = "MyCustomWidget",
    CSharpType = "MyCustomWidget",
    DartStructType = "Pointer<Void>",
    IsWidget = true,
    RequiresCustomMarshalling = true
});
```

### Template Customization

Customize code generation by modifying Scriban templates:

1. Locate templates in `Templates/` directory
2. Edit the relevant `.scriban` file:
   - `CSharpWidget.scriban` - C# widget template
   - `CSharpStruct.scriban` - C# struct template
   - `CSharpEnum.scriban` - C# enum template
   - `DartStruct.scriban` - Dart FFI struct template
   - `DartParser.scriban` - Dart parser template
   - `DartEnum.scriban` - Dart enum template

3. Rebuild and regenerate:
   ```bash
   dotnet build --configuration Release
   dotnet run -- generate
   ```

### Performance Optimization

For large Flutter packages:

```json
{
  "advancedOptions": {
    "parallelGeneration": true,
    "maxDegreeOfParallelism": 8,
    "incrementalGeneration": true,
    "enableCaching": true,
    "formatGeneratedCode": false,
    "logLevel": "Warning"
  }
}
```

These settings:
- Enable parallel file generation for faster processing
- Only regenerate files that have changed
- Cache analysis results
- Skip code formatting for speed
- Reduce logging overhead

## Architecture

The project is organized into several key components:

### Project Structure

```
FlutterSharp.CodeGen/
├── Program.cs                  # CLI entry point and command definitions
├── Analysis/                   # Dart package analysis
│   ├── DartAnalyzerHost.cs     # Dart analyzer orchestration
│   └── ...
├── Config/                     # Configuration system
│   ├── GeneratorConfig.cs      # Configuration classes
│   ├── ConfigLoader.cs         # Configuration loading/validation
│   └── generator-config.json   # Example configuration
├── Generators/                 # Code generation logic
│   ├── CSharp/                 # C# generator implementations
│   │   ├── CSharpWidgetGenerator.cs
│   │   ├── CSharpStructGenerator.cs
│   │   └── CSharpEnumGenerator.cs
│   └── Dart/                   # Dart generator implementations
│       ├── DartParserGenerator.cs
│       ├── DartStructGenerator.cs
│       └── DartEnumGenerator.cs
├── Models/                     # Data models
│   ├── PackageDefinition.cs    # Package metadata
│   ├── WidgetDefinition.cs     # Widget definition
│   ├── TypeDefinition.cs       # Type definition
│   ├── EnumDefinition.cs       # Enum definition
│   └── PropertyDefinition.cs   # Property/parameter definition
├── Templates/                  # Scriban code generation templates
│   ├── CSharpWidget.scriban
│   ├── CSharpStruct.scriban
│   ├── CSharpEnum.scriban
│   ├── DartStruct.scriban
│   ├── DartParser.scriban
│   └── DartEnum.scriban
├── TypeMapping/                # Type mapping system
│   ├── TypeMappingRegistry.cs  # Central type mapping registry
│   ├── DartToCSharpMapper.cs   # Dart to C# mapper
│   ├── CSharpToDartFfiMapper.cs # C# to Dart FFI mapper
│   └── TypeMapping.cs          # Type mapping model
├── Tools/                      # Utility tools
│   └── analyzer/               # Dart analyzer script
│       ├── package_scanner.dart
│       └── pubspec.yaml
└── Sources/                    # Package source handlers
    ├── SdkSource.cs            # Flutter SDK source
    ├── PackageSource.cs        # Pub.dev package source
    └── LocalSource.cs          # Local package source
```

### Key Components

#### 1. CLI Entry Point (Program.cs)

- Defines three main commands: `generate`, `validate`, `list-widgets`
- Handles command-line argument parsing using System.CommandLine
- Manages logging and error reporting

#### 2. Analysis Layer (Analysis/)

- `DartAnalyzerHost.cs` - Orchestrates analysis by calling Dart analyzer script
- Deserializes JSON results into model objects
- Handles timeout and error cases

#### 3. Configuration System (Config/)

- `GeneratorConfig.cs` - Strongly-typed configuration model
- `ConfigLoader.cs` - Loads, validates, and merges configurations
- `generator-config.json` - Example configuration with all options

#### 4. Code Generators (Generators/)

- **CSharpWidgetGenerator** - Generates C# widget wrapper classes
- **CSharpStructGenerator** - Generates C# FFI structs
- **CSharpEnumGenerator** - Generates C# enums with extension methods
- **DartParserGenerator** - Generates Dart widget builder functions
- **DartStructGenerator** - Generates Dart FFI struct definitions
- **DartEnumGenerator** - Generates Dart enum definitions

All generators use Scriban templates for maintainability.

#### 5. Type Mapping System (TypeMapping/)

- `TypeMappingRegistry.cs` - Central repository of type mappings (500+ built-in mappings)
- `DartToCSharpMapper.cs` - Maps Dart types to C# equivalents
- `CSharpToDartFfiMapper.cs` - Maps C# types to Dart FFI types
- Handles:
  - Primitive types (int, string, bool, etc.)
  - Flutter types (Widget, Color, EdgeInsets, etc.)
  - Collection types (List<T>, Map<K,V>, etc.)
  - Generic types with type arguments
  - Nullable types
  - Custom types

#### 6. Data Models (Models/)

- `PackageDefinition.cs` - Complete package metadata with all widgets, types, enums
- `WidgetDefinition.cs` - Widget information including properties, constructors
- `TypeDefinition.cs` - Custom type information
- `EnumDefinition.cs` - Enum values and metadata
- `PropertyDefinition.cs` - Property/parameter information
- `ConstructorDefinition.cs` - Constructor metadata

#### 7. Code Templates (Templates/)

Scriban templates that control code generation output:
- Define structure and format of generated code
- Have access to model properties for dynamic code generation
- Can be customized for different coding standards

#### 8. Package Sources (Sources/)

Different strategies for obtaining packages:
- **SdkSource** - Reads Flutter SDK from local filesystem
- **PackageSource** - Downloads packages from pub.dev
- **LocalSource** - Reads packages from local paths

#### 9. Dart Analyzer Tool (Tools/analyzer/)

Standalone Dart program that:
- Analyzes Dart source files using the Dart analyzer
- Extracts widget, type, and enum definitions
- Outputs JSON for consumption by C#
- Supports include/exclude filtering

### Data Flow

```
User Input (CLI args)
    ↓
Configuration Loading & Merging
    ↓
Package Source Resolution (SDK/Pub/Local)
    ↓
Dart Analyzer Execution (package_scanner.dart)
    ↓
JSON Deserialization → Model Objects
    ↓
Type Mapping & Filtering
    ↓
Code Generators (CSharp & Dart)
    ↓
File Writing (CSharp + Dart outputs)
```

## Development

### Building the Project

```bash
# Debug build
dotnet build

# Release build with optimizations
dotnet build --configuration Release

# Clean and rebuild
dotnet clean && dotnet build --configuration Release
```

### Running Tests

While this project doesn't include a separate test project in this directory, you can:

```bash
# Run manual integration tests
dotnet run -- validate
dotnet run -- list-widgets
dotnet run -- generate --output-csharp ./test-output/csharp --output-dart ./test-output/dart
```

### Debugging

```bash
# Run with verbose logging
dotnet run -- generate --verbose

# Run from IDE (Visual Studio, VS Code)
# Add breakpoints and use Debug configurations

# Attach to running process
# Use your IDE's attach debugger feature
```

### Code Style

The project follows:
- C# naming conventions (PascalCase for public members)
- Microsoft C# coding standards
- Nullable reference type annotations enabled
- Implicit usings enabled

### Contributing

To contribute to this project:

1. **Fork the repository** on GitHub
2. **Create a feature branch** (`git checkout -b feature/your-feature`)
3. **Make your changes** following the code style
4. **Test thoroughly**:
   ```bash
   dotnet build --configuration Release
   dotnet run -- validate
   dotnet run -- generate --verbose
   ```
5. **Commit with clear messages** (`git commit -m "Description of changes"`)
6. **Push to your fork** (`git push origin feature/your-feature`)
7. **Create a Pull Request** on the main repository

### Areas for Contribution

- Additional type mappings for unsupported types
- Template improvements for better generated code
- Performance optimizations
- Documentation enhancements
- Additional source handlers (e.g., Git URLs, local git repositories)
- More comprehensive error handling
- Better logging and diagnostics

### Building Documentation

Documentation for individual components:
- [Config System](Config/README.md) - Configuration options and examples
- [Data Models](Models/README.md) - Model class structure and usage
- [Type Mapping System](TypeMapping/README.md) - Type mapping API and reference
- [Dart Analyzer](Tools/analyzer/README.md) - Analyzer script documentation
- [Analysis System](Analysis/README.md) - DartAnalyzerHost API
- [CLI Usage](CLI_USAGE.md) - Command-line interface reference

## Troubleshooting

### Common Issues and Solutions

#### 1. "Dart is not installed or not found in PATH"

**Problem:** The Dart SDK is not installed or not available in PATH.

**Solutions:**
```bash
# Check if Dart is installed
dart --version

# Add Dart to PATH (macOS/Linux)
export PATH="$PATH:$HOME/flutter/bin/cache/dart-sdk/bin"

# Verify PATH
which dart

# Install Dart if not present
# macOS: brew install dart
# Ubuntu: apt-get install dart
```

#### 2. "Flutter SDK not found"

**Problem:** Flutter SDK cannot be found.

**Solutions:**
```bash
# Method 1: Auto-detect from PATH
flutter --version

# Method 2: Use explicit path
dotnet run -- generate --flutter-sdk-path /path/to/flutter

# Method 3: Set FLUTTER_ROOT environment variable
export FLUTTER_ROOT=/path/to/flutter
dotnet run -- generate

# Method 4: Auto-download via config
# Edit generator-config.json:
# "flutterSdk": { "mode": "clone", "version": "stable" }
```

#### 3. "Analyzer script not found"

**Problem:** The Dart analyzer script cannot be located.

**Solutions:**
```bash
# Verify file exists
ls -la Tools/analyzer/package_scanner.dart

# Verify Dart dependencies are installed
cd Tools/analyzer
dart pub get
cd ../../..

# Specify custom path if needed
# (Not available via CLI, requires code modification)
```

#### 4. "Package not found in pub cache"

**Problem:** Third-party package is not in the local pub cache.

**Solutions:**
```bash
# Download the package
dart pub get

# Or use the update option to fetch latest
dotnet run -- generate --source package --package-name provider --package-version "^6.0.0"

# Check pub cache location
ls ~/.pub-cache/hosted/pub.dartlang.org/
```

#### 5. "No types found matching filter"

**Problem:** Include/exclude filters result in no types being generated.

**Solutions:**
```bash
# List widgets first to see available names
dotnet run -- list-widgets

# Verify filter syntax
dotnet run -- generate --include "Container,Text,Row" --verbose

# Check for typos in widget names
dotnet run -- list-widgets --verbose | grep -i "container"
```

#### 6. "Runtime errors or crashes"

**Problem:** Generation fails with unhandled exceptions.

**Solutions:**
```bash
# Get detailed error information
dotnet run -- generate --verbose 2>&1 | tee error.log

# Verify .NET installation
dotnet --info

# Check system requirements
dart --version
flutter --version

# Try with minimal options
dotnet run -- generate

# Report issue with verbose output
# See Contributing section for issue reporting guidelines
```

#### 7. "Permission denied" errors

**Problem:** Insufficient permissions to write generated files.

**Solutions:**
```bash
# Check output directory permissions
ls -la Generated/

# Ensure write permissions
chmod -R u+w Generated/

# Use different output directory
dotnet run -- generate --output-csharp /tmp/gen-csharp --output-dart /tmp/gen-dart
```

#### 8. "Timeout during analysis"

**Problem:** Analysis takes too long and times out.

**Solutions:**
```bash
# Increase timeout in code
# (Requires code modification - default is 5 minutes)

# Use filtering to reduce analysis scope
dotnet run -- generate --include "Container,Text" --verbose

# Check system resources
top  # Or Task Manager on Windows
free -h  # Or free on Unix-like systems
```

### Getting Help

1. **Check documentation**: Review relevant README files in the project
2. **Verbose logging**: Run with `--verbose` flag for detailed output
3. **List widgets**: Use `list-widgets` to verify package analysis
4. **Validate setup**: Use `validate` command to check prerequisites
5. **Report issues**: Create GitHub issue with verbose output and reproduction steps

## Contributing

Contributions are welcome! Please see the [Development](#development) section for guidelines.

For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

The MIT License allows:
- Commercial use
- Modification
- Distribution
- Private use

With conditions:
- License and copyright notice must be provided
- Software is provided as-is

---

## Additional Resources

- **Flutter Documentation**: [flutter.dev](https://flutter.dev)
- **Dart Documentation**: [dart.dev](https://dart.dev)
- **System.CommandLine**: [github.com/dotnet/command-line-api](https://github.com/dotnet/command-line-api)
- **Scriban Template Engine**: [github.com/scriban/scriban](https://github.com/scriban/scriban)

## Project Status

FlutterSharp is actively maintained. For latest updates and releases, visit the [GitHub repository](https://github.com/Clancey/FlutterSharp).

---

**Last Updated:** November 19, 2025
**Target Framework:** .NET 10.0
**Minimum Dart SDK:** 3.0.0
**Minimum Flutter SDK:** 3.0.0
