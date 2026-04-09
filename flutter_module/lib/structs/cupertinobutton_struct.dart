import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

final class CupertinoButtonStruct extends Struct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  external Pointer<Utf8> id;

  @Int8()
  external int hasOnPressedAction;

  external Pointer<Utf8> onPressedAction;

  @Int8()
  external int hasOnLongPressAction;

  external Pointer<Utf8> onLongPressAction;

  @Int8()
  external int hasOnFocusChangeAction;

  external Pointer<Utf8> onFocusChangeAction;

  @Int8()
  external int hasPadding;

  @Double()
  external double paddingLeft;

  @Double()
  external double paddingTop;

  @Double()
  external double paddingRight;

  @Double()
  external double paddingBottom;

  @Int8()
  external int hasColor;

  @Uint32()
  external int color;

  @Int8()
  external int hasForegroundColor;

  @Uint32()
  external int foregroundColor;

  @Int8()
  external int hasDisabledColor;

  @Uint32()
  external int disabledColor;

  @Int8()
  external int hasMinimumSize;

  @Double()
  external double minimumSizeWidth;

  @Double()
  external double minimumSizeHeight;

  @Int8()
  external int hasPressedOpacity;

  @Double()
  external double pressedOpacity;

  @Int8()
  external int hasBorderRadius;

  @Double()
  external double borderRadius;

  @Int8()
  external int hasAlignment;

  @Double()
  external double alignmentX;

  @Double()
  external double alignmentY;

  @Int32()
  external int sizeStyle;

  @Int8()
  external int autofocus;

  @Int8()
  external int isFilled;

  external Pointer<WidgetStruct> child;
}
