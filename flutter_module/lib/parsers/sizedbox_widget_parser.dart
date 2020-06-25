import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

//Creates a box that will become as large as its parent allows.
class ExpandedSizedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return SizedBox.expand(
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "ExpandedSizedBox";
}

class SizedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return SizedBox(
      width: map["width"],
      height: map["height"],
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "SizedBox";
}
