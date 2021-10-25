using System;

namespace Flutter.Internal {
	public class EventMessage : Message {
		public string EventName { get; set; }
		public object Data { get; set; }
		public bool NeedsReturn { get; set; }

		public override string MessageType => "eventMessage";
	}
}
