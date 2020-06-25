import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class TabBarViewParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return TabBarView(children: DynamicWidgetBuilder.buildWidgets(map['children'], buildContext),);
  }

  @override
  String get widgetName => "TabBarView";
}
