using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace Flutter
{
    /// <summary>
    /// Message received from Dart when an exception occurs in the Flutter runtime.
    /// This enables C# code to be notified of and handle Dart-side errors.
    /// </summary>
    public class DartExceptionMessage
    {
        /// <summary>
        /// The category of the exception (e.g., "ParserError", "BuildError", "CallbackError", "RuntimeError").
        /// </summary>
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }

        /// <summary>
        /// The exception message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// The Dart stack trace where the exception occurred.
        /// </summary>
        [JsonPropertyName("stackTrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Timestamp when the exception occurred (ISO 8601 format).
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Optional widget type that was being processed when the exception occurred.
        /// </summary>
        [JsonPropertyName("widgetType")]
        public string WidgetType { get; set; }

        /// <summary>
        /// Optional method/function name where the exception occurred.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Optional additional context data as JSON.
        /// </summary>
        [JsonPropertyName("context")]
        public string Context { get; set; }

        /// <summary>
        /// Whether the exception was handled locally in Dart (error displayed to user).
        /// </summary>
        [JsonPropertyName("handledLocally")]
        public bool HandledLocally { get; set; }
    }

    /// <summary>
    /// Event args for Dart exception events, providing full exception details to C# handlers.
    /// </summary>
    public class DartExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The category of the exception.
        /// </summary>
        public string ErrorType { get; }

        /// <summary>
        /// The exception message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The Dart stack trace.
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// When the exception occurred.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Optional widget type context.
        /// </summary>
        public string WidgetType { get; }

        /// <summary>
        /// Optional source location context.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Whether the exception was already handled/displayed in Dart.
        /// </summary>
        public bool HandledLocally { get; }

        /// <summary>
        /// Set to true to suppress default logging of this exception.
        /// </summary>
        public bool Handled { get; set; }

        public DartExceptionEventArgs(DartExceptionMessage message)
        {
            ErrorType = message.ErrorType ?? "UnknownError";
            Message = message.Message ?? "Unknown exception";
            StackTrace = message.StackTrace;
            WidgetType = message.WidgetType;
            Source = message.Source;
            HandledLocally = message.HandledLocally;

            if (DateTime.TryParse(message.Timestamp, out var ts))
            {
                Timestamp = ts;
            }
            else
            {
                Timestamp = DateTime.UtcNow;
            }
        }

        public override string ToString()
        {
            var result = $"[Dart {ErrorType}] {Message}";
            if (!string.IsNullOrEmpty(Source))
                result += $" (in {Source})";
            if (!string.IsNullOrEmpty(WidgetType))
                result += $" [Widget: {WidgetType}]";
            return result;
        }
    }

    /// <summary>
    /// Tracks statistics about Dart exceptions received by FlutterManager.
    /// Thread-safe for concurrent access.
    /// </summary>
    public class DartExceptionStats
    {
        private readonly ConcurrentDictionary<string, int> _countsByType = new ConcurrentDictionary<string, int>();
        private int _totalCount;
        private DateTime? _firstException;
        private DateTime? _lastException;

        /// <summary>
        /// Total number of Dart exceptions received.
        /// </summary>
        public int TotalCount => _totalCount;

        /// <summary>
        /// When the first exception was received.
        /// </summary>
        public DateTime? FirstException => _firstException;

        /// <summary>
        /// When the most recent exception was received.
        /// </summary>
        public DateTime? LastException => _lastException;

        /// <summary>
        /// Gets exception counts grouped by error type.
        /// </summary>
        public IReadOnlyDictionary<string, int> CountsByType => _countsByType;

        /// <summary>
        /// Records a new exception occurrence.
        /// </summary>
        internal void RecordException(string errorType)
        {
            var now = DateTime.UtcNow;
            Interlocked.Increment(ref _totalCount);
            _countsByType.AddOrUpdate(errorType ?? "Unknown", 1, (_, count) => count + 1);
            _lastException = now;
            if (_firstException == null)
                _firstException = now;
        }

        /// <summary>
        /// Resets all statistics.
        /// </summary>
        public void Reset()
        {
            _countsByType.Clear();
            _totalCount = 0;
            _firstException = null;
            _lastException = null;
        }

        /// <summary>
        /// Generates a summary report of Dart exceptions.
        /// </summary>
        public string GenerateReport()
        {
            if (_totalCount == 0)
                return "No Dart exceptions recorded.";

            var lines = new List<string>
            {
                $"Dart Exception Statistics",
                $"========================",
                $"Total exceptions: {_totalCount}",
                $"First exception: {_firstException:O}",
                $"Last exception: {_lastException:O}",
                $"",
                $"By type:"
            };

            foreach (var kvp in _countsByType.OrderByDescending(x => x.Value))
            {
                lines.Add($"  {kvp.Key}: {kvp.Value}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
