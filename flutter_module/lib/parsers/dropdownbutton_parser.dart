// Manual parser for DropdownButton widget
// Part of FlutterSharp Phase 4 - Input Widgets

import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/dropdownbutton_struct.dart';
import '../generated/structs/dropdownmenuitem_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design DropdownButton widget.
///
/// A dropdown button lets the user select from a number of items. The button
/// shows the currently selected item as well as an arrow that opens a menu for
/// selecting another item.
class DropdownButtonParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<DropdownButtonStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse items
    final List<DropdownMenuItem<int>> items = [];
    if (map.itemCount > 0 && map.items.address != 0) {
      for (int i = 0; i < map.itemCount; i++) {
        final itemPtr = map.items.elementAt(i).value;
        if (itemPtr.address != 0) {
          final item = _parseDropdownMenuItem(itemPtr.ref, buildContext);
          if (item != null) {
            items.add(item);
          }
        }
      }
    }

    // Parse value (currently selected item)
    int? value;
    if (map.hasValue == 1) {
      value = map.value;
    }

    // Parse callback action ID
    final onChangedAction = map.hasOnChanged == 1
        ? parseString(map.onChangedAction)
        : null;

    // Parse onTap callback
    final onTapAction = map.hasOnTap == 1
        ? parseString(map.onTapAction)
        : null;

    // Parse hint widget
    Widget? hint;
    if (map.hint.address != 0) {
      hint = DynamicWidgetBuilder.buildFromPointer(map.hint, buildContext);
    }

    // Parse disabledHint widget
    Widget? disabledHint;
    if (map.disabledHint.address != 0) {
      disabledHint = DynamicWidgetBuilder.buildFromPointer(map.disabledHint, buildContext);
    }

    // Parse elevation
    int elevation = 8; // Flutter default
    if (map.hasElevation == 1) {
      elevation = map.elevation;
    }

    // Parse underline widget
    Widget? underline;
    if (map.underline.address != 0) {
      underline = DynamicWidgetBuilder.buildFromPointer(map.underline, buildContext);
    }

    // Parse icon widget
    Widget? icon;
    if (map.icon.address != 0) {
      icon = DynamicWidgetBuilder.buildFromPointer(map.icon, buildContext);
    }

    // Parse iconDisabledColor
    Color? iconDisabledColor;
    if (map.hasIconDisabledColor == 1) {
      iconDisabledColor = Color(map.iconDisabledColor);
    }

    // Parse iconEnabledColor
    Color? iconEnabledColor;
    if (map.hasIconEnabledColor == 1) {
      iconEnabledColor = Color(map.iconEnabledColor);
    }

    // Parse iconSize
    double iconSize = 24.0; // Flutter default
    if (map.hasIconSize == 1) {
      iconSize = map.iconSize;
    }

    // Parse isDense
    final isDense = map.isDense == 1;

    // Parse isExpanded
    final isExpanded = map.isExpanded == 1;

    // Parse itemHeight
    double? itemHeight;
    if (map.hasItemHeight == 1) {
      itemHeight = map.itemHeight;
    }

    // Parse focusColor
    Color? focusColor;
    if (map.hasFocusColor == 1) {
      focusColor = Color(map.focusColor);
    }

    // Parse autofocus
    final autofocus = map.autofocus == 1;

    // Parse dropdownColor
    Color? dropdownColor;
    if (map.hasDropdownColor == 1) {
      dropdownColor = Color(map.dropdownColor);
    }

    // Parse menuMaxHeight
    double? menuMaxHeight;
    if (map.hasMenuMaxHeight == 1) {
      menuMaxHeight = map.menuMaxHeight;
    }

    // Parse enableFeedback
    bool? enableFeedback;
    if (map.hasEnableFeedback == 1) {
      enableFeedback = map.enableFeedback == 1;
    }

    // Parse alignment
    AlignmentGeometry alignment = AlignmentDirectional.centerStart;
    if (map.hasAlignment == 1) {
      alignment = _parseAlignmentDirectional(map.alignment);
    }

    // Parse borderRadius
    BorderRadius? borderRadius;
    if (map.hasBorderRadius == 1) {
      borderRadius = BorderRadius.only(
        topLeft: Radius.circular(map.borderRadiusTopLeft),
        topRight: Radius.circular(map.borderRadiusTopRight),
        bottomLeft: Radius.circular(map.borderRadiusBottomLeft),
        bottomRight: Radius.circular(map.borderRadiusBottomRight),
      );
    }

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

    return DropdownButton<int>(
      items: items,
      value: value,
      onChanged: onChangedAction != null
          ? (int? newValue) => _invokeOnChanged(onChangedAction, newValue)
          : null,
      onTap: onTapAction != null
          ? () => raiseMauiEvent(onTapAction, "invoke", {})
          : null,
      hint: hint,
      disabledHint: disabledHint,
      elevation: elevation,
      underline: underline,
      icon: icon,
      iconDisabledColor: iconDisabledColor,
      iconEnabledColor: iconEnabledColor,
      iconSize: iconSize,
      isDense: isDense,
      isExpanded: isExpanded,
      itemHeight: itemHeight,
      focusColor: focusColor,
      autofocus: autofocus,
      dropdownColor: dropdownColor,
      menuMaxHeight: menuMaxHeight,
      enableFeedback: enableFeedback,
      alignment: alignment,
      borderRadius: borderRadius,
      padding: padding,
    );
  }

  /// Parse a DropdownMenuItem from its FFI struct
  DropdownMenuItem<int>? _parseDropdownMenuItem(
      DropdownMenuItemStruct itemStruct,
      BuildContext buildContext) {
    // Parse child widget (required)
    Widget? child;
    if (itemStruct.child.address != 0) {
      child = DynamicWidgetBuilder.buildFromPointer(itemStruct.child, buildContext);
    }
    if (child == null) {
      return null; // Child is required
    }

    // Parse value
    final value = itemStruct.value;

    // Parse enabled
    bool enabled = true; // Default
    if (itemStruct.hasEnabled == 1) {
      enabled = itemStruct.enabled == 1;
    }

    // Parse alignment
    AlignmentGeometry alignment = AlignmentDirectional.centerStart;
    if (itemStruct.hasAlignment == 1) {
      alignment = _parseAlignmentDirectional(itemStruct.alignment);
    }

    return DropdownMenuItem<int>(
      value: value,
      enabled: enabled,
      alignment: alignment,
      child: child,
    );
  }

  /// Parse AlignmentDirectional from int value
  AlignmentGeometry _parseAlignmentDirectional(int value) {
    switch (value) {
      case 0:
        return AlignmentDirectional.topStart;
      case 1:
        return AlignmentDirectional.topCenter;
      case 2:
        return AlignmentDirectional.topEnd;
      case 3:
        return AlignmentDirectional.centerStart;
      case 4:
        return AlignmentDirectional.center;
      case 5:
        return AlignmentDirectional.centerEnd;
      case 6:
        return AlignmentDirectional.bottomStart;
      case 7:
        return AlignmentDirectional.bottomCenter;
      case 8:
        return AlignmentDirectional.bottomEnd;
      default:
        return AlignmentDirectional.centerStart;
    }
  }

  /// Invoke the onChanged callback with the selected value
  void _invokeOnChanged(String actionId, int? value) {
    raiseMauiEvent(actionId, "invoke", {'value': value});
  }

  @override
  String get widgetName => "DropdownButton";
}
