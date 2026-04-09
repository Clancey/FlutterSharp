using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CoreGraphics;
using Flutter.Logging;

namespace Flutter.macOS
{
	/// <summary>
	/// Information about a managed Flutter window.
	/// </summary>
	public class FlutterWindowInfo
	{
		/// <summary>
		/// The unique window ID.
		/// </summary>
		public nint WindowId { get; internal set; }

		/// <summary>
		/// The native NSWindow handle.
		/// </summary>
		public IntPtr WindowHandle { get; internal set; }

		/// <summary>
		/// The Flutter view controller managing this window.
		/// </summary>
		public FlutterMacOSViewController? ViewController { get; internal set; }

		/// <summary>
		/// The window title.
		/// </summary>
		public string Title { get; internal set; } = string.Empty;

		/// <summary>
		/// Whether this is the main application window.
		/// </summary>
		public bool IsMainWindow { get; internal set; }

		/// <summary>
		/// The window frame rectangle.
		/// </summary>
		public CGRect Frame { get; internal set; }

		/// <summary>
		/// The backing scale factor (DPI).
		/// </summary>
		public nfloat ScaleFactor { get; internal set; } = 1.0f;

		/// <summary>
		/// Creation timestamp.
		/// </summary>
		public DateTime CreatedAt { get; internal set; }
	}

	/// <summary>
	/// Event arguments for window lifecycle events.
	/// </summary>
	public class WindowEventArgs : EventArgs
	{
		/// <summary>
		/// The window info.
		/// </summary>
		public FlutterWindowInfo Window { get; }

		public WindowEventArgs(FlutterWindowInfo window)
		{
			Window = window;
		}
	}

	/// <summary>
	/// Manages multiple Flutter windows on macOS.
	/// </summary>
	/// <remarks>
	/// This class provides:
	/// - Window creation and lifecycle management
	/// - Window registry for tracking all open windows
	/// - Focus management and window ordering
	/// - Multi-monitor support
	/// - Window state persistence
	/// </remarks>
	public class MacOSWindowManager : IDisposable
	{
		private static MacOSWindowManager? _instance;
		private static readonly object _instanceLock = new();

		private readonly ConcurrentDictionary<nint, FlutterWindowInfo> _windows = new();
		private nint _mainWindowId;
		private bool _disposed;

		#region Singleton

		/// <summary>
		/// Gets the singleton instance of the window manager.
		/// </summary>
		public static MacOSWindowManager Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_instanceLock)
					{
						_instance ??= new MacOSWindowManager();
					}
				}
				return _instance;
			}
		}

		private MacOSWindowManager()
		{
			FlutterSharpLogger.LogDebug("MacOSWindowManager: Initialized");
		}

		#endregion

		#region Events

		/// <summary>
		/// Raised when a window is created.
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowCreated;

		/// <summary>
		/// Raised when a window is closed.
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowClosed;

		/// <summary>
		/// Raised when a window becomes the key window (focused).
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowBecameKey;

		/// <summary>
		/// Raised when a window resigns key window (lost focus).
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowResignedKey;

		/// <summary>
		/// Raised when a window is moved.
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowMoved;

		/// <summary>
		/// Raised when a window is resized.
		/// </summary>
		public event EventHandler<WindowEventArgs>? WindowResized;

		#endregion

		#region Properties

		/// <summary>
		/// Gets all registered windows.
		/// </summary>
		public IEnumerable<FlutterWindowInfo> AllWindows => _windows.Values;

		/// <summary>
		/// Gets the number of open windows.
		/// </summary>
		public int WindowCount => _windows.Count;

		/// <summary>
		/// Gets the main window info, or null if no main window.
		/// </summary>
		public FlutterWindowInfo? MainWindow =>
			_mainWindowId != 0 && _windows.TryGetValue(_mainWindowId, out var window) ? window : null;

		#endregion

		#region Window Creation

		/// <summary>
		/// Creates a new Flutter window with default settings.
		/// </summary>
		/// <param name="title">Window title</param>
		/// <param name="width">Window width</param>
		/// <param name="height">Window height</param>
		/// <param name="isMainWindow">Whether this is the main application window</param>
		/// <returns>Window info, or null if creation failed</returns>
		public FlutterWindowInfo? CreateWindow(string title, int width = 800, int height = 600, bool isMainWindow = false)
		{
			return CreateWindow(title, new CGRect(0, 0, width, height), NSWindowStyleMask.Standard, isMainWindow);
		}

		/// <summary>
		/// Creates a new Flutter window with custom settings.
		/// </summary>
		/// <param name="title">Window title</param>
		/// <param name="contentRect">Content rectangle</param>
		/// <param name="styleMask">Window style</param>
		/// <param name="isMainWindow">Whether this is the main application window</param>
		/// <returns>Window info, or null if creation failed</returns>
		public FlutterWindowInfo? CreateWindow(string title, CGRect contentRect, NSWindowStyleMask styleMask, bool isMainWindow = false)
		{
			try
			{
				// Create the native window
				var windowHandle = WindowInterop.CreateWindow(contentRect, styleMask);
				if (windowHandle == IntPtr.Zero)
				{
					FlutterSharpLogger.LogError("MacOSWindowManager: Failed to create native window");
					return null;
				}

				// Set window properties
				WindowInterop.SetTitle(windowHandle, title);
				WindowInterop.SetReleasedWhenClosed(windowHandle, false);

				// Enable fullscreen support
				WindowInterop.SetCollectionBehavior(windowHandle,
					NSWindowCollectionBehavior.FullScreenPrimary |
					NSWindowCollectionBehavior.Managed);

				// Get the window number as ID
				var windowId = WindowInterop.GetWindowNumber(windowHandle);

				// Create Flutter view controller for this window
				FlutterMacOSViewController? viewController = null;
				try
				{
					viewController = new FlutterMacOSViewController();

					// Set the Flutter view as the window's content view
					var flutterView = viewController.NativeViewHandle;
					if (flutterView != IntPtr.Zero)
					{
						WindowInterop.SetContentView(windowHandle, flutterView);
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "MacOSWindowManager: Failed to create Flutter view controller");
					WindowInterop.Close(windowHandle);
					return null;
				}

				// Create window info
				var info = new FlutterWindowInfo
				{
					WindowId = windowId,
					WindowHandle = windowHandle,
					ViewController = viewController,
					Title = title,
					IsMainWindow = isMainWindow,
					Frame = contentRect,
					ScaleFactor = WindowInterop.GetBackingScaleFactor(windowHandle),
					CreatedAt = DateTime.UtcNow
				};

				// Register the window
				_windows[windowId] = info;

				if (isMainWindow || _mainWindowId == 0)
				{
					_mainWindowId = windowId;
				}

				// Center and show the window
				WindowInterop.Center(windowHandle);
				WindowInterop.MakeKeyAndOrderFront(windowHandle);

				FlutterSharpLogger.LogInformation("MacOSWindowManager: Created window '{Title}' (ID: {Id})", title, windowId);

				// Raise event
				WindowCreated?.Invoke(this, new WindowEventArgs(info));

				return info;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MacOSWindowManager: Failed to create window");
				return null;
			}
		}

		/// <summary>
		/// Registers an existing window with the manager.
		/// </summary>
		/// <param name="windowHandle">The NSWindow handle</param>
		/// <param name="viewController">The Flutter view controller</param>
		/// <param name="isMainWindow">Whether this is the main window</param>
		/// <returns>Window info</returns>
		public FlutterWindowInfo RegisterWindow(IntPtr windowHandle, FlutterMacOSViewController viewController, bool isMainWindow = false)
		{
			var windowId = WindowInterop.GetWindowNumber(windowHandle);
			var frame = WindowInterop.GetFrame(windowHandle);

			var info = new FlutterWindowInfo
			{
				WindowId = windowId,
				WindowHandle = windowHandle,
				ViewController = viewController,
				Title = string.Empty,
				IsMainWindow = isMainWindow,
				Frame = frame,
				ScaleFactor = WindowInterop.GetBackingScaleFactor(windowHandle),
				CreatedAt = DateTime.UtcNow
			};

			_windows[windowId] = info;

			if (isMainWindow || _mainWindowId == 0)
			{
				_mainWindowId = windowId;
			}

			FlutterSharpLogger.LogDebug("MacOSWindowManager: Registered existing window (ID: {Id})", windowId);

			return info;
		}

		#endregion

		#region Window Management

		/// <summary>
		/// Gets window info by ID.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <returns>Window info, or null if not found</returns>
		public FlutterWindowInfo? GetWindow(nint windowId)
		{
			return _windows.TryGetValue(windowId, out var window) ? window : null;
		}

		/// <summary>
		/// Gets window info by handle.
		/// </summary>
		/// <param name="windowHandle">The NSWindow handle</param>
		/// <returns>Window info, or null if not found</returns>
		public FlutterWindowInfo? GetWindowByHandle(IntPtr windowHandle)
		{
			foreach (var window in _windows.Values)
			{
				if (window.WindowHandle == windowHandle)
					return window;
			}
			return null;
		}

		/// <summary>
		/// Closes a window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void CloseWindow(nint windowId)
		{
			if (!_windows.TryRemove(windowId, out var info))
			{
				FlutterSharpLogger.LogWarning("MacOSWindowManager: Window {Id} not found", windowId);
				return;
			}

			try
			{
				// Dispose the view controller
				info.ViewController?.Dispose();

				// Close the native window
				if (info.WindowHandle != IntPtr.Zero)
				{
					WindowInterop.Close(info.WindowHandle);
				}

				// Update main window reference
				if (_mainWindowId == windowId)
				{
					_mainWindowId = 0;
					// Try to find a new main window
					foreach (var w in _windows.Values)
					{
						_mainWindowId = w.WindowId;
						w.IsMainWindow = true;
						break;
					}
				}

				FlutterSharpLogger.LogInformation("MacOSWindowManager: Closed window '{Title}' (ID: {Id})", info.Title, windowId);

				// Raise event
				WindowClosed?.Invoke(this, new WindowEventArgs(info));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MacOSWindowManager: Error closing window {Id}", windowId);
			}
		}

		/// <summary>
		/// Closes all windows.
		/// </summary>
		public void CloseAllWindows()
		{
			var windowIds = new List<nint>(_windows.Keys);
			foreach (var id in windowIds)
			{
				CloseWindow(id);
			}
		}

		/// <summary>
		/// Focuses a window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void FocusWindow(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.MakeKeyAndOrderFront(info.WindowHandle);
				WindowBecameKey?.Invoke(this, new WindowEventArgs(info));
			}
		}

		/// <summary>
		/// Minimizes a window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void MinimizeWindow(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.Miniaturize(info.WindowHandle);
			}
		}

		/// <summary>
		/// Restores a minimized window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void RestoreWindow(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.Deminiaturize(info.WindowHandle);
			}
		}

		/// <summary>
		/// Toggles window maximized state.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void ToggleMaximize(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.Zoom(info.WindowHandle);
			}
		}

		/// <summary>
		/// Toggles window fullscreen state.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		public void ToggleFullScreen(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.ToggleFullScreen(info.WindowHandle);
			}
		}

		/// <summary>
		/// Sets the window title.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <param name="title">The new title</param>
		public void SetWindowTitle(nint windowId, string title)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.SetTitle(info.WindowHandle, title);
				info.Title = title;
			}
		}

		/// <summary>
		/// Sets the window frame.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <param name="frame">The new frame</param>
		/// <param name="animate">Whether to animate the change</param>
		public void SetWindowFrame(nint windowId, CGRect frame, bool animate = false)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.SetFrame(info.WindowHandle, frame, true, animate);
				info.Frame = frame;
				WindowResized?.Invoke(this, new WindowEventArgs(info));
			}
		}

		/// <summary>
		/// Sets the window size constraints.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <param name="minSize">Minimum size</param>
		/// <param name="maxSize">Maximum size (null for no limit)</param>
		public void SetWindowSizeConstraints(nint windowId, CGSize minSize, CGSize? maxSize = null)
		{
			if (_windows.TryGetValue(windowId, out var info))
			{
				WindowInterop.SetMinSize(info.WindowHandle, minSize);
				if (maxSize.HasValue)
				{
					WindowInterop.SetMaxSize(info.WindowHandle, maxSize.Value);
				}
			}
		}

		#endregion

		#region Screen Information

		/// <summary>
		/// Gets information about the main screen.
		/// </summary>
		/// <returns>Screen frame and visible frame</returns>
		public (CGRect Frame, CGRect VisibleFrame) GetMainScreenInfo()
		{
			var screen = WindowInterop.GetMainScreen();
			if (screen == IntPtr.Zero)
				return (CGRect.Empty, CGRect.Empty);

			return (
				WindowInterop.GetScreenFrame(screen),
				WindowInterop.GetScreenVisibleFrame(screen)
			);
		}

		/// <summary>
		/// Gets information about all screens.
		/// </summary>
		/// <returns>Array of (Frame, VisibleFrame) tuples for each screen</returns>
		public (CGRect Frame, CGRect VisibleFrame)[] GetAllScreensInfo()
		{
			var screens = WindowInterop.GetAllScreens();
			var result = new (CGRect, CGRect)[screens.Length];

			for (int i = 0; i < screens.Length; i++)
			{
				result[i] = (
					WindowInterop.GetScreenFrame(screens[i]),
					WindowInterop.GetScreenVisibleFrame(screens[i])
				);
			}

			return result;
		}

		/// <summary>
		/// Gets the screen containing a specific window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <returns>Screen frame and visible frame, or empty rects if not found</returns>
		public (CGRect Frame, CGRect VisibleFrame) GetWindowScreenInfo(nint windowId)
		{
			if (!_windows.TryGetValue(windowId, out var info))
				return (CGRect.Empty, CGRect.Empty);

			var screen = WindowInterop.GetWindowScreen(info.WindowHandle);
			if (screen == IntPtr.Zero)
				return (CGRect.Empty, CGRect.Empty);

			return (
				WindowInterop.GetScreenFrame(screen),
				WindowInterop.GetScreenVisibleFrame(screen)
			);
		}

		#endregion

		#region Widget Integration

		/// <summary>
		/// Sets the widget for a window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <param name="widget">The widget to display</param>
		public void SetWindowWidget(nint windowId, Widget widget)
		{
			if (_windows.TryGetValue(windowId, out var info) && info.ViewController != null)
			{
				info.ViewController.Widget = widget;
			}
		}

		/// <summary>
		/// Gets the widget from a window.
		/// </summary>
		/// <param name="windowId">The window ID</param>
		/// <returns>The widget, or null if not found</returns>
		public Widget? GetWindowWidget(nint windowId)
		{
			if (_windows.TryGetValue(windowId, out var info) && info.ViewController != null)
			{
				return info.ViewController.Widget;
			}
			return null;
		}

		#endregion

		#region Lifecycle

		/// <summary>
		/// Notifies all windows of lifecycle state change.
		/// </summary>
		/// <param name="state">The new lifecycle state</param>
		public void NotifyAllWindowsLifecycleState(FlutterLifecycleState state)
		{
			foreach (var window in _windows.Values)
			{
				switch (state)
				{
					case FlutterLifecycleState.Resumed:
						window.ViewController?.NotifyResumed();
						break;
					case FlutterLifecycleState.Paused:
						window.ViewController?.NotifyPaused();
						break;
					case FlutterLifecycleState.Inactive:
						window.ViewController?.NotifyInactive();
						break;
					case FlutterLifecycleState.Detached:
						window.ViewController?.NotifyDetached();
						break;
				}
			}
		}

		#endregion

		#region IDisposable

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				CloseAllWindows();
				_windows.Clear();
			}

			_disposed = true;
			FlutterSharpLogger.LogDebug("MacOSWindowManager: Disposed");
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
