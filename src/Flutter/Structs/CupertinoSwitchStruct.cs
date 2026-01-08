// FlutterSharp Manual Implementation
// CupertinoSwitch FFI Struct

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Widgets;
using Flutter.Cupertino;

namespace Flutter.Structs
{
    /// <summary>
    /// An iOS-style switch.
    ///
    /// Used to toggle the on/off state of a single setting.
    ///
    /// The switch itself does not maintain any state. Instead, when the state of
    /// the switch changes, the widget calls the onChanged callback.
    ///
    /// See also:
    ///  * <https://developer.apple.com/design/human-interface-guidelines/toggles>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CupertinoSwitchStruct : WidgetStruct
    {
        /// <summary>
        /// Whether this switch is on or off.
        /// </summary>
        public byte value { get; set; }

        // Has flag for nullable callback: onChanged
        public byte HasonChanged { get; set; }

        // Callback field: onChanged
        IntPtr _onChanged;

        /// <summary>
        /// Called when the user toggles the switch on or off.
        /// If this callback is null, then the switch will be disabled.
        /// </summary>
        public string? onChangedAction
        {
            get => GetString(_onChanged);
            set { SetString(ref _onChanged, value); HasonChanged = (byte)(value != null ? 1 : 0); }
        }

        // Has flag for nullable callback: onFocusChange
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
        /// The color to use for the track when the switch is on.
        /// If null, defaults to CupertinoColors.systemGreen.
        /// </summary>
        public byte HasActiveTrackColor { get; set; }
        public uint activeTrackColor { get; set; }

        /// <summary>
        /// The color to use for the track when the switch is off.
        /// </summary>
        public byte HasInactiveTrackColor { get; set; }
        public uint inactiveTrackColor { get; set; }

        /// <summary>
        /// The color to use for the thumb of the switch.
        /// </summary>
        public byte HasThumbColor { get; set; }
        public uint thumbColor { get; set; }

        /// <summary>
        /// The outline color of the track.
        /// If null, defaults to Colors.transparent.
        /// </summary>
        public byte HasTrackOutlineColor { get; set; }
        public uint trackOutlineColor { get; set; }

        /// <summary>
        /// The outline width of the track.
        /// </summary>
        public byte HasTrackOutlineWidth { get; set; }
        public double trackOutlineWidth { get; set; }

        /// <summary>
        /// The color for the switch's Material when it has the input focus.
        /// </summary>
        public byte HasFocusColor { get; set; }
        public uint focusColor { get; set; }

        /// <summary>
        /// If true, the switch will apply the CupertinoThemeData.
        /// </summary>
        public byte HasApplyTheme { get; set; }
        public byte applyTheme { get; set; }

        /// <summary>
        /// True if this widget will be selected as the initial focus.
        /// Defaults to false.
        /// </summary>
        public byte autofocus { get; set; }

        /// <summary>
        /// Determines the way that drag start behavior is handled.
        /// Defaults to DragStartBehavior.Start.
        /// </summary>
        public int dragStartBehavior { get; set; }
    }
}
