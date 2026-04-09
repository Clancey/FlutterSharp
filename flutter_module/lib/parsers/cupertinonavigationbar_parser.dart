// Manual parser for CupertinoNavigationBar widget
// Part of FlutterSharp Phase 5 - Cupertino Widgets

import 'dart:ffi' hide Size;

import 'package:flutter/cupertino.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/cupertinonavigationbar_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for iOS-style CupertinoNavigationBar widget.
///
/// An iOS-style navigation bar.
///
/// The navigation bar is a toolbar that minimally consists of a widget, normally
/// a page title, in the middle of the toolbar.
///
/// It also supports a leading and trailing widget before and after the middle widget
/// while keeping the middle widget centered.
///
/// The leading widget will automatically be a back chevron icon button (or a close
/// button in case of a fullscreen dialog) to navigate back to the previous route if
/// none is provided and automaticallyImplyLeading is set to true.
class CupertinoNavigationBarParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<CupertinoNavigationBarStruct>.fromAddress(fos.handle.address)
            .ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse leading widget
    Widget? leading;
    if (map.hasLeading == 1 && map.leading.address != 0) {
      leading = DynamicWidgetBuilder.buildFromPointer(
          map.leading.cast<WidgetStruct>(), buildContext);
    }

    // Parse middle widget
    Widget? middle;
    if (map.hasMiddle == 1 && map.middle.address != 0) {
      middle = DynamicWidgetBuilder.buildFromPointer(
          map.middle.cast<WidgetStruct>(), buildContext);
    }

    // Parse trailing widget
    Widget? trailing;
    if (map.hasTrailing == 1 && map.trailing.address != 0) {
      trailing = DynamicWidgetBuilder.buildFromPointer(
          map.trailing.cast<WidgetStruct>(), buildContext);
    }

    // Parse previousPageTitle
    String? previousPageTitle;
    if (map.hasPreviousPageTitle == 1) {
      previousPageTitle = parseString(map.previousPageTitle);
    }

    // Parse background color
    Color? backgroundColor;
    if (map.hasBackgroundColor == 1) {
      backgroundColor = Color(map.backgroundColor);
    }

    // Parse brightness
    Brightness? brightness;
    if (map.hasBrightness == 1) {
      brightness = map.brightness == 0 ? Brightness.dark : Brightness.light;
    }

    // Parse padding (EdgeInsetsDirectional)
    EdgeInsetsDirectional? padding;
    if (map.hasPadding == 1) {
      padding = EdgeInsetsDirectional.fromSTEB(
        map.paddingStart,
        map.paddingTop,
        map.paddingEnd,
        map.paddingBottom,
      );
    }

    // Parse boolean properties
    final automaticallyImplyLeading = map.automaticallyImplyLeading == 1;
    final automaticallyImplyMiddle = map.automaticallyImplyMiddle == 1;
    final transitionBetweenRoutes = map.transitionBetweenRoutes == 1;
    final automaticBackgroundVisibility =
        map.automaticBackgroundVisibility == 1;
    final enableBackgroundFilterBlur = map.enableBackgroundFilterBlur == 1;

    // Create and return the CupertinoNavigationBar widget
    return CupertinoNavigationBar(
      leading: leading,
      automaticallyImplyLeading: automaticallyImplyLeading,
      automaticallyImplyMiddle: automaticallyImplyMiddle,
      previousPageTitle: previousPageTitle,
      middle: middle,
      trailing: trailing,
      backgroundColor: backgroundColor,
      brightness: brightness,
      padding: padding,
      transitionBetweenRoutes: transitionBetweenRoutes,
      automaticBackgroundVisibility: automaticBackgroundVisibility,
      enableBackgroundFilterBlur: enableBackgroundFilterBlur,
    );
  }

  @override
  String get widgetName => "CupertinoNavigationBar";
}
