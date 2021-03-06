import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';

class TabParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    return Tab(
      child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext),
    );
  }

  @override
  String get widgetName => "Tab";
}
