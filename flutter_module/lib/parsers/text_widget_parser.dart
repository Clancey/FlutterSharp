import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';

import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/widgets.dart';

class TextWidgetParser implements WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<TextStruct>.fromAddress(fos.handle.address).ref;
    String data = parseString(map.value);
    String textAlignString = null; //map['textAlign'];
    String overflow = null; //map['overflow'];
    int maxLines = null; //map['maxLines'];
    String semanticsLabel = null; // map['semanticsLabel'];
    bool softWrap = null; //map['softWrap'];
    String textDirectionString = null; //map['textDirection'];
    double textScaleFactor = null; // map['textScaleFactor'];
    var textSpan;
    var textSpanParser = TextSpanParser();
    // if (map.containsKey("textSpan")) {
    //   textSpan = textSpanParser.parse(map['textSpan']);
    // }

    if (textSpan == null) {
      return Text(
        data,
        textAlign: parseTextAlign(textAlignString),
        overflow: parseTextOverflow(overflow),
        maxLines: maxLines,
        semanticsLabel: semanticsLabel,
        softWrap: softWrap,
        textDirection: parseTextDirection(textDirectionString),
        // style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
        textScaleFactor: textScaleFactor,
      );
    } else {
      return Text.rich(
        textSpan,
        textAlign: parseTextAlign(textAlignString),
        overflow: parseTextOverflow(overflow),
        maxLines: maxLines,
        semanticsLabel: semanticsLabel,
        softWrap: softWrap,
        textDirection: parseTextDirection(textDirectionString),
        // style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
        textScaleFactor: textScaleFactor,
      );
    }
  }

  @override
  String get widgetName => "Text";
}

class TextSpanParser {
  TextSpan parse(Map<String, dynamic> map) {
    String clickEvent = map.containsKey("recognizer") ? map['recognizer'] : "";
    var textSpan = TextSpan(
        text: map['value'],
        style: parseTextStyle(map['style']),
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
      textSpan.children.add(parse(childmap));
    }
  }
}
