using System;
using System.Threading.Tasks;
using Flutter;
using Flutter.iOS;
using Foundation;
using UIKit;

namespace FlutterTest.iOS {
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate {
		UIWindow window;
		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			window = new UIWindow {
				BackgroundColor = UIColor.White,
				RootViewController = new FlutterViewController {
					Widget = new FlutterSample(),
				}
			};
			window.MakeKeyAndVisible ();
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			return true;
		}
		// UISceneSession Lifecycle

	}
}

