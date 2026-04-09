using System;
using Flutter.Structs;

namespace Flutter.Widgets
{
    /// <summary>
    /// A widget that catches errors in its child subtree and displays a fallback UI.
    ///
    /// Unlike ErrorOverlay which shows toast-like notifications at the top of the screen,
    /// ErrorBoundary replaces its child with a fallback widget when an error occurs.
    ///
    /// Example:
    /// <code>
    /// new ErrorBoundary(
    ///     child: myWidget,
    ///     onError: (error) => Console.WriteLine($"Error: {error}"),
    ///     showInOverlay: true
    /// )
    /// </code>
    /// </summary>
    public class ErrorBoundary : StatelessWidget
    {
        private Widget? _child;
        private Action<ErrorBoundaryEventArgs>? _onError;
        private bool _showInOverlay;
        private bool _reportToNative;
        private string? _widgetTypeName;

        /// <summary>
        /// Creates a new ErrorBoundary widget.
        /// </summary>
        /// <param name="child">The widget to wrap with error handling.</param>
        /// <param name="onError">Called when an error is caught.</param>
        /// <param name="showInOverlay">Whether to also show the error in the global ErrorOverlay.</param>
        /// <param name="reportToNative">Whether to report errors to C# for logging.</param>
        /// <param name="widgetTypeName">Optional widget type name for error reporting context.</param>
        public ErrorBoundary(
            Widget? child = null,
            Action<ErrorBoundaryEventArgs>? onError = null,
            bool showInOverlay = true,
            bool reportToNative = true,
            string? widgetTypeName = null)
        {
            _child = child;
            _onError = onError;
            _showInOverlay = showInOverlay;
            _reportToNative = reportToNative;
            _widgetTypeName = widgetTypeName;

            var s = GetBackingStruct<ErrorBoundaryStruct>();
            s.child = child;
            s.showInOverlay = (byte)(showInOverlay ? 1 : 0);
            s.reportToNative = (byte)(reportToNative ? 1 : 0);
            s.widgetTypeName = widgetTypeName;

            // Register callback if provided
            if (onError != null)
            {
                Action<string> errorHandler = errorJson =>
                {
                    try
                    {
                        var args = ErrorBoundaryEventArgs.FromJson(errorJson);
                        onError(args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ErrorBoundary] Failed to parse error: {ex.Message}");
                    }
                };
                // RegisterCallback handles the CallbackRegistry.Register and tracking
                // Returns an action ID string like "action_123"
                s.onErrorAction = RegisterCallback(errorHandler);
            }
        }

        /// <summary>
        /// The child widget wrapped by this error boundary.
        /// </summary>
        public Widget? Child
        {
            get => _child;
            set
            {
                _child = value;
                GetBackingStruct<ErrorBoundaryStruct>().child = value;
            }
        }

        /// <summary>
        /// Whether to show errors in the global ErrorOverlay.
        /// </summary>
        public bool ShowInOverlay
        {
            get => _showInOverlay;
            set
            {
                _showInOverlay = value;
                GetBackingStruct<ErrorBoundaryStruct>().showInOverlay = (byte)(value ? 1 : 0);
            }
        }

        /// <summary>
        /// Whether to report errors to C# for logging.
        /// </summary>
        public bool ReportToNative
        {
            get => _reportToNative;
            set
            {
                _reportToNative = value;
                GetBackingStruct<ErrorBoundaryStruct>().reportToNative = (byte)(value ? 1 : 0);
            }
        }

        /// <summary>
        /// Optional widget type name for error reporting context.
        /// </summary>
        public string? WidgetTypeName
        {
            get => _widgetTypeName;
            set
            {
                _widgetTypeName = value;
                GetBackingStruct<ErrorBoundaryStruct>().widgetTypeName = value;
            }
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new ErrorBoundaryStruct();
    }

    /// <summary>
    /// Event arguments for ErrorBoundary error events.
    /// </summary>
    public class ErrorBoundaryEventArgs : EventArgs
    {
        /// <summary>
        /// The type of error (e.g., "WidgetError", "CallbackError").
        /// </summary>
        public string ErrorType { get; set; } = "Error";

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// The stack trace, if available.
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// The widget type that caused the error, if known.
        /// </summary>
        public string? WidgetType { get; set; }

        /// <summary>
        /// Whether the error is recoverable (retry is possible).
        /// </summary>
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// Creates ErrorBoundaryEventArgs from a JSON string.
        /// </summary>
        public static ErrorBoundaryEventArgs FromJson(string json)
        {
            var args = new ErrorBoundaryEventArgs();

            // Simple JSON parsing without external dependencies
            if (string.IsNullOrEmpty(json))
                return args;

            try
            {
                // Parse JSON manually for basic fields
                if (json.Contains("\"errorType\""))
                    args.ErrorType = ExtractJsonValue(json, "errorType") ?? "Error";
                if (json.Contains("\"message\""))
                    args.Message = ExtractJsonValue(json, "message") ?? "";
                if (json.Contains("\"stackTrace\""))
                    args.StackTrace = ExtractJsonValue(json, "stackTrace");
                if (json.Contains("\"widgetType\""))
                    args.WidgetType = ExtractJsonValue(json, "widgetType");
                if (json.Contains("\"isRecoverable\""))
                    args.IsRecoverable = json.Contains("\"isRecoverable\":true") || json.Contains("\"isRecoverable\": true");
            }
            catch
            {
                // If parsing fails, just use defaults
            }

            return args;
        }

        private static string? ExtractJsonValue(string json, string key)
        {
            var pattern = $"\"{key}\":";
            var startIndex = json.IndexOf(pattern);
            if (startIndex < 0) return null;

            startIndex += pattern.Length;

            // Skip whitespace
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
                startIndex++;

            if (startIndex >= json.Length) return null;

            // Check if value is a string (starts with quote)
            if (json[startIndex] == '"')
            {
                startIndex++;
                var endIndex = json.IndexOf('"', startIndex);
                if (endIndex < 0) return null;
                return json.Substring(startIndex, endIndex - startIndex);
            }

            // Otherwise, read until comma or closing brace
            var end = startIndex;
            while (end < json.Length && json[end] != ',' && json[end] != '}')
                end++;

            return json.Substring(startIndex, end - startIndex).Trim();
        }

        public override string ToString() => $"[{ErrorType}] {Message}";
    }
}
