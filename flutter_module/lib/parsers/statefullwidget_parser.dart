import 'dart:ffi';

import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class StatefulWidgetParser extends WidgetParser {
  @override
  Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return DynamicWidgetBuilder.buildMauiComponenet(map.child.ref, buildContext);
  }

  @override
  String get widgetName => "StatefulWidget";
}



