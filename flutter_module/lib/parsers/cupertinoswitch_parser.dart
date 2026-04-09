// Manual parser for CupertinoSwitch widget
// Part of FlutterSharp Phase 5 - Cupertino Widgets

import 'dart:ffi' hide Size;

import 'package:flutter/cupertino.dart';
import 'package:flutter/gestures.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for iOS-style CupertinoSwitch widget.
///
/// An iOS-style switch.
///
/// Used to toggle the on/off state of a single setting.
/// The switch itself does not maintain any state. Instead, when the state of
/// the switch changes, the widget calls the onChanged callback.
class CupertinoSwitchParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<CupertinoSwitchStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse value (required)
    final value = map.value == 1;

    // Parse callback action IDs
    final onChangedAction =
        map.hasOnChangedAction == 1 ? parseString(map.onChangedAction) : null;
    final onFocusChangeAction = map.hasOnFocusChangeAction == 1
        ? parseString(map.onFocusChangeAction)
        : null;

    // Parse colors
    Color? activeTrackColor;
    if (map.hasActiveTrackColor == 1) {
      activeTrackColor = Color(map.activeTrackColor);
    }

    Color? inactiveTrackColor;
    if (map.hasInactiveTrackColor == 1) {
      inactiveTrackColor = Color(map.inactiveTrackColor);
    }

    Color? thumbColor;
    if (map.hasThumbColor == 1) {
      thumbColor = Color(map.thumbColor);
    }

    Color? focusColor;
    if (map.hasFocusColor == 1) {
      focusColor = Color(map.focusColor);
    }

    // Parse applyTheme
    final bool? applyTheme = map.applyTheme == 1;

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    // Parse drag start behavior
    DragStartBehavior dragStartBehavior = DragStartBehavior.start;
    if (map.dragStartBehavior == 1) {
      dragStartBehavior = DragStartBehavior.down;
    }

    // Create the CupertinoSwitch widget
    return CupertinoSwitch(
      value: value,
      onChanged: onChangedAction != null
          ? (newValue) =>
              _invokeActionWithArgs(onChangedAction, {'value': newValue})
          : null,
      activeTrackColor: activeTrackColor,
      inactiveTrackColor: inactiveTrackColor,
      thumbColor: thumbColor,
      trackOutlineColor: null,
      trackOutlineWidth: null,
      focusColor: focusColor,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) =>
              _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
          : null,
      autofocus: autofocus,
      applyTheme: applyTheme,
      dragStartBehavior: dragStartBehavior,
    );
  }

  @override
  String get widgetName => "CupertinoSwitch";
}

/// Invoke a callback action with arguments via the method channel
void _invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
