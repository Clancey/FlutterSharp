#nullable enable
using System;
using System.Collections.Generic;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.StateManagement
{
    /// <summary>
    /// Represents a provider that can be used in a MultiProvider.
    /// </summary>
    public interface IProviderNode
    {
        /// <summary>
        /// Wraps the given child widget with this provider.
        /// </summary>
        Widget Build(Widget? child);
    }

    /// <summary>
    /// A convenience wrapper for creating provider nodes.
    /// </summary>
    public class ProviderNode<T> : IProviderNode where T : class
    {
        private readonly T? _value;

        /// <summary>
        /// Creates a provider node with the given value.
        /// </summary>
        public ProviderNode(T value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public Widget Build(Widget? child)
        {
            return new Provider<T>(_value, child);
        }
    }

    /// <summary>
    /// A widget that composes multiple providers into a single widget tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MultiProvider is a convenience widget that simplifies the nesting of
    /// multiple providers. Instead of deeply nesting providers, you can list
    /// them in a flat array.
    /// </para>
    /// <para>
    /// The providers are applied in order, with each provider wrapping
    /// the next in the list. The child widget is wrapped by the innermost
    /// provider.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Instead of:
    /// // new Provider&lt;AuthService&gt;(auth, child:
    /// //   new Provider&lt;ApiService&gt;(api, child:
    /// //     new Provider&lt;UserService&gt;(user, child: app)))
    ///
    /// // Use MultiProvider:
    /// new MultiProvider(
    ///     providers: new IProviderNode[] {
    ///         new ProviderNode&lt;AuthService&gt;(auth),
    ///         new ProviderNode&lt;ApiService&gt;(api),
    ///         new ProviderNode&lt;UserService&gt;(user),
    ///     },
    ///     child: app
    /// );
    /// </code>
    /// </example>
    public class MultiProvider : Widget
    {
        private readonly IProviderNode[] _providers;
        private Widget? _child;
        private Widget? _builtWidget;

        /// <summary>
        /// Creates a MultiProvider that composes multiple providers.
        /// </summary>
        /// <param name="providers">The list of providers to compose.</param>
        /// <param name="child">The child widget that can access all providers.</param>
        public MultiProvider(
            IProviderNode[] providers,
            Widget? child = null)
        {
            _providers = providers ?? Array.Empty<IProviderNode>();
            _child = child;

            FlutterManager.TrackWidget(this);

            var s = GetBackingStruct<MultiProviderStruct>();
            s.Id = Id;

            // Build the nested provider tree
            RebuildWidgetTree();
        }

        /// <summary>
        /// The list of providers to compose.
        /// </summary>
        public IProviderNode[] Providers => _providers;

        /// <summary>
        /// The child widget that can access all providers.
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

        private void RebuildWidgetTree()
        {
            // Build from inside out: child -> innermost provider -> ... -> outermost provider
            Widget? current = _child;

            // Iterate in reverse so that the first provider in the array
            // ends up as the outermost widget
            for (int i = _providers.Length - 1; i >= 0; i--)
            {
                current = _providers[i].Build(current);
            }

            _builtWidget = current;

            // Update the struct with the built widget
            if (backingStruct is MultiProviderStruct s)
            {
                s.builtChild = (IntPtr)_builtWidget;
            }
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new MultiProviderStruct();
            s.Id = Id;
            s.builtChild = (IntPtr)_builtWidget;
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlutterManager.UntrackWidget(this);
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Extension methods for creating provider nodes.
    /// </summary>
    public static class ProviderExtensions
    {
        /// <summary>
        /// Creates a ProviderNode for use in MultiProvider.
        /// </summary>
        public static IProviderNode AsProviderNode<T>(this T value) where T : class
        {
            return new ProviderNode<T>(value);
        }
    }
}
