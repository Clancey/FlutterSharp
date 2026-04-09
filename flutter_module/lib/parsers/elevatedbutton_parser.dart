// Manual parser for ElevatedButton widget
// Part of FlutterSharp Phase 4 - Button Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design ElevatedButton widget.
///
/// An elevated button is a filled button whose Material elevates when pressed.
/// Use elevated buttons to add dimension to otherwise mostly flat layouts.
class ElevatedButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<ElevatedButtonStruct>.fromAddress(fos.handle.address).ref;

    // Parse callback action IDs
    final onPressedAction =
        map.hasOnPressedAction == 1 ? parseString(map.onPressedAction) : null;
    final onLongPressAction = map.hasOnLongPressAction == 1
        ? parseString(map.onLongPressAction)
        : null;
    final onHoverAction =
        map.hasOnHoverAction == 1 ? parseString(map.onHoverAction) : null;
    final onFocusChangeAction = map.hasOnFocusChangeAction == 1
        ? parseString(map.onFocusChangeAction)
        : null;

    // Parse child widget
    final child = map.hasChild == 1 && map.child.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.child.cast<WidgetStruct>(), buildContext)
        : null;

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    // Parse clipBehavior enum
    Clip? clipBehavior;
    if (map.clipBehavior >= 0 && map.clipBehavior <= 3) {
      clipBehavior = Clip.values[map.clipBehavior];
    }

    return ElevatedButton(
      onPressed:
          onPressedAction != null ? () => invokeAction(onPressedAction) : null,
      onLongPress: onLongPressAction != null
          ? () => invokeAction(onLongPressAction)
          : null,
      onHover: onHoverAction != null
          ? (isHovering) =>
              invokeActionWithArgs(onHoverAction, {'value': isHovering})
          : null,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) =>
              invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
          : null,
      autofocus: autofocus,
      clipBehavior: clipBehavior ?? Clip.none,
      child: child,
    );
  }

  @override
  String get widgetName => "ElevatedButton";
}

/// Invoke a void callback action via the method channel
void invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}

/// Invoke a callback action with arguments via the method channel
void invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
