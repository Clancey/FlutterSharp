import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../structs/listenablebuilder_struct.dart';
import '../maui_flutter.dart';

/// Parser for ListenableBuilder widget.
///
/// The C# ListenableBuilder handles listening to the Listenable and
/// calling the builder callback. This parser simply reads the built
/// widget tree from the struct.
class ListenableBuilderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<ListenableBuilderStruct>.fromAddress(fos.handle.address).ref;

    // Read the built child widget from the struct
    final builtChildAddress = map.child.address;

    if (builtChildAddress == 0) {
      // No built widget, return empty container
      return const SizedBox.shrink();
    }

    // Build the widget from the pointer
    return DynamicWidgetBuilder.buildFromAddress(
            builtChildAddress, buildContext) ??
        const SizedBox.shrink();
  }

  @override
  String get widgetName => "ListenableBuilder";
}
