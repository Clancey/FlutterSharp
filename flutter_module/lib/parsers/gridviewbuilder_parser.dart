import 'dart:ffi';

import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

class GridViewBuilderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<GridViewBuilderStruct>.fromAddress(fos.handle.address).ref;
    final id = parseString(map.id);
    if (id == null) return null;

    return GridView.builder(
      itemCount: map.itemCount,
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: map.crossAxisCount,
        mainAxisSpacing: map.mainAxisSpacing,
        crossAxisSpacing: map.crossAxisSpacing,
        childAspectRatio: map.childAspectRatio,
      ),
      itemBuilder: (c, index) {
        return FutureBuilder(
            future: requestMauiData(id, "ItemBuilder", index),
            builder: (BuildContext context, AsyncSnapshot snapshot) {
              if (snapshot.hasData) {
                var pointer = int.parse(snapshot.data);
                return DynamicWidgetBuilder.buildFromAddress(
                        pointer, context) ??
                    SizedBox.shrink();
              }
              return SizedBox.shrink();
            });
      },
    );
  }

  @override
  String get widgetName => "GridViewBuilder";
}
