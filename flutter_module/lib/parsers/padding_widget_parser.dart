import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class PaddingWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement
    // return Padding(
    //   padding: map.containsKey("padding")
    //       ? parseEdgeInsetsGeometry(map["padding"])
    //       : null,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "Padding";
}
