import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import '../maui_flutter.dart';

class TabBarViewParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MultiChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    return TabBarView(
      children: DynamicWidgetBuilder.buildWidgets(
          map.children.cast<ChildrenStruct>(), buildContext),
    );
  }

  @override
  String get widgetName => "TabBarView";
}
