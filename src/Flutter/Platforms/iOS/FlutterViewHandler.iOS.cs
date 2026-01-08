using System;
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

			view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
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
	}
}
