using System;
using Flutter.Internal;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Flutter.MAUI
{
	/// <summary>
	/// Cross-platform MAUI View that hosts Flutter content.
	/// This view integrates with FlutterManager for state management and uses platform-specific handlers for rendering.
	/// Supports MAUI page lifecycle events (Appearing/Disappearing) for proper Flutter integration.
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
		/// Gets or sets the Flutter widget to display
		/// </summary>
		public Widget? Widget
		{
			get => (Widget?)GetValue(WidgetProperty);
			set => SetValue(WidgetProperty, value);
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

			// Unsubscribe from ready event and detach from page
			if (args.NewHandler == null)
			{
				FlutterManager.OnReady -= OnFlutterReady;
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
	}
}
