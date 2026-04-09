using System;
using System.Text.Json.Serialization;

namespace Flutter
{
    /// <summary>
    /// Message sent from Dart to C# when a state value changes in the Dart UI.
    /// This enables two-way data binding between Dart widgets and C# state.
    /// </summary>
    public class StateNotifyMessage : Message
    {
        /// <summary>
        /// The unique identifier of the notifier being updated.
        /// Maps to a registered BidirectionalNotifier in C#.
        /// </summary>
        [JsonPropertyName("notifierId")]
        public string NotifierId { get; set; }

        /// <summary>
        /// The new value as a JSON-serializable object.
        /// Will be deserialized to the appropriate type based on the notifier's type parameter.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// The type name of the value for deserialization hints.
        /// e.g., "string", "int", "double", "bool"
        /// </summary>
        [JsonPropertyName("valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Optional timestamp when the change occurred on the Dart side.
        /// Used for conflict resolution in race conditions.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        /// <summary>
        /// The widget ID that initiated the state change, if known.
        /// Useful for debugging and preventing circular updates.
        /// </summary>
        [JsonPropertyName("sourceWidgetId")]
        public string SourceWidgetId { get; set; }

        [JsonPropertyName("messageType")]
        public override string MessageType => "StateNotify";
    }

    /// <summary>
    /// Message sent from C# to Dart when a BidirectionalNotifier's value changes.
    /// This notifies Dart widgets to update their local state.
    /// </summary>
    public class StateChangedMessage : Message
    {
        /// <summary>
        /// The unique identifier of the notifier that changed.
        /// </summary>
        [JsonPropertyName("notifierId")]
        public string NotifierId { get; set; }

        /// <summary>
        /// The new value as a JSON-serializable object.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// The type name of the value.
        /// </summary>
        [JsonPropertyName("valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Timestamp when the change occurred on the C# side.
        /// Used for conflict resolution.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("messageType")]
        public override string MessageType => "StateChanged";
    }
}
