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
    return Scaffold( appBar: DynamicWidgetBuilder.buildFromMap(map.appBar.ref, buildContext),
          body: DynamicWidgetBuilder.buildFromMap(map.body.ref, buildContext),
          floatingActionButton: DynamicWidgetBuilder.buildFromMap(map.floatingActionButton.ref, buildContext),
          drawer: DynamicWidgetBuilder.buildFromMap(map.drawer.ref, buildContext),
        );
  }

  @override
  String get widgetName => "Scaffold";
}
