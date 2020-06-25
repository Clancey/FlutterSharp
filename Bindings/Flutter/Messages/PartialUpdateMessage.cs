using System;
using Newtonsoft.Json;

namespace Flutter {
	public class PartialUpdateMessage : Message {
		public override string MessageType => "partialUpdate";
	}
}
