import 'dart:ffi' hide Size;
import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
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

    // Build dark ThemeData from struct values
    ThemeData? darkTheme = _buildDarkThemeData(map);

    // Parse theme mode
    ThemeMode themeMode = ThemeMode.system;
    if (map.hasThemeMode == 1) {
      switch (map.themeMode) {
        case 0:
          themeMode = ThemeMode.system;
          break;
        case 1:
          themeMode = ThemeMode.light;
          break;
        case 2:
          themeMode = ThemeMode.dark;
          break;
      }
    }

    // Parse debug banner setting
    final bool debugShowCheckedModeBanner =
        map.hasDebugShowCheckedModeBanner == 1 &&
            map.debugShowCheckedModeBanner == 1;

    // Parse initial route
    final String? initialRoute =
        map.hasInitialRoute == 1 && map.initialRoute.address != 0
            ? map.initialRoute.toDartString()
            : null;

    return MaterialApp(
      title: title,
      home: home,
      theme: theme,
      darkTheme: darkTheme,
      themeMode: themeMode,
      debugShowCheckedModeBanner: debugShowCheckedModeBanner,
      initialRoute: initialRoute,
    );
  }

  /// Build a TextStyle from struct fields.
  /// Returns null if the style is not set (hasFlag == 0).
  TextStyle? _parseTextStyle({
    required int hasFlag,
    required double fontSize,
    required int fontWeight,
    required int color,
    required double letterSpacing,
    required double height,
  }) {
    if (hasFlag != 1) return null;

    return TextStyle(
      fontSize: fontSize > 0 ? fontSize : null,
      fontWeight: fontWeight > 0 ? _parseFontWeight(fontWeight) : null,
      color: color != 0 ? Color(color) : null,
      letterSpacing: !letterSpacing.isNaN ? letterSpacing : null,
      height: height > 0 ? height : null,
    );
  }

  /// Parse font weight from integer value (100-900).
  FontWeight? _parseFontWeight(int weight) {
    switch (weight) {
      case 100:
        return FontWeight.w100;
      case 200:
        return FontWeight.w200;
      case 300:
        return FontWeight.w300;
      case 400:
        return FontWeight.w400;
      case 500:
        return FontWeight.w500;
      case 600:
        return FontWeight.w600;
      case 700:
        return FontWeight.w700;
      case 800:
        return FontWeight.w800;
      case 900:
        return FontWeight.w900;
      default:
        return null;
    }
  }

  /// Build TextTheme from struct values.
  TextTheme? _buildTextTheme(MaterialAppStruct map) {
    // Check if any text style is defined
    final bool hasAnyStyle = map.hasDisplayLarge == 1 ||
        map.hasDisplayMedium == 1 ||
        map.hasDisplaySmall == 1 ||
        map.hasHeadlineLarge == 1 ||
        map.hasHeadlineMedium == 1 ||
        map.hasHeadlineSmall == 1 ||
        map.hasTitleLarge == 1 ||
        map.hasTitleMedium == 1 ||
        map.hasTitleSmall == 1 ||
        map.hasBodyLarge == 1 ||
        map.hasBodyMedium == 1 ||
        map.hasBodySmall == 1 ||
        map.hasLabelLarge == 1 ||
        map.hasLabelMedium == 1 ||
        map.hasLabelSmall == 1;

    if (!hasAnyStyle) return null;

    return TextTheme(
      displayLarge: _parseTextStyle(
        hasFlag: map.hasDisplayLarge,
        fontSize: map.displayLargeFontSize,
        fontWeight: map.displayLargeFontWeight,
        color: map.displayLargeColor,
        letterSpacing: map.displayLargeLetterSpacing,
        height: map.displayLargeHeight,
      ),
      displayMedium: _parseTextStyle(
        hasFlag: map.hasDisplayMedium,
        fontSize: map.displayMediumFontSize,
        fontWeight: map.displayMediumFontWeight,
        color: map.displayMediumColor,
        letterSpacing: map.displayMediumLetterSpacing,
        height: map.displayMediumHeight,
      ),
      displaySmall: _parseTextStyle(
        hasFlag: map.hasDisplaySmall,
        fontSize: map.displaySmallFontSize,
        fontWeight: map.displaySmallFontWeight,
        color: map.displaySmallColor,
        letterSpacing: map.displaySmallLetterSpacing,
        height: map.displaySmallHeight,
      ),
      headlineLarge: _parseTextStyle(
        hasFlag: map.hasHeadlineLarge,
        fontSize: map.headlineLargeFontSize,
        fontWeight: map.headlineLargeFontWeight,
        color: map.headlineLargeColor,
        letterSpacing: map.headlineLargeLetterSpacing,
        height: map.headlineLargeHeight,
      ),
      headlineMedium: _parseTextStyle(
        hasFlag: map.hasHeadlineMedium,
        fontSize: map.headlineMediumFontSize,
        fontWeight: map.headlineMediumFontWeight,
        color: map.headlineMediumColor,
        letterSpacing: map.headlineMediumLetterSpacing,
        height: map.headlineMediumHeight,
      ),
      headlineSmall: _parseTextStyle(
        hasFlag: map.hasHeadlineSmall,
        fontSize: map.headlineSmallFontSize,
        fontWeight: map.headlineSmallFontWeight,
        color: map.headlineSmallColor,
        letterSpacing: map.headlineSmallLetterSpacing,
        height: map.headlineSmallHeight,
      ),
      titleLarge: _parseTextStyle(
        hasFlag: map.hasTitleLarge,
        fontSize: map.titleLargeFontSize,
        fontWeight: map.titleLargeFontWeight,
        color: map.titleLargeColor,
        letterSpacing: map.titleLargeLetterSpacing,
        height: map.titleLargeHeight,
      ),
      titleMedium: _parseTextStyle(
        hasFlag: map.hasTitleMedium,
        fontSize: map.titleMediumFontSize,
        fontWeight: map.titleMediumFontWeight,
        color: map.titleMediumColor,
        letterSpacing: map.titleMediumLetterSpacing,
        height: map.titleMediumHeight,
      ),
      titleSmall: _parseTextStyle(
        hasFlag: map.hasTitleSmall,
        fontSize: map.titleSmallFontSize,
        fontWeight: map.titleSmallFontWeight,
        color: map.titleSmallColor,
        letterSpacing: map.titleSmallLetterSpacing,
        height: map.titleSmallHeight,
      ),
      bodyLarge: _parseTextStyle(
        hasFlag: map.hasBodyLarge,
        fontSize: map.bodyLargeFontSize,
        fontWeight: map.bodyLargeFontWeight,
        color: map.bodyLargeColor,
        letterSpacing: map.bodyLargeLetterSpacing,
        height: map.bodyLargeHeight,
      ),
      bodyMedium: _parseTextStyle(
        hasFlag: map.hasBodyMedium,
        fontSize: map.bodyMediumFontSize,
        fontWeight: map.bodyMediumFontWeight,
        color: map.bodyMediumColor,
        letterSpacing: map.bodyMediumLetterSpacing,
        height: map.bodyMediumHeight,
      ),
      bodySmall: _parseTextStyle(
        hasFlag: map.hasBodySmall,
        fontSize: map.bodySmallFontSize,
        fontWeight: map.bodySmallFontWeight,
        color: map.bodySmallColor,
        letterSpacing: map.bodySmallLetterSpacing,
        height: map.bodySmallHeight,
      ),
      labelLarge: _parseTextStyle(
        hasFlag: map.hasLabelLarge,
        fontSize: map.labelLargeFontSize,
        fontWeight: map.labelLargeFontWeight,
        color: map.labelLargeColor,
        letterSpacing: map.labelLargeLetterSpacing,
        height: map.labelLargeHeight,
      ),
      labelMedium: _parseTextStyle(
        hasFlag: map.hasLabelMedium,
        fontSize: map.labelMediumFontSize,
        fontWeight: map.labelMediumFontWeight,
        color: map.labelMediumColor,
        letterSpacing: map.labelMediumLetterSpacing,
        height: map.labelMediumHeight,
      ),
      labelSmall: _parseTextStyle(
        hasFlag: map.hasLabelSmall,
        fontSize: map.labelSmallFontSize,
        fontWeight: map.labelSmallFontWeight,
        color: map.labelSmallColor,
        letterSpacing: map.labelSmallLetterSpacing,
        height: map.labelSmallHeight,
      ),
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

    // Parse font family
    final String? fontFamily =
        map.hasFontFamily == 1 && map.fontFamily.address != 0
            ? map.fontFamily.toDartString()
            : null;

    // Build TextTheme from struct
    final TextTheme? textTheme = _buildTextTheme(map);

    // If we have a color scheme seed, use ColorScheme.fromSeed
    if (map.hasColorSchemeSeed == 1 && map.colorSchemeSeed != 0) {
      final Color seedColor = Color(map.colorSchemeSeed);
      return ThemeData(
        useMaterial3: useMaterial3,
        colorScheme: ColorScheme.fromSeed(
          seedColor: seedColor,
          brightness: brightness,
        ),
        fontFamily: fontFamily,
        textTheme: textTheme,
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
        fontFamily: fontFamily,
        textTheme: textTheme,
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
      fontFamily: fontFamily,
      textTheme: textTheme,
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
    if (map.hasAppBarBackgroundColor != 1 &&
        map.hasAppBarForegroundColor != 1) {
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

  /// Build dark AppBarTheme from struct values
  AppBarTheme? _buildDarkAppBarTheme(MaterialAppStruct map) {
    if (map.hasDarkAppBarBackgroundColor != 1 &&
        map.hasDarkAppBarForegroundColor != 1) {
      return null;
    }

    return AppBarTheme(
      backgroundColor: map.hasDarkAppBarBackgroundColor == 1
          ? Color(map.darkAppBarBackgroundColor)
          : null,
      foregroundColor: map.hasDarkAppBarForegroundColor == 1
          ? Color(map.darkAppBarForegroundColor)
          : null,
    );
  }

  /// Build dark ThemeData from struct values.
  /// Returns null if no dark theme is defined.
  ThemeData? _buildDarkThemeData(MaterialAppStruct map) {
    // Check if any dark theme property is set
    final bool hasDarkTheme = map.hasDarkBrightness == 1 ||
        map.hasDarkUseMaterial3 == 1 ||
        map.hasDarkColorSchemeSeed == 1 ||
        map.hasDarkPrimaryColor == 1 ||
        map.hasDarkScaffoldBackgroundColor == 1 ||
        map.hasDarkCardColor == 1 ||
        map.hasDarkDividerColor == 1 ||
        map.hasDarkErrorColor == 1 ||
        map.hasDarkAppBarBackgroundColor == 1 ||
        map.hasDarkAppBarForegroundColor == 1 ||
        map.hasDarkFontFamily == 1;

    if (!hasDarkTheme) {
      return null;
    }

    // Dark theme always uses dark brightness
    const Brightness brightness = Brightness.dark;

    // Determine if using Material 3
    bool useMaterial3 = true;
    if (map.hasDarkUseMaterial3 == 1) {
      useMaterial3 = map.darkUseMaterial3 == 1;
    }

    // Parse font family
    final String? fontFamily =
        map.hasDarkFontFamily == 1 && map.darkFontFamily.address != 0
            ? map.darkFontFamily.toDartString()
            : null;

    // If we have a color scheme seed, use ColorScheme.fromSeed
    if (map.hasDarkColorSchemeSeed == 1 && map.darkColorSchemeSeed != 0) {
      final Color seedColor = Color(map.darkColorSchemeSeed);
      return ThemeData(
        useMaterial3: useMaterial3,
        colorScheme: ColorScheme.fromSeed(
          seedColor: seedColor,
          brightness: brightness,
        ),
        fontFamily: fontFamily,
        scaffoldBackgroundColor: map.hasDarkScaffoldBackgroundColor == 1
            ? Color(map.darkScaffoldBackgroundColor)
            : null,
        cardColor: map.hasDarkCardColor == 1 ? Color(map.darkCardColor) : null,
        dividerColor:
            map.hasDarkDividerColor == 1 ? Color(map.darkDividerColor) : null,
        appBarTheme: _buildDarkAppBarTheme(map),
      );
    }

    // If we have primary color, use that
    if (map.hasDarkPrimaryColor == 1 && map.darkPrimaryColor != 0) {
      final Color primaryColor = Color(map.darkPrimaryColor);
      return ThemeData(
        useMaterial3: useMaterial3,
        brightness: brightness,
        primaryColor: primaryColor,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryColor,
          brightness: brightness,
        ),
        fontFamily: fontFamily,
        scaffoldBackgroundColor: map.hasDarkScaffoldBackgroundColor == 1
            ? Color(map.darkScaffoldBackgroundColor)
            : null,
        cardColor: map.hasDarkCardColor == 1 ? Color(map.darkCardColor) : null,
        dividerColor:
            map.hasDarkDividerColor == 1 ? Color(map.darkDividerColor) : null,
        appBarTheme: _buildDarkAppBarTheme(map),
      );
    }

    // Default dark theme with brightness
    return ThemeData(
      useMaterial3: useMaterial3,
      brightness: brightness,
      fontFamily: fontFamily,
      scaffoldBackgroundColor: map.hasDarkScaffoldBackgroundColor == 1
          ? Color(map.darkScaffoldBackgroundColor)
          : null,
      cardColor: map.hasDarkCardColor == 1 ? Color(map.darkCardColor) : null,
      dividerColor:
          map.hasDarkDividerColor == 1 ? Color(map.darkDividerColor) : null,
      appBarTheme: _buildDarkAppBarTheme(map),
    );
  }

  @override
  String get widgetName => "MaterialApp";
}
