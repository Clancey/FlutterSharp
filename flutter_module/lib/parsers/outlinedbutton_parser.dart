// Manual parser for OutlinedButton widget
// Part of FlutterSharp Phase 4 - Button Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/outlinedbutton_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design OutlinedButton widget.
///
/// An outlined button is a medium-emphasis button with an outlined border
/// and no fill color. Used for important but non-primary actions.
class OutlinedButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<OutlinedButtonStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse callback action IDs
    final onPressedAction = map.hasOnPressed == 1
        ? parseString(map.onPressedAction)
        : null;
    final onLongPressAction = map.hasOnLongPress == 1
        ? parseString(map.onLongPressAction)
        : null;
    final onHoverAction = map.hasOnHover == 1
        ? parseString(map.onHoverAction)
        : null;
    final onFocusChangeAction = map.hasOnFocusChange == 1
        ? parseString(map.onFocusChangeAction)
        : null;

    // Parse child widget
    final child = map.child.address != 0
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

    return OutlinedButton(
      onPressed: onPressedAction != null
          ? () => raiseMauiEvent(onPressedAction, "invoke", null)
          : null,
      onLongPress: onLongPressAction != null
          ? () => raiseMauiEvent(onLongPressAction, "invoke", null)
          : null,
      onHover: onHoverAction != null
          ? (isHovering) => raiseMauiEvent(onHoverAction, "invoke", {'value': isHovering})
          : null,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) => raiseMauiEvent(onFocusChangeAction, "invoke", {'value': hasFocus})
          : null,
      autofocus: autofocus,
      clipBehavior: clipBehavior ?? Clip.none,
      child: child,
    );
  }

  @override
  String get widgetName => "OutlinedButton";
}
