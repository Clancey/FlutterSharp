import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../maui_flutter.dart';

class TabParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Tab(child: DynamicWidgetBuilder.buildFromMap(map['child'], buildContext),);
  }

  @override
  String get widgetName => "Tab";
}
