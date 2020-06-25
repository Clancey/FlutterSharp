import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class OpacityWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Opacity(
      opacity: map["opacity"],
      alwaysIncludeSemantics: map.containsKey("alwaysIncludeSemantics")
          ? map["alwaysIncludeSemantics"]
          : false,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "Opacity";
}
