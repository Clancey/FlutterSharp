import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class ScaffoldParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ScaffoldStruct>.fromAddress(fos.handle.address).ref;
    return Scaffold(
      appBar: DynamicWidgetBuilder.buildFromPointer(map.appBar, buildContext),
      body: DynamicWidgetBuilder.buildFromPointer(map.body, buildContext),
      floatingActionButton: DynamicWidgetBuilder.buildFromPointer(
          map.floatingActionButton, buildContext),
      drawer: DynamicWidgetBuilder.buildFromPointer(map.drawer, buildContext),
    );
  }

  @override
  String get widgetName => "Scaffold";
}
