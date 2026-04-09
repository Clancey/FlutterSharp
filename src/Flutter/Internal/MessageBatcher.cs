using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flutter;
using Flutter.Logging;

namespace Flutter.Internal
{
    /// <summary>
    /// Batches widget update messages to reduce MethodChannel overhead.
    /// Collects updates within a time window and sends them as a single batched message.
    /// </summary>
    public static class MessageBatcher
    {
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<string, PendingUpdate> _pendingUpdates = new();
        private static Timer _flushTimer;
        private static bool _isEnabled = true;
        private static int _batchWindowMs = 16; // Default: ~1 frame at 60fps
        private static int _pendingCount = 0;
        private static long _totalBatchesSent = 0;
        private static long _totalMessagesBatched = 0;
        private static long _totalMessagesUnbatched = 0;

        /// <summary>
        /// Gets or sets whether batching is enabled. When disabled, messages are sent immediately.
        /// </summary>
        public static bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (!value)
                    {
                        // Flush any pending updates when disabling
                        FlushNow();
                    }
                    FlutterSharpLogger.LogDebug("MessageBatcher enabled: {Enabled}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the batch window in milliseconds.
        /// Updates are collected for this duration before being sent as a batch.
        /// Default is 16ms (~1 frame at 60fps).
        /// </summary>
        public static int BatchWindowMs
        {
            get => _batchWindowMs;
            set
            {
                if (value < 1) value = 1;
                if (value > 1000) value = 1000;
                _batchWindowMs = value;
                FlutterSharpLogger.LogDebug("MessageBatcher window: {WindowMs}ms", value);
            }
        }

        /// <summary>
        /// Gets statistics about batching performance.
        /// </summary>
        public static BatchingStats GetStats() => new BatchingStats
        {
            TotalBatchesSent = Interlocked.Read(ref _totalBatchesSent),
            TotalMessagesBatched = Interlocked.Read(ref _totalMessagesBatched),
            TotalMessagesUnbatched = Interlocked.Read(ref _totalMessagesUnbatched),
            PendingUpdates = _pendingCount,
            IsEnabled = _isEnabled,
            BatchWindowMs = _batchWindowMs
        };

        /// <summary>
        /// Resets batching statistics.
        /// </summary>
        public static void ResetStats()
        {
            Interlocked.Exchange(ref _totalBatchesSent, 0);
            Interlocked.Exchange(ref _totalMessagesBatched, 0);
            Interlocked.Exchange(ref _totalMessagesUnbatched, 0);
        }

        /// <summary>
        /// Queues a widget update for batched sending.
        /// If batching is disabled, sends immediately.
        /// </summary>
        /// <param name="componentId">The component ID.</param>
        /// <param name="address">The widget struct pointer address.</param>
        /// <param name="widgetType">Optional widget type name for debugging.</param>
        public static void QueueUpdate(string componentId, long address, string widgetType = null)
        {
            if (!_isEnabled)
            {
                // Batching disabled - send immediately
                SendImmediate(componentId, address);
                Interlocked.Increment(ref _totalMessagesUnbatched);
                return;
            }

            var update = new PendingUpdate
            {
                ComponentId = componentId,
                Address = address,
                WidgetType = widgetType,
                QueuedAt = DateTime.UtcNow
            };

            // Add or update (newer updates for same component replace older ones)
            _pendingUpdates.AddOrUpdate(componentId, update, (_, _) => update);
            Interlocked.Increment(ref _pendingCount);

            EnsureTimerStarted();
        }

        /// <summary>
        /// Forces an immediate flush of all pending updates.
        /// Call this when you need updates to be sent without waiting for the batch window.
        /// </summary>
        public static void FlushNow()
        {
            FlushPendingUpdates();
        }

        private static void EnsureTimerStarted()
        {
            lock (_lock)
            {
                if (_flushTimer == null)
                {
                    _flushTimer = new Timer(
                        _ => FlushPendingUpdates(),
                        null,
                        Timeout.Infinite,
                        Timeout.Infinite);
                }

                // Re-arm the one-shot timer for every queued update. Without this,
                // only the first post-startup flush fires and subsequent SetState()
                // calls remain queued forever.
                _flushTimer.Change(_batchWindowMs, Timeout.Infinite);
            }
        }

        private static void FlushPendingUpdates()
        {
            // Stop timer
            _flushTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            if (_pendingUpdates.IsEmpty)
            {
                return;
            }

            // Collect all pending updates
            var updates = new List<UpdateEntry>();
            var keysToRemove = new List<string>();

            foreach (var kvp in _pendingUpdates)
            {
                updates.Add(new UpdateEntry
                {
                    ComponentId = kvp.Value.ComponentId,
                    Address = kvp.Value.Address
                });
                keysToRemove.Add(kvp.Key);
            }

            // Remove collected updates
            foreach (var key in keysToRemove)
            {
                _pendingUpdates.TryRemove(key, out _);
            }

            Interlocked.Exchange(ref _pendingCount, _pendingUpdates.Count);

            if (updates.Count == 0)
            {
                return;
            }

            // Send batched or single update
            if (updates.Count == 1)
            {
                // Single update - send as regular UpdateComponent for compatibility
                SendImmediate(updates[0].ComponentId, updates[0].Address);
                Interlocked.Increment(ref _totalMessagesUnbatched);
            }
            else
            {
                // Multiple updates - send as batch
                SendBatch(updates);
                Interlocked.Add(ref _totalMessagesBatched, updates.Count);
                Interlocked.Increment(ref _totalBatchesSent);
            }

            FlutterSharpLogger.LogDebug("MessageBatcher flushed {Count} updates", updates.Count);
        }

        private static void SendImmediate(string componentId, long address)
        {
            if (Communicator.SendCommand == null) return;

            try
            {
                var message = new UpdateMessage
                {
                    ComponentId = componentId,
                    Address = address
                };
                var json = JsonSerializer.Serialize(message);
                Communicator.SendCommand.Invoke((message.MessageType, json));
            }
            catch (Exception ex)
            {
                FlutterSharpLogger.LogError(ex, "Error sending immediate update");
            }
        }

        private static void SendBatch(List<UpdateEntry> updates)
        {
            if (Communicator.SendCommand == null) return;

            try
            {
                var message = new BatchedUpdateMessage
                {
                    Updates = updates
                };
                var json = JsonSerializer.Serialize(message);
                Communicator.SendCommand.Invoke((message.MessageType, json));
            }
            catch (Exception ex)
            {
                FlutterSharpLogger.LogError(ex, "Error sending batched updates");
            }
        }

        private class PendingUpdate
        {
            public string ComponentId { get; set; }
            public long Address { get; set; }
            public string WidgetType { get; set; }
            public DateTime QueuedAt { get; set; }
        }
    }

    /// <summary>
    /// A single update entry within a batch.
    /// </summary>
    public class UpdateEntry
    {
        [JsonPropertyName("componentId")]
        public string ComponentId { get; set; }

        [JsonPropertyName("address")]
        public long Address { get; set; }
    }

    /// <summary>
    /// Message containing multiple widget updates.
    /// </summary>
    public class BatchedUpdateMessage : Message
    {
        public override string MessageType => "BatchedUpdate";

        [JsonPropertyName("updates")]
        public List<UpdateEntry> Updates { get; set; } = new();
    }

    /// <summary>
    /// Statistics about message batching performance.
    /// </summary>
    public class BatchingStats
    {
        /// <summary>
        /// Total number of batched messages sent.
        /// </summary>
        public long TotalBatchesSent { get; set; }

        /// <summary>
        /// Total number of individual updates that were batched together.
        /// </summary>
        public long TotalMessagesBatched { get; set; }

        /// <summary>
        /// Total number of updates sent immediately (unbatched).
        /// </summary>
        public long TotalMessagesUnbatched { get; set; }

        /// <summary>
        /// Current number of pending updates waiting to be flushed.
        /// </summary>
        public int PendingUpdates { get; set; }

        /// <summary>
        /// Whether batching is currently enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The batch window in milliseconds.
        /// </summary>
        public int BatchWindowMs { get; set; }

        /// <summary>
        /// The batching efficiency ratio (batched / total).
        /// Higher is better (more messages were batched together).
        /// </summary>
        public double BatchEfficiency
        {
            get
            {
                var total = TotalMessagesBatched + TotalMessagesUnbatched;
                return total > 0 ? (double)TotalMessagesBatched / total : 0;
            }
        }

        /// <summary>
        /// Average messages per batch.
        /// </summary>
        public double AverageMessagesPerBatch
        {
            get
            {
                return TotalBatchesSent > 0 ? (double)TotalMessagesBatched / TotalBatchesSent : 0;
            }
        }

        public override string ToString()
        {
            return $"Batching: {(IsEnabled ? "On" : "Off")}, " +
                   $"Window: {BatchWindowMs}ms, " +
                   $"Batches: {TotalBatchesSent}, " +
                   $"Batched: {TotalMessagesBatched}, " +
                   $"Unbatched: {TotalMessagesUnbatched}, " +
                   $"Efficiency: {BatchEfficiency:P1}, " +
                   $"Avg/Batch: {AverageMessagesPerBatch:F1}";
        }
    }
}
