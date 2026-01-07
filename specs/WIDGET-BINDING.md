# FlutterSharp Widget Binding Specification

## Overview

Widget bindings are C# classes that wrap Flutter widgets, providing a type-safe API for creating and configuring widgets from .NET code. Each Flutter widget has a corresponding C# binding that:

1. Exposes widget properties as C# properties
2. Manages backing struct memory
3. Handles child widget composition
4. Supports callback registration

## Widget Class Hierarchy

```
Widget (abstract base)
├── StatelessWidget (abstract)
│   ├── Container
│   ├── Text
│   ├── Icon
│   └── ... (200+ widgets)
├── StatefulWidget (abstract)
│   ├── TextField
│   ├── Checkbox
│   └── ... (100+ widgets)
├── RenderObjectWidget (abstract)
│   ├── Padding
│   ├── Align
│   └── ... (50+ widgets)
└── ProxyWidget (abstract)
    ├── InheritedWidget
    └── ParentDataWidget
```

## Base Widget Class

### Definition

```csharp
public abstract class Widget : IDisposable
{
    // Unique identifier
    public string Id { get; }

    // Widget type discriminator
    public abstract int WidgetType { get; }

    // Backing struct for FFI
    protected abstract BaseStruct BackingStruct { get; }

    // Memory pinning handle
    private GCHandle _handle;
    private bool _isPinned;
    private bool _isDisposed;

    // Parent tracking
    internal Widget? Parent { get; set; }

    // Children
    protected List<Widget> _children = new();

    protected Widget()
    {
        Id = Guid.NewGuid().ToString("N");
        FlutterManager.TrackWidget(this);
    }

    // Prepare for sending to Dart
    public virtual void PrepareForSending()
    {
        if (!_isPinned)
        {
            _handle = GCHandle.Alloc(BackingStruct, GCHandleType.Pinned);
            _isPinned = true;
        }

        // Prepare children
        foreach (var child in _children)
        {
            child.PrepareForSending();
        }
    }

    // Get pointer to backing struct
    public IntPtr Pointer =>
        _isPinned ? _handle.AddrOfPinnedObject() : IntPtr.Zero;

    // Implicit conversion for FFI
    public static implicit operator IntPtr(Widget w) =>
        w?.Pointer ?? IntPtr.Zero;

    // Dispose pattern
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            FlutterManager.UntrackWidget(this);
            foreach (var child in _children)
            {
                child.Dispose();
            }
        }

        if (_isPinned)
        {
            _handle.Free();
            _isPinned = false;
        }

        FreeUnmanagedResources();
        _isDisposed = true;
    }

    protected virtual void FreeUnmanagedResources() { }

    ~Widget()
    {
        Dispose(false);
    }
}
```

### Widget Registration

```csharp
// Widget type registry
public static class WidgetTypeRegistry
{
    private static readonly Dictionary<Type, int> _typeToId = new();
    private static readonly Dictionary<int, Type> _idToType = new();
    private static int _nextId = 1;

    public static int Register<T>() where T : Widget
    {
        var type = typeof(T);
        if (_typeToId.TryGetValue(type, out var id))
            return id;

        id = _nextId++;
        _typeToId[type] = id;
        _idToType[id] = type;
        return id;
    }

    public static int GetTypeId<T>() where T : Widget =>
        _typeToId.GetValueOrDefault(typeof(T), 0);

    public static Type? GetType(int id) =>
        _idToType.GetValueOrDefault(id);
}
```

## Generated Widget Class Pattern

### Single-Child Widget (Container)

```csharp
/// <summary>
/// A convenience widget that combines common painting, positioning,
/// and sizing widgets.
/// </summary>
public class Container : SingleChildRenderObjectWidget
{
    // Backing struct
    private ContainerStruct _struct;

    // Widget type discriminator
    public override int WidgetType => WidgetTypeRegistry.GetTypeId<Container>();

    // Backing struct access
    protected override BaseStruct BackingStruct => _struct;

    #region Properties

    /// <summary>
    /// Align the child within the container.
    /// </summary>
    public AlignmentGeometry? Alignment
    {
        get => _struct.hasAlignment != 0
               ? (AlignmentGeometry?)_struct.alignment
               : null;
        set {
            if (value.HasValue) {
                _struct.hasAlignment = 1;
                _struct.alignment = value.Value;
            } else {
                _struct.hasAlignment = 0;
            }
        }
    }

    /// <summary>
    /// Empty space to inscribe inside the decoration.
    /// </summary>
    public EdgeInsetsGeometry? Padding
    {
        get => _struct.padding;
        set => _struct.padding = value ?? EdgeInsetsGeometry.Zero;
    }

    /// <summary>
    /// The color to paint behind the child.
    /// </summary>
    public Color? Color
    {
        get => _struct.hasColor != 0 ? (Color?)_struct.color : null;
        set {
            if (value.HasValue) {
                _struct.hasColor = 1;
                _struct.color = value.Value;
            } else {
                _struct.hasColor = 0;
            }
        }
    }

    /// <summary>
    /// The decoration to paint behind the child.
    /// </summary>
    public Decoration? Decoration
    {
        get => _struct.decoration != IntPtr.Zero
               ? DecorationMarshaller.FromPointer(_struct.decoration)
               : null;
        set => _struct.decoration = value != null
               ? DecorationMarshaller.ToPointer(value)
               : IntPtr.Zero;
    }

    /// <summary>
    /// The child contained by the container.
    /// </summary>
    public Widget? Child
    {
        get => _child;
        set {
            _child = value;
            if (value != null) {
                value.Parent = this;
                _children.Clear();
                _children.Add(value);
            }
        }
    }
    private Widget? _child;

    /// <summary>
    /// Additional constraints to apply to the child.
    /// </summary>
    public BoxConstraints? Constraints
    {
        get => _struct.hasConstraints != 0
               ? (BoxConstraints?)_struct.constraints
               : null;
        set {
            if (value.HasValue) {
                _struct.hasConstraints = 1;
                _struct.constraints = value.Value;
            } else {
                _struct.hasConstraints = 0;
            }
        }
    }

    /// <summary>
    /// The transformation matrix to apply before painting.
    /// </summary>
    public Matrix4? Transform { get; set; }

    /// <summary>
    /// The clip behavior when Container.decoration has a clipPath.
    /// </summary>
    public Clip ClipBehavior
    {
        get => (Clip)_struct.clipBehavior;
        set => _struct.clipBehavior = (int)value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a container widget.
    /// </summary>
    public Container(
        Key? key = null,
        AlignmentGeometry? alignment = null,
        EdgeInsetsGeometry? padding = null,
        Color? color = null,
        Decoration? decoration = null,
        Decoration? foregroundDecoration = null,
        double? width = null,
        double? height = null,
        BoxConstraints? constraints = null,
        EdgeInsetsGeometry? margin = null,
        Matrix4? transform = null,
        AlignmentGeometry? transformAlignment = null,
        Widget? child = null,
        Clip clipBehavior = Clip.None)
        : base(key)
    {
        Alignment = alignment;
        Padding = padding;
        Color = color;
        Decoration = decoration;
        Constraints = constraints;
        Transform = transform;
        Child = child;
        ClipBehavior = clipBehavior;

        // Set width/height constraints
        if (width.HasValue || height.HasValue)
        {
            Constraints = new BoxConstraints(
                minWidth: width ?? 0,
                maxWidth: width ?? double.PositiveInfinity,
                minHeight: height ?? 0,
                maxHeight: height ?? double.PositiveInfinity
            );
        }
    }

    #endregion

    #region Prepare

    public override void PrepareForSending()
    {
        // Set child pointer
        if (_child != null)
        {
            _child.PrepareForSending();
            _struct.child = (IntPtr)_child;
        }

        base.PrepareForSending();
    }

    #endregion

    #region Dispose

    protected override void FreeUnmanagedResources()
    {
        // Free string pointers, etc.
        base.FreeUnmanagedResources();
    }

    #endregion
}
```

### Multi-Child Widget (Column)

```csharp
/// <summary>
/// A widget that displays its children in a vertical array.
/// </summary>
public class Column : Flex
{
    private ColumnStruct _struct;

    public override int WidgetType => WidgetTypeRegistry.GetTypeId<Column>();
    protected override BaseStruct BackingStruct => _struct;

    /// <summary>
    /// The children widgets.
    /// </summary>
    public new List<Widget> Children
    {
        get => _children;
        set {
            _children = value ?? new List<Widget>();
            foreach (var child in _children)
            {
                child.Parent = this;
            }
        }
    }

    /// <summary>
    /// How the children should be placed along the main axis.
    /// </summary>
    public MainAxisAlignment MainAxisAlignment
    {
        get => (MainAxisAlignment)_struct.mainAxisAlignment;
        set => _struct.mainAxisAlignment = (int)value;
    }

    /// <summary>
    /// How much space should be occupied in the main axis.
    /// </summary>
    public MainAxisSize MainAxisSize
    {
        get => (MainAxisSize)_struct.mainAxisSize;
        set => _struct.mainAxisSize = (int)value;
    }

    /// <summary>
    /// How the children should be placed along the cross axis.
    /// </summary>
    public CrossAxisAlignment CrossAxisAlignment
    {
        get => (CrossAxisAlignment)_struct.crossAxisAlignment;
        set => _struct.crossAxisAlignment = (int)value;
    }

    public Column(
        Key? key = null,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        MainAxisSize mainAxisSize = MainAxisSize.Max,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        TextDirection? textDirection = null,
        VerticalDirection verticalDirection = VerticalDirection.Down,
        TextBaseline? textBaseline = null,
        List<Widget>? children = null)
        : base(key)
    {
        MainAxisAlignment = mainAxisAlignment;
        MainAxisSize = mainAxisSize;
        CrossAxisAlignment = crossAxisAlignment;
        Children = children ?? new List<Widget>();
    }

    public override void PrepareForSending()
    {
        // Prepare children struct
        var count = _children.Count;
        if (count > 0)
        {
            var ptrs = new IntPtr[count];
            for (int i = 0; i < count; i++)
            {
                _children[i].PrepareForSending();
                ptrs[i] = (IntPtr)_children[i];
            }

            _childrenHandle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
            _struct.children.items = _childrenHandle.AddrOfPinnedObject();
            _struct.children.count = count;
        }

        base.PrepareForSending();
    }

    private GCHandle _childrenHandle;

    protected override void FreeUnmanagedResources()
    {
        if (_childrenHandle.IsAllocated)
            _childrenHandle.Free();
        base.FreeUnmanagedResources();
    }
}
```

### Callback Widget (GestureDetector)

```csharp
/// <summary>
/// A widget that detects gestures.
/// </summary>
public class GestureDetector : SingleChildRenderObjectWidget
{
    private GestureDetectorStruct _struct;

    public override int WidgetType => WidgetTypeRegistry.GetTypeId<GestureDetector>();
    protected override BaseStruct BackingStruct => _struct;

    /// <summary>
    /// Called when the user taps the widget.
    /// </summary>
    public Action? OnTap
    {
        get => _onTap;
        set {
            _onTap = value;
            if (value != null)
            {
                var actionId = CallbackRegistry.Register(value);
                _struct.onTap_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
            }
            else
            {
                FreePointer(ref _struct.onTap_ptr);
            }
        }
    }
    private Action? _onTap;

    /// <summary>
    /// Called when the user double taps the widget.
    /// </summary>
    public Action? OnDoubleTap
    {
        get => _onDoubleTap;
        set {
            _onDoubleTap = value;
            if (value != null)
            {
                var actionId = CallbackRegistry.Register(value);
                _struct.onDoubleTap_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
            }
            else
            {
                FreePointer(ref _struct.onDoubleTap_ptr);
            }
        }
    }
    private Action? _onDoubleTap;

    /// <summary>
    /// Called when the user long presses the widget.
    /// </summary>
    public Action? OnLongPress
    {
        get => _onLongPress;
        set {
            _onLongPress = value;
            if (value != null)
            {
                var actionId = CallbackRegistry.Register(value);
                _struct.onLongPress_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
            }
            else
            {
                FreePointer(ref _struct.onLongPress_ptr);
            }
        }
    }
    private Action? _onLongPress;

    /// <summary>
    /// The child widget.
    /// </summary>
    public Widget? Child
    {
        get => _child;
        set {
            _child = value;
            if (value != null)
            {
                value.Parent = this;
                _children.Clear();
                _children.Add(value);
            }
        }
    }
    private Widget? _child;

    public GestureDetector(
        Key? key = null,
        Widget? child = null,
        Action? onTap = null,
        Action? onDoubleTap = null,
        Action? onLongPress = null,
        HitTestBehavior behavior = HitTestBehavior.DeferToChild)
        : base(key)
    {
        Child = child;
        OnTap = onTap;
        OnDoubleTap = onDoubleTap;
        OnLongPress = onLongPress;
    }

    public override void PrepareForSending()
    {
        if (_child != null)
        {
            _child.PrepareForSending();
            _struct.child = (IntPtr)_child;
        }
        base.PrepareForSending();
    }

    protected override void FreeUnmanagedResources()
    {
        FreePointer(ref _struct.onTap_ptr);
        FreePointer(ref _struct.onDoubleTap_ptr);
        FreePointer(ref _struct.onLongPress_ptr);
        base.FreeUnmanagedResources();
    }

    private void FreePointer(ref IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(ptr);
            ptr = IntPtr.Zero;
        }
    }
}
```

## Property Patterns

### Simple Value Property

```csharp
// Non-nullable value type
public double Opacity
{
    get => _struct.opacity;
    set => _struct.opacity = value;
}

// Nullable value type
public double? Opacity
{
    get => _struct.hasOpacity != 0 ? _struct.opacity : null;
    set {
        if (value.HasValue) {
            _struct.hasOpacity = 1;
            _struct.opacity = value.Value;
        } else {
            _struct.hasOpacity = 0;
        }
    }
}
```

### String Property

```csharp
private IntPtr _text_ptr;

public string? Text
{
    get => _text_ptr == IntPtr.Zero
           ? null
           : Marshal.PtrToStringUTF8(_text_ptr);
    set {
        if (_text_ptr != IntPtr.Zero)
            Marshal.FreeCoTaskMem(_text_ptr);
        _text_ptr = value == null
           ? IntPtr.Zero
           : Marshal.StringToCoTaskMemUTF8(value);
    }
}
```

### Enum Property

```csharp
public TextAlign TextAlign
{
    get => (TextAlign)_struct.textAlign;
    set => _struct.textAlign = (int)value;
}
```

### Nested Struct Property

```csharp
public Color? Color
{
    get => _struct.hasColor != 0 ? (Color?)_struct.color : null;
    set {
        if (value.HasValue) {
            _struct.hasColor = 1;
            _struct.color = (ColorStruct)value.Value;
        } else {
            _struct.hasColor = 0;
        }
    }
}

public EdgeInsets Padding
{
    get => (EdgeInsets)_struct.padding;
    set => _struct.padding = (EdgeInsetsStruct)value;
}
```

### Widget Reference Property

```csharp
private Widget? _child;

public Widget? Child
{
    get => _child;
    set {
        // Remove from old parent
        if (_child != null)
            _child.Parent = null;

        _child = value;

        // Set new parent
        if (value != null)
        {
            value.Parent = this;
            _children.Clear();
            _children.Add(value);
        }
    }
}
```

### Callback Property

```csharp
private Action? _onPressed;

public Action? OnPressed
{
    get => _onPressed;
    set {
        _onPressed = value;
        if (value != null)
        {
            var actionId = CallbackRegistry.Register(value);
            _struct.onPressed_ptr = Marshal.StringToCoTaskMemUTF8(actionId);
        }
        else if (_struct.onPressed_ptr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(_struct.onPressed_ptr);
            _struct.onPressed_ptr = IntPtr.Zero;
        }
    }
}
```

## Widget Composition

### Fluent API

```csharp
// Extension methods for fluent widget building
public static class WidgetExtensions
{
    public static T WithPadding<T>(this T widget, EdgeInsets padding)
        where T : Widget
    {
        return new Padding(padding: padding, child: widget) as T
               ?? throw new InvalidOperationException();
    }

    public static Widget Center(this Widget widget) =>
        new Center(child: widget);

    public static Widget Expanded(this Widget widget, int flex = 1) =>
        new Expanded(flex: flex, child: widget);
}

// Usage
var button = new Text("Click me")
    .WithPadding(EdgeInsets.All(16))
    .Center();
```

### Builder Pattern

```csharp
public class ContainerBuilder
{
    private Color? _color;
    private EdgeInsets? _padding;
    private Widget? _child;

    public ContainerBuilder Color(Color color)
    {
        _color = color;
        return this;
    }

    public ContainerBuilder Padding(EdgeInsets padding)
    {
        _padding = padding;
        return this;
    }

    public ContainerBuilder Child(Widget child)
    {
        _child = child;
        return this;
    }

    public Container Build() =>
        new Container(
            color: _color,
            padding: _padding,
            child: _child
        );
}

// Usage
var container = new ContainerBuilder()
    .Color(Colors.Blue)
    .Padding(EdgeInsets.All(16))
    .Child(new Text("Hello"))
    .Build();
```

## Widget Updates

### Update Protocol

```csharp
public class Widget
{
    // Mark widget as needing update
    public void MarkNeedsUpdate()
    {
        FlutterManager.QueueUpdate(this);
    }

    // Property change triggers update
    public Color Color
    {
        set {
            if (_struct.color != value)
            {
                _struct.color = value;
                MarkNeedsUpdate();
            }
        }
    }
}
```

### Batch Updates

```csharp
// Queue multiple updates
FlutterManager.BeginBatchUpdate();
container.Color = Colors.Red;
text.Data = "Updated text";
FlutterManager.EndBatchUpdate();  // Sends single BatchUpdate message
```

## Widget Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                     Widget Lifecycle                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   1. Construction                                               │
│      new Container(...)                                         │
│      - Allocates backing struct                                 │
│      - Registers with FlutterManager                            │
│      - Initializes properties                                   │
│                                                                 │
│   2. Configuration                                              │
│      container.Color = Colors.Blue;                             │
│      container.Child = new Text("Hello");                       │
│      - Sets struct fields                                       │
│      - Establishes parent-child relationships                   │
│                                                                 │
│   3. Preparation                                                │
│      container.PrepareForSending();                             │
│      - Pins memory                                              │
│      - Recursively prepares children                            │
│      - Sets child pointers in struct                            │
│                                                                 │
│   4. Sending                                                    │
│      FlutterManager.SendWidget(container);                      │
│      - Sends UpdateComponent message                            │
│      - Dart receives and renders                                │
│                                                                 │
│   5. Updates (optional)                                         │
│      container.Color = Colors.Red;                              │
│      - Triggers MarkNeedsUpdate()                               │
│      - Queues update message                                    │
│                                                                 │
│   6. Disposal                                                   │
│      container.Dispose();                                       │
│      - Sends Dispose message                                    │
│      - Unpins memory                                            │
│      - Frees string pointers                                    │
│      - Recursively disposes children                            │
│      - Unregisters from FlutterManager                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Error Handling

### Property Validation

```csharp
public double Opacity
{
    set {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(
                nameof(value), "Opacity must be between 0.0 and 1.0");
        _struct.opacity = value;
    }
}
```

### Parent-Child Validation

```csharp
public Widget? Child
{
    set {
        if (value == this)
            throw new ArgumentException("Widget cannot be its own child");
        if (value != null && value.Parent != null)
            throw new InvalidOperationException("Widget already has a parent");
        // ...
    }
}
```

## See Also

- [CALLBACKS-EVENTS.md](./CALLBACKS-EVENTS.md) - Callback handling
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Type mappings
- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) - FFI protocol
