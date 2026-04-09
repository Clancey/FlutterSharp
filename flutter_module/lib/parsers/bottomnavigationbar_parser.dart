// Manual parser for BottomNavigationBar widget
// Part of FlutterSharp Phase 4 - Material Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design BottomNavigationBar widget.
///
/// A Material Design bottom navigation bar.
///
/// Bottom navigation bars display three to five destinations at the bottom of a screen.
/// Each destination is represented by an icon and an optional text label. When a bottom
/// navigation icon is tapped, the user is taken to the top-level navigation destination
/// associated with that icon.
class BottomNavigationBarParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<BottomNavigationBarStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse items
    final List<BottomNavigationBarItem> items = [];
    if (map.itemCount > 0 && map.items.address != 0) {
      for (int i = 0; i < map.itemCount; i++) {
        final itemPtr = (map.items + i).value;
        if (itemPtr.address != 0) {
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
    final onTapAction = map.hasOnTap == 1 ? parseString(map.onTapAction) : null;

    // Parse elevation
    double? elevation;
    if (map.hasElevation == 1) {
      elevation = map.elevation;
    }

    // Parse type
    BottomNavigationBarType? type;
    if (map.type >= 0 && map.type < BottomNavigationBarType.values.length) {
      type = BottomNavigationBarType.values[map.type];
    }

    // Parse backgroundColor
    Color? backgroundColor;
    if (map.hasBackgroundColor == 1) {
      backgroundColor = Color(map.backgroundColor);
    }

    // Parse iconSize
    double? iconSize;
    if (map.hasIconSize == 1) {
      iconSize = map.iconSize;
    }

    // Parse selectedItemColor
    Color? selectedItemColor;
    if (map.hasSelectedItemColor == 1) {
      selectedItemColor = Color(map.selectedItemColor);
    }

    // Parse unselectedItemColor
    Color? unselectedItemColor;
    if (map.hasUnselectedItemColor == 1) {
      unselectedItemColor = Color(map.unselectedItemColor);
    }

    // Parse selectedFontSize
    double? selectedFontSize;
    if (map.hasSelectedFontSize == 1) {
      selectedFontSize = map.selectedFontSize;
    }

    // Parse unselectedFontSize
    double? unselectedFontSize;
    if (map.hasUnselectedFontSize == 1) {
      unselectedFontSize = map.unselectedFontSize;
    }

    // Parse showSelectedLabels
    bool? showSelectedLabels;
    if (map.hasShowSelectedLabels == 1) {
      showSelectedLabels = map.showSelectedLabels == 1;
    }

    // Parse showUnselectedLabels
    bool? showUnselectedLabels;
    if (map.hasShowUnselectedLabels == 1) {
      showUnselectedLabels = map.showUnselectedLabels == 1;
    }

    // Parse enableFeedback
    final enableFeedback = map.enableFeedback == 1;

    // Parse landscapeLayout
    BottomNavigationBarLandscapeLayout landscapeLayout =
        BottomNavigationBarLandscapeLayout.spread;
    if (map.landscapeLayout >= 0 &&
        map.landscapeLayout <
            BottomNavigationBarLandscapeLayout.values.length) {
      landscapeLayout =
          BottomNavigationBarLandscapeLayout.values[map.landscapeLayout];
    }

    // Parse useLegacyColorScheme
    final useLegacyColorScheme = map.useLegacyColorScheme == 1;

    return BottomNavigationBar(
      items: items,
      currentIndex: currentIndex.clamp(0, items.length - 1),
      onTap: onTapAction != null
          ? (int index) => invokeActionWithIndex(onTapAction, index)
          : null,
      elevation: elevation,
      type: type,
      backgroundColor: backgroundColor,
      iconSize: iconSize ?? 24.0,
      selectedItemColor: selectedItemColor,
      unselectedItemColor: unselectedItemColor,
      selectedFontSize: selectedFontSize ?? 14.0,
      unselectedFontSize: unselectedFontSize ?? 12.0,
      showSelectedLabels: showSelectedLabels,
      showUnselectedLabels: showUnselectedLabels,
      enableFeedback: enableFeedback,
      landscapeLayout: landscapeLayout,
      useLegacyColorScheme: useLegacyColorScheme,
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
  String get widgetName => "BottomNavigationBar";
}

/// Invoke a callback action with an index argument via the method channel
void invokeActionWithIndex(String actionId, int index) {
  raiseMauiEvent(actionId, "invoke", {'index': index});
}
