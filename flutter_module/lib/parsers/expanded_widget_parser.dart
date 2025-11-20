import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class ExpandedWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(
            fos.handle.address)
        .ref;
    //TODO: Implement
    final child = DynamicWidgetBuilder.buildFromPointer(map.child, buildContext);
    if (child == null) return null;
    return Expanded(
      child: child,
      flex: 1, // map.containsKey("flex") ? map["flex"] : 1,
    );
  }

  @override
  String get widgetName => "Expanded";
}
