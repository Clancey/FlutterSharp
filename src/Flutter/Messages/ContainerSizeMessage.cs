using System.Text.Json.Serialization;

namespace Flutter
{
	/// <summary>
	/// Message sent to Flutter to notify about container size changes.
	/// This allows Flutter to adjust its rendering viewport accordingly.
	/// </summary>
	public class ContainerSizeMessage : Message
	{
		/// <summary>
		/// The new container width in logical pixels
		/// </summary>
		[JsonPropertyName("width")]
		public double Width { get; set; }

		/// <summary>
		/// The new container height in logical pixels
		/// </summary>
		[JsonPropertyName("height")]
		public double Height { get; set; }

		/// <summary>
		/// Gets the message type identifier
		/// </summary>
		public override string MessageType => "ContainerSize";
	}
}
