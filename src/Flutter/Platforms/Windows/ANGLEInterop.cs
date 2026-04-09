using System;
using System.Runtime.InteropServices;
using Flutter.Logging;

namespace Flutter.Windows
{
	/// <summary>
	/// ANGLE (Almost Native Graphics Layer Engine) interop for Windows.
	/// ANGLE translates OpenGL ES calls to Direct3D, enabling Flutter rendering on Windows.
	/// </summary>
	/// <remarks>
	/// Flutter uses ANGLE to abstract graphics APIs. This class provides the EGL
	/// (Embedded Graphics Library) interface that ANGLE exposes, allowing C# to:
	/// - Initialize the ANGLE/EGL context
	/// - Share surfaces between ANGLE and Direct3D
	/// - Manage frame synchronization
	/// </remarks>
	internal class ANGLEInterop : IDisposable
	{
		#region EGL Constants

		// EGL success/error codes
		private const int EGL_SUCCESS = 0x3000;
		private const int EGL_NOT_INITIALIZED = 0x3001;
		private const int EGL_BAD_ACCESS = 0x3002;
		private const int EGL_BAD_ALLOC = 0x3003;
		private const int EGL_BAD_ATTRIBUTE = 0x3004;
		private const int EGL_BAD_CONFIG = 0x3005;
		private const int EGL_BAD_CONTEXT = 0x3006;
		private const int EGL_BAD_CURRENT_SURFACE = 0x3007;
		private const int EGL_BAD_DISPLAY = 0x3008;
		private const int EGL_BAD_MATCH = 0x3009;
		private const int EGL_BAD_NATIVE_PIXMAP = 0x300A;
		private const int EGL_BAD_NATIVE_WINDOW = 0x300B;
		private const int EGL_BAD_PARAMETER = 0x300C;
		private const int EGL_BAD_SURFACE = 0x300D;

		// EGL attribute tokens
		private const int EGL_NONE = 0x3038;
		private const int EGL_BUFFER_SIZE = 0x3020;
		private const int EGL_ALPHA_SIZE = 0x3021;
		private const int EGL_BLUE_SIZE = 0x3022;
		private const int EGL_GREEN_SIZE = 0x3023;
		private const int EGL_RED_SIZE = 0x3024;
		private const int EGL_DEPTH_SIZE = 0x3025;
		private const int EGL_STENCIL_SIZE = 0x3026;
		private const int EGL_SURFACE_TYPE = 0x3033;
		private const int EGL_RENDERABLE_TYPE = 0x3040;
		private const int EGL_WINDOW_BIT = 0x0004;
		private const int EGL_OPENGL_ES2_BIT = 0x0004;
		private const int EGL_OPENGL_ES3_BIT = 0x0040;
		private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

		// ANGLE-specific attributes
		private const int EGL_PLATFORM_ANGLE_ANGLE = 0x3202;
		private const int EGL_PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
		private const int EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
		private const int EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE = 0x3207;
		private const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE = 0x3209;
		private const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_HARDWARE_ANGLE = 0x320A;
		private const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_WARP_ANGLE = 0x320C;
		private const int EGL_PLATFORM_ANGLE_D3D11ON12_ANGLE = 0x320D;

		// Special values
		private static readonly IntPtr EGL_NO_DISPLAY = IntPtr.Zero;
		private static readonly IntPtr EGL_NO_CONTEXT = IntPtr.Zero;
		private static readonly IntPtr EGL_NO_SURFACE = IntPtr.Zero;
		private static readonly IntPtr EGL_DEFAULT_DISPLAY = IntPtr.Zero;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the EGL display handle.
		/// </summary>
		public IntPtr Display { get; private set; }

		/// <summary>
		/// Gets the EGL context handle.
		/// </summary>
		public IntPtr Context { get; private set; }

		/// <summary>
		/// Gets the EGL surface handle.
		/// </summary>
		public IntPtr Surface { get; private set; }

		/// <summary>
		/// Gets the EGL config handle.
		/// </summary>
		public IntPtr Config { get; private set; }

		/// <summary>
		/// Gets whether ANGLE/EGL is initialized.
		/// </summary>
		public bool IsInitialized => Display != IntPtr.Zero && Context != IntPtr.Zero;

		/// <summary>
		/// Gets the EGL major version.
		/// </summary>
		public int MajorVersion { get; private set; }

		/// <summary>
		/// Gets the EGL minor version.
		/// </summary>
		public int MinorVersion { get; private set; }

		/// <summary>
		/// Gets whether ANGLE library is available.
		/// </summary>
		public static bool IsAngleAvailable => _isAngleAvailable.Value;

		#endregion

		#region Fields

		private bool _disposed;
		private readonly object _lock = new();
		private static readonly Lazy<bool> _isAngleAvailable = new(CheckAngleAvailable);

		#endregion

		#region Static Helpers

		private static bool CheckAngleAvailable()
		{
			try
			{
				// Try to load libEGL.dll (ANGLE's EGL implementation)
				var handle = NativeLibrary.Load("libEGL.dll");
				if (handle != IntPtr.Zero)
				{
					NativeLibrary.Free(handle);
					FlutterSharpLogger.LogDebug("ANGLE libEGL.dll is available");
					return true;
				}
			}
			catch (DllNotFoundException)
			{
				FlutterSharpLogger.LogWarning("ANGLE libEGL.dll not found");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Error checking ANGLE availability");
			}

			return false;
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Initializes ANGLE/EGL with Direct3D 11 backend.
		/// </summary>
		/// <param name="nativeWindow">Native window handle (HWND)</param>
		/// <param name="useD3D11">Use D3D11 backend (true) or D3D9 (false)</param>
		/// <returns>True if initialization was successful</returns>
		public bool Initialize(IntPtr nativeWindow, bool useD3D11 = true)
		{
			if (!IsAngleAvailable)
			{
				FlutterSharpLogger.LogWarning("ANGLE is not available, skipping EGL initialization");
				return false;
			}

			lock (_lock)
			{
				if (IsInitialized)
				{
					FlutterSharpLogger.LogDebug("ANGLE/EGL already initialized");
					return true;
				}

				try
				{
					// Get platform display with ANGLE D3D backend
					var displayAttribs = new[]
					{
						EGL_PLATFORM_ANGLE_TYPE_ANGLE,
						useD3D11 ? EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE : EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE,
						EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE,
						EGL_PLATFORM_ANGLE_DEVICE_TYPE_HARDWARE_ANGLE,
						EGL_NONE
					};

					Display = eglGetPlatformDisplayEXT(
						EGL_PLATFORM_ANGLE_ANGLE,
						EGL_DEFAULT_DISPLAY,
						displayAttribs);

					if (Display == EGL_NO_DISPLAY)
					{
						FlutterSharpLogger.LogError("eglGetPlatformDisplayEXT failed: {Error}", GetErrorString());
						return false;
					}

					// Initialize EGL
					if (!eglInitialize(Display, out var major, out var minor))
					{
						FlutterSharpLogger.LogError("eglInitialize failed: {Error}", GetErrorString());
						Cleanup();
						return false;
					}

					MajorVersion = major;
					MinorVersion = minor;
					FlutterSharpLogger.LogInformation("EGL initialized: version {Major}.{Minor}", major, minor);

					// Choose config
					var configAttribs = new[]
					{
						EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
						EGL_RED_SIZE, 8,
						EGL_GREEN_SIZE, 8,
						EGL_BLUE_SIZE, 8,
						EGL_ALPHA_SIZE, 8,
						EGL_DEPTH_SIZE, 24,
						EGL_STENCIL_SIZE, 8,
						EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT | EGL_OPENGL_ES3_BIT,
						EGL_NONE
					};

					if (!eglChooseConfig(Display, configAttribs, out var config, 1, out var numConfigs) || numConfigs == 0)
					{
						FlutterSharpLogger.LogError("eglChooseConfig failed: {Error}", GetErrorString());
						Cleanup();
						return false;
					}

					Config = config;

					// Create window surface
					if (nativeWindow != IntPtr.Zero)
					{
						var surfaceAttribs = new[] { EGL_NONE };
						Surface = eglCreateWindowSurface(Display, Config, nativeWindow, surfaceAttribs);

						if (Surface == EGL_NO_SURFACE)
						{
							FlutterSharpLogger.LogError("eglCreateWindowSurface failed: {Error}", GetErrorString());
							Cleanup();
							return false;
						}
					}

					// Create OpenGL ES 3.0 context (or 2.0 as fallback)
					var contextAttribs = new[]
					{
						EGL_CONTEXT_CLIENT_VERSION, 3,
						EGL_NONE
					};

					Context = eglCreateContext(Display, Config, EGL_NO_CONTEXT, contextAttribs);

					if (Context == EGL_NO_CONTEXT)
					{
						// Try OpenGL ES 2.0
						contextAttribs = new[]
						{
							EGL_CONTEXT_CLIENT_VERSION, 2,
							EGL_NONE
						};

						Context = eglCreateContext(Display, Config, EGL_NO_CONTEXT, contextAttribs);

						if (Context == EGL_NO_CONTEXT)
						{
							FlutterSharpLogger.LogError("eglCreateContext failed: {Error}", GetErrorString());
							Cleanup();
							return false;
						}

						FlutterSharpLogger.LogInformation("Created OpenGL ES 2.0 context");
					}
					else
					{
						FlutterSharpLogger.LogInformation("Created OpenGL ES 3.0 context");
					}

					FlutterSharpLogger.LogInformation("ANGLE/EGL initialized successfully");
					return true;
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Exception initializing ANGLE/EGL");
					Cleanup();
					return false;
				}
			}
		}

		/// <summary>
		/// Makes the EGL context current for rendering.
		/// </summary>
		/// <returns>True if successful</returns>
		public bool MakeCurrent()
		{
			if (!IsInitialized)
			{
				return false;
			}

			try
			{
				var result = eglMakeCurrent(Display, Surface, Surface, Context);
				if (!result)
				{
					FlutterSharpLogger.LogWarning("eglMakeCurrent failed: {Error}", GetErrorString());
				}
				return result;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception making EGL context current");
				return false;
			}
		}

		/// <summary>
		/// Releases the current EGL context.
		/// </summary>
		/// <returns>True if successful</returns>
		public bool ReleaseCurrent()
		{
			if (Display == IntPtr.Zero)
			{
				return true;
			}

			try
			{
				return eglMakeCurrent(Display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception releasing EGL context");
				return false;
			}
		}

		/// <summary>
		/// Swaps the front and back buffers.
		/// </summary>
		/// <returns>True if successful</returns>
		public bool SwapBuffers()
		{
			if (!IsInitialized || Surface == EGL_NO_SURFACE)
			{
				return false;
			}

			try
			{
				return eglSwapBuffers(Display, Surface);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception swapping EGL buffers");
				return false;
			}
		}

		/// <summary>
		/// Sets the swap interval (0 = no vsync, 1 = vsync).
		/// </summary>
		/// <param name="interval">Swap interval</param>
		/// <returns>True if successful</returns>
		public bool SetSwapInterval(int interval)
		{
			if (!IsInitialized)
			{
				return false;
			}

			try
			{
				return eglSwapInterval(Display, interval);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception setting swap interval");
				return false;
			}
		}

		#endregion

		#region Helpers

		private string GetErrorString()
		{
			try
			{
				var error = eglGetError();
				return error switch
				{
					EGL_SUCCESS => "EGL_SUCCESS",
					EGL_NOT_INITIALIZED => "EGL_NOT_INITIALIZED",
					EGL_BAD_ACCESS => "EGL_BAD_ACCESS",
					EGL_BAD_ALLOC => "EGL_BAD_ALLOC",
					EGL_BAD_ATTRIBUTE => "EGL_BAD_ATTRIBUTE",
					EGL_BAD_CONFIG => "EGL_BAD_CONFIG",
					EGL_BAD_CONTEXT => "EGL_BAD_CONTEXT",
					EGL_BAD_CURRENT_SURFACE => "EGL_BAD_CURRENT_SURFACE",
					EGL_BAD_DISPLAY => "EGL_BAD_DISPLAY",
					EGL_BAD_MATCH => "EGL_BAD_MATCH",
					EGL_BAD_NATIVE_PIXMAP => "EGL_BAD_NATIVE_PIXMAP",
					EGL_BAD_NATIVE_WINDOW => "EGL_BAD_NATIVE_WINDOW",
					EGL_BAD_PARAMETER => "EGL_BAD_PARAMETER",
					EGL_BAD_SURFACE => "EGL_BAD_SURFACE",
					_ => $"Unknown error: 0x{error:X4}"
				};
			}
			catch
			{
				return "Unable to get error";
			}
		}

		private void Cleanup()
		{
			lock (_lock)
			{
				try
				{
					if (Display != IntPtr.Zero)
					{
						// Release current context
						eglMakeCurrent(Display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);

						// Destroy context
						if (Context != IntPtr.Zero)
						{
							eglDestroyContext(Display, Context);
							Context = IntPtr.Zero;
						}

						// Destroy surface
						if (Surface != IntPtr.Zero)
						{
							eglDestroySurface(Display, Surface);
							Surface = IntPtr.Zero;
						}

						// Terminate display
						eglTerminate(Display);
						Display = IntPtr.Zero;
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogWarning(ex, "Error during ANGLE/EGL cleanup");
				}

				Config = IntPtr.Zero;
				FlutterSharpLogger.LogDebug("ANGLE/EGL resources cleaned up");
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (_disposed)
				return;

			Cleanup();
			_disposed = true;
		}

		#endregion

		#region P/Invoke Declarations

		private const string EGLLibrary = "libEGL.dll";

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr eglGetPlatformDisplayEXT(
			int platform,
			IntPtr nativeDisplay,
			[MarshalAs(UnmanagedType.LPArray)] int[] attribList);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglInitialize(IntPtr display, out int major, out int minor);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglTerminate(IntPtr display);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern int eglGetError();

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglChooseConfig(
			IntPtr display,
			[MarshalAs(UnmanagedType.LPArray)] int[] attribList,
			out IntPtr config,
			int configSize,
			out int numConfig);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr eglCreateWindowSurface(
			IntPtr display,
			IntPtr config,
			IntPtr nativeWindow,
			[MarshalAs(UnmanagedType.LPArray)] int[] attribList);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglDestroySurface(IntPtr display, IntPtr surface);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr eglCreateContext(
			IntPtr display,
			IntPtr config,
			IntPtr shareContext,
			[MarshalAs(UnmanagedType.LPArray)] int[] attribList);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglDestroyContext(IntPtr display, IntPtr context);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglMakeCurrent(
			IntPtr display,
			IntPtr draw,
			IntPtr read,
			IntPtr context);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglSwapBuffers(IntPtr display, IntPtr surface);

		[DllImport(EGLLibrary, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool eglSwapInterval(IntPtr display, int interval);

		#endregion
	}
}
