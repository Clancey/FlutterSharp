using System;
using System.Collections.Concurrent;
using System.Threading;
using Flutter.Logging;

namespace Flutter
{
    /// <summary>
    /// Event args for callback errors
    /// </summary>
    public class CallbackErrorEventArgs : EventArgs
    {
        public long CallbackId { get; }
        public Exception Exception { get; }
        public object[] Arguments { get; }
        public bool Handled { get; set; }

        public CallbackErrorEventArgs(long callbackId, Exception exception, object[] arguments)
        {
            CallbackId = callbackId;
            Exception = exception;
            Arguments = arguments;
            Handled = false;
        }
    }

    /// <summary>
    /// Registry for managing callbacks that are passed from C# to Dart.
    /// Each callback is assigned a unique ID that can be passed across the FFI boundary.
    /// </summary>
    public static class CallbackRegistry
    {
        private static long _nextId = 1;
        private static readonly ConcurrentDictionary<long, Delegate> _callbacks = new();
        private static long _totalInvocations = 0;
        private static long _failedInvocations = 0;

        /// <summary>
        /// Event raised when a callback throws an exception.
        /// Set Handled = true to suppress the exception.
        /// </summary>
        public static event EventHandler<CallbackErrorEventArgs> OnCallbackError;

        /// <summary>
        /// Gets the total number of callback invocations since startup.
        /// </summary>
        public static long TotalInvocations => _totalInvocations;

        /// <summary>
        /// Gets the total number of failed callback invocations since startup.
        /// </summary>
        public static long FailedInvocations => _failedInvocations;

        /// <summary>
        /// Registers a callback and returns a unique ID.
        /// </summary>
        /// <param name="callback">The delegate to register</param>
        /// <returns>A unique callback ID that can be passed to Dart</returns>
        public static long Register(Delegate callback)
        {
            if (callback == null)
                return 0;

            var id = Interlocked.Increment(ref _nextId);
            _callbacks[id] = callback;
            return id;
        }

        /// <summary>
        /// Tries to get a callback by ID.
        /// </summary>
        /// <param name="id">The callback ID</param>
        /// <param name="callback">The callback if found</param>
        /// <returns>True if callback was found</returns>
        public static bool TryGet(long id, out Delegate callback)
        {
            return _callbacks.TryGetValue(id, out callback);
        }

        /// <summary>
        /// Invokes a registered callback by ID.
        /// </summary>
        /// <param name="id">The callback ID</param>
        /// <param name="args">Arguments to pass to the callback</param>
        public static void Invoke(long id, params object[] args)
        {
            Interlocked.Increment(ref _totalInvocations);

            if (_callbacks.TryGetValue(id, out var callback))
            {
                try
                {
                    callback.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _failedInvocations);
                    var errorArgs = new CallbackErrorEventArgs(id, ex, args);
                    OnCallbackError?.Invoke(null, errorArgs);

                    if (!errorArgs.Handled)
                    {
                        FlutterSharpLogger.LogError(ex, "Error invoking callback {CallbackId}", id);
                    }
                }
            }
            else
            {
                FlutterSharpLogger.LogWarning("Callback {CallbackId} not found in registry", id);
            }
        }

        /// <summary>
        /// Invokes a void callback (Action) by ID. More efficient than dynamic invoke.
        /// </summary>
        /// <param name="id">The callback ID</param>
        public static void InvokeVoid(long id)
        {
            Interlocked.Increment(ref _totalInvocations);

            if (_callbacks.TryGetValue(id, out var callback) && callback is Action action)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    HandleInvokeError(id, ex, Array.Empty<object>());
                }
            }
            else if (callback != null)
            {
                // Fall back to dynamic invoke for non-Action delegates
                try
                {
                    callback.DynamicInvoke();
                }
                catch (Exception ex)
                {
                    HandleInvokeError(id, ex, Array.Empty<object>());
                }
            }
            else
            {
                FlutterSharpLogger.LogWarning("Callback {CallbackId} not found in registry", id);
            }
        }

        /// <summary>
        /// Invokes a typed callback (Action&lt;T&gt;) by ID. More efficient than dynamic invoke.
        /// </summary>
        /// <typeparam name="T">The argument type</typeparam>
        /// <param name="id">The callback ID</param>
        /// <param name="arg">The argument to pass</param>
        public static void Invoke<T>(long id, T arg)
        {
            Interlocked.Increment(ref _totalInvocations);

            if (_callbacks.TryGetValue(id, out var callback))
            {
                if (callback is Action<T> typedAction)
                {
                    try
                    {
                        typedAction(arg);
                    }
                    catch (Exception ex)
                    {
                        HandleInvokeError(id, ex, new object[] { arg });
                    }
                }
                else
                {
                    // Fall back to dynamic invoke for type mismatches
                    try
                    {
                        callback.DynamicInvoke(arg);
                    }
                    catch (Exception ex)
                    {
                        HandleInvokeError(id, ex, new object[] { arg });
                    }
                }
            }
            else
            {
                FlutterSharpLogger.LogWarning("Callback {CallbackId} not found in registry", id);
            }
        }

        /// <summary>
        /// Invokes a callback with two typed arguments.
        /// </summary>
        public static void Invoke<T1, T2>(long id, T1 arg1, T2 arg2)
        {
            Interlocked.Increment(ref _totalInvocations);

            if (_callbacks.TryGetValue(id, out var callback))
            {
                if (callback is Action<T1, T2> typedAction)
                {
                    try
                    {
                        typedAction(arg1, arg2);
                    }
                    catch (Exception ex)
                    {
                        HandleInvokeError(id, ex, new object[] { arg1, arg2 });
                    }
                }
                else
                {
                    try
                    {
                        callback.DynamicInvoke(arg1, arg2);
                    }
                    catch (Exception ex)
                    {
                        HandleInvokeError(id, ex, new object[] { arg1, arg2 });
                    }
                }
            }
            else
            {
                FlutterSharpLogger.LogWarning("Callback {CallbackId} not found in registry", id);
            }
        }

        /// <summary>
        /// Helper to handle invoke errors consistently
        /// </summary>
        private static void HandleInvokeError(long id, Exception ex, object[] args)
        {
            Interlocked.Increment(ref _failedInvocations);
            var errorArgs = new CallbackErrorEventArgs(id, ex, args);
            OnCallbackError?.Invoke(null, errorArgs);

            if (!errorArgs.Handled)
            {
                FlutterSharpLogger.LogError(ex, "Error invoking callback {CallbackId}", id);

                // Send error to Flutter overlay for developer visibility
                var message = ex.Message;
                var stackTrace = ex.StackTrace;

                // Unwrap TargetInvocationException
                if (ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null)
                {
                    message = tie.InnerException.Message;
                    stackTrace = tie.InnerException.StackTrace;
                }

                Internal.FlutterManager.SendError(
                    errorType: "CallbackError",
                    message: message,
                    stackTrace: stackTrace,
                    callbackId: id,
                    isRecoverable: false);
            }
        }

        /// <summary>
        /// Unregisters a callback and removes it from the registry.
        /// </summary>
        /// <param name="id">The callback ID to unregister</param>
        public static void Unregister(long id)
        {
            _callbacks.TryRemove(id, out _);
        }

        /// <summary>
        /// Gets the number of registered callbacks (for debugging).
        /// </summary>
        public static int Count => _callbacks.Count;

        /// <summary>
        /// Clears all registered callbacks (use with caution).
        /// </summary>
        public static void Clear()
        {
            _callbacks.Clear();
        }

        /// <summary>
        /// Resets statistics counters.
        /// </summary>
        public static void ResetStatistics()
        {
            Interlocked.Exchange(ref _totalInvocations, 0);
            Interlocked.Exchange(ref _failedInvocations, 0);
        }
    }
}
