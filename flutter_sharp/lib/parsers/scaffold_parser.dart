import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class ScaffoldParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    return Scaffold( appBar: DynamicWidgetBuilder.buildFromMap(map['appBar'], buildContext),
          body: DynamicWidgetBuilder.buildFromMap(map['body'], buildContext),
          floatingActionButton: DynamicWidgetBuilder.buildFromMap(map['floatingActionButton'], buildContext),
          drawer: DynamicWidgetBuilder.buildFromMap(map['drawer'], buildContext),
        );
  }

  @override
  String get widgetName => "Scaffold";
}
