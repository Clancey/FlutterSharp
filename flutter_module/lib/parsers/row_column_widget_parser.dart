import 'dart:ffi';

import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/widgets.dart';

class RowWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<RowStruct>.fromAddress(fos.handle.address).ref;
    //TODO: Fill out more!
    return Row(
      // crossAxisAlignment: map.containsKey('crossAxisAlignment')
      //     ? parseCrossAxisAlignment(map['crossAxisAlignment'])
      //     : CrossAxisAlignment.center,
      // TODO: Add proper enum parsing for mainAxisAlignment when Pointer<Void> is resolved
      // mainAxisAlignment:
      //     parseMainAxisAlignment(map.hasMainAxisAlignment, map.mainAxisAlignment),
      children: DynamicWidgetBuilder.buildWidgets(map.children.cast<ChildrenStruct>(), buildContext),
    );
  }

  @override
  String get widgetName => "Row";
}

class ColumnWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ColumnStruct>.fromAddress(fos.handle.address).ref;

    return Column(
      // TODO: Add proper enum parsing for mainAxisAlignment when Pointer<Void> is resolved
      // mainAxisAlignment:
      //     parseMainAxisAlignment(map.hasMainAxisAlignment, map.mainAxisAlignment),
      // mainAxisSize: map.containsKey('mainAxisSize')
      //     ? parseMainAxisSize(map['mainAxisSize'])
      //     : MainAxisSize.max,
      // textBaseline: map.containsKey('textBaseline')
      //     ? parseTextBaseline(map['textBaseline'])
      //     : null,
      // textDirection: map.containsKey('textDirection')
      //     ? parseTextDirection(map['textDirection'])
      //     : null,
      // verticalDirection: map.containsKey('verticalDirection')
      //     ? parseVerticalDirection(map['verticalDirection'])
      //     : VerticalDirection.down,
      children: DynamicWidgetBuilder.buildWidgets(map.children.cast<ChildrenStruct>(), buildContext),
    );
  }

  @override
  String get widgetName => "Column";
}
