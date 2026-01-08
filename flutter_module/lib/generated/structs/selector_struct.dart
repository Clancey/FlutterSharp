// FFI struct for Selector widget
// Selector is an optimized Consumer that only rebuilds when a selected
// subset of the provider value changes.

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../../flutter_sharp_structs.dart';

/// FFI struct representation of Selector widget.
final class SelectorStruct extends Struct {
  // FlutterObject Struct fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct field
  external Pointer<Utf8> id;

  /// Pointer to the built widget tree.
  /// This is populated by the C# Selector builder callback.
  external Pointer<WidgetStruct> builtChild;
}
