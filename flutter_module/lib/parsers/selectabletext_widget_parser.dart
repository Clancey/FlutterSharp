import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';

class SelectableTextWidgetParser implements WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ScaffoldStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement;
//     String data = map['data'];
//     String textAlignString = map['textAlign'];
//     int maxLines = map['maxLines'];
//     String textDirectionString = map['textDirection'];
// //    double textScaleFactor = map['textScaleFactor'];
//     var textSpan;
//     var textSpanParser = SelectableTextSpanParser();
//     if (map.containsKey("textSpan")) {
//       textSpan = textSpanParser.parse(map['textSpan']);
//     }

//     if (textSpan == null) {
//       return SelectableText(
//         data,
//         textAlign: parseTextAlign(textAlignString),
//         maxLines: maxLines,
//         textDirection: parseTextDirection(textDirectionString),
//         style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
// //        textScaleFactor: textScaleFactor,
//       );
//     } else {
//       return SelectableText.rich(
//         textSpan,
//         textAlign: parseTextAlign(textAlignString),
//         maxLines: maxLines,
//         textDirection: parseTextDirection(textDirectionString),
//         style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
// //        textScaleFactor: textScaleFactor,
//       );
//     }
  }

  @override
  String get widgetName => "SelectableText";
}

class SelectableTextSpanParser {
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
