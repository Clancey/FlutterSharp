import 'dart:convert';
import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class ListViewBuilderParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<ListViewBuilderStruct>.fromAddress(fos.handle.address).ref;
    final id = parseString(map.id);
    return ListView.builder(
      itemCount: map.itemCount,
      itemBuilder: (c, index) {
        return FutureBuilder(
            future: requestMauiData(id, "ItemBuilder", index),
            builder: (BuildContext context, AsyncSnapshot snapshot) {
              if (snapshot.hasData) {
                var pointer = int.parse(snapshot.data);
                return DynamicWidgetBuilder.buildFromAddress(pointer, context);
              }
              return SizedBox.shrink();
            });
      },
    );
  }

  @override
  String get widgetName => "ListViewBuilder";
}
