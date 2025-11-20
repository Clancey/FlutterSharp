# FlutterSharp Code Generator CLI

A comprehensive command-line tool for generating C# and Dart FFI code from Flutter SDK and packages.

## Commands

### 1. `generate` - Generate code from Flutter SDK or packages

Analyzes Flutter packages and generates C# widget wrappers, C# structs, Dart structs, and Dart parsers.

#### Options

**Source Options:**
- `--source <sdk|package|local>` - Source type (default: `sdk`)
  - `sdk`: Generate from Flutter SDK
  - `package`: Generate from a pub package
  - `local`: Generate from a local package path

- `--flutter-sdk-path <path>` - Flutter SDK path (auto-detects if not provided)
- `--package-name <name>` - Package name for pub packages (e.g., "provider")
- `--package-version <version>` - Package version (e.g., "^6.0.0")
- `--package-path <path>` - Local package path

**Output Options:**
- `--output-csharp <directory>` - C# output directory (default: `./Generated/CSharp`)
- `--output-dart <directory>` - Dart output directory (default: `./Generated/Dart`)

**Filtering Options:**
- `--include <types>` - Comma-separated list of types to include
- `--exclude <types>` - Comma-separated list of types to exclude

**General Options:**
- `--verbose` - Enable verbose logging
- `--update-sdk` - Update Flutter SDK to latest release before generating

#### Examples

```bash
# Generate from Flutter SDK (auto-detect)
dotnet run -- generate

# Generate from specific Flutter SDK path
dotnet run -- generate --flutter-sdk-path /path/to/flutter/sdk

# Generate only specific widgets
dotnet run -- generate --include "Container,Text,Row,Column"

# Exclude specific widgets
dotnet run -- generate --exclude "Scaffold,AppBar"

# Generate from a local package
dotnet run -- generate --source local --package-path /path/to/package

# Generate with custom output directories
dotnet run -- generate --output-csharp ./Output/CSharp --output-dart ./Output/Dart

# Update SDK and generate with verbose logging
dotnet run -- generate --update-sdk --verbose
```

#### Output Structure

```
Generated/
├── CSharp/
│   ├── Widgets/          # C# widget wrapper classes
│   │   ├── Container.cs
│   │   ├── Text.cs
│   │   └── ...
│   ├── Structs/          # C# structs for FFI interop
│   │   ├── ContainerStruct.cs
│   │   ├── TextStruct.cs
│   │   └── ...
│   └── Enums/            # C# enums with extensions
│       ├── TextAlign.cs
│       ├── MainAxisAlignment.cs
│       └── ...
└── Dart/
    ├── structs/          # Dart FFI struct definitions
    │   ├── container_struct.dart
    │   ├── text_struct.dart
    │   └── ...
    ├── parsers/          # Dart widget parsers
    │   ├── container_parser.dart
    │   ├── text_parser.dart
    │   └── ...
    └── enums/            # Dart enum definitions
        ├── textalign.dart
        ├── mainaxisalignment.dart
        └── ...
```

### 2. `validate` - Validate setup

Checks that Dart tools and Flutter SDK are properly configured.

#### Options

- `--flutter-sdk-path <path>` - Flutter SDK path (auto-detects if not provided)
- `--verbose` - Enable verbose logging

#### Examples

```bash
# Validate default setup
dotnet run -- validate

# Validate specific Flutter SDK
dotnet run -- validate --flutter-sdk-path /path/to/flutter/sdk --verbose
```

### 3. `list-widgets` - List widgets (dry run)

Lists all widgets and types that would be generated without actually generating code.

#### Options

Same as `generate` command (excluding output and update options):
- `--source <sdk|package|local>`
- `--flutter-sdk-path <path>`
- `--package-name <name>`
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

# List widgets from local package
dotnet run -- list-widgets --source local --package-path /path/to/package --verbose
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
  ...

Types (45):
--------------------------------------------------------------------------------
  EdgeInsets (4 properties)
  TextStyle (15 properties)
  ...

Enums (20):
--------------------------------------------------------------------------------
  TextAlign (7 values)
  MainAxisAlignment (6 values)
  ...

================================================================================
Total items: 215
================================================================================
```

## Prerequisites

1. **Dart SDK** - Must be installed and available in PATH
   ```bash
   dart --version
   ```

2. **Flutter SDK** (optional) - Auto-detected or cloned if not provided
   ```bash
   flutter --version
   ```

3. **Dart Analyzer Script** - Located at `Tools/analyzer/package_scanner.dart`

## Environment Variables

- `FLUTTER_ROOT` - Points to Flutter SDK root (used for auto-detection)
- `PUB_CACHE` - Pub cache directory (default: `~/.pub-cache`)

## Build and Run

```bash
# Build the project
dotnet build --configuration Release

# Run directly
dotnet run -- <command> [options]

# Or run the built executable
dotnet bin/Release/net10.0/FlutterSharp.CodeGen.dll <command> [options]
```

## Error Handling

The CLI provides helpful error messages for common issues:

- **Dart not installed**: "Dart is not installed or not found in PATH"
- **Flutter SDK not found**: "Flutter SDK not found. Please provide --flutter-sdk-path"
- **Package not found**: "Package 'name' not found in pub cache"
- **Analyzer script missing**: "Could not find Dart analyzer script (package_scanner.dart)"

Use `--verbose` flag for detailed error information and stack traces.

## Integration Examples

### CI/CD Pipeline

```bash
# Validate setup
dotnet run -- validate

# Generate from Flutter stable
dotnet run -- generate --update-sdk --output-csharp ./src/Generated/CSharp --output-dart ./flutter/lib/generated

# Check for changes
git diff --exit-code || echo "Generated code updated"
```

### Pre-commit Hook

```bash
#!/bin/sh
# Regenerate code if Flutter SDK changed
if git diff --cached --name-only | grep -q "flutter/"; then
    dotnet run -- generate
    git add Generated/
fi
```

## Advanced Usage

### Custom Type Mapping

The tool uses `TypeMappingRegistry` for type conversions. You can extend it by modifying the registry in the code.

### Template Customization

Generators use Scriban templates. You can provide custom templates:
- Place custom templates in `Templates/` directory
- Templates are auto-detected by generators

### Performance Tips

1. Use `--include` to generate only needed widgets
2. Use `--exclude` to skip large widgets you don't need
3. Cache the Flutter SDK path to avoid re-detection
4. Run `validate` before `generate` to catch issues early

## Troubleshooting

### "Dart analyzer script not found"
Ensure `Tools/analyzer/package_scanner.dart` exists in your project directory.

### "Flutter package root not found"
The Flutter SDK may be corrupted. Try:
```bash
dotnet run -- generate --update-sdk
```

### "Package not found in pub cache"
For pub packages, run `dart pub get` or `flutter pub get` first.

### Runtime errors
Ensure you have the correct .NET runtime installed:
```bash
dotnet --info
```

## Contributing

To extend the CLI:
1. Add new commands in `Program.cs`
2. Follow the existing command pattern
3. Use `LogInfo`, `LogVerbose`, `LogSuccess`, and `LogError` for output
4. Add proper error handling with helpful messages
