# FlutterSharp Architecture

## System Overview

FlutterSharp uses a layered architecture with three primary layers:

```
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                          │
│   .NET MAUI Application using C# Flutter Widget API             │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Binding Layer                             │
│   C# Widget Classes ←→ Memory-Shared Structs ←→ Dart Parsers   │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Engine Layer                             │
│   FlutterManager (C#) ←→ MethodChannel ←→ MauiRenderer (Dart)   │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Flutter Engine                            │
│   Native Flutter rendering on platform                          │
└─────────────────────────────────────────────────────────────────┘
```

## Component Architecture

### C# Side (`src/Flutter/`)

```
src/Flutter/
├── Core/
│   ├── Widget.cs              # Base widget class
│   ├── FlutterManager.cs      # Widget lifecycle management
│   ├── Communicator.cs        # Platform channel bridge
│   └── FlutterStructs.Base.cs # FFI struct foundation
├── Widgets/
│   ├── Container.cs           # Generated widget classes
│   ├── Text.cs
│   └── ... (400+ widgets)
├── Structs/
│   ├── ContainerStruct.cs     # Generated FFI structs
│   ├── TextStruct.cs
│   └── ... (400+ structs)
├── Enums/
│   ├── TextAlign.cs           # Generated enums
│   └── ... (100+ enums)
└── Types/
    ├── Color.cs               # Non-widget types
    ├── EdgeInsets.cs
    └── ... (300+ types)
```

### Dart Side (`flutter_module/lib/`)

```
flutter_module/lib/
├── Core/
│   ├── maui_flutter.dart      # Widget builder system
│   ├── mauiRenderer.dart      # Communication orchestration
│   └── flutter_sharp_structs.dart  # Base FFI structs
├── Generated/
│   ├── Widgets/
│   │   ├── container_parser.dart   # Widget parsers
│   │   └── ... (400+ parsers)
│   ├── Structs/
│   │   ├── container_struct.dart   # FFI struct definitions
│   │   └── ... (400+ structs)
│   └── Enums/
│       └── ... (100+ enums)
└── Utilities/
    ├── generated_parsers.dart       # Parser registry
    └── generated_utility_parsers.dart
```

### Code Generation (`FlutterSharp.CodeGen/`)

```
FlutterSharp.CodeGen/
├── Program.cs                  # CLI entry point
├── Analysis/
│   ├── DartAnalyzerHost.cs    # Dart analyzer orchestration
│   └── WidgetAnalysisEnricher.cs  # Property enrichment
├── Models/
│   ├── WidgetDefinition.cs    # Widget metadata
│   ├── PropertyDefinition.cs  # Property metadata
│   └── ...
├── TypeMapping/
│   ├── TypeMappingRegistry.cs # Built-in mappings
│   ├── DartToCSharpMapper.cs  # Dart → C# mapping
│   └── CSharpToDartFfiMapper.cs # C# → Dart FFI mapping
├── Generators/
│   ├── CSharp/
│   │   ├── CSharpWidgetGenerator.cs
│   │   └── CSharpStructGenerator.cs
│   └── Dart/
│       ├── DartParserGenerator.cs
│       └── DartStructGenerator.cs
├── Templates/
│   ├── CSharpWidget.scriban
│   ├── CSharpStruct.scriban
│   ├── DartParser.scriban
│   └── DartStruct.scriban
└── Tools/
    └── analyzer/
        └── package_scanner.dart  # Dart-side analysis
```

## Class Hierarchy

### Widget Base Class

```csharp
public abstract class Widget : IDisposable
{
    // Identity
    public string Id { get; }

    // FFI backing struct
    protected abstract BaseStruct BackingStruct { get; }

    // Memory management
    private GCHandle _handle;
    public IntPtr Pointer => _handle.AddrOfPinnedObject();

    // Lifecycle
    public void PrepareForSending();
    public void Dispose();

    // Implicit conversion for FFI
    public static implicit operator IntPtr(Widget w) => w.Pointer;
}
```

### BaseStruct Foundation

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BaseStruct
{
    public IntPtr handle;           // GCHandle to widget
    public IntPtr managedHandle;    // Additional managed reference
    public int widgetType;          // Type discriminator
    public IntPtr id;               // Widget ID (UTF8)
}
```

### FlutterManager Lifecycle

```csharp
public static class FlutterManager
{
    // Active widgets by ID
    private static WeakDictionary<string, Widget> AliveWidgets;

    // Root widget
    public static Widget? RootWidget { get; set; }

    // Communication
    public static void SendWidget(Widget widget);
    public static void UpdateWidget(Widget widget);

    // Event routing
    internal static void RouteEvent(string widgetId, string eventType, object data);

    // Cleanup
    public static void Dispose(Widget widget);
}
```

## Data Flow Architecture

### Widget Creation Flow

```
┌────────────────┐
│  User creates  │
│  C# Widget     │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ Widget assigns │     │ BackingStruct   │
│ properties     │────▶│ fields updated  │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐
│ PrepareFor-    │
│ Sending()      │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ GCHandle pins  │     │ Pointer to      │
│ struct memory  │────▶│ struct obtained │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐
│ FlutterManager │
│ .SendWidget()  │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ JSON message   │
│ with pointer   │
│ sent via       │
│ MethodChannel  │
└────────────────┘
```

### Dart Rendering Flow

```
┌────────────────┐
│ MethodChannel  │
│ receives msg   │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ MauiRenderer   │     │ Extract pointer │
│ processes      │────▶│ from message    │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐
│ Pointer.from-  │
│ Address(addr)  │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ Read widgetType│     │ Lookup parser   │
│ discriminator  │────▶│ in registry     │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐
│ Parser reads   │
│ struct fields  │
│ builds widget  │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ MauiComponent  │
│ updates state  │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ Flutter        │
│ rebuilds UI    │
└────────────────┘
```

### Event Flow (Dart → C#)

```
┌────────────────┐
│ User taps      │
│ Flutter widget │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ onPressed      │
│ callback fires │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ Callback has   │     │ Look up action  │
│ action ID      │────▶│ string in map   │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐
│ Send Event     │
│ via Channel    │
│ {widgetId,     │
│  eventType,    │
│  actionId}     │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│ C# receives    │
│ event message  │
└───────┬────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ FlutterManager │     │ Lookup widget   │
│ routes event   │────▶│ by ID           │
└───────┬────────┘     └─────────────────┘
        │
        ▼
┌────────────────┐     ┌─────────────────┐
│ CallbackRegistry │   │ Invoke C#       │
│ resolves action  │──▶│ delegate        │
└────────────────┘     └─────────────────┘
```

## Memory Architecture

### Memory Layout Principle

C# and Dart share the **exact same memory layout** for struct data:

```
Memory Address: 0x7F8A4C2000
┌────────────────────────────────────────────────────────────────┐
│ Offset 0:   handle (8 bytes)        - IntPtr / Pointer<Void>  │
│ Offset 8:   managedHandle (8 bytes) - IntPtr / Pointer<Void>  │
│ Offset 16:  widgetType (4 bytes)    - int / Int32             │
│ Offset 20:  padding (4 bytes)       - alignment               │
│ Offset 24:  id (8 bytes)            - IntPtr / Pointer<Utf8>  │
│ Offset 32:  color (16 bytes)        - ColorStruct             │
│ Offset 48:  child (8 bytes)         - IntPtr / Pointer<Widget>│
│ ...                                                           │
└────────────────────────────────────────────────────────────────┘
```

### GCHandle Pinning

```csharp
// C# side - pin object in memory
GCHandle handle = GCHandle.Alloc(backingStruct, GCHandleType.Pinned);
IntPtr pointer = handle.AddrOfPinnedObject();

// Send pointer to Dart
SendToFlutter(new { address = (long)pointer });
```

```dart
// Dart side - read from pointer
final pointer = Pointer<ContainerStruct>.fromAddress(address);
final struct = pointer.ref;
final color = struct.color;  // Direct memory read
```

### String Marshalling

Strings require special handling:

```csharp
// C# side
public IntPtr text_ptr;  // Pointer to null-terminated UTF8

public string? Text
{
    get => text_ptr == IntPtr.Zero
           ? null
           : Marshal.PtrToStringUTF8(text_ptr);
    set => text_ptr = value == null
           ? IntPtr.Zero
           : Marshal.StringToCoTaskMemUTF8(value);
}
```

```dart
// Dart side
external Pointer<Utf8> text;

String? get textValue => text == nullptr
    ? null
    : text.toDartString();
```

### Widget Reference Marshalling

Child widgets are passed by pointer:

```csharp
// C# struct
public IntPtr child;  // Pointer to child's backing struct

public Widget? Child
{
    set {
        if (value != null) {
            value.PrepareForSending();
            child = (IntPtr)value;
        }
    }
}
```

```dart
// Dart parsing
final childPtr = struct.child;
if (childPtr != nullptr) {
    final childWidget = buildFromPointer(childPtr);
}
```

## Threading Model

### C# Thread

- Main UI thread creates and modifies widgets
- Property setters update backing struct immediately
- `SendWidget()` serializes message and posts to channel

### Platform Channel

- Asynchronous message passing
- JSON serialization for messages (not for struct data)
- Thread-safe by design

### Dart/Flutter Thread

- Flutter UI thread receives messages
- Pointer reads from shared memory
- Widget building on UI thread
- `setState()` triggers Flutter rebuild

### Thread Safety Considerations

1. **Struct Mutations**: Only mutate from C# side before sending
2. **Memory Lifetime**: Structs must remain pinned during Dart access
3. **Event Callbacks**: Posted back to C# main thread

## Plugin Architecture

### Extending with Custom Widgets

```csharp
// Define custom widget
[FlutterWidget("my_package", "MyCustomWidget")]
public class MyCustomWidget : Widget
{
    private MyCustomWidgetStruct _struct;

    public string Title
    {
        get => _struct.title;
        set => _struct.title = value;
    }
}

// Register parser
[DartParser("MyCustomWidget")]
public class MyCustomWidgetParser
{
    public Widget parse(Pointer<MyCustomWidgetStruct> ptr) {
        return MyCustomWidget(
            title: ptr.ref.title.toDartString(),
        );
    }
}
```

### Package Integration Flow

```
┌─────────────────┐
│ pub.dev package │
│ or local source │
└───────┬─────────┘
        │
        ▼
┌─────────────────┐
│ FlutterSharp    │
│ CodeGen analyze │
└───────┬─────────┘
        │
        ▼
┌─────────────────┐
│ Generate C#     │
│ widgets/structs │
└───────┬─────────┘
        │
        ▼
┌─────────────────┐
│ Generate Dart   │
│ parsers/structs │
└───────┬─────────┘
        │
        ▼
┌─────────────────┐
│ Register in     │
│ parser registry │
└─────────────────┘
```

## Error Handling Architecture

### Widget Creation Errors

```csharp
try {
    var widget = new Container { child = invalidWidget };
} catch (FlutterSharpException ex) {
    // Validation error during construction
}
```

### Communication Errors

```csharp
FlutterManager.OnError += (sender, e) => {
    Console.WriteLine($"Flutter error: {e.Message}");
};
```

### Dart-Side Error Handling

```dart
try {
    final widget = buildFromPointer(ptr);
} catch (e) {
    // Return error placeholder widget
    return ErrorWidget(e);
}
```

## Configuration

### C# Configuration

```csharp
FlutterSharp.Configure(options => {
    options.EnableDebugLogging = true;
    options.WidgetPoolSize = 1000;
    options.EventBufferSize = 100;
});
```

### Dart Configuration

```dart
FlutterSharpConfig.configure(
    enableDebugLogging: true,
    validatePointers: true,
);
```

## Performance Optimizations

1. **Object Pooling**: Reuse widget objects when possible
2. **Batch Updates**: Combine multiple widget updates
3. **Lazy Loading**: Only prepare structs when needed
4. **Incremental Updates**: Send only changed properties
5. **Memory Caching**: Cache frequently used widgets

## See Also

- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) - Detailed FFI protocol
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Type mapping specification
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Code generation details
