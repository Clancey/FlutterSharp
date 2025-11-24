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

class IFlutterObjectStruct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
}

final class FlutterObjectStruct extends Struct implements IFlutterObjectStruct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
}

class IWidgetStruct extends IFlutterObjectStruct {
  external Pointer<Utf8> id;
}

//WidgetStruct : FlutterOBjectStruct
final class WidgetStruct extends Struct implements IWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
}

// NOTE: SingleChildRenderObjectWidgetStruct is now generated
// See: lib/generated/structs/singlechildrenderobjectwidget_struct.dart

final class ChildrenStruct extends Struct {
  external Pointer<Uint64> children;
  @Int32()
  external int childrenLength;
}

// NOTE: MultiChildRenderObjectWidgetStruct is now generated
// See: lib/generated/structs/multichildrenderobjectwidget_struct.dart

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
