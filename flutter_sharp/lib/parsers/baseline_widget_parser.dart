
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class BaselineWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Baseline(
      baseline: map["baseline"],
      baselineType: map["baselineType"] == "alphabetic"
          ? TextBaseline.alphabetic
          : TextBaseline.ideographic,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "Baseline";
}
