using System;
using System.Threading.Tasks;
using Flutter;
using Foundation;
using UIKit;

namespace FlutterSample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			window = new UIWindow
			{
				BackgroundColor = UIColor.White,
				RootViewController = new FlutterViewController
				{
					Widget = new FlutterSample(),
				}
			};
			window.MakeKeyAndVisible();
			Reloadify.Reload.Instance.ReplaceType = (arg) => Flutter.HotReload.FlutterHotReloadHelper.RegisterReplacedView(arg.ClassName, arg.Type);
			Reloadify.Reload.Instance.FinishedReload = () => Flutter.HotReload.FlutterHotReloadHelper.TriggerReload();
			Reloadify.Reload.Init();
			Flutter.HotReload.FlutterHotReloadHelper.Init();
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			return true;
		}
		// UISceneSession Lifecycle

	}
}

