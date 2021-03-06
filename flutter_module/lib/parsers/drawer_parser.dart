import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class DrawerParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    //TODO: fill out the drawer more!
    return Drawer(
        child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext));
  }

  @override
  String get widgetName => "Drawer";
}
