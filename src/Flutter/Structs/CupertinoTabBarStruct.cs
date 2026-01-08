// FlutterSharp Manual Implementation
// CupertinoTabBar FFI Struct

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Widgets;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct representation of the iOS-style CupertinoTabBar widget.
    ///
    /// An iOS-styled bottom navigation tab bar.
    ///
    /// Displays multiple items using BottomNavigationBarItem with one item being
    /// currently selected. The selected item will be highlighted with the active color.
    ///
    /// This widget is typically used with CupertinoTabScaffold to provide a persistent
    /// navigation bar at the bottom of the screen.
    ///
    /// See also:
    ///  * CupertinoTabScaffold, which hosts the CupertinoTabBar at the bottom.
    ///  * BottomNavigationBarItem, for the individual items.
    ///  * https://developer.apple.com/design/human-interface-guidelines/tab-bars
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CupertinoTabBarStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to array of BottomNavigationBarItem pointers.
        /// The interactive items laid out within the tab bar.
        /// Must have at least two items.
        /// </summary>
        public IntPtr items { get; set; }

        /// <summary>
        /// The number of items in the items array.
        /// </summary>
        public int itemCount { get; set; }

        /// <summary>
        /// The index into items for the current active item.
        /// Defaults to 0.
        /// </summary>
        public int currentIndex { get; set; }

        // Has flag for nullable callback: onTap
        public byte HasonTap { get; set; }

        // Callback field: onTap
        IntPtr _onTap;

        /// <summary>
        /// Action identifier for onTap callback.
        /// Called when one of the items is tapped with the index of the tapped item.
        /// </summary>
        public string? onTapAction
        {
            get => GetString(_onTap);
            set { SetString(ref _onTap, value); HasonTap = (byte)(value != null ? 1 : 0); }
        }

        // Has flag for nullable property: backgroundColor
        public byte HasbackgroundColor { get; set; }

        /// <summary>
        /// The background color of the tab bar (ARGB uint).
        /// If it contains transparency, the tab bar will automatically produce a
        /// blurring effect to the content behind it.
        /// </summary>
        public uint backgroundColor { get; set; }

        // Has flag for nullable property: activeColor
        public byte HasactiveColor { get; set; }

        /// <summary>
        /// The foreground color of the icon and title for the selected item (ARGB uint).
        /// Defaults to CupertinoColors.activeBlue.
        /// </summary>
        public uint activeColor { get; set; }

        // Has flag for nullable property: inactiveColor
        public byte HasinactiveColor { get; set; }

        /// <summary>
        /// The foreground color of the icon and title for unselected items (ARGB uint).
        /// Defaults to CupertinoColors.inactiveGray.
        /// </summary>
        public uint inactiveColor { get; set; }

        // Has flag for nullable property: iconSize
        public byte HasiconSize { get; set; }

        /// <summary>
        /// The size of all the BottomNavigationBarItem icons.
        /// Defaults to 30.0.
        /// </summary>
        public double iconSize { get; set; }

        // Has flag for nullable property: height
        public byte Hasheight { get; set; }

        /// <summary>
        /// The height of the CupertinoTabBar.
        /// Defaults to 50.0.
        /// </summary>
        public double height { get; set; }

        // Has flag for nullable property: border
        public byte Hasborder { get; set; }

        /// <summary>
        /// The border of the CupertinoTabBar.
        /// Top width.
        /// </summary>
        public double borderTopWidth { get; set; }

        /// <summary>
        /// Border top color (ARGB uint).
        /// </summary>
        public uint borderTopColor { get; set; }
    }
}
