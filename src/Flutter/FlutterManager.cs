using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json;

namespace Flutter.Internal
{
	public static class FlutterManager
	{
		static WeakDictionary<string, Widget> AliveWidgets = new WeakDictionary<string, Widget>();
		static FlutterManager()
		{
			Communicator.OnCommandReceived = OnCommandRecieved;
		}
		internal static readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		static void OnCommandRecieved((string Method, string Data, Action<string> Callback) message)
		{
			switch (message.Method)
			{
				case "Ready":
					return;
				case "Event":
					var foo = message.Data;
					Console.WriteLine(foo);
					var msg = JsonSerializer.Deserialize<EventMessage>(message.Data, serializeOptions);
					if (AliveWidgets.TryGetValue(msg.ComponentId, out var widget))
						widget?.SendEvent(msg.EventName, msg.Data, message.Callback);
					return;

			}
		}

		public static void TrackWidget(Widget widget) => AliveWidgets.Add(widget.Id, widget);
		public static void UntrackWidget(Widget widget)
		{
			var id = widget.Id;
			AliveWidgets.Remove(id);
			Communicator.SendDisposed(id);
		}
		public static async void SendState(Widget widget, string componentID = "0")
		{
			//await Task.Delay (20000);
			try
			{
				widget.PrepareForSending();
				var message = new UpdateMessage { ComponentId = componentID, Address = widget.GetForSending() };
				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand?.Invoke((message.MessageType, json));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
