import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct for the ErrorBoundary widget.
final class ErrorBoundaryStruct extends Struct {
  // FlutterObject Struct base fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct base field
  external Pointer<Utf8> id;

  /// The child widget wrapped by this error boundary.
  external Pointer<WidgetStruct> child;

  /// Whether to show errors in the global error overlay.
  @Int8()
  external int showInOverlay;

  /// Whether to report errors to C# for logging.
  @Int8()
  external int reportToNative;

  /// Has flag for widgetTypeName.
  @Int8()
  external int hasWidgetTypeName;

  /// Optional widget type name for error context.
  external Pointer<Utf8> widgetTypeName;

  /// Has flag for onError callback.
  @Int8()
  external int hasOnErrorAction;

  /// Callback ID for the onError callback.
  external Pointer<Utf8> onErrorAction;
}
