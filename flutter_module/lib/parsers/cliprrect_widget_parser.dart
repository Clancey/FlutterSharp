import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class ClipRRectWidgetParser extends WidgetParser {
  @override
   Widget parse(FlutterObjectStruct fos, BuildContext buildContext) {
     return null;
     //TODO: implement
    // var map = Pointer<AspectRatioStruct>.fromAddress(fos.handle.address).ref;
    // var radius = map['borderRadius'].toString().split(",");
    // double topLeft = double.parse(radius[0]);
    // double topRight = double.parse(radius[1]);
    // double bottomLeft = double.parse(radius[2]);
    // double bottomRight = double.parse(radius[3]);
    // var clipBehaviorString = map['clipBehavior'];
    // return ClipRRect(
    //   borderRadius: BorderRadius.only(
    //       topLeft: Radius.circular(topLeft),
    //       topRight: Radius.circular(topRight),
    //       bottomLeft: Radius.circular(bottomLeft),
    //       bottomRight: Radius.circular(bottomRight)),
    //   clipBehavior: parseClipBehavior(clipBehaviorString),
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map["child"], buildContext),
    // );
  }

  @override
  String get widgetName => "ClipRRect";
}
