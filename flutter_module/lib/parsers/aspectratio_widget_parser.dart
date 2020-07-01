import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class AspectRatioWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<AspectRatioStruct>.fromAddress(fos.handle.address).ref;
    return AspectRatio(
      aspectRatio: map.value,
      child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext),
    );
  }

  @override
  String get widgetName => "AspectRatio";
}
