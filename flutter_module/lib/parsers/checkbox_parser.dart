// Manual parser for Checkbox widget
// Part of FlutterSharp Phase 4 - Input Widgets

import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart' hide CheckboxStruct;
import '../generated/structs/checkbox_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design Checkbox widget.
///
/// A Material Design checkbox that can be in checked, unchecked, or mixed state.
/// The checkbox itself does not maintain any state. Instead, when the state of the
/// checkbox changes, the widget calls the onChanged callback.
class CheckboxParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<CheckboxStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse value (nullable bool for tristate support)
    bool? value;
    if (map.hasValue == 1) {
      value = map.value == 1;
    } else {
      // When hasValue is 0, value is null (mixed state in tristate mode)
      value = null;
    }

    // Parse tristate (default false)
    final tristate = map.tristate == 1;

    // Parse callback action ID
    final onChangedAction = map.hasOnChanged == 1
        ? parseString(map.onChangedAction)
        : null;

    // Parse colors
    Color? activeColor;
    if (map.hasActiveColor == 1) {
      activeColor = Color(map.activeColor);
    }

    Color? checkColor;
    if (map.hasCheckColor == 1) {
      checkColor = Color(map.checkColor);
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
      materialTapTargetSize = MaterialTapTargetSize.values[map.materialTapTargetSize];
    }

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    // Parse isError (default false)
    final isError = map.hasIsError == 1 && map.isError == 1;

    // Parse semanticLabel
    final semanticLabel = map.hasSemanticLabel == 1
        ? parseString(map.semanticLabel)
        : null;

    return Checkbox(
      value: value,
      onChanged: onChangedAction != null
          ? (bool? newValue) => invokeActionWithArgs(onChangedAction, {'value': newValue})
          : null,
      tristate: tristate,
      activeColor: activeColor,
      checkColor: checkColor,
      focusColor: focusColor,
      hoverColor: hoverColor,
      splashRadius: splashRadius,
      materialTapTargetSize: materialTapTargetSize,
      autofocus: autofocus,
      isError: isError,
      semanticLabel: semanticLabel,
    );
  }

  @override
  String get widgetName => "Checkbox";
}

/// Invoke a callback action with arguments via the method channel
void invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
