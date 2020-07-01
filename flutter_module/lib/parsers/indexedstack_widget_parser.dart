import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class IndexedStackWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    // return IndexedStack(
    //   index: map.containsKey("index") ? map["index"] : 0,
    //   alignment: map.containsKey("alignment")
    //       ? parseAlignment(map["alignment"])
    //       : AlignmentDirectional.topStart,
    //   textDirection: map.containsKey("textDirection")
    //       ? parseTextDirection(map["textDirection"])
    //       : null,
    //   children: DynamicWidgetBuilder.buildWidgets(
    //       map['children'], buildContext),
    // );
  }

  @override
  String get widgetName => "IndexedStack";
}
