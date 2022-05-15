using Flutter;
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
		public static bool IsEnabled { get; set; } = true;

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
				Debug.WriteLine("You are using trying to HotReload a view that requires Parameters. Please call `HotReloadHelper.Register(this, params);` in the constructor;");
				//TODO: Notifiy that we couldnt hot reload.
				return view;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error Hotreloading type: {newViewType}");
				Debug.WriteLine(ex);
				//TODO: Notifiy that we couldnt hot reload.
				return view;
			}

		}

		static void TransferState(Widget oldView, Widget newView)
		{

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


		public static void TriggerReload() => HotReloadHandler.Reload();
	}
}
