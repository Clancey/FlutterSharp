using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Flutter;

namespace FlutterSample
{
	[Activity(Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : FlutterActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Widget = new FlutterSample();
		}
	}
}