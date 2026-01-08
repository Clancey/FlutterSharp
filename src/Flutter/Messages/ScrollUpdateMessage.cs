using System.Text.Json.Serialization;

namespace Flutter
{
    /// <summary>
    /// Message received from Dart when scroll position changes.
    /// </summary>
    public class ScrollUpdateMessage
    {
        /// <summary>
        /// The ID of the ScrollController that owns this position.
        /// </summary>
        [JsonPropertyName("controllerId")]
        public string ControllerId { get; set; }

        /// <summary>
        /// The type of scroll event (scrollStart, scrollUpdate, scrollEnd, attach, detach).
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// The current scroll offset.
        /// </summary>
        [JsonPropertyName("offset")]
        public double Offset { get; set; }

        /// <summary>
        /// The change in scroll offset since the last update.
        /// </summary>
        [JsonPropertyName("delta")]
        public double? Delta { get; set; }

        /// <summary>
        /// The velocity at the end of a scroll (pixels per second).
        /// </summary>
        [JsonPropertyName("velocity")]
        public double? Velocity { get; set; }

        /// <summary>
        /// The maximum scroll extent.
        /// </summary>
        [JsonPropertyName("maxScrollExtent")]
        public double? MaxScrollExtent { get; set; }

        /// <summary>
        /// The minimum scroll extent.
        /// </summary>
        [JsonPropertyName("minScrollExtent")]
        public double? MinScrollExtent { get; set; }

        /// <summary>
        /// The dimension of the viewport.
        /// </summary>
        [JsonPropertyName("viewportDimension")]
        public double? ViewportDimension { get; set; }

        /// <summary>
        /// Whether the controller has any attached scroll positions.
        /// </summary>
        [JsonPropertyName("hasClients")]
        public bool? HasClients { get; set; }

        /// <summary>
        /// The axis direction (0=up, 1=right, 2=down, 3=left).
        /// </summary>
        [JsonPropertyName("axisDirection")]
        public int? AxisDirection { get; set; }
    }
}
