// FlutterSharp Manual Implementation
// CupertinoTabBar iOS-style tab bar widget

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Gestures;
using Flutter.UI;
using Flutter.Structs;
using Flutter.Widgets;
using Flutter.Material;
using Flutter.Cupertino;

namespace Flutter.Widgets
{
    /// <summary>
    /// An iOS-styled bottom navigation tab bar.
    ///
    /// Displays multiple items using BottomNavigationBarItem with one item being
    /// currently selected. The selected item will be highlighted with the active color.
    ///
    /// This widget is typically used with CupertinoTabScaffold to provide a persistent
    /// navigation bar at the bottom of the screen.
    ///
    /// When the background color contains transparency, the tab bar will automatically
    /// produce a blurring effect to the content behind it, mimicking native iOS behavior.
    ///
    /// See also:
    ///  * CupertinoTabScaffold, which hosts the CupertinoTabBar at the bottom.
    ///  * BottomNavigationBarItem, for the individual items.
    ///  * BottomNavigationBar, the Material Design equivalent.
    ///  * https://developer.apple.com/design/human-interface-guidelines/tab-bars
    /// </summary>
    public class CupertinoTabBar : StatefulWidget
    {
        private List<BottomNavigationBarItem> _items = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CupertinoTabBar"/> class.
        /// </summary>
        /// <param name="items">The interactive items laid out within the tab bar. Must have at least two items.</param>
        /// <param name="currentIndex">The index into items for the current active item. Defaults to 0.</param>
        /// <param name="onTap">Called when one of the items is tapped with the index of the tapped item.</param>
        /// <param name="backgroundColor">The background color of the tab bar. If it contains transparency, a blur effect is applied.</param>
        /// <param name="activeColor">The foreground color of the icon and title for the selected item.</param>
        /// <param name="inactiveColor">The foreground color of the icon and title for unselected items.</param>
        /// <param name="iconSize">The size of all the BottomNavigationBarItem icons. Defaults to 30.0.</param>
        /// <param name="height">The height of the CupertinoTabBar. Defaults to 50.0.</param>
        /// <param name="borderTopWidth">The width of the top border. Defaults to 0.0 (no border).</param>
        /// <param name="borderTopColor">The color of the top border (ARGB format).</param>
        public CupertinoTabBar(
            List<BottomNavigationBarItem> items,
            int currentIndex = 0,
            Action<int>? onTap = null,
            uint? backgroundColor = null,
            uint? activeColor = null,
            uint? inactiveColor = null,
            double? iconSize = null,
            double? height = null,
            double? borderTopWidth = null,
            uint? borderTopColor = null
        )
        {
            _items = items ?? new List<BottomNavigationBarItem>();

            var s = GetBackingStruct<CupertinoTabBarStruct>();

            // Assign currentIndex
            s.currentIndex = currentIndex;

            // Register onTap callback and assign action ID to struct
            s.onTapAction = RegisterCallback(onTap);

            // Assign backgroundColor
            if (backgroundColor.HasValue)
            {
                s.HasbackgroundColor = 1;
                s.backgroundColor = backgroundColor.Value;
            }

            // Assign activeColor
            if (activeColor.HasValue)
            {
                s.HasactiveColor = 1;
                s.activeColor = activeColor.Value;
            }

            // Assign inactiveColor
            if (inactiveColor.HasValue)
            {
                s.HasinactiveColor = 1;
                s.inactiveColor = inactiveColor.Value;
            }

            // Assign iconSize
            if (iconSize.HasValue)
            {
                s.HasiconSize = 1;
                s.iconSize = iconSize.Value;
            }

            // Assign height
            if (height.HasValue)
            {
                s.Hasheight = 1;
                s.height = height.Value;
            }

            // Assign border
            if (borderTopWidth.HasValue && borderTopWidth.Value > 0)
            {
                s.Hasborder = 1;
                s.borderTopWidth = borderTopWidth.Value;
                s.borderTopColor = borderTopColor ?? 0x33000000; // Default: semi-transparent black
            }
        }

        /// <summary>
        /// Prepares this widget for sending to Dart by serializing items array.
        /// </summary>
        internal new void PrepareForSending()
        {
            var s = GetBackingStruct<CupertinoTabBarStruct>();

            // Prepare and serialize items array
            if (_items.Count > 0)
            {
                var itemPointers = new IntPtr[_items.Count];
                for (int i = 0; i < _items.Count; i++)
                {
                    // Get the widget handle for each item - this prepares it for sending
                    itemPointers[i] = GetWidgetHandle(_items[i]);
                }

                // Allocate unmanaged memory for items array
                int size = IntPtr.Size * _items.Count;
                s.items = Marshal.AllocHGlobal(size);
                Marshal.Copy(itemPointers, 0, s.items, _items.Count);
                s.itemCount = _items.Count;
            }

            base.PrepareForSending();
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new CupertinoTabBarStruct();
    }
}
