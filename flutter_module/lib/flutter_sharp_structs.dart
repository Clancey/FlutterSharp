import 'dart:async';
import 'dart:io' show Platform;
import 'dart:typed_data';
import 'dart:ffi';
import 'package:ffi/ffi.dart';

import 'package:flutter/foundation.dart'
    show debugDefaultTargetPlatformOverride;
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'maui_flutter.dart';

import 'dart:convert';

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

/// FFI struct for widgets with a single child.
final class SingleChildRenderObjectWidgetStruct extends Struct implements ISingleChildRenderObjectWidgetStruct {
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
  int get childrenLength;
}

/// FFI struct for widgets with multiple children.
final class MultiChildRenderObjectWidgetStruct extends Struct implements IMultiChildRenderObjectWidgetStruct {
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
  @override
  @Int32()
  external int childrenLength;
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

//TextFieldStruct : Widget
final class TextFieldStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //TextFieldStruct
  external Pointer<Utf8> value;
  external Pointer<Utf8> hint;
}
