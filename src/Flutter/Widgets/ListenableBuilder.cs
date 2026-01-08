#nullable enable
using System;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.Widgets
{
    /// <summary>
    /// A general-purpose widget for building a widget subtree when a Listenable changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ListenableBuilder is useful for more complex widgets that wish to listen
    /// to changes in other objects as part of a larger build function.
    /// </para>
    /// <para>
    /// Any subtype of Listenable (such as a ChangeNotifier, ValueNotifier, or
    /// Animation) can be used with a ListenableBuilder to rebuild only certain
    /// parts of a widget when the Listenable notifies its listeners.
    /// </para>
    /// <para>
    /// The child parameter is optional but recommended for performance optimization.
    /// If the builder function contains a subtree that does not depend on the
    /// listenable, pass it as the child parameter. The child is passed back to
    /// the builder callback and can be incorporated into the build, avoiding
    /// unnecessary rebuilds.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var counter = new ValueNotifier&lt;int&gt;(0);
    ///
    /// var widget = new ListenableBuilder(
    ///     listenable: counter,
    ///     builder: (child) => new Column {
    ///         new Text($"Count: {counter.Value}"),
    ///         child // Static child widget
    ///     },
    ///     child: new Text("This text doesn't change")
    /// );
    ///
    /// // Later: increment counter to trigger rebuild
    /// counter.Value++;
    /// </code>
    /// </example>
    public class ListenableBuilder : Widget
    {
        private Listenable? _listenable;
        private Func<Widget?, Widget>? _builder;
        private Widget? _child;
        private Widget? _builtWidget;

        /// <summary>
        /// Creates a ListenableBuilder widget.
        /// </summary>
        /// <param name="listenable">The Listenable to listen to for changes.</param>
        /// <param name="builder">
        /// Called every time the listenable notifies about a change.
        /// The child given to the builder should typically be part of the returned widget tree.
        /// </param>
        /// <param name="child">
        /// Optional child widget that doesn't depend on the listenable's state.
        /// This child is passed to the builder for performance optimization.
        /// </param>
        public ListenableBuilder(
            Listenable? listenable = null,
            Func<Widget?, Widget>? builder = null,
            Widget? child = null)
        {
            _listenable = listenable;
            _builder = builder;
            _child = child;

            // Subscribe to listenable notifications
            if (_listenable != null)
            {
                _listenable.AddListener(OnListenableChanged);
            }

            // Track this widget so events can be routed to it
            FlutterManager.TrackWidget(this);

            // Initialize the backing struct
            var s = GetBackingStruct<ListenableBuilderStruct>();
            s.Id = Id;

            // Build the initial widget tree
            RebuildWidgetTree();
        }

        /// <summary>
        /// The Listenable to listen to for changes.
        /// </summary>
        public Listenable? Listenable
        {
            get => _listenable;
            set
            {
                if (_listenable == value) return;

                // Unsubscribe from old listenable
                _listenable?.RemoveListener(OnListenableChanged);

                _listenable = value;

                // Subscribe to new listenable
                _listenable?.AddListener(OnListenableChanged);

                // Rebuild immediately
                RebuildWidgetTree();
            }
        }

        /// <summary>
        /// Called every time the listenable notifies about a change.
        /// </summary>
        public Func<Widget?, Widget>? Builder
        {
            get => _builder;
            set
            {
                _builder = value;
                RebuildWidgetTree();
            }
        }

        /// <summary>
        /// Optional child widget that doesn't depend on the listenable's state.
        /// </summary>
        public Widget? Child
        {
            get => _child;
            set
            {
                _child = value;
                RebuildWidgetTree();
            }
        }

        /// <summary>
        /// Called when the listenable notifies of a change.
        /// </summary>
        private void OnListenableChanged()
        {
            RebuildWidgetTree();
            // Send updated state to Flutter
            FlutterManager.SendState(this);
        }

        /// <summary>
        /// Rebuilds the widget tree by calling the builder.
        /// </summary>
        private void RebuildWidgetTree()
        {
            if (_builder != null)
            {
                _builtWidget = _builder(_child);
            }
            else
            {
                _builtWidget = _child;
            }

            // Update the struct with the built widget
            if (backingStruct is ListenableBuilderStruct s)
            {
                // Use implicit Widget -> IntPtr conversion which calls PrepareForSending internally
                s.builtChild = (IntPtr)_builtWidget;
            }
        }

        /// <summary>
        /// Returns the built widget tree for rendering.
        /// </summary>
        internal Widget? GetBuiltWidget() => _builtWidget;

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new ListenableBuilderStruct();
            s.Id = Id;

            // Set built child using implicit conversion (handles null and PrepareForSending)
            s.builtChild = (IntPtr)_builtWidget;

            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from listenable
                _listenable?.RemoveListener(OnListenableChanged);
                _listenable = null;

                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }
}
