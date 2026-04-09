using Flutter;
using Flutter.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Flutter.HotReload
{

	[AttributeUsage(AttributeTargets.Method)]
	public class OnHotReloadAttribute : Attribute
	{

	}

	public static class HotReloadExtensions
	{
		public static List<MethodInfo> GetOnHotReloadMethods(this Type type) => getOnHotReloadMethods(type).Distinct(new ReflectionMethodComparer()).ToList();

		static IEnumerable<MethodInfo> getOnHotReloadMethods(Type type, bool isSubclass = false)
		{
			var flags = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic;
			if (isSubclass)
				flags = BindingFlags.Static | BindingFlags.NonPublic;
			var foos = type.GetMethods(flags).Where(x => x.GetCustomAttributes(typeof(OnHotReloadAttribute), true).Any()).ToList();
			foreach (var foo in foos)
				yield return foo;

			if (type.BaseType != null)
				foreach (var foo in getOnHotReloadMethods(type.BaseType, true))
					yield return foo;
		}

		class ReflectionMethodComparer : IEqualityComparer<MethodInfo>
		{
			public bool Equals(MethodInfo? g1, MethodInfo? g2) => g1?.MethodHandle == g2?.MethodHandle;

			public int GetHashCode(MethodInfo obj) => obj.MethodHandle.GetHashCode();
		}

	}


	internal interface IHotReloadHandler
	{
		void Reload();
	}

	public static class FlutterHotReloadHelper
	{
		internal static IHotReloadHandler HotReloadHandler { get; set; }
		public static void Init()
		{
			IsEnabled = true;
		}

		public static void Reset()
		{
			replacedViews.Clear();
		}
		public static bool IsEnabled { get; set; }

		public static Widget GetReplacedView(Widget view)
		{
			if (!IsEnabled)
				return view;

			var viewType = view.GetType();
			if (!replacedViews.TryGetValue(viewType.FullName!, out var newViewType) || viewType == newViewType)
				return view;

			try
			{
				//TODO: Add in a way to use IoC and DI
				var newView = (Widget)Activator.CreateInstance(newViewType);
				TransferState(view, newView);
				return newView;
			}
			catch (MissingMethodException)
			{
				var errorMessage = $"Hot reload requires parameterless constructor. Call HotReloadHelper.Register(this, params) for {newViewType.Name}";
				Debug.WriteLine(errorMessage);
				FlutterManager.SendHotReloadFailure(errorMessage, newViewType.Name);
				return view;
			}
			catch (Exception ex)
			{
				var errorMessage = $"Error hot reloading {newViewType.Name}: {ex.Message}";
				Debug.WriteLine($"Error Hotreloading type: {newViewType}");
				Debug.WriteLine(ex);
				FlutterManager.SendHotReloadFailure(errorMessage, newViewType.Name);
				return view;
			}

		}

		/// <summary>
		/// Transfers state from the old widget to the new widget during hot reload.
		/// This copies property values using reflection, excluding read-only and system properties.
		/// </summary>
		/// <param name="oldView">The old widget instance being replaced</param>
		/// <param name="newView">The new widget instance to receive state</param>
		static void TransferState(Widget oldView, Widget newView)
		{
			if (oldView == null || newView == null)
				return;

			var oldType = oldView.GetType();
			var newType = newView.GetType();

			// Properties to skip during transfer (system properties that shouldn't be copied)
			var skipProperties = new HashSet<string>
			{
				"Id",           // Unique identifier per instance
				"backingStruct" // FFI struct is recreated
			};

			// Get all properties from the old type (including inherited ones)
			var properties = oldType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var property in properties)
			{
				try
				{
					// Skip properties that shouldn't be transferred
					if (skipProperties.Contains(property.Name))
						continue;

					// Skip read-only properties (no setter)
					if (!property.CanWrite || !property.CanRead)
						continue;

					// Skip indexers
					if (property.GetIndexParameters().Length > 0)
						continue;

					// Try to find the corresponding property in the new type
					var newProperty = newType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (newProperty == null || !newProperty.CanWrite)
						continue;

					// Check that types are compatible
					if (!newProperty.PropertyType.IsAssignableFrom(property.PropertyType))
						continue;

					// Get the value from the old widget and set it on the new widget
					var value = property.GetValue(oldView);
					newProperty.SetValue(newView, value);
				}
				catch (Exception ex)
				{
					// Log but don't fail the entire transfer for one property
					Debug.WriteLine($"Failed to transfer property '{property.Name}': {ex.Message}");
				}
			}

			// Also transfer private fields for state that might not have property accessors
			TransferFields(oldView, newView, oldType, newType);
		}

		/// <summary>
		/// Transfers field values from the old widget to the new widget.
		/// This handles private state fields that don't have property accessors.
		/// </summary>
		static void TransferFields(Widget oldView, Widget newView, Type oldType, Type newType)
		{
			// Fields to skip during transfer
			var skipFields = new HashSet<string>
			{
				"backingStruct",
				"disposed",
				"_allocatedChildrenArrays",
				"_registeredCallbackIds",
				"<Id>k__BackingField" // Auto-property backing field for Id
			};

			// Walk up the inheritance chain to transfer fields from all levels
			var currentOldType = oldType;
			var currentNewType = newType;

			while (currentOldType != null && currentOldType != typeof(object))
			{
				var fields = currentOldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

				foreach (var field in fields)
				{
					try
					{
						// Skip system fields
						if (skipFields.Contains(field.Name))
							continue;

						// Skip compiler-generated fields for auto-properties we've already handled
						if (field.Name.StartsWith("<") && field.Name.EndsWith(">k__BackingField"))
						{
							// Extract property name and check if we should skip
							var propName = field.Name.Substring(1, field.Name.IndexOf('>') - 1);
							if (propName == "Id")
								continue;
						}

						// Find corresponding field in new type hierarchy
						var newField = FindField(currentNewType, field.Name);
						if (newField == null)
							continue;

						// Check type compatibility
						if (!newField.FieldType.IsAssignableFrom(field.FieldType))
							continue;

						var value = field.GetValue(oldView);
						newField.SetValue(newView, value);
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Failed to transfer field '{field.Name}': {ex.Message}");
					}
				}

				currentOldType = currentOldType.BaseType;
				currentNewType = currentNewType?.BaseType;
			}
		}

		/// <summary>
		/// Finds a field by name in the type hierarchy.
		/// </summary>
		static FieldInfo? FindField(Type? type, string fieldName)
		{
			while (type != null && type != typeof(object))
			{
				var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				if (field != null)
					return field;
				type = type.BaseType;
			}
			return null;
		}

		static Dictionary<string, Type> replacedViews = new Dictionary<string, Type>();
		static Dictionary<string, List<KeyValuePair<Type, Type>>> replacedHandlers = new Dictionary<string, List<KeyValuePair<Type, Type>>>();
		public static void RegisterReplacedView(string oldViewType, Type newViewType)
		{
			if (!IsEnabled)
				return;

			Action<MethodInfo> executeStaticMethod = (method) => {
				try
				{
					method?.Invoke(null, null);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Error calling {method.Name} on type: {newViewType}");
					Debug.WriteLine(ex);
					//TODO: Notifiy that we couldnt execute OnHotReload for the Method;
				}
			};

			var onHotReloadMethods = newViewType.GetOnHotReloadMethods();
			onHotReloadMethods.ForEach(x => executeStaticMethod(x));

			if (typeof(Widget).IsAssignableFrom(newViewType))
				replacedViews[oldViewType] = newViewType;
		}


		/// <summary>
		/// Triggers a hot reload and sends a notification to the Flutter UI.
		/// </summary>
		public static void TriggerReload()
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				HotReloadHandler?.Reload();
				stopwatch.Stop();

				// Send success notification to Flutter
				FlutterManager.SendHotReloadNotification(
					widgetType: null,
					durationMs: (int)stopwatch.ElapsedMilliseconds);
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				Debug.WriteLine($"Hot reload failed: {ex}");

				// Send failure notification to Flutter
				FlutterManager.SendHotReloadFailure(ex.Message);
			}
		}
	}
}
