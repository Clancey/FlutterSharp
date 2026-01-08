// FlutterSharp Manual Implementation
// CupertinoTextField FFI Struct

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Widgets;

namespace Flutter.Structs
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
    ///  * <https://developer.apple.com/design/human-interface-guidelines/text-fields>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CupertinoTextFieldStruct : WidgetStruct
    {
        // Text value
        public byte HasValue { get; set; }
        IntPtr _value;

        /// <summary>
        /// The current text value in the field.
        /// </summary>
        public string? value
        {
            get => GetString(_value);
            set { SetString(ref _value, value); HasValue = (byte)(value != null ? 1 : 0); }
        }

        // Placeholder text
        public byte HasPlaceholder { get; set; }
        IntPtr _placeholder;

        /// <summary>
        /// A lighter colored placeholder hint that appears on the first line of the
        /// text field when the text entry is empty.
        /// </summary>
        public string? placeholder
        {
            get => GetString(_placeholder);
            set { SetString(ref _placeholder, value); HasPlaceholder = (byte)(value != null ? 1 : 0); }
        }

        // Callbacks
        public byte HasonChanged { get; set; }
        IntPtr _onChanged;

        /// <summary>
        /// Called when the user initiates a change to the TextField's value.
        /// </summary>
        public string? onChangedAction
        {
            get => GetString(_onChanged);
            set { SetString(ref _onChanged, value); HasonChanged = (byte)(value != null ? 1 : 0); }
        }

        public byte HasonSubmitted { get; set; }
        IntPtr _onSubmitted;

        /// <summary>
        /// Called when the user indicates that they are done editing the text in the field.
        /// </summary>
        public string? onSubmittedAction
        {
            get => GetString(_onSubmitted);
            set { SetString(ref _onSubmitted, value); HasonSubmitted = (byte)(value != null ? 1 : 0); }
        }

        public byte HasonEditingComplete { get; set; }
        IntPtr _onEditingComplete;

        /// <summary>
        /// Called when the user submits editable content (e.g., user presses the "done" button on the keyboard).
        /// </summary>
        public string? onEditingCompleteAction
        {
            get => GetString(_onEditingComplete);
            set { SetString(ref _onEditingComplete, value); HasonEditingComplete = (byte)(value != null ? 1 : 0); }
        }

        public byte HasonTap { get; set; }
        IntPtr _onTap;

        /// <summary>
        /// Called for each distinct tap except for every second tap of a double tap.
        /// </summary>
        public string? onTapAction
        {
            get => GetString(_onTap);
            set { SetString(ref _onTap, value); HasonTap = (byte)(value != null ? 1 : 0); }
        }

        // Padding - EdgeInsets (left, top, right, bottom)
        public byte HasPadding { get; set; }
        public double paddingLeft { get; set; }
        public double paddingTop { get; set; }
        public double paddingRight { get; set; }
        public double paddingBottom { get; set; }

        // Text styling
        public byte HasStyle { get; set; }
        public double styleFontSize { get; set; }
        public uint styleColor { get; set; }
        public int styleFontWeight { get; set; } // FontWeight enum value

        // Placeholder styling
        public byte HasPlaceholderStyle { get; set; }
        public double placeholderStyleFontSize { get; set; }
        public uint placeholderStyleColor { get; set; }
        public int placeholderStyleFontWeight { get; set; }

        // Cursor appearance
        public byte HasCursorColor { get; set; }
        public uint cursorColor { get; set; }

        public byte HasCursorWidth { get; set; }
        public double cursorWidth { get; set; }

        public byte HasCursorHeight { get; set; }
        public double cursorHeight { get; set; }

        public byte HasCursorRadius { get; set; }
        public double cursorRadius { get; set; }

        // Background decoration
        public byte HasDecoration { get; set; }
        public uint decorationColor { get; set; }
        public double decorationBorderRadius { get; set; }
        public uint decorationBorderColor { get; set; }
        public double decorationBorderWidth { get; set; }

        // Text input configuration
        public byte HasKeyboardType { get; set; }
        public int keyboardType { get; set; } // TextInputType enum

        public byte HasTextInputAction { get; set; }
        public int textInputAction { get; set; } // TextInputAction enum

        public byte HasTextAlign { get; set; }
        public int textAlign { get; set; } // TextAlign enum

        // Line configuration
        public byte HasMaxLines { get; set; }
        public int maxLines { get; set; }

        public byte HasMinLines { get; set; }
        public int minLines { get; set; }

        public byte HasMaxLength { get; set; }
        public int maxLength { get; set; }

        // Boolean flags
        /// <summary>
        /// Whether the text field is enabled.
        /// </summary>
        public bool enabled { get; set; }

        /// <summary>
        /// Whether the text can be changed.
        /// </summary>
        public bool readOnly { get; set; }

        /// <summary>
        /// Whether to hide the text being edited (e.g., for passwords).
        /// </summary>
        public bool obscureText { get; set; }

        /// <summary>
        /// Whether this text field should focus itself if nothing else is already focused.
        /// </summary>
        public bool autofocus { get; set; }

        /// <summary>
        /// Whether to enable autocorrection.
        /// </summary>
        public bool autocorrect { get; set; }

        /// <summary>
        /// Whether to allow the platform to automatically format the text.
        /// </summary>
        public bool enableSuggestions { get; set; }

        /// <summary>
        /// If false the text field is "disabled": it ignores taps and its decoration is grayed out.
        /// </summary>
        public bool expands { get; set; }

        // Obscuring character
        public byte HasObscuringCharacter { get; set; }
        IntPtr _obscuringCharacter;

        /// <summary>
        /// Character used for obscuring text if obscureText is true.
        /// </summary>
        public string? obscuringCharacter
        {
            get => GetString(_obscuringCharacter);
            set { SetString(ref _obscuringCharacter, value); HasObscuringCharacter = (byte)(value != null ? 1 : 0); }
        }

        // Prefix and suffix widgets
        public IntPtr prefix { get; set; }
        public IntPtr suffix { get; set; }

        // Clear button mode (OverlayVisibilityMode enum)
        public byte HasClearButtonMode { get; set; }
        public int clearButtonMode { get; set; }
    }
}
