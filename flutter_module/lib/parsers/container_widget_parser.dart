import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import '../utils.dart' show parseAlignment;
import '../generated/structs/edgeinsetsgeometry_struct.dart';
import '../generated/generated_utility_parsers.dart';
import 'package:flutter/widgets.dart';

class ContainerWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ContainerStruct>.fromAddress(fos.handle.address).ref;
    Alignment alignment = map.hasAlignment == 1
        ? parseAlignment(map.alignment.cast<AlignmentStruct>().ref)
        : Alignment.center;
    Color? color = map.color != 0 ? Color(map.color) : null;
    //TODO: Bring back
    BoxConstraints? constraints =
        null; //parseBoxConstraints(map['constraints']);
    //TODO: decoration, foregroundDecoration and transform properties to be implemented.
    Decoration? decoration = null; //parseBoxDecoration(map['decoration']);
    EdgeInsetsGeometry? margin = map.hasMargin == 1
        ? parseEdgeInsetsGeometry(map.margin.cast<EdgeInsetsGeometryStruct>().ref)
        : null;
    EdgeInsetsGeometry? padding = map.hasPadding == 1
        ? parseEdgeInsetsGeometry(map.padding.cast<EdgeInsetsGeometryStruct>().ref)
        : null;
    var childMap = map.child;
    Widget? child = childMap == null
        ? null
        : DynamicWidgetBuilder.buildFromPointer(childMap, buildContext);

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
