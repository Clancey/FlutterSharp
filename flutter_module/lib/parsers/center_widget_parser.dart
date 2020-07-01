import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class CenterWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    //TODO:Implement

    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    return Center(
      // widthFactor: map.containsKey("widthFactor") ? map["widthFactor"] : null,
      // heightFactor:
      //     map.containsKey("heightFactor") ? map["heightFactor"] : null,
      child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext),
    );
  }

  @override
  String get widgetName => "Center";
}
