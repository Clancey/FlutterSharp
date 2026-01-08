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
    }
}
