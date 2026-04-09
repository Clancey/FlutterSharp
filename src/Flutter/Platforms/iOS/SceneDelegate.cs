using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Flutter.StateRestoration;
using Flutter.Logging;

namespace Flutter.iOS
{
	/// <summary>
	/// Scene delegate helper for FlutterSharp state restoration and memory management.
	/// Use this to enable state persistence across app launches in iOS 13+.
	/// </summary>
	/// <remarks>
	/// To use, create your own UIWindowSceneDelegate that inherits from this class,
	/// or call the static methods from your existing scene delegate.
	/// This class automatically handles:
	/// - Lifecycle state notifications (resume, pause, inactive, detached)
	/// - State restoration via NSUserActivity
	/// - Memory warning handling
	/// </remarks>
	public abstract class FlutterSceneDelegate : UIResponder, IUIWindowSceneDelegate
	{
		/// <summary>
		/// The main window for this scene.
		/// </summary>
		[Export("window")]
		public virtual UIWindow Window { get; set; }

		/// <summary>
		/// The FlutterViewController for this scene, if any.
		/// </summary>
		protected FlutterViewController FlutterViewController { get; set; }

		/// <summary>
		/// Activity type used for state restoration.
		/// Override to customize.
		/// </summary>
		protected virtual string StateRestorationActivityType => NSBundle.MainBundle.BundleIdentifier + ".StateRestoration";

		/// <summary>
		/// Whether memory warning notifications are automatically registered.
		/// Override and return false to handle memory warnings manually.
		/// </summary>
		protected virtual bool AutoRegisterMemoryWarnings => true;

		/// <summary>
		/// Called when the scene is connecting.
		/// Sets up the window and restores state if available.
		/// </summary>
		[Export("scene:willConnectToSession:options:")]
		public virtual void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			if (scene is UIWindowScene windowScene)
			{
				Window = new UIWindow(windowScene);

				// Register for memory warnings
				if (AutoRegisterMemoryWarnings)
				{
					MemoryWarningHandler.Register();
				}

				// Check for state restoration
				if (session.StateRestorationActivity != null)
				{
					RestoreStateFromActivity(session.StateRestorationActivity);
				}

				// Look for handoff activity
				foreach (var userActivity in connectionOptions.UserActivities)
				{
					if (userActivity != null && userActivity.ActivityType == StateRestorationActivityType)
					{
						RestoreStateFromActivity(userActivity);
						break;
					}
				}

				// Setup the root view controller - subclasses should override ConfigureRootViewController
				ConfigureRootViewController(Window);

				Window.MakeKeyAndVisible();
			}
		}

		/// <summary>
		/// Override to configure the root view controller for the window.
		/// </summary>
		protected abstract void ConfigureRootViewController(UIWindow window);

		/// <summary>
		/// Called when the scene is about to disconnect.
		/// </summary>
		[Export("sceneDidDisconnect:")]
		public virtual void DidDisconnect(UIScene scene)
		{
			FlutterSharpLogger.LogDebug("Scene disconnected");

			// Notify lifecycle state change
			Internal.FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Detached);
		}

		/// <summary>
		/// Called when the scene becomes active (foreground).
		/// </summary>
		[Export("sceneDidBecomeActive:")]
		public virtual void DidBecomeActive(UIScene scene)
		{
			Internal.FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);
			FlutterSharpLogger.LogDebug("Scene became active");
		}

		/// <summary>
		/// Called when the scene will resign active.
		/// </summary>
		[Export("sceneWillResignActive:")]
		public virtual void WillResignActive(UIScene scene)
		{
			Internal.FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Inactive);
			FlutterSharpLogger.LogDebug("Scene will resign active");
		}

		/// <summary>
		/// Called when the scene enters the background.
		/// </summary>
		[Export("sceneDidEnterBackground:")]
		public virtual void DidEnterBackground(UIScene scene)
		{
			Internal.FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Paused);
			FlutterSharpLogger.LogDebug("Scene entered background");
		}

		/// <summary>
		/// Called when the scene will enter the foreground.
		/// </summary>
		[Export("sceneWillEnterForeground:")]
		public virtual void WillEnterForeground(UIScene scene)
		{
			Internal.FlutterManager.NotifyLifecycleState(FlutterLifecycleState.Resumed);
			FlutterSharpLogger.LogDebug("Scene will enter foreground");
		}

		/// <summary>
		/// Returns the activity to use for state restoration.
		/// Called by iOS when the app is about to be suspended.
		/// </summary>
		[Export("stateRestorationActivityForScene:")]
		public virtual NSUserActivity GetStateRestorationActivity(UIScene scene)
		{
			return CreateStateRestorationActivity();
		}

		/// <summary>
		/// Creates an NSUserActivity containing the current app state.
		/// </summary>
		protected virtual NSUserActivity CreateStateRestorationActivity()
		{
			var activity = new NSUserActivity(StateRestorationActivityType);
			activity.Title = "FlutterSharp State";

			try
			{
				// Save all registered restorable state
				var stateJson = StateRestorationService.SaveAllStateToJson();
				if (!string.IsNullOrEmpty(stateJson))
				{
					activity.AddUserInfoEntries(new NSDictionary<NSString, NSObject>(
						new NSString("flutter_state"),
						new NSString(stateJson)
					));
				}

				// Save navigation state if available
				var navState = SaveNavigationState();
				if (!string.IsNullOrEmpty(navState))
				{
					activity.AddUserInfoEntries(new NSDictionary<NSString, NSObject>(
						new NSString("navigation_state"),
						new NSString(navState)
					));
				}

				// Save FlutterViewController widget type if available
				if (FlutterViewController?.Widget != null)
				{
					activity.AddUserInfoEntries(new NSDictionary<NSString, NSObject>(
						new NSString("widget_type"),
						new NSString(FlutterViewController.Widget.GetType().AssemblyQualifiedName)
					));
				}

				FlutterSharpLogger.LogInformation("Created state restoration activity with {Count} restorable objects", StateRestorationService.RegisteredCount);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to create state restoration activity");
			}

			return activity;
		}

		/// <summary>
		/// Restores state from an NSUserActivity.
		/// </summary>
		protected virtual void RestoreStateFromActivity(NSUserActivity activity)
		{
			if (activity == null)
				return;

			try
			{
				FlutterSharpLogger.LogDebug("Restoring state from activity: {ActivityType}", activity.ActivityType);

				// Restore widget state
				if (activity.UserInfo?.TryGetValue(new NSString("flutter_state"), out var stateObj) == true &&
					stateObj is NSString stateJson)
				{
					StateRestorationService.RestoreAllStateFromJson(stateJson.ToString());
				}

				// Restore navigation state
				if (activity.UserInfo?.TryGetValue(new NSString("navigation_state"), out var navObj) == true &&
					navObj is NSString navJson)
				{
					RestoreNavigationState(navJson.ToString());
				}

				FlutterSharpLogger.LogInformation("State restoration completed");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to restore state from activity");
			}
		}

		/// <summary>
		/// Override to save custom navigation state.
		/// </summary>
		protected virtual string SaveNavigationState()
		{
			return null;
		}

		/// <summary>
		/// Override to restore custom navigation state.
		/// </summary>
		protected virtual void RestoreNavigationState(string navState)
		{
		}
	}

	/// <summary>
	/// Helper extension methods for UIApplicationDelegate to support state restoration
	/// without requiring a UIWindowSceneDelegate.
	/// </summary>
	public static class FlutterStateRestorationExtensions
	{
		private const string StateRestorationKey = "FlutterSharpState";

		/// <summary>
		/// Encodes FlutterSharp state to NSCoder.
		/// Call this from your AppDelegate's EncodeRestorableState.
		/// </summary>
		public static void EncodeFlutterState(NSCoder coder)
		{
			try
			{
				var stateJson = StateRestorationService.SaveAllStateToJson();
				if (!string.IsNullOrEmpty(stateJson))
				{
					coder.Encode(new NSString(stateJson), StateRestorationKey);
					FlutterSharpLogger.LogDebug("Encoded FlutterSharp state to coder");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to encode FlutterSharp state");
			}
		}

		/// <summary>
		/// Decodes FlutterSharp state from NSCoder.
		/// Call this from your AppDelegate's DecodeRestorableState.
		/// </summary>
		public static void DecodeFlutterState(NSCoder coder)
		{
			try
			{
				if (coder.ContainsKey(StateRestorationKey))
				{
					var stateObj = coder.DecodeObject(StateRestorationKey);
					if (stateObj is NSString stateJson)
					{
						StateRestorationService.RestoreAllStateFromJson(stateJson.ToString());
						FlutterSharpLogger.LogDebug("Decoded FlutterSharp state from coder");
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to decode FlutterSharp state");
			}
		}

		/// <summary>
		/// Creates an NSUserActivity for state restoration.
		/// Call this from stateRestorationActivityForScene: if using scenes,
		/// or from application:shouldSaveSecureApplicationState: otherwise.
		/// </summary>
		public static NSUserActivity CreateFlutterStateActivity(string activityType)
		{
			var activity = new NSUserActivity(activityType);
			activity.Title = "FlutterSharp State";

			try
			{
				var stateJson = StateRestorationService.SaveAllStateToJson();
				if (!string.IsNullOrEmpty(stateJson))
				{
					activity.AddUserInfoEntries(new NSDictionary<NSString, NSObject>(
						new NSString("flutter_state"),
						new NSString(stateJson)
					));
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to create Flutter state activity");
			}

			return activity;
		}

		/// <summary>
		/// Restores state from an NSUserActivity.
		/// Call this from scene:willConnectToSession:options: or application:continueUserActivity:.
		/// </summary>
		public static void RestoreFromFlutterStateActivity(NSUserActivity activity)
		{
			if (activity?.UserInfo == null)
				return;

			try
			{
				if (activity.UserInfo.TryGetValue(new NSString("flutter_state"), out var stateObj) &&
					stateObj is NSString stateJson)
				{
					StateRestorationService.RestoreAllStateFromJson(stateJson.ToString());
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to restore from Flutter state activity");
			}
		}
	}
}
