
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class BaselineWidgetParser extends WidgetParser {
  @override
 Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
   return null;
   //TODO: Implement
    // var map = Pointer<baseline>.fromAddress(fos.handle.address).ref;
    // return Baseline(
    //   baseline: map["baseline"],
    //   baselineType: map["baselineType"] == "alphabetic"
    //       ? TextBaseline.alphabetic
    //       : TextBaseline.ideographic,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "Baseline";
}
