using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Flutter;

namespace FlutterTest.Droid {
	[Activity (Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : FlutterActivity {
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			Xamarin.Essentials.Platform.Init (this, savedInstanceState);
			Widget = new FlutterSample ();
		}

		public override void OnRequestPermissionsResult (int requestCode, string [] permissions, [GeneratedEnum] Android.Content.PM.Permission [] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult (requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult (requestCode, permissions, grantResults);
		}
	}
}