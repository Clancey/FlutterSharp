using System;
using System.Threading.Tasks;
using Flutter.Internal;

namespace Flutter {
	public class FlutterApp {
		static FlutterMethodChannel methodChannel;
		public Widget Widget { get; private set; }
		public Task<int> Run (Widget widget)
		{
			Widget = widget;
			InitCommunication ();
			methodChannel.SetMethodCaller ((channel, method, arguments, result) => {
				Console.WriteLine("*******");
				Console.WriteLine("*******");
				Console.WriteLine("*******");
				Console.WriteLine($"We got a message!!!! {method} - {arguments}");
				if (method == "ready" && Widget != null) {
					FlutterManager.SendState (Widget);
				}
				Communicator.OnCommandReceived?.Invoke ((method, arguments, (x) => result.Complete (x)));
			});
			return FlutterPi.RunApp ();
		}

		static void InitCommunication ()
		{
			if (methodChannel != null)
				return;
			var resp = FlutterPi.Init ();
			methodChannel = FlutterMethodChannel.Create ("com.Microsoft.FlutterSharp/Messages");
			Flutter.Internal.Communicator.SendCommand = (x) => methodChannel.InvokeMethod (x.Method, x.Arguments);
			Console.WriteLine("Communication is setup!");
		}
	}
}
