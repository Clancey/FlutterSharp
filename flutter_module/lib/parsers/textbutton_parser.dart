// Manual parser for TextButton widget
// Part of FlutterSharp Phase 4 - Button Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design TextButton widget.
///
/// A text button is a simple button with no outline or fill color.
/// Typically used for less-pronounced actions in dialogs and cards.
class TextButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<TextButtonStruct>.fromAddress(fos.handle.address).ref;

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

    return TextButton(
      onPressed: onPressedAction != null
          ? () => raiseMauiEvent(onPressedAction, "invoke", null)
          : null,
      onLongPress: onLongPressAction != null
          ? () => raiseMauiEvent(onLongPressAction, "invoke", null)
          : null,
      onHover: onHoverAction != null
          ? (isHovering) =>
              raiseMauiEvent(onHoverAction, "invoke", {'value': isHovering})
          : null,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) =>
              raiseMauiEvent(onFocusChangeAction, "invoke", {'value': hasFocus})
          : null,
      autofocus: autofocus,
      clipBehavior: clipBehavior ?? Clip.none,
      child: child ?? const SizedBox.shrink(),
    );
  }

  @override
  String get widgetName => "TextButton";
}
