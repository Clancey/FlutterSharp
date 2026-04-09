using System;
using System.Collections.Generic;
using System.Threading;

namespace Flutter
{
    /// <summary>
    /// A class that can be extended or mixed in that provides a change notification API using VoidCallback for notifications.
    /// </summary>
    /// <remarks>
    /// ChangeNotifier is optimized for small numbers of listeners. It is O(N) for adding and removing listeners, and O(N) for dispatch.
    /// </remarks>
    public class ChangeNotifier : Listenable, IDisposable
    {
        private List<Action> _listeners;
        private int _notificationCallStackDepth = 0;
        private int _reentrantlyRemovedListeners = 0;
        private bool _debugDisposed = false;

        // Lock object for thread safety
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new ChangeNotifier.
        /// </summary>
        public ChangeNotifier()
        {
            _listeners = new List<Action>();
        }

        /// <summary>
        /// Whether any listeners are currently registered.
        /// </summary>
        /// <remarks>
        /// Clients should not depend on this value for their behavior, because having one listener's logic
        /// change depending on whether other listeners exist can lead to fragile code.
        /// </remarks>
        public bool HasListeners
        {
            get
            {
                lock (_lock)
                {
                    return _listeners.Count > 0;
                }
            }
        }

        /// <summary>
        /// The number of listeners currently registered.
        /// </summary>
        /// <remarks>
        /// This is exposed primarily for debugging and performance monitoring purposes.
        /// </remarks>
        protected int ListenerCount
        {
            get
            {
                lock (_lock)
                {
                    return _listeners.Count;
                }
            }
        }

        /// <summary>
        /// Register a closure to be called when the object changes.
        /// </summary>
        /// <remarks>
        /// If the given closure is already registered, an additional instance is added, and must be removed the same
        /// number of times it is added before it will stop being called.
        /// </remarks>
        /// <param name="listener">The callback to invoke when the object notifies.</param>
        public void AddListener(Action listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            AssertNotDisposed();

            lock (_lock)
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Remove a previously registered closure from the list of closures that are notified when the object changes.
        /// </summary>
        /// <remarks>
        /// If the given listener is not registered, the call is ignored. If the given listener is registered more than
        /// once, only one instance is removed.
        /// </remarks>
        /// <param name="listener">The callback to remove.</param>
        public void RemoveListener(Action listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            // Allow removal even after disposal (Flutter behavior)
            lock (_lock)
            {
                if (_debugDisposed)
                    return;

                int index = _listeners.LastIndexOf(listener);
                if (index >= 0)
                {
                    if (_notificationCallStackDepth > 0)
                    {
                        // We're mid-notification, null out instead of removing to avoid modification during iteration
                        _listeners[index] = null;
                        _reentrantlyRemovedListeners++;
                    }
                    else
                    {
                        _listeners.RemoveAt(index);
                    }
                }
            }
        }

        /// <summary>
        /// Discards any resources used by the object. After this is called, the object is not in a usable state
        /// and should be discarded.
        /// </summary>
        /// <remarks>
        /// This method should only be called by the object's owner.
        /// </remarks>
        public virtual void Dispose()
        {
            lock (_lock)
            {
                if (_debugDisposed)
                    return;

                _debugDisposed = true;
                _listeners.Clear();
                _listeners = null;
            }
        }

        /// <summary>
        /// Call all the registered listeners.
        /// </summary>
        /// <remarks>
        /// Call this method whenever the object changes to notify any clients the object may have changed.
        /// Listeners that are added during this iteration will not be visited. Listeners that are removed
        /// during this iteration will not be visited after they are removed.
        ///
        /// Exceptions thrown by listeners will be caught and reported using debugPrint, but otherwise ignored.
        /// </remarks>
        protected void NotifyListeners()
        {
            AssertNotDisposed();

            List<Action> localListeners;
            lock (_lock)
            {
                if (_listeners.Count == 0)
                    return;

                _notificationCallStackDepth++;
                // Make a copy of listener references for iteration
                localListeners = new List<Action>(_listeners);
            }

            try
            {
                foreach (var listener in localListeners)
                {
                    try
                    {
                        if (listener != null)
                        {
                            listener.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Flutter catches exceptions and reports them, but continues
                        System.Diagnostics.Debug.WriteLine($"ChangeNotifier listener threw exception: {ex}");
                    }
                }
            }
            finally
            {
                lock (_lock)
                {
                    _notificationCallStackDepth--;

                    // Clean up any listeners that were removed during iteration
                    if (_notificationCallStackDepth == 0 && _reentrantlyRemovedListeners > 0)
                    {
                        _listeners.RemoveAll(l => l == null);
                        _reentrantlyRemovedListeners = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Throws if the object has been disposed.
        /// </summary>
        private void AssertNotDisposed()
        {
            if (_debugDisposed)
            {
                throw new ObjectDisposedException(GetType().Name,
                    $"A {GetType().Name} was used after being disposed.\n" +
                    "Once you have called dispose() on a ChangeNotifier, it can no longer be used.");
            }
        }
    }
}
