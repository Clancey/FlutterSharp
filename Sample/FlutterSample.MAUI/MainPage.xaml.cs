using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// Main page demonstrating FlutterView embedded in a MAUI ContentPage.
/// Shows a simple "Hello World" Flutter widget composed from C#.
/// </summary>
public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Set the Flutter widget when the page appears
		flutterView.Widget = new HelloWorldWidget();
	}
}

/// <summary>
/// A simple Flutter widget that displays a welcome message.
/// Demonstrates basic widget composition with Center, Column, and Text.
/// </summary>
public class HelloWorldWidget : StatelessWidget
{
	public override Widget Build() =>
		new Center(child: new Column
		{
			new Text("Hello from FlutterSharp!"),
			new SizedBox(height: 20),
			new Text("This is a .NET MAUI to Flutter interop demo"),
			new SizedBox(height: 20),
			new Text("FlutterView is embedded in a MAUI ContentPage"),
			new SizedBox(height: 40),
			new Row
			{
				new Text("Row item 1"),
				new SizedBox(width: 10),
				new Text("Row item 2"),
				new SizedBox(width: 10),
				new Text("Row item 3"),
			},
		});
}
