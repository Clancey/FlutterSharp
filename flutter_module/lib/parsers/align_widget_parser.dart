import 'dart:ffi';

import 'package:flutter/widgets.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class AlignWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<AlignStruct>.fromAddress(fos.handle.address).ref;
    return Align(
      // Use pointer address != 0 check for pointer fields
      alignment: map.alignment.address != 0
          ? parseAlignment(map.alignment.cast<AlignmentStruct>().ref)
          : Alignment.center,
      // Use has flags for nullable value types
      widthFactor: map.hasWidthFactor == 1 ? map.widthFactor : null,
      heightFactor: map.hasHeightFactor == 1 ? map.heightFactor : null,
      child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext),
    );
  }

  @override
  String get widgetName => "Align";
}
