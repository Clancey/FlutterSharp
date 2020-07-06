import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/material.dart';
import 'drop_cap_text.dart';

class DropCapTextParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    return null;
    //TODO: Implement
    // var map = Pointer<ContainerStruct>.fromAddress(fos.handle.address).ref;
    // return DropCapText(
    //   data: map.containsKey('data') ? map['data'] : null,
    //   selectable: map.containsKey('selectable') ? map['selectable'] : false,
    //   mode: map.containsKey('mode')
    //       ? parseDropCapMode(map['mode'])
    //       : DropCapMode.inside,
    //   style: map.containsKey('style') ? parseTextStyle(map['style']) : null,
    //   dropCapStyle: map.containsKey('dropCapStyle')
    //       ? parseTextStyle(map['dropCapStyle'])
    //       : null,
    //   textAlign: map.containsKey('textAlign')
    //       ? parseTextAlign(map['textAlign'])
    //       : TextAlign.start,
    //   dropCap: map.containsKey('dropCap')
    //       ? parseDropCap(map['dropCap'], buildContext)
    //       : null,
    //   dropCapPadding: map.containsKey('dropCapPadding')
    //       ? parseEdgeInsetsGeometry(map['dropCapPadding'])
    //       : EdgeInsets.zero,
    //   indentation: Offset.zero,
    //   //todo: actually add this
    //   dropCapChars: map.containsKey('dropCapChars') ? map['dropCapChars'] : 1,
    //   forceNoDescent:
    //       map.containsKey('forceNoDescent') ? map['forceNoDescent'] : false,
    //   parseInlineMarkdown: map.containsKey('parseInlineMarkdown')
    //       ? map['parseInlineMarkdown']
    //       : false,
    //   textDirection: map.containsKey('textDirection')
    //       ? parseTextDirection(map['textDirection'])
    //       : TextDirection.ltr,
    //   overflow: map.containsKey('overflow')
    //       ? parseTextOverflow(map['overflow'])
    //       : TextOverflow.clip,
    //   maxLines: map.containsKey('maxLines') ? map['maxLines'] : null,
    //   dropCapPosition: map.containsKey('dropCapPosition')
    //       ? parseDropCapPosition(map['dropCapPosition'])
    //       : null,
    // );
  }

  @override
  String get widgetName => "DropCapText";
}
