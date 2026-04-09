// FlutterSharp Manual Implementation
// CupertinoButton FFI Struct

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Widgets;
using Flutter.Cupertino;

namespace Flutter.Structs
{
    /// <summary>
    /// An iOS-style button.
    ///
    /// Takes in a text or an icon that fades out and in on touch. May optionally have a
    /// background.
    ///
    /// The padding defaults to 16.0 pixels. When using a CupertinoButton within a fixed
    /// height parent, like a CupertinoNavigationBar, a smaller, or even EdgeInsets.zero,
    /// should be used to prevent clipping larger child widgets.
    ///
    /// See also:
    ///  * <https://developer.apple.com/design/human-interface-guidelines/buttons>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CupertinoButtonStruct : WidgetStruct
    {
        // Has flag for nullable property: onPressed
        public byte HasonPressed { get; set; }

        // Callback field: onPressed
        IntPtr _onPressed;

        /// <summary>
        /// Called when the button is tapped or otherwise activated.
        /// If this callback is null, then the button will be disabled.
        /// </summary>
        public string? onPressedAction
        {
            get => GetString(_onPressed);
            set { SetString(ref _onPressed, value); HasonPressed = (byte)(value != null ? 1 : 0); }
        }

        // Has flag for nullable property: onLongPress
        public byte HasonLongPress { get; set; }

        // Callback field: onLongPress
        IntPtr _onLongPress;

        /// <summary>
        /// Called when the button is long-pressed.
        /// </summary>
        public string? onLongPressAction
        {
            get => GetString(_onLongPress);
            set { SetString(ref _onLongPress, value); HasonLongPress = (byte)(value != null ? 1 : 0); }
        }

        // Has flag for nullable property: onFocusChange
        public byte HasonFocusChange { get; set; }

        // Callback field: onFocusChange
        IntPtr _onFocusChange;

        /// <summary>
        /// Handler called when the focus changes.
        /// </summary>
        public string? onFocusChangeAction
        {
            get => GetString(_onFocusChange);
            set { SetString(ref _onFocusChange, value); HasonFocusChange = (byte)(value != null ? 1 : 0); }
        }

        /// <summary>
        /// The amount of space to surround the child inside the bounds of the button.
        /// Defaults to 16.0 pixels.
        /// Stored as: left, top, right, bottom (4 doubles)
        /// </summary>
        public byte HasPadding { get; set; }
        public double paddingLeft { get; set; }
        public double paddingTop { get; set; }
        public double paddingRight { get; set; }
        public double paddingBottom { get; set; }

        /// <summary>
        /// The color of the button's background.
        /// Defaults to null which produces a button with no background.
        /// </summary>
        public byte HasColor { get; set; }
        public uint color { get; set; }

        /// <summary>
        /// The color of button's foreground (text and icons).
        /// </summary>
        public byte HasForegroundColor { get; set; }
        public uint foregroundColor { get; set; }

        /// <summary>
        /// The color of the button's background when the button is disabled.
        /// Defaults to CupertinoColors.quaternarySystemFill.
        /// </summary>
        public byte HasDisabledColor { get; set; }
        public uint disabledColor { get; set; }

        /// <summary>
        /// Minimum size of the button.
        /// Stored as width, height (2 doubles)
        /// </summary>
        public byte HasMinimumSize { get; set; }
        public double minimumSizeWidth { get; set; }
        public double minimumSizeHeight { get; set; }

        /// <summary>
        /// The opacity that the button will fade to when it is pressed.
        /// The button will have an opacity of 1.0 when it is not pressed.
        /// Defaults to 0.4.
        /// </summary>
        public byte HaspressedOpacity { get; set; }
        public double pressedOpacity { get; set; }

        /// <summary>
        /// The radius of the button's corners when it has a background color.
        /// Defaults to 8.0.
        /// </summary>
        public byte HasBorderRadius { get; set; }
        public double borderRadius { get; set; }

        /// <summary>
        /// The alignment of the button's child.
        /// Defaults to Alignment.center.
        /// </summary>
        public byte HasAlignment { get; set; }
        public double alignmentX { get; set; }
        public double alignmentY { get; set; }

        /// <summary>
        /// The size style of the button.
        /// Defaults to CupertinoButtonSize.large.
        /// </summary>
        public CupertinoButtonSize sizeStyle { get; set; }

        /// <summary>
        /// Whether the button should be focused initially.
        /// Defaults to false.
        /// </summary>
        public bool autofocus { get; set; }

        /// <summary>
        /// Whether this is a filled button style.
        /// If true, uses CupertinoButton.filled constructor.
        /// </summary>
        public bool isFilled { get; set; }

        /// <summary>
        /// Typically the button's label widget.
        /// </summary>
        public IntPtr child { get; set; }
    }
}
