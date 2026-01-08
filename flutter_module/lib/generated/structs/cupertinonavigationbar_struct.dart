// FlutterSharp Manual Implementation
// CupertinoNavigationBar FFI Struct

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../../flutter_sharp_structs.dart';

/// FFI struct representation of CupertinoNavigationBar widget.
/// This struct is used to pass widget data across the FFI boundary.
///
/// An iOS-style navigation bar.
///
/// The navigation bar is a toolbar that minimally consists of a widget, normally
/// a page title, in the middle of the toolbar.
///
/// It also supports a leading and trailing widget before and after the middle widget
/// while keeping the middle widget centered.
///
/// See also:
///  * https://developer.apple.com/design/human-interface-guidelines/navigation-bars
final class CupertinoNavigationBarStruct extends Struct {
  // FlutterObject Struct base fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct base field
  external Pointer<Utf8> id;

  // Has flag for nullable widget: leading
  @Int8()
  external int hasLeading;

  /// Widget to place at the start of the navigation bar.
  external Pointer<WidgetStruct> leading;

  // Has flag for nullable widget: middle
  @Int8()
  external int hasMiddle;

  /// Widget to place in the middle of the navigation bar.
  external Pointer<WidgetStruct> middle;

  // Has flag for nullable widget: trailing
  @Int8()
  external int hasTrailing;

  /// Widget to place at the end of the navigation bar.
  external Pointer<WidgetStruct> trailing;

  // Has flag for nullable string: previousPageTitle
  @Int8()
  external int hasPreviousPageTitle;

  /// Manually specify the previous route's title.
  external Pointer<Utf8> previousPageTitle;

  // Background color of the navigation bar
  @Int8()
  external int hasBackgroundColor;
  @Uint32()
  external int backgroundColor;

  // Brightness of the navigation bar
  @Int8()
  external int hasBrightness;
  @Int32()
  external int brightness;

  // Padding for the contents (EdgeInsetsDirectional: start, top, end, bottom)
  @Int8()
  external int hasPadding;
  @Double()
  external double paddingStart;
  @Double()
  external double paddingTop;
  @Double()
  external double paddingEnd;
  @Double()
  external double paddingBottom;

  // Boolean properties (stored as Int8 for FFI)
  @Int8()
  external int automaticallyImplyLeading;

  @Int8()
  external int automaticallyImplyMiddle;

  @Int8()
  external int transitionBetweenRoutes;

  @Int8()
  external int automaticBackgroundVisibility;

  @Int8()
  external int enableBackgroundFilterBlur;
}
