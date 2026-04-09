import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';

class AppBarParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<AppBarStruct>.fromAddress(fos.handle.address).ref;
    final bottom =
        DynamicWidgetBuilder.buildFromPointer(map.bottom, buildContext);
    return AppBar(
        title: DynamicWidgetBuilder.buildFromPointer(map.title, buildContext),
        bottom: bottom is PreferredSizeWidget? ? bottom : null);
  }

  @override
  String get widgetName => "AppBar";
}
