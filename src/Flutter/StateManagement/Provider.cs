#nullable enable
using System;
using System.Collections.Concurrent;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.StateManagement
{
    /// <summary>
    /// A widget that provides a value to its descendants.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provider is the simplest form of dependency injection in FlutterSharp.
    /// It stores a value and makes it available to any widget in its subtree
    /// that uses Consumer&lt;T&gt; to access it.
    /// </para>
    /// <para>
    /// For values that change over time, use ChangeNotifierProvider&lt;T&gt;
    /// which automatically notifies consumers when the value changes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var counter = new CounterService();
    ///
    /// var app = new Provider&lt;CounterService&gt;(
    ///     value: counter,
    ///     child: new MyApp()
    /// );
    /// </code>
    /// </example>
    public class Provider<T> : Widget where T : class
    {
        private static readonly ConcurrentDictionary<Type, object?> _providers = new();

        private readonly T? _value;
        private Widget? _child;

        /// <summary>
        /// Creates a Provider that makes a value available to descendants.
        /// </summary>
        /// <param name="value">The value to provide to descendants.</param>
        /// <param name="child">The widget subtree that can access this value.</param>
        public Provider(T? value, Widget? child = null)
        {
            _value = value;
            _child = child;

            // Register this provider globally by type
            // This is a simplified approach - in a full implementation,
            // we would use widget tree context traversal
            _providers[typeof(T)] = value;

            FlutterManager.TrackWidget(this);

            var s = GetBackingStruct<ProviderStruct>();
            s.Id = Id;
            UpdateChildInStruct();
        }

        /// <summary>
        /// The value provided to descendants.
        /// </summary>
        public T? Value => _value;

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
        /// Gets the value of the nearest ancestor Provider of type T.
        /// </summary>
        /// <returns>The provided value, or null if not found.</returns>
        public static T? Of()
        {
            if (_providers.TryGetValue(typeof(T), out var value))
            {
                return value as T;
            }
            return default;
        }

        /// <summary>
        /// Gets the value of the nearest ancestor Provider of type T.
        /// Throws if the provider is not found.
        /// </summary>
        /// <returns>The provided value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no provider of type T is found.</exception>
        public static T Read()
        {
            var value = Of();
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"No Provider<{typeof(T).Name}> found. " +
                    $"Ensure a Provider<{typeof(T).Name}> is an ancestor of the widget calling Provider<{typeof(T).Name}>.Read().");
            }
            return value;
        }

        private void UpdateChildInStruct()
        {
            var s = GetBackingStruct<ProviderStruct>();
            s.child = (IntPtr)_child;
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new ProviderStruct();
            s.Id = Id;
            s.child = (IntPtr)_child;
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Remove this provider from the global registry
                _providers.TryRemove(typeof(T), out _);
                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Non-generic Provider for static access patterns.
    /// </summary>
    public static class Provider
    {
        private static readonly ConcurrentDictionary<Type, object?> _providers = new();

        /// <summary>
        /// Registers a value as a provider.
        /// </summary>
        internal static void Register<T>(T? value) where T : class
        {
            _providers[typeof(T)] = value;
        }

        /// <summary>
        /// Unregisters a provider.
        /// </summary>
        internal static void Unregister<T>() where T : class
        {
            _providers.TryRemove(typeof(T), out _);
        }

        /// <summary>
        /// Gets the value of a registered provider.
        /// </summary>
        public static T? Of<T>() where T : class
        {
            if (_providers.TryGetValue(typeof(T), out var value))
            {
                return value as T;
            }
            return default;
        }

        /// <summary>
        /// Gets the value of a registered provider. Throws if not found.
        /// </summary>
        public static T Read<T>() where T : class
        {
            var value = Of<T>();
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"No Provider<{typeof(T).Name}> found. " +
                    $"Ensure a Provider<{typeof(T).Name}> is registered before calling Provider.Read<{typeof(T).Name}>().");
            }
            return value;
        }
    }
}
