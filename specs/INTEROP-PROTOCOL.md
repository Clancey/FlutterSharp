# FlutterSharp Interop Protocol Specification

## Overview

FlutterSharp uses a **memory-shared FFI protocol** for communication between C# and Dart. Unlike traditional serialization approaches, data is shared directly through pinned memory pointers, enabling zero-copy data access.

## Protocol Layers

```
┌─────────────────────────────────────────────────────┐
│ Layer 4: Application Protocol                       │
│   Widget updates, events, commands                  │
├─────────────────────────────────────────────────────┤
│ Layer 3: Message Protocol                           │
│   JSON message envelope with type and data          │
├─────────────────────────────────────────────────────┤
│ Layer 2: Transport Protocol                         │
│   Flutter MethodChannel                             │
├─────────────────────────────────────────────────────┤
│ Layer 1: Data Protocol                              │
│   Memory-shared FFI structs                         │
└─────────────────────────────────────────────────────┘
```

## Layer 1: Data Protocol (FFI Structs)

### Struct Layout Rules

All structs follow these rules for cross-platform compatibility:

1. **Sequential Layout**: Fields are laid out in declaration order
2. **Natural Alignment**: Each field is aligned to its natural boundary
3. **Explicit Padding**: No implicit padding between fields
4. **Little Endian**: All multi-byte values use little-endian encoding

### Base Struct Definition

Every widget struct inherits from this base layout:

```csharp
// C# definition
[StructLayout(LayoutKind.Sequential)]
public struct FlutterObjectStruct
{
    public IntPtr handle;           // Offset 0:  GCHandle to widget (8 bytes)
    public IntPtr managedHandle;    // Offset 8:  Reserved (8 bytes)
    public int widgetType;          // Offset 16: Type discriminator (4 bytes)
    // Padding                      // Offset 20: Alignment padding (4 bytes)
}

[StructLayout(LayoutKind.Sequential)]
public struct WidgetStruct : FlutterObjectStruct
{
    // Inherits FlutterObjectStruct fields
    public IntPtr id;               // Offset 24: Widget ID UTF8 pointer (8 bytes)
}
```

```dart
// Dart FFI definition
final class FlutterObjectStruct extends Struct {
  external Pointer<Void> handle;           // 8 bytes
  external Pointer<Void> managedHandle;    // 8 bytes
  @Int32() external int widgetType;        // 4 bytes
  // 4 bytes padding implicit
}

final class WidgetStruct extends Struct implements IWidgetStruct {
  external Pointer<Void> handle;
  external Pointer<Void> managedHandle;
  @Int32() external int widgetType;
  external Pointer<Utf8> id;               // 8 bytes
}
```

### Type Field Values

The `widgetType` field is a discriminator for widget type identification:

| Value | Widget Type |
|-------|-------------|
| 0 | Unknown |
| 1 | Container |
| 2 | Text |
| 3 | Column |
| 4 | Row |
| 5 | Stack |
| ... | ... |

Values are assigned during code generation and stored in a registry.

### Primitive Type Mappings

| Dart Type | C# Type | FFI Size | Dart FFI |
|-----------|---------|----------|----------|
| `int` | `int` | 4 bytes | `@Int32()` |
| `double` | `double` | 8 bytes | `@Double()` |
| `bool` | `byte` | 1 byte | `@Int8()` |
| `String` | `IntPtr` | 8 bytes | `Pointer<Utf8>` |
| `Widget` | `IntPtr` | 8 bytes | `Pointer<WidgetStruct>` |

### Nullable Type Pattern

Nullable types use a "has" flag pattern:

```csharp
// C# struct for nullable double
public byte hasOpacity;    // 0 = null, 1 = has value
public double opacity;     // Only valid if hasOpacity == 1
```

```dart
// Dart FFI
@Int8() external int hasOpacity;
@Double() external double opacity;

double? get opacityValue =>
    hasOpacity == 1 ? opacity : null;
```

### String Marshalling

Strings are passed as pointers to null-terminated UTF-8 encoded data:

```csharp
// C# marshalling
public IntPtr text_ptr;

public string? Text
{
    get => text_ptr == IntPtr.Zero
           ? null
           : Marshal.PtrToStringUTF8(text_ptr);
    set {
        if (text_ptr != IntPtr.Zero) {
            Marshal.FreeCoTaskMem(text_ptr);
        }
        text_ptr = value == null
           ? IntPtr.Zero
           : Marshal.StringToCoTaskMemUTF8(value);
    }
}
```

```dart
// Dart reading
external Pointer<Utf8> text;

String? get textValue =>
    text.address == 0 ? null : text.toDartString();
```

### List/Children Marshalling

Lists of widgets use a dedicated children struct:

```csharp
// C# children struct
[StructLayout(LayoutKind.Sequential)]
public struct ChildrenStruct
{
    public IntPtr items;    // Pointer to array of IntPtr
    public int count;       // Number of children
}

// Usage
public void SetChildren(List<Widget> children)
{
    var ptrs = new IntPtr[children.Count];
    for (int i = 0; i < children.Count; i++) {
        children[i].PrepareForSending();
        ptrs[i] = (IntPtr)children[i];
    }
    // Pin and set pointer
}
```

```dart
// Dart FFI
final class ChildrenStruct extends Struct {
  external Pointer<Pointer<WidgetStruct>> items;
  @Int32() external int count;
}

List<Widget> parseChildren(ChildrenStruct children) {
  final result = <Widget>[];
  for (var i = 0; i < children.count; i++) {
    final ptr = children.items.elementAt(i).value;
    result.add(buildFromPointer(ptr));
  }
  return result;
}
```

### Nested Struct Types

Complex types like Color, EdgeInsets are passed inline:

```csharp
// C# Color struct
[StructLayout(LayoutKind.Sequential)]
public struct ColorStruct
{
    public uint value;  // ARGB packed value
}

// In widget struct
public ColorStruct color;  // Inline, not a pointer
```

```dart
// Dart FFI
final class ColorStruct extends Struct {
  @Uint32() external int value;
}

// Reading
Color parseColor(ColorStruct s) => Color(s.value);
```

### Callback Marshalling

Callbacks are represented as string action identifiers:

```csharp
// C# struct
public IntPtr onPressed_ptr;  // Pointer to UTF8 action ID

public Action? OnPressed
{
    set {
        if (value != null) {
            var actionId = CallbackRegistry.Register(value);
            onPressed_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
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
  return () => MauiRenderer.sendEvent(actionId);
}
```

## Layer 2: Transport Protocol (MethodChannel)

### Channel Definition

```dart
// Dart side
const channel = MethodChannel('com.Microsoft.FlutterSharp/Messages');
```

```csharp
// C# side
var channel = new MethodChannel("com.Microsoft.FlutterSharp/Messages");
```

### Message Invocation

All messages are sent via `invokeMethod` with JSON payloads:

```csharp
// C# sending
await channel.InvokeMethodAsync("ready", jsonPayload);
```

```dart
// Dart receiving
channel.setMethodCallHandler((call) async {
  final data = jsonDecode(call.arguments as String);
  handleMessage(call.method, data);
});
```

## Layer 3: Message Protocol

### Message Envelope

All messages follow this JSON structure:

```json
{
  "MessageType": "<type>",
  "Timestamp": "<ISO 8601>",
  "Data": { ... }
}
```

### Message Types

#### UpdateComponent (C# → Dart)

Sent when a widget needs to be rendered or updated.

```json
{
  "MessageType": "UpdateComponent",
  "ComponentId": "widget_0",
  "Address": 140234567890,
  "WidgetType": 1
}
```

| Field | Type | Description |
|-------|------|-------------|
| ComponentId | string | Unique widget identifier |
| Address | int64 | Memory pointer to struct |
| WidgetType | int | Type discriminator |

#### Event (Dart → C#)

Sent when user interacts with a widget.

```json
{
  "MessageType": "Event",
  "WidgetId": "widget_0",
  "EventType": "onPressed",
  "ActionId": "action_1234",
  "Data": {}
}
```

| Field | Type | Description |
|-------|------|-------------|
| WidgetId | string | Widget that triggered event |
| EventType | string | Type of event |
| ActionId | string | Callback registry ID |
| Data | object | Event-specific data |

#### Ready (Dart → C#)

Sent when Flutter engine is initialized.

```json
{
  "MessageType": "Ready"
}
```

#### Dispose (C# → Dart)

Sent when a widget is disposed.

```json
{
  "MessageType": "Dispose",
  "WidgetId": "widget_0"
}
```

#### BatchUpdate (C# → Dart)

Sent for multiple widget updates.

```json
{
  "MessageType": "BatchUpdate",
  "Updates": [
    { "ComponentId": "widget_0", "Address": 140234567890 },
    { "ComponentId": "widget_1", "Address": 140234568000 }
  ]
}
```

## Layer 4: Application Protocol

### Widget Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                      Widget Lifecycle                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   C# Side                           Dart Side                   │
│   ────────                          ─────────                   │
│                                                                 │
│   1. new Widget()                                               │
│      ↓                                                          │
│   2. Set properties                                             │
│      ↓                                                          │
│   3. PrepareForSending()                                        │
│      - Create struct                                            │
│      - Pin memory                                               │
│      - Get pointer                                              │
│      ↓                                                          │
│   4. SendWidget()                                               │
│      ─────────────────────────────▶ 5. Receive UpdateComponent  │
│                                        ↓                        │
│                                     6. Cast pointer to struct   │
│                                        ↓                        │
│                                     7. Parser reads fields      │
│                                        ↓                        │
│                                     8. Build Flutter widget     │
│                                        ↓                        │
│                                     9. setState() triggers      │
│                                        rebuild                  │
│                                                                 │
│   [Widget in use - memory pinned]                               │
│                                                                 │
│   10. User interaction                                          │
│       ◀───────────────────────────── onTap callback fires       │
│                                        ↓                        │
│       ◀─────────────────────────── 11. Send Event message       │
│      ↓                                                          │
│   12. CallbackRegistry                                          │
│       invokes Action                                            │
│                                                                 │
│   [Widget disposed]                                             │
│                                                                 │
│   13. Dispose()                                                 │
│       - Send Dispose msg ───────────▶ 14. Remove from tree      │
│       - Unpin memory                                            │
│       - Free strings                                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Event Handling Protocol

1. **Callback Registration** (C# side)
   ```csharp
   button.OnPressed = () => Console.WriteLine("Pressed!");
   // Generates: actionId = "action_1234"
   // Stores: CallbackRegistry["action_1234"] = delegate
   ```

2. **Event Trigger** (Dart side)
   ```dart
   onPressed: () {
     MauiRenderer.sendEvent(
       widgetId: "button_0",
       eventType: "onPressed",
       actionId: "action_1234"
     );
   }
   ```

3. **Event Routing** (C# side)
   ```csharp
   void OnEvent(string widgetId, string eventType, string actionId) {
     var action = CallbackRegistry.Get(actionId);
     action?.Invoke();
   }
   ```

### State Synchronization Protocol

Widget updates flow **one-way** from C# to Dart:

```
   C# Widget State            Dart Widget
        ↓                         ↓
   Modify property          (Immutable)
        ↓
   Call UpdateWidget()
        ↓
   Send UpdateComponent
        ───────────────────▶
                                  ↓
                            Rebuild widget
                            from struct
```

To update UI from Dart interaction:
1. Dart sends Event to C#
2. C# modifies widget state
3. C# sends UpdateComponent
4. Dart rebuilds widget

## Memory Management

### Pinned Memory Lifetime

```csharp
public class Widget : IDisposable
{
    private GCHandle _handle;
    private bool _isPinned;

    public void PrepareForSending()
    {
        if (!_isPinned) {
            _handle = GCHandle.Alloc(BackingStruct, GCHandleType.Pinned);
            _isPinned = true;
        }
    }

    public void Dispose()
    {
        if (_isPinned) {
            _handle.Free();
            _isPinned = false;
        }
        FreeStringPointers();
    }
}
```

### String Memory Management

```csharp
// Allocation
text_ptr = Marshal.StringToCoTaskMemUTF8(value);

// Deallocation (in Dispose)
if (text_ptr != IntPtr.Zero) {
    Marshal.FreeCoTaskMem(text_ptr);
    text_ptr = IntPtr.Zero;
}
```

### Widget Reference Counting

```csharp
public static class FlutterManager
{
    private static WeakDictionary<string, Widget> _aliveWidgets;

    public static void TrackWidget(Widget widget)
    {
        _aliveWidgets[widget.Id] = widget;
    }

    public static void UntrackWidget(Widget widget)
    {
        _aliveWidgets.Remove(widget.Id);
    }
}
```

## Error Handling

### Invalid Pointer Access

```dart
Widget buildFromPointer(Pointer<WidgetStruct> ptr) {
  if (ptr.address == 0) {
    return const SizedBox.shrink();  // Null pointer
  }

  try {
    final struct = ptr.ref;
    return _parseWidget(struct);
  } catch (e) {
    return ErrorWidget(e);  // Parsing error
  }
}
```

### Message Validation

```dart
void handleMessage(String method, Map<String, dynamic> data) {
  if (!data.containsKey('MessageType')) {
    print('Invalid message: missing MessageType');
    return;
  }

  switch (data['MessageType']) {
    case 'UpdateComponent':
      if (!data.containsKey('Address')) {
        print('Invalid UpdateComponent: missing Address');
        return;
      }
      // Process...
  }
}
```

## Protocol Versioning

### Version Negotiation

```json
// Initial handshake
{
  "MessageType": "Handshake",
  "ProtocolVersion": "1.0",
  "Features": ["batch_updates", "incremental_sync"]
}
```

### Backward Compatibility

- New optional fields can be added
- Required fields cannot be removed
- Type discriminator ranges are reserved

## Security Considerations

### Pointer Validation

- Validate pointer addresses are in expected range
- Check widget type discriminator before casting
- Limit string lengths to prevent buffer overflow

### Memory Isolation

- C# and Dart run in same process
- Memory shared through explicit pinning only
- No cross-process memory access

## Performance Characteristics

| Operation | Approximate Cost |
|-----------|-----------------|
| Struct field read | ~1ns (direct memory) |
| String read | ~100ns (UTF-8 decode) |
| Widget tree build | ~1ms per 100 widgets |
| Message send | ~100μs (JSON + channel) |
| Full update cycle | ~5ms typical |

## See Also

- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Type mapping details
- [CALLBACKS-EVENTS.md](./CALLBACKS-EVENTS.md) - Callback system
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Overall architecture
