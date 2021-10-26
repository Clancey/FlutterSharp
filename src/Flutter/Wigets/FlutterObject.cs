using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Flutter.Structs;
using System.Text.Json;

namespace Flutter {
	public abstract class FlutterObject: IDisposable {

		public FlutterObject()
		{
			init ();
		}

		protected FlutterObjectStruct FlutterObjectStruct { get; private set; }
		protected T GetBackingStruct<T>() where T : FlutterObjectStruct => (T)FlutterObjectStruct;
		void init ()
		{
			FlutterObjectStruct = CreateBackingStruct();
			FlutterObjectStruct.WidgetType = FlutterType;
		}
		protected virtual FlutterObjectStruct CreateBackingStruct() => new FlutterObjectStruct();
		protected virtual string FlutterType => this.GetType ().Name;

		internal Dictionary<string, Delegate> actions = new Dictionary<string, Delegate> ();
		protected T GetAction<T> (T defaultValue = default, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true) where T : Delegate
		{
			if (actions.TryGetValue (propertyName, out var val))
				return (T)val;
			return defaultValue;
		}
		protected bool SetAction<T> (T value, [CallerMemberName] string propertyName = "", bool shouldCamelCase = true) where T : Delegate
		{
			if (actions.TryGetValue (propertyName, out var val)) {
				if (EqualityComparer<T>.Default.Equals ((T)val, value))
					return false;
			}
			actions [propertyName] = value;
			return true;
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
				string resultString = "";
				if(result != null) {
					if (result is FlutterObject fo)
						resultString = ((long)fo).ToString();
					else
						resultString = JsonSerializer.Serialize(result);
				}
				returnAction?.Invoke (resultString);
			}
		}



		private bool disposedValue;
		protected virtual void Dispose (bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					actions.Clear ();
				}

				FlutterObjectStruct.Dispose();
				FlutterObjectStruct = null;
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
		
		public static implicit operator FlutterObjectStruct (FlutterObject obj) => obj?.FlutterObjectStruct;
		public static implicit operator IntPtr(FlutterObject obj) => obj?.FlutterObjectStruct?.Handle ?? IntPtr.Zero;
		public static implicit operator long (FlutterObject obj) => ((IntPtr)obj).ToInt64();
	}
}
