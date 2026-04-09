using System;
using CoreAnimation;
using CoreGraphics;
using Flutter.Logging;
using Flutter.MAUI;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using MacOSFlutter = Flutter.macOS;

namespace Flutter.MAUI
{
	/// <summary>
	/// Mac Catalyst-specific implementation of the FlutterViewHandler.
	/// Wraps the FlutterMacOSViewController to integrate with MAUI on Mac Catalyst.
	/// </summary>
	/// <remarks>
	/// Mac Catalyst uses UIKit (like iOS), so we use UIView as the native view type.
	/// The underlying Flutter engine uses the macOS embedder APIs.
	/// The Flutter NSView is bridged via CALayer integration.
	/// </remarks>
	public partial class FlutterViewHandler
	{
		private MacOSFlutter.FlutterMacOSViewController? _flutterViewController;
		private CGSize _lastKnownSize;
		private UIView? _containerView;
		private IntPtr _nativeViewHandle;
		private IntPtr _flutterLayerHandle;
		private CALayer? _flutterLayer;

		/// <summary>
		/// Creates the native Mac Catalyst view (a UIView containing the Flutter content).
		/// </summary>
		protected override UIView CreatePlatformView()
		{
			// Create the FlutterMacOSViewController which manages the Flutter engine
			_flutterViewController = new MacOSFlutter.FlutterMacOSViewController();

			// Set the widget if it's already available
			if (VirtualView?.Widget != null)
			{
				_flutterViewController.Widget = VirtualView.Widget;
			}

			// Create a container UIView to host the Flutter content
			_containerView = new UIView
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ClipsToBounds = true
			};

			// If we have a native view handle, we can set up the view hierarchy
			// In simulation mode (no native library), we just return an empty container
			var nativeHandle = _flutterViewController.NativeViewHandle;
			if (nativeHandle != IntPtr.Zero)
			{
				// The native handle is an NSView pointer, which we need to bridge to UIView
				// In Mac Catalyst, we may need to use specific bridging techniques
				// For now, we set up a placeholder until native support is available
				SetupNativeView(nativeHandle);
			}

			return _containerView;
		}

		/// <summary>
		/// Sets up the native Flutter view within the container using CALayer bridging.
		/// </summary>
		/// <remarks>
		/// In Mac Catalyst, we cannot directly add an NSView to a UIView hierarchy.
		/// Instead, we extract the CALayer from the NSView and add it as a sublayer
		/// to the UIView's layer. This provides visual integration while maintaining
		/// the separation between AppKit and UIKit view hierarchies.
		/// </remarks>
		private void SetupNativeView(IntPtr nativeHandle)
		{
			if (nativeHandle == IntPtr.Zero)
			{
				FlutterSharpLogger.LogDebug("NSView handle is null, cannot set up native view");
				return;
			}

			if (_containerView == null)
			{
				FlutterSharpLogger.LogWarning("Container view is null, cannot set up native view");
				return;
			}

			try
			{
				_nativeViewHandle = nativeHandle;

				// Get the CALayer from the NSView
				_flutterLayerHandle = MacOSFlutter.CocoaInterop.GetLayerFromNSView(nativeHandle);
				if (_flutterLayerHandle == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("Could not get CALayer from NSView - Flutter content will not be visible");
					return;
				}

				// Create a managed CALayer wrapper
				_flutterLayer = MacOSFlutter.CocoaInterop.GetManagedLayer(_flutterLayerHandle);
				if (_flutterLayer == null)
				{
					FlutterSharpLogger.LogWarning("Could not create managed CALayer - Flutter content will not be visible");
					return;
				}

				// Ensure the container view has its layer created
				_containerView.Layer.MasksToBounds = true;

				// Add the Flutter layer as a sublayer of the container view's layer
				_containerView.Layer.AddSublayer(_flutterLayer);

				// Set initial size
				SyncLayerFrame();

				FlutterSharpLogger.LogInformation("Successfully integrated Flutter NSView via CALayer bridging");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to set up native Flutter view");
			}
		}

		/// <summary>
		/// Synchronizes the Flutter layer frame with the container view bounds.
		/// </summary>
		private void SyncLayerFrame()
		{
			if (_flutterLayer == null || _containerView == null)
				return;

			var bounds = _containerView.Bounds;
			_flutterLayer.Frame = bounds;

			// Also update the NSView frame if we have access to it
			if (_nativeViewHandle != IntPtr.Zero)
			{
				MacOSFlutter.CocoaInterop.SetNSViewFrame(_nativeViewHandle, bounds);
			}

			FlutterSharpLogger.LogDebug("Synced Flutter layer frame to ({Width}x{Height})", bounds.Width, bounds.Height);
		}

		/// <summary>
		/// Tears down the native view integration.
		/// </summary>
		private void TeardownNativeView()
		{
			if (_flutterLayer != null)
			{
				_flutterLayer.RemoveFromSuperLayer();
				_flutterLayer = null;
			}

			_flutterLayerHandle = IntPtr.Zero;
			_nativeViewHandle = IntPtr.Zero;
		}

		/// <summary>
		/// Connects the handler to the native view.
		/// </summary>
		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);

			// Ensure the widget is set correctly
			if (_flutterViewController != null && VirtualView?.Widget != null)
			{
				_flutterViewController.Widget = VirtualView.Widget;
			}
		}

		/// <summary>
		/// Disconnects the handler from the native view.
		/// </summary>
		protected override void DisconnectHandler(UIView platformView)
		{
			// Tear down the native view layer integration
			TeardownNativeView();

			// Clean up the Flutter view controller
			if (_flutterViewController != null)
			{
				var currentWidget = _flutterViewController.Widget;
				if (currentWidget != null)
				{
					Flutter.Internal.FlutterManager.UntrackWidget(currentWidget);
				}

				// Dispose the FlutterMacOSViewController to release native resources
				_flutterViewController.Dispose();
				_flutterViewController = null;
			}

			_containerView = null;
			base.DisconnectHandler(platformView);
		}

		/// <summary>
		/// Updates the widget displayed in the Flutter view.
		/// </summary>
		partial void UpdateWidget(Widget? widget)
		{
			if (_flutterViewController != null)
			{
				_flutterViewController.Widget = widget;
			}
		}

		/// <summary>
		/// Updates sizing when sizing properties change.
		/// </summary>
		partial void UpdateSizing()
		{
			// Trigger a layout update when sizing properties change
			PlatformView?.SetNeedsLayout();

			// Synchronize the Flutter layer frame
			SyncLayerFrame();
		}

		/// <summary>
		/// Gets the desired size for the Flutter view based on constraints and properties.
		/// </summary>
		public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var virtualView = VirtualView;
			if (virtualView == null)
			{
				return base.GetDesiredSize(widthConstraint, heightConstraint);
			}

			// Check for aspect ratio constraint
			var aspectRatio = virtualView.AspectRatio;
			if (aspectRatio > 0)
			{
				return CalculateSizeWithAspectRatio(widthConstraint, heightConstraint, aspectRatio);
			}

			// If FillAvailableSpace is true, use the full constraint
			if (virtualView.FillAvailableSpace)
			{
				var width = double.IsInfinity(widthConstraint) ? 300 : widthConstraint;
				var height = double.IsInfinity(heightConstraint) ? 300 : heightConstraint;

				// Track size changes and notify the view
				var newSize = new CGSize(width, height);
				if (!_lastKnownSize.Equals(newSize))
				{
					_lastKnownSize = newSize;
					virtualView.OnContainerSizeChanged(width, height);
				}

				return new Microsoft.Maui.Graphics.Size(width, height);
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// Calculates the size while maintaining the specified aspect ratio within constraints.
		/// </summary>
		private Microsoft.Maui.Graphics.Size CalculateSizeWithAspectRatio(double widthConstraint, double heightConstraint, double aspectRatio)
		{
			// Handle infinite constraints
			var hasWidthConstraint = !double.IsInfinity(widthConstraint) && widthConstraint > 0;
			var hasHeightConstraint = !double.IsInfinity(heightConstraint) && heightConstraint > 0;

			double width, height;

			if (!hasWidthConstraint && !hasHeightConstraint)
			{
				// No constraints - use a default size
				width = 300;
				height = 300 / aspectRatio;
			}
			else if (hasWidthConstraint && !hasHeightConstraint)
			{
				// Only width constraint - calculate height from aspect ratio
				width = widthConstraint;
				height = widthConstraint / aspectRatio;
			}
			else if (!hasWidthConstraint && hasHeightConstraint)
			{
				// Only height constraint - calculate width from aspect ratio
				width = heightConstraint * aspectRatio;
				height = heightConstraint;
			}
			else
			{
				// Both constraints present - fit within the constraints while maintaining ratio
				var constraintRatio = widthConstraint / heightConstraint;

				if (constraintRatio > aspectRatio)
				{
					// Container is wider than aspect ratio - constrain by height
					width = heightConstraint * aspectRatio;
					height = heightConstraint;
				}
				else
				{
					// Container is taller than aspect ratio - constrain by width
					width = widthConstraint;
					height = widthConstraint / aspectRatio;
				}
			}

			// Track size changes and notify the view
			var virtualView = VirtualView;
			if (virtualView != null)
			{
				var newSize = new CGSize(width, height);
				if (!_lastKnownSize.Equals(newSize))
				{
					_lastKnownSize = newSize;
					virtualView.OnContainerSizeChanged(width, height);
				}
			}

			return new Microsoft.Maui.Graphics.Size(width, height);
		}

		/// <summary>
		/// Notifies the Flutter engine of lifecycle events.
		/// </summary>
		public void NotifyLifecycleEvent(FlutterLifecycleState state)
		{
			switch (state)
			{
				case FlutterLifecycleState.Resumed:
					_flutterViewController?.NotifyResumed();
					break;
				case FlutterLifecycleState.Inactive:
					_flutterViewController?.NotifyInactive();
					break;
				case FlutterLifecycleState.Paused:
					_flutterViewController?.NotifyPaused();
					break;
				case FlutterLifecycleState.Detached:
					_flutterViewController?.NotifyDetached();
					break;
			}
		}
	}
}
