import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class StatefulWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return DynamicWidgetBuilder.buildMauiComponenet(map, buildContext);
  }

  @override
  String get widgetName => "StatefulWidget";
}



