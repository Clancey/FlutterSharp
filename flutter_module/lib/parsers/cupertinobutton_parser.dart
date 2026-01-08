// Manual parser for CupertinoButton widget
// Part of FlutterSharp Phase 5 - Cupertino Widgets

import 'dart:ffi' hide Size;

import 'package:flutter/cupertino.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for iOS-style CupertinoButton widget.
///
/// An iOS-style button that takes in a text or an icon that fades out and in on touch.
/// May optionally have a background.
class CupertinoButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<CupertinoButtonStruct>.fromAddress(fos.handle.address).ref;

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
    final onFocusChangeAction = map.hasOnFocusChange == 1
        ? parseString(map.onFocusChangeAction)
        : null;

    // Parse padding
    EdgeInsetsGeometry? padding;
    if (map.hasPadding == 1) {
      padding = EdgeInsets.fromLTRB(
        map.paddingLeft,
        map.paddingTop,
        map.paddingRight,
        map.paddingBottom,
      );
    }

    // Parse colors
    Color? color;
    if (map.hasColor == 1) {
      color = Color(map.color);
    }

    // Note: foregroundColor is supported in newer Flutter versions
    // Parsing it but not using yet for backward compatibility

    Color disabledColor = CupertinoColors.quaternarySystemFill;
    if (map.hasDisabledColor == 1) {
      disabledColor = Color(map.disabledColor);
    }

    // Parse minimum size
    Size? minimumSize;
    if (map.hasMinimumSize == 1) {
      minimumSize = Size(map.minimumSizeWidth, map.minimumSizeHeight);
    }

    // Parse pressed opacity
    double? pressedOpacity;
    if (map.hasPressedOpacity == 1) {
      pressedOpacity = map.pressedOpacity;
    }

    // Parse border radius
    BorderRadius? borderRadius;
    if (map.hasBorderRadius == 1) {
      borderRadius = BorderRadius.circular(map.borderRadius);
    }

    // Parse alignment
    AlignmentGeometry alignment = Alignment.center;
    if (map.hasAlignment == 1) {
      alignment = Alignment(map.alignmentX, map.alignmentY);
    }

    // Parse size style enum
    CupertinoButtonSize sizeStyle = CupertinoButtonSize.large;
    if (map.sizeStyle >= 0 && map.sizeStyle <= 2) {
      sizeStyle = CupertinoButtonSize.values[map.sizeStyle];
    }

    // Parse autofocus (default false)
    final autofocus = map.autofocus == 1;

    // Parse isFilled
    final isFilled = map.isFilled == 1;

    // Parse child widget
    final child = map.child.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.child.cast<WidgetStruct>(), buildContext)
        : const SizedBox.shrink();

    // Create the appropriate button type
    if (isFilled) {
      return CupertinoButton.filled(
        onPressed: onPressedAction != null
            ? () => _invokeAction(onPressedAction)
            : null,
        onLongPress: onLongPressAction != null
            ? () => _invokeAction(onLongPressAction)
            : null,
        onFocusChange: onFocusChangeAction != null
            ? (hasFocus) => _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
            : null,
        padding: padding,
        color: color,
        disabledColor: disabledColor,
        minimumSize: minimumSize,
        pressedOpacity: pressedOpacity,
        borderRadius: borderRadius,
        alignment: alignment,
        sizeStyle: sizeStyle,
        autofocus: autofocus,
        child: child!,
      );
    } else {
      return CupertinoButton(
        onPressed: onPressedAction != null
            ? () => _invokeAction(onPressedAction)
            : null,
        onLongPress: onLongPressAction != null
            ? () => _invokeAction(onLongPressAction)
            : null,
        onFocusChange: onFocusChangeAction != null
            ? (hasFocus) => _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
            : null,
        padding: padding,
        color: color,
        disabledColor: disabledColor,
        minimumSize: minimumSize,
        pressedOpacity: pressedOpacity,
        borderRadius: borderRadius,
        alignment: alignment,
        sizeStyle: sizeStyle,
        autofocus: autofocus,
        child: child!,
      );
    }
  }

  @override
  String get widgetName => "CupertinoButton";
}

/// Invoke a void callback action via the method channel
void _invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}

/// Invoke a callback action with arguments via the method channel
void _invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
