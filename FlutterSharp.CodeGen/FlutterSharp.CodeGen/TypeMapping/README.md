# FlutterSharp Type Mapping System

The type mapping system provides a comprehensive framework for translating types between Dart, C#, and Dart FFI (Foreign Function Interface) representations.

## Overview

The type mapping system consists of four main components:

1. **TypeMapping** - Record representing a single type mapping
2. **TypeMappingRegistry** - Central registry storing all type mappings
3. **DartToCSharpMapper** - Maps Dart types to C# types
4. **CSharpToDartFfiMapper** - Maps C# types to Dart FFI struct types

## Quick Start

```csharp
using FlutterSharp.CodeGen.TypeMapping;

// Create registry with default mappings
var registry = new TypeMappingRegistry();

// Create mappers
var dartToCSharp = new DartToCSharpMapper(registry);
var csharpToFfi = new CSharpToDartFfiMapper(registry);

// Map a simple type
string csharpType = dartToCSharp.MapType("String"); // Returns: "string"
string ffiType = csharpToFfi.MapToFfiType("string"); // Returns: "Pointer<Utf8>"

// Map a nullable type
string nullableType = dartToCSharp.MapType("int?"); // Returns: "int?"

// Map a generic type
string listType = dartToCSharp.MapType("List<String>"); // Returns: "List<string>"

// Map a complex Flutter type
string widgetType = dartToCSharp.MapType("Widget"); // Returns: "Widget"
```

## Supported Type Categories

### Primitive Types
- `String` → `string`
- `int` → `int`
- `double` → `double`
- `bool` → `bool`
- `num` → `double`
- `dynamic` → `object`
- `Object` → `object`
- `void` → `void`

### Flutter Core Types
- `Key` → `Key`
- `Duration` → `TimeSpan`
- `DateTime` → `DateTime`

### Flutter Widget Types
- `Widget` → `Widget`
- `StatelessWidget` → `StatelessWidget`
- `StatefulWidget` → `StatefulWidget`
- `PreferredSizeWidget` → `PreferredSizeWidget`
- `BuildContext` → `BuildContext`

### Flutter Material Types
- `Color` → `Color`
- `MaterialColor` → `MaterialColor`
- `MaterialStateProperty<T>` → `MaterialStateProperty<T>`
- `ThemeData` → `ThemeData`
- `IconData` → `IconData`

### Flutter Painting Types
- `EdgeInsets` → `EdgeInsets`
- `EdgeInsetsGeometry` → `EdgeInsetsGeometry`
- `Alignment` → `Alignment`
- `AlignmentGeometry` → `AlignmentGeometry`
- `BorderRadius` → `BorderRadius`
- `BoxDecoration` → `BoxDecoration`
- `TextStyle` → `TextStyle`
- `Gradient` → `Gradient`

### Flutter Rendering Types
- `Size` → `Size`
- `Offset` → `Offset`
- `Rect` → `Rect`
- `Radius` → `Radius`
- `Matrix4` → `Matrix4`
- `BoxConstraints` → `BoxConstraints`

### Collection Types
- `List<T>` → `List<T>`
- `Set<T>` → `HashSet<T>`
- `Map<K,V>` → `Dictionary<K,V>`
- `Iterable<T>` → `IEnumerable<T>`

### Callback Types
- `VoidCallback` → `Action`
- `ValueChanged<T>` → `Action<T>`
- `ValueSetter<T>` → `Action<T>`
- `ValueGetter<T>` → `Func<T>`
- `GestureTapCallback` → `Action`

### Enum Types
- `TextAlign` → `TextAlign`
- `MainAxisAlignment` → `MainAxisAlignment`
- `CrossAxisAlignment` → `CrossAxisAlignment`
- `Axis` → `Axis`
- `FontWeight` → `FontWeight`
- `FontStyle` → `FontStyle`
- `Clip` → `Clip`
- `BoxFit` → `BoxFit`
- And more...

## Advanced Usage

### Getting Type Information

```csharp
var registry = new TypeMappingRegistry();
var mapper = new DartToCSharpMapper(registry);

// Get full type mapping information
TypeMapping? mapping = mapper.MapTypeWithInfo("Widget");
if (mapping != null)
{
    Console.WriteLine($"Dart Type: {mapping.DartType}");
    Console.WriteLine($"C# Type: {mapping.CSharpType}");
    Console.WriteLine($"FFI Type: {mapping.DartStructType}");
    Console.WriteLine($"Parser Function: {mapping.DartParserFunction}");
    Console.WriteLine($"Is Widget: {mapping.IsWidget}");
    Console.WriteLine($"Requires Custom Marshalling: {mapping.RequiresCustomMarshalling}");
}

// Check type characteristics
bool isWidget = mapper.IsWidget("Container"); // true
bool isCollection = mapper.IsCollection("List<String>"); // true
bool isEnum = mapper.IsEnum("TextAlign"); // true
```

### FFI Mapping and Conversion

```csharp
var registry = new TypeMappingRegistry();
var ffiMapper = new CSharpToDartFfiMapper(registry);

// Get FFI type
string ffiType = ffiMapper.MapToFfiType("string"); // "Pointer<Utf8>"

// Get parser function
string? parser = ffiMapper.GetParserFunction("string"); // "parseString"

// Get conversion code
string toFfi = ffiMapper.GetCSharpToDartConversion("string", "myValue");
// Returns: "myValue.ToNativeUtf8()"

string fromFfi = ffiMapper.GetDartToCSharpConversion("string", "pointer");
// Returns: "Marshal.PtrToStringUTF8(pointer)"

// Check if custom marshalling is needed
bool needsCustom = ffiMapper.RequiresCustomMarshalling("Widget"); // true

// Get FFI type size
int? size = ffiMapper.GetFfiTypeSize("int"); // 4
```

### Handling Generic Types

```csharp
var registry = new TypeMappingRegistry();
var mapper = new DartToCSharpMapper(registry);

// Simple generic
string listOfStrings = mapper.MapType("List<String>");
// Returns: "List<string>"

// Nested generic
string complexType = mapper.MapType("Map<String, List<Widget>>");
// Returns: "Dictionary<string, List<Widget>>"

// Generic with nullable type argument
string nullableGeneric = mapper.MapType("List<int?>");
// Returns: "List<int?>"
```

### Custom Type Mappings

```csharp
var registry = new TypeMappingRegistry();

// Register a custom type mapping
registry.RegisterMapping(new TypeMapping
{
    DartType = "MyCustomWidget",
    CSharpType = "MyCustomWidget",
    DartStructType = "Pointer<Void>",
    DartParserFunction = "parseMyCustomWidget",
    IsWidget = true,
    IsCustomType = true,
    Package = "my_package",
    RequiresCustomMarshalling = true,
    Metadata = new Dictionary<string, object>
    {
        ["Description"] = "Custom widget from my_package",
        ["Version"] = "1.0.0"
    }
});

// Use the custom mapping
var mapper = new DartToCSharpMapper(registry);
string mapped = mapper.MapType("MyCustomWidget"); // Returns: "MyCustomWidget"
```

### Querying the Registry

```csharp
var registry = new TypeMappingRegistry();

// Get all mappings
IEnumerable<TypeMapping> all = registry.GetAllMappings();

// Get only widget mappings
IEnumerable<TypeMapping> widgets = registry.GetWidgetMappings();

// Get only primitive mappings
IEnumerable<TypeMapping> primitives = registry.GetPrimitiveMappings();

// Get only collection mappings
IEnumerable<TypeMapping> collections = registry.GetCollectionMappings();

// Check if mapping exists
bool exists = registry.HasMapping("String"); // true

// Get specific mapping
TypeMapping? mapping = registry.GetMapping("Widget");
TypeMapping? mappingByCSharp = registry.GetMappingByCSharpType("string");
```

### Working with Nullable Types

```csharp
var registry = new TypeMappingRegistry();

// Get a mapping
TypeMapping? mapping = registry.GetMapping("String");

// Create nullable version
TypeMapping? nullable = mapping?.AsNullable();
Console.WriteLine(nullable?.IsNullable); // true

// Create non-nullable version
TypeMapping? nonNullable = nullable?.AsNonNullable();
Console.WriteLine(nonNullable?.IsNullable); // false
```

## Type Mapping Properties

Each `TypeMapping` includes the following properties:

- **DartType** - The Dart type name
- **CSharpType** - The C# type name
- **DartStructType** - The FFI struct type for marshalling
- **DartParserFunction** - Function name to parse FFI values in Dart
- **IsNullable** - Whether the type is nullable
- **IsCollection** - Whether the type is a collection
- **IsWidget** - Whether the type is a Flutter widget
- **IsEnum** - Whether the type is an enum
- **IsPrimitive** - Whether the type is a primitive
- **IsGeneric** - Whether the type is generic
- **GenericArguments** - Generic type arguments
- **RequiresCustomMarshalling** - Whether custom marshalling is needed
- **CSharpToDartConversion** - Template for C# to Dart conversion
- **DartToCSharpConversion** - Template for Dart to C# conversion
- **IsCustomType** - Whether this is a user-defined type
- **Package** - The Dart package this type belongs to
- **Metadata** - Additional metadata dictionary

## Extensibility

The system is designed to be extensible:

1. **Add new mappings** via `RegisterMapping()`
2. **Override existing mappings** by registering with the same type name
3. **Add metadata** to mappings for custom processing
4. **Clear and rebuild** the registry as needed

## FFI Struct Types Reference

Common FFI struct types used:

- **Primitives**: `Int32`, `Double`, `Bool`, `Uint32`, `Int64`
- **Strings**: `Pointer<Utf8>`
- **Objects/Widgets**: `Pointer<Void>`
- **Callbacks**: `Pointer<NativeFunction<...>>`

## Parser Functions

Parser functions on the Dart side convert FFI types back to Dart types:

- `parseString(Pointer<Utf8>)` → `String`
- `parseInt(int)` → `int`
- `parseDouble(double)` → `double`
- `parseBool(int)` → `bool`
- `parseWidget(Pointer<Void>)` → `Widget`
- `parseList(Pointer<Void>)` → `List<T>`
- `parseColor(int)` → `Color`
- And more...

## Best Practices

1. **Always use the registry** for type lookups rather than hardcoding mappings
2. **Handle nullable types** explicitly in your code generation
3. **Check `RequiresCustomMarshalling`** before generating FFI code
4. **Use generic type parsing** for complex types like `List<Widget>`
5. **Register custom types** early in your pipeline
6. **Leverage metadata** for additional type information
7. **Test edge cases** like deeply nested generics

## Integration Example

```csharp
// Example: Generate C# property from Dart property definition
public string GenerateProperty(PropertyDefinition prop)
{
    var registry = new TypeMappingRegistry();
    var mapper = new DartToCSharpMapper(registry);

    string csharpType = mapper.MapType(prop.DartType);
    string nullable = prop.IsNullable ? "?" : "";
    string required = prop.IsRequired ? "required " : "";

    return $"public {required}{csharpType}{nullable} {prop.Name} {{ get; set; }}";
}
```

## Future Enhancements

Potential areas for extension:

- Support for more third-party package types
- Custom marshalling code generation
- Type validation and compatibility checking
- Automatic type discovery from Dart source files
- Performance optimizations for large type sets
- Support for Dart 3.x features (sealed classes, patterns, etc.)
