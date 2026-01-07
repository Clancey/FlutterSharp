# FlutterSharp Code Generation Specification

## Overview

The FlutterSharp code generator (`FlutterSharp.CodeGen`) analyzes Dart/Flutter packages and generates:

1. **C# Widget Classes** - Wrapper classes for Flutter widgets
2. **C# Struct Classes** - FFI-compatible struct definitions
3. **C# Enum Classes** - Dart enum equivalents
4. **Dart Struct Definitions** - FFI struct definitions matching C#
5. **Dart Parser Classes** - Widget builders from FFI structs
6. **Dart Enum Definitions** - Enum definitions

## Generation Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                     Source Input                                │
│   Flutter SDK  |  pub.dev Package  |  Local Package            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Dart Analyzer                                │
│   Tools/analyzer/package_scanner.dart                           │
│   - Parses Dart AST                                             │
│   - Extracts widget definitions                                 │
│   - Extracts type definitions                                   │
│   - Extracts enum definitions                                   │
│   - Outputs JSON metadata                                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                  Model Deserialization                          │
│   DartAnalyzerHost.cs                                           │
│   - Deserializes JSON to model objects                          │
│   - Creates WidgetDefinition, PropertyDefinition, etc.          │
│   - Builds complete package definition                          │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Analysis Enrichment                           │
│   WidgetAnalysisEnricher.cs                                     │
│   - Resolves C# types for properties                            │
│   - Resolves Dart FFI types                                     │
│   - Calculates FFI annotations                                  │
│   - Determines marshalling strategies                           │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Type Mapping                                 │
│   TypeMappingRegistry.cs                                        │
│   DartToCSharpMapper.cs                                         │
│   CSharpToDartFfiMapper.cs                                      │
│   - Maps Dart types to C# equivalents                           │
│   - Maps C# types to Dart FFI types                             │
│   - Handles generics, nullability, callbacks                    │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Code Generation                              │
│   Generators/CSharp/CSharpWidgetGenerator.cs                    │
│   Generators/CSharp/CSharpStructGenerator.cs                    │
│   Generators/Dart/DartParserGenerator.cs                        │
│   Generators/Dart/DartStructGenerator.cs                        │
│   - Uses Scriban templates                                      │
│   - Generates source files                                      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      File Output                                │
│   C# files: src/Flutter/Widgets/, Structs/, Enums/              │
│   Dart files: flutter_module/lib/generated/                     │
└─────────────────────────────────────────────────────────────────┘
```

## CLI Interface

### Commands

```bash
# Generate bindings from Flutter SDK
dotnet run -- generate \
  --source sdk \
  --output-csharp ../../src/Flutter \
  --output-dart ../../flutter_module/lib/generated

# Generate bindings from pub.dev package
dotnet run -- generate \
  --source package:provider \
  --output-csharp ./bindings/csharp \
  --output-dart ./bindings/dart

# Generate bindings from local package
dotnet run -- generate \
  --source ./my_package \
  --output-csharp ./bindings/csharp \
  --output-dart ./bindings/dart

# List widgets in a package
dotnet run -- list-widgets --source sdk

# Validate bindings
dotnet run -- validate --source sdk
```

### Options

| Option | Description | Default |
|--------|-------------|---------|
| `--source` | Source type: `sdk`, `package:<name>`, or path | Required |
| `--output-csharp` | C# output directory | `./output/csharp` |
| `--output-dart` | Dart output directory | `./output/dart` |
| `--flutter-sdk` | Flutter SDK path | Auto-detect |
| `--include` | Widget filter pattern | `*` |
| `--exclude` | Widget exclusion pattern | None |
| `--verbose` | Enable verbose output | `false` |

## Dart Analyzer

### Package Scanner Script

Located at `Tools/analyzer/package_scanner.dart`:

```dart
// Analyzes a Dart package and outputs JSON metadata
void main(List<String> args) async {
  final packagePath = args[0];
  final outputPath = args[1];

  final analyzer = PackageAnalyzer();
  final package = await analyzer.analyze(packagePath);

  final json = jsonEncode(package.toJson());
  File(outputPath).writeAsStringSync(json);
}
```

### Extracted Metadata

```json
{
  "name": "widgets",
  "widgets": [
    {
      "name": "Container",
      "library": "package:flutter/widgets.dart",
      "documentation": "A convenience widget that combines...",
      "isAbstract": false,
      "baseClass": "StatelessWidget",
      "constructors": [
        {
          "name": "",
          "isConst": true,
          "parameters": [...]
        }
      ],
      "properties": [
        {
          "name": "color",
          "dartType": "Color?",
          "isRequired": false,
          "isNamed": true,
          "defaultValue": null,
          "documentation": "The color to paint behind the child."
        }
      ]
    }
  ],
  "types": [...],
  "enums": [...]
}
```

## Model Classes

### WidgetDefinition

```csharp
public class WidgetDefinition
{
    public string Name { get; set; }
    public string Library { get; set; }
    public string? Documentation { get; set; }
    public bool IsAbstract { get; set; }
    public string? BaseClass { get; set; }
    public List<ConstructorDefinition> Constructors { get; set; }
    public List<PropertyDefinition> Properties { get; set; }
    public List<string> TypeParameters { get; set; }
}
```

### PropertyDefinition

```csharp
public class PropertyDefinition
{
    // Source metadata
    public string Name { get; set; }
    public string DartType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsNamed { get; set; }
    public string? DefaultValue { get; set; }
    public string? Documentation { get; set; }

    // Enriched data
    public string CSharpType { get; set; }
    public string CSharpFfiType { get; set; }
    public string DartFfiType { get; set; }
    public string? FfiAnnotation { get; set; }
    public PropertyMarshalStrategy MarshalStrategy { get; set; }
}
```

### PropertyMarshalStrategy

```csharp
public enum PropertyMarshalStrategy
{
    Direct,           // Value copied directly (int, double)
    StringPointer,    // String via IntPtr
    WidgetPointer,    // Widget reference via IntPtr
    ChildrenArray,    // List<Widget> via ChildrenStruct
    NestedStruct,     // Inline struct (Color, EdgeInsets)
    NullableFlag,     // has_X flag + value
    Callback,         // Action ID string
    Unsupported       // Cannot marshal
}
```

## Type Mapping Registry

### Built-in Mappings

```csharp
public class TypeMappingRegistry
{
    private static readonly Dictionary<string, TypeMapping> _mappings = new()
    {
        // Primitives
        ["int"] = new("int", "Int32", "@Int32()"),
        ["double"] = new("double", "Double", "@Double()"),
        ["bool"] = new("bool", "Int8", "@Int8()"),
        ["String"] = new("string", "Pointer<Utf8>", null),
        ["num"] = new("double", "Double", "@Double()"),

        // Core types
        ["Widget"] = new("Widget", "Pointer<WidgetStruct>", null),
        ["Color"] = new("Color", "ColorStruct", null),
        ["EdgeInsets"] = new("EdgeInsets", "EdgeInsetsStruct", null),
        ["EdgeInsetsGeometry"] = new("EdgeInsetsGeometry", "EdgeInsetsStruct", null),
        ["Alignment"] = new("Alignment", "AlignmentStruct", null),
        ["AlignmentGeometry"] = new("AlignmentGeometry", "AlignmentStruct", null),
        ["BoxConstraints"] = new("BoxConstraints", "BoxConstraintsStruct", null),
        ["Duration"] = new("TimeSpan", "Int64", "@Int64()"),
        ["Key"] = new("Key", "Pointer<Utf8>", null),

        // Callbacks
        ["VoidCallback"] = new("Action", "Pointer<Utf8>", null),
        ["GestureTapCallback"] = new("Action", "Pointer<Utf8>", null),
        ["ValueChanged<T>"] = new("Action<T>", "Pointer<Utf8>", null),

        // Collections
        ["List<Widget>"] = new("List<Widget>", "Pointer<ChildrenStruct>", null),
        ["List<Widget>?"] = new("List<Widget>?", "Pointer<ChildrenStruct>", null),

        // ... 500+ more mappings
    };

    public static TypeMapping? GetMapping(string dartType)
    {
        if (_mappings.TryGetValue(dartType, out var mapping))
            return mapping;

        // Handle generics, nullability, etc.
        return TryResolveGeneric(dartType);
    }
}
```

### Type Mapping Resolution

```csharp
public class DartToCSharpMapper
{
    public string MapType(string dartType)
    {
        // 1. Check registry
        var mapping = TypeMappingRegistry.GetMapping(dartType);
        if (mapping != null)
            return mapping.CSharpType;

        // 2. Handle nullable
        if (dartType.EndsWith("?"))
        {
            var baseType = dartType[..^1];
            return MapType(baseType) + "?";
        }

        // 3. Handle generics
        if (dartType.Contains("<"))
        {
            return MapGenericType(dartType);
        }

        // 4. Handle function types
        if (dartType.Contains("Function"))
        {
            return MapFunctionType(dartType);
        }

        // 5. Unknown type - use placeholder
        return "object";
    }

    private string MapGenericType(string dartType)
    {
        // List<T> -> List<T>
        // Map<K,V> -> Dictionary<K,V>
        // Set<T> -> HashSet<T>
        // Future<T> -> Task<T>
        // ...
    }

    private string MapFunctionType(string dartType)
    {
        // void Function() -> Action
        // void Function(int) -> Action<int>
        // int Function() -> Func<int>
        // Widget Function(BuildContext) -> Func<BuildContext, Widget>
        // ...
    }
}
```

## Scriban Templates

### CSharpWidget.scriban

```scriban
// Auto-generated - do not edit
using System;
using System.Runtime.InteropServices;
using FlutterSharp.Structs;

namespace FlutterSharp.Widgets
{
    /// <summary>
    {{ documentation | string.strip }}
    /// </summary>
    public {{ if is_abstract }}abstract {{ end }}class {{ name }} : {{ base_class }}
    {
        private {{ struct_name }} _struct;

        protected override BaseStruct BackingStruct => _struct;

        {{ for prop in properties }}
        /// <summary>
        /// {{ prop.documentation | string.strip }}
        /// </summary>
        public {{ prop.csharp_type }} {{ prop.name | pascal_case }}
        {
            {{ if prop.has_getter }}
            get => {{ prop.getter_expression }};
            {{ end }}
            {{ if prop.has_setter }}
            set => {{ prop.setter_expression }};
            {{ end }}
        }
        {{ end }}

        public {{ name }}(
            {{ for prop in required_properties }}
            {{ prop.csharp_type }} {{ prop.name | camel_case }}{{ if !for.last }},{{ end }}
            {{ end }}
            {{ if required_properties.size > 0 && optional_properties.size > 0 }},{{ end }}
            {{ for prop in optional_properties }}
            {{ prop.csharp_type }} {{ prop.name | camel_case }} = {{ prop.default_value }}{{ if !for.last }},{{ end }}
            {{ end }}
        )
        {
            {{ for prop in all_properties }}
            this.{{ prop.name | pascal_case }} = {{ prop.name | camel_case }};
            {{ end }}
        }
    }
}
```

### CSharpStruct.scriban

```scriban
// Auto-generated - do not edit
using System;
using System.Runtime.InteropServices;

namespace FlutterSharp.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct {{ struct_name }}
    {
        // Base fields
        public IntPtr handle;
        public IntPtr managedHandle;
        public int widgetType;
        public IntPtr id;

        {{ for prop in properties }}
        {{ if prop.is_nullable && prop.is_value_type }}
        public byte has{{ prop.name | pascal_case }};
        {{ end }}
        {{ if prop.is_string }}
        private IntPtr _{{ prop.name | camel_case }}_ptr;

        public string? {{ prop.name | pascal_case }}
        {
            get => _{{ prop.name | camel_case }}_ptr == IntPtr.Zero
                   ? null
                   : Marshal.PtrToStringUTF8(_{{ prop.name | camel_case }}_ptr);
            set => _{{ prop.name | camel_case }}_ptr = value == null
                   ? IntPtr.Zero
                   : Marshal.StringToCoTaskMemUTF8(value);
        }
        {{ else }}
        public {{ prop.csharp_ffi_type }} {{ prop.name | camel_case }};
        {{ end }}
        {{ end }}
    }
}
```

### DartParser.scriban

```scriban
// Auto-generated - do not edit
import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:flutter/widgets.dart';
import '../structs/{{ widget_name | snake_case }}_struct.dart';
import '../../maui_flutter.dart';

class {{ parser_name }} {
  Widget parse(Pointer<{{ struct_name }}> ptr) {
    final struct = ptr.ref;

    return {{ widget_name }}(
      {{ for prop in properties }}
      {{ if prop.is_required }}
      {{ prop.name }}: {{ prop.parse_expression }},
      {{ else }}
      {{ if prop.is_nullable }}
      {{ prop.name }}: {{ prop.parse_expression }},
      {{ else }}
      {{ prop.name }}: {{ prop.parse_expression }} ?? {{ prop.default_value }},
      {{ end }}
      {{ end }}
      {{ end }}
      {{ if has_child }}
      child: struct.child.address != 0
          ? DynamicWidgetBuilder.buildFromPointer(struct.child)
          : null,
      {{ end }}
      {{ if has_children }}
      children: _parseChildren(struct.children),
      {{ end }}
    );
  }

  {{ if has_children }}
  List<Widget> _parseChildren(Pointer<ChildrenStruct> ptr) {
    if (ptr.address == 0) return [];
    final children = ptr.ref;
    final result = <Widget>[];
    for (var i = 0; i < children.count; i++) {
      final childPtr = children.items.elementAt(i).value;
      result.add(DynamicWidgetBuilder.buildFromPointer(childPtr));
    }
    return result;
  }
  {{ end }}
}
```

### DartStruct.scriban

```scriban
// Auto-generated - do not edit
import 'dart:ffi';
import 'package:ffi/ffi.dart';

abstract class I{{ struct_name }} {
  {{ for prop in properties }}
  {{ prop.dart_type }} get {{ prop.name }};
  {{ end }}
}

final class {{ struct_name }} extends Struct implements I{{ struct_name }} {
  // Base fields
  external Pointer<Void> handle;
  external Pointer<Void> managedHandle;
  @Int32() external int widgetType;
  external Pointer<Utf8> id;

  {{ for prop in properties }}
  {{ if prop.is_nullable && prop.is_value_type }}
  @Int8() external int has{{ prop.name | pascal_case }};
  {{ end }}
  {{ if prop.ffi_annotation }}
  {{ prop.ffi_annotation }}
  {{ end }}
  external {{ prop.dart_ffi_type }} {{ prop.name }};
  {{ end }}
}
```

## Generation Phases

### Phase 1: Analysis

```csharp
public async Task<PackageDefinition> AnalyzePackageAsync(string source)
{
    // 1. Locate package
    var packagePath = ResolvePackagePath(source);

    // 2. Run Dart analyzer
    var jsonPath = Path.GetTempFileName();
    await RunDartAnalyzer(packagePath, jsonPath);

    // 3. Deserialize JSON
    var json = await File.ReadAllTextAsync(jsonPath);
    var package = JsonSerializer.Deserialize<PackageDefinition>(json);

    return package;
}
```

### Phase 2: Enrichment

```csharp
public void EnrichPackage(PackageDefinition package)
{
    foreach (var widget in package.Widgets)
    {
        foreach (var prop in widget.Properties)
        {
            // Resolve C# type
            prop.CSharpType = DartToCSharpMapper.MapType(prop.DartType);

            // Resolve FFI types
            prop.CSharpFfiType = CSharpToFfiMapper.MapType(prop.CSharpType);
            prop.DartFfiType = DartFfiMapper.MapType(prop.DartType);

            // Determine marshal strategy
            prop.MarshalStrategy = DetermineMarshalStrategy(prop);

            // Get FFI annotation
            prop.FfiAnnotation = GetFfiAnnotation(prop.DartFfiType);
        }
    }
}
```

### Phase 3: Generation

```csharp
public async Task GenerateAsync(PackageDefinition package, GenerationOptions options)
{
    // Load templates
    var widgetTemplate = Template.Parse(LoadTemplate("CSharpWidget.scriban"));
    var structTemplate = Template.Parse(LoadTemplate("CSharpStruct.scriban"));
    var parserTemplate = Template.Parse(LoadTemplate("DartParser.scriban"));
    var dartStructTemplate = Template.Parse(LoadTemplate("DartStruct.scriban"));

    // Generate C# widgets
    foreach (var widget in package.Widgets)
    {
        var model = BuildWidgetModel(widget);
        var code = widgetTemplate.Render(model);
        await WriteFileAsync(
            Path.Combine(options.CSharpOutput, "Widgets", $"{widget.Name}.cs"),
            code);
    }

    // Generate C# structs
    foreach (var widget in package.Widgets)
    {
        var model = BuildStructModel(widget);
        var code = structTemplate.Render(model);
        await WriteFileAsync(
            Path.Combine(options.CSharpOutput, "Structs", $"{widget.Name}Struct.cs"),
            code);
    }

    // Generate Dart parsers
    foreach (var widget in package.Widgets)
    {
        var model = BuildParserModel(widget);
        var code = parserTemplate.Render(model);
        await WriteFileAsync(
            Path.Combine(options.DartOutput, "parsers", $"{ToSnakeCase(widget.Name)}_parser.dart"),
            code);
    }

    // Generate Dart structs
    foreach (var widget in package.Widgets)
    {
        var model = BuildDartStructModel(widget);
        var code = dartStructTemplate.Render(model);
        await WriteFileAsync(
            Path.Combine(options.DartOutput, "structs", $"{ToSnakeCase(widget.Name)}_struct.dart"),
            code);
    }

    // Generate registry files
    await GenerateParserRegistry(package, options);
    await GenerateTypeRegistry(package, options);
}
```

### Phase 4: Registry Generation

```csharp
public async Task GenerateParserRegistry(PackageDefinition package, GenerationOptions options)
{
    var sb = new StringBuilder();
    sb.AppendLine("// Auto-generated parser registry");
    sb.AppendLine("import 'maui_flutter.dart';");
    sb.AppendLine();

    foreach (var widget in package.Widgets)
    {
        sb.AppendLine($"import 'parsers/{ToSnakeCase(widget.Name)}_parser.dart';");
    }

    sb.AppendLine();
    sb.AppendLine("void registerParsers() {");

    foreach (var widget in package.Widgets)
    {
        sb.AppendLine($"  DynamicWidgetBuilder.registerParser({widget.TypeId}, {widget.Name}Parser());");
    }

    sb.AppendLine("}");

    await WriteFileAsync(
        Path.Combine(options.DartOutput, "generated_parsers.dart"),
        sb.ToString());
}
```

## Widget Filtering

### Inclusion/Exclusion Rules

```csharp
public bool ShouldIncludeWidget(WidgetDefinition widget, GenerationOptions options)
{
    // Skip abstract widgets
    if (widget.IsAbstract)
        return false;

    // Skip private widgets
    if (widget.Name.StartsWith("_"))
        return false;

    // Apply include pattern
    if (options.IncludePattern != null)
    {
        if (!Regex.IsMatch(widget.Name, options.IncludePattern))
            return false;
    }

    // Apply exclude pattern
    if (options.ExcludePattern != null)
    {
        if (Regex.IsMatch(widget.Name, options.ExcludePattern))
            return false;
    }

    // Check for unsupported properties
    if (widget.Properties.Any(p => p.MarshalStrategy == PropertyMarshalStrategy.Unsupported))
    {
        Console.WriteLine($"Warning: Skipping {widget.Name} - has unsupported properties");
        return false;
    }

    return true;
}
```

### Default Exclusions

```csharp
private static readonly HashSet<string> ExcludedWidgets = new()
{
    // Render objects (not widgets)
    "RenderObjectWidget",
    "RenderBox",

    // Internal widgets
    "InheritedWidget",
    "ParentDataWidget",

    // Builder widgets (require closures)
    "LayoutBuilder",
    "StatefulBuilder",

    // Platform-specific
    "CupertinoApp",
    "MaterialApp",
};
```

## Error Handling

### Validation Errors

```csharp
public ValidationResult Validate(PackageDefinition package)
{
    var errors = new List<ValidationError>();

    foreach (var widget in package.Widgets)
    {
        // Check for duplicate property names
        var duplicates = widget.Properties
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1);
        foreach (var dup in duplicates)
        {
            errors.Add(new ValidationError(
                widget.Name,
                $"Duplicate property: {dup.Key}"));
        }

        // Check for unmapped types
        foreach (var prop in widget.Properties)
        {
            if (prop.CSharpType == "object")
            {
                errors.Add(new ValidationError(
                    widget.Name,
                    $"Unmapped type for property {prop.Name}: {prop.DartType}"));
            }
        }
    }

    return new ValidationResult(errors);
}
```

### Recovery Strategies

```csharp
public void HandleGenerationError(WidgetDefinition widget, Exception ex)
{
    Console.WriteLine($"Error generating {widget.Name}: {ex.Message}");

    // Generate stub file
    var stub = $@"
// Generation failed for {widget.Name}
// Error: {ex.Message}
// This widget requires manual implementation

namespace FlutterSharp.Widgets
{{
    public class {widget.Name} : Widget
    {{
        // TODO: Implement manually
    }}
}}";

    WriteFile(GetOutputPath(widget, "stub"), stub);
}
```

## Extensibility

### Custom Type Mappings

```csharp
// Add custom type mapping
TypeMappingRegistry.RegisterMapping(
    dartType: "MyCustomType",
    mapping: new TypeMapping(
        csharpType: "MyCustomType",
        ffiType: "Pointer<MyCustomTypeStruct>",
        annotation: null
    )
);
```

### Custom Generators

```csharp
public interface ICodeGenerator
{
    string GeneratedFileExtension { get; }
    Task GenerateAsync(WidgetDefinition widget, TextWriter writer);
}

// Register custom generator
GeneratorRegistry.Register("my-format", new MyCustomGenerator());

// Use custom generator
dotnet run -- generate --source sdk --format my-format
```

### Template Customization

```csharp
// Override template
GeneratorOptions.TemplateOverrides["CSharpWidget"] = @"
// Custom template
public class {{ name }} : Widget
{
    // Custom implementation
}
";
```

## Output Statistics

### Generation Summary

```
FlutterSharp Code Generation Summary
=====================================
Source: Flutter SDK 3.16.0
Output: ./generated

Widgets Generated:     412
Structs Generated:     412
Types Generated:       287
Enums Generated:       108
Parsers Generated:     412

Warnings:              23
  - Unsupported types: 15
  - Deprecated widgets: 8

Errors:                0

Total Files:          1,631
Total Lines:          89,423
Generation Time:      12.4s
```

## See Also

- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Detailed type mapping rules
- [WIDGET-BINDING.md](./WIDGET-BINDING.md) - Widget binding specification
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Overall architecture
