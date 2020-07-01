using System;
using Foundation;
using Flutter.Internal;
using UIKit;

namespace Flutter {
	public partial class FlutterViewController : Flutter.Internal.iOS.FlutterViewController {
		static FlutterEngine SharedEngine;
		FlutterMethodChannel MethodChannel;
		static FlutterEngine CreateDefaultEngine ()
		{
			var engine = new FlutterEngine ("io.flutter", null);
			engine.Run (null);
			return engine;
		}
		public FlutterViewController () : base (CreateDefaultEngine (), null, null)
		{
			init ();
		}

		bool isInitalized;
		void init ()
		{

			if (isInitalized)
				return;
			isInitalized = true;
			GeneratedPluginRegistrant.Register (Engine);
			MethodChannel = FlutterMethodChannel.FromNameAndMessenger ("com.Microsoft.FlutterSharp/Messages", Engine.BinaryMessenger);
			Flutter.Internal.Communicator.SendCommand = (x) => MethodChannel.InvokeMethod (x.Method, (NSString)x.Arguments);
			MethodChannel.SetMethodCaller ((call, result) => {

				if (call.Method == "ready" && Widget != null) {

					FlutterManager.SendState (Widget);
				}
				Flutter.Internal.Communicator.OnCommandReceived?.Invoke ((call.Method, call.Arguments.ToString (), (x) => {
					result ((NSString)x);
				}
				));
			});

		}

		private Widget widget;
		public Widget Widget {
			get => widget;
			set {
				if (widget != null) {
					FlutterManager.UntrackWidget (Widget);
					//Cleanup, send dispose
				}
				widget = value;
				FlutterManager.SendState (widget);
			}
		}
	}
}
