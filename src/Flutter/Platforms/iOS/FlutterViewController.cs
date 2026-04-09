using System;
using System.Collections.Generic;
using Foundation;
using Flutter.Internal;
using Flutter.StateRestoration;
using UIKit;
using Flutter.HotReload;
using Flutter.Logging;
using ObjCRuntime;

namespace Flutter
{
	public partial class FlutterViewController : Flutter.Internal.iOS.FlutterViewController, IHotReloadHandler, IStateRestorable
	{
		static FlutterEngine SharedEngine;
		FlutterMethodChannel MethodChannel;
		private string _restorationId;

		static FlutterEngine CreateDefaultEngine()
		{
			FlutterSharpLogger.LogDebug("Creating FlutterEngine");
			var engine = new FlutterEngine("io.flutter",null);
			FlutterSharpLogger.LogDebug("Engine created with handle {EngineHandle}", engine.Handle);
			var result = engine.Run(null);
			FlutterSharpLogger.LogDebug("Engine.Run completed with result {RunResult}", result);
			return engine;
		}

		public FlutterViewController() : base(CreateDefaultEngine(), null, null)
		{
			_restorationId = "FlutterViewController_" + Guid.NewGuid().ToString("N").Substring(0, 8);
			init();
		}

		/// <summary>
		/// Creates a FlutterViewController with a specific restoration ID.
		/// Use this constructor when you need consistent state restoration across app launches.
		/// </summary>
		/// <param name="restorationId">A unique identifier for state restoration</param>
		public FlutterViewController(string restorationId) : base(CreateDefaultEngine(), null, null)
		{
			_restorationId = restorationId ?? "FlutterViewController_" + Guid.NewGuid().ToString("N").Substring(0, 8);
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
			FlutterSharpPluginRegistration.Register(Engine);
			MethodChannel = FlutterMethodChannel.FromNameAndMessenger("com.Microsoft.FlutterSharp/Messages", Engine.BinaryMessenger);
			Flutter.Internal.Communicator.SendCommand = (x) =>
			{
				BeginInvokeOnMainThread(() => MethodChannel.InvokeMethod(x.Method, (NSString)x.Arguments));
			};
			MethodChannel.SetMethodCaller((call, result) =>
			{
				FlutterSharpLogger.LogDebug("Received method call {MethodName}", call.Method);
				if (call.Method == "ready")
				{
					FlutterSharpLogger.LogDebug("Ready received, widget is null: {WidgetIsNull}", Widget == null);
					isReady = true;
					// Send widget state if widget is already set (handles race condition)
					if (Widget != null)
					{
						FlutterSharpLogger.LogDebug("Sending widget state from ready handler");
						FlutterManager.SendState(Widget);
					}
				}

				// Execute handler with timeout protection to avoid blocking UI thread
				MessageTimeoutHandler.ExecuteWithTimeout(
					() => Flutter.Internal.Communicator.OnCommandReceived?.Invoke((call.Method, call.Arguments.ToString(), (x) =>
					{
						result((NSString)x);
					})),
					call.Method,
					(errorResponse) => result((NSString)errorResponse),
					$"iOS/{call.Method}");
			});

			// Register for state restoration
			StateRestorationService.Register(this);
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
				FlutterSharpLogger.LogDebug("Widget setter called, isReady: {IsReady}", isReady);
				if (widget != null && widget != value)
				{
					FlutterManager.UntrackWidget(widget);
					//Cleanup, send dispose
				}
				widget = value;
				if (isReady)
				{
					FlutterSharpLogger.LogDebug("Sending widget state from Widget setter");
					FlutterManager.SendState(widget);
				}
			}
		}

		#region IStateRestorable Implementation

		/// <summary>
		/// Gets or sets the restoration ID for this view controller.
		/// </summary>
		public string RestorationId
		{
			get => _restorationId;
			set
			{
				if (_restorationId != value)
				{
					// Unregister old ID
					StateRestorationService.Unregister(_restorationId);
					_restorationId = value;
					// Register new ID
					if (!string.IsNullOrEmpty(_restorationId))
					{
						StateRestorationService.Register(this);
					}
				}
			}
		}

		/// <summary>
		/// Saves the current state for restoration.
		/// </summary>
		public Dictionary<string, object> SaveState()
		{
			var state = new Dictionary<string, object>();

			// Save the widget type so we can recreate it
			if (widget != null)
			{
				state["widgetType"] = widget.GetType().AssemblyQualifiedName;

				// If the widget supports state restoration, save its state
				if (widget is IStateRestorable restorableWidget)
				{
					state["widgetState"] = restorableWidget.SaveState();
				}
			}

			// Save ready state
			state["isReady"] = isReady;

			FlutterSharpLogger.LogDebug("Saved FlutterViewController state: widgetType={WidgetType}", widget?.GetType().Name);
			return state;
		}

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		public void RestoreState(Dictionary<string, object> state)
		{
			if (state == null)
				return;

			FlutterSharpLogger.LogDebug("Restoring FlutterViewController state");

			// Try to restore widget type and state
			if (state.TryGetValue("widgetType", out var widgetTypeObj) && widgetTypeObj is string widgetTypeName)
			{
				try
				{
					var widgetType = Type.GetType(widgetTypeName);
					if (widgetType != null && typeof(Widget).IsAssignableFrom(widgetType))
					{
						// Create a new instance of the widget
						var newWidget = (Widget)Activator.CreateInstance(widgetType);

						// Restore widget state if available
						if (newWidget is IStateRestorable restorableWidget &&
							state.TryGetValue("widgetState", out var widgetStateObj) &&
							widgetStateObj is Dictionary<string, object> widgetState)
						{
							restorableWidget.RestoreState(widgetState);
						}

						// Set the widget (this will trigger SendState if ready)
						Widget = newWidget;
						FlutterSharpLogger.LogInformation("Restored widget of type {WidgetType}", widgetType.Name);
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Failed to restore widget of type {WidgetType}", widgetTypeName);
				}
			}
		}

		#endregion

		#region iOS State Restoration

		/// <summary>
		/// Override to provide iOS-native state encoding.
		/// Called by iOS when saving state.
		/// </summary>
		public override void EncodeRestorableState(NSCoder coder)
		{
			base.EncodeRestorableState(coder);

			try
			{
				var state = SaveState();
				var json = System.Text.Json.JsonSerializer.Serialize(state);
				coder.Encode(new NSString(json), "flutterViewControllerState");
				FlutterSharpLogger.LogDebug("Encoded restorable state to NSCoder");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to encode restorable state");
			}
		}

		/// <summary>
		/// Override to restore iOS-native state.
		/// Called by iOS when restoring state.
		/// </summary>
		public override void DecodeRestorableState(NSCoder coder)
		{
			base.DecodeRestorableState(coder);

			try
			{
				if (coder.ContainsKey("flutterViewControllerState"))
				{
					var stateObj = coder.DecodeObject("flutterViewControllerState");
					if (stateObj is NSString json)
					{
						var state = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json.ToString());
						RestoreState(state);
						FlutterSharpLogger.LogDebug("Decoded restorable state from NSCoder");
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to decode restorable state");
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StateRestorationService.Unregister(this);
			}
			base.Dispose(disposing);
		}
	}

	internal static class FlutterSharpPluginRegistration
	{
		static readonly NativeHandle FlutterSharpPluginClass = Class.GetHandle("FlutterSharpPlugin");
		static readonly IntPtr RegisterWithRegistrarSelector = Selector.GetHandle("registerWithRegistrar:");
		static readonly IntPtr RegistrarForPluginSelector = Selector.GetHandle("registrarForPlugin:");

		internal static void Register(FlutterEngine engine)
		{
			ArgumentNullException.ThrowIfNull(engine);

			using var pluginKey = new NSString("FlutterSharpPlugin");
			var registrarHandle = ApiDefinition.Messaging.IntPtr_objc_msgSend_IntPtr(engine.Handle, RegistrarForPluginSelector, pluginKey.Handle);
			if (registrarHandle == IntPtr.Zero)
				throw new InvalidOperationException("FlutterSharpPlugin registrar was not available on the Flutter engine.");

			ApiDefinition.Messaging.void_objc_msgSend_IntPtr(FlutterSharpPluginClass, RegisterWithRegistrarSelector, registrarHandle);
		}
	}
}
