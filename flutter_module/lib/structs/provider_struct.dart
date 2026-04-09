// FFI struct for Provider widget
// Provider is a dependency injection widget that makes a value available
// to its descendants. The value is managed on C# side.

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct representation of Provider widget.
final class ProviderStruct extends Struct {
  // FlutterObject Struct fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct field
  external Pointer<Utf8> id;

  /// Pointer to the child widget tree.
  external Pointer<WidgetStruct> child;
}
