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

class IFlutterObjectStruct{  
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
}

class FlutterObjectStruct extends Struct implements IFlutterObjectStruct {
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
}

class IWidgetStruct extends IFlutterObjectStruct{  
  Pointer<Utf8> id;
}

//WidgetStruct : FlutterOBjectStruct
class WidgetStruct extends Struct implements IWidgetStruct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
}
class ISingleChildRenderObjectWidgetStruct extends IWidgetStruct{  
  Pointer<WidgetStruct> child;
}

//SingleChildRenderObjectWidgetStruct : WidgetStruct
class SingleChildRenderObjectWidgetStruct extends Struct implements ISingleChildRenderObjectWidgetStruct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  Pointer<WidgetStruct> child;
}
class IMultiChildRenderObjectWidgetStruct extends IWidgetStruct{  
  Pointer children;
  int childrenLength;
}

//MultiChildRenderObjectWidgetStruct : WidgetStruct
class MultiChildRenderObjectWidgetStruct extends Struct implements IWidgetStruct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  Pointer children;
  @Int32()
  int childrenLength;
}

class AlignmentStruct extends Struct{
  @Double()
  double x;
  
  @Double()
  double y;
}
//AlignStruct : SingleChildRenderObjectWidgetStruct 
class AlignStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  Pointer<WidgetStruct> child;

// AlignStruct
  @Int8()
  int hasAlignment;
  
  Pointer<AlignmentStruct> alignment;

  @Int8()
  int hasWidthFactor;
  
  @Double()
  double widthFactor;

  @Int8()
  int hasHeightFactor;
  
  @Double()
  double heightFactor;

}



//AppBarStruct : WidgetStruct
class AppBarStruct extends Struct implements IWidgetStruct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //AppBarStruct
  Pointer<WidgetStruct> title;
  Pointer<WidgetStruct> bottom;
}


//AspectRatioStruct : SingleChildRenderObjectWidgetStruct 
class AspectRatioStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  Pointer<WidgetStruct> child;

// AspectRatioStruct
  @Int8()
  int hasValue;
  
  @Double()
  double value;

}


//CheckboxStruct : WidgetStruct 
class CheckboxStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //AspectRatioStruct
  @Int8()
  int value;

}



//ColumnStruct : MultiChildRenderObjectWidgetStruct
class ColumnStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  Pointer children;
  @Int32()
  int childrenLength;

  //ColumnStruct  
  @Int8()
  int hasAlignment;
  
  @Int32()
  int alignment;
}

class EdgeInsetGemoetryStruct extends Struct{
  @Double()
  double left;
  @Double()
  double top;
  @Double()
  double right;
  @Double()
  double bottom;
}

class ColorStruct extends Struct{
  @Int8()
  int red;
  @Int8()
  int green;
  @Int8()
  int blue;
  @Int8()
  int alpha;
}

//ContainerStruct : SingleChildRenderObjectWidgetStruct 
class ContainerStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  Pointer<WidgetStruct> child;

  //ContainerStruct  
  @Int8()
  int hasAlignment;
  Pointer<AlignmentStruct> alignment;
  
  @Int8()
  int hasPadding;
  
  Pointer<EdgeInsetGemoetryStruct> padding;

  @Int8()
  int hasMargin;
  
  Pointer<EdgeInsetGemoetryStruct> margin;

  @Int8()
  int hasColor;
  
  Pointer<ColorStruct> color;

  @Int8()
  int hasWidth;
  
  @Double()
  double width;

  @Int8()
  int hasHeight;
  
  @Double()
  double height;

}


//DefaultTabControllerStruct : SingleChildRenderObjectWidgetStruct 
class DefaultTabControllerStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //SingleChildRenderObjectWidgetStruct
  Pointer<WidgetStruct> child;

// DefaultTabControllerStruct
  @Int32()
  int tabCount;

}



//IconStruct : WidgetStruct 
class IconStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //IconStruct
  Pointer<Utf8> codePoint;
  
  Pointer<Utf8> fontFamily;

}


//ListViewBuilderStruct : Widget 
class ListViewBuilderStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //ListViewBuilderStruct
  @Int64()
  int itemCount;

}

//RowStruct : MultiChildRenderObjectWidgetStruct
class RowStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;
  //MultiChildRenderObjectWidgetStruct
  Pointer children;
  @Int32()
  int childrenLength;

  //RowStruct  
  @Int8()
  int hasAlignment;
  
  @Int32()
  int alignment;
}


//ScaffoldStruct : Widget 
class ScaffoldStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //ScaffoldStruct
  Pointer<AppBarStruct> appBar;
  Pointer<WidgetStruct> floatingActionButton;
  Pointer<WidgetStruct> drawer;
  Pointer<WidgetStruct> body;

}

//TextStruct : Widget 
class TextStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //TextStruct
  Pointer<Utf8> value;
  @Int8()
  int hasScaleFactor;
  
  @Double()
  double scaleFactor;
}

//TextFieldStruct : Widget 
class TextFieldStruct extends Struct{
  //FlutterObject Struct
  Pointer handle;  
  Pointer managedHandle;
  Pointer<Utf8> widgetType;
  //WidgetStruct
  Pointer<Utf8> id;

  //TextFieldStruct
  Pointer<Utf8> value;
  Pointer<Utf8> hint;
}