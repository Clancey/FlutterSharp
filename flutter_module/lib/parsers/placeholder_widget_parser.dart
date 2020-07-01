import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class PlaceholderWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: Implement
    // return Placeholder(
    //   color: map.containsKey('color')
    //       ? parseHexColor(map['color'])
    //       : const Color(0xFF455A64),
    //   strokeWidth: map.containsKey('strokeWidth') ? map['strokeWidth'] : 2.0,
    //   fallbackWidth:
    //       map.containsKey('fallbackWidth') ? map['fallbackWidth'] : 400.0,
    //   fallbackHeight:
    //       map.containsKey('fallbackHeight') ? map['fallbackHeight'] : 400.0,
    // );
  }

  @override
  String get widgetName => "Placeholder";
}
