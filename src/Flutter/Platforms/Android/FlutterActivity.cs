using System;
using Android.OS;
using Flutter.HotReload;
using Flutter.Internal;
using IO.Flutter.Embedding.Engine;
using IO.Flutter.Plugin.Common;
using IO.Flutter.Plugins;

namespace Flutter
{
	public class FlutterActivity : IO.Flutter.Embedding.Android.FlutterActivity, MethodChannel.IMethodCallHandler, IHotReloadHandler
	{

		private Widget widget;
		public Widget Widget
		{
			get => widget;
			set
			{
				if (widget != null && widget != value)
				{
					FlutterManager.UntrackWidget(Widget);
					//Cleanup, send dispose
				}
				widget = value;
				if (isReady)
					FlutterManager.SendState(widget);
			}
		}
		bool isReady;
		MethodChannel channel;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			FlutterHotReloadHelper.HotReloadHandler = this;
			//GeneratedPluginRegistrant.RegisterWith (FlutterEngine);
			channel = new MethodChannel(FlutterEngine.DartExecutor, "com.Microsoft.FlutterSharp/Messages");
			Flutter.Internal.Communicator.SendCommand = (x) => channel.InvokeMethod(x.Method, x.Arguments);
			channel.SetMethodCallHandler(this);
		}
		public void OnMethodCall(MethodCall call, MethodChannel.IResult result)
		{
			if (call.Method == "ready")
			{
				isReady = true;
				FlutterManager.SendState(Widget);
			}

			Flutter.Internal.Communicator.OnCommandReceived?.Invoke((call.Method, call.Arguments().ToString(), (x) => {
				result.Success(x);
			}
			));
		}

		public void Reload()
		{
			if (Widget == null)
				return;
			Widget.PrepareForSending();
			if (isReady)
				FlutterManager.SendState(Widget);
		}
	}
}
