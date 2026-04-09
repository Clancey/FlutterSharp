// FlutterSharp Manual Implementation
// CupertinoTabBar parser

import 'dart:ffi';

import 'package:flutter/cupertino.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/cupertinotabbar_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for iOS-style CupertinoTabBar widget.
///
/// An iOS-styled bottom navigation tab bar.
///
/// Displays multiple items using BottomNavigationBarItem with one item being
/// currently selected. The selected item will be highlighted with the active color.
///
/// This widget is typically used with CupertinoTabScaffold to provide a persistent
/// navigation bar at the bottom of the screen.
class CupertinoTabBarParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<CupertinoTabBarStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse items
    final List<BottomNavigationBarItem> items = [];
    if (map.items.address != 0) {
      final children = map.items.cast<ChildrenStruct>().ref;
      for (int i = 0; i < children.childrenLength; i++) {
        final itemAddress = children.children.elementAt(i).value;
        if (itemAddress != 0) {
          final itemPtr = Pointer<BottomNavigationBarItemStruct>.fromAddress(
              itemAddress.toInt());
          final item = _parseBottomNavigationBarItem(itemPtr.ref, buildContext);
          if (item != null) {
            items.add(item);
          }
        }
      }
    }

    // Must have at least 2 items
    if (items.length < 2) {
      return null;
    }

    // Parse currentIndex
    final currentIndex = map.currentIndex;

    // Parse callback action ID
    final onTapAction =
        map.hasOnTapAction == 1 ? parseString(map.onTapAction) : null;

    // Parse backgroundColor
    Color? backgroundColor;
    if (map.hasBackgroundColor == 1) {
      backgroundColor = Color(map.backgroundColor);
    }

    // Parse activeColor
    Color? activeColor;
    if (map.hasActiveColor == 1) {
      activeColor = Color(map.activeColor);
    }

    // Parse inactiveColor
    final Color inactiveColor = Color(map.inactiveColor);

    // Parse iconSize
    final double iconSize = map.iconSize;

    // Parse height (note: CupertinoTabBar doesn't have a direct height param,
    // but we can wrap in a SizedBox if needed or use the standard height)
    // We'll use height for the standard CupertinoTabBar behavior

    // Parse border
    Border? border;

    return CupertinoTabBar(
      items: items,
      currentIndex: currentIndex.clamp(0, items.length - 1),
      onTap: onTapAction != null
          ? (int index) => _invokeActionWithIndex(onTapAction, index)
          : null,
      backgroundColor: backgroundColor,
      activeColor: activeColor,
      inactiveColor: inactiveColor,
      iconSize: iconSize,
      border: border,
    );
  }

  /// Parse a BottomNavigationBarItem from its FFI struct
  BottomNavigationBarItem? _parseBottomNavigationBarItem(
      BottomNavigationBarItemStruct itemStruct, BuildContext buildContext) {
    // Parse icon (required)
    Widget? icon;
    if (itemStruct.icon.address != 0) {
      icon =
          DynamicWidgetBuilder.buildFromPointer(itemStruct.icon, buildContext);
    }
    if (icon == null) {
      return null; // Icon is required
    }

    // Parse activeIcon
    Widget? activeIcon;
    if (itemStruct.activeIcon.address != 0) {
      activeIcon = DynamicWidgetBuilder.buildFromPointer(
          itemStruct.activeIcon, buildContext);
    }

    // Parse label
    String? label;
    if (itemStruct.hasLabel == 1) {
      label = parseString(itemStruct.label);
    }

    // Parse tooltip
    String? tooltip;
    if (itemStruct.hasTooltip == 1) {
      tooltip = parseString(itemStruct.tooltip);
    }

    // Parse backgroundColor
    Color? backgroundColor;
    if (itemStruct.hasBackgroundColor == 1) {
      backgroundColor = Color(itemStruct.backgroundColor);
    }

    return BottomNavigationBarItem(
      icon: icon,
      activeIcon: activeIcon,
      label: label ?? '',
      tooltip: tooltip,
      backgroundColor: backgroundColor,
    );
  }

  @override
  String get widgetName => "CupertinoTabBar";
}

/// Invoke a callback action with an index argument via the method channel
void _invokeActionWithIndex(String actionId, int index) {
  raiseMauiEvent(actionId, "invoke", {'index': index});
}
