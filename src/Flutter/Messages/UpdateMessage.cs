using System;
using System.Text.Json.Serialization;

namespace Flutter {
	public class UpdateMessage : Message {
		[JsonPropertyName("address")]
		public long Address { get; set; }
		public override string MessageType => "UpdateComponent";
	}
}
