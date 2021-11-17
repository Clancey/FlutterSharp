using System;
using System.Text.Json.Serialization;

namespace Flutter.Internal {
	public class EventMessage : Message {

		[JsonPropertyName("eventName")]
		public string EventName { get; set; }
		[JsonPropertyName("data")]
		public object Data { get; set; }
		[JsonPropertyName("needsReturn")]
		public bool NeedsReturn { get; set; }

		public override string MessageType => "eventMessage";
	}
}
