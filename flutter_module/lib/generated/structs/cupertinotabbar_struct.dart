// FlutterSharp Manual Implementation
// CupertinoTabBar FFI Struct

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../../flutter_sharp_structs.dart';
import 'bottomnavigationbaritem_struct.dart';

/// FFI struct representation of the iOS-style CupertinoTabBar widget.
///
/// An iOS-styled bottom navigation tab bar.
///
/// Displays multiple items using BottomNavigationBarItem with one item being
/// currently selected. The selected item will be highlighted with the active color.
///
/// This widget is typically used with CupertinoTabScaffold to provide a persistent
/// navigation bar at the bottom of the screen.
final class CupertinoTabBarStruct extends Struct {
  // FlutterObject Struct base fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct base field
  external Pointer<Utf8> id;

  /// Pointer to array of BottomNavigationBarItem pointers.
  external Pointer<Pointer<BottomNavigationBarItemStruct>> items;

  /// The number of items in the items array.
  @Int32()
  external int itemCount;

  /// The index into items for the current active item.
  @Int32()
  external int currentIndex;

  // Has flag for nullable callback: onTap
  @Int8()
  external int hasOnTap;

  /// Action identifier for onTap callback.
  external Pointer<Utf8> onTapAction;

  // Has flag for nullable property: backgroundColor
  @Int8()
  external int hasBackgroundColor;

  /// The background color of the tab bar (ARGB uint).
  @Uint32()
  external int backgroundColor;

  // Has flag for nullable property: activeColor
  @Int8()
  external int hasActiveColor;

  /// The foreground color of the icon and title for the selected item (ARGB uint).
  @Uint32()
  external int activeColor;

  // Has flag for nullable property: inactiveColor
  @Int8()
  external int hasInactiveColor;

  /// The foreground color of the icon and title for unselected items (ARGB uint).
  @Uint32()
  external int inactiveColor;

  // Has flag for nullable property: iconSize
  @Int8()
  external int hasIconSize;

  /// The size of all the BottomNavigationBarItem icons.
  @Double()
  external double iconSize;

  // Has flag for nullable property: height
  @Int8()
  external int hasHeight;

  /// The height of the CupertinoTabBar.
  @Double()
  external double height;

  // Has flag for nullable property: border
  @Int8()
  external int hasBorder;

  /// The border top width.
  @Double()
  external double borderTopWidth;

  /// Border top color (ARGB uint).
  @Uint32()
  external int borderTopColor;
}
