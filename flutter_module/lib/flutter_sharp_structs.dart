import 'dart:ffi';
import 'package:ffi/ffi.dart';

// Export generated structs that are referenced by hand-written parsers
export 'generated/structs/align_struct.dart';
export 'generated/structs/aspectratio_struct.dart';
export 'generated/structs/column_struct.dart';
export 'generated/structs/container_struct.dart';
export 'generated/structs/icon_struct.dart';
export 'generated/structs/row_struct.dart';
export 'generated/structs/text_struct.dart';
export 'generated/structs/elevatedbutton_struct.dart';
export 'generated/structs/textbutton_struct.dart';
export 'generated/structs/outlinedbutton_struct.dart';
export 'generated/structs/iconbutton_struct.dart';
export 'generated/structs/floatingactionbutton_struct.dart';
export 'generated/structs/listtile_struct.dart';
export 'generated/structs/listview_struct.dart';
export 'generated/structs/bottomnavigationbar_struct.dart';
export 'generated/structs/bottomnavigationbaritem_struct.dart';
export 'generated/structs/navigator_struct.dart';
export 'generated/structs/cupertinobutton_struct.dart';
export 'generated/structs/cupertinotextfield_struct.dart';
export 'generated/structs/cupertinoswitch_struct.dart';
export 'generated/structs/dropdownbutton_struct.dart';
export 'generated/structs/dropdownmenuitem_struct.dart';

/// Abstract interface for FlutterObjectStruct to allow type-safe parsing.
/// This is used as a parameter type in parser methods.
abstract class IFlutterObjectStruct {
  Pointer get handle;
  Pointer get managedHandle;
  Pointer<Utf8> get widgetType;
}

/// Abstract interface for WidgetStruct to allow type-safe parsing.
abstract class IWidgetStruct extends IFlutterObjectStruct {
  Pointer<Utf8> get id;
}

/// Base FFI struct for all Flutter objects.
final class FlutterObjectStruct extends Struct implements IFlutterObjectStruct {
  @override
  external Pointer handle;
  @override
  external Pointer managedHandle;
  @override
  external Pointer<Utf8> widgetType;
}

/// Base FFI struct for all widgets.
final class WidgetStruct extends Struct implements IWidgetStruct {
  //FlutterObject Struct
  @override
  external Pointer handle;
  @override
  external Pointer managedHandle;
  @override
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  @override
  external Pointer<Utf8> id;
}

/// Abstract interface for SingleChildRenderObjectWidgetStruct.
abstract class ISingleChildRenderObjectWidgetStruct extends IWidgetStruct {
  Pointer<WidgetStruct> get child;
}

/// FFI struct for Material Tab widgets.
final class TabStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  @override
  external Pointer handle;
  @override
  external Pointer managedHandle;
  @override
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  @override
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  @override
  external Pointer<WidgetStruct> child;
  //TabStruct
  external Pointer<Utf8> text;
}

/// FFI struct for widgets with a single child.
final class SingleChildRenderObjectWidgetStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  @override
  external Pointer handle;
  @override
  external Pointer managedHandle;
  @override
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  @override
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  @override
  external Pointer<WidgetStruct> child;
}

final class ChildrenStruct extends Struct {
  external Pointer<Uint64> children;
  @Int32()
  external int childrenLength;
}

/// Abstract interface for MultiChildRenderObjectWidgetStruct.
abstract class IMultiChildRenderObjectWidgetStruct extends IWidgetStruct {
  Pointer get children;
}

/// FFI struct for widgets with multiple children.
final class MultiChildRenderObjectWidgetStruct extends Struct
    implements IMultiChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  @override
  external Pointer handle;
  @override
  external Pointer managedHandle;
  @override
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  @override
  external Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  @override
  external Pointer children;
}

final class AlignmentStruct extends Struct {
  @Double()
  external double x;

  @Double()
  external double y;
}

// NOTE: AlignStruct is now generated
// See: lib/generated/structs/align_struct.dart

//AppBarStruct : WidgetStruct
final class AppBarStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //AppBarStruct
  external Pointer<WidgetStruct> title;
  external Pointer<WidgetStruct> bottom;
}

// NOTE: AspectRatioStruct is now generated
// See: lib/generated/structs/aspectratio_struct.dart

//CheckboxStruct : WidgetStruct
final class CheckboxStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //CheckboxStruct
  @Int8()
  external int value;
}

// NOTE: ColumnStruct is now generated
// See: lib/generated/structs/column_struct.dart

final class EdgeInsetGemoetryStruct extends Struct {
  @Double()
  external double left;
  @Double()
  external double top;
  @Double()
  external double right;
  @Double()
  external double bottom;
}

final class ColorStruct extends Struct {
  @Int8()
  external int red;
  @Int8()
  external int green;
  @Int8()
  external int blue;
  @Int8()
  external int alpha;
}

// NOTE: ContainerStruct is now generated
// See: lib/generated/structs/container_struct.dart

//DefaultTabControllerStruct : SingleChildRenderObjectWidgetStruct
final class DefaultTabControllerStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  external Pointer<WidgetStruct> child;

// DefaultTabControllerStruct
  @Int32()
  external int tabCount;
}

// NOTE: IconStruct is now generated
// See: lib/generated/structs/icon_struct.dart

//ListViewBuilderStruct : Widget
final class ListViewBuilderStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //ListViewBuilderStruct
  @Int32()
  external int itemCount;
}

//GridViewBuilderStruct : Widget
final class GridViewBuilderStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //GridViewBuilderStruct
  @Int32()
  external int itemCount;
  @Int32()
  external int crossAxisCount;
  @Double()
  external double mainAxisSpacing;
  @Double()
  external double crossAxisSpacing;
  @Double()
  external double childAspectRatio;
}

//InfiniteListViewStruct : Widget
final class InfiniteListViewStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //InfiniteListViewStruct
  @Int32()
  external int itemCount;
  external Pointer<Utf8> controllerId;
  @Int8()
  external int hasLoadingIndicator;
  @Double()
  external double loadMoreThreshold;
}

//InfiniteGridViewStruct : Widget
final class InfiniteGridViewStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //InfiniteGridViewStruct
  @Int32()
  external int itemCount;
  external Pointer<Utf8> controllerId;
  @Int32()
  external int crossAxisCount;
  @Double()
  external double mainAxisSpacing;
  @Double()
  external double crossAxisSpacing;
  @Double()
  external double childAspectRatio;
  @Int8()
  external int hasLoadingIndicator;
  @Double()
  external double loadMoreThreshold;
}

// NOTE: RowStruct is now generated
// See: lib/generated/structs/row_struct.dart

//ScaffoldStruct : Widget
final class ScaffoldStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //ScaffoldStruct
  external Pointer<WidgetStruct> appBar;
  external Pointer<WidgetStruct> floatingActionButton;
  external Pointer<WidgetStruct> drawer;
  external Pointer<WidgetStruct> body;
}

// NOTE: TextStruct is now generated
// See: lib/generated/structs/text_struct.dart

//TextFieldStruct : Widget - Material Design text field
final class TextFieldStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //TextFieldStruct - Text value
  @Int8()
  external int hasValue;
  external Pointer<Utf8> value;

  // Hint text (placeholder)
  @Int8()
  external int hasHint;
  external Pointer<Utf8> hint;

  // Label text
  @Int8()
  external int hasLabel;
  external Pointer<Utf8> label;

  // Helper text
  @Int8()
  external int hasHelperText;
  external Pointer<Utf8> helperText;

  // Error text
  @Int8()
  external int hasErrorText;
  external Pointer<Utf8> errorText;

  // Counter text
  @Int8()
  external int hasCounterText;
  external Pointer<Utf8> counterText;

  // Callbacks
  @Int8()
  external int hasOnChanged;
  external Pointer<Utf8> onChangedAction;

  @Int8()
  external int hasOnSubmitted;
  external Pointer<Utf8> onSubmittedAction;

  @Int8()
  external int hasOnEditingComplete;
  external Pointer<Utf8> onEditingCompleteAction;

  @Int8()
  external int hasOnTap;
  external Pointer<Utf8> onTapAction;

  // Text input configuration
  @Int8()
  external int hasKeyboardType;
  @Int32()
  external int keyboardType;

  @Int8()
  external int hasTextInputAction;
  @Int32()
  external int textInputAction;

  @Int8()
  external int hasTextAlign;
  @Int32()
  external int textAlign;

  // Line configuration
  @Int8()
  external int hasMaxLines;
  @Int32()
  external int maxLines;

  @Int8()
  external int hasMinLines;
  @Int32()
  external int minLines;

  @Int8()
  external int hasMaxLength;
  @Int32()
  external int maxLength;

  // Boolean flags
  @Int8()
  external int enabled;
  @Int8()
  external int readOnly;
  @Int8()
  external int obscureText;
  @Int8()
  external int autofocus;
  @Int8()
  external int autocorrect;
  @Int8()
  external int enableSuggestions;
  @Int8()
  external int expands;

  // Obscuring character
  @Int8()
  external int hasObscuringCharacter;
  external Pointer<Utf8> obscuringCharacter;

  // Cursor appearance
  @Int8()
  external int hasCursorColor;
  @Uint32()
  external int cursorColor;

  @Int8()
  external int hasCursorWidth;
  @Double()
  external double cursorWidth;

  @Int8()
  external int hasCursorHeight;
  @Double()
  external double cursorHeight;

  @Int8()
  external int hasCursorRadius;
  @Double()
  external double cursorRadius;

  // Decoration - fill
  @Int8()
  external int filled;

  @Int8()
  external int hasFillColor;
  @Uint32()
  external int fillColor;

  // Decoration - border colors
  @Int8()
  external int hasBorderColor;
  @Uint32()
  external int borderColor;

  @Int8()
  external int hasFocusedBorderColor;
  @Uint32()
  external int focusedBorderColor;

  @Int8()
  external int hasErrorBorderColor;
  @Uint32()
  external int errorBorderColor;

  @Int8()
  external int hasBorderRadius;
  @Double()
  external double borderRadius;

  // Child widgets
  external Pointer prefixIcon;
  external Pointer suffixIcon;
  external Pointer prefix;
  external Pointer suffix;
}

/// MaterialApp struct for FFI interop
/// Matches MaterialAppStruct.cs layout
final class MaterialAppStruct extends Struct {
  // FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  // WidgetStruct
  external Pointer<Utf8> id;

  // MaterialAppStruct
  external Pointer<Utf8> title;
  @Int8()
  external int hasTitle;

  external Pointer<WidgetStruct> home;
  @Int8()
  external int hasHome;

  // Theme: Brightness (0=dark, 1=light)
  @Int8()
  external int hasBrightness;
  @Int32()
  external int brightness;

  // Use Material 3
  @Int8()
  external int hasUseMaterial3;
  @Int8()
  external int useMaterial3;

  // Color scheme seed (ARGB uint)
  @Int8()
  external int hasColorSchemeSeed;
  @Uint32()
  external int colorSchemeSeed;

  // Primary color (ARGB uint)
  @Int8()
  external int hasPrimaryColor;
  @Uint32()
  external int primaryColor;

  // Scaffold background color
  @Int8()
  external int hasScaffoldBackgroundColor;
  @Uint32()
  external int scaffoldBackgroundColor;

  // AppBar background color
  @Int8()
  external int hasAppBarBackgroundColor;
  @Uint32()
  external int appBarBackgroundColor;

  // AppBar foreground color
  @Int8()
  external int hasAppBarForegroundColor;
  @Uint32()
  external int appBarForegroundColor;

  // Card color
  @Int8()
  external int hasCardColor;
  @Uint32()
  external int cardColor;

  // Divider color
  @Int8()
  external int hasDividerColor;
  @Uint32()
  external int dividerColor;

  // Error color
  @Int8()
  external int hasErrorColor;
  @Uint32()
  external int errorColor;

  // Font family
  external Pointer<Utf8> fontFamily;
  @Int8()
  external int hasFontFamily;

  // Debug show checked mode banner
  @Int8()
  external int hasDebugShowCheckedModeBanner;
  @Int8()
  external int debugShowCheckedModeBanner;

  // Initial route
  external Pointer<Utf8> initialRoute;
  @Int8()
  external int hasInitialRoute;

  // ========== Theme Mode ==========
  // 0 = System (follow device setting)
  // 1 = Light (always light)
  // 2 = Dark (always dark)
  @Int8()
  external int hasThemeMode;
  @Int32()
  external int themeMode;

  // ========== Dark Theme properties ==========
  // These mirror the light theme properties but for dark mode

  // Dark theme brightness (for override purposes)
  @Int8()
  external int hasDarkBrightness;
  @Int32()
  external int darkBrightness;

  // Dark theme use Material 3
  @Int8()
  external int hasDarkUseMaterial3;
  @Int8()
  external int darkUseMaterial3;

  // Dark theme color scheme seed (ARGB uint)
  @Int8()
  external int hasDarkColorSchemeSeed;
  @Uint32()
  external int darkColorSchemeSeed;

  // Dark theme primary color (ARGB uint)
  @Int8()
  external int hasDarkPrimaryColor;
  @Uint32()
  external int darkPrimaryColor;

  // Dark theme scaffold background color
  @Int8()
  external int hasDarkScaffoldBackgroundColor;
  @Uint32()
  external int darkScaffoldBackgroundColor;

  // Dark theme AppBar background color
  @Int8()
  external int hasDarkAppBarBackgroundColor;
  @Uint32()
  external int darkAppBarBackgroundColor;

  // Dark theme AppBar foreground color
  @Int8()
  external int hasDarkAppBarForegroundColor;
  @Uint32()
  external int darkAppBarForegroundColor;

  // Dark theme card color
  @Int8()
  external int hasDarkCardColor;
  @Uint32()
  external int darkCardColor;

  // Dark theme divider color
  @Int8()
  external int hasDarkDividerColor;
  @Uint32()
  external int darkDividerColor;

  // Dark theme error color
  @Int8()
  external int hasDarkErrorColor;
  @Uint32()
  external int darkErrorColor;

  // Dark theme font family
  external Pointer<Utf8> darkFontFamily;
  @Int8()
  external int hasDarkFontFamily;

  // ========== TextTheme properties ==========
  // Each text style has: Has flag, fontSize (double), fontWeight (int), color (uint), letterSpacing (double), height (double)

  // Display Large (57px default)
  @Int8()
  external int hasDisplayLarge;
  @Double()
  external double displayLargeFontSize;
  @Int32()
  external int displayLargeFontWeight;
  @Uint32()
  external int displayLargeColor;
  @Double()
  external double displayLargeLetterSpacing;
  @Double()
  external double displayLargeHeight;

  // Display Medium (45px default)
  @Int8()
  external int hasDisplayMedium;
  @Double()
  external double displayMediumFontSize;
  @Int32()
  external int displayMediumFontWeight;
  @Uint32()
  external int displayMediumColor;
  @Double()
  external double displayMediumLetterSpacing;
  @Double()
  external double displayMediumHeight;

  // Display Small (36px default)
  @Int8()
  external int hasDisplaySmall;
  @Double()
  external double displaySmallFontSize;
  @Int32()
  external int displaySmallFontWeight;
  @Uint32()
  external int displaySmallColor;
  @Double()
  external double displaySmallLetterSpacing;
  @Double()
  external double displaySmallHeight;

  // Headline Large (32px default)
  @Int8()
  external int hasHeadlineLarge;
  @Double()
  external double headlineLargeFontSize;
  @Int32()
  external int headlineLargeFontWeight;
  @Uint32()
  external int headlineLargeColor;
  @Double()
  external double headlineLargeLetterSpacing;
  @Double()
  external double headlineLargeHeight;

  // Headline Medium (28px default)
  @Int8()
  external int hasHeadlineMedium;
  @Double()
  external double headlineMediumFontSize;
  @Int32()
  external int headlineMediumFontWeight;
  @Uint32()
  external int headlineMediumColor;
  @Double()
  external double headlineMediumLetterSpacing;
  @Double()
  external double headlineMediumHeight;

  // Headline Small (24px default)
  @Int8()
  external int hasHeadlineSmall;
  @Double()
  external double headlineSmallFontSize;
  @Int32()
  external int headlineSmallFontWeight;
  @Uint32()
  external int headlineSmallColor;
  @Double()
  external double headlineSmallLetterSpacing;
  @Double()
  external double headlineSmallHeight;

  // Title Large (22px default)
  @Int8()
  external int hasTitleLarge;
  @Double()
  external double titleLargeFontSize;
  @Int32()
  external int titleLargeFontWeight;
  @Uint32()
  external int titleLargeColor;
  @Double()
  external double titleLargeLetterSpacing;
  @Double()
  external double titleLargeHeight;

  // Title Medium (16px default)
  @Int8()
  external int hasTitleMedium;
  @Double()
  external double titleMediumFontSize;
  @Int32()
  external int titleMediumFontWeight;
  @Uint32()
  external int titleMediumColor;
  @Double()
  external double titleMediumLetterSpacing;
  @Double()
  external double titleMediumHeight;

  // Title Small (14px default)
  @Int8()
  external int hasTitleSmall;
  @Double()
  external double titleSmallFontSize;
  @Int32()
  external int titleSmallFontWeight;
  @Uint32()
  external int titleSmallColor;
  @Double()
  external double titleSmallLetterSpacing;
  @Double()
  external double titleSmallHeight;

  // Body Large (16px default)
  @Int8()
  external int hasBodyLarge;
  @Double()
  external double bodyLargeFontSize;
  @Int32()
  external int bodyLargeFontWeight;
  @Uint32()
  external int bodyLargeColor;
  @Double()
  external double bodyLargeLetterSpacing;
  @Double()
  external double bodyLargeHeight;

  // Body Medium (14px default)
  @Int8()
  external int hasBodyMedium;
  @Double()
  external double bodyMediumFontSize;
  @Int32()
  external int bodyMediumFontWeight;
  @Uint32()
  external int bodyMediumColor;
  @Double()
  external double bodyMediumLetterSpacing;
  @Double()
  external double bodyMediumHeight;

  // Body Small (12px default)
  @Int8()
  external int hasBodySmall;
  @Double()
  external double bodySmallFontSize;
  @Int32()
  external int bodySmallFontWeight;
  @Uint32()
  external int bodySmallColor;
  @Double()
  external double bodySmallLetterSpacing;
  @Double()
  external double bodySmallHeight;

  // Label Large (14px default)
  @Int8()
  external int hasLabelLarge;
  @Double()
  external double labelLargeFontSize;
  @Int32()
  external int labelLargeFontWeight;
  @Uint32()
  external int labelLargeColor;
  @Double()
  external double labelLargeLetterSpacing;
  @Double()
  external double labelLargeHeight;

  // Label Medium (12px default)
  @Int8()
  external int hasLabelMedium;
  @Double()
  external double labelMediumFontSize;
  @Int32()
  external int labelMediumFontWeight;
  @Uint32()
  external int labelMediumColor;
  @Double()
  external double labelMediumLetterSpacing;
  @Double()
  external double labelMediumHeight;

  // Label Small (11px default)
  @Int8()
  external int hasLabelSmall;
  @Double()
  external double labelSmallFontSize;
  @Int32()
  external int labelSmallFontWeight;
  @Uint32()
  external int labelSmallColor;
  @Double()
  external double labelSmallLetterSpacing;
  @Double()
  external double labelSmallHeight;
}
