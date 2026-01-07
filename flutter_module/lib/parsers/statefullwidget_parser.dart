import 'dart:ffi';

import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart' as mauiFlutter;
import '../maui_flutter.dart' show WidgetParser, DynamicWidgetBuilder;
import 'package:ffi/ffi.dart';

class StatefulWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    // StatefulWidget is abstract and can't be instantiated directly.
    // The actual widget should be handled by its specific parser.
    // If we get here, it means we have a StatefulWidget that needs to be
    // built via its child or a MauiComponent wrapper.
    var map = Pointer<WidgetStruct>.fromAddress(fos.handle.address).ref;
    String? id = parseString(map.id);
    if (id == null) return null;

    // Return a MauiComponent that will handle the stateful widget
    return mauiFlutter.MauiComponent(componentId: id);
  }

  @override
  String get widgetName => "StatefulWidget";
}
