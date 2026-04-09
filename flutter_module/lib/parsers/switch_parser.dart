// Manual parser for Switch widget
// Part of FlutterSharp Phase 4 - Input Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/switch_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design Switch widget.
///
/// A Material Design switch that can be toggled on or off.
/// The switch itself does not maintain any state. Instead, when the state of the
/// switch changes, the widget calls the onChanged callback.
class SwitchParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<SwitchStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse value (required bool)
    final value = map.value == 1;

    // Parse callback action ID
    final onChangedAction =
        map.hasOnChanged == 1 ? parseString(map.onChangedAction) : null;

    // Parse colors
    Color? activeColor;
    if (map.hasActiveColor == 1) {
      activeColor = Color(map.activeColor);
    }

    Color? activeTrackColor;
    if (map.hasActiveTrackColor == 1) {
      activeTrackColor = Color(map.activeTrackColor);
    }

    Color? inactiveThumbColor;
    if (map.hasInactiveThumbColor == 1) {
      inactiveThumbColor = Color(map.inactiveThumbColor);
    }

    Color? inactiveTrackColor;
    if (map.hasInactiveTrackColor == 1) {
      inactiveTrackColor = Color(map.inactiveTrackColor);
    }

    Color? focusColor;
    if (map.hasFocusColor == 1) {
      focusColor = Color(map.focusColor);
    }

    Color? hoverColor;
    if (map.hasHoverColor == 1) {
      hoverColor = Color(map.hoverColor);
    }

    // Parse splashRadius
    double? splashRadius;
    if (map.hasSplashRadius == 1) {
      splashRadius = map.splashRadius;
    }

    // Parse materialTapTargetSize
    MaterialTapTargetSize? materialTapTargetSize;
    if (map.hasMaterialTapTargetSize == 1 &&
        map.materialTapTargetSize >= 0 &&
        map.materialTapTargetSize < MaterialTapTargetSize.values.length) {
      materialTapTargetSize =
          MaterialTapTargetSize.values[map.materialTapTargetSize];
    }

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    return Switch(
      value: value,
      onChanged: onChangedAction != null
          ? (bool newValue) =>
              invokeActionWithArgs(onChangedAction, {'value': newValue})
          : null,
      activeThumbColor: activeColor,
      activeTrackColor: activeTrackColor,
      inactiveThumbColor: inactiveThumbColor,
      inactiveTrackColor: inactiveTrackColor,
      focusColor: focusColor,
      hoverColor: hoverColor,
      splashRadius: splashRadius,
      materialTapTargetSize: materialTapTargetSize,
      autofocus: autofocus,
    );
  }

  @override
  String get widgetName => "Switch";
}

/// Invoke a callback action with arguments via the method channel
void invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
