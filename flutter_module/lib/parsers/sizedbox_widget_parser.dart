import 'dart:ffi';

import 'package:flutter_module/flutter_sharp_structs.dart';

import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

//Creates a box that will become as large as its parent allows.
class ExpandedSizedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return SizedBox.expand(
      child: DynamicWidgetBuilder.buildFromMap(
          map.child.ref, buildContext),
    );
  }

  @override
  String get widgetName => "ExpandedSizedBox";
}

class SizedBoxWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    // return SizedBox(
    //   width: map["width"],
    //   height: map["height"],
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map.child.ref, buildContext),
    // );
  }

  @override
  String get widgetName => "SizedBox";
}
