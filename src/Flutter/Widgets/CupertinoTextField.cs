// FlutterSharp Manual Implementation
// CupertinoTextField Widget

using System;
using System.Collections;
using System.Collections.Generic;
using Flutter;
using Flutter.Enums;
using Flutter.Gestures;
using Flutter.UI;
using Flutter.Structs;
using Flutter.Widgets;
using Flutter.Services;
using Flutter.Cupertino;

namespace Flutter.Widgets
{
    /// <summary>
    /// An iOS-style text field.
    ///
    /// A text field lets the user enter text, either with a hardware keyboard or with
    /// an onscreen keyboard.
    ///
    /// This widget corresponds to both a UITextField and an editable UITextView on iOS.
    ///
    /// The text field calls the onChanged callback whenever the user changes the text
    /// in the field. If the user indicates that they are done typing in the field
    /// (e.g., by pressing a button on the soft keyboard), the text field calls the
    /// onSubmitted callback.
    ///
    /// See also:
    ///  * https://developer.apple.com/design/human-interface-guidelines/text-fields
    /// </summary>
    public class CupertinoTextField : StatefulWidget
    {
        private string _currentValue = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="CupertinoTextField"/> class.
        /// </summary>
        /// <param name="value">The initial text value.</param>
        /// <param name="placeholder">Placeholder text that appears when the text field is empty.</param>
        /// <param name="onChanged">Called when the user changes the text in the field.</param>
        /// <param name="onSubmitted">Called when the user indicates they are done editing (e.g., pressing done on keyboard).</param>
        /// <param name="onEditingComplete">Called when the user submits editable content.</param>
        /// <param name="onTap">Called for each distinct tap except for every second tap of a double tap.</param>
        /// <param name="padding">The amount of space to surround the text inside the field.</param>
        /// <param name="style">The style to use for the text being edited.</param>
        /// <param name="placeholderStyle">The style to use for the placeholder text.</param>
        /// <param name="cursorColor">The color of the cursor.</param>
        /// <param name="cursorWidth">How thick the cursor will be.</param>
        /// <param name="cursorHeight">How tall the cursor will be.</param>
        /// <param name="cursorRadius">The radius of the cursor corner.</param>
        /// <param name="decoration">The decoration to paint behind the text field.</param>
        /// <param name="keyboardType">The type of keyboard to use for editing the text.</param>
        /// <param name="textInputAction">The type of action button to use for the keyboard.</param>
        /// <param name="textAlign">How the text should be aligned horizontally.</param>
        /// <param name="maxLines">The maximum number of lines for the text to span.</param>
        /// <param name="minLines">The minimum number of lines to occupy when the content spans fewer lines.</param>
        /// <param name="maxLength">The maximum number of characters (Unicode scalar values) to allow in the text field.</param>
        /// <param name="enabled">Whether the text field is enabled.</param>
        /// <param name="readOnly">Whether the text can be changed.</param>
        /// <param name="obscureText">Whether to hide the text being edited.</param>
        /// <param name="obscuringCharacter">The character used for obscuring text if obscureText is true.</param>
        /// <param name="autofocus">Whether this text field should focus itself if nothing else is already focused.</param>
        /// <param name="autocorrect">Whether to enable autocorrection.</param>
        /// <param name="enableSuggestions">Whether to allow the platform to automatically format the text.</param>
        /// <param name="expands">Whether this widget's height will be sized to fill its parent.</param>
        /// <param name="prefix">A widget to display before the text.</param>
        /// <param name="suffix">A widget to display after the text.</param>
        /// <param name="clearButtonMode">Show a clear button to clear the text field.</param>
        public CupertinoTextField(
            string? value = null,
            string? placeholder = null,
            Action<string>? onChanged = null,
            Action<string>? onSubmitted = null,
            Action? onEditingComplete = null,
            Action? onTap = null,
            EdgeInsets? padding = null,
            TextStyle? style = null,
            TextStyle? placeholderStyle = null,
            Color? cursorColor = null,
            double? cursorWidth = null,
            double? cursorHeight = null,
            double? cursorRadius = null,
            BoxDecoration? decoration = null,
            TextInputType? keyboardType = null,
            TextInputAction? textInputAction = null,
            TextAlign? textAlign = null,
            int? maxLines = null,
            int? minLines = null,
            int? maxLength = null,
            bool enabled = true,
            bool readOnly = false,
            bool obscureText = false,
            string? obscuringCharacter = null,
            bool autofocus = false,
            bool autocorrect = true,
            bool enableSuggestions = true,
            bool expands = false,
            Widget? prefix = null,
            Widget? suffix = null,
            OverlayVisibilityMode? clearButtonMode = null
        )
        {
            _currentValue = value ?? "";

            var s = GetBackingStruct<CupertinoTextFieldStruct>();

            // Set text value
            s.value = value;

            // Set placeholder
            s.placeholder = placeholder;

            // Register callbacks and assign action IDs to struct
            s.onChangedAction = RegisterCallback(onChanged);
            s.onSubmittedAction = RegisterCallback(onSubmitted);
            s.onEditingCompleteAction = RegisterCallback(onEditingComplete);
            s.onTapAction = RegisterCallback(onTap);

            // Assign padding if provided
            if (padding.HasValue)
            {
                s.HasPadding = 1;
                s.paddingLeft = padding.Value.Left;
                s.paddingTop = padding.Value.Top;
                s.paddingRight = padding.Value.Right;
                s.paddingBottom = padding.Value.Bottom;
            }

            // Assign text style if provided
            if (style != null)
            {
                s.HasStyle = 1;
                s.styleFontSize = style.FontSize ?? 17.0; // CupertinoTextField default
                s.styleColor = style.Color?.Value ?? 0xFF000000;
                s.styleFontWeight = (int)(style.FontWeight ?? FontWeight.Normal);
            }

            // Assign placeholder style if provided
            if (placeholderStyle != null)
            {
                s.HasPlaceholderStyle = 1;
                s.placeholderStyleFontSize = placeholderStyle.FontSize ?? 17.0;
                s.placeholderStyleColor = placeholderStyle.Color?.Value ?? 0xFFC7C7CC;
                s.placeholderStyleFontWeight = (int)(placeholderStyle.FontWeight ?? FontWeight.Normal);
            }

            // Cursor appearance
            if (cursorColor.HasValue)
            {
                s.HasCursorColor = 1;
                s.cursorColor = cursorColor.Value.Value;
            }

            if (cursorWidth.HasValue)
            {
                s.HasCursorWidth = 1;
                s.cursorWidth = cursorWidth.Value;
            }

            if (cursorHeight.HasValue)
            {
                s.HasCursorHeight = 1;
                s.cursorHeight = cursorHeight.Value;
            }

            if (cursorRadius.HasValue)
            {
                s.HasCursorRadius = 1;
                s.cursorRadius = cursorRadius.Value;
            }

            // Box decoration
            if (decoration != null)
            {
                s.HasDecoration = 1;
                s.decorationColor = decoration.Color?.Value ?? 0xFFFFFFFF;
                s.decorationBorderRadius = decoration.BorderRadius ?? 5.0;
                s.decorationBorderColor = decoration.BorderColor?.Value ?? 0xFFC7C7CC;
                s.decorationBorderWidth = decoration.BorderWidth ?? 0.0;
            }

            // Keyboard configuration
            if (keyboardType.HasValue)
            {
                s.HasKeyboardType = 1;
                s.keyboardType = (int)keyboardType.Value;
            }

            if (textInputAction.HasValue)
            {
                s.HasTextInputAction = 1;
                s.textInputAction = (int)textInputAction.Value;
            }

            if (textAlign.HasValue)
            {
                s.HasTextAlign = 1;
                s.textAlign = (int)textAlign.Value;
            }

            // Line configuration
            if (maxLines.HasValue)
            {
                s.HasMaxLines = 1;
                s.maxLines = maxLines.Value;
            }

            if (minLines.HasValue)
            {
                s.HasMinLines = 1;
                s.minLines = minLines.Value;
            }

            if (maxLength.HasValue)
            {
                s.HasMaxLength = 1;
                s.maxLength = maxLength.Value;
            }

            // Boolean flags
            s.enabled = enabled;
            s.readOnly = readOnly;
            s.obscureText = obscureText;
            s.autofocus = autofocus;
            s.autocorrect = autocorrect;
            s.enableSuggestions = enableSuggestions;
            s.expands = expands;

            // Obscuring character
            s.obscuringCharacter = obscuringCharacter;

            // Prefix and suffix widgets
            s.prefix = GetWidgetHandle(prefix);
            s.suffix = GetWidgetHandle(suffix);

            // Clear button mode
            if (clearButtonMode.HasValue)
            {
                s.HasClearButtonMode = 1;
                s.clearButtonMode = (int)clearButtonMode.Value;
            }
        }

        /// <summary>
        /// Gets or sets the current text value.
        /// Setting this will update the text field and trigger a re-render.
        /// </summary>
        public string Value
        {
            get => _currentValue;
            set
            {
                if (_currentValue != value)
                {
                    _currentValue = value;
                    var s = GetBackingStruct<CupertinoTextFieldStruct>();
                    s.value = value;
                    SetState(() => { });
                }
            }
        }

        /// <summary>
        /// Clears the text field.
        /// </summary>
        public void Clear()
        {
            Value = "";
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new CupertinoTextFieldStruct();

        /// <inheritdoc/>
        public override void SendEvent(string eventName, string data, Action<string>? callback = null)
        {
            base.SendEvent(eventName, data, callback);

            // Handle text change events from Dart side
            if (eventName == "OnChange")
            {
                _currentValue = data;
            }
        }
    }

    /// <summary>
    /// Simple box decoration for CupertinoTextField.
    /// </summary>
    public class BoxDecoration
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Gets or sets the border radius.
        /// </summary>
        public double? BorderRadius { get; set; }

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public Color? BorderColor { get; set; }

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        public double? BorderWidth { get; set; }

        /// <summary>
        /// Creates a new BoxDecoration with the specified properties.
        /// </summary>
        public BoxDecoration(
            Color? color = null,
            double? borderRadius = null,
            Color? borderColor = null,
            double? borderWidth = null)
        {
            Color = color;
            BorderRadius = borderRadius;
            BorderColor = borderColor;
            BorderWidth = borderWidth;
        }

        /// <summary>
        /// Creates a standard iOS-style rounded rectangle decoration.
        /// </summary>
        public static BoxDecoration RoundedRectangle(Color? backgroundColor = null)
        {
            return new BoxDecoration(
                color: backgroundColor ?? new Color(0xFFFFFFFF),
                borderRadius: 5.0,
                borderColor: new Color(0xFFC7C7CC),
                borderWidth: 0.5
            );
        }
    }

    /// <summary>
    /// Simple text style for CupertinoTextField.
    /// </summary>
    public class TextStyle
    {
        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double? FontSize { get; set; }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        public FontWeight? FontWeight { get; set; }

        /// <summary>
        /// Creates a new TextStyle with the specified properties.
        /// </summary>
        public TextStyle(
            double? fontSize = null,
            Color? color = null,
            FontWeight? fontWeight = null)
        {
            FontSize = fontSize;
            Color = color;
            FontWeight = fontWeight;
        }
    }

    /// <summary>
    /// Font weight values.
    /// </summary>
    public enum FontWeight
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900
    }
}
