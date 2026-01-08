using System.Text.Json.Serialization;

namespace Flutter
{
    /// <summary>
    /// Message sent to Dart to control scroll position.
    /// </summary>
    public class ScrollCommandMessage
    {
        /// <summary>
        /// The ID of the ScrollController to control.
        /// </summary>
        [JsonPropertyName("controllerId")]
        public string ControllerId { get; set; }

        /// <summary>
        /// The command to execute (jumpTo, animateTo).
        /// </summary>
        [JsonPropertyName("command")]
        public string Command { get; set; }

        /// <summary>
        /// The target scroll offset.
        /// </summary>
        [JsonPropertyName("offset")]
        public double Offset { get; set; }

        /// <summary>
        /// The duration of the animation in milliseconds (for animateTo).
        /// </summary>
        [JsonPropertyName("durationMs")]
        public double? DurationMs { get; set; }

        /// <summary>
        /// The animation curve name (for animateTo).
        /// </summary>
        [JsonPropertyName("curve")]
        public string Curve { get; set; }
    }
}
