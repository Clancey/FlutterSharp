import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/provider_struct.dart';
import '../maui_flutter.dart';

/// Parser for Provider widget.
///
/// Provider is a dependency injection widget that makes a value
/// available to its descendants. The value is managed on the C# side.
/// This parser simply reads the child widget from the struct.
class ProviderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ProviderStruct>.fromAddress(fos.handle.address).ref;

    // Read the child widget from the struct
    final childAddress = map.child.address;

    if (childAddress == 0) {
      // No child widget, return empty container
      return const SizedBox.shrink();
    }

    // Build the child widget from the pointer
    return DynamicWidgetBuilder.buildFromAddress(childAddress, buildContext) ??
        const SizedBox.shrink();
  }

  @override
  String get widgetName => "Provider";
}
