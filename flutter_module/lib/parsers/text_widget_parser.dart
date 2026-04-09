import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import 'package:flutter_module/generated_utility_parsers.dart';

import '../utils.dart' as utils;
import '../maui_flutter.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/widgets.dart';

class TextWidgetParser implements WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<TextStruct>.fromAddress(fos.handle.address).ref;

    // Extract text data (required)
    String? data = map.hasData == 1 && map.data.address != 0
        ? map.data.toDartString()
        : null;

    // Extract optional properties from struct
    TextAlign? textAlign = map.hasTextAlign == 1
        ? utils.parseTextAlignFromInt(map.textAlign)
        : null;
    TextOverflow? overflow = map.hasOverflow == 1
        ? utils.parseTextOverflowFromInt(map.overflow)
        : null;
    TextDirection? textDirection = map.hasTextDirection == 1
        ? utils.parseTextDirectionFromInt(map.textDirection)
        : null;
    TextWidthBasis? textWidthBasis = map.hasTextWidthBasis == 1
        ? utils.parseTextWidthBasisFromInt(map.textWidthBasis)
        : null;

    int? maxLines = map.hasMaxLines == 1 ? map.maxLines : null;
    bool? softWrap = map.hasSoftWrap == 1 ? (map.softWrap == 1) : null;
    double? textScaleFactor =
        map.hasTextScaleFactor == 1 ? map.textScaleFactor : null;

    String? semanticsLabel =
        map.hasSemanticsLabel == 1 && map.semanticsLabel.address != 0
            ? map.semanticsLabel.toDartString()
            : null;

    // Parse TextStyle if provided
    TextStyle? textStyle =
        map.style.address != 0 ? parseTextStyleFromStruct(map.style.ref) : null;

    // Note: textSpan (rich text) support would require additional struct/parser work
    // For now, we use simple Text widget

    return Text(
      data ?? "",
      textAlign: textAlign,
      overflow: overflow,
      maxLines: maxLines,
      semanticsLabel: semanticsLabel,
      softWrap: softWrap,
      textDirection: textDirection,
      style: textStyle,
      textWidthBasis: textWidthBasis,
      // textScaleFactor is deprecated in favor of textScaler, but keep for compatibility
      // ignore: deprecated_member_use
      textScaleFactor: textScaleFactor,
    );
  }

  @override
  String get widgetName => "Text";
}

class TextSpanParser {
  TextSpan parse(Map<String, dynamic> map) {
    var textSpan = TextSpan(
        text: map['value'],
        style: utils.parseTextStyle(map['style']),
        recognizer: TapGestureRecognizer()
          ..onTap = () {
            //TODO: On Tap
          },
        children: []);

    if (map.containsKey('children')) {
      parseChildren(textSpan, map['children']);
    }

    return textSpan;
  }

  void parseChildren(TextSpan textSpan, List<dynamic> childrenSpan) {
    for (var childmap in childrenSpan) {
      textSpan.children?.add(parse(childmap));
    }
  }
}
