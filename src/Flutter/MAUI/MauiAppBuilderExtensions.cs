using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Flutter.Initialization;

namespace Flutter.MAUI
{
	/// <summary>
	/// Extension methods for configuring FlutterSharp in MAUI applications
	/// </summary>
	public static class MauiAppBuilderExtensions
	{
		private static FlutterInitializationOptions? _pendingOptions;
		private static Action<FlutterInitializationResult>? _initCallback;

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

		/// <summary>
		/// Registers FlutterSharp handlers and services with async initialization and validation.
		/// The actual async initialization happens when the first FlutterView is loaded.
		/// </summary>
		/// <param name="builder">The MauiAppBuilder instance</param>
		/// <param name="options">Initialization options with timeout and fallback configuration</param>
		/// <returns>The MauiAppBuilder for chaining</returns>
		/// <remarks>
		/// <para>
		/// This method provides more robust initialization than <see cref="UseFlutterSharp"/>.
		/// Use this when you need:
		/// </para>
		/// <list type="bullet">
		/// <item>Timeout handling for Flutter initialization</item>
		/// <item>Graceful fallback when Flutter fails to load</item>
		/// <item>Detailed diagnostics on initialization failures</item>
		/// <item>Retry logic for flaky environments</item>
		/// </list>
		/// <para>
		/// Example usage:
		/// </para>
		/// <code>
		/// builder.UseFlutterSharpWithValidation(new FlutterInitializationOptions
		/// {
		///     ReadyTimeout = TimeSpan.FromSeconds(10),
		///     OnInitializationFailed = result =>
		///     {
		///         // Show MAUI fallback UI
		///         Application.Current.MainPage = new ContentPage
		///         {
		///             Content = new Label { Text = "Flutter failed to load" }
		///         };
		///     }
		/// });
		/// </code>
		/// </remarks>
		public static MauiAppBuilder UseFlutterSharpWithValidation(
			this MauiAppBuilder builder,
			FlutterInitializationOptions options)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<FlutterView, FlutterViewHandler>();
			});

			// Store options for when async init is triggered
			_pendingOptions = options;

			// Initialize the basic FlutterManager (sets up message handlers)
			Flutter.Internal.FlutterManager.Initialize();

			return builder;
		}

		/// <summary>
		/// Gets the pending initialization options set by UseFlutterSharpWithValidation.
		/// Used internally by FlutterView to complete async initialization.
		/// </summary>
		internal static FlutterInitializationOptions? GetPendingOptions()
		{
			var options = _pendingOptions;
			_pendingOptions = null; // Clear after retrieval
			return options;
		}

		/// <summary>
		/// Manually triggers Flutter async initialization with validation.
		/// Call this if you need to pre-initialize Flutter before showing any FlutterView.
		/// </summary>
		/// <param name="options">Initialization options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The initialization result</returns>
		public static Task<FlutterInitializationResult> InitializeFlutterAsync(
			FlutterInitializationOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			return Flutter.Internal.FlutterManager.InitializeAsync(options, cancellationToken);
		}

		/// <summary>
		/// Gets the result of the last Flutter initialization attempt.
		/// </summary>
		public static FlutterInitializationResult? LastInitializationResult
			=> Flutter.Internal.FlutterManager.LastInitializationResult;

		/// <summary>
		/// Validates the current Flutter configuration.
		/// Call this to check if Flutter is properly configured before attempting initialization.
		/// </summary>
		/// <returns>Validation result with any issues found</returns>
		public static FlutterInitializationValidator.ValidationResult ValidateFlutterConfiguration()
			=> Flutter.Internal.FlutterManager.ValidateConfiguration();

		/// <summary>
		/// Registers FlutterSharp with Shell navigation integration.
		/// Call this after UseFlutterSharp() to enable Flutter → MAUI Shell navigation.
		/// </summary>
		/// <param name="builder">The MauiAppBuilder instance</param>
		/// <param name="shell">The Shell instance to integrate with</param>
		/// <returns>The MauiAppBuilder for chaining</returns>
		public static MauiAppBuilder UseFlutterSharpWithShell(this MauiAppBuilder builder, Shell shell)
		{
			MauiNavigationService.InitializeWithShell(shell);
			return builder;
		}

		/// <summary>
		/// Registers FlutterSharp with NavigationPage integration.
		/// Call this after UseFlutterSharp() to enable Flutter → MAUI NavigationPage navigation.
		/// </summary>
		/// <param name="builder">The MauiAppBuilder instance</param>
		/// <param name="navigationPage">The NavigationPage instance to integrate with</param>
		/// <returns>The MauiAppBuilder for chaining</returns>
		public static MauiAppBuilder UseFlutterSharpWithNavigationPage(this MauiAppBuilder builder, NavigationPage navigationPage)
		{
			MauiNavigationService.InitializeWithNavigationPage(navigationPage);
			return builder;
		}
	}

	/// <summary>
	/// Extension methods for Shell to integrate with FlutterSharp navigation
	/// </summary>
	public static class ShellNavigationExtensions
	{
		/// <summary>
		/// Enables FlutterSharp navigation integration for this Shell.
		/// Call this in the Shell constructor after InitializeComponent().
		/// </summary>
		/// <param name="shell">The Shell instance</param>
		/// <returns>The Shell for chaining</returns>
		public static Shell UseFlutterSharpNavigation(this Shell shell)
		{
			MauiNavigationService.InitializeWithShell(shell);
			return shell;
		}

		/// <summary>
		/// Registers a MAUI page route that Flutter can navigate to.
		/// </summary>
		/// <typeparam name="TPage">The type of page to register</typeparam>
		/// <param name="shell">The Shell instance</param>
		/// <param name="routeName">The route name Flutter will use</param>
		/// <returns>The Shell for chaining</returns>
		public static Shell RegisterFlutterRoute<TPage>(this Shell shell, string routeName) where TPage : Page, new()
		{
			MauiNavigationService.Instance.RegisterRoute<TPage>(routeName);
			return shell;
		}

		/// <summary>
		/// Registers a MAUI page route that Flutter can navigate to.
		/// </summary>
		/// <param name="shell">The Shell instance</param>
		/// <param name="routeName">The route name Flutter will use</param>
		/// <param name="pageFactory">Factory function that creates the page</param>
		/// <returns>The Shell for chaining</returns>
		public static Shell RegisterFlutterRoute(this Shell shell, string routeName, Func<Page> pageFactory)
		{
			MauiNavigationService.Instance.RegisterRoute(routeName, pageFactory);
			return shell;
		}
	}

	/// <summary>
	/// Extension methods for NavigationPage to integrate with FlutterSharp navigation
	/// </summary>
	public static class NavigationPageExtensions
	{
		/// <summary>
		/// Enables FlutterSharp navigation integration for this NavigationPage.
		/// </summary>
		/// <param name="navigationPage">The NavigationPage instance</param>
		/// <returns>The NavigationPage for chaining</returns>
		public static NavigationPage UseFlutterSharpNavigation(this NavigationPage navigationPage)
		{
			MauiNavigationService.InitializeWithNavigationPage(navigationPage);
			return navigationPage;
		}

		/// <summary>
		/// Registers a MAUI page route that Flutter can navigate to.
		/// </summary>
		/// <typeparam name="TPage">The type of page to register</typeparam>
		/// <param name="navigationPage">The NavigationPage instance</param>
		/// <param name="routeName">The route name Flutter will use</param>
		/// <returns>The NavigationPage for chaining</returns>
		public static NavigationPage RegisterFlutterRoute<TPage>(this NavigationPage navigationPage, string routeName) where TPage : Page, new()
		{
			MauiNavigationService.Instance.RegisterRoute<TPage>(routeName);
			return navigationPage;
		}
	}
}
