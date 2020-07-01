import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class AppBarParser extends WidgetParser {
  @override
 Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<AppBarStruct>.fromAddress(fos.handle.address).ref;
    return AppBar(
        title: DynamicWidgetBuilder.buildFromMap(map.title.ref, buildContext),
        bottom: DynamicWidgetBuilder.buildFromMap(map.bottom.ref, buildContext));
  }

  @override
  String get widgetName => "AppBar";
}
