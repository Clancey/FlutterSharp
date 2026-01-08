// FlutterSharp Manual Implementation
// CupertinoSwitch FFI Struct

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../../flutter_sharp_structs.dart';

/// FFI struct representation of CupertinoSwitch widget.
/// This struct is used to pass widget data across the FFI boundary.
///
/// An iOS-style switch.
///
/// Used to toggle the on/off state of a single setting.
///
/// The switch itself does not maintain any state. Instead, when the state of
/// the switch changes, the widget calls the onChanged callback.
final class CupertinoSwitchStruct extends Struct {
  // FlutterObject Struct base fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct base field
  external Pointer<Utf8> id;

  /// Whether this switch is on or off.
  @Int8()
  external int value;

  // Has flag for nullable callback: onChanged
  @Int8()
  external int hasOnChanged;

  /// Action identifier for onChanged callback.
  external Pointer<Utf8> onChangedAction;

  // Has flag for nullable callback: onFocusChange
  @Int8()
  external int hasOnFocusChange;

  /// Action identifier for onFocusChange callback.
  external Pointer<Utf8> onFocusChangeAction;

  // Active track color (when switch is ON)
  @Int8()
  external int hasActiveTrackColor;
  @Uint32()
  external int activeTrackColor;

  // Inactive track color (when switch is OFF)
  @Int8()
  external int hasInactiveTrackColor;
  @Uint32()
  external int inactiveTrackColor;

  // Thumb color
  @Int8()
  external int hasThumbColor;
  @Uint32()
  external int thumbColor;

  // Track outline color
  @Int8()
  external int hasTrackOutlineColor;
  @Uint32()
  external int trackOutlineColor;

  // Track outline width
  @Int8()
  external int hasTrackOutlineWidth;
  @Double()
  external double trackOutlineWidth;

  // Focus color
  @Int8()
  external int hasFocusColor;
  @Uint32()
  external int focusColor;

  // Apply theme
  @Int8()
  external int hasApplyTheme;
  @Int8()
  external int applyTheme;

  // Autofocus
  @Int8()
  external int autofocus;

  // Drag start behavior
  @Int32()
  external int dragStartBehavior;
}
