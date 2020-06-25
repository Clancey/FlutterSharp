import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class DrawerParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Drawer(
        child: DynamicWidgetBuilder.buildFromMap(map['child'], buildContext));
  }

  @override
  String get widgetName => "Drawer";
}
