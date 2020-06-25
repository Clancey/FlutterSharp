import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class AlignWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Align(
      alignment: map.containsKey("alignment")
          ? parseAlignment(map["alignment"])
          : Alignment.center,
      widthFactor: map.containsKey("widthFactor") ? map["widthFactor"] : null,
      heightFactor:
          map.containsKey("heightFactor") ? map["heightFactor"] : null,
      child: DynamicWidgetBuilder.buildFromMap(
          map["child"], buildContext),
    );
  }

  @override
  String get widgetName => "Align";
}
