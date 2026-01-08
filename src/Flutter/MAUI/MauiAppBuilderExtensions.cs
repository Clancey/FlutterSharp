using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;

namespace Flutter.MAUI
{
	/// <summary>
	/// Extension methods for configuring FlutterSharp in MAUI applications
	/// </summary>
	public static class MauiAppBuilderExtensions
	{
		/// <summary>
		/// Registers FlutterSharp handlers and services with the MAUI application
		/// </summary>
		/// <param name="builder">The MauiAppBuilder instance</param>
		/// <returns>The MauiAppBuilder for chaining</returns>
		public static MauiAppBuilder UseFlutterSharp(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<FlutterView, FlutterViewHandler>();
			});

			// Initialize FlutterManager
			Flutter.Internal.FlutterManager.Initialize();

			return builder;
		}
	}
}
