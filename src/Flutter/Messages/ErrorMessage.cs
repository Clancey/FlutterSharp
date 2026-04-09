using System;
using System.Text.Json.Serialization;

namespace Flutter
{
    /// <summary>
    /// Message sent to Dart to display an error in the error overlay.
    /// </summary>
    public class ErrorMessage : Message
    {
        /// <summary>
        /// The type of error (e.g., "CallbackError", "WidgetParseError", "CommunicationError").
        /// </summary>
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }

        /// <summary>
        /// The error message to display.
        /// </summary>
        [JsonPropertyName("message")]
        public string ErrorText { get; set; }

        /// <summary>
        /// Optional stack trace for debugging.
        /// </summary>
        [JsonPropertyName("stackTrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Timestamp when the error occurred (ISO 8601 format).
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Optional widget type that caused the error.
        /// </summary>
        [JsonPropertyName("widgetType")]
        public string WidgetType { get; set; }

        /// <summary>
        /// Optional callback ID that caused the error.
        /// </summary>
        [JsonPropertyName("callbackId")]
        public long? CallbackId { get; set; }

        /// <summary>
        /// Whether this error is recoverable (can be retried).
        /// </summary>
        [JsonPropertyName("isRecoverable")]
        public bool IsRecoverable { get; set; }

        public override string MessageType => "Error";

        public ErrorMessage()
        {
            Timestamp = DateTime.UtcNow.ToString("O");
        }
    }
}
