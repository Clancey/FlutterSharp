import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class CenterWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Center(
      widthFactor: map.containsKey("widthFactor") ? map["widthFactor"] : null,
      heightFactor:
          map.containsKey("heightFactor") ? map["heightFactor"] : null,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "Center";
}
