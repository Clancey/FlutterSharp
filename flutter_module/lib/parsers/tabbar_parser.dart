import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class TabBarParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MultiChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    // map.children.asTypedList()
    return TabBar(
      tabs: DynamicWidgetBuilder.buildWidgets(map.children, buildContext),
    );
  }

  @override
  String get widgetName => "TabBar";
}
