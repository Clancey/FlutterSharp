// Manual parser for Material TextField widget
// Part of FlutterSharp - Material Design text field with full callback support

import 'dart:ffi' hide Size;

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design TextField widget.
///
/// A Material Design text field that lets the user enter text, either with a hardware
/// keyboard or with an onscreen keyboard.
class TextFieldParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<TextFieldStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse text value
    final initialValue = map.hasValue == 1 ? parseString(map.value) : null;

    // Parse decoration text
    final hint = map.hasHint == 1 ? parseString(map.hint) : null;
    final label = map.hasLabel == 1 ? parseString(map.label) : null;
    final helperText =
        map.hasHelperText == 1 ? parseString(map.helperText) : null;
    final errorText = map.hasErrorText == 1 ? parseString(map.errorText) : null;
    final counterText =
        map.hasCounterText == 1 ? parseString(map.counterText) : null;

    // Parse callback action IDs
    final onChangedAction =
        map.hasOnChanged == 1 ? parseString(map.onChangedAction) : null;
    final onSubmittedAction =
        map.hasOnSubmitted == 1 ? parseString(map.onSubmittedAction) : null;
    final onEditingCompleteAction = map.hasOnEditingComplete == 1
        ? parseString(map.onEditingCompleteAction)
        : null;
    final onTapAction = map.hasOnTap == 1 ? parseString(map.onTapAction) : null;

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
      textAlign =
          TextAlign.values[map.textAlign.clamp(0, TextAlign.values.length - 1)];
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

    // Parse decoration fill
    final filled = map.filled == 1;

    Color? fillColor;
    if (map.hasFillColor == 1) {
      fillColor = Color(map.fillColor);
    }

    // Parse border colors
    Color? borderColor;
    if (map.hasBorderColor == 1) {
      borderColor = Color(map.borderColor);
    }

    Color? focusedBorderColor;
    if (map.hasFocusedBorderColor == 1) {
      focusedBorderColor = Color(map.focusedBorderColor);
    }

    Color? errorBorderColor;
    if (map.hasErrorBorderColor == 1) {
      errorBorderColor = Color(map.errorBorderColor);
    }

    double? borderRadius;
    if (map.hasBorderRadius == 1) {
      borderRadius = map.borderRadius;
    }

    // Parse child widgets
    Widget? prefixIconWidget;
    if (map.prefixIcon.address != 0) {
      prefixIconWidget = DynamicWidgetBuilder.buildFromPointer(
          map.prefixIcon.cast<WidgetStruct>(), buildContext);
    }

    Widget? suffixIconWidget;
    if (map.suffixIcon.address != 0) {
      suffixIconWidget = DynamicWidgetBuilder.buildFromPointer(
          map.suffixIcon.cast<WidgetStruct>(), buildContext);
    }

    Widget? prefixWidget;
    if (map.prefix.address != 0) {
      prefixWidget = DynamicWidgetBuilder.buildFromPointer(
          map.prefix.cast<WidgetStruct>(), buildContext);
    }

    Widget? suffixWidget;
    if (map.suffix.address != 0) {
      suffixWidget = DynamicWidgetBuilder.buildFromPointer(
          map.suffix.cast<WidgetStruct>(), buildContext);
    }

    // Create a stateful wrapper to manage the TextEditingController
    return _MaterialTextFieldWrapper(
      id: id,
      initialValue: initialValue ?? '',
      hint: hint,
      label: label,
      helperText: helperText,
      errorText: errorText,
      counterText: counterText,
      onChangedAction: onChangedAction,
      onSubmittedAction: onSubmittedAction,
      onEditingCompleteAction: onEditingCompleteAction,
      onTapAction: onTapAction,
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
      cursorColor: cursorColor,
      cursorWidth: cursorWidth,
      cursorHeight: cursorHeight,
      cursorRadius: cursorRadius,
      filled: filled,
      fillColor: fillColor,
      borderColor: borderColor,
      focusedBorderColor: focusedBorderColor,
      errorBorderColor: errorBorderColor,
      borderRadius: borderRadius,
      prefixIcon: prefixIconWidget,
      suffixIcon: suffixIconWidget,
      prefix: prefixWidget,
      suffix: suffixWidget,
    );
  }

  @override
  String get widgetName => "TextField";
}

/// Stateful wrapper for Material TextField to manage TextEditingController
class _MaterialTextFieldWrapper extends StatefulWidget {
  final String id;
  final String initialValue;
  final String? hint;
  final String? label;
  final String? helperText;
  final String? errorText;
  final String? counterText;
  final String? onChangedAction;
  final String? onSubmittedAction;
  final String? onEditingCompleteAction;
  final String? onTapAction;
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
  final Color? cursorColor;
  final double cursorWidth;
  final double? cursorHeight;
  final Radius? cursorRadius;
  final bool filled;
  final Color? fillColor;
  final Color? borderColor;
  final Color? focusedBorderColor;
  final Color? errorBorderColor;
  final double? borderRadius;
  final Widget? prefixIcon;
  final Widget? suffixIcon;
  final Widget? prefix;
  final Widget? suffix;

  const _MaterialTextFieldWrapper({
    required this.id,
    required this.initialValue,
    this.hint,
    this.label,
    this.helperText,
    this.errorText,
    this.counterText,
    this.onChangedAction,
    this.onSubmittedAction,
    this.onEditingCompleteAction,
    this.onTapAction,
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
    this.cursorColor,
    required this.cursorWidth,
    this.cursorHeight,
    this.cursorRadius,
    required this.filled,
    this.fillColor,
    this.borderColor,
    this.focusedBorderColor,
    this.errorBorderColor,
    this.borderRadius,
    this.prefixIcon,
    this.suffixIcon,
    this.prefix,
    this.suffix,
  });

  @override
  State<_MaterialTextFieldWrapper> createState() =>
      _MaterialTextFieldWrapperState();
}

class _MaterialTextFieldWrapperState extends State<_MaterialTextFieldWrapper> {
  late TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
  }

  @override
  void didUpdateWidget(_MaterialTextFieldWrapper oldWidget) {
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

  InputBorder? _buildBorder(Color? color) {
    if (color == null && widget.borderRadius == null) return null;

    return OutlineInputBorder(
      borderRadius: BorderRadius.circular(widget.borderRadius ?? 4.0),
      borderSide: color != null ? BorderSide(color: color) : const BorderSide(),
    );
  }

  @override
  Widget build(BuildContext context) {
    // Build InputDecoration
    final decoration = InputDecoration(
      hintText: widget.hint,
      labelText: widget.label,
      helperText: widget.helperText,
      errorText: widget.errorText,
      counterText: widget.counterText,
      filled: widget.filled,
      fillColor: widget.fillColor,
      border: _buildBorder(widget.borderColor),
      enabledBorder: _buildBorder(widget.borderColor),
      focusedBorder: _buildBorder(widget.focusedBorderColor),
      errorBorder: _buildBorder(widget.errorBorderColor),
      prefixIcon: widget.prefixIcon,
      suffixIcon: widget.suffixIcon,
      prefix: widget.prefix,
      suffix: widget.suffix,
    );

    return TextField(
      controller: _controller,
      decoration: decoration,
      keyboardType: widget.keyboardType,
      textInputAction: widget.textInputAction,
      textAlign: widget.textAlign,
      maxLines: widget.expands ? null : widget.maxLines,
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
      cursorColor: widget.cursorColor,
      cursorWidth: widget.cursorWidth,
      cursorHeight: widget.cursorHeight,
      cursorRadius: widget.cursorRadius,
      onChanged: (value) {
        // Always notify C# of the change
        raiseMauiEvent(widget.id, 'OnChange', value);
        // Invoke callback if registered
        if (widget.onChangedAction != null) {
          _invokeActionWithArgs(widget.onChangedAction!, {'value': value});
        }
      },
      onSubmitted: (value) {
        // Always notify C# of submission
        raiseMauiEvent(widget.id, 'OnSubmitted', value);
        // Invoke callback if registered
        if (widget.onSubmittedAction != null) {
          _invokeActionWithArgs(widget.onSubmittedAction!, {'value': value});
        }
      },
      onEditingComplete: widget.onEditingCompleteAction != null
          ? () => _invokeAction(widget.onEditingCompleteAction!)
          : null,
      onTap: widget.onTapAction != null
          ? () => _invokeAction(widget.onTapAction!)
          : null,
    );
  }
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
