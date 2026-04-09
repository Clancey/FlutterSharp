namespace FlutterSample.MAUI;

/// <summary>
/// Main application class for the FlutterSharp MAUI sample.
/// Presents the single demo page directly because the sample no longer uses Shell navigation.
/// </summary>
public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage());
	}
}
