import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class FittedBoxWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    return null;
    //TODO: Implement
    // return FittedBox(
    //   alignment: map.containsKey("alignment")
    //       ? parseAlignment(map["alignment"])
    //       : Alignment.center,
    //   fit: map.containsKey("fit") ? parseBoxFit(map["fit"]) : BoxFit.contain,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "FittedBox";
}
