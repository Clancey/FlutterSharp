import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class ExpandedWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Expanded(
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
      flex: map.containsKey("flex") ? map["flex"] : 1,
    );
  }

  @override
  String get widgetName => "Expanded";
}
