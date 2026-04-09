using System;
using Foundation;
using UIKit;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.iOS
{
	/// <summary>
	/// Handles iOS memory warnings and notifies FlutterSharp.
	/// This class integrates with UIApplicationDelegate to receive memory warnings
	/// and propagate them to Flutter and C# subscribers.
	/// </summary>
	public static class MemoryWarningHandler
	{
		private static bool _isRegistered;
		private static NSObject _memoryWarningObserver;

		/// <summary>
		/// Registers for memory warning notifications from iOS.
		/// Call this from your AppDelegate.FinishedLaunching or similar entry point.
		/// </summary>
		public static void Register()
		{
			if (_isRegistered)
				return;

			_memoryWarningObserver = NSNotificationCenter.DefaultCenter.AddObserver(
				UIApplication.DidReceiveMemoryWarningNotification,
				HandleMemoryWarningNotification);

			_isRegistered = true;
			FlutterSharpLogger.LogInformation("Memory warning handler registered");
		}

		/// <summary>
		/// Unregisters from memory warning notifications.
		/// Call this during app cleanup if needed.
		/// </summary>
		public static void Unregister()
		{
			if (!_isRegistered)
				return;

			if (_memoryWarningObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_memoryWarningObserver);
				_memoryWarningObserver.Dispose();
				_memoryWarningObserver = null;
			}

			_isRegistered = false;
			FlutterSharpLogger.LogInformation("Memory warning handler unregistered");
		}

		/// <summary>
		/// Handles the iOS memory warning notification.
		/// </summary>
		private static void HandleMemoryWarningNotification(NSNotification notification)
		{
			FlutterSharpLogger.LogWarning("iOS memory warning notification received");

			// iOS doesn't provide detailed memory pressure levels via UIApplication notifications,
			// so we default to Critical (the typical iOS behavior for DidReceiveMemoryWarning)
			FlutterManager.NotifyMemoryWarning(MemoryWarningLevel.Critical);
		}

		/// <summary>
		/// Call this from your AppDelegate.DidReceiveMemoryWarning override.
		/// This provides an alternative to the notification observer pattern.
		/// </summary>
		/// <param name="application">The UIApplication instance</param>
		public static void HandleDidReceiveMemoryWarning(UIApplication application)
		{
			FlutterSharpLogger.LogWarning("iOS DidReceiveMemoryWarning called");
			FlutterManager.NotifyMemoryWarning(MemoryWarningLevel.Critical);
		}

		/// <summary>
		/// Call this with explicit memory pressure level if you have access to
		/// ProcessInfo memory pressure notifications (iOS 7+).
		/// </summary>
		/// <param name="pressure">The memory pressure level from ProcessInfo</param>
		public static void HandleMemoryPressure(NSProcessInfoThermalState pressure)
		{
			var level = pressure switch
			{
				NSProcessInfoThermalState.Nominal => MemoryWarningLevel.Low,
				NSProcessInfoThermalState.Fair => MemoryWarningLevel.Medium,
				NSProcessInfoThermalState.Serious => MemoryWarningLevel.High,
				NSProcessInfoThermalState.Critical => MemoryWarningLevel.Critical,
				_ => MemoryWarningLevel.Medium
			};

			FlutterSharpLogger.LogWarning("iOS thermal state: {State}, mapped to {Level}", pressure, level);
			FlutterManager.NotifyMemoryWarning(level);
		}
	}

	/// <summary>
	/// Extension methods for UIApplicationDelegate to easily integrate memory warning handling.
	/// </summary>
	public static class UIApplicationDelegateExtensions
	{
		/// <summary>
		/// Call this from your FinishedLaunching to automatically register for memory warnings.
		/// </summary>
		public static void UseFlutterMemoryWarningHandler(this UIApplicationDelegate appDelegate)
		{
			MemoryWarningHandler.Register();
		}
	}
}
