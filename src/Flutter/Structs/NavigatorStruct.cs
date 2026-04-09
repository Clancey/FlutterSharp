// Manual implementation for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Widgets;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for Navigator widget.
    /// Manages a stack of routes/pages for navigation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class NavigatorStruct : WidgetStruct
    {
        // Initial route name (e.g., "/", "/home", "/settings")
        private IntPtr _initialRoute;
        public string? initialRoute
        {
            get => GetString(_initialRoute);
            set
            {
                SetString(ref _initialRoute, value);
                HasinitialRoute = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasinitialRoute { get; set; }

        // Current route name (tracks which route is currently displayed)
        private IntPtr _currentRoute;
        public string? currentRoute
        {
            get => GetString(_currentRoute);
            set
            {
                SetString(ref _currentRoute, value);
                HascurrentRoute = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HascurrentRoute { get; set; }

        // Serialized route names (pipe-separated: "/|/home|/settings|/profile")
        private IntPtr _routeNames;
        public string? routeNames
        {
            get => GetString(_routeNames);
            set
            {
                SetString(ref _routeNames, value);
                HasrouteNames = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasrouteNames { get; set; }

        // Current child widget (the active route's widget)
        public IntPtr currentChild { get; set; }

        // Whether to maintain state of inactive routes (default true)
        public byte maintainState { get; set; }

        // Restoration scope ID for state restoration
        private IntPtr _restorationScopeId;
        public string? restorationScopeId
        {
            get => GetString(_restorationScopeId);
            set
            {
                SetString(ref _restorationScopeId, value);
                HasrestorationScopeId = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasrestorationScopeId { get; set; }

        // Clip behavior for the Navigator
        public Clip clipBehavior { get; set; }

        // Navigator ID for multiple navigators (e.g., nested navigation)
        private IntPtr _navigatorId;
        public string? navigatorId
        {
            get => GetString(_navigatorId);
            set
            {
                SetString(ref _navigatorId, value);
                HasnavigatorId = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasnavigatorId { get; set; }

        // Callback action IDs (for handling navigation events)
        private IntPtr _onRouteChangedAction;
        public string? onRouteChangedAction
        {
            get => GetString(_onRouteChangedAction);
            set
            {
                SetString(ref _onRouteChangedAction, value);
                HasonRouteChangedAction = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasonRouteChangedAction { get; set; }

        private IntPtr _onPopAction;
        public string? onPopAction
        {
            get => GetString(_onPopAction);
            set
            {
                SetString(ref _onPopAction, value);
                HasonPopAction = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasonPopAction { get; set; }

        /// <summary>
        /// Flag indicating whether OnGenerateRoute callback is set on the C# side.
        /// When true, the Navigator can resolve unknown routes dynamically.
        /// </summary>
        public byte hasOnGenerateRoute { get; set; }

        /// <summary>
        /// Flag indicating whether OnUnknownRoute callback is set on the C# side.
        /// When true, the Navigator can provide a fallback route for completely unknown routes.
        /// </summary>
        public byte hasOnUnknownRoute { get; set; }

        // Route transition fields (for MaterialPageRoute, CupertinoPageRoute, etc.)

        /// <summary>
        /// The type of transition animation to use.
        /// 0 = None, 1 = Material, 2 = Cupertino, 3 = Fade, 4 = SlideBottom, 5 = SlideRight, 6 = Zoom
        /// </summary>
        public int transitionType { get; set; }

        /// <summary>
        /// The duration of the push transition in milliseconds.
        /// </summary>
        public int transitionDurationMs { get; set; }

        /// <summary>
        /// The duration of the pop (reverse) transition in milliseconds.
        /// </summary>
        public int reverseTransitionDurationMs { get; set; }

        /// <summary>
        /// Whether this route is a full-screen dialog.
        /// </summary>
        public byte fullscreenDialog { get; set; }

        /// <summary>
        /// Whether this route is opaque (covers the entire navigator area).
        /// </summary>
        public byte opaque { get; set; }

        /// <summary>
        /// Whether a push transition is currently in progress.
        /// </summary>
        public byte isTransitioning { get; set; }

        /// <summary>
        /// Whether this is a pop operation (reverse transition).
        /// </summary>
        public byte isPopping { get; set; }

        // Previous route child widget pointer (for transition animations)
        public IntPtr previousChild { get; set; }

        // Route arguments (JSON-serialized)
        private IntPtr _routeArguments;
        public string? routeArguments
        {
            get => GetString(_routeArguments);
            set
            {
                SetString(ref _routeArguments, value);
                HasrouteArguments = value != null ? (byte)1 : (byte)0;
            }
        }
        public byte HasrouteArguments { get; set; }
    }
}
