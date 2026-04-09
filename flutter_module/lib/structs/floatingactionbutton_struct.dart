import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

final class FloatingActionButtonStruct extends Struct {
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;
  external Pointer<Utf8> id;

  @Int8()
  external int hasOnPressedAction;

  external Pointer<Utf8> onPressedAction;

  @Int8()
  external int hasTooltip;

  external Pointer<Utf8> tooltip;

  @Int8()
  external int hasForegroundColor;

  @Uint32()
  external int foregroundColor;

  @Int8()
  external int hasBackgroundColor;

  @Uint32()
  external int backgroundColor;

  @Int8()
  external int hasFocusColor;

  @Uint32()
  external int focusColor;

  @Int8()
  external int hasHoverColor;

  @Uint32()
  external int hoverColor;

  @Int8()
  external int hasSplashColor;

  @Uint32()
  external int splashColor;

  @Int8()
  external int hasHeroTag;

  external Pointer<Utf8> heroTag;

  @Int8()
  external int hasElevation;

  @Double()
  external double elevation;

  @Int8()
  external int hasFocusElevation;

  @Double()
  external double focusElevation;

  @Int8()
  external int hasHoverElevation;

  @Double()
  external double hoverElevation;

  @Int8()
  external int hasHighlightElevation;

  @Double()
  external double highlightElevation;

  @Int8()
  external int hasDisabledElevation;

  @Double()
  external double disabledElevation;

  @Int8()
  external int mini;

  @Int32()
  external int clipBehavior;

  @Int8()
  external int autofocus;

  @Int8()
  external int isExtended;

  @Int8()
  external int hasEnableFeedback;

  @Int8()
  external int enableFeedback;

  external Pointer<Void> focusNode;
  external Pointer<WidgetStruct> child;
}
