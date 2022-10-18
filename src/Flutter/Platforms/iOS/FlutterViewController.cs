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
			var engine = new FlutterEngine("io.flutter",null);
			engine.Run(null);
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

				if (call.Method == "ready" && Widget != null)
				{
					isReady = true;
					FlutterManager.SendState(Widget);
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
			if (isReady)
				FlutterManager.SendState(Widget);

		}

		private Widget widget;
		public Widget Widget
		{
			get => widget;
			set
			{
				if (widget != null && widget != value)
				{
					FlutterManager.UntrackWidget(widget);
					//Cleanup, send dispose
				}
				widget = value;
				if (isReady)
					FlutterManager.SendState(widget);
			}
		}
	}
}
