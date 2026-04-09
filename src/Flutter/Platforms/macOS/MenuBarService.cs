using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flutter.Logging;

namespace Flutter.macOS
{
	/// <summary>
	/// Represents a menu item definition for the native macOS menu bar.
	/// </summary>
	public class MenuItem
	{
		/// <summary>
		/// The display title of the menu item.
		/// </summary>
		public string Title { get; set; } = "";

		/// <summary>
		/// The keyboard shortcut key (e.g., "n" for Cmd+N). Empty string for no shortcut.
		/// </summary>
		public string KeyEquivalent { get; set; } = "";

		/// <summary>
		/// Modifier keys for the keyboard shortcut (combination of ModifierCommand, ModifierShift, etc.).
		/// Default is Command key only.
		/// </summary>
		public ulong KeyModifiers { get; set; } = MenuBarInterop.ModifierCommand;

		/// <summary>
		/// Action to execute when the menu item is clicked.
		/// </summary>
		public Action? Action { get; set; }

		/// <summary>
		/// Unique identifier for the menu item.
		/// </summary>
		public string Id { get; set; } = "";

		/// <summary>
		/// Whether the menu item is enabled.
		/// </summary>
		public bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Whether the menu item is checked/selected.
		/// </summary>
		public bool IsChecked { get; set; }

		/// <summary>
		/// Whether this item is a separator.
		/// </summary>
		public bool IsSeparator { get; set; }

		/// <summary>
		/// Child menu items (for submenus).
		/// </summary>
		public List<MenuItem>? Children { get; set; }

		/// <summary>
		/// Creates a new menu item.
		/// </summary>
		public MenuItem() { }

		/// <summary>
		/// Creates a new menu item with the specified title and action.
		/// </summary>
		/// <param name="title">The display title</param>
		/// <param name="action">The action to execute when clicked</param>
		/// <param name="keyEquivalent">The keyboard shortcut key</param>
		public MenuItem(string title, Action? action = null, string keyEquivalent = "")
		{
			Title = title;
			Action = action;
			KeyEquivalent = keyEquivalent;
			Id = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Creates a separator menu item.
		/// </summary>
		/// <returns>A separator menu item</returns>
		public static MenuItem Separator() => new MenuItem { IsSeparator = true, Id = Guid.NewGuid().ToString() };
	}

	/// <summary>
	/// Represents a top-level menu in the menu bar (e.g., File, Edit, View).
	/// </summary>
	public class MenuDefinition
	{
		/// <summary>
		/// The display title of the menu.
		/// </summary>
		public string Title { get; set; } = "";

		/// <summary>
		/// The items in this menu.
		/// </summary>
		public List<MenuItem> Items { get; set; } = new();

		/// <summary>
		/// Creates a new menu definition.
		/// </summary>
		public MenuDefinition() { }

		/// <summary>
		/// Creates a new menu definition with the specified title.
		/// </summary>
		/// <param name="title">The menu title</param>
		public MenuDefinition(string title)
		{
			Title = title;
		}
	}

	/// <summary>
	/// Provides high-level management of the native macOS menu bar.
	/// </summary>
	/// <remarks>
	/// This service allows FlutterSharp applications to integrate with the native macOS
	/// menu bar, adding custom menus and handling menu item actions from C# code.
	///
	/// Usage:
	/// <code>
	/// var menuService = MenuBarService.Instance;
	/// menuService.AddMenu(new MenuDefinition("File")
	/// {
	///     Items = new List&lt;MenuItem&gt;
	///     {
	///         new MenuItem("New", () => CreateNewDocument(), "n"),
	///         new MenuItem("Open...", () => OpenDocument(), "o"),
	///         MenuItem.Separator(),
	///         new MenuItem("Save", () => SaveDocument(), "s"),
	///     }
	/// });
	/// </code>
	/// </remarks>
	public class MenuBarService : IDisposable
	{
		#region Singleton

		private static MenuBarService? _instance;
		private static readonly object _lock = new object();

		/// <summary>
		/// Gets the singleton instance of the menu bar service.
		/// </summary>
		public static MenuBarService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance ??= new MenuBarService();
					}
				}
				return _instance;
			}
		}

		#endregion

		#region Fields

		private IntPtr _mainMenu;
		private readonly Dictionary<string, MenuItem> _menuItemRegistry = new();
		private readonly Dictionary<string, IntPtr> _nativeMenuItems = new();
		private readonly Dictionary<string, IntPtr> _nativeMenus = new();
		private readonly List<MenuDefinition> _menuDefinitions = new();
		private bool _disposed;
		private int _nextTag = 1000;

		// Delegate to prevent GC of callback
		private delegate void MenuActionHandler(IntPtr self, IntPtr cmd, IntPtr sender);
		private MenuActionHandler? _menuActionCallback;
		private IntPtr _targetObject;
		private IntPtr _actionSelector;

		#endregion

		#region Constructor

		private MenuBarService()
		{
			try
			{
				// Get or create the main menu
				_mainMenu = MenuBarInterop.GetMainMenu();
				if (_mainMenu == IntPtr.Zero)
				{
					_mainMenu = MenuBarInterop.CreateMenu("");
					MenuBarInterop.SetMainMenu(_mainMenu);
				}

				// Set up the action handler
				SetupActionHandler();

				FlutterSharpLogger.LogInformation("MenuBarService: Initialized successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Failed to initialize");
			}
		}

		private void SetupActionHandler()
		{
			// For Mac Catalyst, we need to create an Objective-C class to handle menu actions
			// This is a simplified approach using function pointers
			_actionSelector = sel_registerName("menuItemClicked:");

			// Store the callback to prevent GC
			_menuActionCallback = OnMenuItemClicked;
		}

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
		private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string selectorName);

		private void OnMenuItemClicked(IntPtr self, IntPtr cmd, IntPtr sender)
		{
			try
			{
				// Get the tag from the sender (menu item)
				var tag = MenuBarInterop.GetTag(sender);

				// Find the menu item by tag
				foreach (var kvp in _menuItemRegistry)
				{
					if (_nativeMenuItems.TryGetValue(kvp.Key, out var nativeItem))
					{
						if (MenuBarInterop.GetTag(nativeItem) == tag)
						{
							FlutterSharpLogger.LogDebug("MenuBarService: Menu item clicked - {Title}", kvp.Value.Title);
							kvp.Value.Action?.Invoke();
							return;
						}
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Error handling menu item click");
			}
		}

		#endregion

		#region Public API

		/// <summary>
		/// Gets all registered menu definitions.
		/// </summary>
		public IReadOnlyList<MenuDefinition> Menus => _menuDefinitions.AsReadOnly();

		/// <summary>
		/// Adds a new menu to the menu bar.
		/// </summary>
		/// <param name="menu">The menu definition to add</param>
		/// <param name="index">Optional index at which to insert (appends if not specified)</param>
		public void AddMenu(MenuDefinition menu, int? index = null)
		{
			if (menu == null)
				throw new ArgumentNullException(nameof(menu));

			try
			{
				_menuDefinitions.Add(menu);
				var nativeMenu = CreateNativeMenu(menu);
				if (nativeMenu != IntPtr.Zero)
				{
					var menuItem = MenuBarInterop.CreateMenuItem(menu.Title, IntPtr.Zero, "");
					if (menuItem != IntPtr.Zero)
					{
						MenuBarInterop.SetSubmenu(menuItem, nativeMenu);

						if (index.HasValue)
						{
							MenuBarInterop.InsertMenuItem(_mainMenu, menuItem, index.Value);
						}
						else
						{
							MenuBarInterop.AddMenuItem(_mainMenu, menuItem);
						}

						_nativeMenus[menu.Title] = nativeMenu;
						FlutterSharpLogger.LogInformation("MenuBarService: Added menu '{Title}'", menu.Title);
					}
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Failed to add menu '{Title}'", menu.Title);
			}
		}

		/// <summary>
		/// Removes a menu from the menu bar.
		/// </summary>
		/// <param name="title">The title of the menu to remove</param>
		public void RemoveMenu(string title)
		{
			try
			{
				var menuToRemove = _menuDefinitions.Find(m => m.Title == title);
				if (menuToRemove != null)
				{
					_menuDefinitions.Remove(menuToRemove);
				}

				if (_nativeMenus.TryGetValue(title, out var nativeMenu))
				{
					// Find and remove the menu item from main menu
					var itemCount = MenuBarInterop.GetMenuItemCount(_mainMenu);
					for (int i = 0; i < itemCount; i++)
					{
						var item = MenuBarInterop.GetMenuItemAtIndex(_mainMenu, i);
						var submenu = MenuBarInterop.GetSubmenu(item);
						if (submenu == nativeMenu)
						{
							MenuBarInterop.RemoveMenuItemAtIndex(_mainMenu, i);
							break;
						}
					}

					_nativeMenus.Remove(title);
					FlutterSharpLogger.LogInformation("MenuBarService: Removed menu '{Title}'", title);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Failed to remove menu '{Title}'", title);
			}
		}

		/// <summary>
		/// Adds a menu item to an existing menu.
		/// </summary>
		/// <param name="menuTitle">The title of the menu to add to</param>
		/// <param name="item">The menu item to add</param>
		public void AddMenuItem(string menuTitle, MenuItem item)
		{
			if (!_nativeMenus.TryGetValue(menuTitle, out var nativeMenu))
			{
				FlutterSharpLogger.LogWarning("MenuBarService: Menu '{Title}' not found", menuTitle);
				return;
			}

			try
			{
				var nativeItem = CreateNativeMenuItem(item);
				if (nativeItem != IntPtr.Zero)
				{
					MenuBarInterop.AddMenuItem(nativeMenu, nativeItem);
					FlutterSharpLogger.LogDebug("MenuBarService: Added item '{ItemTitle}' to menu '{MenuTitle}'",
						item.Title, menuTitle);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Failed to add item to menu '{Title}'", menuTitle);
			}
		}

		/// <summary>
		/// Updates the enabled state of a menu item.
		/// </summary>
		/// <param name="itemId">The ID of the menu item</param>
		/// <param name="enabled">Whether the item should be enabled</param>
		public void SetMenuItemEnabled(string itemId, bool enabled)
		{
			if (_menuItemRegistry.TryGetValue(itemId, out var item))
			{
				item.IsEnabled = enabled;
				if (_nativeMenuItems.TryGetValue(itemId, out var nativeItem))
				{
					MenuBarInterop.SetEnabled(nativeItem, enabled);
				}
			}
		}

		/// <summary>
		/// Updates the checked state of a menu item.
		/// </summary>
		/// <param name="itemId">The ID of the menu item</param>
		/// <param name="checked">Whether the item should be checked</param>
		public void SetMenuItemChecked(string itemId, bool @checked)
		{
			if (_menuItemRegistry.TryGetValue(itemId, out var item))
			{
				item.IsChecked = @checked;
				if (_nativeMenuItems.TryGetValue(itemId, out var nativeItem))
				{
					MenuBarInterop.SetState(nativeItem, @checked ? MenuBarInterop.StateOn : MenuBarInterop.StateOff);
				}
			}
		}

		/// <summary>
		/// Updates the title of a menu item.
		/// </summary>
		/// <param name="itemId">The ID of the menu item</param>
		/// <param name="title">The new title</param>
		public void SetMenuItemTitle(string itemId, string title)
		{
			if (_menuItemRegistry.TryGetValue(itemId, out var item))
			{
				item.Title = title;
				if (_nativeMenuItems.TryGetValue(itemId, out var nativeItem))
				{
					MenuBarInterop.SetTitle(nativeItem, title);
				}
			}
		}

		/// <summary>
		/// Gets a menu item by its ID.
		/// </summary>
		/// <param name="itemId">The ID of the menu item</param>
		/// <returns>The menu item, or null if not found</returns>
		public MenuItem? GetMenuItem(string itemId)
		{
			return _menuItemRegistry.TryGetValue(itemId, out var item) ? item : null;
		}

		/// <summary>
		/// Refreshes the menu bar to reflect any changes.
		/// </summary>
		public void RefreshMenuBar()
		{
			try
			{
				MenuBarInterop.UpdateMenu(_mainMenu);
				foreach (var menu in _nativeMenus.Values)
				{
					MenuBarInterop.UpdateMenu(menu);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarService: Failed to refresh menu bar");
			}
		}

		/// <summary>
		/// Creates standard application menus (File, Edit, View, Window, Help).
		/// </summary>
		public void CreateStandardMenus()
		{
			// File menu
			AddMenu(new MenuDefinition("File")
			{
				Items = new List<MenuItem>
				{
					new MenuItem("New", null, "n") { Id = "file.new" },
					new MenuItem("Open...", null, "o") { Id = "file.open" },
					MenuItem.Separator(),
					new MenuItem("Close", null, "w") { Id = "file.close" },
					new MenuItem("Save", null, "s") { Id = "file.save" },
					new MenuItem("Save As...", null, "S") { Id = "file.saveAs", KeyModifiers = MenuBarInterop.ModifierCommand | MenuBarInterop.ModifierShift },
				}
			});

			// Edit menu
			AddMenu(new MenuDefinition("Edit")
			{
				Items = new List<MenuItem>
				{
					new MenuItem("Undo", null, "z") { Id = "edit.undo" },
					new MenuItem("Redo", null, "Z") { Id = "edit.redo", KeyModifiers = MenuBarInterop.ModifierCommand | MenuBarInterop.ModifierShift },
					MenuItem.Separator(),
					new MenuItem("Cut", null, "x") { Id = "edit.cut" },
					new MenuItem("Copy", null, "c") { Id = "edit.copy" },
					new MenuItem("Paste", null, "v") { Id = "edit.paste" },
					new MenuItem("Delete", null, "") { Id = "edit.delete" },
					MenuItem.Separator(),
					new MenuItem("Select All", null, "a") { Id = "edit.selectAll" },
				}
			});

			// View menu
			AddMenu(new MenuDefinition("View")
			{
				Items = new List<MenuItem>
				{
					new MenuItem("Zoom In", null, "+") { Id = "view.zoomIn" },
					new MenuItem("Zoom Out", null, "-") { Id = "view.zoomOut" },
					new MenuItem("Actual Size", null, "0") { Id = "view.actualSize" },
					MenuItem.Separator(),
					new MenuItem("Enter Full Screen", null, "f") { Id = "view.fullScreen", KeyModifiers = MenuBarInterop.ModifierCommand | MenuBarInterop.ModifierControl },
				}
			});

			// Window menu
			AddMenu(new MenuDefinition("Window")
			{
				Items = new List<MenuItem>
				{
					new MenuItem("Minimize", null, "m") { Id = "window.minimize" },
					new MenuItem("Zoom", null, "") { Id = "window.zoom" },
					MenuItem.Separator(),
					new MenuItem("Bring All to Front", null, "") { Id = "window.bringAllToFront" },
				}
			});

			// Help menu
			AddMenu(new MenuDefinition("Help")
			{
				Items = new List<MenuItem>
				{
					new MenuItem("FlutterSharp Help", null, "?") { Id = "help.main" },
				}
			});

			FlutterSharpLogger.LogInformation("MenuBarService: Created standard application menus");
		}

		#endregion

		#region Private Helpers

		private IntPtr CreateNativeMenu(MenuDefinition menu)
		{
			var nativeMenu = MenuBarInterop.CreateMenu(menu.Title);
			if (nativeMenu == IntPtr.Zero)
				return IntPtr.Zero;

			foreach (var item in menu.Items)
			{
				var nativeItem = CreateNativeMenuItem(item);
				if (nativeItem != IntPtr.Zero)
				{
					MenuBarInterop.AddMenuItem(nativeMenu, nativeItem);
				}
			}

			return nativeMenu;
		}

		private IntPtr CreateNativeMenuItem(MenuItem item)
		{
			IntPtr nativeItem;

			if (item.IsSeparator)
			{
				nativeItem = MenuBarInterop.CreateSeparatorItem();
			}
			else
			{
				nativeItem = MenuBarInterop.CreateMenuItem(
					item.Title,
					item.Action != null ? _actionSelector : IntPtr.Zero,
					item.KeyEquivalent);

				if (nativeItem != IntPtr.Zero)
				{
					// Assign tag for action routing
					var tag = _nextTag++;
					MenuBarInterop.SetTag(nativeItem, tag);

					// Set enabled/checked state
					MenuBarInterop.SetEnabled(nativeItem, item.IsEnabled);
					if (item.IsChecked)
					{
						MenuBarInterop.SetState(nativeItem, MenuBarInterop.StateOn);
					}

					// Register for action handling
					if (!string.IsNullOrEmpty(item.Id))
					{
						_menuItemRegistry[item.Id] = item;
						_nativeMenuItems[item.Id] = nativeItem;
					}

					// Create submenu if children exist
					if (item.Children != null && item.Children.Count > 0)
					{
						var submenu = MenuBarInterop.CreateMenu(item.Title);
						foreach (var child in item.Children)
						{
							var childItem = CreateNativeMenuItem(child);
							if (childItem != IntPtr.Zero)
							{
								MenuBarInterop.AddMenuItem(submenu, childItem);
							}
						}
						MenuBarInterop.SetSubmenu(nativeItem, submenu);
					}
				}
			}

			return nativeItem;
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when a menu item is about to be clicked.
		/// </summary>
		public event EventHandler<MenuItemEventArgs>? MenuItemClicking;

		/// <summary>
		/// Occurs when a menu item has been clicked.
		/// </summary>
		public event EventHandler<MenuItemEventArgs>? MenuItemClicked;

		/// <summary>
		/// Event arguments for menu item events.
		/// </summary>
		public class MenuItemEventArgs : EventArgs
		{
			/// <summary>
			/// The ID of the menu item.
			/// </summary>
			public string ItemId { get; }

			/// <summary>
			/// The menu item that was clicked.
			/// </summary>
			public MenuItem MenuItem { get; }

			/// <summary>
			/// Set to true to cancel the action.
			/// </summary>
			public bool Cancel { get; set; }

			/// <summary>
			/// Creates new menu item event arguments.
			/// </summary>
			public MenuItemEventArgs(string itemId, MenuItem menuItem)
			{
				ItemId = itemId;
				MenuItem = menuItem;
			}
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// Disposes of the menu bar service resources.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_menuItemRegistry.Clear();
			_nativeMenuItems.Clear();
			_nativeMenus.Clear();
			_menuDefinitions.Clear();

			FlutterSharpLogger.LogDebug("MenuBarService: Disposed");
		}

		#endregion
	}
}
