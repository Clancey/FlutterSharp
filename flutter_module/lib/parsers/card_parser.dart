// Manual parser for Card widget
// Part of FlutterSharp Phase 4 - Material Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/card_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design Card widget.
///
/// A Material Design card: a panel with slightly rounded corners and an
/// elevation shadow.
///
/// A card is a sheet of Material used to represent some related information,
/// for example an album, a geographical location, a meal, contact details, etc.
class CardParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<CardStruct>.fromAddress(fos.handle.address).ref;

    // Parse child widget
    Widget? child;
    if (map.child.address != 0) {
      child = DynamicWidgetBuilder.buildFromPointer(map.child, buildContext);
    }

    // Parse color
    Color? color;
    if (map.hasColor == 1) {
      color = Color(map.color);
    }

    // Parse shadowColor
    Color? shadowColor;
    if (map.hasShadowColor == 1) {
      shadowColor = Color(map.shadowColor);
    }

    // Parse surfaceTintColor
    Color? surfaceTintColor;
    if (map.hasSurfaceTintColor == 1) {
      surfaceTintColor = Color(map.surfaceTintColor);
    }

    // Parse elevation
    double? elevation;
    if (map.hasElevation == 1) {
      elevation = map.elevation;
    }

    // Shape is a complex type - not currently supported, use null for default

    // Parse borderOnForeground (default true)
    bool borderOnForeground = true;
    if (map.hasBorderOnForeground == 1) {
      borderOnForeground = map.borderOnForeground == 1;
    }

    // Margin is a complex type - not currently supported, use null for default

    // Parse clipBehavior
    Clip clipBehavior = Clip.none;
    if (map.clipBehavior >= 0 && map.clipBehavior < Clip.values.length) {
      clipBehavior = Clip.values[map.clipBehavior];
    }

    // Parse semanticContainer (default true)
    bool semanticContainer = true;
    if (map.hasSemanticContainer == 1) {
      semanticContainer = map.semanticContainer == 1;
    }

    return Card(
      color: color,
      shadowColor: shadowColor,
      surfaceTintColor: surfaceTintColor,
      elevation: elevation,
      borderOnForeground: borderOnForeground,
      clipBehavior: clipBehavior,
      semanticContainer: semanticContainer,
      child: child,
    );
  }

  @override
  String get widgetName => "Card";
}
