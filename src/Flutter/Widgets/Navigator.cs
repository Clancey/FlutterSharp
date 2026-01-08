// Manual implementation for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Flutter;
using Flutter.Navigation;
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
    /// // Basic named routes (no arguments)
    /// var nav = new Navigator
    /// {
    ///     InitialRoute = "/",
    ///     Routes = new Dictionary&lt;string, Func&lt;Widget&gt;&gt;
    ///     {
    ///         { "/", () => new HomePage() },
    ///         { "/settings", () => new SettingsPage() }
    ///     }
    /// };
    ///
    /// // Named routes with arguments
    /// var navWithArgs = new Navigator
    /// {
    ///     InitialRoute = "/",
    ///     RoutesWithArguments = new Dictionary&lt;string, Func&lt;object?, Widget&gt;&gt;
    ///     {
    ///         { "/", (args) => new HomePage() },
    ///         { "/details", (args) => new DetailsPage((args as DetailsArgs)?.Id) },
    ///         { "/profile", (args) => new ProfilePage { UserId = args as string } }
    ///     }
    /// };
    ///
    /// // Push with arguments
    /// navWithArgs.PushNamed("/details", new DetailsArgs { Id = 42 });
    ///
    /// // Pop the current route
    /// nav.Pop();
    /// </code>
    /// </example>
    public class Navigator : StatefulWidget
    {
        private readonly Dictionary<string, Func<Widget>> _routes = new();
        private readonly Dictionary<string, Func<object?, Widget>> _routesWithArgs = new();
        private readonly Stack<string> _routeStack = new();
        private readonly Stack<Route> _routeObjectStack = new();
        private readonly Dictionary<int, object?> _routeArguments = new(); // Map stack index to arguments
        private string? _currentRoute;
        private object? _currentArguments;
        private Widget? _currentChildWidget;
        private Widget? _previousChildWidget;
        private Route? _currentRouteObject;
        private bool _isTransitioning;
        private bool _isPopping;

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
        /// The dictionary of named routes (without arguments).
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
        /// The dictionary of named routes that receive arguments.
        /// Keys are route names (e.g., "/", "/home", "/details").
        /// Values are functions that receive arguments and return the Widget to display.
        /// </summary>
        /// <example>
        /// <code>
        /// RoutesWithArguments = new Dictionary&lt;string, Func&lt;object?, Widget&gt;&gt;
        /// {
        ///     { "/details", (args) => new DetailsPage(args as DetailsArgs) },
        ///     { "/profile", (args) => new ProfilePage { UserId = (int)args! } }
        /// }
        /// </code>
        /// </example>
        public Dictionary<string, Func<object?, Widget>> RoutesWithArguments
        {
            get => _routesWithArgs;
            init
            {
                _routesWithArgs.Clear();
                foreach (var kvp in value)
                {
                    _routesWithArgs[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Gets the arguments for the current route (if any).
        /// </summary>
        public object? CurrentArguments => _currentArguments;

        /// <summary>
        /// Adds a route to the Navigator (without arguments).
        /// </summary>
        /// <param name="routeName">The name of the route (e.g., "/settings").</param>
        /// <param name="builder">A function that builds the widget for this route.</param>
        public void AddRoute(string routeName, Func<Widget> builder)
        {
            _routes[routeName] = builder;
        }

        /// <summary>
        /// Adds a route to the Navigator that receives arguments.
        /// </summary>
        /// <param name="routeName">The name of the route (e.g., "/details").</param>
        /// <param name="builder">A function that receives arguments and builds the widget.</param>
        public void AddRoute(string routeName, Func<object?, Widget> builder)
        {
            _routesWithArgs[routeName] = builder;
        }

        /// <summary>
        /// Removes a route from the Navigator.
        /// </summary>
        /// <param name="routeName">The name of the route to remove.</param>
        /// <returns>True if the route was removed, false if it wasn't found.</returns>
        public bool RemoveRoute(string routeName)
        {
            return _routes.Remove(routeName) || _routesWithArgs.Remove(routeName);
        }

        /// <summary>
        /// Checks if a route exists (with or without arguments).
        /// </summary>
        private bool HasRoute(string routeName) =>
            _routes.ContainsKey(routeName) || _routesWithArgs.ContainsKey(routeName);

        /// <summary>
        /// Gets all registered route names.
        /// </summary>
        public IReadOnlyCollection<string> RouteNames =>
            _routes.Keys.Concat(_routesWithArgs.Keys).Distinct().ToList();

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
            return PushNamed(routeName, null);
        }

        /// <summary>
        /// Pushes a named route onto the navigation stack with optional arguments.
        /// This is the standard Flutter-style method for named route navigation.
        /// </summary>
        /// <param name="routeName">The name of the route to push (e.g., "/details").</param>
        /// <param name="arguments">Optional arguments to pass to the route builder.</param>
        /// <returns>True if the route was pushed, false if the route doesn't exist.</returns>
        /// <example>
        /// <code>
        /// // Push without arguments
        /// navigator.PushNamed("/settings");
        ///
        /// // Push with arguments
        /// navigator.PushNamed("/details", new { Id = 42, Title = "Item Details" });
        ///
        /// // Push with typed arguments
        /// navigator.PushNamed("/profile", new ProfileArgs { UserId = userId });
        /// </code>
        /// </example>
        public bool PushNamed(string routeName, object? arguments = null)
        {
            if (!HasRoute(routeName))
            {
                return false;
            }

            // Store arguments for this route (by stack index)
            var stackIndex = _routeStack.Count;
            if (arguments != null)
            {
                _routeArguments[stackIndex] = arguments;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            _currentArguments = arguments;
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });
            return true;
        }

        /// <summary>
        /// Pushes a Route object onto the navigation stack.
        /// This method supports MaterialPageRoute, CupertinoPageRoute, and other Route types.
        /// </summary>
        /// <param name="route">The Route to push.</param>
        /// <returns>True if the route was pushed successfully.</returns>
        public bool Push(Route route)
        {
            if (route == null)
            {
                return false;
            }

            // Store previous widget for transition
            _previousChildWidget = _currentChildWidget;
            _isTransitioning = true;
            _isPopping = false;

            // Set up the route
            route.Navigator = this;
            route.IsActive = true;
            route.IsCurrent = true;
            route.IsFirst = _routeObjectStack.Count == 0;

            // Mark previous route as no longer current
            if (_currentRouteObject != null)
            {
                _currentRouteObject.IsCurrent = false;
                _currentRouteObject.DidPushNext();
            }

            // Push the route
            _routeObjectStack.Push(route);
            _currentRouteObject = route;

            // Build the widget
            _currentChildWidget = route.BuildPage(null);

            // Update route name tracking
            var routeName = route.Settings.Name ?? $"/route_{_routeObjectStack.Count}";
            _routeStack.Push(routeName);
            _currentRoute = routeName;

            // Notify about route change
            route.DidPush();
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });

            return true;
        }

        /// <summary>
        /// Pushes a new MaterialPageRoute with the given builder.
        /// </summary>
        /// <param name="builder">A function that creates the widget content.</param>
        /// <param name="routeName">Optional route name.</param>
        /// <param name="arguments">Optional route arguments.</param>
        /// <returns>True if pushed successfully.</returns>
        public bool PushMaterial(Func<Widget> builder, string? routeName = null, object? arguments = null)
        {
            return Push(new MaterialPageRoute(
                builder: builder,
                settings: new RouteSettings(routeName, arguments)
            ));
        }

        /// <summary>
        /// Pushes a new CupertinoPageRoute with the given builder.
        /// </summary>
        /// <param name="builder">A function that creates the widget content.</param>
        /// <param name="routeName">Optional route name.</param>
        /// <param name="arguments">Optional route arguments.</param>
        /// <returns>True if pushed successfully.</returns>
        public bool PushCupertino(Func<Widget> builder, string? routeName = null, object? arguments = null)
        {
            return Push(new CupertinoPageRoute(
                builder: builder,
                settings: new RouteSettings(routeName, arguments)
            ));
        }

        /// <summary>
        /// Pushes a new route and removes all previous routes until predicate returns true.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <param name="predicate">A function that returns true for routes to keep.</param>
        /// <param name="arguments">Optional arguments to pass to the new route.</param>
        /// <returns>True if the operation succeeded.</returns>
        public bool PushAndRemoveUntil(string routeName, Func<string, bool> predicate, object? arguments = null)
        {
            if (!HasRoute(routeName))
            {
                return false;
            }

            // Remove routes until predicate is true
            while (_routeStack.Count > 0 && !predicate(_routeStack.Peek()))
            {
                var poppedIndex = _routeStack.Count - 1;
                _routeArguments.Remove(poppedIndex);
                _routeStack.Pop();
            }

            // Push new route with arguments
            var stackIndex = _routeStack.Count;
            if (arguments != null)
            {
                _routeArguments[stackIndex] = arguments;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            _currentArguments = arguments;
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });
            return true;
        }

        /// <summary>
        /// Replaces the current route with a new route.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <returns>True if the route was replaced, false if the route doesn't exist.</returns>
        public bool PushReplacement(string routeName)
        {
            return PushReplacementNamed(routeName, null);
        }

        /// <summary>
        /// Replaces the current route with a new named route, with optional arguments.
        /// </summary>
        /// <param name="routeName">The name of the route to push.</param>
        /// <param name="arguments">Optional arguments to pass to the route builder.</param>
        /// <returns>True if the route was replaced, false if the route doesn't exist.</returns>
        public bool PushReplacementNamed(string routeName, object? arguments = null)
        {
            if (!HasRoute(routeName))
            {
                return false;
            }

            // Remove arguments for the current route
            if (_routeStack.Count > 0)
            {
                var oldIndex = _routeStack.Count - 1;
                _routeArguments.Remove(oldIndex);
                _routeStack.Pop();
            }

            // Push new route with arguments
            var stackIndex = _routeStack.Count;
            if (arguments != null)
            {
                _routeArguments[stackIndex] = arguments;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            _currentArguments = arguments;
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });
            return true;
        }

        /// <summary>
        /// Pops the current route and immediately pushes a new named route.
        /// Equivalent to calling Pop() followed by PushNamed().
        /// </summary>
        /// <param name="routeName">The name of the route to push after popping.</param>
        /// <param name="arguments">Optional arguments to pass to the new route.</param>
        /// <param name="result">Optional result to pass to the previous route.</param>
        /// <returns>True if successful, false if the route doesn't exist or cannot pop.</returns>
        public bool PopAndPushNamed(string routeName, object? arguments = null, object? result = null)
        {
            if (!HasRoute(routeName))
            {
                return false;
            }

            if (_routeStack.Count <= 1)
            {
                // Can't pop, but can still push replacement
                return PushReplacementNamed(routeName, arguments);
            }

            // Pop the current route
            var poppedIndex = _routeStack.Count - 1;
            _routeArguments.Remove(poppedIndex);
            var poppedRouteName = _routeStack.Pop();
            OnPop?.Invoke(poppedRouteName);

            // Push new route with arguments
            var stackIndex = _routeStack.Count;
            if (arguments != null)
            {
                _routeArguments[stackIndex] = arguments;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            _currentArguments = arguments;
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });
            return true;
        }

        /// <summary>
        /// Pops the current route from the navigation stack.
        /// </summary>
        /// <param name="result">Optional result to return to the previous route.</param>
        /// <returns>True if a route was popped, false if the stack only has one route.</returns>
        public bool Pop(object? result = null)
        {
            if (_routeStack.Count <= 1)
            {
                return false;
            }

            // Store current widget for transition
            _previousChildWidget = _currentChildWidget;
            _isTransitioning = true;
            _isPopping = true;

            // Remove arguments for the popped route
            var poppedIndex = _routeStack.Count - 1;
            _routeArguments.Remove(poppedIndex);

            var poppedRouteName = _routeStack.Pop();
            _currentRoute = _routeStack.Peek();

            // Restore arguments for the current route
            var currentIndex = _routeStack.Count - 1;
            _currentArguments = _routeArguments.TryGetValue(currentIndex, out var args) ? args : null;

            // Handle Route objects if present
            if (_routeObjectStack.Count > 0)
            {
                var poppedRouteObject = _routeObjectStack.Pop();
                poppedRouteObject.DidPop(result);
                poppedRouteObject.IsActive = false;
                poppedRouteObject.IsCurrent = false;

                if (_routeObjectStack.Count > 0)
                {
                    _currentRouteObject = _routeObjectStack.Peek();
                    _currentRouteObject.IsCurrent = true;
                    _currentRouteObject.DidPopNext();
                    _currentChildWidget = _currentRouteObject.BuildPage(null);
                }
                else
                {
                    _currentRouteObject = null;
                    // Fall back to named routes
                    _currentChildWidget = BuildWidgetForRoute(_currentRoute, _currentArguments);
                }
            }
            else
            {
                // Named routes only
                _currentChildWidget = BuildWidgetForRoute(_currentRoute, _currentArguments);
            }

            OnPop?.Invoke(poppedRouteName);
            OnRouteChanged?.Invoke(_currentRoute);
            SetState(() => { });
            return true;
        }

        /// <summary>
        /// Builds the widget for a named route, with or without arguments.
        /// </summary>
        private Widget? BuildWidgetForRoute(string routeName, object? arguments)
        {
            // First check for argument-receiving routes
            if (_routesWithArgs.TryGetValue(routeName, out var builderWithArgs))
            {
                return builderWithArgs(arguments);
            }

            // Fall back to no-args routes
            if (_routes.TryGetValue(routeName, out var builder))
            {
                return builder();
            }

            return null;
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
        /// <param name="arguments">Optional arguments to pass to the new route.</param>
        /// <returns>True if successful, false if the route doesn't exist.</returns>
        public bool PopAllAndPush(string routeName, object? arguments = null)
        {
            if (!HasRoute(routeName))
            {
                return false;
            }

            // Pop all routes and clear arguments
            while (_routeStack.Count > 0)
            {
                var poppedIndex = _routeStack.Count - 1;
                _routeArguments.Remove(poppedIndex);
                var poppedRoute = _routeStack.Pop();
                OnPop?.Invoke(poppedRoute);
            }

            // Push new route with arguments
            if (arguments != null)
            {
                _routeArguments[0] = arguments;
            }

            _routeStack.Push(routeName);
            _currentRoute = routeName;
            _currentArguments = arguments;
            OnRouteChanged?.Invoke(routeName);
            SetState(() => { });
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
            // Include all route names from both dictionaries
            nav.routeNames = string.Join("|", RouteNames);
            nav.maintainState = MaintainState ? (byte)1 : (byte)0;
            nav.restorationScopeId = RestorationScopeId;
            nav.clipBehavior = ClipBehavior;
            nav.navigatorId = Id;

            // Register callbacks using the base class helper
            nav.onRouteChangedAction = RegisterCallback(OnRouteChanged);
            nav.onPopAction = RegisterCallback(OnPop);

            // Set transition information from current route object
            if (_currentRouteObject != null)
            {
                nav.transitionType = (int)_currentRouteObject.TransitionType;
                nav.transitionDurationMs = _currentRouteObject.TransitionDurationMs;
                nav.reverseTransitionDurationMs = _currentRouteObject.ReverseTransitionDurationMs;
                nav.fullscreenDialog = _currentRouteObject.FullscreenDialog ? (byte)1 : (byte)0;
                nav.opaque = _currentRouteObject.Opaque ? (byte)1 : (byte)0;

                // Serialize route arguments if present
                if (_currentRouteObject.Settings.Arguments != null)
                {
                    try
                    {
                        nav.routeArguments = JsonSerializer.Serialize(_currentRouteObject.Settings.Arguments);
                    }
                    catch
                    {
                        // If serialization fails, ignore arguments
                    }
                }

                // Build the current widget from the route
                _currentChildWidget = _currentRouteObject.BuildPage(null);
            }
            else if (_currentRoute != null)
            {
                // Use named route builder (with or without arguments)
                _currentChildWidget = BuildWidgetForRoute(_currentRoute, _currentArguments);
                nav.transitionType = 0; // No transition for named routes

                // Serialize current arguments for named routes
                if (_currentArguments != null)
                {
                    try
                    {
                        nav.routeArguments = JsonSerializer.Serialize(_currentArguments);
                    }
                    catch
                    {
                        // If serialization fails, ignore arguments
                    }
                }
            }

            // Set transition state
            nav.isTransitioning = _isTransitioning ? (byte)1 : (byte)0;
            nav.isPopping = _isPopping ? (byte)1 : (byte)0;

            // Set current child widget pointer
            if (_currentChildWidget != null)
            {
                nav.currentChild = (IntPtr)_currentChildWidget;
            }

            // Set previous child for transition animation
            if (_previousChildWidget != null && _isTransitioning)
            {
                nav.previousChild = (IntPtr)_previousChildWidget;
            }

            // Clear transition state after sending
            _isTransitioning = false;

            return nav;
        }

        /// <summary>
        /// Marks a transition as complete (called by Dart after animation finishes).
        /// </summary>
        internal void OnTransitionComplete()
        {
            _previousChildWidget = null;
            _isPopping = false;
        }
    }
}
