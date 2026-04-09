// FlutterSharp Manual Implementation
// CupertinoTextField FFI Struct

import 'dart:ffi';
import 'package:ffi/ffi.dart';
import '../flutter_sharp_structs.dart';

/// FFI struct representation of CupertinoTextField widget.
/// This struct is used to pass widget data across the FFI boundary.
///
/// An iOS-style text field.
///
/// A text field lets the user enter text, either with a hardware keyboard or with
/// an onscreen keyboard.
final class CupertinoTextFieldStruct extends Struct {
  // FlutterObject Struct base fields
  external Pointer handle;
  external Pointer managedHandle;
  external Pointer<Utf8> widgetType;

  // WidgetStruct base field
  external Pointer<Utf8> id;

  // Text value
  @Int8()
  external int hasValue;
  external Pointer<Utf8> value;

  // Placeholder text
  @Int8()
  external int hasPlaceholder;
  external Pointer<Utf8> placeholder;

  // Callback: onChanged
  @Int8()
  external int hasOnChanged;
  external Pointer<Utf8> onChangedAction;

  // Callback: onSubmitted
  @Int8()
  external int hasOnSubmitted;
  external Pointer<Utf8> onSubmittedAction;

  // Callback: onEditingComplete
  @Int8()
  external int hasOnEditingComplete;
  external Pointer<Utf8> onEditingCompleteAction;

  // Callback: onTap
  @Int8()
  external int hasOnTap;
  external Pointer<Utf8> onTapAction;

  // Padding - EdgeInsets (left, top, right, bottom)
  @Int8()
  external int hasPadding;
  @Double()
  external double paddingLeft;
  @Double()
  external double paddingTop;
  @Double()
  external double paddingRight;
  @Double()
  external double paddingBottom;

  // Text style
  @Int8()
  external int hasStyle;
  @Double()
  external double styleFontSize;
  @Uint32()
  external int styleColor;
  @Int32()
  external int styleFontWeight;

  // Placeholder style
  @Int8()
  external int hasPlaceholderStyle;
  @Double()
  external double placeholderStyleFontSize;
  @Uint32()
  external int placeholderStyleColor;
  @Int32()
  external int placeholderStyleFontWeight;

  // Cursor color
  @Int8()
  external int hasCursorColor;
  @Uint32()
  external int cursorColor;

  // Cursor width
  @Int8()
  external int hasCursorWidth;
  @Double()
  external double cursorWidth;

  // Cursor height
  @Int8()
  external int hasCursorHeight;
  @Double()
  external double cursorHeight;

  // Cursor radius
  @Int8()
  external int hasCursorRadius;
  @Double()
  external double cursorRadius;

  // Box decoration
  @Int8()
  external int hasDecoration;
  @Uint32()
  external int decorationColor;
  @Double()
  external double decorationBorderRadius;
  @Uint32()
  external int decorationBorderColor;
  @Double()
  external double decorationBorderWidth;

  // Keyboard type
  @Int8()
  external int hasKeyboardType;
  @Int32()
  external int keyboardType;

  // Text input action
  @Int8()
  external int hasTextInputAction;
  @Int32()
  external int textInputAction;

  // Text align
  @Int8()
  external int hasTextAlign;
  @Int32()
  external int textAlign;

  // Max lines
  @Int8()
  external int hasMaxLines;
  @Int32()
  external int maxLines;

  // Min lines
  @Int8()
  external int hasMinLines;
  @Int32()
  external int minLines;

  // Max length
  @Int8()
  external int hasMaxLength;
  @Int32()
  external int maxLength;

  // Boolean flags
  @Int8()
  external int enabled;
  @Int8()
  external int readOnly;
  @Int8()
  external int obscureText;
  @Int8()
  external int autofocus;
  @Int8()
  external int autocorrect;
  @Int8()
  external int enableSuggestions;
  @Int8()
  external int expands;

  // Obscuring character
  @Int8()
  external int hasObscuringCharacter;
  external Pointer<Utf8> obscuringCharacter;

  // Prefix and suffix widgets
  external Pointer<WidgetStruct> prefix;
  external Pointer<WidgetStruct> suffix;

  // Clear button mode
  @Int8()
  external int hasClearButtonMode;
  @Int32()
  external int clearButtonMode;
}
