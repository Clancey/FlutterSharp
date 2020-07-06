
import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/material.dart';

class RaisedButtonParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
   // final id = map["id"];
   return null;
  //TODO: implement;
    // var raisedButton = RaisedButton(
    //   color: map.containsKey('color') ? parseHexColor(map['color']) : null,
    //   disabledColor: map.containsKey('disabledColor')
    //       ? parseHexColor(map['disabledColor'])
    //       : null,
    //   disabledElevation:
    //       map.containsKey('disabledElevation') ? map['disabledElevation'] : 0.0,
    //   disabledTextColor: map.containsKey('disabledTextColor')
    //       ? parseHexColor(map['disabledTextColor'])
    //       : null,
    //   elevation: map.containsKey('elevation') ? map['elevation'] : 0.0,
    //   padding: map.containsKey('padding')
    //       ? parseEdgeInsetsGeometry(map['padding'])
    //       : null,
    //   splashColor: map.containsKey('splashColor')
    //       ? parseHexColor(map['splashColor'])
    //       : null,
    //   textColor:
    //       map.containsKey('textColor') ? parseHexColor(map['textColor']) : null,
    //   child: DynamicWidgetBuilder.buildFromMap(
    //       map['child'], buildContext),
    //   onPressed: () {      
    //     raiseMauiEvent(id,"onPressed",null);
    //   },
    // );

    // return raisedButton;
  }

  @override
  String get widgetName => "RaisedButton";
}
