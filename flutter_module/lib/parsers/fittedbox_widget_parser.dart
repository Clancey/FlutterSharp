import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class FittedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: Implement
    // return FittedBox(
    //   alignment: map.containsKey("alignment")
    //       ? parseAlignment(map["alignment"])
    //       : Alignment.center,
    //   fit: map.containsKey("fit") ? parseBoxFit(map["fit"]) : BoxFit.contain,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "FittedBox";
}
