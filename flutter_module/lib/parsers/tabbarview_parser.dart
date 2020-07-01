import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class TabBarViewParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MultiChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return TabBarView(children: DynamicWidgetBuilder.buildWidgets(map.children.ref, buildContext),);
  }

  @override
  String get widgetName => "TabBarView";
}
