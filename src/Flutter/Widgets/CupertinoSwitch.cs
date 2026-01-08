// FlutterSharp Manual Implementation
// CupertinoSwitch Widget

using System;
using System.Collections;
using System.Collections.Generic;
using Flutter;
using Flutter.Enums;
using Flutter.Gestures;
using Flutter.UI;
using Flutter.Structs;
using Flutter.Widgets;
using Flutter.Cupertino;

namespace Flutter.Widgets
{
    /// <summary>
    /// An iOS-style switch.
    ///
    /// Used to toggle the on/off state of a single setting.
    ///
    /// The switch itself does not maintain any state. Instead, when the state of
    /// the switch changes, the widget calls the onChanged callback. Most widgets
    /// that use a switch will listen for the onChanged callback and rebuild the
    /// switch with a new value to update the visual appearance of the switch.
    ///
    /// If the onChanged callback is null, then the switch will be disabled (it
    /// will not respond to input).
    ///
    /// See also:
    ///  * https://developer.apple.com/design/human-interface-guidelines/toggles
    /// </summary>
    public class CupertinoSwitch : StatefulWidget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CupertinoSwitch"/> class.
        /// </summary>
        /// <param name="value">Whether this switch is on or off.</param>
        /// <param name="onChanged">Called when the user toggles the switch on or off. The callback passes the new value. If null, the switch will be disabled.</param>
        /// <param name="activeTrackColor">The color to use for the track when the switch is on. If null, defaults to CupertinoColors.systemGreen.</param>
        /// <param name="inactiveTrackColor">The color to use for the track when the switch is off.</param>
        /// <param name="thumbColor">The color to use for the thumb of the switch.</param>
        /// <param name="trackOutlineColor">The outline color of the track. If null, defaults to Colors.transparent.</param>
        /// <param name="trackOutlineWidth">The outline width of the track.</param>
        /// <param name="focusColor">The color for the switch's Material when it has the input focus.</param>
        /// <param name="onFocusChange">Handler called when the focus changes.</param>
        /// <param name="autofocus">True if this widget will be selected as the initial focus. Defaults to false.</param>
        /// <param name="applyTheme">If true, the switch will apply the CupertinoThemeData.</param>
        /// <param name="dragStartBehavior">Determines the way that drag start behavior is handled. Defaults to DragStartBehavior.Start.</param>
        public CupertinoSwitch(
            bool value,
            Action<bool>? onChanged = null,
            Color? activeTrackColor = null,
            Color? inactiveTrackColor = null,
            Color? thumbColor = null,
            Color? trackOutlineColor = null,
            double? trackOutlineWidth = null,
            Color? focusColor = null,
            Action<bool>? onFocusChange = null,
            bool autofocus = false,
            bool? applyTheme = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.Start
        )
        {
            var s = GetBackingStruct<CupertinoSwitchStruct>();

            // Assign value (required bool)
            s.value = (byte)(value ? 1 : 0);

            // Register callbacks and assign action IDs to struct
            s.onChangedAction = RegisterCallback(onChanged);
            s.onFocusChangeAction = RegisterCallback(onFocusChange);

            // Assign color properties
            if (activeTrackColor.HasValue)
            {
                s.HasActiveTrackColor = 1;
                s.activeTrackColor = activeTrackColor.Value.Value;
            }

            if (inactiveTrackColor.HasValue)
            {
                s.HasInactiveTrackColor = 1;
                s.inactiveTrackColor = inactiveTrackColor.Value.Value;
            }

            if (thumbColor.HasValue)
            {
                s.HasThumbColor = 1;
                s.thumbColor = thumbColor.Value.Value;
            }

            if (trackOutlineColor.HasValue)
            {
                s.HasTrackOutlineColor = 1;
                s.trackOutlineColor = trackOutlineColor.Value.Value;
            }

            if (trackOutlineWidth.HasValue)
            {
                s.HasTrackOutlineWidth = 1;
                s.trackOutlineWidth = trackOutlineWidth.Value;
            }

            if (focusColor.HasValue)
            {
                s.HasFocusColor = 1;
                s.focusColor = focusColor.Value.Value;
            }

            // Assign applyTheme
            if (applyTheme.HasValue)
            {
                s.HasApplyTheme = 1;
                s.applyTheme = (byte)(applyTheme.Value ? 1 : 0);
            }

            // Assign simple properties
            s.autofocus = (byte)(autofocus ? 1 : 0);
            s.dragStartBehavior = (int)dragStartBehavior;
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new CupertinoSwitchStruct();
    }
}
