using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flutter.Logging;

namespace Flutter.Internal
{
	/// <summary>
	/// Handles communication between C# and Flutter via platform channels
	/// </summary>
	public static class Communicator
	{
		private static int _pendingRequests = 0;
		private static int _nextRequestId = 0;
		private static readonly ConcurrentDictionary<int, TaskCompletionSource<object?>> _pendingInvocations = new();

		/// <summary>
		/// Gets the number of pending requests waiting for responses.
		/// </summary>
		public static int PendingRequests => _pendingRequests;
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
				FlutterSharpLogger.LogWarning("Cannot send disposal for widget {WidgetId} - SendCommand not configured", widgetId);
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
				FlutterSharpLogger.LogError("Error sending disposal for widget {WidgetId}: {ErrorMessage}", widgetId, ex.Message);
			}
		}

		/// <summary>
		/// Sends an event response back to Flutter
		/// </summary>
		internal static void SendEventResponse(string eventId, string response)
		{
			if (SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send event response - SendCommand not configured");
				return;
			}

			try
			{
				SendCommand.Invoke(("EventResponse", response));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Error sending event response: {ErrorMessage}", ex.Message);
			}
		}

		/// <summary>
		/// Sends a command to Flutter with timeout protection.
		/// Returns a result indicating success, timeout, or failure.
		/// </summary>
		/// <param name="method">The method name to invoke.</param>
		/// <param name="arguments">The arguments to send (will be JSON serialized if not already a string).</param>
		/// <param name="timeout">Optional custom timeout. Uses default SendTimeout if not specified.</param>
		/// <returns>A MessageResult with success/failure status.</returns>
		public static async Task<MessageResult> SendWithTimeoutAsync(
			string method,
			object arguments,
			TimeSpan? timeout = null)
		{
			if (SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot send {Method} - SendCommand not configured", method);
				return MessageResult.Failed("SendCommand not configured");
			}

			var effectiveTimeout = timeout ?? MessageTimeoutHandler.Options.SendTimeout;
			var startTime = DateTime.UtcNow;

			try
			{
				Interlocked.Increment(ref _pendingRequests);

				using var cts = new CancellationTokenSource();
				cts.CancelAfter(effectiveTimeout);

				var sendTask = Task.Run(() =>
				{
					var json = arguments is string s ? s : JsonSerializer.Serialize(arguments);
					SendCommand.Invoke((method, json));
				}, cts.Token);

				var completedTask = await Task.WhenAny(
					sendTask,
					Task.Delay(Timeout.Infinite, cts.Token));

				var elapsed = DateTime.UtcNow - startTime;

				if (completedTask == sendTask)
				{
					cts.Cancel();
					await sendTask; // Propagate exceptions
					return MessageResult.Succeeded(duration: elapsed);
				}
				else
				{
					FlutterSharpLogger.LogWarning("Send timeout for {Method} after {ElapsedMs}ms", method, elapsed.TotalMilliseconds);
					return MessageResult.Timeout(elapsed, method);
				}
			}
			catch (OperationCanceledException)
			{
				var elapsed = DateTime.UtcNow - startTime;
				return MessageResult.Timeout(elapsed, method);
			}
			catch (Exception ex)
			{
				var elapsed = DateTime.UtcNow - startTime;
				FlutterSharpLogger.LogError(ex, "Error sending {Method}", method);
				return MessageResult.Failed(ex.Message, elapsed);
			}
			finally
			{
				Interlocked.Decrement(ref _pendingRequests);
			}
		}

		/// <summary>
		/// Sends a command with timeout protection (synchronous wrapper).
		/// </summary>
		public static MessageResult SendWithTimeout(
			string method,
			object arguments,
			TimeSpan? timeout = null)
		{
			return SendWithTimeoutAsync(method, arguments, timeout).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Invokes a method on the Flutter side and waits for a response.
		/// </summary>
		/// <param name="method">The method name to invoke.</param>
		/// <param name="arguments">The arguments to send (will be JSON serialized if not already a string).</param>
		/// <param name="timeout">Optional custom timeout. Defaults to 30 seconds.</param>
		/// <returns>The response from Flutter, or null if no response.</returns>
		public static async Task<object?> InvokeMethodAsync(
			string method,
			object? arguments,
			TimeSpan? timeout = null)
		{
			if (SendCommand == null)
			{
				FlutterSharpLogger.LogWarning("Cannot invoke {Method} - SendCommand not configured", method);
				return null;
			}

			var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
			var requestId = Interlocked.Increment(ref _nextRequestId);
			var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			_pendingInvocations[requestId] = tcs;

			try
			{
				Interlocked.Increment(ref _pendingRequests);

				// Build the invocation message
				var invocation = new
				{
					requestId,
					method,
					arguments
				};

				var json = JsonSerializer.Serialize(invocation);
				SendCommand.Invoke(("Invoke", json));

				// Wait for response with timeout
				using var cts = new CancellationTokenSource();
				cts.CancelAfter(effectiveTimeout);

				var delayTask = Task.Delay(Timeout.Infinite, cts.Token);
				var completedTask = await Task.WhenAny(tcs.Task, delayTask);

				if (completedTask == tcs.Task)
				{
					cts.Cancel();
					return await tcs.Task;
				}
				else
				{
					FlutterSharpLogger.LogWarning("Invoke timeout for {Method} after {TimeoutMs}ms", method, effectiveTimeout.TotalMilliseconds);
					return null;
				}
			}
			catch (OperationCanceledException)
			{
				FlutterSharpLogger.LogWarning("Invoke cancelled for {Method}", method);
				return null;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error invoking {Method}", method);
				return null;
			}
			finally
			{
				_pendingInvocations.TryRemove(requestId, out _);
				Interlocked.Decrement(ref _pendingRequests);
			}
		}

		/// <summary>
		/// Handles an invoke response from Flutter.
		/// </summary>
		/// <param name="requestId">The original request ID.</param>
		/// <param name="result">The result from Flutter.</param>
		internal static void HandleInvokeResponse(int requestId, object? result)
		{
			if (_pendingInvocations.TryRemove(requestId, out var tcs))
			{
				tcs.TrySetResult(result);
			}
			else
			{
				FlutterSharpLogger.LogDebug("Received response for unknown request ID: {RequestId}", requestId);
			}
		}

		/// <summary>
		/// Handles an invoke error from Flutter.
		/// </summary>
		/// <param name="requestId">The original request ID.</param>
		/// <param name="error">The error message.</param>
		internal static void HandleInvokeError(int requestId, string error)
		{
			if (_pendingInvocations.TryRemove(requestId, out var tcs))
			{
				tcs.TrySetException(new InvalidOperationException(error));
			}
			else
			{
				FlutterSharpLogger.LogDebug("Received error for unknown request ID: {RequestId}", requestId);
			}
		}
	}
}
