using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Flutter.Logging;

namespace Flutter.Windows
{
	/// <summary>
	/// P/Invoke declarations for Flutter Windows embedder native library.
	/// This class provides the low-level interface to flutter_windows.dll.
	/// </summary>
	/// <remarks>
	/// The Flutter Windows embedder exposes a C API that can be called from any language.
	/// This class wraps those native functions for use from C#.
	///
	/// Note: The actual flutter_windows.dll must be present in the application directory
	/// or system PATH for these functions to work. The library is typically built as part
	/// of the Flutter build process.
	/// </remarks>
	internal static class FlutterWindowsNative
	{
		// Name of the Flutter Windows native library
		private const string FlutterLibrary = "flutter_windows.dll";

		// Flag to track if native library is available
		private static bool? _isNativeAvailable;

		/// <summary>
		/// Checks if the Flutter Windows native library is available.
		/// </summary>
		public static bool IsNativeAvailable
		{
			get
			{
				if (_isNativeAvailable == null)
				{
					_isNativeAvailable = CheckNativeLibrary();
				}
				return _isNativeAvailable.Value;
			}
		}

		private static bool CheckNativeLibrary()
		{
			try
			{
				var handle = NativeLibrary.Load(FlutterLibrary);
				NativeLibrary.Free(handle);
				FlutterSharpLogger.LogDebug("Flutter Windows native library is available");
				return true;
			}
			catch (DllNotFoundException)
			{
				FlutterSharpLogger.LogWarning("Flutter Windows native library not found: {Library}", FlutterLibrary);
				return false;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Error checking Flutter Windows native library");
				return false;
			}
		}

		#region Engine Management

		/// <summary>
		/// Creates a new Flutter engine instance.
		/// </summary>
		/// <returns>Handle to the created engine, or IntPtr.Zero if creation failed</returns>
		public static IntPtr CreateEngine()
		{
			if (!IsNativeAvailable)
			{
				FlutterSharpLogger.LogDebug("Creating simulated Flutter engine (native library not available)");
				return IntPtr.Zero;
			}

			try
			{
				return FlutterDesktopEngineCreate(IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to create Flutter engine");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Destroys a Flutter engine instance.
		/// </summary>
		/// <param name="engine">Handle to the engine to destroy</param>
		public static void DestroyEngine(IntPtr engine)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
				return;

			try
			{
				FlutterDesktopEngineDestroy(engine);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to destroy Flutter engine");
			}
		}

		/// <summary>
		/// Runs the Flutter engine with the specified entry point.
		/// </summary>
		public static bool RunEngine(IntPtr engine, string? entryPoint = null)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
				return false;

			try
			{
				return FlutterDesktopEngineRun(engine, entryPoint ?? string.Empty);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to run Flutter engine");
				return false;
			}
		}

		#endregion

		#region View Controller Management

		/// <summary>
		/// Creates a Flutter view controller for the specified engine with default size.
		/// </summary>
		public static IntPtr CreateViewController(IntPtr engine)
		{
			return CreateViewController(engine, 800, 600);
		}

		/// <summary>
		/// Creates a Flutter view controller for the specified engine with specified size.
		/// </summary>
		/// <param name="engine">Handle to the Flutter engine</param>
		/// <param name="width">Initial width in physical pixels</param>
		/// <param name="height">Initial height in physical pixels</param>
		/// <returns>Handle to the created view controller, or IntPtr.Zero if creation failed</returns>
		public static IntPtr CreateViewController(IntPtr engine, int width, int height)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
			{
				FlutterSharpLogger.LogDebug("Creating simulated view controller (native library or engine not available)");
				return IntPtr.Zero;
			}

			try
			{
				var properties = new FlutterDesktopViewControllerProperties
				{
					width = width,
					height = height
				};
				FlutterSharpLogger.LogDebug("Creating Flutter view controller with size {Width}x{Height}", width, height);
				return FlutterDesktopViewControllerCreate(ref properties, engine);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to create Flutter view controller");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Destroys a Flutter view controller.
		/// </summary>
		public static void DestroyViewController(IntPtr viewController)
		{
			if (viewController == IntPtr.Zero || !IsNativeAvailable)
				return;

			try
			{
				FlutterDesktopViewControllerDestroy(viewController);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to destroy Flutter view controller");
			}
		}

		/// <summary>
		/// Gets the native window handle for the Flutter view.
		/// </summary>
		public static IntPtr GetViewHandle(IntPtr viewController)
		{
			if (viewController == IntPtr.Zero || !IsNativeAvailable)
				return IntPtr.Zero;

			try
			{
				var view = FlutterDesktopViewControllerGetView(viewController);
				return FlutterDesktopViewGetHWND(view);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to get Flutter view handle");
				return IntPtr.Zero;
			}
		}

		#endregion

		#region Message Channel

		/// <summary>
		/// Sends a message to Flutter via the method channel.
		/// </summary>
		public static void SendMessage(IntPtr engine, string channelName, string method, string arguments)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
			{
				FlutterSharpLogger.LogDebug("Simulating message send: {Method}", method);
				return;
			}

			try
			{
				var messenger = FlutterDesktopEngineGetMessenger(engine);
				if (messenger == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("Binary messenger not available");
					return;
				}

				// Build the method call message (JSON codec format)
				var message = System.Text.Json.JsonSerializer.Serialize(new
				{
					method,
					args = arguments
				});

				var messageBytes = Encoding.UTF8.GetBytes(message);
				var channelBytes = Encoding.UTF8.GetBytes(channelName);

				unsafe
				{
					fixed (byte* messagePtr = messageBytes)
					fixed (byte* channelPtr = channelBytes)
					{
						FlutterDesktopMessengerSend(
							messenger,
							(IntPtr)channelPtr,
							(IntPtr)messagePtr,
							(nuint)messageBytes.Length);
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to send message {Method} to Flutter", method);
			}
		}

		// Store handler delegates to prevent garbage collection
		private static readonly Dictionary<string, FlutterDesktopMessageCallback> _registeredHandlers = new();

		/// <summary>
		/// Sets up a callback to receive messages from Flutter.
		/// </summary>
		public static void SetMessageHandler(IntPtr engine, string channelName, Action<string, string, Action<string>> handler)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
				return;

			try
			{
				var messenger = FlutterDesktopEngineGetMessenger(engine);
				if (messenger == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("Binary messenger not available for setting handler");
					return;
				}

				// Create the native callback delegate
				var handlerDelegate = new FlutterDesktopMessageCallback((messengerPtr, message, userData) =>
				{
					try
					{
						// Parse the incoming message
						var messageStr = message != IntPtr.Zero
							? Marshal.PtrToStringUTF8(message) ?? string.Empty
							: string.Empty;

						FlutterSharpLogger.LogDebug("Received message from Flutter: {Message}", messageStr);

						// Parse as JSON to extract method and arguments
						var parsed = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(messageStr);
						var method = parsed.GetProperty("method").GetString() ?? string.Empty;
						var args = parsed.TryGetProperty("args", out var argsElement)
							? argsElement.GetString() ?? string.Empty
							: string.Empty;

						handler(method, args, response =>
						{
							// Send response back to Flutter
							var responseBytes = Encoding.UTF8.GetBytes(response);
							unsafe
							{
								fixed (byte* responsePtr = responseBytes)
								{
									FlutterDesktopMessengerSendResponse(
										messengerPtr,
										IntPtr.Zero,  // Reply handle
										(IntPtr)responsePtr,
										(nuint)responseBytes.Length);
								}
							}
						});
					}
					catch (Exception ex)
					{
						FlutterSharpLogger.LogError(ex, "Error handling message from Flutter");
					}
				});

				// Store the delegate to prevent garbage collection
				_registeredHandlers[channelName] = handlerDelegate;

				var channelBytes = Encoding.UTF8.GetBytes(channelName);
				unsafe
				{
					fixed (byte* channelPtr = channelBytes)
					{
						FlutterDesktopMessengerSetCallback(
							messenger,
							(IntPtr)channelPtr,
							handlerDelegate,
							IntPtr.Zero);
					}
				}

				FlutterSharpLogger.LogDebug("Message handler registered for channel {Channel}", channelName);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to set message handler for channel {Channel}", channelName);
			}
		}

		/// <summary>
		/// Removes a previously registered message handler.
		/// </summary>
		public static void RemoveMessageHandler(IntPtr engine, string channelName)
		{
			if (engine == IntPtr.Zero || !IsNativeAvailable)
				return;

			try
			{
				var messenger = FlutterDesktopEngineGetMessenger(engine);
				if (messenger == IntPtr.Zero)
					return;

				var channelBytes = Encoding.UTF8.GetBytes(channelName);
				unsafe
				{
					fixed (byte* channelPtr = channelBytes)
					{
						FlutterDesktopMessengerSetCallback(
							messenger,
							(IntPtr)channelPtr,
							null!,  // Passing null removes the callback
							IntPtr.Zero);
					}
				}

				_registeredHandlers.Remove(channelName);
				FlutterSharpLogger.LogDebug("Message handler removed for channel {Channel}", channelName);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to remove message handler for channel {Channel}", channelName);
			}
		}

		#endregion

		#region DPI Management

		/// <summary>
		/// Standard Windows DPI baseline (96 DPI = 100% scaling).
		/// </summary>
		public const uint DefaultDpi = 96;

		/// <summary>
		/// Gets the DPI for a specific window handle.
		/// Uses GetDpiForWindow on Windows 10 v1607+, falls back to system DPI on older versions.
		/// </summary>
		/// <param name="hwnd">Window handle to query DPI for</param>
		/// <returns>DPI value (96 = 100%, 120 = 125%, 144 = 150%, etc.)</returns>
		public static uint GetDpi(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
			{
				return GetSystemDpi();
			}

			try
			{
				// Try GetDpiForWindow first (Windows 10 v1607+)
				var dpi = GetDpiForWindow(hwnd);
				if (dpi != 0)
				{
					return dpi;
				}
			}
			catch (EntryPointNotFoundException)
			{
				// GetDpiForWindow not available on this Windows version
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "GetDpiForWindow failed, falling back to monitor DPI");
			}

			// Fall back to monitor DPI
			return GetMonitorDpi(hwnd);
		}

		/// <summary>
		/// Gets the DPI for the monitor containing the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to find monitor for</param>
		/// <returns>DPI value for the monitor</returns>
		public static uint GetMonitorDpi(IntPtr hwnd)
		{
			try
			{
				var hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
				if (hMonitor != IntPtr.Zero)
				{
					if (GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _) == 0)
					{
						return dpiX;
					}
				}
			}
			catch (DllNotFoundException)
			{
				// shcore.dll not available on Windows 7
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "GetDpiForMonitor failed, falling back to system DPI");
			}

			return GetSystemDpi();
		}

		/// <summary>
		/// Gets the system DPI (fallback for older Windows versions).
		/// </summary>
		/// <returns>System DPI value</returns>
		public static uint GetSystemDpi()
		{
			try
			{
				var dpi = GetDpiForSystem();
				if (dpi != 0)
				{
					return dpi;
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "GetDpiForSystem failed, using default 96 DPI");
			}

			return DefaultDpi;
		}

		/// <summary>
		/// Gets the DPI scale factor (1.0 = 100%, 1.25 = 125%, etc.).
		/// </summary>
		/// <param name="hwnd">Window handle to query DPI for</param>
		/// <returns>Scale factor as a double</returns>
		public static double GetDpiScaleFactor(IntPtr hwnd)
		{
			return GetDpi(hwnd) / (double)DefaultDpi;
		}

		/// <summary>
		/// Gets monitor information including work area.
		/// </summary>
		/// <param name="hwnd">Window handle to get monitor info for</param>
		/// <returns>Tuple of (monitorBounds, workArea) or null if failed</returns>
		public static (System.Drawing.Rectangle MonitorBounds, System.Drawing.Rectangle WorkArea)? GetMonitorBounds(IntPtr hwnd)
		{
			try
			{
				var hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
				if (hMonitor == IntPtr.Zero)
				{
					return null;
				}

				var monitorInfo = new MONITORINFO
				{
					cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
				};

				if (GetMonitorInfo(hMonitor, ref monitorInfo))
				{
					var monitorBounds = new System.Drawing.Rectangle(
						monitorInfo.rcMonitor.left,
						monitorInfo.rcMonitor.top,
						monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left,
						monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top);

					var workArea = new System.Drawing.Rectangle(
						monitorInfo.rcWork.left,
						monitorInfo.rcWork.top,
						monitorInfo.rcWork.right - monitorInfo.rcWork.left,
						monitorInfo.rcWork.bottom - monitorInfo.rcWork.top);

					return (monitorBounds, workArea);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Failed to get monitor bounds");
			}

			return null;
		}

		/// <summary>
		/// Converts logical pixels to physical pixels using the window's DPI.
		/// </summary>
		/// <param name="hwnd">Window handle for DPI reference</param>
		/// <param name="logicalPixels">Size in logical pixels</param>
		/// <returns>Size in physical pixels</returns>
		public static int LogicalToPhysical(IntPtr hwnd, double logicalPixels)
		{
			var scale = GetDpiScaleFactor(hwnd);
			return (int)Math.Round(logicalPixels * scale);
		}

		/// <summary>
		/// Converts physical pixels to logical pixels using the window's DPI.
		/// </summary>
		/// <param name="hwnd">Window handle for DPI reference</param>
		/// <param name="physicalPixels">Size in physical pixels</param>
		/// <returns>Size in logical pixels</returns>
		public static double PhysicalToLogical(IntPtr hwnd, int physicalPixels)
		{
			var scale = GetDpiScaleFactor(hwnd);
			return physicalPixels / scale;
		}

		#endregion

		#region Input Event Handling

		/// <summary>
		/// Forwards a Windows message to the Flutter view controller for input processing.
		/// </summary>
		/// <param name="viewController">Handle to the Flutter view controller</param>
		/// <param name="hwnd">Window handle</param>
		/// <param name="message">Windows message ID (WM_*)</param>
		/// <param name="wParam">Message wParam</param>
		/// <param name="lParam">Message lParam</param>
		/// <param name="result">Output: result to return from window procedure</param>
		/// <returns>True if Flutter handled the message, false otherwise</returns>
		public static bool HandleWindowMessage(IntPtr viewController, IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, out IntPtr result)
		{
			result = IntPtr.Zero;

			if (viewController == IntPtr.Zero || !IsNativeAvailable)
			{
				return false;
			}

			try
			{
				return FlutterDesktopViewControllerHandleTopLevelWindowProc(
					viewController, hwnd, message, wParam, lParam, out result);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to forward window message {Message} to Flutter", message);
				return false;
			}
		}

		/// <summary>
		/// Processes an external window message for lifecycle state updates.
		/// </summary>
		public static bool ProcessExternalWindowMessage(IntPtr engine, IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, out IntPtr result)
		{
			result = IntPtr.Zero;

			if (engine == IntPtr.Zero || !IsNativeAvailable)
			{
				return false;
			}

			try
			{
				return FlutterDesktopEngineProcessExternalWindowMessage(
					engine, hwnd, message, wParam, lParam, out result);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to process external window message {Message}", message);
				return false;
			}
		}

		#endregion

		#region Native P/Invoke Declarations

		// Callback delegate for receiving messages from Flutter
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void FlutterDesktopMessageCallback(IntPtr messenger, IntPtr message, IntPtr userData);

		// View controller properties structure
		[StructLayout(LayoutKind.Sequential)]
		private struct FlutterDesktopViewControllerProperties
		{
			public int width;
			public int height;
		}

		#region Win32 DPI APIs

		private const int MONITOR_DEFAULTTONEAREST = 2;

		/// <summary>
		/// Monitor DPI type enum matching Windows API.
		/// </summary>
		private enum MONITOR_DPI_TYPE
		{
			MDT_EFFECTIVE_DPI = 0,
			MDT_ANGULAR_DPI = 1,
			MDT_RAW_DPI = 2,
			MDT_DEFAULT = MDT_EFFECTIVE_DPI
		}

		// Win32 GetDpiForWindow - available on Windows 10 version 1607+
		[DllImport("user32.dll")]
		private static extern uint GetDpiForWindow(IntPtr hwnd);

		// Win32 GetDpiForMonitor - available on Windows 8.1+
		[DllImport("shcore.dll")]
		private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

		// Win32 MonitorFromWindow
		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		// Win32 GetSystemDpiForProcess - available on Windows 10 version 1803+
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetSystemDpiForProcess(IntPtr hProcess);

		// Win32 GetDpiForSystem - fallback for older systems
		[DllImport("user32.dll")]
		private static extern uint GetDpiForSystem();

		// Win32 RECT structure
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		// Win32 MONITORINFO structure
		[StructLayout(LayoutKind.Sequential)]
		private struct MONITORINFO
		{
			public uint cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;
		}

		// Win32 GetMonitorInfo
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

		#endregion

		// Engine lifecycle
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopEngineCreate(IntPtr properties);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FlutterDesktopEngineDestroy(IntPtr engine);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FlutterDesktopEngineRun(IntPtr engine, [MarshalAs(UnmanagedType.LPStr)] string entryPoint);

		// View controller lifecycle
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopViewControllerCreate(ref FlutterDesktopViewControllerProperties properties, IntPtr engine);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FlutterDesktopViewControllerDestroy(IntPtr viewController);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopViewControllerGetView(IntPtr viewController);

		// View
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopViewGetHWND(IntPtr view);

		// Messenger
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopEngineGetMessenger(IntPtr engine);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FlutterDesktopMessengerSend(IntPtr messenger, IntPtr channel, IntPtr message, nuint messageSize);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FlutterDesktopMessengerSendResponse(IntPtr messenger, IntPtr handle, IntPtr data, nuint dataLength);

		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FlutterDesktopMessengerSetCallback(
			IntPtr messenger,
			IntPtr channel,
			FlutterDesktopMessageCallback callback,
			IntPtr userData);

		// Input event handling - forwards Windows messages to Flutter
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FlutterDesktopViewControllerHandleTopLevelWindowProc(
			IntPtr controller,
			IntPtr hwnd,
			uint message,
			IntPtr wParam,
			IntPtr lParam,
			out IntPtr result);

		// External window message processing for lifecycle updates
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FlutterDesktopEngineProcessExternalWindowMessage(
			IntPtr engine,
			IntPtr hwnd,
			uint message,
			IntPtr wParam,
			IntPtr lParam,
			out IntPtr result);

		#endregion
	}
}
