// Manual parser for CupertinoTextField widget
// Part of FlutterSharp Phase 5 - Cupertino Widgets

import 'dart:ffi' hide Size;

import 'package:flutter/cupertino.dart';
import 'package:flutter/services.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import '../generated/structs/cupertinotextfield_struct.dart';

/// Parser for iOS-style CupertinoTextField widget.
///
/// An iOS-style text field that lets the user enter text, either with a hardware
/// keyboard or with an onscreen keyboard.
class CupertinoTextFieldParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<CupertinoTextFieldStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse text value
    final initialValue = map.hasValue == 1 ? parseString(map.value) : null;

    // Parse placeholder
    final placeholder = map.hasPlaceholder == 1 ? parseString(map.placeholder) : null;

    // Parse callback action IDs
    final onChangedAction = map.hasOnChanged == 1
        ? parseString(map.onChangedAction)
        : null;
    final onSubmittedAction = map.hasOnSubmitted == 1
        ? parseString(map.onSubmittedAction)
        : null;
    final onEditingCompleteAction = map.hasOnEditingComplete == 1
        ? parseString(map.onEditingCompleteAction)
        : null;
    final onTapAction = map.hasOnTap == 1
        ? parseString(map.onTapAction)
        : null;

    // Parse padding
    EdgeInsetsGeometry? padding;
    if (map.hasPadding == 1) {
      padding = EdgeInsets.fromLTRB(
        map.paddingLeft,
        map.paddingTop,
        map.paddingRight,
        map.paddingBottom,
      );
    }

    // Parse text style
    TextStyle? style;
    if (map.hasStyle == 1) {
      style = TextStyle(
        fontSize: map.styleFontSize,
        color: Color(map.styleColor),
        fontWeight: _fontWeightFromInt(map.styleFontWeight),
      );
    }

    // Parse placeholder style
    TextStyle? placeholderStyle;
    if (map.hasPlaceholderStyle == 1) {
      placeholderStyle = TextStyle(
        fontSize: map.placeholderStyleFontSize,
        color: Color(map.placeholderStyleColor),
        fontWeight: _fontWeightFromInt(map.placeholderStyleFontWeight),
      );
    }

    // Parse cursor color
    Color? cursorColor;
    if (map.hasCursorColor == 1) {
      cursorColor = Color(map.cursorColor);
    }

    // Parse cursor dimensions
    double cursorWidth = 2.0; // Default
    if (map.hasCursorWidth == 1) {
      cursorWidth = map.cursorWidth;
    }

    double? cursorHeight;
    if (map.hasCursorHeight == 1) {
      cursorHeight = map.cursorHeight;
    }

    Radius? cursorRadius;
    if (map.hasCursorRadius == 1) {
      cursorRadius = Radius.circular(map.cursorRadius);
    }

    // Parse box decoration
    BoxDecoration? decoration;
    if (map.hasDecoration == 1) {
      decoration = BoxDecoration(
        color: Color(map.decorationColor),
        borderRadius: BorderRadius.circular(map.decorationBorderRadius),
        border: map.decorationBorderWidth > 0
            ? Border.all(
                color: Color(map.decorationBorderColor),
                width: map.decorationBorderWidth,
              )
            : null,
      );
    }

    // Parse keyboard type
    TextInputType keyboardType = TextInputType.text;
    if (map.hasKeyboardType == 1) {
      keyboardType = _textInputTypeFromInt(map.keyboardType);
    }

    // Parse text input action
    TextInputAction? textInputAction;
    if (map.hasTextInputAction == 1) {
      textInputAction = _textInputActionFromInt(map.textInputAction);
    }

    // Parse text alignment
    TextAlign textAlign = TextAlign.start;
    if (map.hasTextAlign == 1) {
      textAlign = TextAlign.values[map.textAlign.clamp(0, TextAlign.values.length - 1)];
    }

    // Parse line configuration
    int? maxLines = 1;
    if (map.hasMaxLines == 1) {
      maxLines = map.maxLines;
    }

    int? minLines;
    if (map.hasMinLines == 1) {
      minLines = map.minLines;
    }

    int? maxLength;
    if (map.hasMaxLength == 1) {
      maxLength = map.maxLength;
    }

    // Parse boolean flags
    final enabled = map.enabled == 1;
    final readOnly = map.readOnly == 1;
    final obscureText = map.obscureText == 1;
    final autofocus = map.autofocus == 1;
    final autocorrect = map.autocorrect == 1;
    final enableSuggestions = map.enableSuggestions == 1;
    final expands = map.expands == 1;

    // Parse obscuring character
    String obscuringCharacter = '•';
    if (map.hasObscuringCharacter == 1) {
      final char = parseString(map.obscuringCharacter);
      if (char != null && char.isNotEmpty) {
        obscuringCharacter = char;
      }
    }

    // Parse prefix widget
    Widget? prefixWidget;
    if (map.prefix.address != 0) {
      prefixWidget = DynamicWidgetBuilder.buildFromPointer(
          map.prefix.cast<WidgetStruct>(), buildContext);
    }

    // Parse suffix widget
    Widget? suffixWidget;
    if (map.suffix.address != 0) {
      suffixWidget = DynamicWidgetBuilder.buildFromPointer(
          map.suffix.cast<WidgetStruct>(), buildContext);
    }

    // Parse clear button mode
    OverlayVisibilityMode clearButtonMode = OverlayVisibilityMode.never;
    if (map.hasClearButtonMode == 1) {
      clearButtonMode = OverlayVisibilityMode.values[
          map.clearButtonMode.clamp(0, OverlayVisibilityMode.values.length - 1)];
    }

    // Create a stateful wrapper to manage the TextEditingController
    return _CupertinoTextFieldWrapper(
      id: id,
      initialValue: initialValue ?? '',
      placeholder: placeholder,
      onChangedAction: onChangedAction,
      onSubmittedAction: onSubmittedAction,
      onEditingCompleteAction: onEditingCompleteAction,
      onTapAction: onTapAction,
      padding: padding,
      style: style,
      placeholderStyle: placeholderStyle,
      cursorColor: cursorColor,
      cursorWidth: cursorWidth,
      cursorHeight: cursorHeight,
      cursorRadius: cursorRadius,
      decoration: decoration,
      keyboardType: keyboardType,
      textInputAction: textInputAction,
      textAlign: textAlign,
      maxLines: maxLines,
      minLines: minLines,
      maxLength: maxLength,
      enabled: enabled,
      readOnly: readOnly,
      obscureText: obscureText,
      obscuringCharacter: obscuringCharacter,
      autofocus: autofocus,
      autocorrect: autocorrect,
      enableSuggestions: enableSuggestions,
      expands: expands,
      prefix: prefixWidget,
      suffix: suffixWidget,
      clearButtonMode: clearButtonMode,
    );
  }

  @override
  String get widgetName => "CupertinoTextField";
}

/// Stateful wrapper for CupertinoTextField to manage TextEditingController
class _CupertinoTextFieldWrapper extends StatefulWidget {
  final String id;
  final String initialValue;
  final String? placeholder;
  final String? onChangedAction;
  final String? onSubmittedAction;
  final String? onEditingCompleteAction;
  final String? onTapAction;
  final EdgeInsetsGeometry? padding;
  final TextStyle? style;
  final TextStyle? placeholderStyle;
  final Color? cursorColor;
  final double cursorWidth;
  final double? cursorHeight;
  final Radius? cursorRadius;
  final BoxDecoration? decoration;
  final TextInputType keyboardType;
  final TextInputAction? textInputAction;
  final TextAlign textAlign;
  final int? maxLines;
  final int? minLines;
  final int? maxLength;
  final bool enabled;
  final bool readOnly;
  final bool obscureText;
  final String obscuringCharacter;
  final bool autofocus;
  final bool autocorrect;
  final bool enableSuggestions;
  final bool expands;
  final Widget? prefix;
  final Widget? suffix;
  final OverlayVisibilityMode clearButtonMode;

  const _CupertinoTextFieldWrapper({
    required this.id,
    required this.initialValue,
    this.placeholder,
    this.onChangedAction,
    this.onSubmittedAction,
    this.onEditingCompleteAction,
    this.onTapAction,
    this.padding,
    this.style,
    this.placeholderStyle,
    this.cursorColor,
    required this.cursorWidth,
    this.cursorHeight,
    this.cursorRadius,
    this.decoration,
    required this.keyboardType,
    this.textInputAction,
    required this.textAlign,
    this.maxLines,
    this.minLines,
    this.maxLength,
    required this.enabled,
    required this.readOnly,
    required this.obscureText,
    required this.obscuringCharacter,
    required this.autofocus,
    required this.autocorrect,
    required this.enableSuggestions,
    required this.expands,
    this.prefix,
    this.suffix,
    required this.clearButtonMode,
  });

  @override
  State<_CupertinoTextFieldWrapper> createState() => _CupertinoTextFieldWrapperState();
}

class _CupertinoTextFieldWrapperState extends State<_CupertinoTextFieldWrapper> {
  late TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
  }

  @override
  void didUpdateWidget(_CupertinoTextFieldWrapper oldWidget) {
    super.didUpdateWidget(oldWidget);
    // Update controller text if the initial value changed from C# side
    if (widget.initialValue != oldWidget.initialValue &&
        widget.initialValue != _controller.text) {
      _controller.text = widget.initialValue;
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return CupertinoTextField(
      controller: _controller,
      placeholder: widget.placeholder,
      padding: widget.padding ?? const EdgeInsets.all(6.0),
      style: widget.style,
      placeholderStyle: widget.placeholderStyle ??
          const TextStyle(
            fontWeight: FontWeight.w400,
            color: CupertinoColors.placeholderText,
          ),
      cursorColor: widget.cursorColor,
      cursorWidth: widget.cursorWidth,
      cursorHeight: widget.cursorHeight,
      cursorRadius: widget.cursorRadius ?? const Radius.circular(2.0),
      decoration: widget.decoration ?? _kDefaultRoundedBorderDecoration,
      keyboardType: widget.keyboardType,
      textInputAction: widget.textInputAction,
      textAlign: widget.textAlign,
      maxLines: widget.maxLines,
      minLines: widget.minLines,
      maxLength: widget.maxLength,
      enabled: widget.enabled,
      readOnly: widget.readOnly,
      obscureText: widget.obscureText,
      obscuringCharacter: widget.obscuringCharacter,
      autofocus: widget.autofocus,
      autocorrect: widget.autocorrect,
      enableSuggestions: widget.enableSuggestions,
      expands: widget.expands,
      prefix: widget.prefix,
      suffix: widget.suffix,
      clearButtonMode: widget.clearButtonMode,
      onChanged: widget.onChangedAction != null
          ? (value) {
              raiseMauiEvent(widget.id, 'OnChange', value);
              _invokeActionWithArgs(widget.onChangedAction!, {'value': value});
            }
          : null,
      onSubmitted: widget.onSubmittedAction != null
          ? (value) {
              raiseMauiEvent(widget.id, 'OnSubmitted', value);
              _invokeActionWithArgs(widget.onSubmittedAction!, {'value': value});
            }
          : null,
      onEditingComplete: widget.onEditingCompleteAction != null
          ? () => _invokeAction(widget.onEditingCompleteAction!)
          : null,
      onTap: widget.onTapAction != null
          ? () => _invokeAction(widget.onTapAction!)
          : null,
    );
  }
}

/// Default rounded border decoration for CupertinoTextField
const BoxDecoration _kDefaultRoundedBorderDecoration = BoxDecoration(
  color: CupertinoDynamicColor.withBrightness(
    color: CupertinoColors.white,
    darkColor: CupertinoColors.black,
  ),
  border: _kDefaultRoundedBorder,
  borderRadius: BorderRadius.all(Radius.circular(5.0)),
);

const Border _kDefaultRoundedBorder = Border.fromBorderSide(
  BorderSide(
    color: CupertinoDynamicColor.withBrightness(
      color: Color(0x33000000),
      darkColor: Color(0x33FFFFFF),
    ),
    width: 0.0,
  ),
);

/// Convert integer to FontWeight
FontWeight _fontWeightFromInt(int value) {
  return switch (value) {
    100 => FontWeight.w100,
    200 => FontWeight.w200,
    300 => FontWeight.w300,
    400 => FontWeight.w400,
    500 => FontWeight.w500,
    600 => FontWeight.w600,
    700 => FontWeight.w700,
    800 => FontWeight.w800,
    900 => FontWeight.w900,
    _ => FontWeight.w400,
  };
}

/// Convert integer to TextInputType
TextInputType _textInputTypeFromInt(int value) {
  return switch (value) {
    0 => TextInputType.text,
    1 => TextInputType.multiline,
    2 => TextInputType.number,
    3 => TextInputType.phone,
    4 => TextInputType.datetime,
    5 => TextInputType.emailAddress,
    6 => TextInputType.url,
    7 => TextInputType.visiblePassword,
    8 => TextInputType.name,
    9 => TextInputType.streetAddress,
    10 => TextInputType.none,
    _ => TextInputType.text,
  };
}

/// Convert integer to TextInputAction
TextInputAction _textInputActionFromInt(int value) {
  return switch (value) {
    0 => TextInputAction.none,
    1 => TextInputAction.unspecified,
    2 => TextInputAction.done,
    3 => TextInputAction.go,
    4 => TextInputAction.search,
    5 => TextInputAction.send,
    6 => TextInputAction.next,
    7 => TextInputAction.previous,
    8 => TextInputAction.continueAction,
    9 => TextInputAction.join,
    10 => TextInputAction.route,
    11 => TextInputAction.emergencyCall,
    12 => TextInputAction.newline,
    _ => TextInputAction.done,
  };
}

/// Invoke a void callback action via the method channel
void _invokeAction(String actionId) {
  raiseMauiEvent(actionId, "invoke", null);
}

/// Invoke a callback action with arguments via the method channel
void _invokeActionWithArgs(String actionId, Map<String, dynamic> args) {
  raiseMauiEvent(actionId, "invoke", args);
}
