using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Flutter {
	[JsonConverter (typeof (FlutterObjectConverter))]
	public abstract class FlutterObject {
		public FlutterObject()
		{
			init ();
		}


		void init ()
		{
			properties ["type"] = type;
		}
		protected virtual string type => this.GetType ().Name;

		internal Dictionary<string, Delegate> actions = new Dictionary<string, Delegate> ();
		internal Dictionary<string, object> properties = new Dictionary<string, object> ();
		protected T GetProperty<T> (T defaultValue = default, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true)
		{
			propertyName = camelCase (propertyName, shouldCamelCase);
			if (properties.TryGetValue (propertyName, out var val))
				return (T)val;
			return defaultValue;
		}
		protected bool SetProperty<T> (T value, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true)
		{
			propertyName = camelCase (propertyName, shouldCamelCase);
			if (properties.TryGetValue (propertyName, out object val)) {
				if (EqualityComparer<T>.Default.Equals ((T)val, value))
					return false;
			}
			if(value is Delegate d) {
				actions [propertyName] = d;
				properties [propertyName] = true;
			}else if (value == null)
				properties.Remove (propertyName);
			else {
				properties [propertyName] = value;
			}
			//CallPropertyChanged (propertyName, value);
			return true;
		}

		internal virtual void BeforeJSon()
		{

		}

		static string camelCase (string org, bool shouldCamelCase) => shouldCamelCase ? char.ToLower (org [0]) + org.Substring (1) : org;

		internal void UpdatePropertyFromDart(string key, object value)
		{

		}
		internal void SendEvent(string key, object value, Action<string> returnAction)
		{
			if (!actions.TryGetValue (key, out var action))
				return;
			Delegate foo = new Func<bool, object> ((o) => {
				return true;
			});
			object result = null;
			if (value != null)
				result = action.DynamicInvoke (value);
			else
				result = action.DynamicInvoke ();
			if (returnAction != null) {
				var json = result == null ? "" : JsonConvert.SerializeObject (result);
				returnAction?.Invoke (json);
			}
		}
	}
}
