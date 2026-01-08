import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import '../scroll_controller_manager.dart';

/// Parser for InfiniteGridView - a scrollable grid with infinite scrolling support.
///
/// The InfiniteGridView uses a builder pattern similar to GridView.builder but
/// includes automatic loading indicator handling and scroll controller integration.
class InfiniteGridViewParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<InfiniteGridViewStruct>.fromAddress(fos.handle.address).ref;
    final id = parseString(map.id);
    if (id == null) return null;

    final itemCount = map.itemCount;
    final crossAxisCount = map.crossAxisCount > 0 ? map.crossAxisCount : 2;
    final mainAxisSpacing = map.mainAxisSpacing;
    final crossAxisSpacing = map.crossAxisSpacing;
    final childAspectRatio = map.childAspectRatio > 0 ? map.childAspectRatio : 1.0;
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

    // Calculate the number of actual items (excluding loading indicators)
    // Loading indicators take up `crossAxisCount` slots to span the full width
    final actualItemCount = hasLoadingIndicator
        ? itemCount - crossAxisCount
        : itemCount;

    return GridView.builder(
      controller: controller,
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: crossAxisCount,
        mainAxisSpacing: mainAxisSpacing,
        crossAxisSpacing: crossAxisSpacing,
        childAspectRatio: childAspectRatio,
      ),
      itemCount: itemCount,
      itemBuilder: (context, index) {
        // Check if this is a loading indicator position
        if (hasLoadingIndicator && index >= actualItemCount) {
          // Only show loading indicator in the first slot of the loading row
          if (index == actualItemCount) {
            return FutureBuilder(
              future: requestMauiData(id, "ItemBuilder", index),
              builder: (BuildContext context, AsyncSnapshot snapshot) {
                if (snapshot.hasData) {
                  var pointer = int.parse(snapshot.data);
                  if (pointer != 0) {
                    return DynamicWidgetBuilder.buildFromAddress(pointer, context) ??
                        _buildDefaultLoadingIndicator();
                  }
                }
                return _buildDefaultLoadingIndicator();
              },
            );
          }
          // Other slots in loading row are empty
          return const SizedBox.shrink();
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
  String get widgetName => "InfiniteGridView";
}
