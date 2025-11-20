import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class DefaultTabControllerParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<DefaultTabControllerStruct>.fromAddress(fos.handle.address).ref;
    final child = DynamicWidgetBuilder.buildFromPointer(map.child, buildContext);
    if (child == null) return null;
    return DefaultTabController(
        length: map.tabCount,
        child: child);
  }

  @override
  String get widgetName => "DefaultTabController";
}
