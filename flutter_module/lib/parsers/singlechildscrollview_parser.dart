// Manual parser for SingleChildScrollView widget
// Part of FlutterSharp - Scroll Widgets

import 'dart:ffi';

import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import '../structs/singlechildscrollview_struct.dart';

/// Parser for SingleChildScrollView widget.
///
/// A box in which a single widget can be scrolled.
///
/// This widget is useful when you have a single box that will normally be
/// entirely visible, but you need to make sure it can be scrolled if the
/// container gets too small in one axis (the scroll direction).
class SingleChildScrollViewParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<SingleChildScrollViewStruct>.fromAddress(fos.handle.address)
            .ref;

    // Get widget ID for debugging/tracking
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse scroll direction
    Axis scrollDirection = Axis.vertical;
    if (map.scrollDirection == 0) {
      scrollDirection = Axis.horizontal;
    } else {
      scrollDirection = Axis.vertical;
    }

    // Parse boolean properties
    final reverse = map.reverse == 1;
    final primary = map.hasPrimary == 1 ? (map.primary == 1) : null;

    // Parse child widget
    Widget? child;
    if (map.hasChild == 1 && map.child.address != 0) {
      child = DynamicWidgetBuilder.buildFromPointer(map.child, buildContext);
    }

    // Parse padding
    EdgeInsetsGeometry? padding;
    if (map.padding.address != 0) {
      // For now, we'll skip padding parsing as it requires complex marshaling
      // This can be implemented later with EdgeInsetsGeometry parser
      // padding = parseEdgeInsetsGeometry(map.padding);
    }

    // Parse controller ID and get/create the ScrollController
    ScrollController? controller;
    if (map.hasController == 1 && map.controller.address != 0) {
      // For now, we'll use the scrollControllerManager approach like ListView
      // This requires the controller ID to be passed as a string
      // The controller pointer parsing would need additional infrastructure
    }

    // Parse physics
    ScrollPhysics? physics;
    if (map.hasPhysics == 1 && map.physics.address != 0) {
      // Physics parsing would require additional marshaling infrastructure
      // For now, we'll use default physics
    }

    // Parse drag start behavior
    DragStartBehavior dragStartBehavior = DragStartBehavior.start;
    if (map.dragStartBehavior == 0) {
      dragStartBehavior = DragStartBehavior.down;
    } else {
      dragStartBehavior = DragStartBehavior.start;
    }

    // Parse clip behavior
    Clip clipBehavior = Clip.hardEdge;
    switch (map.clipBehavior) {
      case 0:
        clipBehavior = Clip.none;
        break;
      case 1:
        clipBehavior = Clip.hardEdge;
        break;
      case 2:
        clipBehavior = Clip.antiAlias;
        break;
      case 3:
        clipBehavior = Clip.antiAliasWithSaveLayer;
        break;
      default:
        clipBehavior = Clip.hardEdge;
    }

    // Parse restoration ID
    String? restorationId;
    if (map.hasRestorationId == 1) {
      restorationId = parseString(map.restorationId);
    }

    // Parse keyboard dismiss behavior
    ScrollViewKeyboardDismissBehavior? keyboardDismissBehavior;
    if (map.hasKeyboardDismissBehavior == 1) {
      switch (map.keyboardDismissBehavior) {
        case 0:
          keyboardDismissBehavior = ScrollViewKeyboardDismissBehavior.manual;
          break;
        case 1:
          keyboardDismissBehavior = ScrollViewKeyboardDismissBehavior.onDrag;
          break;
        default:
          keyboardDismissBehavior = null;
      }
    }

    return SingleChildScrollView(
      scrollDirection: scrollDirection,
      reverse: reverse,
      padding: padding,
      controller: controller,
      primary: primary,
      physics: physics,
      child: child,
      dragStartBehavior: dragStartBehavior,
      clipBehavior: clipBehavior,
      restorationId: restorationId,
      keyboardDismissBehavior: keyboardDismissBehavior,
    );
  }

  @override
  String get widgetName => "SingleChildScrollView";
}
