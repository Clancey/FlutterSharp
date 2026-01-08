using System.Text.Json.Serialization;

namespace Flutter
{
	/// <summary>
	/// Message sent to Flutter to notify about app lifecycle state changes.
	/// This allows Flutter to pause animations, stop timers, etc. when the app is backgrounded.
	/// </summary>
	public class LifecycleMessage : Message
	{
		/// <summary>
		/// The lifecycle state: "resumed", "inactive", "paused", or "detached"
		/// </summary>
		[JsonPropertyName("state")]
		public string State { get; set; } = "resumed";

		/// <summary>
		/// Gets the message type identifier
		/// </summary>
		public override string MessageType => "Lifecycle";
	}
}
