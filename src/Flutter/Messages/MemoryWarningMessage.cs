using System.Text.Json.Serialization;

namespace Flutter
{
	/// <summary>
	/// Message sent to Flutter to notify about memory warnings from the system.
	/// This allows Flutter to release caches and reduce memory footprint.
	/// </summary>
	public class MemoryWarningMessage : Message
	{
		/// <summary>
		/// The severity level of the memory warning: "low", "medium", "high", or "critical"
		/// </summary>
		[JsonPropertyName("level")]
		public string Level { get; set; } = "medium";

		/// <summary>
		/// Optional timestamp of when the warning was received
		/// </summary>
		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }

		/// <summary>
		/// Gets the message type identifier
		/// </summary>
		public override string MessageType => "MemoryWarning";
	}
}
