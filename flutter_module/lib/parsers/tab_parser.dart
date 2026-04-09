import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class TabParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<TabStruct>.fromAddress(fos.handle.address).ref;
    final child = map.child.address == 0
        ? null
        : DynamicWidgetBuilder.buildFromPointer(map.child, buildContext);
    final text = parseString(map.text);

    if (text == null && child == null) {
      debugPrint(
          '[TabParser] Received a Tab without text or child; using an empty placeholder child.');
      return const Tab(child: SizedBox.shrink());
    }

    return Tab(
      text: text,
      child: child,
    );
  }

  @override
  String get widgetName => "Tab";
}
