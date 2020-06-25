import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/widgets.dart';

class TextWidgetParser implements WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    String data = map['value'];
    String textAlignString = map['textAlign'];
    String overflow = map['overflow'];
    int maxLines = map['maxLines'];
    String semanticsLabel = map['semanticsLabel'];
    bool softWrap = map['softWrap'];
    String textDirectionString = map['textDirection'];
    double textScaleFactor = map['textScaleFactor'];
    var textSpan;
    var textSpanParser = TextSpanParser();
    if (map.containsKey("textSpan")) {
      textSpan = textSpanParser.parse(map['textSpan']);
    }

    if (textSpan == null) {
      return Text(
        data,
        textAlign: parseTextAlign(textAlignString),
        overflow: parseTextOverflow(overflow),
        maxLines: maxLines,
        semanticsLabel: semanticsLabel,
        softWrap: softWrap,
        textDirection: parseTextDirection(textDirectionString),
        style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
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
        style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
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

  void parseChildren(
      TextSpan textSpan, List<dynamic> childrenSpan) {
    for (var childmap in childrenSpan) {
      textSpan.children.add(parse(childmap));
    }
  }
}
