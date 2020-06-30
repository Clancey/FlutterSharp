using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Flutter.Structs;
using Newtonsoft.Json;

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

		private bool disposedValue;
		protected virtual void Dispose (bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects)
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
		
		public static implicit operator FlutterObjectStruct (FlutterObject obj) => obj.FlutterObjectStruct;
		public static implicit operator IntPtr(FlutterObject obj) => obj.FlutterObjectStruct.Handle;
	}
}
