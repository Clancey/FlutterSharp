import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../structs/consumer_struct.dart';
import '../maui_flutter.dart';

/// Parser for Consumer widget.
///
/// Consumer listens to a Provider and rebuilds when the value changes.
/// The C# Consumer handles subscription to ChangeNotifier and calls
/// its builder callback. This parser reads the built widget from the struct.
class ConsumerParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ConsumerStruct>.fromAddress(fos.handle.address).ref;

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
  String get widgetName => "Consumer";
}
