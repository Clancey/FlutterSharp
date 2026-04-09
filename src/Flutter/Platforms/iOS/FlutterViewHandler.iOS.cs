using System;
using CoreGraphics;
using Flutter.MAUI;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Flutter.MAUI
{
	/// <summary>
	/// iOS-specific implementation of the FlutterViewHandler
	/// Wraps the existing FlutterViewController to integrate with MAUI
	/// </summary>
	public partial class FlutterViewHandler : ViewHandler<IFlutterView, UIView>
	{
		private FlutterViewController? _flutterViewController;
		private CGSize _lastKnownSize;

		/// <summary>
		/// Creates the native iOS view (FlutterViewController's View)
		/// </summary>
		protected override UIView CreatePlatformView()
		{
			// Create the FlutterViewController which manages the Flutter engine and rendering
			_flutterViewController = new FlutterViewController();

			// Set the widget if it's already available
			if (VirtualView?.Widget != null)
			{
				_flutterViewController.Widget = VirtualView.Widget;
			}

			// Return the view from the FlutterViewController
			// This UIView contains the Flutter rendering surface
			var view = _flutterViewController.View;
			if (view == null)
			{
				throw new InvalidOperationException("FlutterViewController.View is null");
			}

			// Enable flexible sizing to work with MAUI's layout system
			view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			// Set clip to bounds for proper aspect ratio handling
			view.ClipsToBounds = true;

			return view;
		}

		/// <summary>
		/// Connects the handler to the native view
		/// </summary>
		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);

			// The FlutterViewController is already initialized at this point
			// We just ensure the widget is set correctly
			if (_flutterViewController != null && VirtualView?.Widget != null)
			{
				_flutterViewController.Widget = VirtualView.Widget;
			}
		}

		/// <summary>
		/// Disconnects the handler from the native view
		/// </summary>
		protected override void DisconnectHandler(UIView platformView)
		{
			// Clean up the widget reference
			if (_flutterViewController != null)
			{
				// Untrack the widget if there was one
				var currentWidget = _flutterViewController.Widget;
				if (currentWidget != null)
				{
					Flutter.Internal.FlutterManager.UntrackWidget(currentWidget);
				}
			}

			base.DisconnectHandler(platformView);
		}

		/// <summary>
		/// Updates the widget displayed in the Flutter view
		/// </summary>
		partial void UpdateWidget(Widget? widget)
		{
			if (_flutterViewController != null)
			{
				_flutterViewController.Widget = widget;
			}
		}

		/// <summary>
		/// Updates sizing when sizing properties change
		/// </summary>
		partial void UpdateSizing()
		{
			// Trigger a layout update when sizing properties change
			PlatformView?.SetNeedsLayout();
		}

		/// <summary>
		/// Gets the desired size for the Flutter view based on constraints and properties
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
		/// Calculates the size while maintaining the specified aspect ratio within constraints
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
	}
}
