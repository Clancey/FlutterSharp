using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Flutter;
using Flutter.HotReload;

namespace FlutterSample
{
	[Activity(Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : FlutterActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Reloadify.Reload.Instance.ReplaceType = (x) => FlutterHotReloadHelper.RegisterReplacedView(x.ClassName,x.Type);
			Reloadify.Reload.Instance.FinishedReload = () => FlutterHotReloadHelper.TriggerReload();
			Reloadify.Reload.Init();
			Widget = new FlutterSample();
		}
	}
}