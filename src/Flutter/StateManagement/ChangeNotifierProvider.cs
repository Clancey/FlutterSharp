#nullable enable
using System;
using System.Collections.Concurrent;
using Flutter.Foundation;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.StateManagement
{
    /// <summary>
    /// A Provider that listens to a ChangeNotifier and automatically
    /// notifies consumers when the value changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ChangeNotifierProvider combines Provider and ListenableBuilder patterns.
    /// It provides a ChangeNotifier to descendants and automatically triggers
    /// rebuilds when the notifier calls NotifyListeners().
    /// </para>
    /// <para>
    /// Use this for state classes that extend ChangeNotifier, such as
    /// view models, services, or any class with observable state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class CounterViewModel : ChangeNotifier
    /// {
    ///     private int _count = 0;
    ///     public int Count => _count;
    ///
    ///     public void Increment()
    ///     {
    ///         _count++;
    ///         NotifyListeners();
    ///     }
    /// }
    ///
    /// var app = new ChangeNotifierProvider&lt;CounterViewModel&gt;(
    ///     create: () => new CounterViewModel(),
    ///     child: new Consumer&lt;CounterViewModel&gt;(
    ///         builder: (vm) => new Text($"Count: {vm.Count}")
    ///     )
    /// );
    /// </code>
    /// </example>
    public class ChangeNotifierProvider<T> : Widget where T : ChangeNotifier
    {
        private static readonly ConcurrentDictionary<Type, ChangeNotifier?> _providers = new();

        private readonly T? _value;
        private readonly Func<T>? _create;
        private readonly bool _disposeValue;
        private Widget? _child;
        private bool _valueCreated;

        /// <summary>
        /// Creates a ChangeNotifierProvider with an existing value.
        /// </summary>
        /// <param name="value">The ChangeNotifier to provide.</param>
        /// <param name="child">The widget subtree that can access this value.</param>
        /// <param name="disposeValue">Whether to dispose the value when this provider is disposed.</param>
        public ChangeNotifierProvider(T value, Widget? child = null, bool disposeValue = true)
        {
            _value = value;
            _child = child;
            _disposeValue = disposeValue;
            _valueCreated = true;

            RegisterProvider();
            SubscribeToNotifier();
            InitializeWidget();
        }

        /// <summary>
        /// Creates a ChangeNotifierProvider with a factory function.
        /// </summary>
        /// <param name="create">A factory function that creates the ChangeNotifier.</param>
        /// <param name="child">The widget subtree that can access this value.</param>
        public ChangeNotifierProvider(Func<T> create, Widget? child = null)
        {
            _create = create;
            _child = child;
            _disposeValue = true; // Always dispose created values

            // Lazy creation - value is created when first accessed
            InitializeWidget();
        }

        /// <summary>
        /// The ChangeNotifier provided to descendants.
        /// </summary>
        public T? Value
        {
            get
            {
                if (!_valueCreated && _create != null)
                {
                    var createdValue = _create();
                    _providers[typeof(T)] = createdValue;
                    createdValue?.AddListener(OnValueChanged);
                    _valueCreated = true;
                    return createdValue;
                }
                return _value;
            }
        }

        /// <summary>
        /// The child widget subtree.
        /// </summary>
        public Widget? Child
        {
            get => _child;
            set
            {
                _child = value;
                UpdateChildInStruct();
            }
        }

        /// <summary>
        /// Gets the value of the nearest ancestor ChangeNotifierProvider of type T.
        /// </summary>
        public static T? Of()
        {
            if (_providers.TryGetValue(typeof(T), out var value))
            {
                return value as T;
            }
            return default;
        }

        /// <summary>
        /// Gets the value of the nearest ancestor ChangeNotifierProvider of type T.
        /// Throws if not found.
        /// </summary>
        public static T Read()
        {
            var value = Of();
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"No ChangeNotifierProvider<{typeof(T).Name}> found. " +
                    $"Ensure a ChangeNotifierProvider<{typeof(T).Name}> is an ancestor of the widget calling ChangeNotifierProvider<{typeof(T).Name}>.Read().");
            }
            return value;
        }

        /// <summary>
        /// Watches the provider and triggers rebuilds when it changes.
        /// Use this inside Consumer widgets for automatic updates.
        /// </summary>
        public static T Watch()
        {
            // In a real implementation, this would register a dependency
            // For now, it's equivalent to Read()
            return Read();
        }

        private void RegisterProvider()
        {
            _providers[typeof(T)] = _value;
        }

        private void SubscribeToNotifier()
        {
            _value?.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            // Notify Flutter that the state has changed
            FlutterManager.SendState(this);
        }

        private void InitializeWidget()
        {
            FlutterManager.TrackWidget(this);

            var s = GetBackingStruct<ChangeNotifierProviderStruct>();
            s.Id = Id;
            UpdateChildInStruct();
        }

        private void UpdateChildInStruct()
        {
            var s = GetBackingStruct<ChangeNotifierProviderStruct>();
            s.child = (IntPtr)_child;
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new ChangeNotifierProviderStruct();
            s.Id = Id;
            s.child = (IntPtr)_child;
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from the notifier
                if (_value != null)
                {
                    _value.RemoveListener(OnValueChanged);

                    // Dispose the value if we own it
                    if (_disposeValue)
                    {
                        _value.Dispose();
                    }
                }

                // Remove from global registry
                _providers.TryRemove(typeof(T), out _);
                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }
}
