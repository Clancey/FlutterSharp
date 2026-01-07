using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Flutter
{
    /// <summary>
    /// Registry for managing callbacks that are passed from C# to Dart.
    /// Each callback is assigned a unique ID that can be passed across the FFI boundary.
    /// </summary>
    public static class CallbackRegistry
    {
        private static long _nextId = 1;
        private static readonly ConcurrentDictionary<long, Delegate> _callbacks = new();

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
        /// Invokes a registered callback by ID.
        /// </summary>
        /// <param name="id">The callback ID</param>
        /// <param name="args">Arguments to pass to the callback</param>
        public static void Invoke(long id, params object[] args)
        {
            if (_callbacks.TryGetValue(id, out var callback))
            {
                try
                {
                    callback.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking callback {id}: {ex}");
                }
            }
            else
            {
                Console.WriteLine($"Callback {id} not found in registry");
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
    }
}
