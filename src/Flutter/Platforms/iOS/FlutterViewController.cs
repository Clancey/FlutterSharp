using System;
using Foundation;
using Flutter.Internal;
using UIKit;
using Flutter.HotReload;
using CoreGraphics;

namespace Flutter
{
	class MyFlutterPlatformView : Internal.FlutterPlatformView
	{
		private readonly UIView view;

		public MyFlutterPlatformView(UIView view)
		{
			this.view = view;
		}
		public override UIView View => view;
	}
	class MyFlutterNativeViewFactory : NSObject, IFlutterPlatformViewFactory
	{
		public Internal.FlutterPlatformView ViewIdentifier(CGRect frame, long viewId, NSObject args)
		{
			var dictionary = args as NSDictionary<NSString, NSString>;
			var id = dictionary[(NSString)"id"].ToString();
			FlutterManager.AliveWidgets.TryGetValue(id, out var widget);
			var fv = widget as FlutterPlatformView;
			var view = fv.CreateView(frame);
			return new MyFlutterPlatformView(view);
		}

		[Export("createArgsCodec")]
		FlutterMessageCodec CreateArgsCodec => FlutterMessageCodec.SharedInstance();
	}
	public partial class FlutterViewController : Flutter.Internal.FlutterViewController, IHotReloadHandler
	{
		internal static FlutterEngine SharedEngine;
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
			this.PluginRegistry.RegistrarForPlugin("FlutterSharp").RegisterViewFactory(new MyFlutterNativeViewFactory(), "FlutterSharpNativeView");
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
