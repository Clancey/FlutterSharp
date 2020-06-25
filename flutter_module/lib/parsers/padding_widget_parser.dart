import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class PaddingWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Padding(
      padding: map.containsKey("padding")
          ? parseEdgeInsetsGeometry(map["padding"])
          : null,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "Padding";
}
