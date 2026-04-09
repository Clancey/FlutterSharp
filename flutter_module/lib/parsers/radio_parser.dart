// Manual parser for Radio widget
// Part of FlutterSharp Phase 4 - Input Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/radio_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design Radio widget.
///
/// A Material Design radio button used to select between a number of
/// mutually exclusive values. When one radio button in a group is selected,
/// the other radio buttons in the group cease to be selected.
///
/// The radio button itself does not maintain any state. Instead, selecting the
/// radio invokes the onChanged callback, passing value as a parameter. If
/// groupValue and value match, this radio will be selected.
class RadioParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<RadioStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    final value = map.value;

    int? groupValue;
    if (map.hasGroupValue == 1) {
      groupValue = map.groupValue;
    }

    final onChangedAction =
        map.hasOnChangedAction == 1 ? parseString(map.onChangedAction) : null;

    final toggleable = map.toggleable == 1;

    Color? activeColor;
    if (map.hasActiveColor == 1) {
      activeColor = Color(map.activeColor);
    }

    Color? focusColor;
    if (map.hasFocusColor == 1) {
      focusColor = Color(map.focusColor);
    }

    Color? hoverColor;
    if (map.hasHoverColor == 1) {
      hoverColor = Color(map.hoverColor);
    }

    double? splashRadius;
    if (map.hasSplashRadius == 1) {
      splashRadius = map.splashRadius;
    }

    MaterialTapTargetSize? materialTapTargetSize;
    if (map.hasMaterialTapTargetSize == 1 &&
        map.materialTapTargetSize >= 0 &&
        map.materialTapTargetSize < MaterialTapTargetSize.values.length) {
      materialTapTargetSize =
          MaterialTapTargetSize.values[map.materialTapTargetSize];
    }

    final autofocus = map.autofocus == 1;

    return Radio<int>(
      value: value,
      groupValue: groupValue,
      onChanged: onChangedAction != null
          ? (int? newValue) =>
              invokeActionWithArgs(onChangedAction, {'value': newValue})
          : null,
      toggleable: toggleable,
      activeColor: activeColor,
      focusColor: focusColor,
      hoverColor: hoverColor,
      splashRadius: splashRadius,
      materialTapTargetSize: materialTapTargetSize,
      autofocus: autofocus,
    );
  }

  @override
  String get widgetName => "Radio";
}

void invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
