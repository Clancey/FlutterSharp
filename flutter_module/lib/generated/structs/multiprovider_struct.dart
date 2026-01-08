// FFI struct for MultiProvider widget
// MultiProvider composes multiple providers into a nested widget tree.

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../../flutter_sharp_structs.dart';

/// FFI struct representation of MultiProvider widget.
final class MultiProviderStruct extends Struct {
  // FlutterObject Struct fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct field
  external Pointer<Utf8> id;

  /// Pointer to the built widget tree (the outermost provider).
  external Pointer<WidgetStruct> builtChild;
}
