using System;
using System.Runtime.InteropServices;
using CoreGraphics;
using Flutter.Logging;

namespace Flutter.macOS
{
	/// <summary>
	/// Window style mask options for NSWindow.
	/// </summary>
	[Flags]
	public enum NSWindowStyleMask : ulong
	{
		/// <summary>
		/// Window with no style.
		/// </summary>
		Borderless = 0,

		/// <summary>
		/// Window with title bar.
		/// </summary>
		Titled = 1 << 0,

		/// <summary>
		/// Window with close button.
		/// </summary>
		Closable = 1 << 1,

		/// <summary>
		/// Window with minimize button.
		/// </summary>
		Miniaturizable = 1 << 2,

		/// <summary>
		/// Window is resizable.
		/// </summary>
		Resizable = 1 << 3,

		/// <summary>
		/// Utility window style.
		/// </summary>
		UtilityWindow = 1 << 4,

		/// <summary>
		/// Document modal window.
		/// </summary>
		DocModalWindow = 1 << 6,

		/// <summary>
		/// Non-activating panel style.
		/// </summary>
		NonactivatingPanel = 1 << 7,

		/// <summary>
		/// Unified title and toolbar style.
		/// </summary>
		UnifiedTitleAndToolbar = 1 << 12,

		/// <summary>
		/// Full screen window.
		/// </summary>
		FullScreen = 1 << 14,

		/// <summary>
		/// Full size content view.
		/// </summary>
		FullSizeContentView = 1 << 15,

		/// <summary>
		/// Standard window with title, close, minimize, and resize.
		/// </summary>
		Standard = Titled | Closable | Miniaturizable | Resizable
	}

	/// <summary>
	/// Window collection behavior options for NSWindow.
	/// </summary>
	[Flags]
	public enum NSWindowCollectionBehavior : ulong
	{
		/// <summary>
		/// Default behavior.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Can join all spaces.
		/// </summary>
		CanJoinAllSpaces = 1 << 0,

		/// <summary>
		/// Move to active space.
		/// </summary>
		MoveToActiveSpace = 1 << 1,

		/// <summary>
		/// Managed (participates in window management).
		/// </summary>
		Managed = 1 << 2,

		/// <summary>
		/// Transient window.
		/// </summary>
		Transient = 1 << 3,

		/// <summary>
		/// Stationary window.
		/// </summary>
		Stationary = 1 << 4,

		/// <summary>
		/// Participates in Expose.
		/// </summary>
		ParticipatesInCycle = 1 << 5,

		/// <summary>
		/// Ignores Expose.
		/// </summary>
		IgnoresCycle = 1 << 6,

		/// <summary>
		/// Allows full screen.
		/// </summary>
		FullScreenPrimary = 1 << 7,

		/// <summary>
		/// Full screen auxiliary.
		/// </summary>
		FullScreenAuxiliary = 1 << 8,

		/// <summary>
		/// Full screen disallows tiling.
		/// </summary>
		FullScreenDisallowsTiling = 1 << 11,

		/// <summary>
		/// Full screen allows tiling.
		/// </summary>
		FullScreenAllowsTiling = 1 << 12
	}

	/// <summary>
	/// Provides native NSWindow and NSScreen interoperability for macOS window management.
	/// </summary>
	/// <remarks>
	/// This class provides low-level P/Invoke access to macOS AppKit window classes:
	/// - NSWindow - represents a window
	/// - NSScreen - represents a display/monitor
	/// - NSWindowController - manages window lifecycle
	/// </remarks>
	internal static class WindowInterop
	{
		#region Objective-C Runtime P/Invoke

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
		private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPStr)] string className);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
		private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string selectorName);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_Bool(IntPtr receiver, IntPtr selector, bool arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern bool objc_msgSend_Bool(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_SetFrame(IntPtr receiver, IntPtr selector, CGRect frame);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern CGRect objc_msgSend_CGRect(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_SetFrame_Bool(IntPtr receiver, IntPtr selector, CGRect frame, bool animate);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_SetFrame_Bool_Bool(IntPtr receiver, IntPtr selector, CGRect frame, bool display, bool animate);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern nfloat objc_msgSend_NFloat(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_NFloat(IntPtr receiver, IntPtr selector, nfloat arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_ULong(IntPtr receiver, IntPtr selector, ulong arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern ulong objc_msgSend_ULong(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_InitWindow(IntPtr receiver, IntPtr selector,
			CGRect contentRect, ulong styleMask, ulong backing, bool defer);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr_Int(IntPtr receiver, IntPtr selector, nint arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern nint objc_msgSend_NInt(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern CGSize objc_msgSend_CGSize(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_SetSize(IntPtr receiver, IntPtr selector, CGSize size);

		#endregion

		#region Selector Cache

		// NSWindow selectors
		private static IntPtr _selAlloc;
		private static IntPtr _selInit;
		private static IntPtr _selInitWithContentRect;
		private static IntPtr _selContentView;
		private static IntPtr _selSetContentView;
		private static IntPtr _selFrame;
		private static IntPtr _selSetFrame;
		private static IntPtr _selSetFrameDisplay;
		private static IntPtr _selSetFrameDisplayAnimate;
		private static IntPtr _selTitle;
		private static IntPtr _selSetTitle;
		private static IntPtr _selMakeKeyAndOrderFront;
		private static IntPtr _selOrderFront;
		private static IntPtr _selOrderOut;
		private static IntPtr _selClose;
		private static IntPtr _selMiniaturize;
		private static IntPtr _selDeminiaturize;
		private static IntPtr _selZoom;
		private static IntPtr _selCenter;
		private static IntPtr _selIsVisible;
		private static IntPtr _selIsKeyWindow;
		private static IntPtr _selIsMainWindow;
		private static IntPtr _selIsMiniaturized;
		private static IntPtr _selIsZoomed;
		private static IntPtr _selStyleMask;
		private static IntPtr _selSetStyleMask;
		private static IntPtr _selLevel;
		private static IntPtr _selSetLevel;
		private static IntPtr _selDelegate;
		private static IntPtr _selSetDelegate;
		private static IntPtr _selScreen;
		private static IntPtr _selBackingScaleFactor;
		private static IntPtr _selMinSize;
		private static IntPtr _selSetMinSize;
		private static IntPtr _selMaxSize;
		private static IntPtr _selSetMaxSize;
		private static IntPtr _selContentMinSize;
		private static IntPtr _selSetContentMinSize;
		private static IntPtr _selContentMaxSize;
		private static IntPtr _selSetContentMaxSize;
		private static IntPtr _selToggleFullScreen;
		private static IntPtr _selCollectionBehavior;
		private static IntPtr _selSetCollectionBehavior;
		private static IntPtr _selWindowNumber;
		private static IntPtr _selReleasedWhenClosed;
		private static IntPtr _selSetReleasedWhenClosed;

		// NSScreen selectors
		private static IntPtr _selMainScreen;
		private static IntPtr _selScreens;
		private static IntPtr _selVisibleFrame;
		private static IntPtr _selCount;
		private static IntPtr _selObjectAtIndex;

		// Class references
		private static IntPtr _classNSWindow;
		private static IntPtr _classNSScreen;
		private static IntPtr _classNSString;

		private static bool _selectorsInitialized;

		private static void EnsureSelectorsInitialized()
		{
			if (_selectorsInitialized)
				return;

			try
			{
				// Classes
				_classNSWindow = objc_getClass("NSWindow");
				_classNSScreen = objc_getClass("NSScreen");
				_classNSString = objc_getClass("NSString");

				// Alloc/Init
				_selAlloc = sel_registerName("alloc");
				_selInit = sel_registerName("init");
				_selInitWithContentRect = sel_registerName("initWithContentRect:styleMask:backing:defer:");

				// NSWindow content/view
				_selContentView = sel_registerName("contentView");
				_selSetContentView = sel_registerName("setContentView:");

				// NSWindow frame/geometry
				_selFrame = sel_registerName("frame");
				_selSetFrame = sel_registerName("setFrame:");
				_selSetFrameDisplay = sel_registerName("setFrame:display:");
				_selSetFrameDisplayAnimate = sel_registerName("setFrame:display:animate:");
				_selMinSize = sel_registerName("minSize");
				_selSetMinSize = sel_registerName("setMinSize:");
				_selMaxSize = sel_registerName("maxSize");
				_selSetMaxSize = sel_registerName("setMaxSize:");
				_selContentMinSize = sel_registerName("contentMinSize");
				_selSetContentMinSize = sel_registerName("setContentMinSize:");
				_selContentMaxSize = sel_registerName("contentMaxSize");
				_selSetContentMaxSize = sel_registerName("setContentMaxSize:");

				// NSWindow title
				_selTitle = sel_registerName("title");
				_selSetTitle = sel_registerName("setTitle:");

				// NSWindow ordering/visibility
				_selMakeKeyAndOrderFront = sel_registerName("makeKeyAndOrderFront:");
				_selOrderFront = sel_registerName("orderFront:");
				_selOrderOut = sel_registerName("orderOut:");
				_selClose = sel_registerName("close");
				_selMiniaturize = sel_registerName("miniaturize:");
				_selDeminiaturize = sel_registerName("deminiaturize:");
				_selZoom = sel_registerName("zoom:");
				_selCenter = sel_registerName("center");
				_selIsVisible = sel_registerName("isVisible");
				_selIsKeyWindow = sel_registerName("isKeyWindow");
				_selIsMainWindow = sel_registerName("isMainWindow");
				_selIsMiniaturized = sel_registerName("isMiniaturized");
				_selIsZoomed = sel_registerName("isZoomed");
				_selToggleFullScreen = sel_registerName("toggleFullScreen:");

				// NSWindow style/behavior
				_selStyleMask = sel_registerName("styleMask");
				_selSetStyleMask = sel_registerName("setStyleMask:");
				_selLevel = sel_registerName("level");
				_selSetLevel = sel_registerName("setLevel:");
				_selCollectionBehavior = sel_registerName("collectionBehavior");
				_selSetCollectionBehavior = sel_registerName("setCollectionBehavior:");
				_selWindowNumber = sel_registerName("windowNumber");
				_selReleasedWhenClosed = sel_registerName("isReleasedWhenClosed");
				_selSetReleasedWhenClosed = sel_registerName("setReleasedWhenClosed:");

				// NSWindow delegate
				_selDelegate = sel_registerName("delegate");
				_selSetDelegate = sel_registerName("setDelegate:");

				// NSWindow screen
				_selScreen = sel_registerName("screen");
				_selBackingScaleFactor = sel_registerName("backingScaleFactor");

				// NSScreen selectors
				_selMainScreen = sel_registerName("mainScreen");
				_selScreens = sel_registerName("screens");
				_selVisibleFrame = sel_registerName("visibleFrame");
				_selCount = sel_registerName("count");
				_selObjectAtIndex = sel_registerName("objectAtIndex:");

				_selectorsInitialized = true;
				FlutterSharpLogger.LogDebug("WindowInterop: Selectors initialized successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to initialize selectors");
			}
		}

		#endregion

		#region NSWindow Creation

		/// <summary>
		/// Creates a new NSWindow with the specified content rect and style.
		/// </summary>
		/// <param name="contentRect">The content area rectangle</param>
		/// <param name="styleMask">Window style mask</param>
		/// <param name="defer">Whether to defer window creation</param>
		/// <returns>Handle to the new NSWindow, or IntPtr.Zero if failed</returns>
		public static IntPtr CreateWindow(CGRect contentRect, NSWindowStyleMask styleMask, bool defer = false)
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSWindow == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("WindowInterop: NSWindow class not available");
					return IntPtr.Zero;
				}

				// NSBackingStoreBuffered = 2
				const ulong backingStore = 2;

				var windowAlloc = objc_msgSend_IntPtr(_classNSWindow, _selAlloc);
				if (windowAlloc == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("WindowInterop: Failed to allocate NSWindow");
					return IntPtr.Zero;
				}

				var window = objc_msgSend_InitWindow(windowAlloc, _selInitWithContentRect,
					contentRect, (ulong)styleMask, backingStore, defer);

				if (window != IntPtr.Zero)
				{
					FlutterSharpLogger.LogDebug("WindowInterop: Created NSWindow at {Handle}", window);
				}
				else
				{
					FlutterSharpLogger.LogWarning("WindowInterop: NSWindow init failed");
				}

				return window;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to create window");
				return IntPtr.Zero;
			}
		}

		#endregion

		#region NSWindow Properties

		/// <summary>
		/// Gets the content view of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>Handle to the content NSView, or IntPtr.Zero if failed</returns>
		public static IntPtr GetContentView(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_IntPtr(window, _selContentView);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get content view");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Sets the content view of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="contentView">Handle to the NSView to set as content</param>
		public static void SetContentView(IntPtr window, IntPtr contentView)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selSetContentView, contentView);
				FlutterSharpLogger.LogDebug("WindowInterop: Set content view for window");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set content view");
			}
		}

		/// <summary>
		/// Gets the frame rectangle of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>The window frame rectangle</returns>
		public static CGRect GetFrame(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return CGRect.Empty;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_CGRect(window, _selFrame);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get frame");
				return CGRect.Empty;
			}
		}

		/// <summary>
		/// Sets the frame rectangle of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="frame">The new frame rectangle</param>
		/// <param name="display">Whether to display immediately</param>
		/// <param name="animate">Whether to animate the change</param>
		public static void SetFrame(IntPtr window, CGRect frame, bool display = true, bool animate = false)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetFrame_Bool_Bool(window, _selSetFrameDisplayAnimate, frame, display, animate);
				FlutterSharpLogger.LogDebug("WindowInterop: Set frame to ({X}, {Y}, {Width}, {Height})",
					frame.X, frame.Y, frame.Width, frame.Height);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set frame");
			}
		}

		/// <summary>
		/// Sets the title of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="title">The new title</param>
		public static unsafe void SetTitle(IntPtr window, string title)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				var nsString = CreateNSString(title);
				if (nsString != IntPtr.Zero)
				{
					objc_msgSend_Void_IntPtr(window, _selSetTitle, nsString);
					FlutterSharpLogger.LogDebug("WindowInterop: Set title to '{Title}'", title);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set title");
			}
		}

		/// <summary>
		/// Gets the backing scale factor (DPI) of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>The backing scale factor (1.0 for non-Retina, 2.0 for Retina)</returns>
		public static nfloat GetBackingScaleFactor(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return 1.0f;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_NFloat(window, _selBackingScaleFactor);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get backing scale factor");
				return 1.0f;
			}
		}

		/// <summary>
		/// Gets the window number (unique identifier).
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>The window number</returns>
		public static nint GetWindowNumber(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return 0;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_NInt(window, _selWindowNumber);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get window number");
				return 0;
			}
		}

		/// <summary>
		/// Sets the minimum size constraint of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="minSize">The minimum size</param>
		public static void SetMinSize(IntPtr window, CGSize minSize)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetSize(window, _selSetMinSize, minSize);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set min size");
			}
		}

		/// <summary>
		/// Sets the maximum size constraint of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="maxSize">The maximum size</param>
		public static void SetMaxSize(IntPtr window, CGSize maxSize)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetSize(window, _selSetMaxSize, maxSize);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set max size");
			}
		}

		/// <summary>
		/// Gets the style mask of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>The style mask</returns>
		public static NSWindowStyleMask GetStyleMask(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return NSWindowStyleMask.Borderless;

			try
			{
				EnsureSelectorsInitialized();
				return (NSWindowStyleMask)objc_msgSend_ULong(window, _selStyleMask);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get style mask");
				return NSWindowStyleMask.Borderless;
			}
		}

		/// <summary>
		/// Sets the style mask of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="styleMask">The new style mask</param>
		public static void SetStyleMask(IntPtr window, NSWindowStyleMask styleMask)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_ULong(window, _selSetStyleMask, (ulong)styleMask);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set style mask");
			}
		}

		/// <summary>
		/// Sets the collection behavior of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="behavior">The collection behavior</param>
		public static void SetCollectionBehavior(IntPtr window, NSWindowCollectionBehavior behavior)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_ULong(window, _selSetCollectionBehavior, (ulong)behavior);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set collection behavior");
			}
		}

		/// <summary>
		/// Sets the delegate of an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="delegate">Handle to the delegate object</param>
		public static void SetDelegate(IntPtr window, IntPtr @delegate)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selSetDelegate, @delegate);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set delegate");
			}
		}

		/// <summary>
		/// Sets whether the window is released when closed.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <param name="release">Whether to release when closed</param>
		public static void SetReleasedWhenClosed(IntPtr window, bool release)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_Bool(window, _selSetReleasedWhenClosed, release);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to set released when closed");
			}
		}

		#endregion

		#region NSWindow State

		/// <summary>
		/// Checks if an NSWindow is visible.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>True if visible</returns>
		public static bool IsVisible(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(window, _selIsVisible);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to check visibility");
				return false;
			}
		}

		/// <summary>
		/// Checks if an NSWindow is the key window.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>True if key window</returns>
		public static bool IsKeyWindow(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(window, _selIsKeyWindow);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to check key window");
				return false;
			}
		}

		/// <summary>
		/// Checks if an NSWindow is the main window.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>True if main window</returns>
		public static bool IsMainWindow(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(window, _selIsMainWindow);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to check main window");
				return false;
			}
		}

		/// <summary>
		/// Checks if an NSWindow is miniaturized (minimized).
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>True if miniaturized</returns>
		public static bool IsMiniaturized(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(window, _selIsMiniaturized);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to check miniaturized");
				return false;
			}
		}

		/// <summary>
		/// Checks if an NSWindow is zoomed (maximized).
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>True if zoomed</returns>
		public static bool IsZoomed(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(window, _selIsZoomed);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to check zoomed");
				return false;
			}
		}

		#endregion

		#region NSWindow Actions

		/// <summary>
		/// Makes an NSWindow the key window and orders it to front.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void MakeKeyAndOrderFront(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selMakeKeyAndOrderFront, IntPtr.Zero);
				FlutterSharpLogger.LogDebug("WindowInterop: Made window key and ordered front");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to make key and order front");
			}
		}

		/// <summary>
		/// Orders an NSWindow to front.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void OrderFront(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selOrderFront, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to order front");
			}
		}

		/// <summary>
		/// Orders an NSWindow out (hides it).
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void OrderOut(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selOrderOut, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to order out");
			}
		}

		/// <summary>
		/// Closes an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void Close(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(window, _selClose);
				FlutterSharpLogger.LogDebug("WindowInterop: Closed window");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to close window");
			}
		}

		/// <summary>
		/// Miniaturizes (minimizes) an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void Miniaturize(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selMiniaturize, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to miniaturize");
			}
		}

		/// <summary>
		/// Deminiaturizes (restores) an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void Deminiaturize(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selDeminiaturize, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to deminiaturize");
			}
		}

		/// <summary>
		/// Zooms (maximizes/restores) an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void Zoom(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selZoom, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to zoom");
			}
		}

		/// <summary>
		/// Centers an NSWindow on the screen.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void Center(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(window, _selCenter);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to center");
			}
		}

		/// <summary>
		/// Toggles fullscreen mode for an NSWindow.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		public static void ToggleFullScreen(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(window, _selToggleFullScreen, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to toggle fullscreen");
			}
		}

		#endregion

		#region NSScreen Methods

		/// <summary>
		/// Gets the main screen.
		/// </summary>
		/// <returns>Handle to the main NSScreen, or IntPtr.Zero if failed</returns>
		public static IntPtr GetMainScreen()
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSScreen == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("WindowInterop: NSScreen class not available");
					return IntPtr.Zero;
				}

				return objc_msgSend_IntPtr(_classNSScreen, _selMainScreen);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get main screen");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets the visible frame of an NSScreen.
		/// </summary>
		/// <param name="screen">Handle to the NSScreen</param>
		/// <returns>The visible frame rectangle (excludes menu bar and dock)</returns>
		public static CGRect GetScreenVisibleFrame(IntPtr screen)
		{
			if (screen == IntPtr.Zero)
				return CGRect.Empty;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_CGRect(screen, _selVisibleFrame);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get screen visible frame");
				return CGRect.Empty;
			}
		}

		/// <summary>
		/// Gets the full frame of an NSScreen.
		/// </summary>
		/// <param name="screen">Handle to the NSScreen</param>
		/// <returns>The full frame rectangle</returns>
		public static CGRect GetScreenFrame(IntPtr screen)
		{
			if (screen == IntPtr.Zero)
				return CGRect.Empty;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_CGRect(screen, _selFrame);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get screen frame");
				return CGRect.Empty;
			}
		}

		/// <summary>
		/// Gets the screen that contains the window.
		/// </summary>
		/// <param name="window">Handle to the NSWindow</param>
		/// <returns>Handle to the NSScreen, or IntPtr.Zero if failed</returns>
		public static IntPtr GetWindowScreen(IntPtr window)
		{
			if (window == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_IntPtr(window, _selScreen);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get window screen");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets all available screens.
		/// </summary>
		/// <returns>Array of NSScreen handles</returns>
		public static IntPtr[] GetAllScreens()
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSScreen == IntPtr.Zero)
					return Array.Empty<IntPtr>();

				var screensArray = objc_msgSend_IntPtr(_classNSScreen, _selScreens);
				if (screensArray == IntPtr.Zero)
					return Array.Empty<IntPtr>();

				var count = objc_msgSend_NInt(screensArray, _selCount);
				if (count == 0)
					return Array.Empty<IntPtr>();

				var result = new IntPtr[count];
				for (nint i = 0; i < count; i++)
				{
					result[i] = objc_msgSend_IntPtr_Int(screensArray, _selObjectAtIndex, i);
				}

				return result;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to get all screens");
				return Array.Empty<IntPtr>();
			}
		}

		#endregion

		#region Helper Methods

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_InitWithString(IntPtr receiver, IntPtr selector, IntPtr utf8String);

		private static IntPtr _selInitWithUTF8String;

		/// <summary>
		/// Creates an NSString from a C# string.
		/// </summary>
		/// <param name="str">The string to convert</param>
		/// <returns>Handle to the NSString, or IntPtr.Zero if failed</returns>
		private static unsafe IntPtr CreateNSString(string str)
		{
			if (string.IsNullOrEmpty(str))
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				if (_classNSString == IntPtr.Zero)
					return IntPtr.Zero;

				if (_selInitWithUTF8String == IntPtr.Zero)
					_selInitWithUTF8String = sel_registerName("initWithUTF8String:");

				var bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
				fixed (byte* ptr = bytes)
				{
					var alloc = objc_msgSend_IntPtr(_classNSString, _selAlloc);
					return objc_msgSend_InitWithString(alloc, _selInitWithUTF8String, (IntPtr)ptr);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "WindowInterop: Failed to create NSString");
				return IntPtr.Zero;
			}
		}

		#endregion
	}
}
