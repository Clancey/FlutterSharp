using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Flutter.Diagnostics;
using Flutter.Gestures;
using Flutter.Widgets;
using Flutter.Logging;
using Flutter.Initialization;

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
		private static Dictionary<string, ScrollController> ScrollControllers = new Dictionary<string, ScrollController>();
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
		/// Gets or sets the root widget for the Flutter view
		/// </summary>
		public static Widget? RootWidget { get; set; }

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

		/// <summary>
		/// Initializes Flutter asynchronously with validation and timeout handling.
		/// This is the preferred way to initialize Flutter as it provides detailed error information
		/// and allows for graceful fallback when initialization fails.
		/// </summary>
		/// <param name="options">Initialization options. If null, uses defaults.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>The initialization result with success/failure information.</returns>
		/// <remarks>
		/// <para>
		/// Unlike the synchronous <see cref="Initialize"/> method, this method:
		/// </para>
		/// <list type="bullet">
		/// <item>Waits for the Flutter runtime to signal ready (with configurable timeout)</item>
		/// <item>Returns detailed diagnostics on failure</item>
		/// <item>Supports retry logic</item>
		/// <item>Supports cancellation</item>
		/// <item>Allows degraded mode operation</item>
		/// </list>
		/// <para>
		/// Example usage:
		/// </para>
		/// <code>
		/// var result = await FlutterManager.InitializeAsync(new FlutterInitializationOptions
		/// {
		///     ReadyTimeout = TimeSpan.FromSeconds(10),
		///     OnInitializationFailed = result => ShowFallbackUI()
		/// });
		///
		/// if (!result.IsSuccess)
		/// {
		///     logger.LogError("Flutter failed: {Error}", result.ErrorMessage);
		/// }
		/// </code>
		/// </remarks>
		public static Task<FlutterInitializationResult> InitializeAsync(
			FlutterInitializationOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			return FlutterInitializationValidator.InitializeAsync(options, cancellationToken);
		}

		/// <summary>
		/// Gets the result of the last initialization attempt, if any.
		/// </summary>
		public static FlutterInitializationResult? LastInitializationResult
			=> FlutterInitializationValidator.LastInitializationResult;

		/// <summary>
		/// Gets or sets the message timeout options used for communication with Dart.
		/// </summary>
		public static MessageTimeoutOptions TimeoutOptions
		{
			get => MessageTimeoutHandler.Options;
			set => MessageTimeoutHandler.Options = value;
		}

		/// <summary>
		/// Gets or sets whether message batching is enabled.
		/// When enabled, multiple widget updates within a time window are batched together
		/// and sent as a single message, reducing MethodChannel overhead.
		/// Default is true.
		/// </summary>
		public static bool BatchingEnabled
		{
			get => MessageBatcher.IsEnabled;
			set => MessageBatcher.IsEnabled = value;
		}

		/// <summary>
		/// Gets or sets the batch window in milliseconds.
		/// Updates are collected for this duration before being sent as a batch.
		/// Default is 16ms (~1 frame at 60fps). Valid range: 1-1000.
		/// </summary>
		public static int BatchWindowMs
		{
			get => MessageBatcher.BatchWindowMs;
			set => MessageBatcher.BatchWindowMs = value;
		}

		/// <summary>
		/// Gets statistics about message batching performance.
		/// </summary>
		public static BatchingStats GetBatchingStats() => MessageBatcher.GetStats();

		/// <summary>
		/// Resets the message batching statistics.
		/// </summary>
		public static void ResetBatchingStats() => MessageBatcher.ResetStats();

		/// <summary>
		/// Forces an immediate flush of all pending batched updates.
		/// Call this when you need updates to be sent without waiting for the batch window.
		/// </summary>
		public static void FlushBatchedUpdates() => MessageBatcher.FlushNow();

		/// <summary>
		/// Enables or disables rendering metrics collection on both C# and Dart sides.
		/// </summary>
		/// <param name="enabled">Whether to enable metrics collection.</param>
		/// <param name="targetFps">Target FPS for jank calculation (default 60).</param>
		public static void SetRenderingMetricsEnabled(bool enabled, double targetFps = 60.0)
		{
			// Enable C# side
			RenderingMetrics.Enabled = enabled;
			RenderingMetrics.TargetFps = targetFps;

			// Send command to Dart to enable/disable their side
			try
			{
				var message = new Dictionary<string, object>
				{
					["messageType"] = "EnableRenderingMetrics",
					["enabled"] = enabled,
					["targetFps"] = targetFps
				};
				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand?.Invoke(("EnableRenderingMetrics", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to send rendering metrics enable command to Dart");
			}
		}

		/// <summary>
		/// Gets the current rendering statistics from the C# side.
		/// </summary>
		/// <returns>Current rendering statistics, or null if metrics are disabled.</returns>
		public static RenderingStats? GetRenderingStats()
		{
			return RenderingMetrics.Enabled ? RenderingMetrics.GetStats() : null;
		}

		/// <summary>
		/// Logs the current rendering metrics report to the configured logger.
		/// </summary>
		public static void LogRenderingMetrics()
		{
			if (RenderingMetrics.Enabled)
			{
				RenderingMetrics.LogReport();
			}
			else
			{
				FlutterSharpLogger.LogInformation("Rendering metrics are disabled. Call SetRenderingMetricsEnabled(true) to enable.");
			}
		}

		/// <summary>
		/// Gets timeout statistics including number of timeouts and handler status.
		/// </summary>
		public static MessageTimeoutStats GetTimeoutStats() => MessageTimeoutHandler.GetStats();

		/// <summary>
		/// Event raised when a message operation times out.
		/// </summary>
		public static event EventHandler<MessageTimeoutEventArgs> OnMessageTimeout
		{
			add => MessageTimeoutHandler.OnTimeout += value;
			remove => MessageTimeoutHandler.OnTimeout -= value;
		}

		/// <summary>
		/// Validates the current Flutter configuration without initializing.
		/// Useful for pre-flight checks.
		/// </summary>
		/// <returns>Validation result with any issues found.</returns>
		public static FlutterInitializationValidator.ValidationResult ValidateConfiguration()
			=> FlutterInitializationValidator.ValidateConfiguration();

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

				case "StateNotify":
					HandleStateNotify(message.Data, message.Callback);
					return;

				case "ScrollUpdate":
					HandleScrollUpdate(message.Data, message.Callback);
					return;

				case "DartException":
					HandleDartException(message.Data, message.Callback);
					return;

				case "RenderingMetrics":
					HandleRenderingMetrics(message.Data, message.Callback);
					return;

				case "InvokeResponse":
					HandleInvokeResponse(message.Data);
					return;

				case "InvokeError":
					HandleInvokeError(message.Data);
					return;

				case "widgetTree":
					Diagnostics.WidgetInspector.HandleWidgetTree(message.Data);
					return;

				case "widgetSelected":
					Diagnostics.WidgetInspector.HandleWidgetSelected(message.Data);
					return;

				default:
					FlutterSharpLogger.LogWarning("Unknown method {Method}", message.Method);
					return;
			}
		}

		private static void HandleEvent(string data, Action<string> callback)
		{
			try
			{
				FlutterSharpLogger.LogDebug("Received event: {Data}", data);
				var msg = JsonSerializer.Deserialize<EventMessage>(data, serializeOptions);

				if (msg == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize event message");
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

				FlutterSharpLogger.LogWarning("No handler found for event {EventName} on component {ComponentId}", msg.EventName, msg.ComponentId);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling event");
			}
		}

		/// <summary>
		/// Handles state notification messages from Dart.
		/// When a Dart widget updates a value bound to a BidirectionalNotifier,
		/// this method receives the update and applies it to the C# notifier.
		/// </summary>
		private static void HandleStateNotify(string data, Action<string> callback)
		{
			try
			{
				var message = JsonSerializer.Deserialize<StateNotifyMessage>(data, serializeOptions);
				if (message == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize StateNotify message");
					callback?.Invoke("{\"success\": false, \"error\": \"Invalid message\"}");
					return;
				}

				if (string.IsNullOrEmpty(message.NotifierId))
				{
					FlutterSharpLogger.LogWarning("StateNotify missing notifierId");
					callback?.Invoke("{\"success\": false, \"error\": \"Missing notifierId\"}");
					return;
				}

				// Convert the value based on type hint if needed
				object value = message.Value;
				if (value is JsonElement jsonElement)
				{
					value = ConvertJsonElement(jsonElement);
				}

				// Apply the update to the notifier
				var success = NotifierRegistry.HandleStateNotify(
					message.NotifierId,
					value,
					message.SourceWidgetId);

				if (success)
				{
					// Raise event for observers
					OnStateNotifyReceived?.Invoke(null, new StateNotifyReceivedEventArgs(
						message.NotifierId, value, message.SourceWidgetId));
				}

				callback?.Invoke(success
					? "{\"success\": true}"
					: "{\"success\": false, \"error\": \"Notifier not found\"}");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling StateNotify");
				callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
			}
		}

		/// <summary>
		/// Event raised when a state notification is received from Dart.
		/// </summary>
		public static event EventHandler<StateNotifyReceivedEventArgs> OnStateNotifyReceived;

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
		/// Event raised when an exception occurs in Dart code.
		/// Subscribe to this event to be notified of Dart-side errors.
		/// Set Handled = true in EventArgs to suppress default logging.
		/// </summary>
		public static event EventHandler<DartExceptionEventArgs> OnDartException;

		/// <summary>
		/// Gets statistics about Dart exceptions received.
		/// </summary>
		public static DartExceptionStats DartExceptionStats { get; } = new DartExceptionStats();

		/// <summary>
		/// Handles exception messages from Dart.
		/// When Dart code throws an exception, this handler receives the details
		/// and raises the OnDartException event so C# code can respond.
		/// </summary>
		private static void HandleDartException(string data, Action<string> callback)
		{
			try
			{
				FlutterSharpLogger.LogDebug("Received Dart exception: {Data}", data);
				var message = JsonSerializer.Deserialize<DartExceptionMessage>(data, serializeOptions);
				if (message == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize Dart exception message");
					return;
				}

				// Update statistics
				DartExceptionStats.RecordException(message.ErrorType);

				// Raise event for subscribers
				var args = new DartExceptionEventArgs(message);
				OnDartException?.Invoke(null, args);

				// Default logging if not handled by subscriber
				if (!args.Handled)
				{
					if (message.HandledLocally)
					{
						FlutterSharpLogger.LogDebug("Dart exception (handled locally): [{ErrorType}] {Message}",
							message.ErrorType, message.Message);
					}
					else
					{
						FlutterSharpLogger.LogWarning("Dart exception: [{ErrorType}] {Message}",
							message.ErrorType, message.Message);
						if (!string.IsNullOrEmpty(message.StackTrace))
						{
							FlutterSharpLogger.LogDebug("Dart stack trace:\n{StackTrace}", message.StackTrace);
						}
					}
				}

				// Acknowledge receipt
				callback?.Invoke("{}");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling Dart exception");
			}
		}

		/// <summary>
		/// Handles rendering metrics from Dart side.
		/// Called periodically when RenderingMetrics is enabled on Dart side.
		/// </summary>
		private static void HandleRenderingMetrics(string data, Action<string> callback)
		{
			try
			{
				var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data, serializeOptions);
				if (message == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize rendering metrics message");
					return;
				}

				// Parse frame timing data from Dart
				var totalFrameCount = message.TryGetValue("totalFrameCount", out var fc) ? fc.GetInt32() : 0;
				var totalJankFrames = message.TryGetValue("totalJankFrames", out var jf) ? jf.GetInt32() : 0;
				var currentFps = message.TryGetValue("currentFps", out var cf) ? cf.GetDouble() : 0;
				var averageFps = message.TryGetValue("averageFps", out var af) ? af.GetDouble() : 0;
				var averageBuildTimeMs = message.TryGetValue("averageBuildTimeMs", out var bt) ? bt.GetDouble() : 0;
				var averageRasterTimeMs = message.TryGetValue("averageRasterTimeMs", out var rt) ? rt.GetDouble() : 0;
				var worstBuildTimeMs = message.TryGetValue("worstBuildTimeMs", out var wb) ? wb.GetDouble() : 0;
				var worstRasterTimeMs = message.TryGetValue("worstRasterTimeMs", out var wr) ? wr.GetDouble() : 0;
				var p50FrameTimeMs = message.TryGetValue("p50FrameTimeMs", out var p50) ? p50.GetDouble() : 0;
				var p95FrameTimeMs = message.TryGetValue("p95FrameTimeMs", out var p95) ? p95.GetDouble() : 0;
				var p99FrameTimeMs = message.TryGetValue("p99FrameTimeMs", out var p99) ? p99.GetDouble() : 0;
				var jankPercentage = message.TryGetValue("jankPercentage", out var jp) ? jp.GetDouble() : 0;

				// Record to RenderingMetrics
				if (Diagnostics.RenderingMetrics.Enabled)
				{
					// Record Dart frame timings
					var frameTimeMs = p50FrameTimeMs > 0 ? p50FrameTimeMs : 16.67;
					Diagnostics.RenderingMetrics.RecordDartFrame(frameTimeMs, averageBuildTimeMs, averageRasterTimeMs);
				}

				// Log verbose metrics if enabled
				if (FlutterSharpLogger.VerboseLoggingEnabled)
				{
					FlutterSharpLogger.LogDebug(
						"Dart rendering metrics: FPS={CurrentFps:F1}/{AverageFps:F1}, " +
						"Frames={TotalFrames}, Jank={JankFrames} ({JankPct:F1}%), " +
						"Build={BuildMs:F2}ms, Raster={RasterMs:F2}ms",
						currentFps, averageFps, totalFrameCount, totalJankFrames, jankPercentage,
						averageBuildTimeMs, averageRasterTimeMs
					);
				}

				// Raise event for subscribers
				OnRenderingMetricsReceived?.Invoke(null, new RenderingMetricsEventArgs(
					totalFrameCount, totalJankFrames, currentFps, averageFps,
					TimeSpan.FromMilliseconds(averageBuildTimeMs),
					TimeSpan.FromMilliseconds(averageRasterTimeMs),
					jankPercentage
				));

				callback?.Invoke("{\"success\": true}");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling rendering metrics");
				callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
			}
		}

		/// <summary>
		/// Event raised when rendering metrics are received from Dart.
		/// </summary>
		public static event EventHandler<RenderingMetricsEventArgs> OnRenderingMetricsReceived;

		/// <summary>
		/// Handles invoke response from Dart side.
		/// Routes the response back to the waiting TaskCompletionSource.
		/// </summary>
		private static void HandleInvokeResponse(string data)
		{
			try
			{
				var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data, serializeOptions);
				if (response == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize invoke response");
					return;
				}

				if (!response.TryGetValue("requestId", out var requestIdElement))
				{
					FlutterSharpLogger.LogWarning("Invoke response missing requestId");
					return;
				}

				var requestId = requestIdElement.GetInt32();
				object result = null;

				if (response.TryGetValue("result", out var resultElement))
				{
					// Convert JsonElement to appropriate type
					result = resultElement.ValueKind switch
					{
						JsonValueKind.String => resultElement.GetString(),
						JsonValueKind.Number => resultElement.TryGetInt32(out var i) ? i : resultElement.GetDouble(),
						JsonValueKind.True => true,
						JsonValueKind.False => false,
						JsonValueKind.Null => null,
						_ => resultElement.ToString()
					};
				}

				Communicator.HandleInvokeResponse(requestId, result);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling invoke response");
			}
		}

		/// <summary>
		/// Handles invoke error from Dart side.
		/// Routes the error back to the waiting TaskCompletionSource.
		/// </summary>
		private static void HandleInvokeError(string data)
		{
			try
			{
				var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data, serializeOptions);
				if (response == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize invoke error");
					return;
				}

				if (!response.TryGetValue("requestId", out var requestIdElement))
				{
					FlutterSharpLogger.LogWarning("Invoke error missing requestId");
					return;
				}

				var requestId = requestIdElement.GetInt32();
				var error = response.TryGetValue("error", out var errorElement)
					? errorElement.GetString() ?? "Unknown error"
					: "Unknown error";

				Communicator.HandleInvokeError(requestId, error);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling invoke error");
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
					FlutterSharpLogger.LogWarning("Failed to deserialize action message");
					return;
				}

				// Extract actionId (format: "action_123")
				if (!message.TryGetValue("actionId", out var actionIdElement))
				{
					FlutterSharpLogger.LogWarning("Action message missing actionId");
					return;
				}

				var actionIdStr = actionIdElement.GetString();
				if (string.IsNullOrEmpty(actionIdStr))
				{
					FlutterSharpLogger.LogWarning("Empty actionId");
					return;
				}

				// Parse the numeric ID from "action_123" format
				long actionId = 0;
				if (actionIdStr.StartsWith("action_"))
				{
					if (!long.TryParse(actionIdStr.Substring(7), out actionId))
					{
						FlutterSharpLogger.LogWarning("Invalid actionId format: {ActionId}", actionIdStr);
						return;
					}
				}
				else if (!long.TryParse(actionIdStr, out actionId))
				{
					FlutterSharpLogger.LogWarning("Invalid actionId: {ActionId}", actionIdStr);
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
				FlutterSharpLogger.LogError(ex, "Error handling action");
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
					FlutterSharpLogger.LogWarning("Unknown eventType {EventType}", eventType);
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
		/// <param name="immediate">If true, bypasses batching and sends immediately</param>
		public static void SendState(Widget widget, string componentID = "0", bool immediate = false)
		{
			if (widget == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send null widget");
				return;
			}

			if (Communicator.SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send state - SendCommand not configured");
				return;
			}

			try
			{
				FlutterSharpLogger.LogDebug("SendState called for C# widget type: {WidgetType}", widget.GetType().Name);

				// If widget is a StatelessWidget or StatefulWidget, call Build() to get the actual widget tree
				// This is necessary because custom StatelessWidget subclasses don't have Dart parsers
				Widget widgetToSend = widget;
				if (widget is StatelessWidget statelessWidget)
				{
					FlutterSharpLogger.LogDebug("Building StatelessWidget: {WidgetType}", widget.GetType().Name);
					widgetToSend = statelessWidget.Build();
					FlutterSharpLogger.LogDebug("Build result type: {ResultType}", widgetToSend?.GetType().Name);
				}
				else if (widget is StatefulWidget statefulWidget)
				{
					FlutterSharpLogger.LogDebug("Building StatefulWidget: {WidgetType}", widget.GetType().Name);
					widgetToSend = statefulWidget.Build();
					FlutterSharpLogger.LogDebug("Build result type: {ResultType}", widgetToSend?.GetType().Name);
				}

				if (widgetToSend == null)
				{
					FlutterSharpLogger.LogWarning("Build() returned null");
					return;
				}

				widgetToSend.PrepareForSending();
				var structPtr = (IntPtr)widgetToSend;
				FlutterSharpLogger.LogDebug("Widget struct address: 0x{StructPtr:X}", structPtr);

				// Use message batching unless immediate sending is requested
				if (!immediate && MessageBatcher.IsEnabled)
				{
					MessageBatcher.QueueUpdate(componentID, (long)structPtr, widgetToSend.GetType().Name);
					FlutterSharpLogger.LogDebug("Update queued for batching");
				}
				else
				{
					var message = new UpdateMessage { ComponentId = componentID, Address = (long)structPtr };
					var json = JsonSerializer.Serialize(message);
					FlutterSharpLogger.LogDebug("Sending JSON: {Json}", json);
					Communicator.SendCommand.Invoke((message.MessageType, json));
					FlutterSharpLogger.LogDebug("SendCommand invoked successfully");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending widget state");
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
		/// Event raised when a memory warning is received from the system.
		/// Subscribe to this event to handle memory cleanup in your code.
		/// </summary>
		public static event EventHandler<MemoryWarningEventArgs> OnMemoryWarning;

		/// <summary>
		/// Notifies Flutter and C# subscribers about a memory warning from the system.
		/// This allows Flutter to release caches and reduce memory footprint.
		/// </summary>
		/// <param name="level">The severity level of the memory warning</param>
		public static void NotifyMemoryWarning(MemoryWarningLevel level)
		{
			try
			{
				FlutterSharpLogger.LogWarning("Memory warning received: {Level}", level);

				// Notify C# subscribers first
				var eventArgs = new MemoryWarningEventArgs(level);
				OnMemoryWarning?.Invoke(null, eventArgs);

				// Notify Flutter
				if (_isReady && Communicator.SendCommand != null)
				{
					var levelString = level switch
					{
						MemoryWarningLevel.Low => "low",
						MemoryWarningLevel.Medium => "medium",
						MemoryWarningLevel.High => "high",
						MemoryWarningLevel.Critical => "critical",
						_ => "medium"
					};

					var message = new MemoryWarningMessage
					{
						Level = levelString,
						Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
					};
					var json = JsonSerializer.Serialize(message);
					Communicator.SendCommand.Invoke(("MemoryWarning", json));
				}

				// Perform automatic cleanup for critical memory situations
				if (level >= MemoryWarningLevel.High)
				{
					PerformMemoryCleanup(level);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling memory warning");
			}
		}

		/// <summary>
		/// Performs automatic memory cleanup when critical memory warnings are received.
		/// </summary>
		private static void PerformMemoryCleanup(MemoryWarningLevel level)
		{
			try
			{
				// Request GC
				GC.Collect(level == MemoryWarningLevel.Critical ? 2 : 1, GCCollectionMode.Optimized);

				// Clean up disposed widgets from tracking
				lock (_lock)
				{
					AliveWidgets.Cleanup();
				}

				FlutterSharpLogger.LogDebug("Performed memory cleanup for level {Level}", level);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error during memory cleanup");
			}
		}

		/// <summary>
		/// Notifies Flutter about app lifecycle state changes.
		/// This allows Flutter to pause animations, stop timers, etc. when the app is backgrounded.
		/// </summary>
		/// <param name="state">The new lifecycle state</param>
		public static void NotifyLifecycleState(FlutterLifecycleState state)
		{
			if (!_isReady || Communicator.SendCommand == null)
				return;

			try
			{
				var stateString = state switch
				{
					FlutterLifecycleState.Resumed => "resumed",
					FlutterLifecycleState.Inactive => "inactive",
					FlutterLifecycleState.Paused => "paused",
					FlutterLifecycleState.Detached => "detached",
					_ => "resumed"
				};

				var message = new LifecycleMessage { State = stateString };
				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand.Invoke(("Lifecycle", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending lifecycle state");
			}
		}

		/// <summary>
		/// Notifies Flutter of container size changes.
		/// This allows Flutter to adjust its rendering viewport accordingly.
		/// </summary>
		/// <param name="width">The new container width in logical pixels</param>
		/// <param name="height">The new container height in logical pixels</param>
		public static void NotifyContainerSize(double width, double height)
		{
			if (!_isReady || Communicator.SendCommand == null)
				return;

			try
			{
				var message = new ContainerSizeMessage { Width = width, Height = height };
				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand.Invoke(("ContainerSize", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending container size");
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

		#region ScrollController Management

		/// <summary>
		/// Registers a ScrollController for receiving scroll updates from Dart.
		/// </summary>
		internal static void RegisterScrollController(ScrollController controller)
		{
			if (controller == null)
				return;

			lock (_lock)
			{
				ScrollControllers[controller.ControllerId] = controller;
			}
		}

		/// <summary>
		/// Unregisters a ScrollController.
		/// </summary>
		internal static void UnregisterScrollController(string controllerId)
		{
			if (string.IsNullOrEmpty(controllerId))
				return;

			lock (_lock)
			{
				ScrollControllers.Remove(controllerId);
			}
		}

		/// <summary>
		/// Gets a registered ScrollController by ID.
		/// </summary>
		internal static ScrollController GetScrollController(string controllerId)
		{
			lock (_lock)
			{
				if (ScrollControllers.TryGetValue(controllerId, out var controller))
					return controller;
				return null;
			}
		}

		/// <summary>
		/// Handles scroll position updates from Dart.
		/// </summary>
		private static void HandleScrollUpdate(string data, Action<string> callback)
		{
			try
			{
				var message = JsonSerializer.Deserialize<ScrollUpdateMessage>(data, serializeOptions);
				if (message == null)
				{
					FlutterSharpLogger.LogWarning("Failed to deserialize ScrollUpdate message");
					callback?.Invoke("{\"success\": false, \"error\": \"Invalid message\"}");
					return;
				}

				ScrollController controller = null;
				lock (_lock)
				{
					ScrollControllers.TryGetValue(message.ControllerId, out controller);
				}

				if (controller == null)
				{
					callback?.Invoke("{\"success\": false, \"error\": \"Controller not found\"}");
					return;
				}

				// Route to appropriate handler based on event type
				switch (message.EventType)
				{
					case "attach":
						controller.NotifyAttach();
						break;

					case "detach":
						controller.NotifyDetach();
						break;

					case "scrollStart":
						controller.NotifyScrollStart(new ScrollStartDetails
						{
							Offset = message.Offset,
							AxisDirection = (AxisDirection)(message.AxisDirection ?? 0)
						});
						break;

					case "scrollUpdate":
						controller.NotifyScrollUpdate(new ScrollUpdateDetails
						{
							Offset = message.Offset,
							Delta = message.Delta ?? 0,
							MaxScrollExtent = message.MaxScrollExtent ?? 0,
							MinScrollExtent = message.MinScrollExtent ?? 0,
							ViewportDimension = message.ViewportDimension ?? 0,
							AxisDirection = (AxisDirection)(message.AxisDirection ?? 0)
						});
						break;

					case "scrollEnd":
						controller.NotifyScrollEnd(new ScrollEndDetails
						{
							Offset = message.Offset,
							Velocity = message.Velocity ?? 0,
							AxisDirection = (AxisDirection)(message.AxisDirection ?? 0)
						});
						break;

					default:
						// Simple offset update
						controller.UpdateFromDart(
							message.Offset,
							message.MaxScrollExtent ?? 0,
							message.MinScrollExtent ?? 0,
							message.ViewportDimension ?? 0,
							message.HasClients ?? true);
						break;
				}

				callback?.Invoke("{\"success\": true}");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling ScrollUpdate");
				callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
			}
		}

		/// <summary>
		/// Handles scroll position updates from binary protocol.
		/// Called by BinaryCommunicator when binary scroll data is received.
		/// </summary>
		internal static void HandleBinaryScrollUpdate(string controllerId, double offset, double maxExtent, double viewportDimension, ScrollEventType eventType)
		{
			try
			{
				if (!ScrollControllers.TryGetValue(controllerId, out var controller))
				{
					FlutterSharpLogger.LogDebug("ScrollController not found for binary update: {ControllerId}", controllerId);
					return;
				}

				switch (eventType)
				{
					case ScrollEventType.Start:
						controller.NotifyScrollStart(new ScrollStartDetails
						{
							Offset = offset
						});
						break;

					case ScrollEventType.Update:
						controller.NotifyScrollUpdate(new ScrollUpdateDetails
						{
							Offset = offset,
							MaxScrollExtent = maxExtent,
							ViewportDimension = viewportDimension
						});
						break;

					case ScrollEventType.End:
						controller.NotifyScrollEnd(new ScrollEndDetails
						{
							Offset = offset
						});
						break;
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error handling binary scroll update");
			}
		}

		/// <summary>
		/// Sends a scroll command to Dart (jumpTo, animateTo, etc.).
		/// </summary>
		internal static void SendScrollCommand(string controllerId, string command, double offset, double? durationMs = null, string curve = null)
		{
			if (Communicator.SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send scroll command - SendCommand not configured");
				return;
			}

			try
			{
				var message = new ScrollCommandMessage
				{
					ControllerId = controllerId,
					Command = command,
					Offset = offset,
					DurationMs = durationMs,
					Curve = curve
				};

				var json = JsonSerializer.Serialize(message, serializeOptions);
				Communicator.SendCommand.Invoke(("ScrollCommand", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending scroll command");
			}
		}

		/// <summary>
		/// Sends a notification to Dart that an async callback has completed.
		/// This is used by RefreshIndicator and similar widgets with Future callbacks.
		/// </summary>
		/// <param name="widgetId">The ID of the widget whose callback completed.</param>
		internal static void SendAsyncCallbackComplete(string widgetId)
		{
			if (Communicator.SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send async callback complete - SendCommand not configured");
				return;
			}

			try
			{
				var message = new Dictionary<string, object>
				{
					["widgetId"] = widgetId,
					["type"] = "AsyncCallbackComplete"
				};

				var json = JsonSerializer.Serialize(message, serializeOptions);
				Communicator.SendCommand.Invoke(("AsyncCallbackComplete", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending async callback complete");
			}
		}

		#endregion

		#region Error Overlay

		/// <summary>
		/// Sends an error to be displayed in the Flutter error overlay.
		/// This is useful for surfacing C# exceptions to developers in debug mode.
		/// </summary>
		/// <param name="errorType">The type of error (e.g., "CallbackError", "WidgetParseError")</param>
		/// <param name="message">The error message to display</param>
		/// <param name="stackTrace">Optional stack trace</param>
		/// <param name="widgetType">Optional widget type that caused the error</param>
		/// <param name="callbackId">Optional callback ID that caused the error</param>
		/// <param name="isRecoverable">Whether the error is recoverable</param>
		public static void SendError(
			string errorType,
			string message,
			string stackTrace = null,
			string widgetType = null,
			long? callbackId = null,
			bool isRecoverable = false)
		{
			if (!_isReady || Communicator.SendCommand == null)
			{
				// Log even if we can't send to Dart
				FlutterSharpLogger.LogError("Error overlay: [{ErrorType}] {Message}", errorType, message);
				return;
			}

			try
			{
				var errorMessage = new ErrorMessage
				{
					ErrorType = errorType,
					ErrorText = message,
					StackTrace = stackTrace,
					WidgetType = widgetType,
					CallbackId = callbackId,
					IsRecoverable = isRecoverable
				};

				var json = JsonSerializer.Serialize(errorMessage, serializeOptions);
				Communicator.SendCommand.Invoke(("Error", json));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending error notification");
			}
		}

		/// <summary>
		/// Sends an exception to the Flutter error overlay.
		/// </summary>
		/// <param name="exception">The exception to display</param>
		/// <param name="errorType">The type of error</param>
		/// <param name="widgetType">Optional widget type that caused the error</param>
		/// <param name="isRecoverable">Whether the error is recoverable</param>
		public static void SendError(
			Exception exception,
			string errorType = "Exception",
			string widgetType = null,
			bool isRecoverable = false)
		{
			if (exception == null)
				return;

			var message = exception.Message;
			var stackTrace = exception.StackTrace;

			// Unwrap TargetInvocationException to get the actual error
			if (exception is System.Reflection.TargetInvocationException tie && tie.InnerException != null)
			{
				message = tie.InnerException.Message;
				stackTrace = tie.InnerException.StackTrace;
			}

			SendError(errorType, message, stackTrace, widgetType, isRecoverable: isRecoverable);
		}

		/// <summary>
		/// Event raised when an error is about to be sent to the overlay.
		/// Set Cancel = true to suppress the error display.
		/// </summary>
		public static event EventHandler<ErrorOverlayEventArgs> OnErrorSending;

		/// <summary>
		/// Sends an error with event notification for filtering.
		/// </summary>
		internal static void SendErrorWithEvent(
			string errorType,
			string message,
			string stackTrace = null,
			string widgetType = null,
			long? callbackId = null,
			bool isRecoverable = false)
		{
			var eventArgs = new ErrorOverlayEventArgs(errorType, message, stackTrace, widgetType, callbackId, isRecoverable);
			OnErrorSending?.Invoke(null, eventArgs);

			if (!eventArgs.Cancel)
			{
				SendError(errorType, message, stackTrace, widgetType, callbackId, isRecoverable);
			}
		}

		#endregion

		#region Hot Reload Notifications

		/// <summary>
		/// Sends a hot reload notification to be displayed in Flutter.
		/// This provides visual feedback when hot reload completes.
		/// </summary>
		/// <param name="widgetType">Optional widget type that was reloaded.</param>
		/// <param name="durationMs">Optional duration of the hot reload in milliseconds.</param>
		public static void SendHotReloadNotification(string widgetType = null, int? durationMs = null)
		{
			if (Communicator.SendCommand == null)
			{
				FlutterSharpLogger.LogDebug("Hot reload notification skipped - SendCommand not configured");
				return;
			}

			try
			{
				var message = new Messages.HotReloadNotificationMessage
				{
					WidgetType = widgetType,
					Success = true,
					DurationMs = durationMs
				};

				var json = JsonSerializer.Serialize(message, serializeOptions);
				Communicator.SendCommand.Invoke((message.MessageType, json));
				FlutterSharpLogger.LogDebug("Hot reload notification sent: {WidgetType}", widgetType ?? "all widgets");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending hot reload notification");
			}
		}

		/// <summary>
		/// Sends a hot reload failure notification to be displayed in Flutter.
		/// </summary>
		/// <param name="errorMessage">The error message describing what went wrong.</param>
		/// <param name="widgetType">Optional widget type that failed to reload.</param>
		public static void SendHotReloadFailure(string errorMessage, string widgetType = null)
		{
			if (Communicator.SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Hot reload failure notification skipped - SendCommand not configured");
				return;
			}

			try
			{
				var message = new Messages.HotReloadNotificationMessage
				{
					WidgetType = widgetType,
					Success = false,
					ErrorMessage = errorMessage
				};

				var json = JsonSerializer.Serialize(message, serializeOptions);
				Communicator.SendCommand.Invoke((message.MessageType, json));
				FlutterSharpLogger.LogWarning("Hot reload failure notification sent: {ErrorMessage}", errorMessage);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending hot reload failure notification");
			}
		}

		#endregion
	}

	/// <summary>
	/// Event args for error overlay events.
	/// </summary>
	public class ErrorOverlayEventArgs : EventArgs
	{
		public string ErrorType { get; }
		public string Message { get; }
		public string StackTrace { get; }
		public string WidgetType { get; }
		public long? CallbackId { get; }
		public bool IsRecoverable { get; }
		public bool Cancel { get; set; }

		public ErrorOverlayEventArgs(
			string errorType,
			string message,
			string stackTrace,
			string widgetType,
			long? callbackId,
			bool isRecoverable)
		{
			ErrorType = errorType;
			Message = message;
			StackTrace = stackTrace;
			WidgetType = widgetType;
			CallbackId = callbackId;
			IsRecoverable = isRecoverable;
			Cancel = false;
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

	/// <summary>
	/// Event args for when a state notification is received from Dart.
	/// </summary>
	public class StateNotifyReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// The notifier ID that received the update.
		/// </summary>
		public string NotifierId { get; }

		/// <summary>
		/// The new value.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// The widget ID that initiated the change, if known.
		/// </summary>
		public string SourceWidgetId { get; }

		public StateNotifyReceivedEventArgs(string notifierId, object value, string sourceWidgetId)
		{
			NotifierId = notifierId;
			Value = value;
			SourceWidgetId = sourceWidgetId;
		}
	}

	/// <summary>
	/// Event args for when rendering metrics are received from Dart.
	/// </summary>
	public class RenderingMetricsEventArgs : EventArgs
	{
		/// <summary>
		/// Total frame count since metrics started.
		/// </summary>
		public int TotalFrameCount { get; }

		/// <summary>
		/// Total jank frames detected.
		/// </summary>
		public int TotalJankFrames { get; }

		/// <summary>
		/// Current FPS (rolling average).
		/// </summary>
		public double CurrentFps { get; }

		/// <summary>
		/// Average FPS over all frames.
		/// </summary>
		public double AverageFps { get; }

		/// <summary>
		/// Average build time per frame.
		/// </summary>
		public TimeSpan AverageBuildTime { get; }

		/// <summary>
		/// Average rasterization time per frame.
		/// </summary>
		public TimeSpan AverageRasterTime { get; }

		/// <summary>
		/// Percentage of frames that experienced jank.
		/// </summary>
		public double JankPercentage { get; }

		public RenderingMetricsEventArgs(
			int totalFrameCount,
			int totalJankFrames,
			double currentFps,
			double averageFps,
			TimeSpan averageBuildTime,
			TimeSpan averageRasterTime,
			double jankPercentage)
		{
			TotalFrameCount = totalFrameCount;
			TotalJankFrames = totalJankFrames;
			CurrentFps = currentFps;
			AverageFps = averageFps;
			AverageBuildTime = averageBuildTime;
			AverageRasterTime = averageRasterTime;
			JankPercentage = jankPercentage;
		}
	}
}
