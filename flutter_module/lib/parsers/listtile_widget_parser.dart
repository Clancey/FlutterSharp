import 'dart:ffi';

import 'package:ffi/ffi.dart';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';

import '../utils.dart';

class ListTileWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SingleChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    final id = Utf8.fromUtf8(map.id);
    return null;
    // bool isThreeLine =
    //     map.containsKey("isThreeLine") ? map["isThreeLine"] : false;
    // EdgeInsetsGeometry contentPadding = map.containsKey("contentPadding")
    //     ? parseEdgeInsetsGeometry(map["contentPadding"])
    //     : null;
    // bool dense = map.containsKey("dense") ? map["dense"] : false;
    // bool enabled = map.containsKey("enabled") ? map["enabled"] : true;
    // Widget leading = map.containsKey("leading")
    //     ? DynamicWidgetBuilder.buildFromMap(
    //         map["leading"], buildContext)
    //     : null;
    // bool selected = map.containsKey("selected") ? map["selected"] : false;
    // Widget subtitle = map.containsKey("subtitle")
    //     ? DynamicWidgetBuilder.buildFromMap(
    //         map["subtitle"], buildContext)
    //     : null;
    // Widget title = map.containsKey("title")
    //     ? DynamicWidgetBuilder.buildFromMap(
    //         map["title"], buildContext)
    //     : null;
    // Widget trailing = map.containsKey("trailing")
    //     ? DynamicWidgetBuilder.buildFromMap(
    //         map["trailing"], buildContext)
    //     : null;
    // String tapEvent = map.containsKey("tapEvent") ? map["tapEvent"] : null;

    // return ListTile(
    //   isThreeLine: isThreeLine,
    //   leading: leading,
    //   title: title,
    //   subtitle: subtitle,
    //   trailing: trailing,
    //   dense: dense,
    //   contentPadding: contentPadding,
    //   enabled: enabled,
    //   onTap: () {
    //     //TODO:Send Tap Event
    //   },
    //   selected: selected,
    // );
  }

  @override
  String get widgetName => "ListTile";
}
