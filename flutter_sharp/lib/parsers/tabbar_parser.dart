import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class TabBarParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return TabBar(tabs: DynamicWidgetBuilder.buildWidgets(map['tabs'], buildContext),);
  }

  @override
  String get widgetName => "TabBar";
}
