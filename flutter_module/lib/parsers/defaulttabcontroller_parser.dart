import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class DefaultTabControllerParser extends WidgetParser {
  @override
   Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<DefaultTabControllerStruct>.fromAddress(fos.handle.address).ref;
    return DefaultTabController(
        length: map.tabCount,
        child: DynamicWidgetBuilder.buildFromMap(map.child.ref, buildContext));
  }

  @override
  String get widgetName => "DefaultTabController";
}
