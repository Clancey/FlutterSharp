# FlutterSharp Specification

## Project Overview

**FlutterSharp** is a complete interoperability layer enabling C#/.NET MAUI applications to use Flutter's widget system natively. The project provides:

1. **Full Widget Bindings** - C# classes for all 400+ Flutter widgets
2. **Bidirectional Communication** - C# → Dart and Dart → C# event flow
3. **Automatic Code Generation** - Generate bindings from any Dart/Flutter package
4. **Zero-Copy Interop** - Direct memory sharing via FFI for maximum performance

## Goals

### Primary Goals

1. **Complete Flutter Widget Coverage**
   - Generate C# bindings for all public Flutter widgets
   - Support widget properties, constructors, and documentation
   - Handle widget composition and nesting

2. **Type-Safe Interop**
   - Full type mapping between Dart and C# type systems
   - Compile-time type checking on both sides
   - Proper nullable type handling

3. **High Performance**
   - Zero-copy memory sharing via pinned structs
   - Minimal serialization overhead
   - Efficient widget tree updates

4. **Developer Experience**
   - Familiar C# API patterns
   - Full IntelliSense and documentation
   - Simple integration with .NET MAUI

### Secondary Goals

1. **Extensibility** - Support custom Flutter packages beyond the SDK
2. **Testability** - Enable unit testing of C# widget code
3. **Hot Reload Compatibility** - Work with Flutter's hot reload
4. **Cross-Platform** - Support all Flutter platforms (iOS, Android, Windows, macOS, Linux, Web)

## Core Concepts

### 1. Widget Definition

A Flutter widget in C# consists of:

```csharp
public class Container : Widget
{
    // Backing struct for FFI
    private ContainerStruct _struct;

    // Properties map to struct fields
    public Color? Color
    {
        get => _struct.color;
        set => _struct.color = value;
    }

    public Widget? Child
    {
        get => GetChild();
        set => SetChild(value);
    }
}
```

### 2. Memory-Shared Structs

Both C# and Dart share the same memory layout:

```csharp
// C# struct
[StructLayout(LayoutKind.Sequential)]
public struct ContainerStruct
{
    public IntPtr handle;
    public int widgetType;
    public IntPtr id;
    public ColorStruct color;
    public IntPtr child;
}
```

```dart
// Dart FFI struct - same memory layout
final class ContainerStruct extends Struct {
  external Pointer<Void> handle;
  @Int32() external int widgetType;
  external Pointer<Utf8> id;
  external ColorStruct color;
  external Pointer<WidgetStruct> child;
}
```

### 3. Communication Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        .NET MAUI App                            │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │   var container = new Container {                         │  │
│  │       Color = Colors.Blue,                                │  │
│  │       Child = new Text("Hello")                           │  │
│  │   };                                                      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              ▼                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │   FlutterManager.SendWidget(container)                    │  │
│  │   - Prepares backing struct                               │  │
│  │   - Pins memory with GCHandle                             │  │
│  │   - Sends pointer via MethodChannel                       │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────│──────────────────────────────────┘
                               │
                    MethodChannel Message
                    { type: "UpdateComponent",
                      address: 0x7F8A4C2000 }
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Flutter Engine                              │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │   MauiRenderer receives message                           │  │
│  │   - Casts address to Pointer<ContainerStruct>             │  │
│  │   - Reads struct fields directly from memory              │  │
│  │   - Invokes ContainerParser.parse()                       │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              ▼                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │   ContainerParser builds Flutter widget:                  │  │
│  │   Container(                                              │  │
│  │     color: Color(struct.color.value),                     │  │
│  │     child: buildFromPointer(struct.child)                 │  │
│  │   )                                                       │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              ▼                                  │
│               Flutter renders native UI                         │
└─────────────────────────────────────────────────────────────────┘
```

## Target Use Cases

### 1. MAUI Apps with Flutter UI

```csharp
public class MyApp : Application
{
    public MyApp()
    {
        MainPage = new FlutterPage(
            new MaterialApp(
                home: new Scaffold(
                    appBar: new AppBar(title: new Text("My App")),
                    body: new Center(
                        child: new ElevatedButton(
                            onPressed: () => Console.WriteLine("Pressed!"),
                            child: new Text("Click Me")
                        )
                    )
                )
            )
        );
    }
}
```

### 2. Hybrid Native + Flutter Apps

```csharp
// Native MAUI page with embedded Flutter view
public class HybridPage : ContentPage
{
    public HybridPage()
    {
        Content = new Grid
        {
            Children = {
                // Native MAUI header
                new Label { Text = "Native Header" },
                // Flutter widget embedded
                new FlutterView(
                    new ListView.builder(
                        itemCount: 1000,
                        itemBuilder: (ctx, i) => new ListTile(
                            title: new Text($"Item {i}")
                        )
                    )
                )
            }
        };
    }
}
```

### 3. Code Generation for Packages

```bash
# Generate bindings for any pub.dev package
fluttersharp generate --source package:riverpod --output ./bindings

# Generate bindings for local Flutter package
fluttersharp generate --source ./my_widgets --output ./bindings
```

## Project Components

| Component | Description |
|-----------|-------------|
| `src/Flutter/` | C# runtime and widget bindings |
| `FlutterSharp.CodeGen/` | Code generation tool |
| `flutter_module/` | Dart/Flutter runtime and parsers |
| `Sample/` | Example .NET MAUI application |

## Versioning Strategy

- **FlutterSharp version** follows semver independently
- **Flutter SDK version** is specified per-project
- Code generation can target specific Flutter versions
- Bindings are regenerated when Flutter SDK updates

## Platform Support

| Platform | Status | Notes |
|----------|--------|-------|
| Android | Supported | Via Flutter Android embedding |
| iOS | Supported | Via Flutter iOS embedding |
| macOS | Supported | Via Flutter macOS embedding |
| Windows | Supported | Via Flutter Windows embedding |
| Linux | Planned | Via Flutter Linux embedding |
| Web | Experimental | Via Flutter Web (limited FFI) |

## Related Specifications

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System architecture
- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) - FFI memory sharing protocol
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Code generation system
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Dart to C# type mapping
- [WIDGET-BINDING.md](./WIDGET-BINDING.md) - Widget binding specification
- [CALLBACKS-EVENTS.md](./CALLBACKS-EVENTS.md) - Callback and event handling
- [ROADMAP.md](./ROADMAP.md) - Implementation phases
