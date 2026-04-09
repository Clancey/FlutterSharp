import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import '../utils.dart' show parseAlignment;
import '../generated/generated_utility_parsers.dart'
    show parseEdgeInsetsGeometry;
import 'package:flutter/widgets.dart';

class ContainerWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ContainerStruct>.fromAddress(fos.handle.address).ref;
    // Use pointer address check for pointer fields
    Alignment alignment = map.alignment.address != 0
        ? parseAlignment(map.alignment.cast<AlignmentStruct>().ref)
        : Alignment.center;
    Color? color = map.color != 0 ? Color(map.color) : null;
    //TODO: Bring back
    BoxConstraints? constraints =
        null; //parseBoxConstraints(map['constraints']);
    //TODO: decoration, foregroundDecoration and transform properties to be implemented.
    Decoration? decoration = null; //parseBoxDecoration(map['decoration']);
    // Use pointer address check for pointer fields
    EdgeInsetsGeometry? margin = map.margin.address != 0
        ? parseEdgeInsetsGeometry(map.margin.ref)
        : null;
    EdgeInsetsGeometry? padding = map.padding.address != 0
        ? parseEdgeInsetsGeometry(map.padding.ref)
        : null;
    var childMap = map.child;
    Widget? child = childMap.address == 0
        ? null
        : DynamicWidgetBuilder.buildFromPointer(childMap, buildContext);

    var containerWidget = Container(
      alignment: alignment,
      padding: padding,
      color: color,
      decoration: decoration,
      margin: margin,
      // Use has flags for nullable value types
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
