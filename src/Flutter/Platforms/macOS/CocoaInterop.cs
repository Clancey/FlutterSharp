using System;
using System.Runtime.InteropServices;
using CoreAnimation;
using CoreGraphics;
using Flutter.Logging;
using ObjCRuntime;

namespace Flutter.macOS
{
	/// <summary>
	/// Provides Cocoa interoperability for bridging NSView to UIView in Mac Catalyst.
	/// </summary>
	/// <remarks>
	/// Mac Catalyst uses UIKit, but the Flutter macOS embedder provides an NSView.
	/// This class bridges the two by extracting the CALayer from the NSView and
	/// using it directly in the UIView hierarchy.
	/// </remarks>
	internal static class CocoaInterop
	{
		#region Objective-C Runtime P/Invoke

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
		private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPStr)] string className);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
		private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string selectorName);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void(IntPtr receiver, IntPtr selector, IntPtr arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_Bool(IntPtr receiver, IntPtr selector, bool arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_SetFrame(IntPtr receiver, IntPtr selector, CGRect frame);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern CGRect objc_msgSend_CGRect(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern bool objc_msgSend_Bool(IntPtr receiver, IntPtr selector);

		#endregion

		#region Selector Cache

		// Cache selectors for performance
		private static IntPtr _selLayer;
		private static IntPtr _selSetWantsLayer;
		private static IntPtr _selWantsLayer;
		private static IntPtr _selFrame;
		private static IntPtr _selSetFrame;
		private static IntPtr _selBounds;
		private static IntPtr _selSetBounds;
		private static IntPtr _selSetNeedsDisplay;
		private static IntPtr _selAddSublayer;
		private static IntPtr _selRemoveFromSuperlayer;
		private static IntPtr _selSuperlayer;
		private static IntPtr _selSetAutoresizingMask;

		private static void EnsureSelectorsInitialized()
		{
			if (_selLayer == IntPtr.Zero)
			{
				_selLayer = sel_registerName("layer");
				_selSetWantsLayer = sel_registerName("setWantsLayer:");
				_selWantsLayer = sel_registerName("wantsLayer");
				_selFrame = sel_registerName("frame");
				_selSetFrame = sel_registerName("setFrame:");
				_selBounds = sel_registerName("bounds");
				_selSetBounds = sel_registerName("setBounds:");
				_selSetNeedsDisplay = sel_registerName("setNeedsDisplay");
				_selAddSublayer = sel_registerName("addSublayer:");
				_selRemoveFromSuperlayer = sel_registerName("removeFromSuperlayer");
				_selSuperlayer = sel_registerName("superlayer");
				_selSetAutoresizingMask = sel_registerName("setAutoresizingMask:");
			}
		}

		#endregion

		#region NSView Integration

		/// <summary>
		/// Gets the CALayer from an NSView pointer.
		/// </summary>
		/// <param name="nsViewHandle">Handle to the NSView</param>
		/// <returns>The CALayer handle, or IntPtr.Zero if failed</returns>
		public static IntPtr GetLayerFromNSView(IntPtr nsViewHandle)
		{
			if (nsViewHandle == IntPtr.Zero)
			{
				FlutterSharpLogger.LogDebug("NSView handle is null, cannot get layer");
				return IntPtr.Zero;
			}

			try
			{
				EnsureSelectorsInitialized();

				// First, ensure the view has a layer (NSView needs wantsLayer = YES for layer-backed views)
				var wantsLayer = objc_msgSend_Bool(nsViewHandle, _selWantsLayer);
				if (!wantsLayer)
				{
					FlutterSharpLogger.LogDebug("Setting wantsLayer = YES on NSView");
					objc_msgSend_Void_Bool(nsViewHandle, _selSetWantsLayer, true);
				}

				// Get the layer
				var layerHandle = objc_msgSend_IntPtr(nsViewHandle, _selLayer);
				if (layerHandle != IntPtr.Zero)
				{
					FlutterSharpLogger.LogDebug("Successfully obtained CALayer from NSView: {Layer}", layerHandle);
				}
				else
				{
					FlutterSharpLogger.LogWarning("Failed to get CALayer from NSView (returned null)");
				}

				return layerHandle;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error getting CALayer from NSView");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets the frame rectangle of an NSView.
		/// </summary>
		/// <param name="nsViewHandle">Handle to the NSView</param>
		/// <returns>The frame rectangle</returns>
		public static CGRect GetNSViewFrame(IntPtr nsViewHandle)
		{
			if (nsViewHandle == IntPtr.Zero)
				return CGRect.Empty;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_CGRect(nsViewHandle, _selFrame);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error getting NSView frame");
				return CGRect.Empty;
			}
		}

		/// <summary>
		/// Sets the frame rectangle of an NSView.
		/// </summary>
		/// <param name="nsViewHandle">Handle to the NSView</param>
		/// <param name="frame">The new frame rectangle</param>
		public static void SetNSViewFrame(IntPtr nsViewHandle, CGRect frame)
		{
			if (nsViewHandle == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetFrame(nsViewHandle, _selSetFrame, frame);
				FlutterSharpLogger.LogDebug("Set NSView frame to ({X}, {Y}, {Width}, {Height})",
					frame.X, frame.Y, frame.Width, frame.Height);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error setting NSView frame");
			}
		}

		/// <summary>
		/// Gets the bounds rectangle of an NSView.
		/// </summary>
		/// <param name="nsViewHandle">Handle to the NSView</param>
		/// <returns>The bounds rectangle</returns>
		public static CGRect GetNSViewBounds(IntPtr nsViewHandle)
		{
			if (nsViewHandle == IntPtr.Zero)
				return CGRect.Empty;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_CGRect(nsViewHandle, _selBounds);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error getting NSView bounds");
				return CGRect.Empty;
			}
		}

		/// <summary>
		/// Marks an NSView as needing display.
		/// </summary>
		/// <param name="nsViewHandle">Handle to the NSView</param>
		public static void SetNSViewNeedsDisplay(IntPtr nsViewHandle)
		{
			if (nsViewHandle == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(nsViewHandle, _selSetNeedsDisplay, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error marking NSView needs display");
			}
		}

		#endregion

		#region CALayer Operations

		/// <summary>
		/// Adds a sublayer to a CALayer.
		/// </summary>
		/// <param name="parentLayer">Handle to the parent CALayer</param>
		/// <param name="sublayer">Handle to the sublayer to add</param>
		public static void AddSublayer(IntPtr parentLayer, IntPtr sublayer)
		{
			if (parentLayer == IntPtr.Zero || sublayer == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(parentLayer, _selAddSublayer, sublayer);
				FlutterSharpLogger.LogDebug("Added sublayer {Sublayer} to parent {Parent}", sublayer, parentLayer);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error adding sublayer");
			}
		}

		/// <summary>
		/// Removes a CALayer from its superlayer.
		/// </summary>
		/// <param name="layer">Handle to the layer to remove</param>
		public static void RemoveFromSuperlayer(IntPtr layer)
		{
			if (layer == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(layer, _selRemoveFromSuperlayer, IntPtr.Zero);
				FlutterSharpLogger.LogDebug("Removed layer from superlayer");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error removing from superlayer");
			}
		}

		/// <summary>
		/// Gets the superlayer of a CALayer.
		/// </summary>
		/// <param name="layer">Handle to the layer</param>
		/// <returns>Handle to the superlayer, or IntPtr.Zero if none</returns>
		public static IntPtr GetSuperlayer(IntPtr layer)
		{
			if (layer == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_IntPtr(layer, _selSuperlayer);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error getting superlayer");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Sets the frame of a CALayer.
		/// </summary>
		/// <param name="layer">Handle to the CALayer</param>
		/// <param name="frame">The new frame rectangle</param>
		public static void SetLayerFrame(IntPtr layer, CGRect frame)
		{
			if (layer == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetFrame(layer, _selSetFrame, frame);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error setting CALayer frame");
			}
		}

		/// <summary>
		/// Sets the bounds of a CALayer.
		/// </summary>
		/// <param name="layer">Handle to the CALayer</param>
		/// <param name="bounds">The new bounds rectangle</param>
		public static void SetLayerBounds(IntPtr layer, CGRect bounds)
		{
			if (layer == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_SetFrame(layer, _selSetBounds, bounds);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error setting CALayer bounds");
			}
		}

		#endregion

		#region Managed CALayer Wrapper

		/// <summary>
		/// Creates a managed CALayer wrapper from an IntPtr handle.
		/// </summary>
		/// <param name="handle">Handle to the native CALayer</param>
		/// <returns>A managed CALayer instance, or null if handle is invalid</returns>
		public static CALayer? GetManagedLayer(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			try
			{
				return Runtime.GetNSObject<CALayer>(handle);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error creating managed CALayer from handle");
				return null;
			}
		}

		#endregion
	}
}
