using System;
using Newtonsoft.Json;

namespace Flutter {
	public class UpdateMessage : Message {
		[JsonProperty ("state")]
		public Widget State { get; set; }
		public override string MessageType => "UpdateComponent";
	}
}
