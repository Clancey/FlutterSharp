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

class ISingleChildRenderObjectWidgetStruct extends IWidgetStruct {
  external Pointer<WidgetStruct> child;
}

//SingleChildRenderObjectWidgetStruct : WidgetStruct
final class SingleChildRenderObjectWidgetStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  external Pointer<WidgetStruct> child;
}

final class ChildrenStruct extends Struct {
  external Pointer<Uint64> children;
  @Int32()
  external int childrenLength;
}

class IMultiChildRenderObjectWidgetStruct extends IWidgetStruct {
  external Pointer<ChildrenStruct> children;
}

//MultiChildRenderObjectWidgetStruct : WidgetStruct
final class MultiChildRenderObjectWidgetStruct extends Struct
    implements IMultiChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  external Pointer<ChildrenStruct> children;
}

final class AlignmentStruct extends Struct {
  @Double()
  external double x;

  @Double()
  external double y;
}

//AlignStruct : SingleChildRenderObjectWidgetStruct
final class AlignStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  external Pointer<WidgetStruct> child;

// AlignStruct
  @Int8()
  external int hasAlignment;

  external Pointer<AlignmentStruct> alignment;

  @Int8()
  external int hasWidthFactor;

  @Double()
  external double widthFactor;

  @Int8()
  external int hasHeightFactor;

  @Double()
  external double heightFactor;
}

//AppBarStruct : WidgetStruct
final class AppBarStruct extends Struct implements IWidgetStruct {
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

//AspectRatioStruct : SingleChildRenderObjectWidgetStruct
final class AspectRatioStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  external Pointer<WidgetStruct> child;

// AspectRatioStruct
  @Int8()
  external int hasValue;

  @Double()
  external double value;
}

//CheckboxStruct : WidgetStruct
final class CheckboxStruct extends Struct implements IWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //AspectRatioStruct
  @Int8()
  external int value;
}

//ColumnStruct : MultiChildRenderObjectWidgetStruct
final class ColumnStruct extends Struct
    implements IMultiChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  external Pointer<ChildrenStruct> children;

  //ColumnStruct
  @Int8()
  external int hasAlignment;

  @Int32()
  external int alignment;
}

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

//ContainerStruct : SingleChildRenderObjectWidgetStruct
final class ContainerStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  external Pointer<WidgetStruct> child;

  //ContainerStruct
  @Int8()
  external int hasAlignment;
  external Pointer<AlignmentStruct> alignment;

  @Int8()
  external int hasPadding;

  external Pointer<EdgeInsetGemoetryStruct> padding;

  @Int8()
  external int hasMargin;

  external Pointer<EdgeInsetGemoetryStruct> margin;

  @Int8()
  external int hasColor;

  external Pointer<ColorStruct> color;

  @Int8()
  external int hasWidth;

  @Double()
  external double width;

  @Int8()
  external int hasHeight;

  @Double()
  external double height;
}

//DefaultTabControllerStruct : SingleChildRenderObjectWidgetStruct
final class DefaultTabControllerStruct extends Struct
    implements ISingleChildRenderObjectWidgetStruct {
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

//IconStruct : WidgetStruct
final class IconStruct extends Struct implements IWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //IconStruct
  external Pointer<Utf8> codePoint;

  external Pointer<Utf8> fontFamily;
}

//ListViewBuilderStruct : Widget
final class ListViewBuilderStruct extends Struct implements IWidgetStruct {
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

//RowStruct : MultiChildRenderObjectWidgetStruct
final class RowStruct extends Struct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  external Pointer<ChildrenStruct> children;
  //RowStruct
  @Int8()
  external int hasAlignment;

  @Int32()
  external int alignment;
}

//ScaffoldStruct : Widget
final class ScaffoldStruct extends Struct implements IWidgetStruct {
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

//TextStruct : Widget
final class TextStruct extends Struct implements IWidgetStruct {
  //FlutterObject Struct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  //WidgetStruct
  external Pointer<Utf8> id;

  //TextStruct
  external Pointer<Utf8> value;
  @Int8()
  external int hasScaleFactor;

  @Double()
  external double scaleFactor;
}

//TextFieldStruct : Widget
final class TextFieldStruct extends Struct implements IWidgetStruct {
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
