import 'dart:ffi' hide Size;
import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for MaterialApp widget from C# FFI struct.
class MaterialAppParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MaterialAppStruct>.fromAddress(fos.handle.address).ref;

    // Parse title
    final String title = map.hasTitle == 1 && map.title.address != 0
        ? map.title.toDartString()
        : '';

    // Parse home widget
    final Widget? home = map.hasHome == 1
        ? DynamicWidgetBuilder.buildFromPointer(map.home, buildContext)
        : null;

    // Build ThemeData from struct values
    ThemeData? theme = _buildThemeData(map);

    // Parse debug banner setting
    final bool debugShowCheckedModeBanner =
        map.hasDebugShowCheckedModeBanner == 1 && map.debugShowCheckedModeBanner == 1;

    // Parse initial route
    final String? initialRoute = map.hasInitialRoute == 1 && map.initialRoute.address != 0
        ? map.initialRoute.toDartString()
        : null;

    return MaterialApp(
      title: title,
      home: home,
      theme: theme,
      debugShowCheckedModeBanner: debugShowCheckedModeBanner,
      initialRoute: initialRoute,
    );
  }

  /// Build ThemeData from struct values
  ThemeData? _buildThemeData(MaterialAppStruct map) {
    // Determine brightness
    Brightness brightness = Brightness.light;
    if (map.hasBrightness == 1) {
      brightness = map.brightness == 0 ? Brightness.dark : Brightness.light;
    }

    // Determine if using Material 3
    bool useMaterial3 = true;
    if (map.hasUseMaterial3 == 1) {
      useMaterial3 = map.useMaterial3 == 1;
    }

    // If we have a color scheme seed, use ColorScheme.fromSeed
    if (map.hasColorSchemeSeed == 1 && map.colorSchemeSeed != 0) {
      final Color seedColor = Color(map.colorSchemeSeed);
      return ThemeData(
        useMaterial3: useMaterial3,
        colorScheme: ColorScheme.fromSeed(
          seedColor: seedColor,
          brightness: brightness,
        ),
        fontFamily: map.hasFontFamily == 1 && map.fontFamily.address != 0
            ? map.fontFamily.toDartString()
            : null,
        scaffoldBackgroundColor: map.hasScaffoldBackgroundColor == 1
            ? Color(map.scaffoldBackgroundColor)
            : null,
        cardColor: map.hasCardColor == 1 ? Color(map.cardColor) : null,
        dividerColor: map.hasDividerColor == 1 ? Color(map.dividerColor) : null,
        appBarTheme: _buildAppBarTheme(map),
      );
    }

    // If we have primary color, use that
    if (map.hasPrimaryColor == 1 && map.primaryColor != 0) {
      final Color primaryColor = Color(map.primaryColor);
      return ThemeData(
        useMaterial3: useMaterial3,
        brightness: brightness,
        primaryColor: primaryColor,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryColor,
          brightness: brightness,
        ),
        fontFamily: map.hasFontFamily == 1 && map.fontFamily.address != 0
            ? map.fontFamily.toDartString()
            : null,
        scaffoldBackgroundColor: map.hasScaffoldBackgroundColor == 1
            ? Color(map.scaffoldBackgroundColor)
            : null,
        cardColor: map.hasCardColor == 1 ? Color(map.cardColor) : null,
        dividerColor: map.hasDividerColor == 1 ? Color(map.dividerColor) : null,
        appBarTheme: _buildAppBarTheme(map),
      );
    }

    // Default theme with brightness
    return ThemeData(
      useMaterial3: useMaterial3,
      brightness: brightness,
      fontFamily: map.hasFontFamily == 1 && map.fontFamily.address != 0
          ? map.fontFamily.toDartString()
          : null,
      scaffoldBackgroundColor: map.hasScaffoldBackgroundColor == 1
          ? Color(map.scaffoldBackgroundColor)
          : null,
      cardColor: map.hasCardColor == 1 ? Color(map.cardColor) : null,
      dividerColor: map.hasDividerColor == 1 ? Color(map.dividerColor) : null,
      appBarTheme: _buildAppBarTheme(map),
    );
  }

  /// Build AppBarTheme from struct values
  AppBarTheme? _buildAppBarTheme(MaterialAppStruct map) {
    if (map.hasAppBarBackgroundColor != 1 && map.hasAppBarForegroundColor != 1) {
      return null;
    }

    return AppBarTheme(
      backgroundColor: map.hasAppBarBackgroundColor == 1
          ? Color(map.appBarBackgroundColor)
          : null,
      foregroundColor: map.hasAppBarForegroundColor == 1
          ? Color(map.appBarForegroundColor)
          : null,
    );
  }

  @override
  String get widgetName => "MaterialApp";
}
