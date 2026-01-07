# FlutterSharp Type Mapping Specification

## Overview

FlutterSharp maps Dart types to C# types for the binding layer, and both to FFI-compatible types for the interop protocol. This document specifies all type mappings and their marshalling strategies.

## Mapping Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                        Dart Type                                │
│   e.g., String?, Color, Widget, void Function(int)              │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                        C# Type                                  │
│   e.g., string?, Color, Widget, Action<int>                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      C# FFI Type                                │
│   e.g., IntPtr, ColorStruct, IntPtr, IntPtr                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Dart FFI Type                               │
│   e.g., Pointer<Utf8>, ColorStruct, Pointer<WidgetStruct>, ...  │
└─────────────────────────────────────────────────────────────────┘
```

## Primitive Types

### Numeric Types

| Dart Type | C# Type | C# FFI | Dart FFI | Annotation | Size |
|-----------|---------|--------|----------|------------|------|
| `int` | `int` | `int` | `int` | `@Int32()` | 4 |
| `double` | `double` | `double` | `double` | `@Double()` | 8 |
| `num` | `double` | `double` | `double` | `@Double()` | 8 |

### Boolean Type

| Dart Type | C# Type | C# FFI | Dart FFI | Annotation | Size |
|-----------|---------|--------|----------|------------|------|
| `bool` | `bool` | `byte` | `int` | `@Int8()` | 1 |

**Marshalling:**
```csharp
// C# struct
public byte isEnabled;

public bool IsEnabled
{
    get => isEnabled != 0;
    set => isEnabled = value ? (byte)1 : (byte)0;
}
```

```dart
// Dart FFI
@Int8() external int isEnabled;

bool get isEnabledValue => isEnabled != 0;
```

### String Type

| Dart Type | C# Type | C# FFI | Dart FFI | Annotation | Size |
|-----------|---------|--------|----------|------------|------|
| `String` | `string` | `IntPtr` | `Pointer<Utf8>` | - | 8 |
| `String?` | `string?` | `IntPtr` | `Pointer<Utf8>` | - | 8 |

**Marshalling:**
```csharp
// C# struct
private IntPtr _text_ptr;

public string? Text
{
    get => _text_ptr == IntPtr.Zero
           ? null
           : Marshal.PtrToStringUTF8(_text_ptr);
    set {
        // Free existing
        if (_text_ptr != IntPtr.Zero)
            Marshal.FreeCoTaskMem(_text_ptr);
        // Allocate new
        _text_ptr = value == null
           ? IntPtr.Zero
           : Marshal.StringToCoTaskMemUTF8(value);
    }
}
```

```dart
// Dart FFI
external Pointer<Utf8> text;

String? get textValue =>
    text.address == 0 ? null : text.toDartString();
```

## Core Flutter Types

### Color

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Color` | `Color` | `ColorStruct` | `ColorStruct` | 4 |
| `Color?` | `Color?` | `byte` + `ColorStruct` | nullable pattern | 5 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ColorStruct
{
    public uint value;  // ARGB packed

    public static implicit operator Color(ColorStruct s) =>
        new Color(s.value);
    public static implicit operator ColorStruct(Color c) =>
        new ColorStruct { value = c.Value };
}
```

```dart
final class ColorStruct extends Struct {
  @Uint32() external int value;
}

Color toColor(ColorStruct s) => Color(s.value);
```

### EdgeInsets

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `EdgeInsets` | `EdgeInsets` | `EdgeInsetsStruct` | `EdgeInsetsStruct` | 32 |
| `EdgeInsetsGeometry` | `EdgeInsetsGeometry` | `EdgeInsetsStruct` | `EdgeInsetsStruct` | 32 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct EdgeInsetsStruct
{
    public double left;
    public double top;
    public double right;
    public double bottom;
}
```

```dart
final class EdgeInsetsStruct extends Struct {
  @Double() external double left;
  @Double() external double top;
  @Double() external double right;
  @Double() external double bottom;
}

EdgeInsets toEdgeInsets(EdgeInsetsStruct s) =>
    EdgeInsets.only(
      left: s.left,
      top: s.top,
      right: s.right,
      bottom: s.bottom,
    );
```

### Alignment

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Alignment` | `Alignment` | `AlignmentStruct` | `AlignmentStruct` | 16 |
| `AlignmentGeometry` | `AlignmentGeometry` | `AlignmentStruct` | `AlignmentStruct` | 16 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct AlignmentStruct
{
    public double x;
    public double y;
}
```

### BoxConstraints

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `BoxConstraints` | `BoxConstraints` | `BoxConstraintsStruct` | `BoxConstraintsStruct` | 32 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BoxConstraintsStruct
{
    public double minWidth;
    public double maxWidth;
    public double minHeight;
    public double maxHeight;
}
```

### Size

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Size` | `Size` | `SizeStruct` | `SizeStruct` | 16 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct SizeStruct
{
    public double width;
    public double height;
}
```

### Offset

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Offset` | `Offset` | `OffsetStruct` | `OffsetStruct` | 16 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct OffsetStruct
{
    public double dx;
    public double dy;
}
```

### Rect

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Rect` | `Rect` | `RectStruct` | `RectStruct` | 32 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct RectStruct
{
    public double left;
    public double top;
    public double right;
    public double bottom;
}
```

### Duration / TimeSpan

| Dart Type | C# Type | C# FFI | Dart FFI | Annotation | Size |
|-----------|---------|--------|----------|------------|------|
| `Duration` | `TimeSpan` | `long` | `int` | `@Int64()` | 8 |

**Marshalling:**
```csharp
// C# struct - microseconds
public long durationMicroseconds;

public TimeSpan Duration
{
    get => TimeSpan.FromMicroseconds(durationMicroseconds);
    set => durationMicroseconds = (long)value.TotalMicroseconds;
}
```

```dart
// Dart reading
@Int64() external int durationMicroseconds;

Duration get durationValue =>
    Duration(microseconds: durationMicroseconds);
```

### Key

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Key` | `Key` | `IntPtr` | `Pointer<Utf8>` | 8 |
| `Key?` | `Key?` | `IntPtr` | `Pointer<Utf8>` | 8 |

Keys are serialized as their string value.

## Widget Types

### Single Widget

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `Widget` | `Widget` | `IntPtr` | `Pointer<WidgetStruct>` | 8 |
| `Widget?` | `Widget?` | `IntPtr` | `Pointer<WidgetStruct>` | 8 |

**Marshalling:**
```csharp
// C# struct
public IntPtr child;

public Widget? Child
{
    get => child == IntPtr.Zero ? null : FlutterManager.GetWidget(child);
    set {
        if (value != null) {
            value.PrepareForSending();
            child = (IntPtr)value;
        } else {
            child = IntPtr.Zero;
        }
    }
}
```

```dart
// Dart reading
external Pointer<WidgetStruct> child;

Widget? get childWidget =>
    child.address == 0
        ? null
        : DynamicWidgetBuilder.buildFromPointer(child);
```

### Widget List (Children)

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `List<Widget>` | `List<Widget>` | `ChildrenStruct` | `ChildrenStruct` | 16 |
| `List<Widget>?` | `List<Widget>?` | `ChildrenStruct` | `ChildrenStruct` | 16 |

**Struct Definition:**
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ChildrenStruct
{
    public IntPtr items;   // Pointer to IntPtr array
    public int count;
    private int _padding;  // Alignment
}
```

```dart
final class ChildrenStruct extends Struct {
  external Pointer<Pointer<WidgetStruct>> items;
  @Int32() external int count;
}
```

**Marshalling:**
```csharp
// C# setting children
public void SetChildren(List<Widget> children)
{
    var count = children.Count;
    var ptrs = new IntPtr[count];

    for (int i = 0; i < count; i++) {
        children[i].PrepareForSending();
        ptrs[i] = (IntPtr)children[i];
    }

    // Pin array and store pointer
    _childrenHandle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
    _struct.children.items = _childrenHandle.AddrOfPinnedObject();
    _struct.children.count = count;
}
```

```dart
// Dart reading
List<Widget> parseChildren(ChildrenStruct children) {
  if (children.items.address == 0) return [];

  final result = <Widget>[];
  for (var i = 0; i < children.count; i++) {
    final ptr = children.items.elementAt(i).value;
    if (ptr.address != 0) {
      result.add(DynamicWidgetBuilder.buildFromPointer(ptr));
    }
  }
  return result;
}
```

## Callback Types

### VoidCallback / Action

| Dart Type | C# Type | C# FFI | Dart FFI | Size |
|-----------|---------|--------|----------|------|
| `VoidCallback` | `Action` | `IntPtr` | `Pointer<Utf8>` | 8 |
| `void Function()` | `Action` | `IntPtr` | `Pointer<Utf8>` | 8 |

**Marshalling:**
```csharp
// C# struct
private IntPtr _onPressed_ptr;

public Action? OnPressed
{
    set {
        if (value != null) {
            var actionId = CallbackRegistry.Register(value);
            _onPressed_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
        } else {
            _onPressed_ptr = IntPtr.Zero;
        }
    }
}
```

```dart
// Dart reading
external Pointer<Utf8> onPressed;

VoidCallback? get onPressedCallback {
  if (onPressed.address == 0) return null;
  final actionId = onPressed.toDartString();
  return () => MauiRenderer.invokeAction(actionId);
}
```

### Parameterized Callbacks

| Dart Type | C# Type | C# FFI | Size |
|-----------|---------|--------|------|
| `void Function(int)` | `Action<int>` | `IntPtr` | 8 |
| `void Function(String)` | `Action<string>` | `IntPtr` | 8 |
| `void Function(bool)` | `Action<bool>` | `IntPtr` | 8 |
| `ValueChanged<T>` | `Action<T>` | `IntPtr` | 8 |

**Event Data Marshalling:**

When a callback with parameters is invoked, event data is passed via the event message:

```json
{
  "MessageType": "Event",
  "ActionId": "action_1234",
  "Data": {
    "value": 42
  }
}
```

```csharp
// C# event handling
void OnEvent(string actionId, JObject data)
{
    var action = CallbackRegistry.Get<Action<int>>(actionId);
    var value = data["value"].ToObject<int>();
    action?.Invoke(value);
}
```

### Builder Callbacks

| Dart Type | C# Type | C# FFI | Size |
|-----------|---------|--------|------|
| `Widget Function(BuildContext)` | `Func<BuildContext, Widget>` | `IntPtr` | 8 |
| `WidgetBuilder` | `Func<BuildContext, Widget>` | `IntPtr` | 8 |
| `IndexedWidgetBuilder` | `Func<BuildContext, int, Widget>` | `IntPtr` | 8 |

Builder callbacks require special handling as they're called synchronously from Dart:

```dart
// Dart invoking builder
Widget buildChild(BuildContext context) {
  final result = MauiRenderer.invokeBuilder(builderActionId, context);
  return DynamicWidgetBuilder.buildFromPointer(result);
}
```

## Collection Types

### List

| Dart Type | C# Type | Notes |
|-----------|---------|-------|
| `List<int>` | `List<int>` | Inline array + count |
| `List<double>` | `List<double>` | Inline array + count |
| `List<String>` | `List<string>` | Array of pointers |
| `List<Widget>` | `List<Widget>` | ChildrenStruct |
| `List<T>` | `List<T>` | Varies by T |

### Map

| Dart Type | C# Type | Notes |
|-----------|---------|-------|
| `Map<String, dynamic>` | `Dictionary<string, object>` | JSON serialized |
| `Map<K, V>` | `Dictionary<K, V>` | JSON serialized |

Maps are typically serialized as JSON due to their dynamic nature.

### Set

| Dart Type | C# Type | Notes |
|-----------|---------|-------|
| `Set<T>` | `ISet<T>` | As array |

## Nullable Types

### Nullable Value Types Pattern

For nullable value types (int?, double?, bool?, etc.), use a "has" flag:

```csharp
// C# struct
public byte hasOpacity;
public double opacity;

public double? Opacity
{
    get => hasOpacity != 0 ? opacity : null;
    set {
        if (value.HasValue) {
            hasOpacity = 1;
            opacity = value.Value;
        } else {
            hasOpacity = 0;
        }
    }
}
```

```dart
// Dart FFI
@Int8() external int hasOpacity;
@Double() external double opacity;

double? get opacityValue => hasOpacity != 0 ? opacity : null;
```

### Nullable Reference Types

Reference types (strings, widgets) use null pointer (address 0):

```csharp
// C# - nullable string
private IntPtr _text_ptr;

public string? Text
{
    get => _text_ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(_text_ptr);
}
```

```dart
// Dart - check for null pointer
String? get textValue =>
    text.address == 0 ? null : text.toDartString();
```

## Enum Types

### Simple Enums

| Dart Type | C# Type | C# FFI | Dart FFI | Annotation | Size |
|-----------|---------|--------|----------|------------|------|
| `TextAlign` | `TextAlign` | `int` | `int` | `@Int32()` | 4 |
| `Clip` | `Clip` | `int` | `int` | `@Int32()` | 4 |
| Any enum | Same name | `int` | `int` | `@Int32()` | 4 |

**Generation:**
```csharp
// C# enum
public enum TextAlign
{
    Left = 0,
    Right = 1,
    Center = 2,
    Justify = 3,
    Start = 4,
    End = 5,
}
```

```dart
// Dart enum
enum TextAlign {
  left,
  right,
  center,
  justify,
  start,
  end,
}
```

**Struct Usage:**
```csharp
// C# struct
public int textAlign;

public TextAlign TextAlign
{
    get => (TextAlign)textAlign;
    set => textAlign = (int)value;
}
```

## Generic Types

### Generic Type Resolution

Generic types are resolved at code generation time:

| Dart Type | C# Type | FFI Type |
|-----------|---------|----------|
| `List<Widget>` | `List<Widget>` | `ChildrenStruct` |
| `ValueNotifier<int>` | `ValueNotifier<int>` | Custom struct |
| `FutureOr<T>` | `T` or `Task<T>` | Depends on usage |

### Type Parameter Constraints

```csharp
// Generic widget with constraint
public class AnimatedBuilder<T> : Widget where T : struct
{
    // ...
}
```

## Special Types

### Object / dynamic

| Dart Type | C# Type | C# FFI | Notes |
|-----------|---------|--------|-------|
| `Object` | `object` | N/A | JSON serialized |
| `dynamic` | `object` | N/A | JSON serialized |

Dynamic types are serialized as JSON and passed as strings.

### Decoration

| Dart Type | C# Type | C# FFI | Notes |
|-----------|---------|--------|-------|
| `Decoration` | `Decoration` | `DecorationStruct` | Abstract base |
| `BoxDecoration` | `BoxDecoration` | `BoxDecorationStruct` | Concrete |
| `ShapeDecoration` | `ShapeDecoration` | `ShapeDecorationStruct` | Concrete |

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BoxDecorationStruct
{
    public ColorStruct color;
    public IntPtr image;           // DecorationImage
    public IntPtr border;          // BoxBorder
    public IntPtr borderRadius;    // BorderRadius
    public IntPtr boxShadow;       // List<BoxShadow>
    public IntPtr gradient;        // Gradient
    public int backgroundBlendMode;
    public int shape;
}
```

### TextStyle

| Dart Type | C# Type | C# FFI | Size |
|-----------|---------|--------|------|
| `TextStyle` | `TextStyle` | `TextStyleStruct` | ~120 |

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TextStyleStruct
{
    public byte inherit;
    public ColorStruct color;
    public ColorStruct backgroundColor;
    public double fontSize;
    public int fontWeight;
    public int fontStyle;
    public double letterSpacing;
    public double wordSpacing;
    public int textBaseline;
    public double height;
    public IntPtr fontFamily;
    public IntPtr fontFeatures;
    public IntPtr decoration;
    public ColorStruct decorationColor;
    public int decorationStyle;
    public double decorationThickness;
}
```

## Unsupported Types

The following types cannot be directly marshalled and require special handling:

| Dart Type | Reason | Workaround |
|-----------|--------|------------|
| `BuildContext` | Runtime only | Proxy ID |
| `State<T>` | Internal | Not exposed |
| `Element` | Internal | Not exposed |
| `RenderObject` | Internal | Not exposed |
| `GlobalKey` | Complex state | Key string |
| `AnimationController` | Stateful | Separate API |
| `ScrollController` | Stateful | Separate API |
| `FocusNode` | Stateful | Separate API |

## Default Values

### Compile-Time Constants

Default values that are compile-time constants in Dart map directly:

| Dart Default | C# Default |
|--------------|------------|
| `null` | `null` |
| `true` | `true` |
| `false` | `false` |
| `0` | `0` |
| `0.0` | `0.0` |
| `""` | `""` |
| `const EdgeInsets.zero` | `EdgeInsets.Zero` |
| `const Color(0xFF000000)` | `new Color(0xFF000000)` |

### Runtime Values

Some Dart defaults are not compile-time constants in C#:

| Dart Default | C# Approach |
|--------------|-------------|
| `Duration.zero` | `TimeSpan.Zero` (static readonly) |
| `Alignment.center` | `Alignment.Center` (static readonly) |
| `Colors.transparent` | `Colors.Transparent` (static readonly) |

```csharp
// C# - use default(T) and set in constructor body
public Container(
    Color? color = default,
    EdgeInsets? padding = default)
{
    Color = color ?? Colors.Transparent;
    Padding = padding ?? EdgeInsets.Zero;
}
```

## Type Mapping Registry

### Registration API

```csharp
// Register custom type mapping
TypeMappingRegistry.Register(new TypeMapping
{
    DartType = "MyCustomType",
    CSharpType = "MyCustomType",
    CSharpFfiType = "MyCustomTypeStruct",
    DartFfiType = "MyCustomTypeStruct",
    FfiAnnotation = null,
    MarshalStrategy = PropertyMarshalStrategy.NestedStruct,
});
```

### Lookup Priority

1. Exact match in registry
2. Nullable wrapper check
3. Generic type resolution
4. Function type analysis
5. Default to `object` (with warning)

## See Also

- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) - FFI protocol details
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Code generation
- [WIDGET-BINDING.md](./WIDGET-BINDING.md) - Widget binding
