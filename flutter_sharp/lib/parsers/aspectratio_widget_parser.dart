
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class AspectRatioWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return AspectRatio(
      aspectRatio: map["aspectRatio"],
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "AspectRatio";
}
