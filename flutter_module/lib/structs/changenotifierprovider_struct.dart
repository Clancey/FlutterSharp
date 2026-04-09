// FFI struct for ChangeNotifierProvider widget
// ChangeNotifierProvider manages a ChangeNotifier and notifies consumers
// when it changes. The ChangeNotifier is managed on C# side.

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct representation of ChangeNotifierProvider widget.
final class ChangeNotifierProviderStruct extends Struct {
  // FlutterObject Struct fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct field
  external Pointer<Utf8> id;

  /// Pointer to the child widget tree.
  external Pointer<WidgetStruct> child;
}
