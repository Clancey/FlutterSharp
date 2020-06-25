import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class RowWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Row(
      crossAxisAlignment: map.containsKey('crossAxisAlignment')
          ? parseCrossAxisAlignment(map['crossAxisAlignment'])
          : CrossAxisAlignment.center,
      mainAxisAlignment: map.containsKey('mainAxisAlignment')
          ? parseMainAxisAlignment(map['mainAxisAlignment'])
          : MainAxisAlignment.start,
      mainAxisSize: map.containsKey('mainAxisSize')
          ? parseMainAxisSize(map['mainAxisSize'])
          : MainAxisSize.max,
      textBaseline: map.containsKey('textBaseline')
          ? parseTextBaseline(map['textBaseline'])
          : null,
      textDirection: map.containsKey('textDirection')
          ? parseTextDirection(map['textDirection'])
          : null,
      verticalDirection: map.containsKey('verticalDirection')
          ? parseVerticalDirection(map['verticalDirection'])
          : VerticalDirection.down,
      children: DynamicWidgetBuilder.buildWidgets(
          map['children'], buildContext),
    );
  }

  @override
  String get widgetName => "Row";
}

class ColumnWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    var children = DynamicWidgetBuilder.buildWidgets(
          map['children'], buildContext);
    return Column(
      crossAxisAlignment: map.containsKey('crossAxisAlignment')
          ? parseCrossAxisAlignment(map['crossAxisAlignment'])
          : CrossAxisAlignment.center,
      mainAxisAlignment: map.containsKey('mainAxisAlignment')
          ? parseMainAxisAlignment(map['mainAxisAlignment'])
          : MainAxisAlignment.start,
      mainAxisSize: map.containsKey('mainAxisSize')
          ? parseMainAxisSize(map['mainAxisSize'])
          : MainAxisSize.max,
      textBaseline: map.containsKey('textBaseline')
          ? parseTextBaseline(map['textBaseline'])
          : null,
      textDirection: map.containsKey('textDirection')
          ? parseTextDirection(map['textDirection'])
          : null,
      verticalDirection: map.containsKey('verticalDirection')
          ? parseVerticalDirection(map['verticalDirection'])
          : VerticalDirection.down,
      children: children ,
    );
  }

  @override
  String get widgetName => "Column";
}
