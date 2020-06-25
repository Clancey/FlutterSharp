﻿using System;
using System.Runtime.CompilerServices;
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
			var message = new UpdateMessage { ComponentId = componentID, State = widget };
			var json = JsonConvert.SerializeObject (message);
			Communicator.SendCommand?.Invoke ((message.MessageType, json));
		}
	}
}
