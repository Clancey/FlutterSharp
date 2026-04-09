using System;
using System.Runtime.InteropServices;
using System.Text;
using Flutter.Logging;

namespace Flutter.macOS
{
	/// <summary>
	/// P/Invoke declarations for Flutter macOS embedder native library.
	/// This class provides the low-level interface to the Flutter macOS framework.
	/// </summary>
	/// <remarks>
	/// The Flutter macOS embedder exposes a C API similar to Windows.
	/// This class wraps those native functions for use from C#.
	///
	/// Note: The actual Flutter.framework must be present in the application bundle
	/// for these functions to work. The framework is typically built as part
	/// of the Flutter build process.
	/// </remarks>
	internal static class FlutterMacOSNative
	{
		// Name of the Flutter macOS native library
		// On macOS, this is typically libflutter_macos.dylib or part of Flutter.framework
		private const string FlutterLibrary = "libflutter_macos";

		// Flag to track if native library is available
		private static bool? _isNativeAvailable;

		/// <summary>
		/// Checks if the Flutter macOS native library is available.
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
				// Try to load the library to verify it's available
				var handle = NativeLibrary.Load(FlutterLibrary);
				NativeLibrary.Free(handle);
				FlutterSharpLogger.LogDebug("Flutter macOS native library is available");
				return true;
			}
			catch (DllNotFoundException)
			{
				FlutterSharpLogger.LogWarning("Flutter macOS native library not found: {Library}", FlutterLibrary);
				return false;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Error checking Flutter macOS native library");
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
		/// Creates a Flutter view controller for the specified engine.
		/// </summary>
		public static IntPtr CreateViewController(IntPtr engine)
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
					width = 800,
					height = 600
				};
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
		/// Gets the native NSView handle for the Flutter view.
		/// </summary>
		public static IntPtr GetViewHandle(IntPtr viewController)
		{
			if (viewController == IntPtr.Zero || !IsNativeAvailable)
				return IntPtr.Zero;

			try
			{
				var view = FlutterDesktopViewControllerGetView(viewController);
				return FlutterDesktopViewGetNSView(view);
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

				// Store the handler reference to prevent garbage collection
				var handlerDelegate = new FlutterDesktopMessageCallback((messenger, message, userData) =>
				{
					try
					{
						// Parse the incoming message
						var messageStr = message != IntPtr.Zero
							? Marshal.PtrToStringUTF8(message) ?? string.Empty
							: string.Empty;

						// Parse as JSON to extract method and arguments
						var parsed = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(messageStr);
						var methodName = parsed.GetProperty("method").GetString() ?? string.Empty;
						var args = parsed.TryGetProperty("args", out var argsElement)
							? argsElement.GetString() ?? string.Empty
							: string.Empty;

						handler(methodName, args, response =>
						{
							// Send response back to Flutter
							var responseBytes = Encoding.UTF8.GetBytes(response);
							unsafe
							{
								fixed (byte* responsePtr = responseBytes)
								{
									FlutterDesktopMessengerSendResponse(
										messenger,
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
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to set message handler for channel {Channel}", channelName);
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

		// View - returns NSView* on macOS
		[DllImport(FlutterLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FlutterDesktopViewGetNSView(IntPtr view);

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

		#endregion
	}
}
