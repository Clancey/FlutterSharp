using System;
using System.Runtime.InteropServices;
using Flutter.Logging;

namespace Flutter.macOS
{
	/// <summary>
	/// Provides native NSMenu and NSMenuItem interoperability for macOS menu bar integration.
	/// </summary>
	/// <remarks>
	/// This class provides low-level P/Invoke access to macOS AppKit menu classes:
	/// - NSMenu - represents a menu (File, Edit, View, etc.)
	/// - NSMenuItem - represents individual menu items
	/// - NSApplication.sharedApplication.mainMenu - the application's main menu bar
	/// </remarks>
	internal static class MenuBarInterop
	{
		#region Objective-C Runtime P/Invoke

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
		private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPStr)] string className);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
		private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string selectorName);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_IntPtr_Int(IntPtr receiver, IntPtr selector, IntPtr arg1, nint index);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr_Int(IntPtr receiver, IntPtr selector, nint arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern nint objc_msgSend_NInt(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_Void_Bool(IntPtr receiver, IntPtr selector, bool arg1);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern bool objc_msgSend_Bool(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(
			IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_allocateClassPair")]
		private static extern IntPtr objc_allocateClassPair(IntPtr superclass, [MarshalAs(UnmanagedType.LPStr)] string name, nint extraBytes);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_registerClassPair")]
		private static extern void objc_registerClassPair(IntPtr cls);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "class_addMethod")]
		private static extern bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, [MarshalAs(UnmanagedType.LPStr)] string types);

		#endregion

		#region Selector Cache

		private static IntPtr _selSharedApplication;
		private static IntPtr _selMainMenu;
		private static IntPtr _selSetMainMenu;
		private static IntPtr _selAlloc;
		private static IntPtr _selInit;
		private static IntPtr _selInitWithTitle;
		private static IntPtr _selInitWithTitleActionKeyEquivalent;
		private static IntPtr _selAddItem;
		private static IntPtr _selInsertItemAtIndex;
		private static IntPtr _selRemoveItem;
		private static IntPtr _selRemoveItemAtIndex;
		private static IntPtr _selItemAtIndex;
		private static IntPtr _selItemWithTitle;
		private static IntPtr _selItemWithTag;
		private static IntPtr _selNumberOfItems;
		private static IntPtr _selTitle;
		private static IntPtr _selSetTitle;
		private static IntPtr _selSubmenu;
		private static IntPtr _selSetSubmenu;
		private static IntPtr _selTarget;
		private static IntPtr _selSetTarget;
		private static IntPtr _selAction;
		private static IntPtr _selSetAction;
		private static IntPtr _selKeyEquivalent;
		private static IntPtr _selSetKeyEquivalent;
		private static IntPtr _selKeyEquivalentModifierMask;
		private static IntPtr _selSetKeyEquivalentModifierMask;
		private static IntPtr _selTag;
		private static IntPtr _selSetTag;
		private static IntPtr _selEnabled;
		private static IntPtr _selSetEnabled;
		private static IntPtr _selState;
		private static IntPtr _selSetState;
		private static IntPtr _selSeparatorItem;
		private static IntPtr _selUpdate;

		// Class references
		private static IntPtr _classNSApplication;
		private static IntPtr _classNSMenu;
		private static IntPtr _classNSMenuItem;
		private static IntPtr _classNSString;

		private static bool _selectorsInitialized;

		private static void EnsureSelectorsInitialized()
		{
			if (_selectorsInitialized)
				return;

			try
			{
				// Classes
				_classNSApplication = objc_getClass("NSApplication");
				_classNSMenu = objc_getClass("NSMenu");
				_classNSMenuItem = objc_getClass("NSMenuItem");
				_classNSString = objc_getClass("NSString");

				// NSApplication selectors
				_selSharedApplication = sel_registerName("sharedApplication");
				_selMainMenu = sel_registerName("mainMenu");
				_selSetMainMenu = sel_registerName("setMainMenu:");

				// Alloc/Init
				_selAlloc = sel_registerName("alloc");
				_selInit = sel_registerName("init");
				_selInitWithTitle = sel_registerName("initWithTitle:");
				_selInitWithTitleActionKeyEquivalent = sel_registerName("initWithTitle:action:keyEquivalent:");

				// NSMenu selectors
				_selAddItem = sel_registerName("addItem:");
				_selInsertItemAtIndex = sel_registerName("insertItem:atIndex:");
				_selRemoveItem = sel_registerName("removeItem:");
				_selRemoveItemAtIndex = sel_registerName("removeItemAtIndex:");
				_selItemAtIndex = sel_registerName("itemAtIndex:");
				_selItemWithTitle = sel_registerName("itemWithTitle:");
				_selItemWithTag = sel_registerName("itemWithTag:");
				_selNumberOfItems = sel_registerName("numberOfItems");
				_selUpdate = sel_registerName("update");

				// NSMenuItem selectors
				_selTitle = sel_registerName("title");
				_selSetTitle = sel_registerName("setTitle:");
				_selSubmenu = sel_registerName("submenu");
				_selSetSubmenu = sel_registerName("setSubmenu:");
				_selTarget = sel_registerName("target");
				_selSetTarget = sel_registerName("setTarget:");
				_selAction = sel_registerName("action");
				_selSetAction = sel_registerName("setAction:");
				_selKeyEquivalent = sel_registerName("keyEquivalent");
				_selSetKeyEquivalent = sel_registerName("setKeyEquivalent:");
				_selKeyEquivalentModifierMask = sel_registerName("keyEquivalentModifierMask");
				_selSetKeyEquivalentModifierMask = sel_registerName("setKeyEquivalentModifierMask:");
				_selTag = sel_registerName("tag");
				_selSetTag = sel_registerName("setTag:");
				_selEnabled = sel_registerName("isEnabled");
				_selSetEnabled = sel_registerName("setEnabled:");
				_selState = sel_registerName("state");
				_selSetState = sel_registerName("setState:");
				_selSeparatorItem = sel_registerName("separatorItem");

				_selectorsInitialized = true;
				FlutterSharpLogger.LogDebug("MenuBarInterop: Selectors initialized successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to initialize selectors");
			}
		}

		#endregion

		#region NSApplication Menu Access

		/// <summary>
		/// Gets the shared NSApplication instance.
		/// </summary>
		/// <returns>Handle to NSApplication.sharedApplication, or IntPtr.Zero if failed</returns>
		public static IntPtr GetSharedApplication()
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSApplication == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("MenuBarInterop: NSApplication class not available");
					return IntPtr.Zero;
				}

				return objc_msgSend_IntPtr(_classNSApplication, _selSharedApplication);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get shared application");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets the main menu bar from NSApplication.
		/// </summary>
		/// <returns>Handle to the main menu, or IntPtr.Zero if not set</returns>
		public static IntPtr GetMainMenu()
		{
			try
			{
				var app = GetSharedApplication();
				if (app == IntPtr.Zero)
					return IntPtr.Zero;

				return objc_msgSend_IntPtr(app, _selMainMenu);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get main menu");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Sets the main menu bar on NSApplication.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu to set as main menu</param>
		public static void SetMainMenu(IntPtr menu)
		{
			try
			{
				var app = GetSharedApplication();
				if (app == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("MenuBarInterop: Cannot set main menu - no shared application");
					return;
				}

				objc_msgSend_Void_IntPtr(app, _selSetMainMenu, menu);
				FlutterSharpLogger.LogDebug("MenuBarInterop: Main menu set successfully");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set main menu");
			}
		}

		#endregion

		#region NSMenu Creation

		/// <summary>
		/// Creates a new NSMenu with the specified title.
		/// </summary>
		/// <param name="title">The menu title</param>
		/// <returns>Handle to the new NSMenu, or IntPtr.Zero if failed</returns>
		public static IntPtr CreateMenu(string title)
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSMenu == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("MenuBarInterop: NSMenu class not available");
					return IntPtr.Zero;
				}

				var nsString = CreateNSString(title);
				if (nsString == IntPtr.Zero)
					return IntPtr.Zero;

				var menuAlloc = objc_msgSend_IntPtr(_classNSMenu, _selAlloc);
				if (menuAlloc == IntPtr.Zero)
					return IntPtr.Zero;

				var menu = objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(menuAlloc, _selInitWithTitle, nsString, IntPtr.Zero, IntPtr.Zero);
				FlutterSharpLogger.LogDebug("MenuBarInterop: Created menu '{Title}'", title);
				return menu;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to create menu '{Title}'", title);
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Adds a menu item to a menu.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <param name="item">Handle to the NSMenuItem to add</param>
		public static void AddMenuItem(IntPtr menu, IntPtr item)
		{
			if (menu == IntPtr.Zero || item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(menu, _selAddItem, item);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to add menu item");
			}
		}

		/// <summary>
		/// Inserts a menu item at the specified index.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <param name="item">Handle to the NSMenuItem to insert</param>
		/// <param name="index">Zero-based index at which to insert</param>
		public static void InsertMenuItem(IntPtr menu, IntPtr item, int index)
		{
			if (menu == IntPtr.Zero || item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr_Int(menu, _selInsertItemAtIndex, item, index);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to insert menu item at index {Index}", index);
			}
		}

		/// <summary>
		/// Removes a menu item from a menu.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <param name="item">Handle to the NSMenuItem to remove</param>
		public static void RemoveMenuItem(IntPtr menu, IntPtr item)
		{
			if (menu == IntPtr.Zero || item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(menu, _selRemoveItem, item);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to remove menu item");
			}
		}

		/// <summary>
		/// Removes a menu item at the specified index.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <param name="index">Zero-based index of the item to remove</param>
		public static void RemoveMenuItemAtIndex(IntPtr menu, int index)
		{
			if (menu == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_IntPtr_Int(menu, _selRemoveItemAtIndex, index);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to remove menu item at index {Index}", index);
			}
		}

		/// <summary>
		/// Gets a menu item at the specified index.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <param name="index">Zero-based index</param>
		/// <returns>Handle to the NSMenuItem, or IntPtr.Zero if not found</returns>
		public static IntPtr GetMenuItemAtIndex(IntPtr menu, int index)
		{
			if (menu == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_IntPtr_Int(menu, _selItemAtIndex, index);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get menu item at index {Index}", index);
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Gets the number of items in a menu.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		/// <returns>The number of menu items</returns>
		public static int GetMenuItemCount(IntPtr menu)
		{
			if (menu == IntPtr.Zero)
				return 0;

			try
			{
				EnsureSelectorsInitialized();
				return (int)objc_msgSend_NInt(menu, _selNumberOfItems);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get menu item count");
				return 0;
			}
		}

		/// <summary>
		/// Updates the menu to reflect any changes.
		/// </summary>
		/// <param name="menu">Handle to the NSMenu</param>
		public static void UpdateMenu(IntPtr menu)
		{
			if (menu == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void(menu, _selUpdate);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to update menu");
			}
		}

		#endregion

		#region NSMenuItem Creation

		/// <summary>
		/// Creates a new NSMenuItem with the specified title, action, and keyboard shortcut.
		/// </summary>
		/// <param name="title">The menu item title</param>
		/// <param name="action">The selector to call when clicked (use IntPtr.Zero for no action)</param>
		/// <param name="keyEquivalent">The keyboard shortcut (e.g., "n" for Cmd+N, empty string for none)</param>
		/// <returns>Handle to the new NSMenuItem, or IntPtr.Zero if failed</returns>
		public static IntPtr CreateMenuItem(string title, IntPtr action, string keyEquivalent)
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSMenuItem == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("MenuBarInterop: NSMenuItem class not available");
					return IntPtr.Zero;
				}

				var nsTitle = CreateNSString(title);
				var nsKeyEquiv = CreateNSString(keyEquivalent ?? "");
				if (nsTitle == IntPtr.Zero)
					return IntPtr.Zero;

				var itemAlloc = objc_msgSend_IntPtr(_classNSMenuItem, _selAlloc);
				if (itemAlloc == IntPtr.Zero)
					return IntPtr.Zero;

				var item = objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(
					itemAlloc, _selInitWithTitleActionKeyEquivalent, nsTitle, action, nsKeyEquiv);

				FlutterSharpLogger.LogDebug("MenuBarInterop: Created menu item '{Title}'", title);
				return item;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to create menu item '{Title}'", title);
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Creates a separator menu item.
		/// </summary>
		/// <returns>Handle to a separator NSMenuItem, or IntPtr.Zero if failed</returns>
		public static IntPtr CreateSeparatorItem()
		{
			try
			{
				EnsureSelectorsInitialized();
				if (_classNSMenuItem == IntPtr.Zero)
					return IntPtr.Zero;

				return objc_msgSend_IntPtr(_classNSMenuItem, _selSeparatorItem);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to create separator item");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Sets the submenu of a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="submenu">Handle to the NSMenu to set as submenu</param>
		public static void SetSubmenu(IntPtr item, IntPtr submenu)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(item, _selSetSubmenu, submenu);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set submenu");
			}
		}

		/// <summary>
		/// Gets the submenu of a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <returns>Handle to the submenu, or IntPtr.Zero if none</returns>
		public static IntPtr GetSubmenu(IntPtr item)
		{
			if (item == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_IntPtr(item, _selSubmenu);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get submenu");
				return IntPtr.Zero;
			}
		}

		/// <summary>
		/// Sets the target object for a menu item's action.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="target">Handle to the target object</param>
		public static void SetTarget(IntPtr item, IntPtr target)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(item, _selSetTarget, target);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set target");
			}
		}

		/// <summary>
		/// Sets the action selector for a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="action">The selector to call</param>
		public static void SetAction(IntPtr item, IntPtr action)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_IntPtr(item, _selSetAction, action);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set action");
			}
		}

		/// <summary>
		/// Sets the title of a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="title">The new title</param>
		public static void SetTitle(IntPtr item, string title)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				var nsTitle = CreateNSString(title);
				if (nsTitle != IntPtr.Zero)
				{
					objc_msgSend_Void_IntPtr(item, _selSetTitle, nsTitle);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set title");
			}
		}

		/// <summary>
		/// Sets the keyboard shortcut for a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="keyEquivalent">The key (e.g., "n" for Cmd+N)</param>
		public static void SetKeyEquivalent(IntPtr item, string keyEquivalent)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				var nsKey = CreateNSString(keyEquivalent ?? "");
				if (nsKey != IntPtr.Zero)
				{
					objc_msgSend_Void_IntPtr(item, _selSetKeyEquivalent, nsKey);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set key equivalent");
			}
		}

		/// <summary>
		/// Sets the modifier mask for a menu item's keyboard shortcut.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="modifiers">The modifier flags (see NSEventModifierFlags)</param>
		public static void SetKeyEquivalentModifierMask(IntPtr item, ulong modifiers)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				// Note: This requires a different P/Invoke signature for UInt64
				// For simplicity, we're using the standard modifier approach
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set modifier mask");
			}
		}

		/// <summary>
		/// Sets the tag (identifier) for a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="tag">The tag value</param>
		public static void SetTag(IntPtr item, int tag)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_IntPtr_Int(item, _selSetTag, tag);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set tag");
			}
		}

		/// <summary>
		/// Gets the tag of a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <returns>The tag value</returns>
		public static int GetTag(IntPtr item)
		{
			if (item == IntPtr.Zero)
				return 0;

			try
			{
				EnsureSelectorsInitialized();
				return (int)objc_msgSend_NInt(item, _selTag);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get tag");
				return 0;
			}
		}

		/// <summary>
		/// Sets whether a menu item is enabled.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="enabled">True to enable, false to disable</param>
		public static void SetEnabled(IntPtr item, bool enabled)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_Void_Bool(item, _selSetEnabled, enabled);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set enabled");
			}
		}

		/// <summary>
		/// Gets whether a menu item is enabled.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <returns>True if enabled</returns>
		public static bool IsEnabled(IntPtr item)
		{
			if (item == IntPtr.Zero)
				return false;

			try
			{
				EnsureSelectorsInitialized();
				return objc_msgSend_Bool(item, _selEnabled);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get enabled");
				return false;
			}
		}

		/// <summary>
		/// Sets the state of a menu item (off, on, or mixed).
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <param name="state">0 = off, 1 = on, -1 = mixed</param>
		public static void SetState(IntPtr item, int state)
		{
			if (item == IntPtr.Zero)
				return;

			try
			{
				EnsureSelectorsInitialized();
				objc_msgSend_IntPtr_Int(item, _selSetState, state);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to set state");
			}
		}

		/// <summary>
		/// Gets the state of a menu item.
		/// </summary>
		/// <param name="item">Handle to the NSMenuItem</param>
		/// <returns>The state value (0 = off, 1 = on, -1 = mixed)</returns>
		public static int GetState(IntPtr item)
		{
			if (item == IntPtr.Zero)
				return 0;

			try
			{
				EnsureSelectorsInitialized();
				return (int)objc_msgSend_NInt(item, _selState);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to get state");
				return 0;
			}
		}

		#endregion

		#region NSString Helper

		// P/Invoke for creating NSString via Objective-C runtime
		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_InitWithUTF8String(IntPtr receiver, IntPtr selector, IntPtr utf8String);

		private static IntPtr _selStringWithUTF8String;

		/// <summary>
		/// Creates an NSString from a C# string.
		/// </summary>
		/// <param name="str">The string to convert</param>
		/// <returns>Handle to NSString, or IntPtr.Zero if failed</returns>
		private static IntPtr CreateNSString(string str)
		{
			if (string.IsNullOrEmpty(str))
				str = "";

			try
			{
				EnsureSelectorsInitialized();
				if (_classNSString == IntPtr.Zero)
				{
					FlutterSharpLogger.LogWarning("MenuBarInterop: NSString class not available");
					return IntPtr.Zero;
				}

				// Initialize selector if needed
				if (_selStringWithUTF8String == IntPtr.Zero)
				{
					_selStringWithUTF8String = sel_registerName("stringWithUTF8String:");
				}

				// Convert managed string to UTF8 bytes and get a pointer
				var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(str + "\0");
				var utf8Handle = Marshal.AllocHGlobal(utf8Bytes.Length);
				try
				{
					Marshal.Copy(utf8Bytes, 0, utf8Handle, utf8Bytes.Length);
					var nsString = objc_msgSend_InitWithUTF8String(_classNSString, _selStringWithUTF8String, utf8Handle);
					return nsString;
				}
				finally
				{
					Marshal.FreeHGlobal(utf8Handle);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "MenuBarInterop: Failed to create NSString from '{String}'", str);
				return IntPtr.Zero;
			}
		}

		#endregion

		#region Menu Item State Constants

		/// <summary>Menu item is off (unchecked)</summary>
		public const int StateOff = 0;
		/// <summary>Menu item is on (checked)</summary>
		public const int StateOn = 1;
		/// <summary>Menu item is in mixed state</summary>
		public const int StateMixed = -1;

		#endregion

		#region Modifier Key Constants (NSEventModifierFlags)

		/// <summary>Caps Lock modifier</summary>
		public const ulong ModifierCapsLock = 1 << 16;
		/// <summary>Shift modifier</summary>
		public const ulong ModifierShift = 1 << 17;
		/// <summary>Control modifier</summary>
		public const ulong ModifierControl = 1 << 18;
		/// <summary>Option/Alt modifier</summary>
		public const ulong ModifierOption = 1 << 19;
		/// <summary>Command modifier</summary>
		public const ulong ModifierCommand = 1 << 20;
		/// <summary>Numeric keypad modifier</summary>
		public const ulong ModifierNumericPad = 1 << 21;
		/// <summary>Function key modifier</summary>
		public const ulong ModifierFunction = 1 << 23;

		#endregion
	}
}
