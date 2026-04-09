using System;
using Flutter.Logging;

namespace Flutter.Windows
{
	/// <summary>
	/// Manages GPU rendering for Flutter on Windows using ANGLE and Direct3D.
	/// </summary>
	/// <remarks>
	/// This class coordinates the rendering pipeline between:
	/// - Flutter engine (renders via ANGLE/OpenGL ES)
	/// - ANGLE (translates OpenGL ES to Direct3D)
	/// - Direct3D 11 (native Windows GPU API)
	///
	/// The Flutter Windows embedder typically handles rendering internally,
	/// but this class provides integration points for:
	/// - Custom compositing
	/// - Frame metrics collection
	/// - GPU resource management
	/// - Performance monitoring
	/// </remarks>
	public class FlutterWindowsRendering : IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets the Direct3D interop instance.
		/// </summary>
		internal Direct3DInterop? Direct3D { get; private set; }

		/// <summary>
		/// Gets the ANGLE interop instance.
		/// </summary>
		internal ANGLEInterop? ANGLE { get; private set; }

		/// <summary>
		/// Gets whether GPU rendering is initialized.
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Gets the current rendering mode.
		/// </summary>
		public RenderingMode Mode { get; private set; } = RenderingMode.Automatic;

		/// <summary>
		/// Gets the active rendering backend.
		/// </summary>
		public RenderingBackend ActiveBackend { get; private set; } = RenderingBackend.None;

		/// <summary>
		/// Gets rendering statistics.
		/// </summary>
		public RenderingStats Stats { get; } = new();

		/// <summary>
		/// Gets or sets whether vsync is enabled.
		/// </summary>
		public bool VsyncEnabled
		{
			get => _vsyncEnabled;
			set
			{
				_vsyncEnabled = value;
				ANGLE?.SetSwapInterval(value ? 1 : 0);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Raised when a frame is rendered.
		/// </summary>
		public event EventHandler<FrameRenderedEventArgs>? FrameRendered;

		/// <summary>
		/// Raised when rendering backend changes.
		/// </summary>
		public event EventHandler<RenderingBackendChangedEventArgs>? BackendChanged;

		#endregion

		#region Fields

		private bool _disposed;
		private bool _vsyncEnabled = true;
		private IntPtr _hwnd;
		private int _width;
		private int _height;
		private readonly object _lock = new();
		private long _frameCount;
		private DateTime _lastFrameTime = DateTime.UtcNow;

		#endregion

		#region Initialization

		/// <summary>
		/// Initializes GPU rendering for the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to render to</param>
		/// <param name="width">Initial width in pixels</param>
		/// <param name="height">Initial height in pixels</param>
		/// <param name="mode">Rendering mode (Automatic, Hardware, Software)</param>
		/// <returns>True if initialization was successful</returns>
		public bool Initialize(IntPtr hwnd, int width, int height, RenderingMode mode = RenderingMode.Automatic)
		{
			if (hwnd == IntPtr.Zero)
			{
				FlutterSharpLogger.LogError("Cannot initialize rendering: Invalid window handle");
				return false;
			}

			lock (_lock)
			{
				if (IsInitialized)
				{
					FlutterSharpLogger.LogDebug("Rendering already initialized");
					return true;
				}

				_hwnd = hwnd;
				_width = Math.Max(1, width);
				_height = Math.Max(1, height);
				Mode = mode;

				FlutterSharpLogger.LogInformation(
					"Initializing Windows GPU rendering: {Width}x{Height}, Mode: {Mode}",
					_width, _height, mode);

				bool success = false;

				switch (mode)
				{
					case RenderingMode.Hardware:
						success = TryInitializeHardwareRendering();
						break;

					case RenderingMode.Software:
						success = TryInitializeSoftwareRendering();
						break;

					case RenderingMode.Automatic:
					default:
						// Try hardware first, fall back to software
						success = TryInitializeHardwareRendering();
						if (!success)
						{
							FlutterSharpLogger.LogInformation("Hardware rendering failed, trying software fallback");
							success = TryInitializeSoftwareRendering();
						}
						break;
				}

				IsInitialized = success;

				if (success)
				{
					FlutterSharpLogger.LogInformation(
						"Windows GPU rendering initialized successfully. Backend: {Backend}",
						ActiveBackend);
				}
				else
				{
					FlutterSharpLogger.LogWarning("Windows GPU rendering initialization failed");
				}

				return success;
			}
		}

		private bool TryInitializeHardwareRendering()
		{
			// Try ANGLE first (Flutter's preferred rendering path)
			if (ANGLEInterop.IsAngleAvailable)
			{
				try
				{
					ANGLE = new ANGLEInterop();
					if (ANGLE.Initialize(_hwnd, useD3D11: true))
					{
						ANGLE.SetSwapInterval(_vsyncEnabled ? 1 : 0);
						ActiveBackend = RenderingBackend.ANGLE_D3D11;
						RaiseBackendChanged(RenderingBackend.None, ActiveBackend);
						return true;
					}

					// Try D3D9 fallback via ANGLE
					ANGLE.Dispose();
					ANGLE = new ANGLEInterop();
					if (ANGLE.Initialize(_hwnd, useD3D11: false))
					{
						ANGLE.SetSwapInterval(_vsyncEnabled ? 1 : 0);
						ActiveBackend = RenderingBackend.ANGLE_D3D9;
						RaiseBackendChanged(RenderingBackend.None, ActiveBackend);
						return true;
					}

					ANGLE.Dispose();
					ANGLE = null;
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogWarning(ex, "ANGLE initialization failed");
					ANGLE?.Dispose();
					ANGLE = null;
				}
			}

			// Try Direct3D 11 directly
			try
			{
				Direct3D = new Direct3DInterop();
				bool debugMode = false;
#if DEBUG
				debugMode = true;
#endif
				if (Direct3D.InitializeDevice(debugMode))
				{
					if (Direct3D.CreateSwapChain(_hwnd, _width, _height))
					{
						ActiveBackend = RenderingBackend.Direct3D11;
						RaiseBackendChanged(RenderingBackend.None, ActiveBackend);
						return true;
					}
				}

				Direct3D.Dispose();
				Direct3D = null;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogWarning(ex, "Direct3D initialization failed");
				Direct3D?.Dispose();
				Direct3D = null;
			}

			return false;
		}

		private bool TryInitializeSoftwareRendering()
		{
			// Software rendering uses CPU-based rasterization
			// The Flutter embedder handles this internally
			ActiveBackend = RenderingBackend.Software;
			RaiseBackendChanged(RenderingBackend.None, ActiveBackend);
			FlutterSharpLogger.LogInformation("Using software rendering (CPU-based)");
			return true;
		}

		#endregion

		#region Rendering Operations

		/// <summary>
		/// Notifies that a frame has been rendered.
		/// </summary>
		/// <remarks>
		/// Called by the Flutter engine or compositor after completing a frame.
		/// Updates statistics and raises the FrameRendered event.
		/// </remarks>
		public void OnFrameRendered()
		{
			var now = DateTime.UtcNow;
			var frameDuration = now - _lastFrameTime;
			_lastFrameTime = now;
			_frameCount++;

			Stats.RecordFrame(frameDuration);

			FrameRendered?.Invoke(this, new FrameRenderedEventArgs(
				_frameCount,
				frameDuration,
				Stats.CurrentFPS));
		}

		/// <summary>
		/// Presents the current frame to the display.
		/// </summary>
		/// <returns>True if present was successful</returns>
		public bool Present()
		{
			if (!IsInitialized)
				return false;

			try
			{
				bool result;

				switch (ActiveBackend)
				{
					case RenderingBackend.ANGLE_D3D11:
					case RenderingBackend.ANGLE_D3D9:
						result = ANGLE?.SwapBuffers() ?? false;
						break;

					case RenderingBackend.Direct3D11:
						result = Direct3D?.Present(_vsyncEnabled) ?? false;
						break;

					case RenderingBackend.Software:
						// Software rendering presents via GDI/WM_PAINT
						result = true;
						break;

					default:
						result = false;
						break;
				}

				if (result)
				{
					OnFrameRendered();
				}

				return result;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Present failed");
				return false;
			}
		}

		/// <summary>
		/// Updates the rendering size.
		/// </summary>
		/// <param name="width">New width in pixels</param>
		/// <param name="height">New height in pixels</param>
		/// <returns>True if resize was successful</returns>
		public bool UpdateSize(int width, int height)
		{
			if (width <= 0 || height <= 0)
				return false;

			if (_width == width && _height == height)
				return true;

			lock (_lock)
			{
				_width = width;
				_height = height;

				FlutterSharpLogger.LogDebug("Updating rendering size to {Width}x{Height}", width, height);

				try
				{
					switch (ActiveBackend)
					{
						case RenderingBackend.Direct3D11:
							return Direct3D?.ResizeBuffers(width, height) ?? false;

						case RenderingBackend.ANGLE_D3D11:
						case RenderingBackend.ANGLE_D3D9:
							// ANGLE handles resize via EGL surface recreation
							// The Flutter embedder typically manages this
							return true;

						case RenderingBackend.Software:
							// Software rendering resizes automatically
							return true;

						default:
							return false;
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Failed to update rendering size");
					return false;
				}
			}
		}

		/// <summary>
		/// Makes the rendering context current for the calling thread.
		/// </summary>
		/// <returns>True if successful</returns>
		public bool MakeContextCurrent()
		{
			if (!IsInitialized)
				return false;

			switch (ActiveBackend)
			{
				case RenderingBackend.ANGLE_D3D11:
				case RenderingBackend.ANGLE_D3D9:
					return ANGLE?.MakeCurrent() ?? false;

				case RenderingBackend.Direct3D11:
					// D3D11 doesn't have a current context model like OpenGL
					return true;

				case RenderingBackend.Software:
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Releases the rendering context from the current thread.
		/// </summary>
		/// <returns>True if successful</returns>
		public bool ReleaseContext()
		{
			if (!IsInitialized)
				return false;

			switch (ActiveBackend)
			{
				case RenderingBackend.ANGLE_D3D11:
				case RenderingBackend.ANGLE_D3D9:
					return ANGLE?.ReleaseCurrent() ?? false;

				case RenderingBackend.Direct3D11:
				case RenderingBackend.Software:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Helpers

		private void RaiseBackendChanged(RenderingBackend oldBackend, RenderingBackend newBackend)
		{
			BackendChanged?.Invoke(this, new RenderingBackendChangedEventArgs(oldBackend, newBackend));
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (_disposed)
				return;

			lock (_lock)
			{
				ANGLE?.Dispose();
				ANGLE = null;

				Direct3D?.Dispose();
				Direct3D = null;

				IsInitialized = false;
				ActiveBackend = RenderingBackend.None;
				_disposed = true;

				FlutterSharpLogger.LogDebug("FlutterWindowsRendering disposed");
			}
		}

		#endregion
	}

	#region Supporting Types

	/// <summary>
	/// Rendering mode selection.
	/// </summary>
	public enum RenderingMode
	{
		/// <summary>
		/// Automatically select best available rendering backend.
		/// </summary>
		Automatic,

		/// <summary>
		/// Force hardware (GPU) rendering.
		/// </summary>
		Hardware,

		/// <summary>
		/// Force software (CPU) rendering.
		/// </summary>
		Software
	}

	/// <summary>
	/// Active rendering backend.
	/// </summary>
	public enum RenderingBackend
	{
		/// <summary>
		/// No rendering backend active.
		/// </summary>
		None,

		/// <summary>
		/// ANGLE with Direct3D 11 backend.
		/// </summary>
		ANGLE_D3D11,

		/// <summary>
		/// ANGLE with Direct3D 9 backend.
		/// </summary>
		ANGLE_D3D9,

		/// <summary>
		/// Direct3D 11 directly (without ANGLE).
		/// </summary>
		Direct3D11,

		/// <summary>
		/// Software/CPU rendering.
		/// </summary>
		Software
	}

	/// <summary>
	/// Event args for frame rendered event.
	/// </summary>
	public class FrameRenderedEventArgs : EventArgs
	{
		public long FrameNumber { get; }
		public TimeSpan FrameDuration { get; }
		public double CurrentFPS { get; }

		public FrameRenderedEventArgs(long frameNumber, TimeSpan frameDuration, double fps)
		{
			FrameNumber = frameNumber;
			FrameDuration = frameDuration;
			CurrentFPS = fps;
		}
	}

	/// <summary>
	/// Event args for backend changed event.
	/// </summary>
	public class RenderingBackendChangedEventArgs : EventArgs
	{
		public RenderingBackend OldBackend { get; }
		public RenderingBackend NewBackend { get; }

		public RenderingBackendChangedEventArgs(RenderingBackend oldBackend, RenderingBackend newBackend)
		{
			OldBackend = oldBackend;
			NewBackend = newBackend;
		}
	}

	/// <summary>
	/// Rendering statistics.
	/// </summary>
	public class RenderingStats
	{
		private const int SampleCount = 60; // 1 second at 60 FPS
		private readonly TimeSpan[] _frameDurations = new TimeSpan[SampleCount];
		private int _currentIndex;
		private int _samplesFilled;
		private readonly object _lock = new();

		/// <summary>
		/// Gets the current FPS (frames per second).
		/// </summary>
		public double CurrentFPS
		{
			get
			{
				lock (_lock)
				{
					if (_samplesFilled == 0)
						return 0;

					var totalMs = 0.0;
					var count = Math.Min(_samplesFilled, SampleCount);
					for (int i = 0; i < count; i++)
					{
						totalMs += _frameDurations[i].TotalMilliseconds;
					}

					var avgMs = totalMs / count;
					return avgMs > 0 ? 1000.0 / avgMs : 0;
				}
			}
		}

		/// <summary>
		/// Gets the average frame time in milliseconds.
		/// </summary>
		public double AverageFrameTimeMs
		{
			get
			{
				lock (_lock)
				{
					if (_samplesFilled == 0)
						return 0;

					var totalMs = 0.0;
					var count = Math.Min(_samplesFilled, SampleCount);
					for (int i = 0; i < count; i++)
					{
						totalMs += _frameDurations[i].TotalMilliseconds;
					}

					return totalMs / count;
				}
			}
		}

		/// <summary>
		/// Gets the last frame time in milliseconds.
		/// </summary>
		public double LastFrameTimeMs { get; private set; }

		/// <summary>
		/// Gets the total number of frames rendered.
		/// </summary>
		public long TotalFrames { get; private set; }

		/// <summary>
		/// Records a frame duration.
		/// </summary>
		public void RecordFrame(TimeSpan duration)
		{
			lock (_lock)
			{
				_frameDurations[_currentIndex] = duration;
				_currentIndex = (_currentIndex + 1) % SampleCount;
				if (_samplesFilled < SampleCount)
					_samplesFilled++;

				LastFrameTimeMs = duration.TotalMilliseconds;
				TotalFrames++;
			}
		}

		/// <summary>
		/// Resets all statistics.
		/// </summary>
		public void Reset()
		{
			lock (_lock)
			{
				Array.Clear(_frameDurations, 0, SampleCount);
				_currentIndex = 0;
				_samplesFilled = 0;
				LastFrameTimeMs = 0;
				TotalFrames = 0;
			}
		}
	}

	#endregion
}
