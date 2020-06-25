import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class ContainerWidgetParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    Alignment alignment = parseAlignment(map['alignment']);
    Color color = parseHexColor(map['color']);
    BoxConstraints constraints = parseBoxConstraints(map['constraints']);
    //TODO: decoration, foregroundDecoration and transform properties to be implemented.
    Decoration decoration = parseBoxDecoration(map['decoration']);
    EdgeInsetsGeometry margin = parseEdgeInsetsGeometry(map['margin']);
    EdgeInsetsGeometry padding = parseEdgeInsetsGeometry(map['padding']);
    Map<String, dynamic> childMap = map['child'];
    Widget child = childMap == null
        ? null
        : DynamicWidgetBuilder.buildFromMap(childMap, buildContext);

    String clickEvent =
        map.containsKey("click_event") ? map['click_event'] : "";

    var containerWidget = Container(
      alignment: alignment,
      padding: padding,
      color: color,
      decoration: decoration,
      margin: margin,
      width: map['width'],
      height: map['height'],
      constraints: constraints,
      child: child,
    );

    if (clickEvent != null) {
      return GestureDetector(
        onTap: () {
            //TODO: On Tap
        },
        child: containerWidget,
      );
    } else {
      return containerWidget;
    }
  }

  @override
  String get widgetName => "Container";
}
