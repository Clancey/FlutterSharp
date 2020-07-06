import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class PositionedWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement
    // return Positioned(
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    //   top: map.containsKey("top") ? map["top"] : null,
    //   right: map.containsKey("right") ? map["right"] : null,
    //   bottom: map.containsKey("bottom") ? map["bottom"] : null,
    //   left: map.containsKey("left") ? map["left"] : null,
    //   width: map.containsKey("width") ? map["width"] : null,
    //   height: map.containsKey("height") ? map["height"] : null,
    // );
  }

  @override
  String get widgetName => "Positioned";
}

class StackWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement;
    // return Stack(
    //   alignment: map.containsKey("alignment")
    //       ? parseAlignment(map["alignment"])
    //       : AlignmentDirectional.topStart,
    //   textDirection: map.containsKey("textDirection")
    //       ? parseTextDirection(map["textDirection"])
    //       : null,
    //   fit: map.containsKey("fit") ? parseStackFit(map["fit"]) : StackFit.loose,
    //   overflow: map.containsKey("overflow")
    //       ? parseOverflow(map["overflow"])
    //       : Overflow.clip,
    //   children: DynamicWidgetBuilder.buildWidgets(
    //       map['children'], buildContext),
    // );
  }

  @override
  String get widgetName => "Stack";
}
