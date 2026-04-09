using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using Flutter.HotReload;
using Flutter.Internal;
using Flutter.Logging;
using Flutter.StateRestoration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;

namespace Flutter.Windows
{
	/// <summary>
	/// Windows platform handler for Flutter content.
	/// This control hosts a Flutter engine and renders Flutter widgets on Windows.
	/// </summary>
	public class FlutterControl : Grid, IHotReloadHandler, IStateRestorable, IDisposable
	{
		private IntPtr _flutterEngine;
		private IntPtr _flutterViewController;
		private IntPtr _hwnd;
		private string _restorationId;
		private Widget? _widget;
		private bool _isReady;
		private bool _isInitialized;
		private bool _isDisposed;
		private int _pointerDeviceId;

		// DPI and sizing state
		private uint _currentDpi = FlutterWindowsNative.DefaultDpi;
		private double _dpiScaleFactor = 1.0;
		private int _physicalWidth;
		private int _physicalHeight;
		private double _logicalWidth;
		private double _logicalHeight;

		// GPU rendering
		private FlutterWindowsRendering? _renderer;

		// Method channel name - must match other platforms
		private const string ChannelName = "com.Microsoft.FlutterSharp/Messages";

		// Win32 message constants for input
		private const uint WM_KEYDOWN = 0x0100;
		private const uint WM_KEYUP = 0x0101;
		private const uint WM_CHAR = 0x0102;
		private const uint WM_SYSKEYDOWN = 0x0104;
		private const uint WM_SYSKEYUP = 0x0105;
		private const uint WM_MOUSEMOVE = 0x0200;
		private const uint WM_LBUTTONDOWN = 0x0201;
		private const uint WM_LBUTTONUP = 0x0202;
		private const uint WM_RBUTTONDOWN = 0x0204;
		private const uint WM_RBUTTONUP = 0x0205;
		private const uint WM_MBUTTONDOWN = 0x0207;
		private const uint WM_MBUTTONUP = 0x0208;
		private const uint WM_MOUSEWHEEL = 0x020A;
		private const uint WM_MOUSEHWHEEL = 0x020E;
		private const uint WM_POINTERDOWN = 0x0246;
		private const uint WM_POINTERUP = 0x0247;
		private const uint WM_POINTERUPDATE = 0x0245;
		private const uint WM_DPICHANGED = 0x02E0;

		/// <summary>
		/// Creates a new FlutterControl with a generated restoration ID.
		/// </summary>
		public FlutterControl()
		{
			_restorationId = "FlutterControl_" + Guid.NewGuid().ToString("N")[..8];
			Initialize();
		}

		/// <summary>
		/// Creates a FlutterControl with a specific restoration ID for state persistence.
		/// </summary>
		/// <param name="restorationId">A unique identifier for state restoration</param>
		public FlutterControl(string restorationId)
		{
			_restorationId = restorationId ?? "FlutterControl_" + Guid.NewGuid().ToString("N")[..8];
			Initialize();
		}

		#region DPI and Size Properties

		/// <summary>
		/// Gets the current DPI for this control (96 = 100%, 120 = 125%, etc.).
		/// </summary>
		public uint CurrentDpi => _currentDpi;

		/// <summary>
		/// Gets the current DPI scale factor (1.0 = 100%, 1.25 = 125%, etc.).
		/// </summary>
		public double DpiScaleFactor => _dpiScaleFactor;

		/// <summary>
		/// Gets the current physical width in pixels.
		/// </summary>
		public int PhysicalWidth => _physicalWidth;

		/// <summary>
		/// Gets the current physical height in pixels.
		/// </summary>
		public int PhysicalHeight => _physicalHeight;

		/// <summary>
		/// Gets the current logical width in device-independent pixels.
		/// </summary>
		public double LogicalWidth => _logicalWidth;

		/// <summary>
		/// Gets the current logical height in device-independent pixels.
		/// </summary>
		public double LogicalHeight => _logicalHeight;

		/// <summary>
		/// Gets the GPU renderer for this control.
		/// </summary>
		/// <remarks>
		/// May be null if GPU rendering is not available or failed to initialize.
		/// Use RenderingBackend property to check the active rendering mode.
		/// </remarks>
		public FlutterWindowsRendering? Renderer => _renderer;

		/// <summary>
		/// Gets the active rendering backend.
		/// </summary>
		public RenderingBackend RenderingBackend => _renderer?.ActiveBackend ?? RenderingBackend.None;

		/// <summary>
		/// Gets the current rendering statistics.
		/// </summary>
		public RenderingStats? RenderingStats => _renderer?.Stats;

		/// <summary>
		/// Event raised when DPI changes (e.g., moving window between monitors).
		/// </summary>
		public event EventHandler<DpiChangedEventArgs>? DpiChanged;

		/// <summary>
		/// Event raised when the size changes.
		/// </summary>
		public event EventHandler<SizeChangedEventArgs>? FlutterSizeChanged;

		#endregion

		/// <summary>
		/// Initializes the Flutter engine and method channel.
		/// </summary>
		private void Initialize()
		{
			if (_isInitialized)
				return;

			FlutterSharpLogger.LogDebug("Initializing FlutterControl on Windows");

			try
			{
				// Initialize the Flutter engine
				InitializeFlutterEngine();

				// Set up method channel communication
				SetupMethodChannel();

				// Set up input event handlers
				SetupInputHandlers();

				// Register hot reload handler
				FlutterHotReloadHelper.HotReloadHandler = this;

				// Register for state restoration
				StateRestorationService.Register(this);

				_isInitialized = true;
				FlutterSharpLogger.LogInformation("FlutterControl initialized successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to initialize FlutterControl");
				throw;
			}
		}

		/// <summary>
		/// Initializes the Flutter engine using native Windows APIs.
		/// </summary>
		private void InitializeFlutterEngine()
		{
			FlutterSharpLogger.LogDebug("Creating Flutter engine for Windows");

			// Try to initialize the native Flutter engine
			// This requires the Flutter Windows embedder library (flutter_windows.dll)
			try
			{
				_flutterEngine = FlutterWindowsNative.CreateEngine();
				if (_flutterEngine == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("Flutter engine creation returned null - running in simulation mode");
					// Continue without native engine - allow development/testing
				}
				else
				{
					FlutterSharpLogger.LogDebug("Flutter engine created with handle {Handle}", _flutterEngine);

					// Create the view controller
					_flutterViewController = FlutterWindowsNative.CreateViewController(_flutterEngine);
					FlutterSharpLogger.LogDebug("Flutter view controller created with handle {Handle}", _flutterViewController);

					// Get the window handle for input forwarding
					if (_flutterViewController != IntPtr.Zero)
					{
						_hwnd = FlutterWindowsNative.GetViewHandle(_flutterViewController);
						FlutterSharpLogger.LogDebug("Flutter view HWND obtained: {Handle}", _hwnd);

						// Query initial DPI after obtaining HWND
						if (_hwnd != IntPtr.Zero)
						{
							_currentDpi = FlutterWindowsNative.GetDpi(_hwnd);
							_dpiScaleFactor = _currentDpi / (double)FlutterWindowsNative.DefaultDpi;
							FlutterSharpLogger.LogDebug(
								"Initial DPI detected: {Dpi} (scale factor: {ScaleFactor:F2})",
								_currentDpi, _dpiScaleFactor);

							// Initialize GPU rendering
							InitializeRendering();
						}
					}
				}
			}
			catch (DllNotFoundException ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Flutter Windows native library not found - running in simulation mode");
				// This is expected if the flutter_windows.dll is not present
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

			// Set up the message handler for receiving messages from Flutter
			if (_flutterEngine != IntPtr.Zero)
			{
				FlutterWindowsNative.SetMessageHandler(_flutterEngine, ChannelName, OnMethodCall);
				FlutterSharpLogger.LogDebug("Native message handler registered for channel {Channel}", ChannelName);
			}
			else
			{
				FlutterSharpLogger.LogDebug("Running in simulation mode - no native message handler registered");
			}

			FlutterSharpLogger.LogDebug("Method channel setup complete for Windows");
		}

		/// <summary>
		/// Initializes GPU rendering with ANGLE/Direct3D.
		/// </summary>
		private void InitializeRendering()
		{
			if (_hwnd == IntPtr.Zero)
			{
				FlutterSharpLogger.LogWarning("Cannot initialize rendering: No valid HWND");
				return;
			}

			try
			{
				_renderer = new FlutterWindowsRendering();

				// Determine initial size
				int initialWidth = _physicalWidth > 0 ? _physicalWidth : 800;
				int initialHeight = _physicalHeight > 0 ? _physicalHeight : 600;

				// Initialize with automatic backend selection (ANGLE -> D3D11 -> Software)
				if (_renderer.Initialize(_hwnd, initialWidth, initialHeight, RenderingMode.Automatic))
				{
					FlutterSharpLogger.LogInformation(
						"GPU rendering initialized. Backend: {Backend}, Feature Level: {FeatureLevel}",
						_renderer.ActiveBackend,
						_renderer.Direct3D?.FeatureLevel.ToString() ?? "N/A");

					// Subscribe to rendering events
					_renderer.FrameRendered += OnFrameRendered;
					_renderer.BackendChanged += OnBackendChanged;
				}
				else
				{
					FlutterSharpLogger.LogWarning("GPU rendering initialization failed - Flutter will use internal rendering");
					_renderer.Dispose();
					_renderer = null;
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception during rendering initialization");
				_renderer?.Dispose();
				_renderer = null;
			}
		}

		/// <summary>
		/// Called when a frame has been rendered.
		/// </summary>
		private void OnFrameRendered(object? sender, FrameRenderedEventArgs e)
		{
			// Log frame timing occasionally for diagnostics
			if (e.FrameNumber % 300 == 0) // Every ~5 seconds at 60fps
			{
				FlutterSharpLogger.LogDebug(
					"Rendering stats - Frame #{Frame}, FPS: {FPS:F1}, Last frame: {FrameTime:F2}ms",
					e.FrameNumber, e.CurrentFPS, e.FrameDuration.TotalMilliseconds);
			}
		}

		/// <summary>
		/// Called when the rendering backend changes.
		/// </summary>
		private void OnBackendChanged(object? sender, RenderingBackendChangedEventArgs e)
		{
			FlutterSharpLogger.LogInformation(
				"Rendering backend changed: {OldBackend} -> {NewBackend}",
				e.OldBackend, e.NewBackend);
		}

		/// <summary>
		/// Sets up input event handlers for keyboard and pointer input.
		/// </summary>
		private void SetupInputHandlers()
		{
			// Enable focus to receive keyboard events
			IsTabStop = true;
			AllowFocusOnInteraction = true;

			// Keyboard events
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			CharacterReceived += OnCharacterReceived;

			// Pointer/mouse events
			PointerPressed += OnPointerPressed;
			PointerReleased += OnPointerReleased;
			PointerMoved += OnPointerMoved;
			PointerEntered += OnPointerEntered;
			PointerExited += OnPointerExited;
			PointerWheelChanged += OnPointerWheelChanged;
			PointerCanceled += OnPointerCanceled;

			// Focus events
			GotFocus += OnGotFocus;
			LostFocus += OnLostFocus;

			FlutterSharpLogger.LogDebug("Input handlers set up for Windows");
		}

		#region Input Event Handlers

		/// <summary>
		/// Handles key down events.
		/// </summary>
		private void OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (ForwardKeyEventToFlutter(e, WM_KEYDOWN))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles key up events.
		/// </summary>
		private void OnKeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (ForwardKeyEventToFlutter(e, WM_KEYUP))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles character input events (text entry).
		/// </summary>
		private void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs e)
		{
			if (_flutterViewController != IntPtr.Zero && _hwnd != IntPtr.Zero)
			{
				// Forward character as WM_CHAR message
				var wParam = (IntPtr)e.Character;
				var lParam = IntPtr.Zero;

				if (FlutterWindowsNative.HandleWindowMessage(
					_flutterViewController, _hwnd, WM_CHAR, wParam, lParam, out _))
				{
					e.Handled = true;
				}
			}
		}

		/// <summary>
		/// Handles pointer pressed events (mouse down, touch down).
		/// </summary>
		private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			// Capture pointer to receive events even outside the control
			CapturePointer(e.Pointer);

			// Request focus for keyboard input
			Focus(FocusState.Pointer);

			if (ForwardPointerEventToFlutter(e, GetPointerDownMessage(e)))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles pointer released events (mouse up, touch up).
		/// </summary>
		private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			ReleasePointerCapture(e.Pointer);

			if (ForwardPointerEventToFlutter(e, GetPointerUpMessage(e)))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles pointer moved events (mouse move, touch move).
		/// </summary>
		private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (ForwardPointerEventToFlutter(e, WM_MOUSEMOVE))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles pointer entered events.
		/// </summary>
		private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
		{
			// Forward as pointer move for hover tracking
			ForwardPointerEventToFlutter(e, WM_MOUSEMOVE);
		}

		/// <summary>
		/// Handles pointer exited events.
		/// </summary>
		private void OnPointerExited(object sender, PointerRoutedEventArgs e)
		{
			// Send a pointer leave event via method channel
			// Flutter's HandleTopLevelWindowProc doesn't have a direct leave message,
			// but the view will detect the pointer has left
			ForwardPointerEventToFlutter(e, WM_MOUSEMOVE);
		}

		/// <summary>
		/// Handles pointer wheel changed events (scroll).
		/// </summary>
		private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
		{
			var properties = e.GetCurrentPoint(this).Properties;
			var delta = properties.MouseWheelDelta;

			// Determine if this is horizontal or vertical scroll
			uint message = properties.IsHorizontalMouseWheel ? WM_MOUSEHWHEEL : WM_MOUSEWHEEL;

			if (ForwardPointerEventToFlutter(e, message, delta))
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles pointer canceled events.
		/// </summary>
		private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			ReleasePointerCaptures();
		}

		/// <summary>
		/// Handles focus gained events.
		/// </summary>
		private void OnGotFocus(object sender, RoutedEventArgs e)
		{
			FlutterSharpLogger.LogDebug("FlutterControl got focus");
			// Notify Flutter of focus change if needed
		}

		/// <summary>
		/// Handles focus lost events.
		/// </summary>
		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			FlutterSharpLogger.LogDebug("FlutterControl lost focus");
			// Notify Flutter of focus change if needed
		}

		/// <summary>
		/// Forwards a keyboard event to Flutter via the native embedder.
		/// </summary>
		private bool ForwardKeyEventToFlutter(KeyRoutedEventArgs e, uint message)
		{
			if (_flutterViewController == IntPtr.Zero || _hwnd == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				// Convert VirtualKey to Win32 virtual key code
				var virtualKey = (int)e.Key;
				var wParam = (IntPtr)virtualKey;

				// Build lParam with scan code and extended key flag
				// lParam format: bits 0-15 = repeat count, 16-23 = scan code, 24 = extended key flag
				var scanCode = MapVirtualKeyToScanCode(virtualKey);
				var extended = IsExtendedKey(e.Key) ? 1 : 0;
				var lParam = (IntPtr)((scanCode << 16) | (extended << 24) | 1);

				return FlutterWindowsNative.HandleWindowMessage(
					_flutterViewController, _hwnd, message, wParam, lParam, out _);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error forwarding key event to Flutter");
				return false;
			}
		}

		/// <summary>
		/// Forwards a pointer event to Flutter via the native embedder.
		/// </summary>
		private bool ForwardPointerEventToFlutter(PointerRoutedEventArgs e, uint message, int wheelDelta = 0)
		{
			if (_flutterViewController == IntPtr.Zero || _hwnd == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				var point = e.GetCurrentPoint(this);
				var position = point.Position;

				// Pack x,y coordinates into lParam (low word = x, high word = y)
				var x = (int)position.X;
				var y = (int)position.Y;
				var lParam = (IntPtr)((y << 16) | (x & 0xFFFF));

				// Build wParam with button states and modifier keys
				IntPtr wParam;
				if (message == WM_MOUSEWHEEL || message == WM_MOUSEHWHEEL)
				{
					// For wheel messages, wParam high word = wheel delta, low word = key states
					wParam = (IntPtr)((wheelDelta << 16) | GetMouseKeyState());
				}
				else
				{
					wParam = (IntPtr)GetMouseKeyState();
				}

				return FlutterWindowsNative.HandleWindowMessage(
					_flutterViewController, _hwnd, message, wParam, lParam, out _);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error forwarding pointer event to Flutter");
				return false;
			}
		}

		/// <summary>
		/// Gets the appropriate WM_*BUTTONDOWN message for the pointer event.
		/// </summary>
		private uint GetPointerDownMessage(PointerRoutedEventArgs e)
		{
			var properties = e.GetCurrentPoint(this).Properties;

			if (properties.IsLeftButtonPressed)
				return WM_LBUTTONDOWN;
			if (properties.IsRightButtonPressed)
				return WM_RBUTTONDOWN;
			if (properties.IsMiddleButtonPressed)
				return WM_MBUTTONDOWN;

			// Default to left button for touch/pen
			return WM_LBUTTONDOWN;
		}

		/// <summary>
		/// Gets the appropriate WM_*BUTTONUP message for the pointer event.
		/// </summary>
		private uint GetPointerUpMessage(PointerRoutedEventArgs e)
		{
			var properties = e.GetCurrentPoint(this).Properties;

			// Check which button was just released by examining what's NOT pressed
			if (!properties.IsLeftButtonPressed)
				return WM_LBUTTONUP;
			if (!properties.IsRightButtonPressed)
				return WM_RBUTTONUP;
			if (!properties.IsMiddleButtonPressed)
				return WM_MBUTTONUP;

			return WM_LBUTTONUP;
		}

		/// <summary>
		/// Gets the current mouse button and modifier key state for wParam.
		/// </summary>
		private int GetMouseKeyState()
		{
			int state = 0;

			// MK_LBUTTON = 0x0001, MK_RBUTTON = 0x0002, MK_MBUTTON = 0x0010
			// MK_SHIFT = 0x0004, MK_CONTROL = 0x0008
			var modifiers = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
			if ((modifiers & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
				state |= 0x0004; // MK_SHIFT

			modifiers = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
			if ((modifiers & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
				state |= 0x0008; // MK_CONTROL

			return state;
		}

		/// <summary>
		/// Maps a virtual key code to a scan code.
		/// </summary>
		private int MapVirtualKeyToScanCode(int virtualKey)
		{
			// Use P/Invoke to get the actual scan code
			return (int)MapVirtualKeyW((uint)virtualKey, 0);
		}

		/// <summary>
		/// Determines if a key is an extended key (e.g., arrow keys, Insert, Delete, etc.).
		/// </summary>
		private bool IsExtendedKey(VirtualKey key)
		{
			return key switch
			{
				VirtualKey.Insert or
				VirtualKey.Delete or
				VirtualKey.Home or
				VirtualKey.End or
				VirtualKey.PageUp or
				VirtualKey.PageDown or
				VirtualKey.Left or
				VirtualKey.Right or
				VirtualKey.Up or
				VirtualKey.Down or
				VirtualKey.NumberKeyLock or
				VirtualKey.Divide or
				VirtualKey.RightControl or
				VirtualKey.RightMenu => true,
				_ => false
			};
		}

		[DllImport("user32.dll")]
		private static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

		#endregion

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
				FlutterWindowsNative.SendMessage(_flutterEngine, ChannelName, method, arguments);
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
		internal void OnMethodCall(string method, string arguments, Action<string> result)
		{
			FlutterSharpLogger.LogDebug("Received method call {Method} from Flutter", method);

			if (method == "ready")
			{
				FlutterSharpLogger.LogDebug("Ready received from Flutter, widget is null: {IsNull}", _widget == null);
				_isReady = true;

				// Send initial window metrics to Flutter
				SendInitialWindowMetrics();

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
				$"Windows/{method}");
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
		/// Gets or sets the restoration ID for this control.
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

			FlutterSharpLogger.LogDebug("Saved FlutterControl state: widgetType={WidgetType}", _widget?.GetType().Name);
			return state;
		}

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		public void RestoreState(Dictionary<string, object> state)
		{
			if (state == null)
				return;

			FlutterSharpLogger.LogDebug("Restoring FlutterControl state");

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

		#region Windows State Restoration

		/// <summary>
		/// Saves state to Windows local settings for application suspend/resume.
		/// </summary>
		public void SaveToLocalSettings()
		{
			try
			{
				var state = SaveState();
				var json = JsonSerializer.Serialize(state);
				global::Windows.Storage.ApplicationData.Current.LocalSettings.Values[_restorationId] = json;
				FlutterSharpLogger.LogDebug("Saved state to Windows local settings");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to save state to local settings");
			}
		}

		/// <summary>
		/// Restores state from Windows local settings.
		/// </summary>
		public void RestoreFromLocalSettings()
		{
			try
			{
				if (global::Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue(_restorationId, out var value) &&
					value is string json)
				{
					var state = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
					if (state != null)
					{
						RestoreState(state);
						FlutterSharpLogger.LogDebug("Restored state from Windows local settings");
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to restore state from local settings");
			}
		}

		#endregion

		#region Lifecycle Management

		/// <summary>
		/// Notifies Flutter that the application resumed.
		/// Call this when the window is activated.
		/// </summary>
		public void NotifyResumed()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);
		}

		/// <summary>
		/// Notifies Flutter that the application is inactive.
		/// Call this when the window loses focus.
		/// </summary>
		public void NotifyInactive()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Inactive);
		}

		/// <summary>
		/// Notifies Flutter that the application is paused.
		/// Call this when the window is minimized.
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

		#region Window Sizing and DPI

		/// <summary>
		/// Updates the control size and notifies Flutter.
		/// Call this when the container size changes.
		/// </summary>
		/// <param name="logicalWidth">Width in logical (device-independent) pixels</param>
		/// <param name="logicalHeight">Height in logical (device-independent) pixels</param>
		public void UpdateSize(double logicalWidth, double logicalHeight)
		{
			if (logicalWidth <= 0 || logicalHeight <= 0)
				return;

			// Store logical size
			_logicalWidth = logicalWidth;
			_logicalHeight = logicalHeight;

			// Calculate physical size based on DPI
			var physicalWidth = (int)Math.Round(logicalWidth * _dpiScaleFactor);
			var physicalHeight = (int)Math.Round(logicalHeight * _dpiScaleFactor);

			// Check if size actually changed
			if (physicalWidth == _physicalWidth && physicalHeight == _physicalHeight)
				return;

			var oldWidth = _physicalWidth;
			var oldHeight = _physicalHeight;

			_physicalWidth = physicalWidth;
			_physicalHeight = physicalHeight;

			FlutterSharpLogger.LogDebug(
				"FlutterControl size updated: logical={LogicalWidth}x{LogicalHeight}, physical={PhysicalWidth}x{PhysicalHeight}, DPI={Dpi}",
				logicalWidth, logicalHeight, physicalWidth, physicalHeight, _currentDpi);

			// Notify Flutter of the size change
			NotifyFlutterOfSizeChange();

			// Raise the size changed event
			FlutterSizeChanged?.Invoke(this, new SizeChangedEventArgs(
				oldWidth, oldHeight, physicalWidth, physicalHeight, _dpiScaleFactor));
		}

		/// <summary>
		/// Refreshes the DPI value from the window handle and updates if changed.
		/// Call this when you suspect DPI may have changed (e.g., after window move).
		/// </summary>
		public void RefreshDpi()
		{
			if (_hwnd == IntPtr.Zero)
				return;

			var newDpi = FlutterWindowsNative.GetDpi(_hwnd);
			UpdateDpi(newDpi);
		}

		/// <summary>
		/// Updates the DPI and notifies Flutter if changed.
		/// </summary>
		/// <param name="newDpi">New DPI value</param>
		private void UpdateDpi(uint newDpi)
		{
			if (newDpi == _currentDpi)
				return;

			var oldDpi = _currentDpi;
			_currentDpi = newDpi;
			_dpiScaleFactor = newDpi / (double)FlutterWindowsNative.DefaultDpi;

			FlutterSharpLogger.LogInformation(
				"DPI changed from {OldDpi} to {NewDpi} (scale factor: {ScaleFactor:F2})",
				oldDpi, newDpi, _dpiScaleFactor);

			// Recalculate physical size if we have a logical size
			if (_logicalWidth > 0 && _logicalHeight > 0)
			{
				var newPhysicalWidth = (int)Math.Round(_logicalWidth * _dpiScaleFactor);
				var newPhysicalHeight = (int)Math.Round(_logicalHeight * _dpiScaleFactor);

				if (newPhysicalWidth != _physicalWidth || newPhysicalHeight != _physicalHeight)
				{
					_physicalWidth = newPhysicalWidth;
					_physicalHeight = newPhysicalHeight;
				}
			}

			// Notify Flutter of DPI change
			NotifyFlutterOfDpiChange();

			// Raise the DPI changed event
			DpiChanged?.Invoke(this, new DpiChangedEventArgs(oldDpi, newDpi, _dpiScaleFactor));
		}

		/// <summary>
		/// Sends size information to Flutter via method channel.
		/// </summary>
		private void NotifyFlutterOfSizeChange()
		{
			if (!_isReady)
				return;

			var sizeInfo = JsonSerializer.Serialize(new
			{
				logicalWidth = _logicalWidth,
				logicalHeight = _logicalHeight,
				physicalWidth = _physicalWidth,
				physicalHeight = _physicalHeight,
				dpi = _currentDpi,
				devicePixelRatio = _dpiScaleFactor
			});

			SendMessageToFlutter("windowSizeChanged", sizeInfo);
		}

		/// <summary>
		/// Sends DPI information to Flutter via method channel.
		/// </summary>
		private void NotifyFlutterOfDpiChange()
		{
			if (!_isReady)
				return;

			var dpiInfo = JsonSerializer.Serialize(new
			{
				dpi = _currentDpi,
				devicePixelRatio = _dpiScaleFactor,
				physicalWidth = _physicalWidth,
				physicalHeight = _physicalHeight
			});

			SendMessageToFlutter("dpiChanged", dpiInfo);
		}

		/// <summary>
		/// Sends initial window metrics to Flutter.
		/// Called when Flutter signals ready.
		/// </summary>
		private void SendInitialWindowMetrics()
		{
			if (_physicalWidth <= 0 || _physicalHeight <= 0)
			{
				// Use default size if not yet set
				_physicalWidth = 800;
				_physicalHeight = 600;
				_logicalWidth = 800 / _dpiScaleFactor;
				_logicalHeight = 600 / _dpiScaleFactor;
			}

			var metrics = JsonSerializer.Serialize(new
			{
				logicalWidth = _logicalWidth,
				logicalHeight = _logicalHeight,
				physicalWidth = _physicalWidth,
				physicalHeight = _physicalHeight,
				dpi = _currentDpi,
				devicePixelRatio = _dpiScaleFactor
			});

			SendMessageToFlutter("windowMetrics", metrics);
		}

		/// <summary>
		/// Gets the monitor bounds for the window containing this control.
		/// </summary>
		/// <returns>Monitor bounds and work area, or null if unavailable</returns>
		public (System.Drawing.Rectangle MonitorBounds, System.Drawing.Rectangle WorkArea)? GetMonitorBounds()
		{
			return FlutterWindowsNative.GetMonitorBounds(_hwnd);
		}

		#endregion

		#region IDisposable Implementation

		/// <summary>
		/// Disposes the FlutterControl and releases native resources.
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
				// Dispose GPU rendering
				if (_renderer != null)
				{
					_renderer.FrameRendered -= OnFrameRendered;
					_renderer.BackendChanged -= OnBackendChanged;
					_renderer.Dispose();
					_renderer = null;
				}

				// Unregister from state restoration
				StateRestorationService.Unregister(this);

				// Untrack the widget
				if (_widget != null)
				{
					FlutterManager.UntrackWidget(_widget);
				}
			}

			// Remove the message handler before destroying the engine
			if (_flutterEngine != IntPtr.Zero)
			{
				FlutterWindowsNative.RemoveMessageHandler(_flutterEngine, ChannelName);
			}

			// Release native resources
			if (_flutterViewController != IntPtr.Zero)
			{
				FlutterWindowsNative.DestroyViewController(_flutterViewController);
				_flutterViewController = IntPtr.Zero;
			}

			if (_flutterEngine != IntPtr.Zero)
			{
				FlutterWindowsNative.DestroyEngine(_flutterEngine);
				_flutterEngine = IntPtr.Zero;
			}

			_isDisposed = true;
			FlutterSharpLogger.LogDebug("FlutterControl disposed");
		}

		/// <summary>
		/// Finalizer to ensure native resources are released.
		/// </summary>
		~FlutterControl()
		{
			Dispose(false);
		}

		#endregion
	}

	/// <summary>
	/// Event arguments for DPI change events.
	/// </summary>
	public class DpiChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the previous DPI value.
		/// </summary>
		public uint OldDpi { get; }

		/// <summary>
		/// Gets the new DPI value.
		/// </summary>
		public uint NewDpi { get; }

		/// <summary>
		/// Gets the new DPI scale factor (NewDpi / 96.0).
		/// </summary>
		public double NewScaleFactor { get; }

		/// <summary>
		/// Creates a new DPI changed event args instance.
		/// </summary>
		public DpiChangedEventArgs(uint oldDpi, uint newDpi, double newScaleFactor)
		{
			OldDpi = oldDpi;
			NewDpi = newDpi;
			NewScaleFactor = newScaleFactor;
		}
	}

	/// <summary>
	/// Event arguments for size change events.
	/// </summary>
	public class SizeChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the previous physical width in pixels.
		/// </summary>
		public int OldWidth { get; }

		/// <summary>
		/// Gets the previous physical height in pixels.
		/// </summary>
		public int OldHeight { get; }

		/// <summary>
		/// Gets the new physical width in pixels.
		/// </summary>
		public int NewWidth { get; }

		/// <summary>
		/// Gets the new physical height in pixels.
		/// </summary>
		public int NewHeight { get; }

		/// <summary>
		/// Gets the current DPI scale factor.
		/// </summary>
		public double DpiScaleFactor { get; }

		/// <summary>
		/// Creates a new size changed event args instance.
		/// </summary>
		public SizeChangedEventArgs(int oldWidth, int oldHeight, int newWidth, int newHeight, double dpiScaleFactor)
		{
			OldWidth = oldWidth;
			OldHeight = oldHeight;
			NewWidth = newWidth;
			NewHeight = newHeight;
			DpiScaleFactor = dpiScaleFactor;
		}
	}
}
