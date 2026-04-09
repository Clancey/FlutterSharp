// FFI struct for Consumer widget
// Consumer listens to a Provider and rebuilds when the value changes.
// The C# Consumer handles subscription and calls the builder callback.

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct representation of Consumer widget.
final class ConsumerStruct extends Struct {
  // FlutterObject Struct fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct field
  external Pointer<Utf8> id;

  /// Pointer to the built widget tree.
  /// This is populated by the C# Consumer builder callback.
  external Pointer<WidgetStruct> builtChild;
}
