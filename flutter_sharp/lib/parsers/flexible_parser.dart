import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class FlexibleParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Flexible(
        child: DynamicWidgetBuilder.buildFromMap(map['child'], buildContext));
  }

  @override
  String get widgetName => "Flexible";
}
