// Manual parser for Slider widget
// Part of FlutterSharp Phase 4 - Input Widgets

import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/slider_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design Slider widget.
///
/// A Material Design slider that can be used to select from a range of values.
/// The slider itself does not maintain any state. Instead, when the state of the
/// slider changes, the widget calls the onChanged callback.
class SliderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<SliderStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse value (required double)
    final value = map.value;

    // Parse min and max
    final min = map.min;
    final max = map.max;

    // Parse callback action IDs
    final onChangedAction = map.hasOnChanged == 1
        ? parseString(map.onChangedAction)
        : null;

    final onChangeStartAction = map.hasOnChangeStart == 1
        ? parseString(map.onChangeStartAction)
        : null;

    final onChangeEndAction = map.hasOnChangeEnd == 1
        ? parseString(map.onChangeEndAction)
        : null;

    // Parse divisions
    int? divisions;
    if (map.hasDivisions == 1) {
      divisions = map.divisions;
    }

    // Parse label
    String? label;
    if (map.hasLabel == 1) {
      label = parseString(map.label);
    }

    // Parse colors
    Color? activeColor;
    if (map.hasActiveColor == 1) {
      activeColor = Color(map.activeColor);
    }

    Color? inactiveColor;
    if (map.hasInactiveColor == 1) {
      inactiveColor = Color(map.inactiveColor);
    }

    Color? thumbColor;
    if (map.hasThumbColor == 1) {
      thumbColor = Color(map.thumbColor);
    }

    Color? secondaryActiveColor;
    if (map.hasSecondaryActiveColor == 1) {
      secondaryActiveColor = Color(map.secondaryActiveColor);
    }

    // Parse secondaryTrackValue
    double? secondaryTrackValue;
    if (map.hasSecondaryTrackValue == 1) {
      secondaryTrackValue = map.secondaryTrackValue;
    }

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    // Parse allowedInteraction
    SliderInteraction? allowedInteraction;
    if (map.hasAllowedInteraction == 1 &&
        map.allowedInteraction >= 0 &&
        map.allowedInteraction < SliderInteraction.values.length) {
      allowedInteraction = SliderInteraction.values[map.allowedInteraction];
    }

    return Slider(
      value: value,
      min: min,
      max: max,
      onChanged: onChangedAction != null
          ? (double newValue) => invokeActionWithArgs(onChangedAction, {'value': newValue})
          : null,
      onChangeStart: onChangeStartAction != null
          ? (double newValue) => invokeActionWithArgs(onChangeStartAction, {'value': newValue})
          : null,
      onChangeEnd: onChangeEndAction != null
          ? (double newValue) => invokeActionWithArgs(onChangeEndAction, {'value': newValue})
          : null,
      divisions: divisions,
      label: label,
      activeColor: activeColor,
      inactiveColor: inactiveColor,
      thumbColor: thumbColor,
      secondaryActiveColor: secondaryActiveColor,
      secondaryTrackValue: secondaryTrackValue,
      autofocus: autofocus,
      allowedInteraction: allowedInteraction,
    );
  }

  @override
  String get widgetName => "Slider";
}

/// Invoke a callback action with arguments via the method channel
void invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
