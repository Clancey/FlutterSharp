using System;
using System.Collections.Generic;
using System.Text.Json;
using Flutter.HotReload;
using Flutter.Internal;
using Flutter.Logging;
using Flutter.StateRestoration;

namespace Flutter.macOS
{
	/// <summary>
	/// macOS platform handler for Flutter content.
	/// This class hosts a Flutter engine and manages Flutter widgets on macOS.
	/// </summary>
	/// <remarks>
	/// This is the macOS equivalent of iOS's FlutterViewController and Windows' FlutterControl.
	/// It manages the Flutter engine lifecycle, method channel communication, and widget state.
	/// </remarks>
	public class FlutterMacOSViewController : IHotReloadHandler, IStateRestorable, IDisposable
	{
		private IntPtr _flutterEngine;
		private IntPtr _flutterViewController;
		private string _restorationId;
		private Widget? _widget;
		private bool _isReady;
		private bool _isInitialized;
		private bool _isDisposed;

		// Method channel name - must match other platforms
		private const string ChannelName = "com.Microsoft.FlutterSharp/Messages";

		/// <summary>
		/// Gets the native NSView handle for the Flutter content.
		/// This can be used to embed the Flutter view in a Cocoa view hierarchy.
		/// </summary>
		public IntPtr NativeViewHandle => FlutterMacOSNative.GetViewHandle(_flutterViewController);

		/// <summary>
		/// Gets whether the Flutter engine has been initialized successfully.
		/// </summary>
		public bool IsInitialized => _isInitialized;

		/// <summary>
		/// Gets whether Flutter is ready to receive widget state.
		/// </summary>
		public bool IsReady => _isReady;

		/// <summary>
		/// Creates a new FlutterMacOSViewController with a generated restoration ID.
		/// </summary>
		public FlutterMacOSViewController()
		{
			_restorationId = "FlutterMacOSViewController_" + Guid.NewGuid().ToString("N")[..8];
			Initialize();
		}

		/// <summary>
		/// Creates a FlutterMacOSViewController with a specific restoration ID for state persistence.
		/// </summary>
		/// <param name="restorationId">A unique identifier for state restoration</param>
		public FlutterMacOSViewController(string restorationId)
		{
			_restorationId = restorationId ?? "FlutterMacOSViewController_" + Guid.NewGuid().ToString("N")[..8];
			Initialize();
		}

		/// <summary>
		/// Initializes the Flutter engine and method channel.
		/// </summary>
		private void Initialize()
		{
			if (_isInitialized)
				return;

			FlutterSharpLogger.LogDebug("Initializing FlutterMacOSViewController");

			try
			{
				// Initialize the Flutter engine
				InitializeFlutterEngine();

				// Set up method channel communication
				SetupMethodChannel();

				// Register hot reload handler
				FlutterHotReloadHelper.HotReloadHandler = this;

				// Register for state restoration
				StateRestorationService.Register(this);

				_isInitialized = true;
				FlutterSharpLogger.LogInformation("FlutterMacOSViewController initialized successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to initialize FlutterMacOSViewController");
				throw;
			}
		}

		/// <summary>
		/// Initializes the Flutter engine using native macOS APIs.
		/// </summary>
		private void InitializeFlutterEngine()
		{
			FlutterSharpLogger.LogDebug("Creating Flutter engine for macOS");

			try
			{
				_flutterEngine = FlutterMacOSNative.CreateEngine();
				if (_flutterEngine == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("Flutter engine creation returned null - running in simulation mode");
					// Continue without native engine - allow development/testing
				}
				else
				{
					FlutterSharpLogger.LogDebug("Flutter engine created with handle {Handle}", _flutterEngine);

					// Run the engine
					var runResult = FlutterMacOSNative.RunEngine(_flutterEngine);
					FlutterSharpLogger.LogDebug("Flutter engine run result: {Result}", runResult);

					// Create the view controller
					_flutterViewController = FlutterMacOSNative.CreateViewController(_flutterEngine);
					FlutterSharpLogger.LogDebug("Flutter view controller created with handle {Handle}", _flutterViewController);
				}
			}
			catch (DllNotFoundException ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Flutter macOS native library not found - running in simulation mode");
				// Continue in simulation mode for development/testing
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Unexpected error initializing Flutter engine");
				throw;
			}
		}

		/// <summary>
		/// Sets up the method channel for bidirectional communication with Flutter.
		/// </summary>
		private void SetupMethodChannel()
		{
			// Set up the command sender - this sends messages to Flutter
			Communicator.SendCommand = (message) =>
			{
				FlutterSharpLogger.LogDebug("Sending command {Method} to Flutter", message.Method);
				SendMessageToFlutter(message.Method, message.Arguments);
			};

			// Set up message handler to receive messages from Flutter
			if (_flutterEngine != IntPtr.Zero)
			{
				FlutterMacOSNative.SetMessageHandler(_flutterEngine, ChannelName, OnMethodCall);
			}

			FlutterSharpLogger.LogDebug("Method channel setup complete for macOS");
		}

		/// <summary>
		/// Sends a message to Flutter via the method channel.
		/// </summary>
		private void SendMessageToFlutter(string method, string arguments)
		{
			if (_flutterEngine == IntPtr.Zero)
			{
				FlutterSharpLogger.LogDebug("Simulating send to Flutter: {Method}", method);
				// In simulation mode, we can optionally trigger the ready state
				if (!_isReady && method == "Update")
				{
					// Simulate ready state for testing
					_isReady = true;
				}
				return;
			}

			try
			{
				FlutterMacOSNative.SendMessage(_flutterEngine, ChannelName, method, arguments);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to send message {Method} to Flutter", method);
			}
		}

		/// <summary>
		/// Handles incoming method calls from Flutter.
		/// Called by the native layer when Flutter sends a message.
		/// </summary>
		private void OnMethodCall(string method, string arguments, Action<string> result)
		{
			FlutterSharpLogger.LogDebug("Received method call {Method} from Flutter", method);

			if (method == "ready")
			{
				FlutterSharpLogger.LogDebug("Ready received from Flutter, widget is null: {IsNull}", _widget == null);
				_isReady = true;

				// Send widget state if widget is already set
				if (_widget != null)
				{
					FlutterSharpLogger.LogDebug("Sending widget state from ready handler");
					FlutterManager.SendState(_widget);
				}
			}

			// Execute handler with timeout protection
			MessageTimeoutHandler.ExecuteWithTimeout(
				() => Communicator.OnCommandReceived?.Invoke((method, arguments, (response) =>
				{
					result(response);
				})),
				method,
				(errorResponse) => result(errorResponse),
				$"macOS/{method}");
		}

		#region Widget Property

		/// <summary>
		/// Gets or sets the Flutter widget to display.
		/// </summary>
		public Widget? Widget
		{
			get => _widget;
			set
			{
				FlutterSharpLogger.LogDebug("Widget setter called, isReady: {IsReady}", _isReady);

				if (_widget != null && _widget != value)
				{
					FlutterManager.UntrackWidget(_widget);
				}

				_widget = value;

				if (_isReady && _widget != null)
				{
					FlutterSharpLogger.LogDebug("Sending widget state from Widget setter");
					FlutterManager.SendState(_widget);
				}
			}
		}

		#endregion

		#region IHotReloadHandler Implementation

		/// <summary>
		/// Reloads the Flutter widget when hot reload is triggered.
		/// </summary>
		public void Reload()
		{
			if (_widget == null)
				return;

			_widget.PrepareForSending();
			if (_isReady)
			{
				FlutterManager.SendState(_widget);
			}
		}

		#endregion

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
					StateRestorationService.Unregister(_restorationId);
					_restorationId = value;
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

			if (_widget != null)
			{
				state["widgetType"] = _widget.GetType().AssemblyQualifiedName!;

				if (_widget is IStateRestorable restorableWidget)
				{
					state["widgetState"] = restorableWidget.SaveState();
				}
			}

			state["isReady"] = _isReady;

			FlutterSharpLogger.LogDebug("Saved FlutterMacOSViewController state: widgetType={WidgetType}", _widget?.GetType().Name);
			return state;
		}

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		public void RestoreState(Dictionary<string, object> state)
		{
			if (state == null)
				return;

			FlutterSharpLogger.LogDebug("Restoring FlutterMacOSViewController state");

			if (state.TryGetValue("widgetType", out var widgetTypeObj) && widgetTypeObj is string widgetTypeName)
			{
				try
				{
					var widgetType = Type.GetType(widgetTypeName);
					if (widgetType != null && typeof(Widget).IsAssignableFrom(widgetType))
					{
						var newWidget = (Widget?)Activator.CreateInstance(widgetType);

						if (newWidget is IStateRestorable restorableWidget &&
							state.TryGetValue("widgetState", out var widgetStateObj) &&
							widgetStateObj is Dictionary<string, object> widgetState)
						{
							restorableWidget.RestoreState(widgetState);
						}

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

		#region macOS State Restoration

		/// <summary>
		/// Saves state for macOS app state restoration.
		/// Call this from your NSApplication delegate when saving state.
		/// </summary>
		public string SerializeStateToJson()
		{
			try
			{
				var state = SaveState();
				return JsonSerializer.Serialize(state);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to serialize state to JSON");
				return string.Empty;
			}
		}

		/// <summary>
		/// Restores state from macOS app state restoration.
		/// Call this from your NSApplication delegate when restoring state.
		/// </summary>
		public void RestoreStateFromJson(string json)
		{
			if (string.IsNullOrEmpty(json))
				return;

			try
			{
				var state = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
				if (state != null)
				{
					RestoreState(state);
					FlutterSharpLogger.LogDebug("Restored state from JSON");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to restore state from JSON");
			}
		}

		#endregion

		#region Lifecycle Management

		/// <summary>
		/// Notifies Flutter that the application resumed.
		/// Call this when the window becomes main.
		/// </summary>
		public void NotifyResumed()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);
		}

		/// <summary>
		/// Notifies Flutter that the application is inactive.
		/// Call this when the window resigns main.
		/// </summary>
		public void NotifyInactive()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Inactive);
		}

		/// <summary>
		/// Notifies Flutter that the application is paused.
		/// Call this when the window is minimized or hidden.
		/// </summary>
		public void NotifyPaused()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Paused);
		}

		/// <summary>
		/// Notifies Flutter that the application is detaching.
		/// Call this when the window is closing.
		/// </summary>
		public void NotifyDetached()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Detached);
		}

		#endregion

		#region IDisposable Implementation

		/// <summary>
		/// Disposes the FlutterMacOSViewController and releases native resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes managed and unmanaged resources.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				// Unregister from state restoration
				StateRestorationService.Unregister(this);

				// Untrack the widget
				if (_widget != null)
				{
					FlutterManager.UntrackWidget(_widget);
				}
			}

			// Release native resources
			if (_flutterViewController != IntPtr.Zero)
			{
				FlutterMacOSNative.DestroyViewController(_flutterViewController);
				_flutterViewController = IntPtr.Zero;
			}

			if (_flutterEngine != IntPtr.Zero)
			{
				FlutterMacOSNative.DestroyEngine(_flutterEngine);
				_flutterEngine = IntPtr.Zero;
			}

			_isDisposed = true;
			FlutterSharpLogger.LogDebug("FlutterMacOSViewController disposed");
		}

		/// <summary>
		/// Finalizer to ensure native resources are released.
		/// </summary>
		~FlutterMacOSViewController()
		{
			Dispose(false);
		}

		#endregion
	}
}
