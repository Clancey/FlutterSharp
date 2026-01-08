#nullable enable
using System;
using System.Collections.Generic;
using Flutter.Foundation;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.StateManagement
{
    /// <summary>
    /// A widget that selects a subset of a provided value and only rebuilds
    /// when that subset changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Selector is an optimization over Consumer. It allows you to select
    /// a specific piece of data from a larger provided value, and only
    /// rebuild when that specific piece changes.
    /// </para>
    /// <para>
    /// This is useful when a provider contains a large model but you only
    /// care about one or two properties in a particular widget.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Only rebuilds when user.name changes, not when other User properties change
    /// new Selector&lt;UserModel, string&gt;(
    ///     selector: (model) => model.Name,
    ///     builder: (name, child) => new Text($"Hello, {name}!")
    /// );
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of the provider value.</typeparam>
    /// <typeparam name="S">The type of the selected subset.</typeparam>
    public class Selector<T, S> : Widget where T : class
    {
        private Func<T, S>? _selector;
        private Func<S, Widget?, Widget>? _builder;
        private Widget? _child;
        private Widget? _builtWidget;
        private T? _subscribedValue;
        private S? _lastSelectedValue;

        /// <summary>
        /// Creates a Selector that selects a subset of a Provider value.
        /// </summary>
        /// <param name="selector">A function that selects a subset from the provided value.</param>
        /// <param name="builder">A function that builds a widget using the selected value.</param>
        /// <param name="child">Optional child widget that doesn't depend on the selected value.</param>
        public Selector(
            Func<T, S>? selector = null,
            Func<S, Widget?, Widget>? builder = null,
            Widget? child = null)
        {
            _selector = selector;
            _builder = builder;
            _child = child;

            FlutterManager.TrackWidget(this);

            var s = GetBackingStruct<SelectorStruct>();
            s.Id = Id;

            // Subscribe to provider if it's a ChangeNotifier
            SubscribeToProvider();

            // Initial build
            RebuildWidgetTree();
        }

        /// <summary>
        /// Alternative constructor with a simpler builder signature.
        /// </summary>
        public Selector(Func<T, S> selector, Func<S, Widget> builder)
            : this(selector, (value, child) => builder(value))
        {
        }

        /// <summary>
        /// The selector function that extracts a subset from the provider value.
        /// </summary>
        public Func<T, S>? SelectorFunc
        {
            get => _selector;
            set
            {
                _selector = value;
                _lastSelectedValue = default;
                RebuildWidgetTree();
                FlutterManager.SendState(this);
            }
        }

        /// <summary>
        /// The builder function that creates the widget.
        /// </summary>
        public Func<S, Widget?, Widget>? Builder
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
        /// Optional child widget that doesn't depend on the selected value.
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
            var providerValue = GetProviderValue();

            if (providerValue == null || _selector == null)
            {
                return;
            }

            // Get the new selected value
            var newSelectedValue = _selector(providerValue);

            // Only rebuild if the selected value has changed
            if (!EqualityComparer<S>.Default.Equals(_lastSelectedValue, newSelectedValue))
            {
                _lastSelectedValue = newSelectedValue;
                RebuildWidgetTree();
                FlutterManager.SendState(this);
            }
        }

        private void RebuildWidgetTree()
        {
            var providerValue = GetProviderValue();

            if (_builder != null && providerValue != null && _selector != null)
            {
                var selectedValue = _selector(providerValue);
                _lastSelectedValue = selectedValue;
                _builtWidget = _builder(selectedValue, _child);
            }
            else
            {
                _builtWidget = _child;
            }

            // Update the struct with the built widget
            if (backingStruct is SelectorStruct s)
            {
                s.builtChild = (IntPtr)_builtWidget;
            }
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new SelectorStruct();
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
                _lastSelectedValue = default;

                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }
}
