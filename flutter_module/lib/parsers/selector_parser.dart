import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../structs/selector_struct.dart';
import '../maui_flutter.dart';

/// Parser for Selector widget.
///
/// Selector is an optimized Consumer that only rebuilds when a selected
/// subset of the provider value changes. The C# Selector handles the
/// selection and comparison logic. This parser reads the built widget.
class SelectorParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<SelectorStruct>.fromAddress(fos.handle.address).ref;

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
  String get widgetName => "Selector";
}
