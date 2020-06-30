using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Flutter {
	[JsonConverter (typeof (FlutterObjectConverter))]
	public abstract class FlutterObject: IDisposable {

		public FlutterObject()
		{
			init ();
		}


		void init ()
		{
		}
		protected virtual string type => this.GetType ().Name;

		internal Dictionary<string, Delegate> actions = new Dictionary<string, Delegate> ();
		internal Dictionary<string, object> properties = new Dictionary<string, object> ();
		private bool disposedValue;

		protected T GetProperty<T> (T defaultValue = default, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true)
		{
			//propertyName = camelCase (propertyName, shouldCamelCase);
			//if (properties.TryGetValue (propertyName, out var val))
			//	return (T)val;
			return defaultValue;
		}
		protected bool SetProperty<T> (T value, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true)
		{
			//propertyName = camelCase (propertyName, shouldCamelCase);
			//if (properties.TryGetValue (propertyName, out object val)) {
			//	if (EqualityComparer<T>.Default.Equals ((T)val, value))
			//		return false;
			//}
			//if(value is Delegate d) {
			//	actions [propertyName] = d;
			//	properties [propertyName] = true;
			//}else if (value == null)
			//	properties.Remove (propertyName);
			//else {
			//	properties [propertyName] = value;
			//}
			////CallPropertyChanged (propertyName, value);
			return true;
		}

		internal virtual void BeforeJSon()
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

		protected virtual void Dispose (bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		~FlutterObject ()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose (disposing: false);
		}

		public void Dispose ()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
