using System.Text.Json.Serialization;

namespace Flutter
{
	/// <summary>
	/// Message sent when a widget is disposed on the C# side
	/// </summary>
	public class DisposedMessage : Message
	{
		[JsonPropertyName("widgetId")]
		public string WidgetId { get; set; }

		public override string MessageType => "DisposedComponent";
	}
}
