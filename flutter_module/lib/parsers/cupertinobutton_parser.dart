import 'dart:ffi' hide Size;

import 'package:flutter/cupertino.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart' show parseString;
import '../maui_flutter.dart';

class CupertinoButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<CupertinoButtonStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse callback action IDs
    final onPressedAction =
        map.hasOnPressedAction == 1 ? parseString(map.onPressedAction) : null;
    final onLongPressAction = map.hasOnLongPressAction == 1
        ? parseString(map.onLongPressAction)
        : null;
    final onFocusChangeAction = map.hasOnFocusChangeAction == 1
        ? parseString(map.onFocusChangeAction)
        : null;

    EdgeInsetsGeometry? padding;
    if (map.hasPadding == 1) {
      padding = EdgeInsets.fromLTRB(
        map.paddingLeft,
        map.paddingTop,
        map.paddingRight,
        map.paddingBottom,
      );
    }

    final color = map.hasColor == 1 ? Color(map.color) : null;
    final foregroundColor =
        map.hasForegroundColor == 1 ? Color(map.foregroundColor) : null;
    final disabledColor =
        map.hasDisabledColor == 1 ? Color(map.disabledColor) : null;
    final resolvedDisabledColor =
        disabledColor ?? CupertinoColors.quaternarySystemFill;

    Size? minimumSize;
    if (map.hasMinimumSize == 1) {
      minimumSize = Size(map.minimumSizeWidth, map.minimumSizeHeight);
    }

    double? pressedOpacity;
    if (map.hasPressedOpacity == 1) {
      pressedOpacity = map.pressedOpacity;
    }

    BorderRadius? borderRadius;
    if (map.hasBorderRadius == 1) {
      borderRadius = BorderRadius.circular(map.borderRadius);
    }

    AlignmentGeometry alignment = Alignment.center;
    if (map.hasAlignment == 1) {
      alignment = Alignment(map.alignmentX, map.alignmentY);
    }

    final sizeStyle =
        map.sizeStyle >= 0 && map.sizeStyle < CupertinoButtonSize.values.length
            ? CupertinoButtonSize.values[map.sizeStyle]
            : CupertinoButtonSize.large;
    final autofocus = map.autofocus == 1;
    final child =
        DynamicWidgetBuilder.buildFromPointerNotNull(map.child, buildContext);

    if (map.isFilled == 1) {
      return CupertinoButton.filled(
        onPressed: onPressedAction != null
            ? () => _invokeAction(onPressedAction)
            : null,
        onLongPress: onLongPressAction != null
            ? () => _invokeAction(onLongPressAction)
            : null,
        onFocusChange: onFocusChangeAction != null
            ? (hasFocus) =>
                _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
            : null,
        padding: padding,
        color: color ?? CupertinoColors.activeBlue,
        foregroundColor: foregroundColor,
        disabledColor: resolvedDisabledColor,
        minimumSize: minimumSize,
        pressedOpacity: pressedOpacity,
        borderRadius: borderRadius,
        alignment: alignment,
        sizeStyle: sizeStyle,
        autofocus: autofocus,
        child: child,
      );
    }

    return CupertinoButton(
      onPressed:
          onPressedAction != null ? () => _invokeAction(onPressedAction) : null,
      onLongPress: onLongPressAction != null
          ? () => _invokeAction(onLongPressAction)
          : null,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) =>
              _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
          : null,
      padding: padding,
      color: color,
      foregroundColor: foregroundColor,
      disabledColor: resolvedDisabledColor,
      minimumSize: minimumSize,
      pressedOpacity: pressedOpacity,
      borderRadius: borderRadius,
      alignment: alignment,
      sizeStyle: sizeStyle,
      autofocus: autofocus,
      child: child,
    );
  }

  @override
  String get widgetName => "CupertinoButton";
}

void _invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}

void _invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
