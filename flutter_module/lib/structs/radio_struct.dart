import 'dart:ffi';
import 'package:ffi/ffi.dart';

final class RadioStruct extends Struct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  external Pointer<Utf8> id;

  @Int32()
  external int value;

  @Int8()
  external int hasGroupValue;

  @Int32()
  external int groupValue;

  @Int8()
  external int hasOnChangedAction;

  external Pointer<Utf8> onChangedAction;

  @Int8()
  external int toggleable;

  @Int8()
  external int hasActiveColor;

  @Int32()
  external int activeColor;

  @Int8()
  external int hasFocusColor;

  @Int32()
  external int focusColor;

  @Int8()
  external int hasHoverColor;

  @Int32()
  external int hoverColor;

  @Int8()
  external int hasSplashRadius;

  @Double()
  external double splashRadius;

  @Int8()
  external int hasMaterialTapTargetSize;

  @Int32()
  external int materialTapTargetSize;

  @Int8()
  external int autofocus;
}
