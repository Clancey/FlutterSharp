// Manual implementation for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

using System;
using System.Collections.Generic;
using System.Linq;
using Flutter;
using Flutter.Structs;

namespace Flutter.Widgets
{
    /// <summary>
    /// A widget that manages a set of child widgets with a stack discipline.
    ///
    /// This Navigator implementation uses named routes and provides push/pop methods
    /// for navigation. Routes are defined as string names mapped to widget builder functions.
    /// </summary>
    /// <example>
    /// <code>
    /// var nav = new Navigator
    /// {
    ///     InitialRoute = "/",
    ///     Routes = new Dictionary&lt;string, Func&lt;Widget&gt;&gt;
    ///     {
    ///         { "/", () => new HomePage() },
    ///         { "/details", () => new DetailsPage() },
    ///         { "/settings", () => new SettingsPage() }
    ///     },
    ///     OnRouteChanged = (routeName) => Console.WriteLine($"Navigated to {routeName}")
    /// };
    ///
    /// // Push a new route
    /// nav.Push("/details");
    ///
    /// // Pop the current route
    /// nav.Pop();
    /// </code>
    /// </example>
    public class Navigator : StatefulWidget
    {
        private readonly Dictionary<string, Func<Widget>> _routes = new();
        private readonly Stack<string> _routeStack = new();
        private string? _currentRoute;
        private Widget? _currentChildWidget;

        /// <summary>
        /// Initializes a new instance of the <see cref="Navigator"/> class.
        /// </summary>
        /// <param name="initialRoute">The name of the first route to show. Defaults to "/".</param>
        /// <param name="maintainState">Whether to maintain the state of inactive routes.</param>
        /// <param name="clipBehavior">How to clip the Navigator content.</param>
        /// <param name="restorationScopeId">Restoration ID for state restoration.</param>
        public Navigator(
            string? initialRoute = "/",
            bool maintainState = true,
            Clip clipBehavior = Clip.HardEdge,
            string? restorationScopeId = null)
        {
            InitialRoute = initialRoute ?? "/";
            MaintainState = maintainState;
            ClipBehavior = clipBehavior;
            RestorationScopeId = restorationScopeId;

            _currentRoute = InitialRoute;
            _routeStack.Push(_currentRoute);
        }

        /// <summary>
        /// The dictionary of named routes.
        /// Keys are route names (e.g., "/", "/home", "/settings").
        /// Values are functions that return the Widget to display for that route.
        /// </summary>
        public Dictionary<string, Func<Widget>> Routes
        {
            get => _routes;
            init
            {
                _routes.Clear();
                foreach (var kvp in value)
                {
                    _routes[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Adds a route to the Navigator.
        /// </summary>
        /// <param name="routeName">The name of the route (e.g., "/settings").</param>
        /// <param name="builder">A function that builds the widget for this route.</param>
        public void AddRoute(string routeName, Func<Widget> builder)
        {
            _routes[routeName] = builder;
        }

        /// <summary>
        /// Removes a route from the Navigator.
        /// </summary>
        /// <param name="routeName">The name of the route to remove.</param>
        /// <returns>True if the route was removed, false if it wasn't found.</returns>
        public bool RemoveRoute(string routeName)
        {
            return _routes.Remove(routeName);
        }

        /// <summary>
        /// Gets or sets the initial route name.
        /// </summary>
        public string InitialRoute { get; set; }

        /// <summary>
        /// Gets the current route name.
        /// </summary>
        public string? CurrentRoute => _currentRoute;

        /// <summary>
        /// Gets the route stack (bottom to top).
        /// </summary>
        public IReadOnlyCollection<string> RouteStack => _routeStack.Reverse().ToList();

        /// <summary>
        /// Gets or sets whether to maintain state of inactive routes.
        /// </summary>
        public bool MaintainState { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when the route changes.
        /// </summary>
        public Action<string>? OnRouteChanged { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when a route is popped.
        /// The callback receives the route name that was popped.
        /// </summary>
        public Action<string>? OnPop { get; set; }

        /// <summary>
        /// Gets or sets the restoration scope ID.
        /// </summary>
        public string? RestorationScopeId { get; set; }

        /// <summary>
        /// Gets or sets the clip behavior.
        /// </summary>
        public Clip ClipBehavior { get; set; }

        /// <summary>
        /// Gets whether the navigator can pop the current route.
        /// Returns true if there is more than one route on the stack.
        /// </summary>
        public bool CanPop => _routeStack.Count > 1;

        /// <summary>
        /// Pushes a new route onto the navigation stack.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <returns>True if the route was pushed, false if the route doesn't exist.</returns>
        public bool Push(string routeName)
        {
            if (!_routes.ContainsKey(routeName))
            {
                return false;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            OnRouteChanged?.Invoke(routeName);
            return true;
        }

        /// <summary>
        /// Pushes a new route and removes all previous routes until predicate returns true.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <param name="predicate">A function that returns true for routes to keep.</param>
        /// <returns>True if the operation succeeded.</returns>
        public bool PushAndRemoveUntil(string routeName, Func<string, bool> predicate)
        {
            if (!_routes.ContainsKey(routeName))
            {
                return false;
            }

            // Remove routes until predicate is true
            while (_routeStack.Count > 0 && !predicate(_routeStack.Peek()))
            {
                _routeStack.Pop();
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            OnRouteChanged?.Invoke(routeName);
            return true;
        }

        /// <summary>
        /// Replaces the current route with a new route.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <returns>True if the route was replaced, false if the route doesn't exist.</returns>
        public bool PushReplacement(string routeName)
        {
            if (!_routes.ContainsKey(routeName))
            {
                return false;
            }

            if (_routeStack.Count > 0)
            {
                _routeStack.Pop();
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            OnRouteChanged?.Invoke(routeName);
            return true;
        }

        /// <summary>
        /// Pops the current route from the navigation stack.
        /// </summary>
        /// <returns>True if a route was popped, false if the stack only has one route.</returns>
        public bool Pop()
        {
            if (_routeStack.Count <= 1)
            {
                return false;
            }

            var poppedRoute = _routeStack.Pop();
            _currentRoute = _routeStack.Peek();
            OnPop?.Invoke(poppedRoute);
            OnRouteChanged?.Invoke(_currentRoute);
            return true;
        }

        /// <summary>
        /// Pops routes until the predicate returns true.
        /// </summary>
        /// <param name="predicate">A function that returns true for the route to stop at.</param>
        public void PopUntil(Func<string, bool> predicate)
        {
            while (_routeStack.Count > 1 && !predicate(_routeStack.Peek()))
            {
                var poppedRoute = _routeStack.Pop();
                OnPop?.Invoke(poppedRoute);
            }

            _currentRoute = _routeStack.Peek();
            OnRouteChanged?.Invoke(_currentRoute);
        }

        /// <summary>
        /// Pops all routes and pushes the specified route (resets to a single route).
        /// </summary>
        /// <param name="routeName">The name of the route to navigate to.</param>
        /// <returns>True if successful, false if the route doesn't exist.</returns>
        public bool PopAllAndPush(string routeName)
        {
            if (!_routes.ContainsKey(routeName))
            {
                return false;
            }

            while (_routeStack.Count > 0)
            {
                var poppedRoute = _routeStack.Pop();
                OnPop?.Invoke(poppedRoute);
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            OnRouteChanged?.Invoke(routeName);
            return true;
        }

        /// <summary>
        /// Creates the backing struct for this widget.
        /// </summary>
        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var nav = new NavigatorStruct();

            nav.initialRoute = InitialRoute;
            nav.currentRoute = _currentRoute;
            nav.routeNames = string.Join("|", _routes.Keys);
            nav.maintainState = MaintainState ? (byte)1 : (byte)0;
            nav.restorationScopeId = RestorationScopeId;
            nav.clipBehavior = ClipBehavior;
            nav.navigatorId = Id;

            // Register callbacks using the base class helper
            nav.onRouteChangedAction = RegisterCallback(OnRouteChanged);
            nav.onPopAction = RegisterCallback(OnPop);

            // Build the current route's widget
            if (_currentRoute != null && _routes.TryGetValue(_currentRoute, out var builder))
            {
                _currentChildWidget = builder();
                nav.currentChild = (IntPtr)_currentChildWidget;
            }

            return nav;
        }
    }
}
