using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Flutter.Logging;

namespace Flutter
{
    /// <summary>
    /// Registry for all BidirectionalNotifier instances.
    /// Enables Dart to find and update notifiers by ID.
    /// </summary>
    public static class NotifierRegistry
    {
        private static readonly ConcurrentDictionary<string, IBidirectionalNotifier> _notifiers
            = new ConcurrentDictionary<string, IBidirectionalNotifier>();

        private static long _nextId = 0;

        /// <summary>
        /// Generates a unique ID for a new notifier.
        /// </summary>
        internal static string GenerateId()
        {
            return $"notifier_{Interlocked.Increment(ref _nextId)}";
        }

        /// <summary>
        /// Registers a notifier with the given ID.
        /// </summary>
        internal static void Register(string id, IBidirectionalNotifier notifier)
        {
            _notifiers[id] = notifier;
        }

        /// <summary>
        /// Unregisters a notifier.
        /// </summary>
        internal static void Unregister(string id)
        {
            _notifiers.TryRemove(id, out _);
        }

        /// <summary>
        /// Gets a notifier by ID.
        /// </summary>
        public static IBidirectionalNotifier Get(string id)
        {
            _notifiers.TryGetValue(id, out var notifier);
            return notifier;
        }

        /// <summary>
        /// Gets a typed notifier by ID.
        /// </summary>
        public static BidirectionalNotifier<T> Get<T>(string id)
        {
            return Get(id) as BidirectionalNotifier<T>;
        }

        /// <summary>
        /// Handles an incoming state notification from Dart.
        /// </summary>
        /// <param name="notifierId">The notifier ID</param>
        /// <param name="value">The new value</param>
        /// <param name="sourceWidgetId">The widget that initiated the change (to prevent circular updates)</param>
        /// <returns>True if the update was applied</returns>
        public static bool HandleStateNotify(string notifierId, object value, string sourceWidgetId)
        {
            var notifier = Get(notifierId);
            if (notifier == null)
            {
                FlutterSharpLogger.LogWarning("No notifier found with ID {NotifierId}", notifierId);
                return false;
            }

            return notifier.UpdateFromDart(value, sourceWidgetId);
        }

        /// <summary>
        /// Gets the count of registered notifiers.
        /// </summary>
        public static int Count => _notifiers.Count;

        /// <summary>
        /// Clears all registered notifiers. Use for testing or app reset.
        /// </summary>
        public static void Clear()
        {
            _notifiers.Clear();
        }
    }

    /// <summary>
    /// Interface for type-erased access to BidirectionalNotifier.
    /// </summary>
    public interface IBidirectionalNotifier : IDisposable
    {
        /// <summary>
        /// The unique ID of this notifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Updates the value from Dart side.
        /// </summary>
        /// <param name="value">The new value (may need type conversion)</param>
        /// <param name="sourceWidgetId">The widget that initiated the change</param>
        /// <returns>True if the update was applied</returns>
        bool UpdateFromDart(object value, string sourceWidgetId);

        /// <summary>
        /// Gets the current value as object.
        /// </summary>
        object GetValue();

        /// <summary>
        /// Gets the type name for serialization.
        /// </summary>
        string ValueTypeName { get; }
    }

    /// <summary>
    /// A ValueNotifier that supports two-way binding with Dart.
    /// Changes made in Dart are synchronized back to C#, and vice versa.
    /// </summary>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    /// <remarks>
    /// Usage:
    /// <code>
    /// var counter = new BidirectionalNotifier&lt;int&gt;(0);
    /// // Use with ListenableBuilder or Consumer
    /// // When Dart UI changes the value, this notifier is automatically updated
    /// </code>
    /// </remarks>
    public class BidirectionalNotifier<T> : ValueNotifier<T>, IBidirectionalNotifier
    {
        private readonly string _id;
        private readonly object _updateLock = new object();
        private bool _isUpdatingFromDart = false;
        private string _lastSourceWidgetId;

        /// <summary>
        /// Event raised when the value is updated from Dart side.
        /// </summary>
        public event EventHandler<StateChangedFromDartEventArgs<T>> OnValueChangedFromDart;

        /// <summary>
        /// Gets the unique ID of this notifier.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Gets the type name for serialization.
        /// </summary>
        public string ValueTypeName => GetValueTypeName(typeof(T));

        /// <summary>
        /// Creates a new BidirectionalNotifier with the specified initial value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public BidirectionalNotifier(T value) : base(value)
        {
            _id = NotifierRegistry.GenerateId();
            NotifierRegistry.Register(_id, this);

            // Subscribe to our own changes to broadcast to Dart
            AddListener(OnValueChanged);
        }

        /// <summary>
        /// Creates a new BidirectionalNotifier with a specific ID (for reconnecting after app restart).
        /// </summary>
        /// <param name="id">The specific ID to use.</param>
        /// <param name="value">The initial value.</param>
        public BidirectionalNotifier(string id, T value) : base(value)
        {
            _id = id;
            NotifierRegistry.Register(_id, this);
            AddListener(OnValueChanged);
        }

        /// <summary>
        /// Called when the value changes (either from C# or Dart).
        /// Broadcasts the change to Dart if it originated from C#.
        /// </summary>
        private void OnValueChanged()
        {
            // Don't broadcast back to Dart if the change came from Dart
            if (_isUpdatingFromDart)
                return;

            BroadcastToDart();
        }

        /// <summary>
        /// Sends the current value to Dart via the platform channel.
        /// </summary>
        private void BroadcastToDart()
        {
            if (Internal.Communicator.SendCommand == null)
                return;

            try
            {
                var message = new StateChangedMessage
                {
                    ComponentId = "0", // Global state
                    NotifierId = _id,
                    Value = SerializeValue(Value),
                    ValueType = ValueTypeName,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var json = JsonSerializer.Serialize(message);
                Internal.Communicator.SendCommand.Invoke((message.MessageType, json));
            }
            catch (Exception ex)
            {
                FlutterSharpLogger.LogError(ex, "Error broadcasting to Dart");
            }
        }

        /// <summary>
        /// Updates the value from Dart side.
        /// </summary>
        public bool UpdateFromDart(object value, string sourceWidgetId)
        {
            lock (_updateLock)
            {
                try
                {
                    _isUpdatingFromDart = true;
                    _lastSourceWidgetId = sourceWidgetId;

                    var convertedValue = ConvertValue(value);
                    var oldValue = Value;

                    Value = convertedValue;

                    // Raise the Dart-specific event
                    OnValueChangedFromDart?.Invoke(this, new StateChangedFromDartEventArgs<T>(
                        oldValue, convertedValue, sourceWidgetId));

                    return true;
                }
                catch (Exception ex)
                {
                    FlutterSharpLogger.LogError(ex, "Error updating from Dart");
                    return false;
                }
                finally
                {
                    _isUpdatingFromDart = false;
                }
            }
        }

        /// <summary>
        /// Gets the current value as object.
        /// </summary>
        public object GetValue() => Value;

        /// <summary>
        /// Converts an incoming value from Dart to the appropriate type.
        /// </summary>
        private T ConvertValue(object value)
        {
            if (value == null)
                return default(T);

            if (value is T typedValue)
                return typedValue;

            if (value is JsonElement jsonElement)
            {
                return DeserializeJsonElement(jsonElement);
            }

            // Try standard conversion
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Deserializes a JsonElement to the target type.
        /// </summary>
        private T DeserializeJsonElement(JsonElement element)
        {
            var type = typeof(T);

            if (type == typeof(string))
                return (T)(object)element.GetString();

            if (type == typeof(int))
                return (T)(object)element.GetInt32();

            if (type == typeof(long))
                return (T)(object)element.GetInt64();

            if (type == typeof(double))
                return (T)(object)element.GetDouble();

            if (type == typeof(float))
                return (T)(object)(float)element.GetDouble();

            if (type == typeof(bool))
                return (T)(object)element.GetBoolean();

            if (type == typeof(DateTime))
                return (T)(object)DateTime.Parse(element.GetString());

            // For complex types, use full deserialization
            return JsonSerializer.Deserialize<T>(element.GetRawText());
        }

        /// <summary>
        /// Serializes a value for transmission to Dart.
        /// </summary>
        private object SerializeValue(T value)
        {
            if (value == null)
                return null;

            // Primitives can be sent directly
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return value;

            // For complex types, serialize to JSON string
            return JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Gets the type name for serialization purposes.
        /// </summary>
        private static string GetValueTypeName(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(DateTime)) return "datetime";
            return type.FullName;
        }

        /// <summary>
        /// Disposes the notifier and unregisters it from the registry.
        /// </summary>
        public override void Dispose()
        {
            RemoveListener(OnValueChanged);
            NotifierRegistry.Unregister(_id);
            base.Dispose();
        }
    }

    /// <summary>
    /// Event args for when a value is updated from Dart.
    /// </summary>
    public class StateChangedFromDartEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The previous value.
        /// </summary>
        public T OldValue { get; }

        /// <summary>
        /// The new value.
        /// </summary>
        public T NewValue { get; }

        /// <summary>
        /// The widget ID that initiated the change, if known.
        /// </summary>
        public string SourceWidgetId { get; }

        public StateChangedFromDartEventArgs(T oldValue, T newValue, string sourceWidgetId)
        {
            OldValue = oldValue;
            NewValue = newValue;
            SourceWidgetId = sourceWidgetId;
        }
    }
}
