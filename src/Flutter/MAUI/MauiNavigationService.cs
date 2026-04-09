using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.MAUI
{
	/// <summary>
	/// Navigation service that bridges Flutter and MAUI navigation systems.
	/// Enables Flutter widgets to perform MAUI navigation and receive navigation events.
	/// </summary>
	public class MauiNavigationService
	{
		private static MauiNavigationService? _instance;
		private static readonly object _lock = new object();

		private Shell? _shell;
		private NavigationPage? _navigationPage;
		private readonly Dictionary<string, Func<Page>> _pageFactories = new();
		private readonly Dictionary<string, Type> _pageTypes = new();

		/// <summary>
		/// Gets the singleton instance of the navigation service.
		/// </summary>
		public static MauiNavigationService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance ??= new MauiNavigationService();
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// Event raised when a navigation occurs in MAUI.
		/// </summary>
		public event EventHandler<MauiNavigationEventArgs>? NavigationOccurred;

		/// <summary>
		/// Event raised before navigation, allowing cancellation.
		/// </summary>
		public event EventHandler<MauiNavigatingEventArgs>? Navigating;

		/// <summary>
		/// The current Shell instance, if using Shell navigation.
		/// </summary>
		public Shell? Shell
		{
			get => _shell;
			set
			{
				if (_shell != null)
				{
					_shell.Navigating -= OnShellNavigating;
					_shell.Navigated -= OnShellNavigated;
				}
				_shell = value;
				if (_shell != null)
				{
					_shell.Navigating += OnShellNavigating;
					_shell.Navigated += OnShellNavigated;
				}
			}
		}

		/// <summary>
		/// The current NavigationPage instance, if using hierarchical navigation.
		/// </summary>
		public NavigationPage? NavigationPage
		{
			get => _navigationPage;
			set
			{
				if (_navigationPage != null)
				{
					_navigationPage.Pushed -= OnNavigationPagePushed;
					_navigationPage.Popped -= OnNavigationPagePopped;
				}
				_navigationPage = value;
				if (_navigationPage != null)
				{
					_navigationPage.Pushed += OnNavigationPagePushed;
					_navigationPage.Popped += OnNavigationPagePopped;
				}
			}
		}

		/// <summary>
		/// Gets the current route path in Shell navigation.
		/// </summary>
		public string? CurrentRoute => _shell?.CurrentState?.Location?.OriginalString;

		/// <summary>
		/// Gets the current page in NavigationPage.
		/// </summary>
		public Page? CurrentPage => _navigationPage?.CurrentPage ?? _shell?.CurrentPage;

		/// <summary>
		/// Initializes the navigation service with the application's Shell.
		/// Call this in AppShell constructor or App initialization.
		/// </summary>
		/// <param name="shell">The Shell instance to track</param>
		public static void InitializeWithShell(Shell shell)
		{
			Instance.Shell = shell;
			RegisterFlutterNavigationHandler();
		}

		/// <summary>
		/// Initializes the navigation service with a NavigationPage.
		/// Call this in App initialization when using hierarchical navigation.
		/// </summary>
		/// <param name="navigationPage">The NavigationPage instance to track</param>
		public static void InitializeWithNavigationPage(NavigationPage navigationPage)
		{
			Instance.NavigationPage = navigationPage;
			RegisterFlutterNavigationHandler();
		}

		/// <summary>
		/// Registers the handler for Flutter navigation requests.
		/// </summary>
		private static void RegisterFlutterNavigationHandler()
		{
			FlutterManager.RegisterEventHandler("MauiNavigation", Instance.HandleFlutterNavigationRequest);
		}

		/// <summary>
		/// Registers a page factory for a named route.
		/// This allows Flutter to navigate to MAUI pages by name.
		/// </summary>
		/// <param name="routeName">The route name to register</param>
		/// <param name="pageFactory">Factory function that creates the page</param>
		public void RegisterRoute(string routeName, Func<Page> pageFactory)
		{
			_pageFactories[routeName] = pageFactory;
		}

		/// <summary>
		/// Registers a page type for a named route.
		/// The page will be created using Activator.CreateInstance.
		/// </summary>
		/// <param name="routeName">The route name to register</param>
		/// <param name="pageType">The type of page to create</param>
		public void RegisterRoute(string routeName, Type pageType)
		{
			if (!typeof(Page).IsAssignableFrom(pageType))
				throw new ArgumentException($"Type {pageType.Name} is not a Page", nameof(pageType));
			_pageTypes[routeName] = pageType;
		}

		/// <summary>
		/// Registers a page type for a named route.
		/// </summary>
		/// <typeparam name="TPage">The type of page to create</typeparam>
		/// <param name="routeName">The route name to register</param>
		public void RegisterRoute<TPage>(string routeName) where TPage : Page, new()
		{
			_pageFactories[routeName] = () => new TPage();
		}

		/// <summary>
		/// Navigates to a Shell route.
		/// </summary>
		/// <param name="route">The route to navigate to (e.g., "//MainPage" or "details")</param>
		/// <param name="animate">Whether to animate the navigation</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task GoToAsync(string route, bool animate = true)
		{
			if (_shell == null)
				throw new InvalidOperationException("Shell not initialized. Call InitializeWithShell first.");

			await _shell.GoToAsync(route, animate);
		}

		/// <summary>
		/// Navigates to a Shell route with query parameters.
		/// </summary>
		/// <param name="route">The route to navigate to</param>
		/// <param name="parameters">Query parameters to pass</param>
		/// <param name="animate">Whether to animate the navigation</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task GoToAsync(string route, IDictionary<string, object> parameters, bool animate = true)
		{
			if (_shell == null)
				throw new InvalidOperationException("Shell not initialized. Call InitializeWithShell first.");

			await _shell.GoToAsync(route, animate, parameters);
		}

		/// <summary>
		/// Pushes a new page onto the NavigationPage stack.
		/// </summary>
		/// <param name="page">The page to push</param>
		/// <param name="animate">Whether to animate the push</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task PushAsync(Page page, bool animate = true)
		{
			if (_navigationPage != null)
			{
				await _navigationPage.PushAsync(page, animate);
			}
			else if (_shell != null)
			{
				await _shell.Navigation.PushAsync(page, animate);
			}
			else
			{
				throw new InvalidOperationException("No navigation context initialized.");
			}
		}

		/// <summary>
		/// Pushes a new page using a registered route name.
		/// </summary>
		/// <param name="routeName">The name of the registered route</param>
		/// <param name="animate">Whether to animate the push</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task PushAsync(string routeName, bool animate = true)
		{
			var page = CreatePage(routeName);
			await PushAsync(page, animate);
		}

		/// <summary>
		/// Pushes a modal page.
		/// </summary>
		/// <param name="page">The page to push modally</param>
		/// <param name="animate">Whether to animate the push</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task PushModalAsync(Page page, bool animate = true)
		{
			var navigation = GetCurrentNavigation();
			await navigation.PushModalAsync(page, animate);
		}

		/// <summary>
		/// Pops the current page from the navigation stack.
		/// </summary>
		/// <param name="animate">Whether to animate the pop</param>
		/// <returns>The popped page, or null if at the root</returns>
		public async Task<Page?> PopAsync(bool animate = true)
		{
			if (_navigationPage != null)
			{
				return await _navigationPage.PopAsync(animate);
			}
			else if (_shell != null)
			{
				return await _shell.Navigation.PopAsync(animate);
			}
			return null;
		}

		/// <summary>
		/// Pops to the root page.
		/// </summary>
		/// <param name="animate">Whether to animate</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task PopToRootAsync(bool animate = true)
		{
			var navigation = GetCurrentNavigation();
			await navigation.PopToRootAsync(animate);
		}

		/// <summary>
		/// Pops a modal page.
		/// </summary>
		/// <param name="animate">Whether to animate the pop</param>
		/// <returns>The popped page</returns>
		public async Task<Page?> PopModalAsync(bool animate = true)
		{
			var navigation = GetCurrentNavigation();
			return await navigation.PopModalAsync(animate);
		}

		/// <summary>
		/// Navigates back in Shell (using ".." route).
		/// </summary>
		/// <param name="animate">Whether to animate</param>
		/// <returns>A task that completes when navigation finishes</returns>
		public async Task GoBackAsync(bool animate = true)
		{
			if (_shell != null)
			{
				await _shell.GoToAsync("..", animate);
			}
			else
			{
				await PopAsync(animate);
			}
		}

		/// <summary>
		/// Creates a page from a registered route name.
		/// </summary>
		private Page CreatePage(string routeName)
		{
			if (_pageFactories.TryGetValue(routeName, out var factory))
			{
				return factory();
			}

			if (_pageTypes.TryGetValue(routeName, out var pageType))
			{
				return (Page)Activator.CreateInstance(pageType)!;
			}

			throw new ArgumentException($"No page registered for route '{routeName}'");
		}

		/// <summary>
		/// Gets the current INavigation interface.
		/// </summary>
		private INavigation GetCurrentNavigation()
		{
			if (_navigationPage != null)
				return _navigationPage.Navigation;
			if (_shell != null)
				return _shell.Navigation;
			throw new InvalidOperationException("No navigation context initialized.");
		}

		/// <summary>
		/// Handles navigation requests from Flutter.
		/// </summary>
		private void HandleFlutterNavigationRequest(string eventName, string data, Action<string> callback)
		{
			try
			{
				var request = System.Text.Json.JsonSerializer.Deserialize<NavigationRequest>(data);
				if (request == null)
				{
					callback?.Invoke("{\"success\": false, \"error\": \"Invalid request\"}");
					return;
				}

				// Execute navigation on main thread
				Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
				{
					try
					{
						await ExecuteNavigationRequest(request);
						callback?.Invoke("{\"success\": true}");
					}
					catch (Exception ex)
					{
						FlutterSharpLogger.LogError(ex, "MauiNavigationService navigation failed");
						callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
					}
				});
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MauiNavigationService failed to parse navigation request");
				callback?.Invoke($"{{\"success\": false, \"error\": \"{ex.Message}\"}}");
			}
		}

		/// <summary>
		/// Executes a navigation request.
		/// </summary>
		private async Task ExecuteNavigationRequest(NavigationRequest request)
		{
			switch (request.Action?.ToLowerInvariant())
			{
				case "push":
					if (!string.IsNullOrEmpty(request.Route))
					{
						if (_shell != null && request.UseShellNavigation)
						{
							await GoToAsync(request.Route, request.Animate);
						}
						else
						{
							await PushAsync(request.Route, request.Animate);
						}
					}
					break;

				case "pop":
					await PopAsync(request.Animate);
					break;

				case "poptoroot":
					await PopToRootAsync(request.Animate);
					break;

				case "pushmodal":
					if (!string.IsNullOrEmpty(request.Route))
					{
						var page = CreatePage(request.Route);
						await PushModalAsync(page, request.Animate);
					}
					break;

				case "popmodal":
					await PopModalAsync(request.Animate);
					break;

				case "goto":
					if (!string.IsNullOrEmpty(request.Route))
					{
						await GoToAsync(request.Route, request.Animate);
					}
					break;

				case "goback":
					await GoBackAsync(request.Animate);
					break;

				default:
					throw new ArgumentException($"Unknown navigation action: {request.Action}");
			}
		}

		#region Shell Navigation Events

		private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
		{
			var args = new MauiNavigatingEventArgs(
				e.Current?.Location?.OriginalString ?? "",
				e.Target?.Location?.OriginalString ?? "",
				NavigationType.Shell);

			Navigating?.Invoke(this, args);

			if (args.Cancel)
			{
				e.Cancel();
			}

			// Notify Flutter about navigation start
			NotifyFlutterNavigating(args);
		}

		private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
		{
			var args = new MauiNavigationEventArgs(
				e.Previous?.Location?.OriginalString ?? "",
				e.Current?.Location?.OriginalString ?? "",
				NavigationType.Shell,
				e.Source);

			NavigationOccurred?.Invoke(this, args);

			// Notify Flutter about navigation completion
			NotifyFlutterNavigated(args);
		}

		#endregion

		#region NavigationPage Events

		private void OnNavigationPagePushed(object? sender, NavigationEventArgs e)
		{
			var args = new MauiNavigationEventArgs(
				"",
				e.Page.GetType().Name,
				NavigationType.Push,
				ShellNavigationSource.Push);

			NavigationOccurred?.Invoke(this, args);
			NotifyFlutterNavigated(args);
		}

		private void OnNavigationPagePopped(object? sender, NavigationEventArgs e)
		{
			var args = new MauiNavigationEventArgs(
				e.Page.GetType().Name,
				"",
				NavigationType.Pop,
				ShellNavigationSource.Pop);

			NavigationOccurred?.Invoke(this, args);
			NotifyFlutterNavigated(args);
		}

		#endregion

		#region Flutter Notifications

		/// <summary>
		/// Notifies Flutter that navigation is starting.
		/// </summary>
		private void NotifyFlutterNavigating(MauiNavigatingEventArgs args)
		{
			try
			{
				var data = System.Text.Json.JsonSerializer.Serialize(new
				{
					type = "navigating",
					from = args.FromRoute,
					to = args.ToRoute,
					navigationType = args.NavigationType.ToString()
				});

				Communicator.SendCommand?.Invoke(("MauiNavigating", data));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MauiNavigationService failed to notify Flutter about navigating event");
			}
		}

		/// <summary>
		/// Notifies Flutter that navigation has completed.
		/// </summary>
		private void NotifyFlutterNavigated(MauiNavigationEventArgs args)
		{
			try
			{
				var data = System.Text.Json.JsonSerializer.Serialize(new
				{
					type = "navigated",
					from = args.FromRoute,
					to = args.ToRoute,
					navigationType = args.NavigationType.ToString(),
					source = args.Source.ToString()
				});

				Communicator.SendCommand?.Invoke(("MauiNavigated", data));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MauiNavigationService failed to notify Flutter about navigated event");
			}
		}

		#endregion
	}

	/// <summary>
	/// Types of navigation operations.
	/// </summary>
	public enum NavigationType
	{
		/// <summary>Push navigation (adding page to stack)</summary>
		Push,
		/// <summary>Pop navigation (removing page from stack)</summary>
		Pop,
		/// <summary>Shell route navigation</summary>
		Shell,
		/// <summary>Modal push</summary>
		Modal,
		/// <summary>Pop to root</summary>
		PopToRoot,
		/// <summary>Replace navigation</summary>
		Replace
	}

	/// <summary>
	/// Event args for navigation occurred events.
	/// </summary>
	public class MauiNavigationEventArgs : EventArgs
	{
		/// <summary>The route navigated from</summary>
		public string FromRoute { get; }
		/// <summary>The route navigated to</summary>
		public string ToRoute { get; }
		/// <summary>The type of navigation</summary>
		public NavigationType NavigationType { get; }
		/// <summary>The Shell navigation source (if applicable)</summary>
		public ShellNavigationSource Source { get; }

		public MauiNavigationEventArgs(string fromRoute, string toRoute, NavigationType navigationType, ShellNavigationSource source = ShellNavigationSource.Unknown)
		{
			FromRoute = fromRoute;
			ToRoute = toRoute;
			NavigationType = navigationType;
			Source = source;
		}
	}

	/// <summary>
	/// Event args for navigating events (before navigation).
	/// </summary>
	public class MauiNavigatingEventArgs : EventArgs
	{
		/// <summary>The route navigating from</summary>
		public string FromRoute { get; }
		/// <summary>The route navigating to</summary>
		public string ToRoute { get; }
		/// <summary>The type of navigation</summary>
		public NavigationType NavigationType { get; }
		/// <summary>Set to true to cancel navigation</summary>
		public bool Cancel { get; set; }

		public MauiNavigatingEventArgs(string fromRoute, string toRoute, NavigationType navigationType)
		{
			FromRoute = fromRoute;
			ToRoute = toRoute;
			NavigationType = navigationType;
		}
	}

	/// <summary>
	/// Represents a navigation request from Flutter.
	/// </summary>
	internal class NavigationRequest
	{
		/// <summary>The navigation action (push, pop, goto, etc.)</summary>
		public string? Action { get; set; }
		/// <summary>The route to navigate to</summary>
		public string? Route { get; set; }
		/// <summary>Whether to animate the navigation</summary>
		public bool Animate { get; set; } = true;
		/// <summary>Whether to use Shell navigation instead of NavigationPage</summary>
		public bool UseShellNavigation { get; set; } = true;
		/// <summary>Additional parameters to pass</summary>
		public Dictionary<string, object>? Parameters { get; set; }
	}
}
