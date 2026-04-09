using System;
using System.Text.Json.Serialization;

namespace Flutter.Messages
{
    /// <summary>
    /// Message sent to Dart when a hot reload occurs.
    /// Displays a visual notification in the Flutter UI.
    /// </summary>
    public class HotReloadNotificationMessage : Flutter.Message
    {
        /// <summary>
        /// Gets the message type identifier for the method channel.
        /// </summary>
        public override string MessageType => "HotReload";

        /// <summary>
        /// UTC timestamp when the hot reload occurred.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Optional name of the widget type that was reloaded.
        /// </summary>
        [JsonPropertyName("widgetType")]
        public string WidgetType { get; set; }

        /// <summary>
        /// Number of widgets reloaded.
        /// </summary>
        [JsonPropertyName("widgetsReloaded")]
        public int WidgetsReloaded { get; set; } = 1;

        /// <summary>
        /// Whether the hot reload was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the reload failed.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the hot reload operation in milliseconds.
        /// </summary>
        [JsonPropertyName("durationMs")]
        public int? DurationMs { get; set; }

        /// <summary>
        /// Creates a new hot reload notification message with the current timestamp.
        /// </summary>
        public HotReloadNotificationMessage()
        {
            Timestamp = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Creates a success notification for a hot reload.
        /// </summary>
        /// <param name="widgetType">Optional widget type that was reloaded.</param>
        /// <param name="durationMs">Optional duration in milliseconds.</param>
        /// <returns>A new success notification message.</returns>
        public static HotReloadNotificationMessage CreateSuccess(string widgetType = null, int? durationMs = null)
        {
            return new HotReloadNotificationMessage
            {
                WidgetType = widgetType,
                Success = true,
                DurationMs = durationMs
            };
        }

        /// <summary>
        /// Creates a failure notification for a hot reload.
        /// </summary>
        /// <param name="errorMessage">The error message describing what went wrong.</param>
        /// <param name="widgetType">Optional widget type that failed to reload.</param>
        /// <returns>A new failure notification message.</returns>
        public static HotReloadNotificationMessage CreateFailure(string errorMessage, string widgetType = null)
        {
            return new HotReloadNotificationMessage
            {
                WidgetType = widgetType,
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
