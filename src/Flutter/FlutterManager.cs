using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Flutter.Gestures;
using Flutter.Widgets;

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
		/// Event raised before an action callback is invoked.
		/// Set Cancel = true in EventArgs to prevent invocation.
		/// </summary>
		public static event EventHandler<ActionInvokingEventArgs> OnActionInvoking;

		/// <summary>
		/// Event raised after an action callback is invoked successfully.
		/// </summary>
		public static event EventHandler<ActionInvokedEventArgs> OnActionInvoked;

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

				// Extract widget type hint if available
				var widgetType = message.TryGetValue("widgetType", out var wtElem) ? wtElem.GetString() : null;

				// Raise pre-invoke event for filtering/interception
				var invokingArgs = new ActionInvokingEventArgs(actionId, args, widgetType);
				OnActionInvoking?.Invoke(null, invokingArgs);

				if (invokingArgs.Cancel)
				{
					callback?.Invoke("{\"success\": true, \"cancelled\": true}");
					return;
				}

				// Use typed invocation when possible for better performance
				InvokeCallbackTyped(actionId, args);

				// Raise post-invoke event for logging/debugging
				OnActionInvoked?.Invoke(null, new ActionInvokedEventArgs(actionId, args, widgetType));

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
		/// Invokes a callback using typed methods when possible for better performance.
		/// Falls back to DynamicInvoke for unknown types.
		/// </summary>
		private static void InvokeCallbackTyped(long actionId, object[] args)
		{
			// No arguments - use void callback
			if (args == null || args.Length == 0)
			{
				CallbackRegistry.InvokeVoid(actionId);
				return;
			}

			// Single argument - try typed invocation
			if (args.Length == 1)
			{
				var arg = args[0];
				if (arg == null)
				{
					CallbackRegistry.Invoke(actionId, args);
					return;
				}

				// Route to type-specific invoke method
				switch (arg)
				{
					case bool boolVal:
						CallbackRegistry.Invoke(actionId, boolVal);
						break;
					case int intVal:
						CallbackRegistry.Invoke(actionId, intVal);
						break;
					case double doubleVal:
						CallbackRegistry.Invoke(actionId, doubleVal);
						break;
					case string strVal:
						CallbackRegistry.Invoke(actionId, strVal);
						break;
					// Gesture event types
					case TapDownDetails tapDown:
						CallbackRegistry.Invoke(actionId, tapDown);
						break;
					case TapUpDetails tapUp:
						CallbackRegistry.Invoke(actionId, tapUp);
						break;
					case DragUpdateDetails dragUpdate:
						CallbackRegistry.Invoke(actionId, dragUpdate);
						break;
					case DragEndDetails dragEnd:
						CallbackRegistry.Invoke(actionId, dragEnd);
						break;
					case DragStartDetails dragStart:
						CallbackRegistry.Invoke(actionId, dragStart);
						break;
					case ScaleUpdateDetails scaleUpdate:
						CallbackRegistry.Invoke(actionId, scaleUpdate);
						break;
					case ScaleStartDetails scaleStart:
						CallbackRegistry.Invoke(actionId, scaleStart);
						break;
					case ScaleEndDetails scaleEnd:
						CallbackRegistry.Invoke(actionId, scaleEnd);
						break;
					case LongPressStartDetails lpStart:
						CallbackRegistry.Invoke(actionId, lpStart);
						break;
					case LongPressMoveUpdateDetails lpMove:
						CallbackRegistry.Invoke(actionId, lpMove);
						break;
					case LongPressEndDetails lpEnd:
						CallbackRegistry.Invoke(actionId, lpEnd);
						break;
					case ForcePressDetails forcePress:
						CallbackRegistry.Invoke(actionId, forcePress);
						break;
					case PointerDownEvent ptrDown:
						CallbackRegistry.Invoke(actionId, ptrDown);
						break;
					case PointerMoveEvent ptrMove:
						CallbackRegistry.Invoke(actionId, ptrMove);
						break;
					case PointerUpEvent ptrUp:
						CallbackRegistry.Invoke(actionId, ptrUp);
						break;
					case PointerCancelEvent ptrCancel:
						CallbackRegistry.Invoke(actionId, ptrCancel);
						break;
					case PointerHoverEvent ptrHover:
						CallbackRegistry.Invoke(actionId, ptrHover);
						break;
					case PointerScrollEvent ptrScroll:
						CallbackRegistry.Invoke(actionId, ptrScroll);
						break;
					default:
						// Fall back to dynamic invoke for unknown types
						CallbackRegistry.Invoke(actionId, args);
						break;
				}
				return;
			}

			// Multiple arguments - use dynamic invoke
			CallbackRegistry.Invoke(actionId, args);
		}

		/// <summary>
		/// Extracts typed arguments from the action message.
		/// Converts JSON values to appropriate C# types including gesture event data.
		/// </summary>
		private static object[] ExtractCallbackArguments(Dictionary<string, JsonElement> message)
		{
			var args = new List<object>();

			// Check for 'value' key (used by ValueChanged<T> callbacks)
			if (message.TryGetValue("value", out var valueElement))
			{
				args.Add(ConvertJsonElement(valueElement));
				return args.ToArray();
			}

			// Check for eventType hint from Dart side
			if (message.TryGetValue("eventType", out var eventTypeElement))
			{
				var eventType = eventTypeElement.GetString();
				var eventData = CreateGestureEventFromType(eventType, message);
				if (eventData != null)
				{
					args.Add(eventData);
					return args.ToArray();
				}
			}

			// Auto-detect event type based on fields present
			var detectedEvent = TryCreateGestureEvent(message);
			if (detectedEvent != null)
			{
				args.Add(detectedEvent);
				return args.ToArray();
			}

			return args.ToArray();
		}

		/// <summary>
		/// Creates a gesture event object based on the explicit eventType field.
		/// </summary>
		private static object CreateGestureEventFromType(string eventType, Dictionary<string, JsonElement> message)
		{
			switch (eventType)
			{
				// Tap events
				case "TapDownDetails":
					return new TapDownDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Kind = ExtractInt(message, "kind")
					};
				case "TapUpDetails":
					return new TapUpDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Kind = ExtractInt(message, "kind")
					};
				case "TapMoveDetails":
					return new TapMoveDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition")
					};

				// Drag events
				case "DragDownDetails":
					return new DragDownDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition")
					};
				case "DragStartDetails":
					return new DragStartDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition")
					};
				case "DragUpdateDetails":
					return new DragUpdateDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Delta = ExtractOffset(message, "delta")
					};
				case "DragEndDetails":
					return new DragEndDetails
					{
						Velocity = ExtractVelocity(message, "velocity"),
						PrimaryVelocity = ExtractDouble(message, "primaryVelocity")
					};

				// Long press events
				case "LongPressDownDetails":
					return new LongPressDownDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Kind = ExtractInt(message, "kind")
					};
				case "LongPressStartDetails":
					return new LongPressStartDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition")
					};
				case "LongPressMoveUpdateDetails":
					return new LongPressMoveUpdateDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						OffsetFromOrigin = ExtractOffset(message, "offsetFromOrigin"),
						LocalOffsetFromOrigin = ExtractOffset(message, "localOffsetFromOrigin")
					};
				case "LongPressEndDetails":
					return new LongPressEndDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Velocity = ExtractVelocity(message, "velocity")
					};

				// Scale events
				case "ScaleStartDetails":
					return new ScaleStartDetails
					{
						FocalPoint = ExtractOffset(message, "focalPoint"),
						LocalFocalPoint = ExtractOffset(message, "localFocalPoint"),
						PointerCount = ExtractInt(message, "pointerCount")
					};
				case "ScaleUpdateDetails":
					return new ScaleUpdateDetails
					{
						FocalPoint = ExtractOffset(message, "focalPoint"),
						LocalFocalPoint = ExtractOffset(message, "localFocalPoint"),
						FocalPointDelta = ExtractOffset(message, "focalPointDelta"),
						Scale = ExtractDouble(message, "scale", 1.0),
						HorizontalScale = ExtractDouble(message, "horizontalScale", 1.0),
						VerticalScale = ExtractDouble(message, "verticalScale", 1.0),
						Rotation = ExtractDouble(message, "rotation"),
						PointerCount = ExtractInt(message, "pointerCount")
					};
				case "ScaleEndDetails":
					return new ScaleEndDetails
					{
						Velocity = ExtractVelocity(message, "velocity"),
						PointerCount = ExtractInt(message, "pointerCount")
					};

				// Force press events
				case "ForcePressDetails":
					return new ForcePressDetails
					{
						GlobalPosition = ExtractOffset(message, "globalPosition"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Pressure = ExtractDouble(message, "pressure")
					};

				// Pointer events
				case "PointerDownEvent":
					return new PointerDownEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Pointer = ExtractInt(message, "pointer"),
						Kind = ExtractInt(message, "kind")
					};
				case "PointerMoveEvent":
					return new PointerMoveEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Delta = ExtractOffset(message, "delta"),
						Pointer = ExtractInt(message, "pointer")
					};
				case "PointerUpEvent":
					return new PointerUpEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Pointer = ExtractInt(message, "pointer"),
						Kind = ExtractInt(message, "kind")
					};
				case "PointerCancelEvent":
					return new PointerCancelEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Pointer = ExtractInt(message, "pointer"),
						Kind = ExtractInt(message, "kind")
					};
				case "PointerHoverEvent":
					return new PointerHoverEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						Delta = ExtractOffset(message, "delta"),
						Pointer = ExtractInt(message, "pointer")
					};
				case "PointerScrollEvent":
					return new PointerScrollEvent
					{
						Position = ExtractOffset(message, "position"),
						LocalPosition = ExtractOffset(message, "localPosition"),
						ScrollDelta = ExtractOffset(message, "scrollDelta"),
						Pointer = ExtractInt(message, "pointer")
					};

				default:
					Console.WriteLine($"FlutterManager: Unknown eventType '{eventType}'");
					return null;
			}
		}

		/// <summary>
		/// Attempts to auto-detect and create a gesture event based on present fields.
		/// </summary>
		private static object TryCreateGestureEvent(Dictionary<string, JsonElement> message)
		{
			// Check for scale/zoom events (have focalPoint)
			if (message.ContainsKey("focalPoint"))
			{
				if (message.ContainsKey("scale"))
				{
					return CreateGestureEventFromType("ScaleUpdateDetails", message);
				}
				return CreateGestureEventFromType("ScaleStartDetails", message);
			}

			// Check for drag events (have delta without focalPoint)
			if (message.ContainsKey("delta") && message.ContainsKey("globalPosition"))
			{
				return CreateGestureEventFromType("DragUpdateDetails", message);
			}

			// Check for velocity-only events (DragEndDetails, ScaleEndDetails)
			if (message.ContainsKey("velocity") && !message.ContainsKey("globalPosition"))
			{
				if (message.ContainsKey("pointerCount"))
				{
					return CreateGestureEventFromType("ScaleEndDetails", message);
				}
				return CreateGestureEventFromType("DragEndDetails", message);
			}

			// Check for force press events
			if (message.ContainsKey("pressure") && message.ContainsKey("globalPosition"))
			{
				return CreateGestureEventFromType("ForcePressDetails", message);
			}

			// Check for pointer events (have position instead of globalPosition)
			if (message.ContainsKey("position") && !message.ContainsKey("globalPosition"))
			{
				if (message.ContainsKey("scrollDelta"))
				{
					return CreateGestureEventFromType("PointerScrollEvent", message);
				}
				if (message.ContainsKey("delta"))
				{
					return CreateGestureEventFromType("PointerMoveEvent", message);
				}
				// Can't distinguish up/down/cancel without more info - return generic down
				return CreateGestureEventFromType("PointerDownEvent", message);
			}

			// Check for long press events (have offsetFromOrigin)
			if (message.ContainsKey("offsetFromOrigin"))
			{
				return CreateGestureEventFromType("LongPressMoveUpdateDetails", message);
			}

			// Check for tap events (have globalPosition, localPosition)
			if (message.ContainsKey("globalPosition") || message.ContainsKey("localPosition"))
			{
				// Default to TapDownDetails for simple position events
				return CreateGestureEventFromType("TapDownDetails", message);
			}

			return null;
		}

		/// <summary>
		/// Extracts an Offset from a nested JSON object.
		/// </summary>
		private static Offset ExtractOffset(Dictionary<string, JsonElement> message, string key)
		{
			if (!message.TryGetValue(key, out var element))
				return new Offset(0, 0);

			if (element.ValueKind == JsonValueKind.Object)
			{
				double dx = 0, dy = 0;
				if (element.TryGetProperty("dx", out var dxElem) || element.TryGetProperty("x", out dxElem))
					dx = dxElem.GetDouble();
				if (element.TryGetProperty("dy", out var dyElem) || element.TryGetProperty("y", out dyElem))
					dy = dyElem.GetDouble();
				return new Offset(dx, dy);
			}

			return new Offset(0, 0);
		}

		/// <summary>
		/// Extracts a Velocity from a nested JSON object.
		/// </summary>
		private static Velocity ExtractVelocity(Dictionary<string, JsonElement> message, string key)
		{
			if (!message.TryGetValue(key, out var element))
				return new Velocity(new Offset(0, 0));

			if (element.ValueKind == JsonValueKind.Object)
			{
				if (element.TryGetProperty("pixelsPerSecond", out var ppsElem) && ppsElem.ValueKind == JsonValueKind.Object)
				{
					double dx = 0, dy = 0;
					if (ppsElem.TryGetProperty("dx", out var dxElem) || ppsElem.TryGetProperty("x", out dxElem))
						dx = dxElem.GetDouble();
					if (ppsElem.TryGetProperty("dy", out var dyElem) || ppsElem.TryGetProperty("y", out dyElem))
						dy = dyElem.GetDouble();
					return new Velocity(new Offset(dx, dy));
				}
				// Alternative: velocity might be sent as direct dx/dy
				double vdx = 0, vdy = 0;
				if (element.TryGetProperty("dx", out var vdxElem) || element.TryGetProperty("x", out vdxElem))
					vdx = vdxElem.GetDouble();
				if (element.TryGetProperty("dy", out var vdyElem) || element.TryGetProperty("y", out vdyElem))
					vdy = vdyElem.GetDouble();
				return new Velocity(new Offset(vdx, vdy));
			}

			return new Velocity(new Offset(0, 0));
		}

		/// <summary>
		/// Extracts an integer from the message.
		/// </summary>
		private static int ExtractInt(Dictionary<string, JsonElement> message, string key, int defaultValue = 0)
		{
			if (message.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.Number)
				return element.GetInt32();
			return defaultValue;
		}

		/// <summary>
		/// Extracts a double from the message.
		/// </summary>
		private static double ExtractDouble(Dictionary<string, JsonElement> message, string key, double defaultValue = 0.0)
		{
			if (message.TryGetValue(key, out var element) && element.ValueKind == JsonValueKind.Number)
				return element.GetDouble();
			return defaultValue;
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
				Console.WriteLine($"[FlutterManager] SendState called for C# widget type: {widget.GetType().Name}");

				// If widget is a StatelessWidget or StatefulWidget, call Build() to get the actual widget tree
				// This is necessary because custom StatelessWidget subclasses don't have Dart parsers
				Widget widgetToSend = widget;
				if (widget is StatelessWidget statelessWidget)
				{
					Console.WriteLine($"[FlutterManager] Building StatelessWidget: {widget.GetType().Name}");
					widgetToSend = statelessWidget.Build();
					Console.WriteLine($"[FlutterManager] Build result type: {widgetToSend?.GetType().Name}");
				}
				else if (widget is StatefulWidget statefulWidget)
				{
					Console.WriteLine($"[FlutterManager] Building StatefulWidget: {widget.GetType().Name}");
					widgetToSend = statefulWidget.Build();
					Console.WriteLine($"[FlutterManager] Build result type: {widgetToSend?.GetType().Name}");
				}

				if (widgetToSend == null)
				{
					Console.WriteLine("FlutterManager: Build() returned null");
					return;
				}

				widgetToSend.PrepareForSending();
				var structPtr = (IntPtr)widgetToSend;
				Console.WriteLine($"[FlutterManager] Widget struct address: 0x{structPtr:X}");
				var message = new UpdateMessage { ComponentId = componentID, Address = widgetToSend };
				var json = JsonSerializer.Serialize(message);
				Console.WriteLine($"[FlutterManager] Sending JSON: {json}");
				Communicator.SendCommand.Invoke((message.MessageType, json));
				Console.WriteLine($"[FlutterManager] SendCommand invoked successfully");
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

		/// <summary>
		/// Gets diagnostic information about event routing.
		/// </summary>
		public static EventRoutingStats GetEventStats()
		{
			return new EventRoutingStats
			{
				TotalCallbackInvocations = CallbackRegistry.TotalInvocations,
				FailedCallbackInvocations = CallbackRegistry.FailedInvocations,
				RegisteredCallbacks = CallbackRegistry.Count,
				TrackedWidgets = AliveWidgets.Count,
				RegisteredEventHandlers = EventHandlers.Count
			};
		}
	}

	/// <summary>
	/// Event args for action invocation pre-event.
	/// </summary>
	public class ActionInvokingEventArgs : EventArgs
	{
		/// <summary>
		/// The callback ID being invoked.
		/// </summary>
		public long ActionId { get; }

		/// <summary>
		/// The arguments to be passed to the callback.
		/// </summary>
		public object[] Arguments { get; }

		/// <summary>
		/// The widget type that raised the event, if known.
		/// </summary>
		public string WidgetType { get; }

		/// <summary>
		/// Set to true to cancel the callback invocation.
		/// </summary>
		public bool Cancel { get; set; }

		public ActionInvokingEventArgs(long actionId, object[] arguments, string widgetType)
		{
			ActionId = actionId;
			Arguments = arguments;
			WidgetType = widgetType;
			Cancel = false;
		}
	}

	/// <summary>
	/// Event args for action invocation post-event.
	/// </summary>
	public class ActionInvokedEventArgs : EventArgs
	{
		/// <summary>
		/// The callback ID that was invoked.
		/// </summary>
		public long ActionId { get; }

		/// <summary>
		/// The arguments that were passed to the callback.
		/// </summary>
		public object[] Arguments { get; }

		/// <summary>
		/// The widget type that raised the event, if known.
		/// </summary>
		public string WidgetType { get; }

		public ActionInvokedEventArgs(long actionId, object[] arguments, string widgetType)
		{
			ActionId = actionId;
			Arguments = arguments;
			WidgetType = widgetType;
		}
	}

	/// <summary>
	/// Diagnostic information about event routing.
	/// </summary>
	public class EventRoutingStats
	{
		/// <summary>
		/// Total number of callback invocations since startup.
		/// </summary>
		public long TotalCallbackInvocations { get; set; }

		/// <summary>
		/// Total number of failed callback invocations since startup.
		/// </summary>
		public long FailedCallbackInvocations { get; set; }

		/// <summary>
		/// Number of currently registered callbacks.
		/// </summary>
		public int RegisteredCallbacks { get; set; }

		/// <summary>
		/// Number of currently tracked widgets.
		/// </summary>
		public int TrackedWidgets { get; set; }

		/// <summary>
		/// Number of registered event handlers.
		/// </summary>
		public int RegisteredEventHandlers { get; set; }

		public override string ToString()
		{
			return $"EventRoutingStats {{ Invocations: {TotalCallbackInvocations}, Failed: {FailedCallbackInvocations}, Callbacks: {RegisteredCallbacks}, Widgets: {TrackedWidgets}, Handlers: {RegisteredEventHandlers} }}";
		}
	}
}
