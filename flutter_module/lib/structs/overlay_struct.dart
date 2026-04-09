import 'dart:ffi';
import 'package:ffi/ffi.dart';

final class OverlayStruct extends Struct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  external Pointer<Utf8> id;

  external Pointer<Void> initialEntries;

  @Int32()
  external int clipBehavior;
}
