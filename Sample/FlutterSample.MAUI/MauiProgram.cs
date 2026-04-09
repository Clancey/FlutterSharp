using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// Entry point for the .NET MAUI application.
/// Configures FlutterSharp integration and application services.
/// </summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.UseFlutterSharp() // Register FlutterSharp handlers and services
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
