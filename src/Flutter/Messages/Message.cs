using System;
using System.Text.Json.Serialization;

namespace Flutter {
	public abstract class Message {
		[JsonPropertyName ("componentId")]
		public string ComponentId { get; set; }
		[JsonPropertyName("messageType")]
		public abstract string MessageType { get; }
	}
}
