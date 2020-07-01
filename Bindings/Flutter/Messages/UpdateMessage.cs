using System;
using Newtonsoft.Json;

namespace Flutter {
	public class UpdateMessage : Message {
		[JsonProperty ("address")]
		public int Address { get; set; }
		public override string MessageType => "UpdateComponent";
	}
}
