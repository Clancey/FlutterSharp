import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class WrapWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Wrap(
      direction: map.containsKey("direction")
          ? parseAxis(map["direction"])
          : Axis.horizontal,
      alignment: map.containsKey("alignment")
          ? parseWrapAlignment(map["alignment"])
          : WrapAlignment.start,
      spacing: map.containsKey("spacing") ? map["spacing"] : 0.0,
      runAlignment: map.containsKey("runAlignment")
          ? parseWrapAlignment(map["runAlignment"])
          : WrapAlignment.start,
      runSpacing: map.containsKey("runSpacing") ? map["runSpacing"] : 0.0,
      crossAxisAlignment: map.containsKey("crossAxisAlignment")
          ? parseWrapCrossAlignment(map["crossAxisAlignment"])
          : WrapCrossAlignment.start,
      textDirection: map.containsKey("textDirection")
          ? parseTextDirection(map["textDirection"])
          : null,
      verticalDirection: map.containsKey("verticalDirection")
          ? parseVerticalDirection(map["verticalDirection"])
          : VerticalDirection.down,
      children: DynamicWidgetBuilder.buildWidgets(
          map['children'], buildContext),
    );
  }

  @override
  String get widgetName => "Wrap";
}
