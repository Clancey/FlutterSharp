using System;
using Flutter.Internal;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Flutter.MAUI
{
	/// <summary>
	/// Cross-platform MAUI View that hosts Flutter content.
	/// This view integrates with FlutterManager for state management and uses platform-specific handlers for rendering.
	/// Supports MAUI page lifecycle events (Appearing/Disappearing) for proper Flutter integration.
	/// Handles sizing constraints including aspect ratio, min/max dimensions, and orientation changes.
	/// </summary>
	public class FlutterView : View, IFlutterView
	{
		/// <summary>
		/// Bindable property for the Widget
		/// </summary>
		public static readonly BindableProperty WidgetProperty = BindableProperty.Create(
			nameof(Widget),
			typeof(Widget),
			typeof(FlutterView),
			null,
			propertyChanged: OnWidgetChanged);

		/// <summary>
		/// Bindable property for AspectRatio
		/// </summary>
		public static readonly BindableProperty AspectRatioProperty = BindableProperty.Create(
			nameof(AspectRatio),
			typeof(double),
			typeof(FlutterView),
			0.0,
			propertyChanged: OnSizingPropertyChanged);

		/// <summary>
		/// Bindable property for FillAvailableSpace
		/// </summary>
		public static readonly BindableProperty FillAvailableSpaceProperty = BindableProperty.Create(
			nameof(FillAvailableSpace),
			typeof(bool),
			typeof(FlutterView),
			true,
			propertyChanged: OnSizingPropertyChanged);

		/// <summary>
		/// Gets or sets the Flutter widget to display
		/// </summary>
		public Widget? Widget
		{
			get => (Widget?)GetValue(WidgetProperty);
			set => SetValue(WidgetProperty, value);
		}

		/// <summary>
		/// Gets or sets the aspect ratio (width/height) to maintain for the Flutter content.
		/// When set to a positive value, the view will size itself to maintain this ratio within its constraints.
		/// A value of 0 or negative means no aspect ratio constraint.
		/// Example: 16.0/9.0 for 16:9 aspect ratio, 1.0 for square.
		/// </summary>
		public double AspectRatio
		{
			get => (double)GetValue(AspectRatioProperty);
			set => SetValue(AspectRatioProperty, value);
		}

		/// <summary>
		/// Gets or sets whether the Flutter view should fill the available space.
		/// When true (default), the view expands to fill its container.
		/// When false, the view sizes to its content or specified dimensions.
		/// </summary>
		public bool FillAvailableSpace
		{
			get => (bool)GetValue(FillAvailableSpaceProperty);
			set => SetValue(FillAvailableSpaceProperty, value);
		}

		private static void OnSizingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is FlutterView flutterView)
			{
				// Trigger re-measure when sizing properties change
				flutterView.InvalidateMeasure();
			}
		}

		private static void OnWidgetChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is FlutterView flutterView)
			{
				var oldWidget = oldValue as Widget;
				var newWidget = newValue as Widget;

				// Untrack old widget
				if (oldWidget != null)
				{
					FlutterManager.UntrackWidget(oldWidget);
				}

				// Track new widget
				if (newWidget != null)
				{
					FlutterManager.TrackWidget(newWidget);

					// Send state if Flutter is ready
					if (FlutterManager.IsReady)
					{
						FlutterManager.SendState(newWidget);
					}
				}
			}
		}

		private bool _isAppearing;
		private Page? _parentPage;
		private double _containerWidth;
		private double _containerHeight;
		private DisplayOrientation _lastOrientation;

		/// <summary>
		/// Event raised when the FlutterView appears on screen
		/// </summary>
		public event EventHandler? Appearing;

		/// <summary>
		/// Event raised when the FlutterView disappears from screen
		/// </summary>
		public event EventHandler? Disappearing;

		/// <summary>
		/// Event raised when the app resumes from background
		/// </summary>
		public event EventHandler? Resumed;

		/// <summary>
		/// Event raised when the app goes to background
		/// </summary>
		public event EventHandler? Paused;

		/// <summary>
		/// Event raised when the container size changes
		/// </summary>
		public event EventHandler<SizeChangedEventArgs>? ContainerSizeChanged;

		/// <summary>
		/// Event raised when orientation changes
		/// </summary>
		public event EventHandler<DisplayOrientation>? OrientationChanged;

		/// <summary>
		/// Initializes a new instance of the FlutterView
		/// </summary>
		public FlutterView()
		{
			// Ensure FlutterManager is initialized
			FlutterManager.Initialize();

			// Subscribe to ready event if not already ready
			if (!FlutterManager.IsReady)
			{
				FlutterManager.OnReady += OnFlutterReady;
			}

			// Subscribe to display changes for orientation handling
			_lastOrientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
			DeviceDisplay.Current.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		private void OnFlutterReady()
		{
			// Send current widget state when Flutter becomes ready
			if (Widget != null)
			{
				FlutterManager.SendState(Widget);
			}
		}

		/// <summary>
		/// Called when the handler changes - hooks up lifecycle events
		/// </summary>
		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

			if (Handler != null)
			{
				// Track widget when handler is attached
				if (Widget != null)
				{
					FlutterManager.TrackWidget(Widget);

					// Send state if Flutter is ready
					if (FlutterManager.IsReady)
					{
						FlutterManager.SendState(Widget);
					}
				}

				// Try to find and attach to parent page for lifecycle events
				AttachToParentPage();
			}
		}

		/// <summary>
		/// Cleanup when handler is being changed
		/// </summary>
		protected override void OnHandlerChanging(HandlerChangingEventArgs args)
		{
			base.OnHandlerChanging(args);

			// Untrack widget when handler is being removed
			if (args.NewHandler == null && Widget != null)
			{
				FlutterManager.UntrackWidget(Widget);
			}

			// Unsubscribe from events and detach from page
			if (args.NewHandler == null)
			{
				FlutterManager.OnReady -= OnFlutterReady;
				DeviceDisplay.Current.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
				DetachFromParentPage();
			}
		}

		/// <summary>
		/// Called when this view's parent changes
		/// </summary>
		protected override void OnParentChanged()
		{
			base.OnParentChanged();

			// Re-attach to parent page when parent changes
			if (Handler != null)
			{
				DetachFromParentPage();
				AttachToParentPage();
			}
		}

		private void AttachToParentPage()
		{
			// Walk up the visual tree to find the parent page
			Element? current = Parent;
			while (current != null)
			{
				if (current is Page page)
				{
					_parentPage = page;
					_parentPage.Appearing += OnParentPageAppearing;
					_parentPage.Disappearing += OnParentPageDisappearing;
					break;
				}
				current = current.Parent;
			}
		}

		private void DetachFromParentPage()
		{
			if (_parentPage != null)
			{
				_parentPage.Appearing -= OnParentPageAppearing;
				_parentPage.Disappearing -= OnParentPageDisappearing;
				_parentPage = null;
			}
		}

		private void OnParentPageAppearing(object? sender, EventArgs e)
		{
			OnAppearing();
		}

		private void OnParentPageDisappearing(object? sender, EventArgs e)
		{
			OnDisappearing();
		}

		/// <summary>
		/// Called when the FlutterView appears on screen.
		/// Override this method to perform custom initialization when the view becomes visible.
		/// </summary>
		protected virtual void OnAppearing()
		{
			if (_isAppearing)
				return;

			_isAppearing = true;

			// Refresh widget state when appearing
			if (Widget != null && FlutterManager.IsReady)
			{
				FlutterManager.SendState(Widget);
			}

			// Notify Flutter about lifecycle change
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);

			Appearing?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when the FlutterView disappears from screen.
		/// Override this method to perform cleanup when the view is no longer visible.
		/// </summary>
		protected virtual void OnDisappearing()
		{
			if (!_isAppearing)
				return;

			_isAppearing = false;

			// Notify Flutter about lifecycle change
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Inactive);

			Disappearing?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when the app resumes from background.
		/// Override this method to refresh data or restart animations.
		/// </summary>
		public virtual void OnResumed()
		{
			// Refresh widget state when resuming
			if (Widget != null && FlutterManager.IsReady)
			{
				FlutterManager.SendState(Widget);
			}

			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);

			Resumed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when the app goes to background.
		/// Override this method to save state or pause expensive operations.
		/// </summary>
		public virtual void OnPaused()
		{
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Paused);

			Paused?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when the display info changes (orientation, resolution, etc.)
		/// </summary>
		private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			var newOrientation = e.DisplayInfo.Orientation;
			if (newOrientation != _lastOrientation)
			{
				_lastOrientation = newOrientation;

				// Trigger re-measure on orientation change
				InvalidateMeasure();

				// Notify subscribers
				OrientationChanged?.Invoke(this, newOrientation);

				// Refresh widget state after orientation change
				if (Widget != null && FlutterManager.IsReady)
				{
					FlutterManager.SendState(Widget);
				}
			}
		}

		/// <summary>
		/// Notifies the view that the container size has changed.
		/// This triggers a re-measure and notifies the Flutter engine of the new size.
		/// </summary>
		/// <param name="width">The new container width</param>
		/// <param name="height">The new container height</param>
		public void OnContainerSizeChanged(double width, double height)
		{
			if (Math.Abs(_containerWidth - width) > 0.001 || Math.Abs(_containerHeight - height) > 0.001)
			{
				_containerWidth = width;
				_containerHeight = height;

				// Notify subscribers
				ContainerSizeChanged?.Invoke(this, new SizeChangedEventArgs(width, height));

				// Trigger re-measure
				InvalidateMeasure();

				// Send updated container size to Flutter
				FlutterManager.NotifyContainerSize(width, height);
			}
		}

		/// <summary>
		/// Gets the current container size
		/// </summary>
		public Microsoft.Maui.Graphics.Size ContainerSize => new Microsoft.Maui.Graphics.Size(_containerWidth, _containerHeight);

		/// <summary>
		/// Gets the current display orientation
		/// </summary>
		public DisplayOrientation CurrentOrientation => _lastOrientation;

		/// <summary>
		/// Override to provide size calculation with aspect ratio support
		/// </summary>
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			// If aspect ratio is specified, calculate size maintaining that ratio
			if (AspectRatio > 0)
			{
				var size = CalculateSizeWithAspectRatio(widthConstraint, heightConstraint, AspectRatio);
				return new SizeRequest(size, size);
			}

			// If FillAvailableSpace is true, use the full constraint
			if (FillAvailableSpace)
			{
				var width = double.IsInfinity(widthConstraint) ? 300 : widthConstraint;
				var height = double.IsInfinity(heightConstraint) ? 300 : heightConstraint;
				var size = new Microsoft.Maui.Graphics.Size(width, height);
				return new SizeRequest(size, new Microsoft.Maui.Graphics.Size(100, 100));
			}

			// Otherwise use base measurement
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// Calculates the size while maintaining the specified aspect ratio within constraints
		/// </summary>
		private static Microsoft.Maui.Graphics.Size CalculateSizeWithAspectRatio(double widthConstraint, double heightConstraint, double aspectRatio)
		{
			// Handle infinite constraints
			var hasWidthConstraint = !double.IsInfinity(widthConstraint) && widthConstraint > 0;
			var hasHeightConstraint = !double.IsInfinity(heightConstraint) && heightConstraint > 0;

			if (!hasWidthConstraint && !hasHeightConstraint)
			{
				// No constraints - use a default size
				return new Microsoft.Maui.Graphics.Size(300, 300 / aspectRatio);
			}

			if (hasWidthConstraint && !hasHeightConstraint)
			{
				// Only width constraint - calculate height from aspect ratio
				return new Microsoft.Maui.Graphics.Size(widthConstraint, widthConstraint / aspectRatio);
			}

			if (!hasWidthConstraint && hasHeightConstraint)
			{
				// Only height constraint - calculate width from aspect ratio
				return new Microsoft.Maui.Graphics.Size(heightConstraint * aspectRatio, heightConstraint);
			}

			// Both constraints present - fit within the constraints while maintaining ratio
			var constraintRatio = widthConstraint / heightConstraint;

			if (constraintRatio > aspectRatio)
			{
				// Container is wider than aspect ratio - constrain by height
				return new Microsoft.Maui.Graphics.Size(heightConstraint * aspectRatio, heightConstraint);
			}
			else
			{
				// Container is taller than aspect ratio - constrain by width
				return new Microsoft.Maui.Graphics.Size(widthConstraint, widthConstraint / aspectRatio);
			}
		}
	}

	/// <summary>
	/// Event args for container size changed events
	/// </summary>
	public class SizeChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The new width of the container
		/// </summary>
		public double Width { get; }

		/// <summary>
		/// The new height of the container
		/// </summary>
		public double Height { get; }

		/// <summary>
		/// Creates a new SizeChangedEventArgs
		/// </summary>
		public SizeChangedEventArgs(double width, double height)
		{
			Width = width;
			Height = height;
		}
	}
}
