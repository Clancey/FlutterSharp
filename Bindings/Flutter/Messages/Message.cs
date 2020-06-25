using System;
using Newtonsoft.Json;

namespace Flutter {
	public abstract class Message {
		[JsonProperty ("componentId")]
		public string ComponentId { get; set; }
		[JsonProperty("messageType")]
		public abstract string MessageType { get; }
	}
}
