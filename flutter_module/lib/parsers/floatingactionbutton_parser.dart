// Manual parser for FloatingActionButton widget
// Part of FlutterSharp Phase 4 - Button Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design FloatingActionButton widget.
///
/// A floating action button is a circular icon button that hovers over content
/// to promote a primary action in the application. Floating action buttons are
/// most commonly used in the Scaffold.floatingActionButton field.
class FloatingActionButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<FloatingActionButtonStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse callback action ID
    final onPressedAction = map.hasOnPressed == 1
        ? parseString(map.onPressedAction)
        : null;

    // Parse tooltip
    final tooltip = map.hasTooltip == 1
        ? parseString(map.tooltip)
        : null;

    // Parse heroTag
    final heroTag = map.hasHeroTag == 1
        ? parseString(map.heroTag)
        : null;

    // Parse color properties
    Color? foregroundColor;
    if (map.hasForegroundColor == 1) {
      foregroundColor = Color(map.foregroundColor);
    }

    Color? backgroundColor;
    if (map.hasBackgroundColor == 1) {
      backgroundColor = Color(map.backgroundColor);
    }

    Color? focusColor;
    if (map.hasFocusColor == 1) {
      focusColor = Color(map.focusColor);
    }

    Color? hoverColor;
    if (map.hasHoverColor == 1) {
      hoverColor = Color(map.hoverColor);
    }

    Color? splashColor;
    if (map.hasSplashColor == 1) {
      splashColor = Color(map.splashColor);
    }

    // Parse elevation properties
    double? elevation;
    if (map.hasElevation == 1) {
      elevation = map.elevation;
    }

    double? focusElevation;
    if (map.hasFocusElevation == 1) {
      focusElevation = map.focusElevation;
    }

    double? hoverElevation;
    if (map.hasHoverElevation == 1) {
      hoverElevation = map.hoverElevation;
    }

    double? highlightElevation;
    if (map.hasHighlightElevation == 1) {
      highlightElevation = map.highlightElevation;
    }

    double? disabledElevation;
    if (map.hasDisabledElevation == 1) {
      disabledElevation = map.disabledElevation;
    }

    // Parse boolean properties
    final mini = map.mini == 1;
    final autofocus = map.autofocus == 1;
    final isExtended = map.isExtended == 1;

    bool? enableFeedback;
    if (map.hasEnableFeedback == 1) {
      enableFeedback = map.enableFeedback == 1;
    }

    // Parse clipBehavior enum
    Clip clipBehavior = Clip.none;
    if (map.clipBehavior >= 0 && map.clipBehavior <= 3) {
      clipBehavior = Clip.values[map.clipBehavior];
    }

    // Parse child widget
    final child = map.child.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.child.cast<WidgetStruct>(), buildContext)
        : null;

    return FloatingActionButton(
      onPressed: onPressedAction != null
          ? () => invokeAction(onPressedAction)
          : null,
      tooltip: tooltip,
      foregroundColor: foregroundColor,
      backgroundColor: backgroundColor,
      focusColor: focusColor,
      hoverColor: hoverColor,
      splashColor: splashColor,
      heroTag: heroTag,
      elevation: elevation,
      focusElevation: focusElevation,
      hoverElevation: hoverElevation,
      highlightElevation: highlightElevation,
      disabledElevation: disabledElevation,
      mini: mini,
      clipBehavior: clipBehavior,
      autofocus: autofocus,
      isExtended: isExtended,
      enableFeedback: enableFeedback,
      child: child,
    );
  }

  @override
  String get widgetName => "FloatingActionButton";
}

/// Invoke a void callback action via the method channel
void invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}
