# FlutterSharp Callbacks and Events Specification

## Overview

FlutterSharp supports bidirectional communication between C# and Dart for handling user interactions and events. This document specifies how callbacks are registered, invoked, and how events flow between the two runtimes.

## Callback Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Callback Flow                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   C# Side                           Dart Side                   │
│   ────────                          ─────────                   │
│                                                                 │
│   1. Developer sets callback:                                   │
│      button.OnPressed = () => {                                 │
│          Console.WriteLine("Clicked!");                         │
│      };                                                         │
│                                                                 │
│   2. CallbackRegistry.Register()                                │
│      - Generates unique actionId                                │
│      - Stores delegate reference                                │
│      - Returns "action_12345"                                   │
│                                                                 │
│   3. Action ID stored in struct:                                │
│      _struct.onPressed_ptr = "action_12345"                     │
│                                                                 │
│   4. Widget sent to Dart                                        │
│      ─────────────────────────────▶ 5. Parser reads actionId    │
│                                                                 │
│                                     6. Creates Dart callback:   │
│                                        onPressed: () {          │
│                                          invokeAction(id);      │
│                                        }                        │
│                                                                 │
│   [User taps button]                                            │
│                                                                 │
│                                     7. onPressed fires          │
│                                        invokeAction("action_..") │
│                                        ↓                        │
│                                     8. Send Event message       │
│      ◀─────────────────────────────────                         │
│                                                                 │
│   9. FlutterManager receives event                              │
│      ↓                                                          │
│   10. CallbackRegistry.Invoke("action_12345")                   │
│       ↓                                                         │
│   11. Console.WriteLine("Clicked!")                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Callback Registry (C#)

### Implementation

```csharp
public static class CallbackRegistry
{
    // Thread-safe storage
    private static readonly ConcurrentDictionary<string, Delegate> _callbacks = new();
    private static long _nextId = 0;

    /// <summary>
    /// Register a callback and get its unique action ID.
    /// </summary>
    public static string Register(Delegate callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var id = $"action_{Interlocked.Increment(ref _nextId)}";
        _callbacks[id] = callback;
        return id;
    }

    /// <summary>
    /// Register a typed callback.
    /// </summary>
    public static string Register<T>(Action<T> callback) =>
        Register((Delegate)callback);

    /// <summary>
    /// Get a callback by its action ID.
    /// </summary>
    public static Delegate? Get(string actionId)
    {
        _callbacks.TryGetValue(actionId, out var callback);
        return callback;
    }

    /// <summary>
    /// Get a typed callback.
    /// </summary>
    public static T? Get<T>(string actionId) where T : Delegate
    {
        return Get(actionId) as T;
    }

    /// <summary>
    /// Invoke a callback by its action ID.
    /// </summary>
    public static void Invoke(string actionId, params object?[] args)
    {
        var callback = Get(actionId);
        if (callback == null)
        {
            Console.WriteLine($"Warning: No callback found for {actionId}");
            return;
        }

        try
        {
            callback.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking callback {actionId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Invoke a callback with typed arguments.
    /// </summary>
    public static void Invoke<T>(string actionId, T arg)
    {
        var callback = Get<Action<T>>(actionId);
        callback?.Invoke(arg);
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public static bool Unregister(string actionId) =>
        _callbacks.TryRemove(actionId, out _);

    /// <summary>
    /// Unregister all callbacks for a widget.
    /// </summary>
    public static void UnregisterAll(string widgetId)
    {
        var prefix = $"{widgetId}_";
        var toRemove = _callbacks.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in toRemove)
        {
            _callbacks.TryRemove(key, out _);
        }
    }
}
```

### Usage in Widget

```csharp
public class Button : Widget
{
    private Action? _onPressed;
    private string? _onPressedActionId;

    public Action? OnPressed
    {
        get => _onPressed;
        set {
            // Unregister old callback
            if (_onPressedActionId != null)
            {
                CallbackRegistry.Unregister(_onPressedActionId);
                _onPressedActionId = null;
            }

            _onPressed = value;

            // Register new callback
            if (value != null)
            {
                _onPressedActionId = CallbackRegistry.Register(value);
                _struct.onPressed_ptr = Marshal.StringToCoTaskMemUTF8(_onPressedActionId);
            }
            else
            {
                _struct.onPressed_ptr = IntPtr.Zero;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_onPressedActionId != null)
        {
            CallbackRegistry.Unregister(_onPressedActionId);
        }
        base.Dispose(disposing);
    }
}
```

## Dart Side Callback Handling

### Parser Callback Creation

```dart
class ButtonParser {
  Widget parse(Pointer<ButtonStruct> ptr) {
    final struct = ptr.ref;

    // Read action ID
    final onPressedId = struct.onPressed.address != 0
        ? struct.onPressed.toDartString()
        : null;

    return ElevatedButton(
      onPressed: onPressedId != null
          ? () => MauiRenderer.invokeAction(onPressedId)
          : null,
      child: _parseChild(struct.child),
    );
  }
}
```

### MauiRenderer Event Dispatch

```dart
class MauiRenderer {
  static const _channel = MethodChannel('com.Microsoft.FlutterSharp/Messages');

  /// Invoke an action registered in C#
  static Future<void> invokeAction(String actionId, [Map<String, dynamic>? data]) async {
    final message = jsonEncode({
      'MessageType': 'Event',
      'ActionId': actionId,
      'Data': data ?? {},
    });

    await _channel.invokeMethod('event', message);
  }

  /// Invoke action with typed parameter
  static Future<void> invokeActionWithValue<T>(String actionId, T value) async {
    await invokeAction(actionId, {'value': value});
  }
}
```

## Event Types

### VoidCallback (No Parameters)

```dart
// Dart side
void Function()? onPressed;
```

```csharp
// C# side
public Action? OnPressed { get; set; }
```

**Event Message:**
```json
{
  "MessageType": "Event",
  "ActionId": "action_12345",
  "Data": {}
}
```

### ValueChanged<T> (Single Parameter)

```dart
// Dart side
void Function(bool)? onChanged;
```

```csharp
// C# side
public Action<bool>? OnChanged { get; set; }
```

**Event Message:**
```json
{
  "MessageType": "Event",
  "ActionId": "action_12346",
  "Data": {
    "value": true
  }
}
```

### Complex Event Data

```dart
// Dart side - TapDownDetails
void Function(TapDownDetails)? onTapDown;
```

```csharp
// C# side
public Action<TapDownDetails>? OnTapDown { get; set; }

public struct TapDownDetails
{
    public Offset GlobalPosition;
    public Offset LocalPosition;
    public PointerDeviceKind Kind;
}
```

**Event Message:**
```json
{
  "MessageType": "Event",
  "ActionId": "action_12347",
  "Data": {
    "globalPosition": { "dx": 100.0, "dy": 200.0 },
    "localPosition": { "dx": 50.0, "dy": 50.0 },
    "kind": 0
  }
}
```

### C# Event Handling

```csharp
public static class FlutterManager
{
    public static void HandleEvent(string json)
    {
        var message = JsonSerializer.Deserialize<EventMessage>(json);

        switch (message.EventType)
        {
            case "VoidCallback":
                CallbackRegistry.Invoke(message.ActionId);
                break;

            case "ValueChanged<bool>":
                var boolValue = message.Data.GetProperty("value").GetBoolean();
                CallbackRegistry.Invoke<bool>(message.ActionId, boolValue);
                break;

            case "ValueChanged<int>":
                var intValue = message.Data.GetProperty("value").GetInt32();
                CallbackRegistry.Invoke<int>(message.ActionId, intValue);
                break;

            case "TapDownDetails":
                var details = JsonSerializer.Deserialize<TapDownDetails>(message.Data);
                CallbackRegistry.Invoke<TapDownDetails>(message.ActionId, details);
                break;

            default:
                // Dynamic invocation with JSON data
                CallbackRegistry.InvokeWithJson(message.ActionId, message.Data);
                break;
        }
    }
}
```

## Callback Type Mappings

| Dart Callback | C# Type | Event Data |
|---------------|---------|------------|
| `VoidCallback` | `Action` | None |
| `GestureTapCallback` | `Action` | None |
| `GestureTapDownCallback` | `Action<TapDownDetails>` | Position data |
| `GestureTapUpCallback` | `Action<TapUpDetails>` | Position data |
| `GestureDragStartCallback` | `Action<DragStartDetails>` | Position, velocity |
| `GestureDragUpdateCallback` | `Action<DragUpdateDetails>` | Delta, position |
| `GestureDragEndCallback` | `Action<DragEndDetails>` | Velocity |
| `GestureScaleStartCallback` | `Action<ScaleStartDetails>` | Focal point |
| `GestureScaleUpdateCallback` | `Action<ScaleUpdateDetails>` | Scale, rotation |
| `ValueChanged<bool>` | `Action<bool>` | Bool value |
| `ValueChanged<int>` | `Action<int>` | Int value |
| `ValueChanged<double>` | `Action<double>` | Double value |
| `ValueChanged<String>` | `Action<string>` | String value |
| `FormFieldSetter<T>` | `Action<T>` | Typed value |

## Builder Callbacks

### Synchronous Builder Pattern

Builder callbacks require synchronous response from C#:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Builder Callback Flow                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Dart Side                         C# Side                     │
│   ─────────                         ────────                    │
│                                                                 │
│   1. ListView.builder needs                                     │
│      to build item at index 5                                   │
│                                                                 │
│   2. Invoke builder callback                                    │
│      ─────────────────────────────▶ 3. Receive BuildItem event  │
│      (blocks waiting for response)                              │
│                                     4. Find builder in registry │
│                                        ↓                        │
│                                     5. Invoke builder(index: 5) │
│                                        ↓                        │
│                                     6. Developer code returns   │
│                                        new Text("Item 5")       │
│                                        ↓                        │
│                                     7. PrepareForSending()      │
│                                        ↓                        │
│      ◀───────────────────────────── 8. Return widget pointer    │
│                                                                 │
│   9. Receive pointer                                            │
│      ↓                                                          │
│   10. Parse widget from pointer                                 │
│       ↓                                                         │
│   11. Return widget to ListView                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Builder Implementation

```csharp
// C# side
public class ListView : Widget
{
    private Func<int, Widget>? _itemBuilder;
    private string? _itemBuilderActionId;

    public Func<int, Widget>? ItemBuilder
    {
        get => _itemBuilder;
        set {
            _itemBuilder = value;
            if (value != null)
            {
                _itemBuilderActionId = CallbackRegistry.Register(
                    new Func<int, IntPtr>(index =>
                    {
                        var widget = value(index);
                        widget.PrepareForSending();
                        return (IntPtr)widget;
                    })
                );
                _struct.itemBuilder_ptr = Marshal.StringToCoTaskMemUTF8(_itemBuilderActionId);
            }
        }
    }
}
```

```dart
// Dart side
class ListViewParser {
  Widget parse(Pointer<ListViewStruct> ptr) {
    final struct = ptr.ref;
    final builderId = struct.itemBuilder.address != 0
        ? struct.itemBuilder.toDartString()
        : null;

    return ListView.builder(
      itemCount: struct.itemCount,
      itemBuilder: builderId != null
          ? (context, index) => _invokeBuilder(builderId, index)
          : null,
    );
  }

  Widget _invokeBuilder(String builderId, int index) {
    // Synchronous call to C#
    final address = MauiRenderer.invokeBuilderSync(builderId, {'index': index});
    return DynamicWidgetBuilder.buildFromPointer(
        Pointer<WidgetStruct>.fromAddress(address));
  }
}
```

### Synchronous Channel

```dart
class MauiRenderer {
  // For builder callbacks that need synchronous response
  static int invokeBuilderSync(String actionId, Map<String, dynamic> data) {
    // Uses platform-specific synchronous invocation
    return _syncChannel.invokeMethod('invokeBuilder', {
      'actionId': actionId,
      'data': data,
    });
  }
}
```

## Animation Callbacks

### Animation Listener

```csharp
public class AnimatedContainer : Widget
{
    public Action? OnEnd { get; set; }

    // Duration of animation
    public TimeSpan Duration { get; set; }

    // Called when animation value changes
    public Action<double>? OnAnimationValue { get; set; }
}
```

### Animation Event Flow

```dart
AnimatedContainer(
  duration: parseDuration(struct.duration),
  onEnd: () => MauiRenderer.invokeAction(struct.onEnd.toDartString()),
  // Animation value changes are sampled, not every frame
  // to avoid overwhelming the channel
);
```

## Focus and Text Input Events

### Focus Events

```csharp
public class TextField : Widget
{
    public Action? OnFocusChange { get; set; }
    public Action<bool>? OnFocusChanged { get; set; }
}
```

### Text Change Events

```csharp
public class TextField : Widget
{
    public Action<string>? OnChanged { get; set; }
    public Action<string>? OnSubmitted { get; set; }

    // Text controller state
    public string Text
    {
        get => _text;
        set {
            _text = value;
            // Send text update to Dart if widget is active
            if (IsActive)
            {
                FlutterManager.SendTextUpdate(Id, value);
            }
        }
    }
}
```

```dart
// Dart parser
TextField(
  onChanged: (value) => MauiRenderer.invokeAction(
      struct.onChanged.toDartString(),
      {'value': value}
  ),
  onSubmitted: (value) => MauiRenderer.invokeAction(
      struct.onSubmitted.toDartString(),
      {'value': value}
  ),
);
```

## Scroll Events

### Scroll Notification

```csharp
public class NotificationListener : Widget
{
    public Func<ScrollNotification, bool>? OnNotification { get; set; }
}

public class ScrollNotification
{
    public double Pixels { get; set; }
    public double MinScrollExtent { get; set; }
    public double MaxScrollExtent { get; set; }
    public double ViewportDimension { get; set; }
}
```

```dart
NotificationListener<ScrollNotification>(
  onNotification: (notification) {
    final data = {
      'pixels': notification.metrics.pixels,
      'minScrollExtent': notification.metrics.minScrollExtent,
      'maxScrollExtent': notification.metrics.maxScrollExtent,
      'viewportDimension': notification.metrics.viewportDimension,
    };
    return MauiRenderer.invokeActionWithResult<bool>(
        struct.onNotification.toDartString(),
        data
    );
  },
);
```

## Event Debouncing and Throttling

### High-Frequency Events

For events that fire rapidly (scroll, drag), apply debouncing:

```dart
class DebouncedEventSender {
  final String actionId;
  final Duration debounceTime;
  Timer? _timer;
  Map<String, dynamic>? _pendingData;

  DebouncedEventSender(this.actionId, {this.debounceTime = const Duration(milliseconds: 16)});

  void send(Map<String, dynamic> data) {
    _pendingData = data;
    _timer?.cancel();
    _timer = Timer(debounceTime, () {
      if (_pendingData != null) {
        MauiRenderer.invokeAction(actionId, _pendingData!);
        _pendingData = null;
      }
    });
  }

  void flush() {
    _timer?.cancel();
    if (_pendingData != null) {
      MauiRenderer.invokeAction(actionId, _pendingData!);
      _pendingData = null;
    }
  }
}
```

## Error Handling

### Callback Exceptions

```csharp
public static class CallbackRegistry
{
    public static event EventHandler<CallbackErrorEventArgs>? OnCallbackError;

    public static void Invoke(string actionId, params object?[] args)
    {
        var callback = Get(actionId);
        if (callback == null) return;

        try
        {
            callback.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            var errorArgs = new CallbackErrorEventArgs(actionId, ex);
            OnCallbackError?.Invoke(null, errorArgs);

            if (!errorArgs.Handled)
            {
                throw;
            }
        }
    }
}
```

### Dart Error Handling

```dart
class MauiRenderer {
  static Future<void> invokeAction(String actionId, [Map<String, dynamic>? data]) async {
    try {
      await _channel.invokeMethod('event', jsonEncode({
        'MessageType': 'Event',
        'ActionId': actionId,
        'Data': data ?? {},
      }));
    } catch (e) {
      print('Error invoking action $actionId: $e');
      // Optionally show error UI
    }
  }
}
```

## Memory Management

### Callback Lifecycle

```csharp
public class Widget : IDisposable
{
    private readonly List<string> _registeredCallbacks = new();

    protected string RegisterCallback(Delegate callback)
    {
        var actionId = CallbackRegistry.Register(callback);
        _registeredCallbacks.Add(actionId);
        return actionId;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var actionId in _registeredCallbacks)
            {
                CallbackRegistry.Unregister(actionId);
            }
            _registeredCallbacks.Clear();
        }
        base.Dispose(disposing);
    }
}
```

### Weak References for Long-Lived Callbacks

```csharp
public static class WeakCallbackRegistry
{
    private static readonly ConcurrentDictionary<string, WeakReference<Delegate>> _callbacks = new();

    public static string Register(Delegate callback)
    {
        var id = $"weak_action_{Guid.NewGuid():N}";
        _callbacks[id] = new WeakReference<Delegate>(callback);
        return id;
    }

    public static void Invoke(string actionId, params object?[] args)
    {
        if (_callbacks.TryGetValue(actionId, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var callback))
            {
                callback.DynamicInvoke(args);
            }
            else
            {
                // Callback was garbage collected
                _callbacks.TryRemove(actionId, out _);
            }
        }
    }
}
```

## Performance Considerations

1. **Batch Events**: Group rapid events into batches
2. **Debounce High-Frequency**: Scroll, drag events
3. **Avoid Large Payloads**: Minimize event data size
4. **Use Weak References**: For long-lived callbacks
5. **Unregister Promptly**: When widgets are disposed

## See Also

- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) - Message protocol
- [WIDGET-BINDING.md](./WIDGET-BINDING.md) - Widget implementation
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Type mappings
