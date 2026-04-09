import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import 'package:flutter/material.dart';

class IconWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    //TODO: Implement!
    return Icon(
      // map.containsKey('data')
      //     ? getIconUsingPrefix(name: map['data'])
      //     :
      Icons.android,
      // size: map.containsKey("size") ? map['size'] : null,
      // color: map.containsKey('color') ? parseHexColor(map['color']) : null,
      // semanticLabel:
      //     map.containsKey('semanticLabel') ? map['semanticLabel'] : null,
      // textDirection: map.containsKey('textDirection')
      //     ? parseTextDirection(map['textDirection'])
      //     : null,
    );
  }

  @override
  String get widgetName => "Icon";
}
