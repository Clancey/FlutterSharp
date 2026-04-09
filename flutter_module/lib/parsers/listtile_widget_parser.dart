// Manual parser for ListTile widget
// Part of FlutterSharp Phase 4 - List Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design ListTile widget.
///
/// A single fixed-height row that typically contains some text as well as
/// a leading or trailing icon.
///
/// A list tile contains one to three lines of text optionally flanked by icons
/// or other widgets, such as check boxes. The icons (or other widgets) for the
/// tile are defined with the [leading] and [trailing] parameters. The first
/// line of text is not optional and is specified with [title]. The value of
/// [subtitle], which is optional, will occupy the space allocated for an
/// additional line of text, or two lines if [isThreeLine] is true.
class ListTileWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<ListTileStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse child widgets
    final leading = map.leading.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.leading.cast<WidgetStruct>(), buildContext)
        : null;

    final title = map.title.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.title.cast<WidgetStruct>(), buildContext)
        : null;

    final subtitle = map.subtitle.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.subtitle.cast<WidgetStruct>(), buildContext)
        : null;

    final trailing = map.trailing.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.trailing.cast<WidgetStruct>(), buildContext)
        : null;

    // Parse boolean properties
    final isThreeLine = map.hasIsThreeLine == 1 ? map.isThreeLine == 1 : false;
    final dense = map.hasDense == 1 ? map.dense == 1 : null;
    final enabled = map.hasEnabled == 1 ? map.enabled == 1 : true;
    final selected = map.hasSelected == 1 ? map.selected == 1 : false;
    final autofocus = map.hasAutofocus == 1 ? map.autofocus == 1 : false;
    final enableFeedback =
        map.hasEnableFeedback == 1 ? map.enableFeedback == 1 : null;

    // Parse callback action IDs
    final onTapAction =
        map.onTapAction.address != 0 ? parseString(map.onTapAction) : null;
    final onLongPressAction = map.onLongPressAction.address != 0
        ? parseString(map.onLongPressAction)
        : null;
    final onFocusChangeAction = map.onFocusChangeAction.address != 0
        ? parseString(map.onFocusChangeAction)
        : null;

    // Parse colors (stored as ARGB ints)
    final tileColor = map.hasTileColor == 1 ? Color(map.tileColor) : null;
    final selectedTileColor =
        map.hasSelectedTileColor == 1 ? Color(map.selectedTileColor) : null;
    final iconColor = map.hasIconColor == 1 ? Color(map.iconColor) : null;
    final textColor = map.hasTextColor == 1 ? Color(map.textColor) : null;
    final focusColor = map.hasFocusColor == 1 ? Color(map.focusColor) : null;
    final hoverColor = map.hasHoverColor == 1 ? Color(map.hoverColor) : null;
    final splashColor = map.hasSplashColor == 1 ? Color(map.splashColor) : null;

    // Parse numeric properties
    final horizontalTitleGap =
        map.hasHorizontalTitleGap == 1 ? map.horizontalTitleGap : null;
    final minVerticalPadding =
        map.hasMinVerticalPadding == 1 ? map.minVerticalPadding : null;
    final minLeadingWidth =
        map.hasMinLeadingWidth == 1 ? map.minLeadingWidth : null;
    final minTileHeight = map.hasMinTileHeight == 1 ? map.minTileHeight : null;

    // Parse enum properties (sentinel value -1 means null)
    ListTileTitleAlignment? titleAlignment;
    if (map.titleAlignment >= 0 &&
        map.titleAlignment < ListTileTitleAlignment.values.length) {
      titleAlignment = ListTileTitleAlignment.values[map.titleAlignment];
    }

    ListTileStyle? style;
    if (map.style >= 0 && map.style < ListTileStyle.values.length) {
      style = ListTileStyle.values[map.style];
    }

    return ListTile(
      leading: leading,
      title: title,
      subtitle: subtitle,
      trailing: trailing,
      isThreeLine: isThreeLine,
      dense: dense,
      enabled: enabled,
      selected: selected,
      onTap: onTapAction != null ? () => _invokeAction(onTapAction) : null,
      onLongPress: onLongPressAction != null
          ? () => _invokeAction(onLongPressAction)
          : null,
      onFocusChange: onFocusChangeAction != null
          ? (hasFocus) =>
              _invokeActionWithArgs(onFocusChangeAction, {'value': hasFocus})
          : null,
      tileColor: tileColor,
      selectedTileColor: selectedTileColor,
      iconColor: iconColor,
      textColor: textColor,
      focusColor: focusColor,
      hoverColor: hoverColor,
      splashColor: splashColor,
      // contentPadding: not currently supported (complex type)
      autofocus: autofocus,
      enableFeedback: enableFeedback,
      horizontalTitleGap: horizontalTitleGap,
      minVerticalPadding: minVerticalPadding,
      minLeadingWidth: minLeadingWidth,
      minTileHeight: minTileHeight,
      titleAlignment: titleAlignment,
      style: style,
    );
  }

  @override
  String get widgetName => "ListTile";
}

/// Invoke a void callback action via the method channel
void _invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}

/// Invoke a callback action with arguments via the method channel
void _invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
