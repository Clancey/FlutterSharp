import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class ListViewBuilderParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    final id = map["id"];
    return ListView.builder(
      itemCount: map["itemCount"],
      itemBuilder: (c, index) {
        return FutureBuilder(
            future: requestMauiData(id, "itemBuilder", index),
            builder: (BuildContext context, AsyncSnapshot snapshot) {
              if (snapshot.hasData)
                return DynamicWidgetBuilder.buildFromJson(
                    snapshot.data, context);
              return Text("...");
            });
      },
    );
  }

  @override
  String get widgetName => "ListViewBuilder";
}
