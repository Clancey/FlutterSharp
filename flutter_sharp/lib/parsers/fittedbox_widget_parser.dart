import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class FittedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return FittedBox(
      alignment: map.containsKey("alignment")
          ? parseAlignment(map["alignment"])
          : Alignment.center,
      fit: map.containsKey("fit") ? parseBoxFit(map["fit"]) : BoxFit.contain,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "FittedBox";
}
