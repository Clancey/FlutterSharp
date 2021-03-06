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
      alignment: map.hasAlignment == 1
          ? parseAlignment(map.alignment.ref)
          : Alignment.center,
      widthFactor: map.hasWidthFactor == 1 ? map.widthFactor : null,
      heightFactor: map.hasHeightFactor == 1 ? map.heightFactor : null,
      child: DynamicWidgetBuilder.buildFromPointer(map.child, buildContext),
    );
  }

  @override
  String get widgetName => "Align";
}
