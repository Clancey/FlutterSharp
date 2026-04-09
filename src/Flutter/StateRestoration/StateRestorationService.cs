using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.StateRestoration
{
	/// <summary>
	/// Manages state restoration for FlutterSharp widgets across app launches.
	/// Supports saving widget state to NSUserActivity (iOS) or similar mechanisms.
	/// </summary>
	public static class StateRestorationService
	{
		private static readonly object _lock = new object();
		private static Dictionary<string, IStateRestorable> _restorables = new Dictionary<string, IStateRestorable>();
		private static Dictionary<string, Dictionary<string, object>> _pendingRestorations = new Dictionary<string, Dictionary<string, object>>();
		private static bool _isRestoring = false;

		/// <summary>
		/// Gets or sets whether state restoration is enabled.
		/// </summary>
		public static bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Gets whether state restoration is currently in progress.
		/// </summary>
		public static bool IsRestoring => _isRestoring;

		/// <summary>
		/// Event raised when state restoration completes.
		/// </summary>
		public static event EventHandler<StateRestorationCompletedEventArgs> OnRestorationCompleted;

		/// <summary>
		/// Event raised before state is saved.
		/// </summary>
		public static event EventHandler<StateRestorationEventArgs> OnStateSaving;

		/// <summary>
		/// Event raised after state is saved.
		/// </summary>
		public static event EventHandler<StateRestorationEventArgs> OnStateSaved;

		/// <summary>
		/// Registers a restorable object for state management.
		/// </summary>
		/// <param name="restorable">The object to register</param>
		public static void Register(IStateRestorable restorable)
		{
			if (!IsEnabled || restorable == null || string.IsNullOrEmpty(restorable.RestorationId))
				return;

			lock (_lock)
			{
				_restorables[restorable.RestorationId] = restorable;

				// Check if there's pending state to restore
				if (_pendingRestorations.TryGetValue(restorable.RestorationId, out var pendingState))
				{
					try
					{
						FlutterSharpLogger.LogDebug("Restoring pending state for {RestorationId}", restorable.RestorationId);
						restorable.RestoreState(pendingState);
						_pendingRestorations.Remove(restorable.RestorationId);
					}
					catch (Exception ex)
					{
						FlutterSharpLogger.LogError(ex, "Failed to restore state for {RestorationId}", restorable.RestorationId);
					}
				}
			}
		}

		/// <summary>
		/// Unregisters a restorable object.
		/// </summary>
		/// <param name="restorable">The object to unregister</param>
		public static void Unregister(IStateRestorable restorable)
		{
			if (restorable == null || string.IsNullOrEmpty(restorable.RestorationId))
				return;

			lock (_lock)
			{
				_restorables.Remove(restorable.RestorationId);
			}
		}

		/// <summary>
		/// Unregisters a restorable object by its restoration ID.
		/// </summary>
		/// <param name="restorationId">The restoration ID</param>
		public static void Unregister(string restorationId)
		{
			if (string.IsNullOrEmpty(restorationId))
				return;

			lock (_lock)
			{
				_restorables.Remove(restorationId);
			}
		}

		/// <summary>
		/// Saves all registered restorable objects' state.
		/// Returns a serializable dictionary that can be persisted.
		/// </summary>
		/// <returns>Dictionary containing all saved state</returns>
		public static Dictionary<string, object> SaveAllState()
		{
			if (!IsEnabled)
				return new Dictionary<string, object>();

			OnStateSaving?.Invoke(null, new StateRestorationEventArgs());

			var allState = new Dictionary<string, object>();

			lock (_lock)
			{
				foreach (var kvp in _restorables)
				{
					try
					{
						var state = kvp.Value.SaveState();
						if (state != null && state.Count > 0)
						{
							allState[kvp.Key] = state;
							FlutterSharpLogger.LogDebug("Saved state for {RestorationId} with {Count} properties", kvp.Key, state.Count);
						}
					}
					catch (Exception ex)
					{
						FlutterSharpLogger.LogError(ex, "Failed to save state for {RestorationId}", kvp.Key);
					}
				}
			}

			OnStateSaved?.Invoke(null, new StateRestorationEventArgs { SavedCount = allState.Count });
			return allState;
		}

		/// <summary>
		/// Saves all state to a JSON string for persistence.
		/// </summary>
		/// <returns>JSON string containing all saved state</returns>
		public static string SaveAllStateToJson()
		{
			var state = SaveAllState();
			return JsonSerializer.Serialize(state, FlutterManager.serializeOptions);
		}

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		/// <param name="allState">Dictionary containing saved state</param>
		public static void RestoreAllState(Dictionary<string, object> allState)
		{
			if (!IsEnabled || allState == null)
				return;

			_isRestoring = true;
			var restoredCount = 0;
			var pendingCount = 0;

			try
			{
				lock (_lock)
				{
					foreach (var kvp in allState)
					{
						// Convert JsonElement to Dictionary if needed
						Dictionary<string, object> state;
						if (kvp.Value is JsonElement jsonElement)
						{
							state = ConvertJsonElementToDictionary(jsonElement);
						}
						else if (kvp.Value is Dictionary<string, object> dict)
						{
							state = dict;
						}
						else
						{
							FlutterSharpLogger.LogWarning("Unknown state type for {RestorationId}: {Type}", kvp.Key, kvp.Value?.GetType().Name);
							continue;
						}

						// Try to restore immediately if registered
						if (_restorables.TryGetValue(kvp.Key, out var restorable))
						{
							try
							{
								restorable.RestoreState(state);
								restoredCount++;
								FlutterSharpLogger.LogDebug("Restored state for {RestorationId}", kvp.Key);
							}
							catch (Exception ex)
							{
								FlutterSharpLogger.LogError(ex, "Failed to restore state for {RestorationId}", kvp.Key);
							}
						}
						else
						{
							// Save for later when the restorable registers
							_pendingRestorations[kvp.Key] = state;
							pendingCount++;
							FlutterSharpLogger.LogDebug("Queued pending restoration for {RestorationId}", kvp.Key);
						}
					}
				}

				FlutterSharpLogger.LogInformation("State restoration complete: {Restored} restored, {Pending} pending", restoredCount, pendingCount);
			}
			finally
			{
				_isRestoring = false;
				OnRestorationCompleted?.Invoke(null, new StateRestorationCompletedEventArgs
				{
					RestoredCount = restoredCount,
					PendingCount = pendingCount
				});
			}
		}

		/// <summary>
		/// Restores state from a JSON string.
		/// </summary>
		/// <param name="json">JSON string containing saved state</param>
		public static void RestoreAllStateFromJson(string json)
		{
			if (string.IsNullOrEmpty(json))
				return;

			try
			{
				var state = JsonSerializer.Deserialize<Dictionary<string, object>>(json, FlutterManager.serializeOptions);
				RestoreAllState(state);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to parse state restoration JSON");
			}
		}

		/// <summary>
		/// Clears all pending restorations.
		/// </summary>
		public static void ClearPendingRestorations()
		{
			lock (_lock)
			{
				_pendingRestorations.Clear();
			}
		}

		/// <summary>
		/// Gets the count of registered restorables.
		/// </summary>
		public static int RegisteredCount
		{
			get
			{
				lock (_lock)
				{
					return _restorables.Count;
				}
			}
		}

		/// <summary>
		/// Gets the count of pending restorations.
		/// </summary>
		public static int PendingCount
		{
			get
			{
				lock (_lock)
				{
					return _pendingRestorations.Count;
				}
			}
		}

		/// <summary>
		/// Helper method to automatically save/restore properties marked with [Restorable].
		/// </summary>
		/// <param name="target">The object to save state from</param>
		/// <returns>Dictionary of saved state</returns>
		public static Dictionary<string, object> SaveAnnotatedState(object target)
		{
			var state = new Dictionary<string, object>();
			if (target == null)
				return state;

			var type = target.GetType();
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.GetCustomAttribute<RestorableAttribute>() != null);

			foreach (var member in members)
			{
				var attr = member.GetCustomAttribute<RestorableAttribute>();
				var key = attr.Key ?? member.Name;

				object value = null;
				if (member is PropertyInfo prop && prop.CanRead)
				{
					value = prop.GetValue(target);
				}
				else if (member is FieldInfo field)
				{
					value = field.GetValue(target);
				}

				if (value != null)
				{
					// Handle special types that need conversion
					if (IsSerializable(value))
					{
						state[key] = value;
					}
				}
			}

			return state;
		}

		/// <summary>
		/// Helper method to automatically restore properties marked with [Restorable].
		/// </summary>
		/// <param name="target">The object to restore state to</param>
		/// <param name="state">The saved state dictionary</param>
		public static void RestoreAnnotatedState(object target, Dictionary<string, object> state)
		{
			if (target == null || state == null)
				return;

			var type = target.GetType();
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.GetCustomAttribute<RestorableAttribute>() != null);

			foreach (var member in members)
			{
				var attr = member.GetCustomAttribute<RestorableAttribute>();
				var key = attr.Key ?? member.Name;

				if (!state.TryGetValue(key, out var value))
					continue;

				try
				{
					if (member is PropertyInfo prop && prop.CanWrite)
					{
						var converted = ConvertValue(value, prop.PropertyType);
						prop.SetValue(target, converted);
					}
					else if (member is FieldInfo field)
					{
						var converted = ConvertValue(value, field.FieldType);
						field.SetValue(target, converted);
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogWarning("Failed to restore property {Key}: {Error}", key, ex.Message);
				}
			}
		}

		/// <summary>
		/// Converts a JsonElement to a Dictionary<string, object>.
		/// </summary>
		private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
		{
			var dict = new Dictionary<string, object>();

			if (element.ValueKind != JsonValueKind.Object)
				return dict;

			foreach (var prop in element.EnumerateObject())
			{
				dict[prop.Name] = ConvertJsonElement(prop.Value);
			}

			return dict;
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
					return ConvertJsonElementToDictionary(element);
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
		/// Checks if a value can be serialized.
		/// </summary>
		private static bool IsSerializable(object value)
		{
			if (value == null)
				return true;

			var type = value.GetType();

			// Primitives and strings
			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
				type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
				type == typeof(Guid) || type == typeof(TimeSpan))
				return true;

			// Nullable primitives
			if (Nullable.GetUnderlyingType(type) != null)
				return true;

			// Enums
			if (type.IsEnum)
				return true;

			// Arrays and lists of serializable types
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				return elementType.IsPrimitive || elementType == typeof(string);
			}

			if (type.IsGenericType)
			{
				var genericDef = type.GetGenericTypeDefinition();
				if (genericDef == typeof(List<>) || genericDef == typeof(Dictionary<,>))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Converts a value to the specified type.
		/// </summary>
		private static object ConvertValue(object value, Type targetType)
		{
			if (value == null)
				return null;

			if (targetType.IsAssignableFrom(value.GetType()))
				return value;

			// Handle JsonElement
			if (value is JsonElement jsonElement)
			{
				return ConvertJsonElementToType(jsonElement, targetType);
			}

			// Handle nullable types
			var underlyingType = Nullable.GetUnderlyingType(targetType);
			if (underlyingType != null)
			{
				if (value == null)
					return null;
				return Convert.ChangeType(value, underlyingType);
			}

			// Handle enums
			if (targetType.IsEnum)
			{
				if (value is string strVal)
					return Enum.Parse(targetType, strVal, true);
				if (value is int intVal)
					return Enum.ToObject(targetType, intVal);
			}

			// Default conversion
			return Convert.ChangeType(value, targetType);
		}

		/// <summary>
		/// Converts a JsonElement to a specific type.
		/// </summary>
		private static object ConvertJsonElementToType(JsonElement element, Type targetType)
		{
			if (targetType == typeof(string))
				return element.GetString();
			if (targetType == typeof(int) || targetType == typeof(int?))
				return element.GetInt32();
			if (targetType == typeof(long) || targetType == typeof(long?))
				return element.GetInt64();
			if (targetType == typeof(double) || targetType == typeof(double?))
				return element.GetDouble();
			if (targetType == typeof(float) || targetType == typeof(float?))
				return element.GetSingle();
			if (targetType == typeof(bool) || targetType == typeof(bool?))
				return element.GetBoolean();
			if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
				return element.GetDateTime();
			if (targetType.IsEnum)
			{
				if (element.ValueKind == JsonValueKind.String)
					return Enum.Parse(targetType, element.GetString(), true);
				if (element.ValueKind == JsonValueKind.Number)
					return Enum.ToObject(targetType, element.GetInt32());
			}

			return ConvertJsonElement(element);
		}
	}

	/// <summary>
	/// Event args for state restoration events.
	/// </summary>
	public class StateRestorationEventArgs : EventArgs
	{
		/// <summary>
		/// Number of states saved (for OnStateSaved).
		/// </summary>
		public int SavedCount { get; set; }
	}

	/// <summary>
	/// Event args for state restoration completion.
	/// </summary>
	public class StateRestorationCompletedEventArgs : EventArgs
	{
		/// <summary>
		/// Number of states successfully restored.
		/// </summary>
		public int RestoredCount { get; set; }

		/// <summary>
		/// Number of states pending restoration (object not yet registered).
		/// </summary>
		public int PendingCount { get; set; }
	}
}
