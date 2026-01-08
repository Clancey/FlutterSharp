namespace FlutterSample.MAUI;

/// <summary>
/// Main application class for the FlutterSharp MAUI sample.
/// Sets up the Shell-based navigation structure.
/// </summary>
public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
