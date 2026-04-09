using System;
using System.Runtime.InteropServices;
using Flutter.Widgets;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for the ErrorBoundary widget.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class ErrorBoundaryStruct : SingleChildRenderObjectWidgetStruct
    {
        /// <summary>
        /// Whether to show errors in the global error overlay.
        /// </summary>
        public byte showInOverlay { get; set; }

        /// <summary>
        /// Whether to report errors to C# for logging.
        /// </summary>
        public byte reportToNative { get; set; }

        // Has flag for nullable property: widgetTypeName
        public byte HaswidgetTypeName { get; set; }

        // String field: widgetTypeName
        private IntPtr _widgetTypeName;

        /// <summary>
        /// Optional widget type name for error context.
        /// </summary>
        public string? widgetTypeName
        {
            get => GetString(_widgetTypeName);
            set { SetString(ref _widgetTypeName, value); HaswidgetTypeName = (byte)(value != null ? 1 : 0); }
        }

        // Has flag for nullable callback: onError
        public byte HasonError { get; set; }

        // Callback field: onError
        // Using action string pattern - Dart will dispatch action to C# via method channel
        IntPtr _onError;

        /// <summary>
        /// Action identifier for onError callback.
        /// Called when an error is caught in the child widget subtree.
        /// Set to a string identifier (e.g., "action_123") that C# will recognize.
        /// </summary>
        public string? onErrorAction
        {
            get => GetString(_onError);
            set { SetString(ref _onError, value); HasonError = (byte)(value != null ? 1 : 0); }
        }
    }
}
