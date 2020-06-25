import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class AppBarParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return AppBar(
        title: DynamicWidgetBuilder.buildFromMap(map['title'], buildContext),
        bottom: DynamicWidgetBuilder.buildFromMap(map['bottom'], buildContext));
  }

  @override
  String get widgetName => "AppBar";
}
