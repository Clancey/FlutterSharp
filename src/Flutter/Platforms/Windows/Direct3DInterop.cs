using System;
using System.Runtime.InteropServices;
using Flutter.Logging;

namespace Flutter.Windows
{
	/// <summary>
	/// Direct3D 11 interop for Windows GPU rendering.
	/// Provides device creation, swapchain management, and texture handling.
	/// </summary>
	/// <remarks>
	/// This class wraps the essential Direct3D 11 and DXGI functionality needed
	/// for GPU-accelerated Flutter rendering on Windows. The Flutter engine uses
	/// ANGLE (OpenGL ES to Direct3D translation) for rendering, and this class
	/// provides the underlying Direct3D infrastructure.
	/// </remarks>
	internal class Direct3DInterop : IDisposable
	{
		#region Constants

		private const uint D3D11_SDK_VERSION = 7;
		private const uint DXGI_FORMAT_B8G8R8A8_UNORM = 87;
		private const uint DXGI_FORMAT_UNKNOWN = 0;
		private const uint DXGI_USAGE_RENDER_TARGET_OUTPUT = 0x20;
		private const uint DXGI_SWAP_EFFECT_FLIP_DISCARD = 4;
		private const uint DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL = 3;
		private const uint DXGI_SWAP_EFFECT_DISCARD = 0;
		private const uint DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH = 2;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Direct3D 11 device handle.
		/// </summary>
		public IntPtr Device { get; private set; }

		/// <summary>
		/// Gets the Direct3D 11 device context handle.
		/// </summary>
		public IntPtr DeviceContext { get; private set; }

		/// <summary>
		/// Gets the DXGI swapchain handle.
		/// </summary>
		public IntPtr SwapChain { get; private set; }

		/// <summary>
		/// Gets the render target view for the back buffer.
		/// </summary>
		public IntPtr RenderTargetView { get; private set; }

		/// <summary>
		/// Gets whether the Direct3D device is initialized.
		/// </summary>
		public bool IsInitialized => Device != IntPtr.Zero;

		/// <summary>
		/// Gets the current feature level of the device.
		/// </summary>
		public D3D_FEATURE_LEVEL FeatureLevel { get; private set; }

		/// <summary>
		/// Gets whether the device was created in debug mode.
		/// </summary>
		public bool IsDebugMode { get; private set; }

		/// <summary>
		/// Gets the current swapchain width in pixels.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the current swapchain height in pixels.
		/// </summary>
		public int Height { get; private set; }

		#endregion

		#region Fields

		private bool _disposed;
		private IntPtr _dxgiFactory;
		private IntPtr _dxgiAdapter;
		private readonly object _lock = new();

		#endregion

		#region Initialization

		/// <summary>
		/// Initializes the Direct3D 11 device with hardware acceleration.
		/// </summary>
		/// <param name="enableDebug">Enable debug layer for development</param>
		/// <returns>True if device was created successfully</returns>
		public bool InitializeDevice(bool enableDebug = false)
		{
			lock (_lock)
			{
				if (IsInitialized)
				{
					FlutterSharpLogger.LogDebug("Direct3D device already initialized");
					return true;
				}

				try
				{
					IsDebugMode = enableDebug;

					// Define feature levels to try in order of preference
					var featureLevels = new[]
					{
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_1,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_0,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_3,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_2,
						D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_1,
					};

					// Set up creation flags
					uint flags = 0;
					if (enableDebug)
					{
						flags |= (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
					}
					// Enable BGRA support for interop with Direct2D and WIC
					flags |= (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;

					// Try to create hardware device
					var hr = D3D11CreateDevice(
						IntPtr.Zero,                    // Default adapter
						D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
						IntPtr.Zero,                    // No software module
						flags,
						featureLevels,
						(uint)featureLevels.Length,
						D3D11_SDK_VERSION,
						out var device,
						out var featureLevel,
						out var deviceContext);

					if (hr < 0)
					{
						// Try WARP software renderer as fallback
						FlutterSharpLogger.LogWarning("Hardware D3D11 failed (HRESULT: 0x{Hr:X8}), trying WARP", hr);

						hr = D3D11CreateDevice(
							IntPtr.Zero,
							D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_WARP,
							IntPtr.Zero,
							flags,
							featureLevels,
							(uint)featureLevels.Length,
							D3D11_SDK_VERSION,
							out device,
							out featureLevel,
							out deviceContext);

						if (hr < 0)
						{
							FlutterSharpLogger.LogError("Failed to create D3D11 device (HRESULT: 0x{Hr:X8})", hr);
							return false;
						}

						FlutterSharpLogger.LogInformation("Using WARP software renderer");
					}

					Device = device;
					DeviceContext = deviceContext;
					FeatureLevel = featureLevel;

					FlutterSharpLogger.LogInformation(
						"Direct3D 11 device created successfully. Feature Level: {FeatureLevel}",
						FeatureLevel);

					return true;
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Exception creating Direct3D device");
					Cleanup();
					return false;
				}
			}
		}

		/// <summary>
		/// Creates a DXGI swapchain for the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to create swapchain for</param>
		/// <param name="width">Initial width in pixels</param>
		/// <param name="height">Initial height in pixels</param>
		/// <returns>True if swapchain was created successfully</returns>
		public bool CreateSwapChain(IntPtr hwnd, int width, int height)
		{
			if (!IsInitialized)
			{
				FlutterSharpLogger.LogError("Cannot create swapchain: Device not initialized");
				return false;
			}

			if (hwnd == IntPtr.Zero)
			{
				FlutterSharpLogger.LogError("Cannot create swapchain: Invalid window handle");
				return false;
			}

			lock (_lock)
			{
				try
				{
					Width = Math.Max(1, width);
					Height = Math.Max(1, height);

					// Get the DXGI device from D3D11 device
					var iidDxgiDevice = new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
					var hr = Marshal.QueryInterface(Device, ref iidDxgiDevice, out var dxgiDevice);
					if (hr < 0)
					{
						FlutterSharpLogger.LogError("Failed to get DXGI device (HRESULT: 0x{Hr:X8})", hr);
						return false;
					}

					try
					{
						// Get the adapter from DXGI device
						hr = DXGIDeviceGetAdapter(dxgiDevice, out _dxgiAdapter);
						if (hr < 0)
						{
							FlutterSharpLogger.LogError("Failed to get DXGI adapter (HRESULT: 0x{Hr:X8})", hr);
							return false;
						}

						// Get the factory from adapter
						var iidFactory = new Guid("7b7166ec-21c7-44ae-b21a-c9ae321ae369");
						hr = DXGIObjectGetParent(_dxgiAdapter, ref iidFactory, out _dxgiFactory);
						if (hr < 0)
						{
							FlutterSharpLogger.LogError("Failed to get DXGI factory (HRESULT: 0x{Hr:X8})", hr);
							return false;
						}

						// Create swapchain description
						var swapChainDesc = new DXGI_SWAP_CHAIN_DESC
						{
							BufferDesc = new DXGI_MODE_DESC
							{
								Width = (uint)Width,
								Height = (uint)Height,
								RefreshRate = new DXGI_RATIONAL { Numerator = 60, Denominator = 1 },
								Format = DXGI_FORMAT_B8G8R8A8_UNORM,
								ScanlineOrdering = 0,
								Scaling = 0
							},
							SampleDesc = new DXGI_SAMPLE_DESC { Count = 1, Quality = 0 },
							BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
							BufferCount = 2,
							OutputWindow = hwnd,
							Windowed = 1,  // TRUE
							SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD,
							Flags = 0
						};

						// Create swapchain
						hr = IDXGIFactory_CreateSwapChain(_dxgiFactory, Device, ref swapChainDesc, out var swapChain);
						if (hr < 0)
						{
							// Try with FLIP_SEQUENTIAL for older Windows versions
							swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
							hr = IDXGIFactory_CreateSwapChain(_dxgiFactory, Device, ref swapChainDesc, out swapChain);
						}
						if (hr < 0)
						{
							// Try with DISCARD for even older systems
							swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
							hr = IDXGIFactory_CreateSwapChain(_dxgiFactory, Device, ref swapChainDesc, out swapChain);
						}

						if (hr < 0)
						{
							FlutterSharpLogger.LogError("Failed to create swapchain (HRESULT: 0x{Hr:X8})", hr);
							return false;
						}

						SwapChain = swapChain;

						// Create render target view for back buffer
						if (!CreateRenderTargetView())
						{
							return false;
						}

						FlutterSharpLogger.LogInformation(
							"Direct3D swapchain created: {Width}x{Height}",
							Width, Height);

						return true;
					}
					finally
					{
						if (dxgiDevice != IntPtr.Zero)
						{
							Marshal.Release(dxgiDevice);
						}
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Exception creating swapchain");
					ReleaseSwapChain();
					return false;
				}
			}
		}

		/// <summary>
		/// Resizes the swapchain buffers.
		/// </summary>
		/// <param name="width">New width in pixels</param>
		/// <param name="height">New height in pixels</param>
		/// <returns>True if resize was successful</returns>
		public bool ResizeBuffers(int width, int height)
		{
			if (SwapChain == IntPtr.Zero)
			{
				return false;
			}

			lock (_lock)
			{
				try
				{
					Width = Math.Max(1, width);
					Height = Math.Max(1, height);

					// Release existing render target view
					if (RenderTargetView != IntPtr.Zero)
					{
						Marshal.Release(RenderTargetView);
						RenderTargetView = IntPtr.Zero;
					}

					// Resize swapchain buffers
					var hr = IDXGISwapChain_ResizeBuffers(
						SwapChain,
						2,
						(uint)Width,
						(uint)Height,
						DXGI_FORMAT_UNKNOWN,
						0);

					if (hr < 0)
					{
						FlutterSharpLogger.LogError("Failed to resize swapchain buffers (HRESULT: 0x{Hr:X8})", hr);
						return false;
					}

					// Recreate render target view
					if (!CreateRenderTargetView())
					{
						return false;
					}

					FlutterSharpLogger.LogDebug("Swapchain resized to {Width}x{Height}", Width, Height);
					return true;
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Exception resizing swapchain buffers");
					return false;
				}
			}
		}

		/// <summary>
		/// Presents the rendered frame to the display.
		/// </summary>
		/// <param name="vsync">Use vertical sync (true = 1, false = 0)</param>
		/// <returns>True if present was successful</returns>
		public bool Present(bool vsync = true)
		{
			if (SwapChain == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				var syncInterval = vsync ? 1u : 0u;
				var hr = IDXGISwapChain_Present(SwapChain, syncInterval, 0);

				if (hr < 0)
				{
					FlutterSharpLogger.LogWarning("Present failed (HRESULT: 0x{Hr:X8})", hr);
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception during present");
				return false;
			}
		}

		#endregion

		#region Private Helpers

		private bool CreateRenderTargetView()
		{
			if (SwapChain == IntPtr.Zero || Device == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				// Get back buffer from swapchain
				var iidTexture2D = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
				var hr = IDXGISwapChain_GetBuffer(SwapChain, 0, ref iidTexture2D, out var backBuffer);

				if (hr < 0)
				{
					FlutterSharpLogger.LogError("Failed to get back buffer (HRESULT: 0x{Hr:X8})", hr);
					return false;
				}

				try
				{
					// Create render target view for back buffer
					hr = ID3D11Device_CreateRenderTargetView(Device, backBuffer, IntPtr.Zero, out var rtv);

					if (hr < 0)
					{
						FlutterSharpLogger.LogError("Failed to create render target view (HRESULT: 0x{Hr:X8})", hr);
						return false;
					}

					RenderTargetView = rtv;
					return true;
				}
				finally
				{
					if (backBuffer != IntPtr.Zero)
					{
						Marshal.Release(backBuffer);
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Exception creating render target view");
				return false;
			}
		}

		private void ReleaseSwapChain()
		{
			if (RenderTargetView != IntPtr.Zero)
			{
				Marshal.Release(RenderTargetView);
				RenderTargetView = IntPtr.Zero;
			}

			if (SwapChain != IntPtr.Zero)
			{
				Marshal.Release(SwapChain);
				SwapChain = IntPtr.Zero;
			}

			if (_dxgiFactory != IntPtr.Zero)
			{
				Marshal.Release(_dxgiFactory);
				_dxgiFactory = IntPtr.Zero;
			}

			if (_dxgiAdapter != IntPtr.Zero)
			{
				Marshal.Release(_dxgiAdapter);
				_dxgiAdapter = IntPtr.Zero;
			}
		}

		private void Cleanup()
		{
			lock (_lock)
			{
				ReleaseSwapChain();

				if (DeviceContext != IntPtr.Zero)
				{
					Marshal.Release(DeviceContext);
					DeviceContext = IntPtr.Zero;
				}

				if (Device != IntPtr.Zero)
				{
					Marshal.Release(Device);
					Device = IntPtr.Zero;
				}

				FlutterSharpLogger.LogDebug("Direct3D resources cleaned up");
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

		#region Enums and Structs

		/// <summary>
		/// Direct3D feature levels.
		/// </summary>
		public enum D3D_FEATURE_LEVEL
		{
			D3D_FEATURE_LEVEL_9_1 = 0x9100,
			D3D_FEATURE_LEVEL_9_2 = 0x9200,
			D3D_FEATURE_LEVEL_9_3 = 0x9300,
			D3D_FEATURE_LEVEL_10_0 = 0xa000,
			D3D_FEATURE_LEVEL_10_1 = 0xa100,
			D3D_FEATURE_LEVEL_11_0 = 0xb000,
			D3D_FEATURE_LEVEL_11_1 = 0xb100,
			D3D_FEATURE_LEVEL_12_0 = 0xc000,
			D3D_FEATURE_LEVEL_12_1 = 0xc100,
		}

		/// <summary>
		/// Direct3D driver types.
		/// </summary>
		private enum D3D_DRIVER_TYPE
		{
			D3D_DRIVER_TYPE_UNKNOWN = 0,
			D3D_DRIVER_TYPE_HARDWARE = 1,
			D3D_DRIVER_TYPE_REFERENCE = 2,
			D3D_DRIVER_TYPE_NULL = 3,
			D3D_DRIVER_TYPE_SOFTWARE = 4,
			D3D_DRIVER_TYPE_WARP = 5,
		}

		/// <summary>
		/// Device creation flags.
		/// </summary>
		[Flags]
		private enum D3D11_CREATE_DEVICE_FLAG : uint
		{
			D3D11_CREATE_DEVICE_SINGLETHREADED = 0x1,
			D3D11_CREATE_DEVICE_DEBUG = 0x2,
			D3D11_CREATE_DEVICE_SWITCH_TO_REF = 0x4,
			D3D11_CREATE_DEVICE_PREVENT_INTERNAL_THREADING_OPTIMIZATIONS = 0x8,
			D3D11_CREATE_DEVICE_BGRA_SUPPORT = 0x20,
			D3D11_CREATE_DEVICE_DEBUGGABLE = 0x40,
			D3D11_CREATE_DEVICE_PREVENT_ALTERING_LAYER_SETTINGS_FROM_REGISTRY = 0x80,
			D3D11_CREATE_DEVICE_DISABLE_GPU_TIMEOUT = 0x100,
			D3D11_CREATE_DEVICE_VIDEO_SUPPORT = 0x800,
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DXGI_RATIONAL
		{
			public uint Numerator;
			public uint Denominator;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DXGI_MODE_DESC
		{
			public uint Width;
			public uint Height;
			public DXGI_RATIONAL RefreshRate;
			public uint Format;
			public uint ScanlineOrdering;
			public uint Scaling;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DXGI_SAMPLE_DESC
		{
			public uint Count;
			public uint Quality;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DXGI_SWAP_CHAIN_DESC
		{
			public DXGI_MODE_DESC BufferDesc;
			public DXGI_SAMPLE_DESC SampleDesc;
			public uint BufferUsage;
			public uint BufferCount;
			public IntPtr OutputWindow;
			public int Windowed;
			public uint SwapEffect;
			public uint Flags;
		}

		#endregion

		#region P/Invoke Declarations

		// Direct3D 11
		[DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
		private static extern int D3D11CreateDevice(
			IntPtr pAdapter,
			D3D_DRIVER_TYPE driverType,
			IntPtr software,
			uint flags,
			[MarshalAs(UnmanagedType.LPArray)] D3D_FEATURE_LEVEL[] pFeatureLevels,
			uint featureLevels,
			uint sdkVersion,
			out IntPtr ppDevice,
			out D3D_FEATURE_LEVEL pFeatureLevel,
			out IntPtr ppImmediateContext);

		// DXGI device methods (vtable offsets)
		[DllImport("d3d11.dll", EntryPoint = "?", CallingConvention = CallingConvention.StdCall)]
		private static extern int DXGIDeviceGetAdapter(IntPtr device, out IntPtr adapter);

		// We'll use manual vtable calls for COM interface methods
		private static int DXGIObjectGetParent(IntPtr obj, ref Guid riid, out IntPtr parent)
		{
			// IDXGIObject::GetParent is at vtable index 6
			var vtable = Marshal.ReadIntPtr(obj);
			var getParent = Marshal.ReadIntPtr(vtable, 6 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<DXGIObjectGetParentDelegate>(getParent);
			return func(obj, ref riid, out parent);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DXGIObjectGetParentDelegate(IntPtr obj, ref Guid riid, out IntPtr parent);

		private static int IDXGIFactory_CreateSwapChain(IntPtr factory, IntPtr device, ref DXGI_SWAP_CHAIN_DESC desc, out IntPtr swapchain)
		{
			// IDXGIFactory::CreateSwapChain is at vtable index 10
			var vtable = Marshal.ReadIntPtr(factory);
			var createSwapChain = Marshal.ReadIntPtr(vtable, 10 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<IDXGIFactoryCreateSwapChainDelegate>(createSwapChain);
			return func(factory, device, ref desc, out swapchain);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int IDXGIFactoryCreateSwapChainDelegate(
			IntPtr factory, IntPtr device, ref DXGI_SWAP_CHAIN_DESC desc, out IntPtr swapchain);

		private static int IDXGISwapChain_GetBuffer(IntPtr swapchain, uint buffer, ref Guid riid, out IntPtr surface)
		{
			// IDXGISwapChain::GetBuffer is at vtable index 9
			var vtable = Marshal.ReadIntPtr(swapchain);
			var getBuffer = Marshal.ReadIntPtr(vtable, 9 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<IDXGISwapChainGetBufferDelegate>(getBuffer);
			return func(swapchain, buffer, ref riid, out surface);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int IDXGISwapChainGetBufferDelegate(IntPtr swapchain, uint buffer, ref Guid riid, out IntPtr surface);

		private static int IDXGISwapChain_Present(IntPtr swapchain, uint syncInterval, uint flags)
		{
			// IDXGISwapChain::Present is at vtable index 8
			var vtable = Marshal.ReadIntPtr(swapchain);
			var present = Marshal.ReadIntPtr(vtable, 8 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<IDXGISwapChainPresentDelegate>(present);
			return func(swapchain, syncInterval, flags);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int IDXGISwapChainPresentDelegate(IntPtr swapchain, uint syncInterval, uint flags);

		private static int IDXGISwapChain_ResizeBuffers(IntPtr swapchain, uint bufferCount, uint width, uint height, uint format, uint flags)
		{
			// IDXGISwapChain::ResizeBuffers is at vtable index 13
			var vtable = Marshal.ReadIntPtr(swapchain);
			var resizeBuffers = Marshal.ReadIntPtr(vtable, 13 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<IDXGISwapChainResizeBuffersDelegate>(resizeBuffers);
			return func(swapchain, bufferCount, width, height, format, flags);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int IDXGISwapChainResizeBuffersDelegate(
			IntPtr swapchain, uint bufferCount, uint width, uint height, uint format, uint flags);

		private static int ID3D11Device_CreateRenderTargetView(IntPtr device, IntPtr resource, IntPtr desc, out IntPtr rtv)
		{
			// ID3D11Device::CreateRenderTargetView is at vtable index 9
			var vtable = Marshal.ReadIntPtr(device);
			var createRTV = Marshal.ReadIntPtr(vtable, 9 * IntPtr.Size);
			var func = Marshal.GetDelegateForFunctionPointer<ID3D11DeviceCreateRenderTargetViewDelegate>(createRTV);
			return func(device, resource, desc, out rtv);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int ID3D11DeviceCreateRenderTargetViewDelegate(
			IntPtr device, IntPtr resource, IntPtr desc, out IntPtr rtv);

		#endregion
	}
}
