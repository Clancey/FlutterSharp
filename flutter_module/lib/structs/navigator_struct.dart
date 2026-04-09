// Manual implementation for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct for Navigator widget.
/// Manages a stack of routes/pages for navigation.
final class NavigatorStruct extends Struct {
  // Inherited from WidgetStruct
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  external Pointer<Utf8> id;

  // Initial route name (e.g., "/", "/home", "/settings")
  external Pointer<Utf8> initialRoute;
  @Int8()
  external int hasInitialRoute;

  // Current route name (tracks which route is currently displayed)
  external Pointer<Utf8> currentRoute;
  @Int8()
  external int hasCurrentRoute;

  // Serialized route names (pipe-separated: "/|/home|/settings|/profile")
  external Pointer<Utf8> routeNames;
  @Int8()
  external int hasRouteNames;

  // Current child widget (the active route's widget)
  external Pointer<WidgetStruct> currentChild;

  // Whether to maintain state of inactive routes (default true)
  @Int8()
  external int maintainState;

  // Restoration scope ID for state restoration
  external Pointer<Utf8> restorationScopeId;
  @Int8()
  external int hasRestorationScopeId;

  // Clip behavior for the Navigator
  @Int32()
  external int clipBehavior;

  // Navigator ID for multiple navigators (e.g., nested navigation)
  external Pointer<Utf8> navigatorId;
  @Int8()
  external int hasNavigatorId;

  // Callback action IDs (for handling navigation events)
  external Pointer<Utf8> onRouteChangedAction;
  @Int8()
  external int hasOnRouteChangedAction;

  external Pointer<Utf8> onPopAction;
  @Int8()
  external int hasOnPopAction;

  /// Flag indicating whether OnGenerateRoute callback is set on the C# side.
  /// When 1, the Navigator can resolve unknown routes dynamically.
  @Int8()
  external int hasOnGenerateRoute;

  /// Flag indicating whether OnUnknownRoute callback is set on the C# side.
  /// When 1, the Navigator can provide a fallback route for completely unknown routes.
  @Int8()
  external int hasOnUnknownRoute;

  // Route transition fields (for MaterialPageRoute, CupertinoPageRoute, etc.)

  /// The type of transition animation to use.
  /// 0 = None, 1 = Material, 2 = Cupertino, 3 = Fade, 4 = SlideBottom, 5 = SlideRight, 6 = Zoom
  @Int32()
  external int transitionType;

  /// The duration of the push transition in milliseconds.
  @Int32()
  external int transitionDurationMs;

  /// The duration of the pop (reverse) transition in milliseconds.
  @Int32()
  external int reverseTransitionDurationMs;

  /// Whether this route is a full-screen dialog.
  @Int8()
  external int fullscreenDialog;

  /// Whether this route is opaque (covers the entire navigator area).
  @Int8()
  external int opaque;

  /// Whether a push transition is currently in progress.
  @Int8()
  external int isTransitioning;

  /// Whether this is a pop operation (reverse transition).
  @Int8()
  external int isPopping;

  /// Previous route child widget pointer (for transition animations).
  external Pointer<WidgetStruct> previousChild;

  /// Route arguments (JSON-serialized).
  external Pointer<Utf8> routeArguments;
  @Int8()
  external int hasRouteArguments;
}
