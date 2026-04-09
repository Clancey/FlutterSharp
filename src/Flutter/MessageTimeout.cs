using System;
using System.Threading;
using System.Threading.Tasks;
using Flutter.Logging;

namespace Flutter.Internal
{
	/// <summary>
	/// Configuration options for message timeout handling.
	/// </summary>
	public class MessageTimeoutOptions
	{
		/// <summary>
		/// Default timeout for message handlers (callback invocations, event handlers).
		/// Default is 5 seconds.
		/// </summary>
		public TimeSpan HandlerTimeout { get; set; } = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Timeout for waiting for Dart responses when using request-response pattern.
		/// Default is 10 seconds.
		/// </summary>
		public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Timeout for send operations. Default is 2 seconds.
		/// </summary>
		public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(2);

		/// <summary>
		/// Whether to run handlers on a background thread to avoid blocking the UI thread.
		/// Default is true.
		/// </summary>
		public bool RunHandlersOnBackgroundThread { get; set; } = true;

		/// <summary>
		/// Whether to send timeout errors to the error overlay. Default is true.
		/// </summary>
		public bool ShowTimeoutErrors { get; set; } = true;

		/// <summary>
		/// Maximum number of concurrent message handlers. Default is 10.
		/// Helps prevent thread pool exhaustion.
		/// </summary>
		public int MaxConcurrentHandlers { get; set; } = 10;

		/// <summary>
		/// Default options instance.
		/// </summary>
		public static MessageTimeoutOptions Default { get; } = new MessageTimeoutOptions();
	}

	/// <summary>
	/// Result of a message operation.
	/// </summary>
	public class MessageResult
	{
		/// <summary>
		/// Whether the operation completed successfully.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Whether the operation timed out.
		/// </summary>
		public bool TimedOut { get; set; }

		/// <summary>
		/// Error message if the operation failed.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// The response data, if any.
		/// </summary>
		public string Response { get; set; }

		/// <summary>
		/// How long the operation took.
		/// </summary>
		public TimeSpan Duration { get; set; }

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		public static MessageResult Succeeded(string response = null, TimeSpan duration = default)
			=> new MessageResult { Success = true, Response = response, Duration = duration };

		/// <summary>
		/// Creates a timeout result.
		/// </summary>
		public static MessageResult Timeout(TimeSpan duration, string operation = null)
			=> new MessageResult
			{
				Success = false,
				TimedOut = true,
				ErrorMessage = $"Operation '{operation ?? "message"}' timed out after {duration.TotalMilliseconds:F0}ms",
				Duration = duration
			};

		/// <summary>
		/// Creates a failure result.
		/// </summary>
		public static MessageResult Failed(string error, TimeSpan duration = default)
			=> new MessageResult { Success = false, ErrorMessage = error, Duration = duration };
	}

	/// <summary>
	/// Event args for message timeout events.
	/// </summary>
	public class MessageTimeoutEventArgs : EventArgs
	{
		/// <summary>
		/// The type of operation that timed out.
		/// </summary>
		public string OperationType { get; }

		/// <summary>
		/// The method name or message type.
		/// </summary>
		public string Method { get; }

		/// <summary>
		/// The configured timeout that was exceeded.
		/// </summary>
		public TimeSpan Timeout { get; }

		/// <summary>
		/// How long the operation was running before timeout was triggered.
		/// </summary>
		public TimeSpan ElapsedTime { get; }

		/// <summary>
		/// Additional context about the timeout (widget ID, callback ID, etc.).
		/// </summary>
		public string Context { get; }

		public MessageTimeoutEventArgs(string operationType, string method, TimeSpan timeout, TimeSpan elapsed, string context = null)
		{
			OperationType = operationType;
			Method = method;
			Timeout = timeout;
			ElapsedTime = elapsed;
			Context = context;
		}
	}

	/// <summary>
	/// Handles message timeouts and provides async execution with timeout support.
	/// </summary>
	public static class MessageTimeoutHandler
	{
		private static MessageTimeoutOptions _options = MessageTimeoutOptions.Default;
		private static SemaphoreSlim _handlerSemaphore;
		private static long _totalTimeouts = 0;
		private static long _handlerTimeouts = 0;
		private static long _responseTimeouts = 0;

		/// <summary>
		/// Gets or sets the timeout options.
		/// </summary>
		public static MessageTimeoutOptions Options
		{
			get => _options;
			set
			{
				_options = value ?? MessageTimeoutOptions.Default;
				UpdateSemaphore();
			}
		}

		/// <summary>
		/// Event raised when a message operation times out.
		/// </summary>
		public static event EventHandler<MessageTimeoutEventArgs> OnTimeout;

		/// <summary>
		/// Gets the total number of timeouts since application start.
		/// </summary>
		public static long TotalTimeouts => Interlocked.Read(ref _totalTimeouts);

		/// <summary>
		/// Gets the number of handler timeouts since application start.
		/// </summary>
		public static long HandlerTimeouts => Interlocked.Read(ref _handlerTimeouts);

		/// <summary>
		/// Gets the number of response timeouts since application start.
		/// </summary>
		public static long ResponseTimeouts => Interlocked.Read(ref _responseTimeouts);

		static MessageTimeoutHandler()
		{
			UpdateSemaphore();
		}

		private static void UpdateSemaphore()
		{
			var newSemaphore = new SemaphoreSlim(_options.MaxConcurrentHandlers, _options.MaxConcurrentHandlers);
			var oldSemaphore = Interlocked.Exchange(ref _handlerSemaphore, newSemaphore);
			oldSemaphore?.Dispose();
		}

		/// <summary>
		/// Executes a handler with timeout protection.
		/// Returns immediately if the handler completes, or after timeout if it doesn't.
		/// </summary>
		/// <param name="handler">The handler to execute.</param>
		/// <param name="method">The method name for logging/diagnostics.</param>
		/// <param name="context">Optional context for logging.</param>
		/// <param name="timeout">Optional custom timeout. Uses HandlerTimeout if not specified.</param>
		/// <returns>The result of the operation.</returns>
		public static async Task<MessageResult> ExecuteWithTimeoutAsync(
			Func<Task> handler,
			string method,
			string context = null,
			TimeSpan? timeout = null)
		{
			var effectiveTimeout = timeout ?? _options.HandlerTimeout;
			var startTime = DateTime.UtcNow;

			// Try to acquire semaphore to limit concurrent handlers
			if (!await _handlerSemaphore.WaitAsync(TimeSpan.FromSeconds(1)))
			{
				FlutterSharpLogger.LogWarning("Too many concurrent handlers, queueing: {Method}", method);
				await _handlerSemaphore.WaitAsync();
			}

			try
			{
				using var cts = new CancellationTokenSource();
				cts.CancelAfter(effectiveTimeout);

				var handlerTask = handler();
				var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);

				var completedTask = await Task.WhenAny(handlerTask, timeoutTask);
				var elapsed = DateTime.UtcNow - startTime;

				if (completedTask == handlerTask)
				{
					// Handler completed before timeout
					cts.Cancel(); // Cancel the timeout task
					await handlerTask; // Propagate any exceptions
					return MessageResult.Succeeded(duration: elapsed);
				}
				else
				{
					// Timeout occurred
					RecordTimeout("Handler", method, effectiveTimeout, elapsed, context);
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
				FlutterSharpLogger.LogError(ex, "Handler failed: {Method}", method);
				return MessageResult.Failed(ex.Message, elapsed);
			}
			finally
			{
				_handlerSemaphore.Release();
			}
		}

		/// <summary>
		/// Executes a handler synchronously on a background thread with timeout protection.
		/// This is the main entry point for MethodChannel handlers to avoid blocking the UI thread.
		/// </summary>
		/// <param name="handler">The synchronous handler to execute.</param>
		/// <param name="method">The method name for logging/diagnostics.</param>
		/// <param name="callback">Optional callback to invoke with results.</param>
		/// <param name="context">Optional context for logging.</param>
		public static void ExecuteWithTimeout(
			Action handler,
			string method,
			Action<string> callback = null,
			string context = null)
		{
			if (!_options.RunHandlersOnBackgroundThread)
			{
				// Execute directly on current thread (legacy behavior)
				ExecuteHandlerDirect(handler, method, callback, context);
				return;
			}

			// Execute on thread pool with timeout
			Task.Run(async () =>
			{
				var result = await ExecuteWithTimeoutAsync(
					() =>
					{
						handler();
						return Task.CompletedTask;
					},
					method,
					context,
					_options.HandlerTimeout);

				if (result.TimedOut)
				{
					callback?.Invoke($"{{\"success\": false, \"error\": \"Handler timed out after {result.Duration.TotalMilliseconds:F0}ms\", \"timedOut\": true}}");
				}
				else if (!result.Success)
				{
					callback?.Invoke($"{{\"success\": false, \"error\": \"{EscapeJson(result.ErrorMessage)}\"}}");
				}
				// Note: Success response is handled by the handler itself via callback
			});
		}

		/// <summary>
		/// Executes handler directly on current thread (no timeout protection).
		/// </summary>
		private static void ExecuteHandlerDirect(
			Action handler,
			string method,
			Action<string> callback,
			string context)
		{
			try
			{
				handler();
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Handler failed: {Method}", method);
				callback?.Invoke($"{{\"success\": false, \"error\": \"{EscapeJson(ex.Message)}\"}}");
			}
		}

		/// <summary>
		/// Waits for a response with timeout.
		/// </summary>
		/// <param name="tcs">The TaskCompletionSource to wait on.</param>
		/// <param name="method">The method name for logging.</param>
		/// <param name="timeout">Optional custom timeout.</param>
		/// <returns>The result with the response or timeout.</returns>
		public static async Task<MessageResult> WaitForResponseAsync<T>(
			TaskCompletionSource<T> tcs,
			string method,
			TimeSpan? timeout = null)
		{
			var effectiveTimeout = timeout ?? _options.ResponseTimeout;
			var startTime = DateTime.UtcNow;

			using var cts = new CancellationTokenSource();
			cts.CancelAfter(effectiveTimeout);

			try
			{
				var completedTask = await Task.WhenAny(
					tcs.Task,
					Task.Delay(Timeout.Infinite, cts.Token));

				var elapsed = DateTime.UtcNow - startTime;

				if (completedTask == tcs.Task)
				{
					cts.Cancel();
					var result = await tcs.Task;
					return MessageResult.Succeeded(result?.ToString(), elapsed);
				}
				else
				{
					RecordTimeout("Response", method, effectiveTimeout, elapsed);
					tcs.TrySetCanceled();
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
				return MessageResult.Failed(ex.Message, elapsed);
			}
		}

		/// <summary>
		/// Records a timeout event.
		/// </summary>
		private static void RecordTimeout(string type, string method, TimeSpan timeout, TimeSpan elapsed, string context = null)
		{
			Interlocked.Increment(ref _totalTimeouts);

			if (type == "Handler")
			{
				Interlocked.Increment(ref _handlerTimeouts);
			}
			else if (type == "Response")
			{
				Interlocked.Increment(ref _responseTimeouts);
			}

			FlutterSharpLogger.LogWarning(
				"{Type} timeout for {Method}: exceeded {Timeout:F0}ms (elapsed: {Elapsed:F0}ms). Context: {Context}",
				type, method, timeout.TotalMilliseconds, elapsed.TotalMilliseconds, context ?? "none");

			// Raise event for subscribers
			OnTimeout?.Invoke(null, new MessageTimeoutEventArgs(type, method, timeout, elapsed, context));

			// Send to error overlay if configured
			if (_options.ShowTimeoutErrors)
			{
				FlutterManager.SendError(
					"Timeout",
					$"{type} timeout for '{method}': exceeded {timeout.TotalMilliseconds:F0}ms",
					context,
					isRecoverable: true);
			}
		}

		/// <summary>
		/// Resets timeout statistics. Useful for testing.
		/// </summary>
		public static void ResetStats()
		{
			Interlocked.Exchange(ref _totalTimeouts, 0);
			Interlocked.Exchange(ref _handlerTimeouts, 0);
			Interlocked.Exchange(ref _responseTimeouts, 0);
		}

		/// <summary>
		/// Gets timeout statistics.
		/// </summary>
		public static MessageTimeoutStats GetStats()
		{
			return new MessageTimeoutStats
			{
				TotalTimeouts = TotalTimeouts,
				HandlerTimeouts = HandlerTimeouts,
				ResponseTimeouts = ResponseTimeouts,
				MaxConcurrentHandlers = _options.MaxConcurrentHandlers,
				CurrentHandlerSlots = _handlerSemaphore?.CurrentCount ?? 0
			};
		}

		private static string EscapeJson(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			return value
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t");
		}
	}

	/// <summary>
	/// Statistics about message timeouts.
	/// </summary>
	public class MessageTimeoutStats
	{
		/// <summary>
		/// Total number of timeouts.
		/// </summary>
		public long TotalTimeouts { get; set; }

		/// <summary>
		/// Number of handler timeouts.
		/// </summary>
		public long HandlerTimeouts { get; set; }

		/// <summary>
		/// Number of response timeouts.
		/// </summary>
		public long ResponseTimeouts { get; set; }

		/// <summary>
		/// Maximum concurrent handlers allowed.
		/// </summary>
		public int MaxConcurrentHandlers { get; set; }

		/// <summary>
		/// Currently available handler slots.
		/// </summary>
		public int CurrentHandlerSlots { get; set; }

		public override string ToString()
		{
			return $"MessageTimeoutStats {{ Total: {TotalTimeouts}, Handlers: {HandlerTimeouts}, Responses: {ResponseTimeouts}, Slots: {CurrentHandlerSlots}/{MaxConcurrentHandlers} }}";
		}
	}
}
