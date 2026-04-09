import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';

class ScaffoldParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ScaffoldStruct>.fromAddress(fos.handle.address).ref;
    final appBar =
        DynamicWidgetBuilder.buildFromPointer(map.appBar, buildContext);
    return Scaffold(
      appBar: appBar is PreferredSizeWidget? ? appBar : null,
      body: DynamicWidgetBuilder.buildFromPointer(map.body, buildContext),
      floatingActionButton: DynamicWidgetBuilder.buildFromPointer(
          map.floatingActionButton, buildContext),
      drawer: DynamicWidgetBuilder.buildFromPointer(map.drawer, buildContext),
    );
  }

  @override
  String get widgetName => "Scaffold";
}
