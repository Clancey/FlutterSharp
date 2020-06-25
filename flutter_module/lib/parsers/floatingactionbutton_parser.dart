import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class FloatingActionButtonParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    final id = map["id"];
    return FloatingActionButton(
        tooltip: map["tooltip"],
        onPressed: () { 
            raiseMauiEvent(id,"onPressed",null);
         },
        child: DynamicWidgetBuilder.buildFromMap(map['child'], buildContext));
  }

  @override
  String get widgetName => "FloatingActionButton";
}
