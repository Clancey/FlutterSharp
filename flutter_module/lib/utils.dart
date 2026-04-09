import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';

import 'maui_flutter.dart';
import 'parsers/drop_cap_text.dart';

TextAlign parseTextAlign(String? textAlignString) {
  //left the system decide
  TextAlign textAlign = TextAlign.start;
  if (textAlignString == null) return textAlign;
  switch (textAlignString) {
    case "left":
      textAlign = TextAlign.left;
      break;
    case "right":
      textAlign = TextAlign.right;
      break;
    case "center":
      textAlign = TextAlign.center;
      break;
    case "justify":
      textAlign = TextAlign.justify;
      break;
    case "start":
      textAlign = TextAlign.start;
      break;
    case "end":
      textAlign = TextAlign.end;
      break;
    default:
      textAlign = TextAlign.start;
  }
  return textAlign;
}

TextOverflow parseTextOverflow(String? textOverflowString) {
  TextOverflow textOverflow = TextOverflow.ellipsis;
  if (textOverflowString == null) return textOverflow;
  switch (textOverflowString) {
    case "ellipsis":
      textOverflow = TextOverflow.ellipsis;
      break;
    case "clip":
      textOverflow = TextOverflow.clip;
      break;
    case "fade":
      textOverflow = TextOverflow.fade;
      break;
    default:
      textOverflow = TextOverflow.fade;
  }
  return textOverflow;
}

TextDecoration parseTextDecoration(String textDecorationString) {
  TextDecoration textDecoration = TextDecoration.none;
  switch (textDecorationString) {
    case "lineThrough":
      textDecoration = TextDecoration.lineThrough;
      break;
    case "overline":
      textDecoration = TextDecoration.overline;
      break;
    case "underline":
      textDecoration = TextDecoration.underline;
      break;
    case "none":
    default:
      textDecoration = TextDecoration.none;
  }
  return textDecoration;
}

TextDirection parseTextDirection(String? textDirectionString) {
  TextDirection textDirection = TextDirection.ltr;
  if (textDirectionString == null) return textDirection;
  switch (textDirectionString) {
    case 'ltr':
      textDirection = TextDirection.ltr;
      break;
    case 'rtl':
      textDirection = TextDirection.rtl;
      break;
    default:
      textDirection = TextDirection.ltr;
  }
  return textDirection;
}

// ============================================================================
// Integer-based enum parsing functions for FFI struct values
// ============================================================================

/// Parse TextAlign from integer value (matches C# TextAlign enum)
/// 0=Left, 1=Right, 2=Center, 3=Justify, 4=Start, 5=End
TextAlign? parseTextAlignFromInt(int? value) {
  if (value == null || value < 0 || value > 5) return null;
  return TextAlign.values[value];
}

/// Parse TextOverflow from integer value (matches C# TextOverflow enum)
/// 0=Clip, 1=Fade, 2=Ellipsis, 3=Visible
TextOverflow? parseTextOverflowFromInt(int? value) {
  if (value == null || value < 0 || value > 3) return null;
  return TextOverflow.values[value];
}

/// Parse TextDirection from integer value
/// 0=rtl, 1=ltr
TextDirection? parseTextDirectionFromInt(int? value) {
  if (value == null || value < 0 || value > 1) return null;
  return TextDirection.values[value];
}

/// Parse TextWidthBasis from integer value
/// 0=parent, 1=longestLine
TextWidthBasis? parseTextWidthBasisFromInt(int? value) {
  if (value == null || value < 0 || value > 1) return null;
  return TextWidthBasis.values[value];
}

FontWeight parseFontWeight(String textFontWeight) {
  FontWeight fontWeight = FontWeight.normal;
  switch (textFontWeight) {
    case 'w100':
      fontWeight = FontWeight.w100;
      break;
    case 'w200':
      fontWeight = FontWeight.w200;
      break;
    case 'w300':
      fontWeight = FontWeight.w300;
      break;
    case 'normal':
    case 'w400':
      fontWeight = FontWeight.w400;
      break;
    case 'w500':
      fontWeight = FontWeight.w500;
      break;
    case 'w600':
      fontWeight = FontWeight.w600;
      break;
    case 'bold':
    case 'w700':
      fontWeight = FontWeight.w700;
      break;
    case 'w800':
      fontWeight = FontWeight.w800;
      break;
    case 'w900':
      fontWeight = FontWeight.w900;
      break;
    default:
      fontWeight = FontWeight.normal;
  }
  return fontWeight;
}

Color? parseColor(ColorStruct color) {
  return Color.fromARGB(color.alpha, color.red, color.green, color.blue);
}

Color? parseHexColor(String? hexColorString) {
  if (hexColorString == null) {
    return null;
  }
  hexColorString = hexColorString.toUpperCase().replaceAll("#", "");
  if (hexColorString.length == 6) {
    hexColorString = "FF" + hexColorString;
  }
  int colorInt = int.parse(hexColorString, radix: 16);
  return Color(colorInt);
}

TextStyle parseTextStyle(Map<String, dynamic> map) {
  //TODO: more properties need to be implemented, such as decorationColor, decorationStyle, wordSpacing and so on.
  String color = map['color'];
  String debugLabel = map['debugLabel'];
  String decoration = map['decoration'];
  String fontFamily = map['fontFamily'];
  double fontSize = map['fontSize'];
  String fontWeight = map['fontWeight'];
  FontStyle fontStyle =
      'italic' == map['fontStyle'] ? FontStyle.italic : FontStyle.normal;

  return TextStyle(
    color: parseHexColor(color),
    debugLabel: debugLabel,
    decoration: parseTextDecoration(decoration),
    fontSize: fontSize,
    fontFamily: fontFamily,
    fontStyle: fontStyle,
    fontWeight: parseFontWeight(fontWeight),
  );
}

Alignment parseAlignment(AlignmentStruct alignStruct) {
  return Alignment(alignStruct.x, alignStruct.y);
}

const double infinity = 9999999999;

BoxConstraints? parseBoxConstraints(Map<String, dynamic> map) {
  double minWidth = 0.0;
  double maxWidth = double.infinity;
  double minHeight = 0.0;
  double maxHeight = double.infinity;
  if (map.containsKey('minWidth')) {
    var minWidthValue = map['minWidth'];

    if (minWidthValue != null) {
      if (minWidthValue >= infinity) {
        minWidth = double.infinity;
      } else {
        minWidth = minWidthValue;
      }
    }
  }

  if (map.containsKey('maxWidth')) {
    var maxWidthValue = map['maxWidth'];

    if (maxWidthValue != null) {
      if (maxWidthValue >= infinity) {
        maxWidth = double.infinity;
      } else {
        maxWidth = maxWidthValue;
      }
    }
  }

  if (map.containsKey('minHeight')) {
    var minHeightValue = map['minHeight'];

    if (minHeightValue != null) {
      if (minHeightValue >= infinity) {
        minHeight = double.infinity;
      } else {
        minHeight = minHeightValue;
      }
    }
  }

  if (map.containsKey('maxHeight')) {
    var maxHeightValue = map['maxHeight'];

    if (maxHeightValue != null) {
      if (maxHeightValue >= infinity) {
        maxHeight = double.infinity;
      } else {
        maxHeight = maxHeightValue;
      }
    }
  }

  return BoxConstraints(
    minWidth: minWidth,
    maxWidth: maxWidth,
    minHeight: minHeight,
    maxHeight: maxHeight,
  );
}

EdgeInsetsGeometry? parseEdgeInsetsGeometry(
    int hasMargin, EdgeInsetGemoetryStruct struct) {
  if (hasMargin == 0) return null;
  return EdgeInsets.fromLTRB(
      struct.left, struct.top, struct.right, struct.bottom);
}

CrossAxisAlignment parseCrossAxisAlignment(String crossAxisAlignmentString) {
  var intValue = int.tryParse(crossAxisAlignmentString);
  if (intValue != null) {
    return CrossAxisAlignment.values[intValue];
  }
  switch (crossAxisAlignmentString) {
    case 'start':
      return CrossAxisAlignment.start;
    case 'end':
      return CrossAxisAlignment.end;
    case 'center':
      return CrossAxisAlignment.center;
    case 'stretch':
      return CrossAxisAlignment.stretch;
    case 'baseline':
      return CrossAxisAlignment.baseline;
  }
  return CrossAxisAlignment.center;
}

MainAxisAlignment parseMainAxisAlignment(int hasMainAxos, int mainAxisValue) {
  if (hasMainAxos == 0) return MainAxisAlignment.start;
  return MainAxisAlignment.values[mainAxisValue];
}

MainAxisSize parseMainAxisSize(String mainAxisSizeString) =>
    mainAxisSizeString == 'min' ? MainAxisSize.min : MainAxisSize.max;

TextBaseline parseTextBaseline(String parseTextBaselineString) =>
    'alphabetic' == parseTextBaselineString
        ? TextBaseline.alphabetic
        : TextBaseline.ideographic;

VerticalDirection parseVerticalDirection(String verticalDirectionString) =>
    'up' == verticalDirectionString
        ? VerticalDirection.up
        : VerticalDirection.down;

T? _parseEnumValue<T extends Enum>(
  dynamic value,
  List<T> values, {
  Map<String, String> aliases = const {},
}) {
  if (value == null) return null;
  if (value is T) return value;
  if (value is num) {
    final index = value.toInt();
    if (index < 0 || index >= values.length) return null;
    return values[index];
  }
  if (value is String) {
    final normalized = value.trim();
    if (normalized.isEmpty) return null;
    final aliased = aliases[normalized] ?? normalized;
    for (final candidate in values) {
      if (candidate.name == aliased) return candidate;
    }
  }
  return null;
}

BlendMode? parseBlendMode(dynamic value) {
  return _parseEnumValue(value, BlendMode.values);
}

BoxFit? parseBoxFit(dynamic value) {
  return _parseEnumValue(value, BoxFit.values);
}

Brightness? parseBrightness(dynamic value) {
  return _parseEnumValue(value, Brightness.values);
}

TextCapitalization? parseTextCapitalization(dynamic value) {
  return _parseEnumValue(value, TextCapitalization.values);
}

MaterialTapTargetSize? parseMaterialTapTargetSize(dynamic value) {
  return _parseEnumValue(value, MaterialTapTargetSize.values);
}

HitTestBehavior? parseHitTestBehavior(dynamic value) {
  return _parseEnumValue(value, HitTestBehavior.values);
}

FlexFit? parseFlexFit(dynamic value) {
  return _parseEnumValue(value, FlexFit.values);
}

ScrollViewKeyboardDismissBehavior? parseScrollViewKeyboardDismissBehavior(
    dynamic value) {
  return _parseEnumValue(value, ScrollViewKeyboardDismissBehavior.values);
}

ImageRepeat? parseImageRepeat(String imageRepeatString) {
  switch (imageRepeatString) {
    case 'repeat':
      return ImageRepeat.repeat;
    case 'repeatX':
      return ImageRepeat.repeatX;
    case 'repeatY':
      return ImageRepeat.repeatY;
    case 'noRepeat':
      return ImageRepeat.noRepeat;

    default:
      return ImageRepeat.noRepeat;
  }
}

Rect parseRect(String fromLTRBString) {
  var strings = fromLTRBString.split(',');
  return Rect.fromLTRB(double.parse(strings[0]), double.parse(strings[1]),
      double.parse(strings[2]), double.parse(strings[3]));
}

FilterQuality? parseFilterQuality(String filterQualityString) {
  switch (filterQualityString) {
    case 'none':
      return FilterQuality.none;
    case 'low':
      return FilterQuality.low;
    case 'medium':
      return FilterQuality.medium;
    case 'high':
      return FilterQuality.high;
    default:
      return FilterQuality.low;
  }
}

String? getLoadMoreUrl(String? url, int currentNo, int pageSize) {
  if (url == null) {
    return null;
  }

  url = url.trim();
  if (url.contains("?")) {
    url = url +
        "&startNo=" +
        currentNo.toString() +
        "&pageSize=" +
        pageSize.toString();
  } else {
    url = url +
        "?startNo=" +
        currentNo.toString() +
        "&pageSize=" +
        pageSize.toString();
  }
  return url;
}

StackFit? parseStackFit(String value) {
  switch (value) {
    case 'loose':
      return StackFit.loose;
    case 'expand':
      return StackFit.expand;
    case 'passthrough':
      return StackFit.passthrough;
    default:
      return StackFit.loose;
  }
}

Clip? parseClip(dynamic value) {
  return _parseEnumValue(
    value,
    Clip.values,
    aliases: const {'visible': 'none'},
  );
}

Axis parseAxis(String axisString) {
  switch (axisString) {
    case "horizontal":
      return Axis.horizontal;
    case "vertical":
      return Axis.vertical;
  }
  return Axis.horizontal;
}

//WrapAlignment
WrapAlignment parseWrapAlignment(String wrapAlignmentString) {
  switch (wrapAlignmentString) {
    case "start":
      return WrapAlignment.start;
    case "end":
      return WrapAlignment.end;
    case "center":
      return WrapAlignment.center;
    case "spaceBetween":
      return WrapAlignment.spaceBetween;
    case "spaceAround":
      return WrapAlignment.spaceAround;
    case "spaceEvenly":
      return WrapAlignment.spaceEvenly;
  }
  return WrapAlignment.start;
}

//WrapCrossAlignment
WrapCrossAlignment parseWrapCrossAlignment(String wrapCrossAlignmentString) {
  switch (wrapCrossAlignmentString) {
    case "start":
      return WrapCrossAlignment.start;
    case "end":
      return WrapCrossAlignment.end;
    case "center":
      return WrapCrossAlignment.center;
  }

  return WrapCrossAlignment.start;
}

Clip? parseClipBehavior(String clipBehaviorString) {
  switch (clipBehaviorString) {
    case "antiAlias":
      return Clip.antiAlias;
    case "none":
      return Clip.none;
    case "hardEdge":
      return Clip.hardEdge;
    case "antiAliasWithSaveLayer":
      return Clip.antiAliasWithSaveLayer;
  }
  return Clip.antiAlias;
}

DropCapMode? parseDropCapMode(String value) {
  switch (value) {
    case 'inside':
      return DropCapMode.inside;
    case 'upwards':
      return DropCapMode.upwards;
    case 'aside':
      return DropCapMode.aside;
    default:
      return DropCapMode.inside;
  }
}

DropCapPosition? parseDropCapPosition(String value) {
  switch (value) {
    case 'start':
      return DropCapPosition.start;
    case 'end':
      return DropCapPosition.end;
    default:
      return DropCapPosition.start;
  }
}

DropCap parseDropCap(Map<String, dynamic> map, BuildContext buildContext) {
  return DropCap(
    width: map['width'],
    height: map['height'],
    child: DynamicWidgetBuilder.buildFromMap(map["child"], buildContext),
  );
}

BoxDecoration? parseBoxDecoration(Map<String, dynamic> map) {
  return BoxDecoration(
    color: parseColor(map['color']),
    border: parseBorder(map['border']),
  );
}

Border? parseBorder(dynamic jsonValue) {
  if (jsonValue == null) return null;
  final color = parseColor(jsonValue['color']);
  if (color == null) return null;
  return Border.all(color: color, width: jsonValue['width']);
}

String? parseString(Pointer<Utf8> input) {
  if (input.address == 0) return null;
  return input.toDartString();
}

/// Placeholder parser for types that couldn't be mapped during code generation.
/// This allows the code to compile while specific parsers are being implemented.
/// TODO: Replace calls to parseInvalidType with proper parsers for each type.
dynamic parseInvalidType(dynamic input) {
  // Log a warning in debug mode to help identify which types need implementation
  assert(() {
    print(
        'WARNING: parseInvalidType called - type mapping needed for input: $input');
    return true;
  }());
  return null;
}

/// Placeholder parser for Widget types that couldn't be directly parsed.
/// Note: This is a stub that returns null. Actual widget parsing should use
/// DynamicWidgetBuilder.buildFromPointer with proper BuildContext.
Widget? parseWidget(dynamic input) {
  // Widget parsing requires BuildContext which is not available in utility functions.
  // This stub returns null - actual widget parsing should be done in the parser.
  return null;
}

// Note: parseTextStyle is defined earlier in this file (line ~147) with Map<String, dynamic> signature.
// For FFI struct-based parsing, use the function above or extend it to handle TextStyleStruct.

/// Placeholder parser for ScrollController - returns null for now.
ScrollController? parseScrollController(dynamic input) {
  // TODO: Implement proper ScrollController parsing
  return null;
}

/// Placeholder parser for Curve - returns linear for now.
Curve parseCurve(dynamic input) {
  // TODO: Implement proper Curve parsing from enum/struct
  return Curves.linear;
}

/// Placeholder parser for generic object types.
dynamic parseObject(dynamic input) {
  return input;
}

/// Placeholder parser for dynamic types.
dynamic parsedynamic(dynamic input) {
  return input;
}

/// Placeholder parser for BoxShape enum.
BoxShape parseBoxShape(dynamic input) {
  if (input is int) {
    return BoxShape.values[input.clamp(0, BoxShape.values.length - 1)];
  }
  return BoxShape.rectangle;
}

// ============================================================================
// Additional placeholder parsers for types that need proper implementation
// These stubs allow code to compile while specific parsers are developed.
// ============================================================================

/// Placeholder for Action type.
dynamic parseAction(dynamic input) => null;

/// Placeholder for ActionDispatcher type.
dynamic parseActionDispatcher(dynamic input) => null;

/// Placeholder for BoxBorder type.
BoxBorder? parseBoxBorder(dynamic input) => null;

/// Placeholder for DraggableScrollableController.
DraggableScrollableController? parseDraggableScrollableController(
        dynamic input) =>
    null;

/// Placeholder for ExpansibleController (custom type).
dynamic parseExpansibleController(dynamic input) => null;

/// Placeholder for GlobalKey.
GlobalKey? parseGlobalKey(dynamic input) => null;

/// Placeholder for IconThemeData.
IconThemeData? parseIconThemeData(dynamic input) => null;

/// Placeholder for ListWheelChildDelegate.
ListWheelChildDelegate? parseListWheelChildDelegate(dynamic input) => null;

/// Placeholder for MagnifierDecoration.
dynamic parseMagnifierDecoration(dynamic input) => null;

/// Placeholder for MenuController.
MenuController? parseMenuController(dynamic input) => null;

/// Placeholder for OverlayPortalController.
OverlayPortalController? parseOverlayPortalController(dynamic input) => null;

/// Placeholder for PageController.
PageController? parsePageController(dynamic input) => null;

/// Placeholder for PageStorageBucket.
PageStorageBucket? parsePageStorageBucket(dynamic input) => null;

/// Placeholder for RouteInformationProvider.
RouteInformationProvider? parseRouteInformationProvider(dynamic input) => null;

/// Placeholder for RouterDelegate.
RouterDelegate<dynamic>? parseRouterDelegate(dynamic input) => null;

/// Placeholder for SelectionContainerDelegate.
dynamic parseSelectionContainerDelegate(dynamic input) => null;

/// Placeholder for SemanticsGestureDelegate.
dynamic parseSemanticsGestureDelegate(dynamic input) => null;

/// Placeholder for SliverChildDelegate.
SliverChildDelegate? parseSliverChildDelegate(dynamic input) => null;

/// Placeholder for SliverPersistentHeaderDelegate.
SliverPersistentHeaderDelegate? parseSliverPersistentHeaderDelegate(
        dynamic input) =>
    null;

/// Placeholder for generic type T.
dynamic parseT(dynamic input) => input;

/// Placeholder for TextEditingController.
TextEditingController? parseTextEditingController(dynamic input) => null;

/// Placeholder for TreeSliverController.
dynamic parseTreeSliverController(dynamic input) => null;

/// Placeholder for UndoHistoryController.
UndoHistoryController? parseUndoHistoryController(dynamic input) => null;

/// Placeholder for WebImageInfo.
dynamic parseWebImageInfo(dynamic input) => null;
