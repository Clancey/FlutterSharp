import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class ContainerWidgetParser extends WidgetParser {
  @override
   Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ContainerStruct>.fromAddress(fos.handle.address).ref;
    Alignment alignment = map.hasAlignment == 1 ? parseAlignment(map.alignment.ref) : Alignment.center;
    Color color = map.hasColor == 1 ?  parseColor(map.color.ref) : null;
    //TODO: Bring back
    BoxConstraints constraints =  null;//parseBoxConstraints(map['constraints']);
    //TODO: decoration, foregroundDecoration and transform properties to be implemented.
    Decoration decoration = null; //parseBoxDecoration(map['decoration']);
    EdgeInsetsGeometry margin = parseEdgeInsetsGeometry(map.hasMargin, map.margin.ref);
    EdgeInsetsGeometry padding = parseEdgeInsetsGeometry(map.hasPadding,map.padding.ref);
    var childMap = map.child;
    Widget child = childMap == null
        ? null
        : DynamicWidgetBuilder.buildFromMap(childMap.ref, buildContext);

    var containerWidget = Container(
      alignment: alignment,
      padding: padding,
      color: color,
      decoration: decoration,
      margin: margin,
      width: map.hasWidth == 1 ? map.width : null,
      height: map.hasHeight == 1 ? map.height : null,
      constraints: constraints,
      child: child,
    );

    // if (clickEvent != null) {
    //   return GestureDetector(
    //     onTap: () {
    //         //TODO: On Tap
    //     },
    //     child: containerWidget,
    //   );
    // } else {
      return containerWidget;
    // }
  }

  @override
  String get widgetName => "Container";
}
