import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../structs/multiprovider_struct.dart';
import '../maui_flutter.dart';

/// Parser for MultiProvider widget.
///
/// MultiProvider composes multiple providers into a nested widget tree.
/// The C# MultiProvider handles building the nested structure. This parser
/// reads the outermost built widget from the struct.
class MultiProviderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MultiProviderStruct>.fromAddress(fos.handle.address).ref;

    // Read the built child widget from the struct
    final builtChildAddress = map.builtChild.address;

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
  String get widgetName => "MultiProvider";
}
