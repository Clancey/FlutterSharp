import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class PageViewWidgetParser extends WidgetParser {
  @override
    Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement;
    // var scrollDirection = Axis.vertical;
    // if (map.containsKey("scrollDirection") &&
    //     "horizontal" == map["scrollDirection"]) {
    //   scrollDirection = Axis.horizontal;
    // }
    // return PageView(
    //   scrollDirection: scrollDirection,
    //   reverse: map.containsKey("reverse") ? map["reverse"] : false,
    //   pageSnapping:
    //       map.containsKey("pageSnapping") ? map["pageSnapping"] : true,
    //   children: DynamicWidgetBuilder.buildWidgets(
    //       map['children'], buildContext),
    // );
  }

  @override
  String get widgetName => "PageView";
}
