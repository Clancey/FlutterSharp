using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Flutter.HotReload;
using Flutter.Internal;
using Flutter.Logging;
using Flutter.StateRestoration;
using Flutter.Widgets;
using IO.Flutter.Embedding.Engine;
using IO.Flutter.Plugin.Common;
using IO.Flutter.Plugins;

namespace Flutter
{
	/// <summary>
	/// Base activity for Flutter/C# integration on Android.
	/// Supports state restoration, lifecycle management, and hot reload.
	/// </summary>
	public class FlutterActivity : IO.Flutter.Embedding.Android.FlutterActivity,
		MethodChannel.IMethodCallHandler, IHotReloadHandler, IStateRestorable
	{
		private const string StateKey = "FlutterSharpState";

		private Widget widget;
		private string _restorationId;
		private bool isReady;
		private MethodChannel channel;

		/// <summary>
		/// Creates a FlutterActivity with an auto-generated restoration ID.
		/// </summary>
		public FlutterActivity()
		{
			_restorationId = "FlutterActivity_" + Guid.NewGuid().ToString("N").Substring(0, 8);
		}

		/// <summary>
		/// Creates a FlutterActivity with a specific restoration ID.
		/// Use this constructor when you need consistent state restoration across app launches.
		/// </summary>
		/// <param name="restorationId">A unique identifier for state restoration</param>
		public FlutterActivity(string restorationId)
		{
			_restorationId = restorationId ?? "FlutterActivity_" + Guid.NewGuid().ToString("N").Substring(0, 8);
		}

		/// <summary>
		/// Gets or sets the Flutter widget displayed by this activity.
		/// </summary>
		public Widget Widget
		{
			get => widget;
			set
			{
				FlutterSharpLogger.LogDebug("Widget setter called, isReady: {IsReady}", isReady);
				if (widget != null && widget != value)
				{
					FlutterManager.UntrackWidget(widget);
					//Cleanup, send dispose
				}
				widget = value;
				if (isReady)
				{
					FlutterSharpLogger.LogDebug("Sending widget state from Widget setter");
					FlutterManager.SendState(widget);
				}
			}
		}

		#region Activity Lifecycle

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			FlutterSharpLogger.LogDebug("FlutterActivity.OnCreate called");

			FlutterHotReloadHelper.HotReloadHandler = this;

			channel = new MethodChannel(FlutterEngine.DartExecutor, "com.Microsoft.FlutterSharp/Messages");
			Flutter.Internal.Communicator.SendCommand = (x) => channel.InvokeMethod(x.Method, x.Arguments);
			channel.SetMethodCallHandler(this);

			// Register for state restoration
			StateRestorationService.Register(this);

			// Restore state from savedInstanceState bundle if available
			if (savedInstanceState != null)
			{
				RestoreFromBundle(savedInstanceState);
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			FlutterSharpLogger.LogDebug("FlutterActivity.OnResume called");
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);
		}

		protected override void OnPause()
		{
			FlutterSharpLogger.LogDebug("FlutterActivity.OnPause called");
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Paused);
			base.OnPause();
		}

		protected override void OnStop()
		{
			FlutterSharpLogger.LogDebug("FlutterActivity.OnStop called");
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Inactive);
			base.OnStop();
		}

		protected override void OnDestroy()
		{
			FlutterSharpLogger.LogDebug("FlutterActivity.OnDestroy called");
			FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Detached);
			StateRestorationService.Unregister(this);
			base.OnDestroy();
		}

		/// <summary>
		/// Called by Android to save instance state before the activity is destroyed.
		/// Used for configuration changes (rotation) and process death scenarios.
		/// </summary>
		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			SaveToBundle(outState);
		}

		#endregion

		#region Back Button Handling

		private bool _backPressHandled;
		private readonly object _backPressLock = new();

		/// <summary>
		/// Handles the Android back button press.
		/// Sends a BackPressed message to Flutter and waits for a response.
		/// If a Navigator widget can handle the back press (has routes to pop),
		/// the back navigation is handled by Flutter. Otherwise, the activity finishes.
		/// </summary>
		public override void OnBackPressed()
		{
			FlutterSharpLogger.LogDebug("FlutterActivity.OnBackPressed called");

			// First, try to handle back navigation through the widget tree
			if (TryHandleBackNavigation())
			{
				FlutterSharpLogger.LogDebug("Back navigation handled by C# widget");
				return;
			}

			// Send back press event to Flutter for PopScope/WillPopScope handling
			Task.Run(async () =>
			{
				var handled = await SendBackPressedToFlutterAsync();

				// If Flutter didn't handle it, finish the activity
				if (!handled)
				{
					RunOnUiThread(() =>
					{
						FlutterSharpLogger.LogDebug("Back press not handled by Flutter, finishing activity");
						Finish();
					});
				}
			});
		}

		/// <summary>
		/// Tries to handle back navigation through the C# widget tree.
		/// Looks for Navigator widgets and calls Pop() on them.
		/// </summary>
		/// <returns>True if back navigation was handled, false otherwise.</returns>
		private bool TryHandleBackNavigation()
		{
			if (widget == null)
				return false;

			// Check if the widget itself is a Navigator
			if (widget is Navigator navigator)
			{
				if (navigator.CanPop)
				{
					navigator.Pop();
					return true;
				}
				return false;
			}

			// For StatefulWidget/StatelessWidget, check if their built widget contains a Navigator
			// This is a simplified check - in production you might want to traverse the widget tree
			var navigatorWidget = FindNavigator(widget);
			if (navigatorWidget != null && navigatorWidget.CanPop)
			{
				navigatorWidget.Pop();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Recursively searches for a Navigator widget in the widget tree.
		/// </summary>
		private Navigator? FindNavigator(Widget widget)
		{
			if (widget is Navigator nav)
				return nav;

			// Check StatefulWidget and StatelessWidget Build results
			if (widget is StatefulWidget stateful)
			{
				var built = stateful.Build();
				return FindNavigator(built);
			}

			if (widget is StatelessWidget stateless)
			{
				var built = stateless.Build();
				return FindNavigator(built);
			}

			// TODO: Add support for traversing multi-child widgets if needed

			return null;
		}

		/// <summary>
		/// Sends a BackPressed message to Flutter and waits for a response.
		/// </summary>
		/// <returns>True if Flutter handled the back press, false otherwise.</returns>
		private async Task<bool> SendBackPressedToFlutterAsync()
		{
			if (!isReady || channel == null)
			{
				FlutterSharpLogger.LogDebug("Flutter not ready, cannot send back press");
				return false;
			}

			try
			{
				lock (_backPressLock)
				{
					_backPressHandled = false;
				}

				var tcs = new TaskCompletionSource<bool>();

				RunOnUiThread(async () =>
				{
					try
					{
						var result = await channel.InvokeMethodAsync("BackPressed", null);
						var handled = result?.ToString() == "true" || result?.ToString() == "True";
						tcs.TrySetResult(handled);
					}
					catch (Exception ex)
					{
						FlutterSharpLogger.LogWarning("Error invoking BackPressed on Flutter: {ErrorMessage}", ex.Message);
						tcs.TrySetResult(false);
					}
				});

				// Wait for Flutter response with configurable timeout
				var backPressTimeout = (int)MessageTimeoutHandler.Options.ResponseTimeout.TotalMilliseconds;
				if (backPressTimeout < 500)
					backPressTimeout = 2000; // Minimum 2 seconds for back press

				var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(backPressTimeout));
				if (completedTask == tcs.Task)
				{
					return tcs.Task.Result;
				}

				FlutterSharpLogger.LogWarning("BackPressed timeout after {TimeoutMs}ms, assuming not handled", backPressTimeout);
				return false;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Error sending BackPressed to Flutter");
				return false;
			}
		}

		#endregion

		#region State Restoration Bundle Handling

		/// <summary>
		/// Saves the current state to an Android Bundle.
		/// </summary>
		private void SaveToBundle(Bundle bundle)
		{
			try
			{
				var state = SaveState();
				var json = System.Text.Json.JsonSerializer.Serialize(state);
				bundle.PutString(StateKey, json);
				FlutterSharpLogger.LogDebug("Saved FlutterActivity state to Bundle: widgetType={WidgetType}", widget?.GetType().Name);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to save state to Bundle");
			}
		}

		/// <summary>
		/// Restores state from an Android Bundle.
		/// </summary>
		private void RestoreFromBundle(Bundle bundle)
		{
			try
			{
				var json = bundle.GetString(StateKey);
				if (!string.IsNullOrEmpty(json))
				{
					var state = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
					RestoreState(state);
					FlutterSharpLogger.LogDebug("Restored FlutterActivity state from Bundle");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to restore state from Bundle");
			}
		}

		#endregion

		#region IStateRestorable Implementation

		/// <summary>
		/// Gets or sets the restoration ID for this activity.
		/// </summary>
		public string RestorationId
		{
			get => _restorationId;
			set
			{
				if (_restorationId != value)
				{
					// Unregister old ID
					StateRestorationService.Unregister(_restorationId);
					_restorationId = value;
					// Register new ID
					if (!string.IsNullOrEmpty(_restorationId))
					{
						StateRestorationService.Register(this);
					}
				}
			}
		}

		/// <summary>
		/// Saves the current state for restoration.
		/// </summary>
		public Dictionary<string, object> SaveState()
		{
			var state = new Dictionary<string, object>();

			// Save the widget type so we can recreate it
			if (widget != null)
			{
				state["widgetType"] = widget.GetType().AssemblyQualifiedName;

				// If the widget supports state restoration, save its state
				if (widget is IStateRestorable restorableWidget)
				{
					state["widgetState"] = restorableWidget.SaveState();
				}
			}

			// Save ready state
			state["isReady"] = isReady;

			FlutterSharpLogger.LogDebug("Saved FlutterActivity state: widgetType={WidgetType}", widget?.GetType().Name);
			return state;
		}

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		public void RestoreState(Dictionary<string, object> state)
		{
			if (state == null)
				return;

			FlutterSharpLogger.LogDebug("Restoring FlutterActivity state");

			// Try to restore widget type and state
			if (state.TryGetValue("widgetType", out var widgetTypeObj) && widgetTypeObj is string widgetTypeName)
			{
				try
				{
					var widgetType = Type.GetType(widgetTypeName);
					if (widgetType != null && typeof(Widget).IsAssignableFrom(widgetType))
					{
						// Create a new instance of the widget
						var newWidget = (Widget)Activator.CreateInstance(widgetType);

						// Restore widget state if available
						if (newWidget is IStateRestorable restorableWidget &&
							state.TryGetValue("widgetState", out var widgetStateObj) &&
							widgetStateObj is Dictionary<string, object> widgetState)
						{
							restorableWidget.RestoreState(widgetState);
						}

						// Set the widget (this will trigger SendState if ready)
						Widget = newWidget;
						FlutterSharpLogger.LogInformation("Restored widget of type {WidgetType}", widgetType.Name);
					}
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Failed to restore widget of type {WidgetType}", widgetTypeName);
				}
			}
		}

		#endregion

		#region MethodChannel Handler

		public void OnMethodCall(MethodCall call, MethodChannel.IResult result)
		{
			FlutterSharpLogger.LogDebug("Received method call {MethodName}", call.Method);
			if (call.Method == "ready")
			{
				FlutterSharpLogger.LogDebug("Ready received, widget is null: {WidgetIsNull}", Widget == null);
				isReady = true;
				// Send widget state if widget is already set (handles race condition)
				if (Widget != null)
				{
					FlutterSharpLogger.LogDebug("Sending widget state from ready handler");
					FlutterManager.SendState(Widget);
				}
			}

			// Execute handler with timeout protection to avoid blocking UI thread
			MessageTimeoutHandler.ExecuteWithTimeout(
				() => Flutter.Internal.Communicator.OnCommandReceived?.Invoke((call.Method, call.Arguments()?.ToString() ?? "", (x) =>
				{
					result.Success(x);
				})),
				call.Method,
				(errorResponse) => result.Success(errorResponse),
				$"Android/{call.Method}");
		}

		#endregion

		#region Hot Reload

		public void Reload()
		{
			if (Widget == null)
				return;
			Widget.PrepareForSending();
			if (isReady)
				FlutterManager.SendState(Widget);
		}

		#endregion
	}
}
