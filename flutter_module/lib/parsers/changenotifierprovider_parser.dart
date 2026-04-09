import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../structs/changenotifierprovider_struct.dart';
import '../maui_flutter.dart';

/// Parser for ChangeNotifierProvider widget.
///
/// ChangeNotifierProvider manages a ChangeNotifier and notifies consumers
/// when it changes. The C# side handles all state management. This parser
/// simply reads the child widget from the struct.
class ChangeNotifierProviderParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<ChangeNotifierProviderStruct>.fromAddress(fos.handle.address)
            .ref;

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
  String get widgetName => "ChangeNotifierProvider";
}
