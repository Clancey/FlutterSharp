using System;
using Foundation;
using Flutter.Internal;
using UIKit;
using Flutter.HotReload;

namespace Flutter
{
	public partial class FlutterViewController : Flutter.Internal.iOS.FlutterViewController, IHotReloadHandler
	{
		static FlutterEngine SharedEngine;
		FlutterMethodChannel MethodChannel;
		static FlutterEngine CreateDefaultEngine()
		{
			Console.WriteLine("[FlutterViewController] Creating FlutterEngine...");
			var engine = new FlutterEngine("io.flutter",null);
			Console.WriteLine($"[FlutterViewController] Engine created: {engine.Handle}");
			var result = engine.Run(null);
			Console.WriteLine($"[FlutterViewController] Engine.Run result: {result}");
			return engine;
		}
		public FlutterViewController() : base(CreateDefaultEngine(), null, null)
		{
			init();
		}
		bool isReady = false;
		bool isInitalized;
		void init()
		{

			if (isInitalized)
				return;
			FlutterHotReloadHelper.HotReloadHandler = this;
			isInitalized = true;
			GeneratedPluginRegistrant.Register(Engine);
			MethodChannel = FlutterMethodChannel.FromNameAndMessenger("com.Microsoft.FlutterSharp/Messages", Engine.BinaryMessenger);
			Flutter.Internal.Communicator.SendCommand = (x) => MethodChannel.InvokeMethod(x.Method, (NSString)x.Arguments);
			MethodChannel.SetMethodCaller((call, result) =>
			{
				Console.WriteLine($"[FlutterViewController] Received method call: {call.Method}");
				if (call.Method == "ready")
				{
					Console.WriteLine($"[FlutterViewController] Ready received! Widget is null: {Widget == null}");
					isReady = true;
					// Send widget state if widget is already set (handles race condition)
					if (Widget != null)
					{
						Console.WriteLine($"[FlutterViewController] Sending widget state from ready handler");
						FlutterManager.SendState(Widget);
					}
				}
				Flutter.Internal.Communicator.OnCommandReceived?.Invoke((call.Method, call.Arguments.ToString(), (x) =>
				{
					result((NSString)x);
				}
				));
			});

		}

		public void Reload()
		{
			if (Widget == null)
				return;
			Widget.PrepareForSending(); 
			if (isReady)
				FlutterManager.SendState(Widget);

		}

		private Widget widget;
		public Widget Widget
		{
			get => widget;
			set
			{
				Console.WriteLine($"[FlutterViewController] Widget setter called. isReady: {isReady}");
				if (widget != null && widget != value)
				{
					FlutterManager.UntrackWidget(widget);
					//Cleanup, send dispose
				}
				widget = value;
				if (isReady)
				{
					Console.WriteLine($"[FlutterViewController] Sending widget state from Widget setter");
					FlutterManager.SendState(widget);
				}
			}
		}
	}
}
