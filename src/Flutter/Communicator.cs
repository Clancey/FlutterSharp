using System;
using System.Text.Json;

namespace Flutter.Internal
{
	/// <summary>
	/// Handles communication between C# and Flutter via platform channels
	/// </summary>
	public static class Communicator
	{
		/// <summary>
		/// Callback invoked when a command is received from Flutter
		/// </summary>
		public static Action<(string Method, string Arguments, Action<string> callback)> OnCommandReceived { get; set; }

		/// <summary>
		/// Delegate to send commands to Flutter. Must be set by the platform implementation.
		/// </summary>
		public static Action<(string Method, string Arguments)> SendCommand { get; set; }

		/// <summary>
		/// Sends a disposal notification to Flutter when a widget is disposed on C# side
		/// </summary>
		/// <param name="widgetId">The unique ID of the disposed widget</param>
		internal static void SendDisposed(string widgetId)
		{
			if (SendCommand == null)
			{
				Console.WriteLine($"Warning: Cannot send disposal for widget {widgetId} - SendCommand not configured");
				return;
			}

			try
			{
				var message = new DisposedMessage
				{
					WidgetId = widgetId,
					ComponentId = "0" // Default component
				};
				var json = JsonSerializer.Serialize(message);
				SendCommand.Invoke((message.MessageType, json));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending disposal for widget {widgetId}: {ex.Message}");
			}
		}

		/// <summary>
		/// Sends an event response back to Flutter
		/// </summary>
		internal static void SendEventResponse(string eventId, string response)
		{
			if (SendCommand == null)
			{
				Console.WriteLine($"Warning: Cannot send event response - SendCommand not configured");
				return;
			}

			try
			{
				SendCommand.Invoke(("EventResponse", response));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending event response: {ex.Message}");
			}
		}
	}
}
