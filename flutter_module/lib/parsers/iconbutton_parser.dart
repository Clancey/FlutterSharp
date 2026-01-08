// Manual parser for IconButton widget
// Part of FlutterSharp Phase 4 - Button Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/iconbutton_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design IconButton widget.
///
/// An icon button is a picture printed on a Material widget that reacts
/// to touches by filling with color (ink).
class IconButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<IconButtonStruct>.fromAddress(fos.handle.address).ref;

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

    // Parse tooltip
    final tooltip = map.hasTooltip == 1
        ? parseString(map.tooltip)
        : null;

    // Parse icon widget (required)
    final icon = map.icon.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.icon.cast<WidgetStruct>(), buildContext)
        : null;

    // If no icon provided, return null (icon is required)
    if (icon == null) return null;

    // Parse iconSize (optional)
    final iconSize = map.hasIconSize == 1 ? map.iconSize : null;

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    return IconButton(
      onPressed: onPressedAction != null
          ? () => raiseMauiEvent(onPressedAction, "invoke", null)
          : null,
      onLongPress: onLongPressAction != null
          ? () => raiseMauiEvent(onLongPressAction, "invoke", null)
          : null,
      icon: icon,
      tooltip: tooltip,
      iconSize: iconSize,
      autofocus: autofocus,
    );
  }

  @override
  String get widgetName => "IconButton";
}
