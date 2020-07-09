using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flutter.Internal {
	public static class FlutterManager {
		static WeakDictionary<string, Widget> AliveWidgets = new WeakDictionary<string, Widget>();
		static FlutterManager ()
		{
			Communicator.OnCommandReceived = OnCommandRecieved;
		}

		static void OnCommandRecieved ((string Method, string Data, Action<string> Callback) message)
		{
			switch (message.Method) {
			case "Ready":
				return;
			case "Event":
				var msg = JsonConvert.DeserializeObject<EventMessage> (message.Data);
				if(AliveWidgets.TryGetValue (msg.ComponentId, out var widget))
					widget?.SendEvent (msg.EventName,msg.Data,message.Callback);
				return;

			}
		}

		public static void TrackWidget (Widget widget) => AliveWidgets.Add(widget.Id,widget);
		public static void UntrackWidget(Widget widget)
		{
			var id = widget.Id;
			AliveWidgets.Remove (id);
			Communicator.SendDisposed (id);
		}
		public static void SendState (Widget widget, string componentID = "0")
		{
			//await Task.Delay (20000);
			try {
				widget.PrepareForSending ();
				var message = new UpdateMessage { ComponentId = componentID, Address = widget };
				var json = JsonConvert.SerializeObject (message);
				Communicator.SendCommand?.Invoke ((message.MessageType, json));
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
}
