import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class SafeAreaWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<RowStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: implement
    // var left = map.containsKey("left") ? map["left"] : true;
    // var right = map.containsKey("right") ? map["right"] : true;
    // var top = map.containsKey("top") ? map["top"] : true;
    // var bottom = map.containsKey("bottom") ? map["bottom"] : true;
    // var edgeInsets = map.containsKey("minimum")
    //     ? parseEdgeInsetsGeometry(map['minimum'])
    //     : EdgeInsets.zero;
    // var maintainBottomViewPadding = map.containsKey("maintainBottomViewPadding")
    //     ? map["maintainBottomViewPadding"]
    //     : false;
    // return SafeArea(
    //   left: left,
    //   right: right,
    //   top: top,
    //   bottom: bottom,
    //   minimum: edgeInsets,
    //   maintainBottomViewPadding: maintainBottomViewPadding,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "SafeArea";
}
