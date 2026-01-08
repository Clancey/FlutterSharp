using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Flutter.Internal
{
	/// <summary>
	/// Manages the communication between C# widgets and the Flutter runtime.
	/// Thread-safe singleton that handles widget tracking, state updates, and event routing.
	/// </summary>
	public static class FlutterManager
	{
		private static readonly object _lock = new object();
		private static WeakDictionary<string, Widget> AliveWidgets = new WeakDictionary<string, Widget>();
		private static Dictionary<string, Action<string, string, Action<string>>> EventHandlers = new Dictionary<string, Action<string, string, Action<string>>>();
		private static bool _isInitialized = false;
		private static bool _isReady = false;

		internal static readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		/// <summary>
		/// Gets whether Flutter is ready to receive messages
		/// </summary>
		public static bool IsReady => _isReady;

		/// <summary>
		/// Gets whether FlutterManager has been initialized
		/// </summary>
		public static bool IsInitialized => _isInitialized;

		/// <summary>
		/// Initializes the FlutterManager. Must be called before using any other methods.
		/// Safe to call multiple times - subsequent calls are ignored.
		/// </summary>
		public static void Initialize()
		{
			if (_isInitialized)
				return;

			lock (_lock)
			{
				if (_isInitialized)
					return;

				Communicator.OnCommandReceived = OnCommandReceived;
				_isInitialized = true;
			}
		}

		// Static constructor for backwards compatibility - auto-initializes
		static FlutterManager()
		{
			Initialize();
		}

		private static void OnCommandReceived((string Method, string Data, Action<string> Callback) message)
		{
			switch (message.Method)
			{
				case "Ready":
				case "ready":
					_isReady = true;
					OnReady?.Invoke();
					return;

				case "Event":
					HandleEvent(message.Data, message.Callback);
					return;

				case "HandleAction":
					HandleAction(message.Data, message.Callback);
					return;

				default:
					Console.WriteLine($"FlutterManager: Unknown method '{message.Method}'");
					return;
			}
		}

		private static void HandleEvent(string data, Action<string> callback)
		{
			try
			{
				Console.WriteLine($"FlutterManager: Received event: {data}");
				var msg = JsonSerializer.Deserialize<EventMessage>(data, serializeOptions);

				if (msg == null)
				{
					Console.WriteLine("FlutterManager: Failed to deserialize event message");
					return;
				}

				// First, try widget-specific event handler
				Widget widget = null;
				lock (_lock)
				{
					AliveWidgets.TryGetValue(msg.ComponentId, out widget);
				}

				if (widget != null)
				{
					var dataStr = msg.Data is string s ? s : JsonSerializer.Serialize(msg.Data, serializeOptions);
					widget.SendEvent(msg.EventName, dataStr, callback);
					return;
				}

				// Then try registered event handlers
				Action<string, string, Action<string>> handler = null;
				lock (_lock)
				{
					EventHandlers.TryGetValue(msg.EventName, out handler);
				}

				if (handler != null)
				{
					var dataStr = msg.Data is string s ? s : JsonSerializer.Serialize(msg.Data, serializeOptions);
					handler(msg.EventName, dataStr, callback);
					return;
				}

				Console.WriteLine($"FlutterManager: No handler found for event '{msg.EventName}' on component '{msg.ComponentId}'");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"FlutterManager: Error handling event: {ex}");
			}
		}

		/// <summary>
		/// Handles action callbacks from Dart side.
		/// When a user interacts with a Flutter widget (e.g., taps a button),
		/// the Dart side sends an action message with the callback ID.
		/// </summary>
		private static void HandleAction(string data, Action<string> callback)
		{
			try
			{
				var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data, serializeOptions);
				if (message == null)
				{
					Console.WriteLine("FlutterManager: Failed to deserialize action message");
					return;
				}

				// Extract actionId (format: "action_123")
				if (!message.TryGetValue("actionId", out var actionIdElement))
				{
					Console.WriteLine("FlutterManager: Action message missing actionId");
					return;
				}

				var actionIdStr = actionIdElement.GetString();
				if (string.IsNullOrEmpty(actionIdStr))
				{
					Console.WriteLine("FlutterManager: Empty actionId");
					return;
				}

				// Parse the numeric ID from "action_123" format
				long actionId = 0;
				if (actionIdStr.StartsWith("action_"))
				{
					if (!long.TryParse(actionIdStr.Substring(7), out actionId))
					{
						Console.WriteLine($"FlutterManager: Invalid actionId format: {actionIdStr}");
						return;
					}
				}
				else if (!long.TryParse(actionIdStr, out actionId))
				{
					Console.WriteLine($"FlutterManager: Invalid actionId: {actionIdStr}");
					return;
				}

				// Extract typed arguments if present
				var args = ExtractCallbackArguments(message);

				// Invoke the callback
				CallbackRegistry.Invoke(actionId, args);

				// Send success response if callback provided
				callback?.Invoke("{\"success\": true}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"FlutterManager: Error handling action: {ex}");
				callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
			}
		}

		/// <summary>
		/// Extracts typed arguments from the action message.
		/// Converts JSON values to appropriate C# types.
		/// </summary>
		private static object[] ExtractCallbackArguments(Dictionary<string, JsonElement> message)
		{
			var args = new List<object>();

			// Check for 'value' key (used by ValueChanged<T> callbacks)
			if (message.TryGetValue("value", out var valueElement))
			{
				args.Add(ConvertJsonElement(valueElement));
			}

			// Check for gesture details (globalPosition, localPosition)
			if (message.TryGetValue("globalPosition", out var globalPos) ||
			    message.TryGetValue("localPosition", out var localPos))
			{
				// For gesture callbacks, create a details dictionary
				var details = new Dictionary<string, object>();

				if (message.TryGetValue("globalPosition", out globalPos))
				{
					details["globalPosition"] = ConvertJsonElement(globalPos);
				}
				if (message.TryGetValue("localPosition", out localPos))
				{
					details["localPosition"] = ConvertJsonElement(localPos);
				}

				args.Add(details);
			}

			return args.ToArray();
		}

		/// <summary>
		/// Converts a JsonElement to an appropriate C# type.
		/// </summary>
		private static object ConvertJsonElement(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.String:
					return element.GetString();
				case JsonValueKind.Number:
					if (element.TryGetInt32(out var intVal))
						return intVal;
					if (element.TryGetInt64(out var longVal))
						return longVal;
					return element.GetDouble();
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				case JsonValueKind.Null:
					return null;
				case JsonValueKind.Object:
					var dict = new Dictionary<string, object>();
					foreach (var prop in element.EnumerateObject())
					{
						dict[prop.Name] = ConvertJsonElement(prop.Value);
					}
					return dict;
				case JsonValueKind.Array:
					var list = new List<object>();
					foreach (var item in element.EnumerateArray())
					{
						list.Add(ConvertJsonElement(item));
					}
					return list;
				default:
					return element.ToString();
			}
		}

		/// <summary>
		/// Event raised when Flutter signals it's ready
		/// </summary>
		public static event Action OnReady;

		/// <summary>
		/// Registers a global event handler for a specific event type
		/// </summary>
		public static void RegisterEventHandler(string eventName, Action<string, string, Action<string>> handler)
		{
			lock (_lock)
			{
				EventHandlers[eventName] = handler;
			}
		}

		/// <summary>
		/// Unregisters a global event handler
		/// </summary>
		public static void UnregisterEventHandler(string eventName)
		{
			lock (_lock)
			{
				EventHandlers.Remove(eventName);
			}
		}

		/// <summary>
		/// Tracks a widget for lifecycle management and event routing
		/// </summary>
		public static void TrackWidget(Widget widget)
		{
			if (widget == null)
				return;

			lock (_lock)
			{
				AliveWidgets.Add(widget.Id, widget);
			}
		}

		/// <summary>
		/// Untracks a widget and notifies Flutter of its disposal
		/// </summary>
		public static void UntrackWidget(Widget widget)
		{
			if (widget == null)
				return;

			var id = widget.Id;
			lock (_lock)
			{
				AliveWidgets.Remove(id);
			}
			Communicator.SendDisposed(id);
		}

		/// <summary>
		/// Gets a tracked widget by ID
		/// </summary>
		public static Widget GetWidget(string widgetId)
		{
			lock (_lock)
			{
				if (AliveWidgets.TryGetValue(widgetId, out var widget))
					return widget;
				return null;
			}
		}

		/// <summary>
		/// Sends the current state of a widget to Flutter
		/// </summary>
		/// <param name="widget">The widget to send</param>
		/// <param name="componentID">The component ID (defaults to "0" for root)</param>
		public static void SendState(Widget widget, string componentID = "0")
		{
			if (widget == null)
			{
				Console.WriteLine("FlutterManager: Cannot send null widget");
				return;
			}

			if (Communicator.SendCommand == null)
			{
				Console.WriteLine("FlutterManager: Cannot send state - SendCommand not configured");
				return;
			}

			try
			{
				widget.PrepareForSending();
				var message = new UpdateMessage { ComponentId = componentID, Address = widget };
				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand.Invoke((message.MessageType, json));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"FlutterManager: Error sending widget state: {ex}");
			}
		}

		/// <summary>
		/// Sends the current state of a widget to Flutter asynchronously
		/// </summary>
		public static async Task SendStateAsync(Widget widget, string componentID = "0")
		{
			await Task.Run(() => SendState(widget, componentID));
		}

		/// <summary>
		/// Resets the FlutterManager state. Useful for testing or app restart.
		/// </summary>
		public static void Reset()
		{
			lock (_lock)
			{
				AliveWidgets.Clear();
				EventHandlers.Clear();
				_isReady = false;
			}
		}
	}
}
