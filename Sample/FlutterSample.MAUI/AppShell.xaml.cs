namespace FlutterSample.MAUI;

/// <summary>
/// Shell navigation structure for the FlutterSharp MAUI sample.
/// Provides tab-based navigation between different Flutter widget demos.
/// </summary>
public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for programmatic navigation
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(CounterPage), typeof(CounterPage));
		Routing.RegisterRoute(nameof(ListDemoPage), typeof(ListDemoPage));
	}
}
