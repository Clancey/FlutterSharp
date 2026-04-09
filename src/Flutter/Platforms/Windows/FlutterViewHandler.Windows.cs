using System;
using Flutter.Logging;
using Flutter.MAUI;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WinSize = global::Windows.Foundation.Size;
using WindowsFlutter = Flutter.Windows;

namespace Flutter.MAUI
{
	/// <summary>
	/// Windows-specific implementation of the FlutterViewHandler.
	/// Wraps the FlutterControl to integrate with MAUI on Windows.
	/// </summary>
	public partial class FlutterViewHandler : ViewHandler<IFlutterView, Grid>
	{
		private WindowsFlutter.FlutterControl? _flutterControl;
		private WinSize _lastKnownSize;
		private bool _isSubscribedToXamlRoot;

		/// <summary>
		/// Creates the native Windows view (FlutterControl wrapped in a Grid).
		/// </summary>
		protected override Grid CreatePlatformView()
		{
			// Create the FlutterControl which manages the Flutter engine
			_flutterControl = new WindowsFlutter.FlutterControl();

			// Set the widget if it's already available
			if (VirtualView?.Widget != null)
			{
				_flutterControl.Widget = VirtualView.Widget;
			}

			// Enable flexible sizing
			_flutterControl.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			_flutterControl.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			return _flutterControl;
		}

		/// <summary>
		/// Connects the handler to the native view.
		/// </summary>
		protected override void ConnectHandler(Grid platformView)
		{
			base.ConnectHandler(platformView);

			// Ensure the widget is set correctly
			if (_flutterControl != null && VirtualView?.Widget != null)
			{
				_flutterControl.Widget = VirtualView.Widget;
			}

			// Subscribe to size changes
			platformView.SizeChanged += OnPlatformViewSizeChanged;

			// Subscribe to XamlRoot changes for DPI tracking
			// XamlRoot may not be available until the view is loaded
			if (platformView.XamlRoot != null)
			{
				SubscribeToXamlRootChanges(platformView);
			}
			else
			{
				platformView.Loaded += OnPlatformViewLoaded;
			}

			// Lifecycle management is handled at the application level
			// via FlutterControl.NotifyResumed/NotifyPaused/NotifyInactive/NotifyDetached
			// The application should call these methods in response to window events
		}

		/// <summary>
		/// Handles the Loaded event to subscribe to XamlRoot changes.
		/// </summary>
		private void OnPlatformViewLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			if (sender is Grid grid)
			{
				grid.Loaded -= OnPlatformViewLoaded;
				SubscribeToXamlRootChanges(grid);
			}
		}

		/// <summary>
		/// Subscribes to XamlRoot.Changed for DPI tracking.
		/// </summary>
		private void SubscribeToXamlRootChanges(Grid platformView)
		{
			if (_isSubscribedToXamlRoot || platformView.XamlRoot == null)
				return;

			platformView.XamlRoot.Changed += OnXamlRootChanged;
			_isSubscribedToXamlRoot = true;

			// Query initial DPI/scale from XamlRoot
			UpdateDpiFromXamlRoot(platformView.XamlRoot);
		}

		/// <summary>
		/// Handles XamlRoot changes (including DPI/scale changes).
		/// </summary>
		private void OnXamlRootChanged(Microsoft.UI.Xaml.XamlRoot sender, Microsoft.UI.Xaml.XamlRootChangedEventArgs args)
		{
			UpdateDpiFromXamlRoot(sender);
		}

		/// <summary>
		/// Updates the FlutterControl with DPI info from XamlRoot.
		/// </summary>
		private void UpdateDpiFromXamlRoot(Microsoft.UI.Xaml.XamlRoot xamlRoot)
		{
			// XamlRoot.RasterizationScale is the device pixel ratio (1.0, 1.25, 1.5, 2.0, etc.)
			var scale = xamlRoot.RasterizationScale;
			var dpi = (uint)(scale * 96);

			FlutterSharpLogger.LogDebug(
				"XamlRoot scale changed: scale={Scale:F2}, dpi={Dpi}",
				scale, dpi);

			// Refresh DPI in FlutterControl (it will only update if changed)
			_flutterControl?.RefreshDpi();
		}

		/// <summary>
		/// Handles size changed events from the platform view.
		/// </summary>
		private void OnPlatformViewSizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
		{
			var newSize = e.NewSize;

			// Notify FlutterControl of the size change
			// The size from WinUI is in logical (DIP) pixels
			_flutterControl?.UpdateSize(newSize.Width, newSize.Height);

			// Also track for MAUI sizing
			var virtualView = VirtualView;
			if (virtualView != null && !_lastKnownSize.Equals(newSize))
			{
				_lastKnownSize = new WinSize(newSize.Width, newSize.Height);
				virtualView.OnContainerSizeChanged(newSize.Width, newSize.Height);
			}
		}

		/// <summary>
		/// Disconnects the handler from the native view.
		/// </summary>
		protected override void DisconnectHandler(Grid platformView)
		{
			// Unsubscribe from size changes
			platformView.SizeChanged -= OnPlatformViewSizeChanged;
			platformView.Loaded -= OnPlatformViewLoaded;

			// Unsubscribe from XamlRoot changes
			if (_isSubscribedToXamlRoot && platformView.XamlRoot != null)
			{
				platformView.XamlRoot.Changed -= OnXamlRootChanged;
				_isSubscribedToXamlRoot = false;
			}

			// Clean up the widget reference
			if (_flutterControl != null)
			{
				var currentWidget = _flutterControl.Widget;
				if (currentWidget != null)
				{
					Flutter.Internal.FlutterManager.UntrackWidget(currentWidget);
				}

				// Dispose the FlutterControl to release native resources
				_flutterControl.Dispose();
				_flutterControl = null;
			}

			base.DisconnectHandler(platformView);
		}

		/// <summary>
		/// Updates the widget displayed in the Flutter view.
		/// </summary>
		partial void UpdateWidget(Widget? widget)
		{
			if (_flutterControl != null)
			{
				_flutterControl.Widget = widget;
			}
		}

		/// <summary>
		/// Updates sizing when sizing properties change.
		/// </summary>
		partial void UpdateSizing()
		{
			// Trigger a layout update when sizing properties change
			PlatformView?.InvalidateMeasure();
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
				var newSize = new WinSize(width, height);
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
				var newSize = new WinSize(width, height);
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
