#nullable enable
using System;
using Flutter.Foundation;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.StateManagement
{
    /// <summary>
    /// A widget that consumes a Provider and rebuilds when its value changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Consumer is used to listen to a Provider and rebuild whenever the
    /// provided value changes. It's the primary way to read and react to
    /// state provided by Provider or ChangeNotifierProvider.
    /// </para>
    /// <para>
    /// The builder callback receives the current value and returns a widget.
    /// When the value changes (for ChangeNotifier-based providers), the
    /// builder is called again with the new value.
    /// </para>
    /// <para>
    /// For optimal performance, pass static widgets that don't depend on
    /// the value as the child parameter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// new Consumer&lt;CounterViewModel&gt;(
    ///     builder: (vm, child) => new Column {
    ///         new Text($"Count: {vm.Count}"),
    ///         child! // Static content
    ///     },
    ///     child: new Text("This doesn't rebuild")
    /// );
    /// </code>
    /// </example>
    public class Consumer<T> : Widget where T : class
    {
        private Func<T, Widget?, Widget>? _builder;
        private Widget? _child;
        private Widget? _builtWidget;
        private T? _subscribedValue;

        /// <summary>
        /// Creates a Consumer that listens to a Provider of type T.
        /// </summary>
        /// <param name="builder">
        /// A function that builds a widget using the provided value.
        /// The second parameter is the optional child widget for optimization.
        /// </param>
        /// <param name="child">
        /// Optional child widget that doesn't depend on the provided value.
        /// Passed to the builder for performance optimization.
        /// </param>
        public Consumer(
            Func<T, Widget?, Widget>? builder = null,
            Widget? child = null)
        {
            _builder = builder;
            _child = child;

            FlutterManager.TrackWidget(this);

            var s = GetBackingStruct<ConsumerStruct>();
            s.Id = Id;

            // Subscribe to provider if it's a ChangeNotifier
            SubscribeToProvider();

            // Initial build
            RebuildWidgetTree();
        }

        /// <summary>
        /// Alternative constructor with a simpler builder signature.
        /// </summary>
        /// <param name="builder">A function that builds a widget using just the provided value.</param>
        public Consumer(Func<T, Widget> builder)
            : this((value, child) => builder(value))
        {
        }

        /// <summary>
        /// The builder function that creates the widget.
        /// </summary>
        public Func<T, Widget?, Widget>? Builder
        {
            get => _builder;
            set
            {
                _builder = value;
                RebuildWidgetTree();
                FlutterManager.SendState(this);
            }
        }

        /// <summary>
        /// Optional child widget that doesn't depend on the provided value.
        /// </summary>
        public Widget? Child
        {
            get => _child;
            set
            {
                _child = value;
                RebuildWidgetTree();
                FlutterManager.SendState(this);
            }
        }

        private void SubscribeToProvider()
        {
            // Get the value from the provider
            var value = GetProviderValue();

            // If T is a ChangeNotifier, subscribe to its changes
            if (value is ChangeNotifier notifier)
            {
                _subscribedValue = value;
                notifier.AddListener(OnProviderChanged);
            }
        }

        private T? GetProviderValue()
        {
            // Try to get the value from the Provider registry
            return Provider.Of<T>();
        }

        private void OnProviderChanged()
        {
            RebuildWidgetTree();
            FlutterManager.SendState(this);
        }

        private void RebuildWidgetTree()
        {
            var value = GetProviderValue();

            if (_builder != null && value != null)
            {
                _builtWidget = _builder(value, _child);
            }
            else
            {
                _builtWidget = _child;
            }

            // Update the struct with the built widget
            if (backingStruct is ConsumerStruct s)
            {
                s.builtChild = (IntPtr)_builtWidget;
            }
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new ConsumerStruct();
            s.Id = Id;
            s.builtChild = (IntPtr)_builtWidget;
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from the notifier
                if (_subscribedValue is ChangeNotifier notifier)
                {
                    notifier.RemoveListener(OnProviderChanged);
                }
                _subscribedValue = default;

                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }
}
