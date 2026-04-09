import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import '../scroll_controller_manager.dart';

/// Parser for InfiniteListView - a scrollable list with infinite scrolling support.
///
/// The InfiniteListView uses a builder pattern similar to ListView.builder but
/// includes automatic loading indicator handling and scroll controller integration.
class InfiniteListViewParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map =
        Pointer<InfiniteListViewStruct>.fromAddress(fos.handle.address).ref;
    final id = parseString(map.id);
    if (id == null) return null;

    final itemCount = map.itemCount;
    final hasLoadingIndicator = map.hasLoadingIndicator == 1;
    final controllerIdPtr = map.controllerId;

    // Get or create scroll controller if ID is provided
    ScrollController? controller;
    if (controllerIdPtr.address != 0) {
      final controllerId = controllerIdPtr.toDartString();
      if (controllerId.isNotEmpty) {
        controller = scrollControllerManager.getController(controllerId);
      }
    }

    return ListView.builder(
      controller: controller,
      itemCount: itemCount,
      itemBuilder: (context, index) {
        // If this is the loading indicator position, show a loading widget
        if (hasLoadingIndicator && index == itemCount - 1) {
          return FutureBuilder(
            future: requestMauiData(id, "ItemBuilder", index),
            builder: (BuildContext context, AsyncSnapshot snapshot) {
              if (snapshot.hasData) {
                var pointer = int.parse(snapshot.data);
                if (pointer != 0) {
                  return DynamicWidgetBuilder.buildFromAddress(
                          pointer, context) ??
                      _buildDefaultLoadingIndicator();
                }
              }
              return _buildDefaultLoadingIndicator();
            },
          );
        }

        // Regular item
        return FutureBuilder(
          future: requestMauiData(id, "ItemBuilder", index),
          builder: (BuildContext context, AsyncSnapshot snapshot) {
            if (snapshot.hasData) {
              var pointer = int.parse(snapshot.data);
              return DynamicWidgetBuilder.buildFromAddress(pointer, context) ??
                  const SizedBox.shrink();
            }
            return const SizedBox.shrink();
          },
        );
      },
    );
  }

  Widget _buildDefaultLoadingIndicator() {
    return const Center(
      child: Padding(
        padding: EdgeInsets.all(16.0),
        child: CircularProgressIndicator(),
      ),
    );
  }

  @override
  String get widgetName => "InfiniteListView";
}
