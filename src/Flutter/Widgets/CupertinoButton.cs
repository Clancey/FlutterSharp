// FlutterSharp Manual Implementation
// CupertinoButton Widget

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
    /// An iOS-style button.
    ///
    /// Takes in a text or an icon that fades out and in on touch. May optionally have a
    /// background.
    ///
    /// The padding defaults to 16.0 pixels. When using a CupertinoButton within a fixed
    /// height parent, like a CupertinoNavigationBar, a smaller, or even EdgeInsets.zero,
    /// should be used to prevent clipping larger child widgets.
    ///
    /// If the onPressed callback is null, then the button will be disabled and will not
    /// react to touch.
    ///
    /// See also:
    ///  * https://developer.apple.com/design/human-interface-guidelines/buttons
    /// </summary>
    public class CupertinoButton : StatelessWidget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CupertinoButton"/> class.
        /// </summary>
        /// <param name="child">The widget below this widget in the tree, typically the button's label.</param>
        /// <param name="onPressed">Called when the button is tapped or otherwise activated. If null, the button will be disabled.</param>
        /// <param name="onLongPress">Called when the button is long-pressed.</param>
        /// <param name="onFocusChange">Handler called when the focus changes.</param>
        /// <param name="padding">The amount of space to surround the child inside the bounds of the button. Defaults to 16.0 pixels.</param>
        /// <param name="color">The color of the button's background. Defaults to null which produces a button with no background.</param>
        /// <param name="foregroundColor">The color of the button's foreground (text and icons).</param>
        /// <param name="disabledColor">The color of the button's background when the button is disabled.</param>
        /// <param name="minimumSize">Minimum size of the button.</param>
        /// <param name="pressedOpacity">The opacity that the button will fade to when it is pressed. Defaults to 0.4.</param>
        /// <param name="borderRadius">The radius of the button's corners when it has a background color. Defaults to 8.0.</param>
        /// <param name="alignment">The alignment of the button's child. Defaults to Alignment.center.</param>
        /// <param name="sizeStyle">The size style of the button. Defaults to CupertinoButtonSize.Large.</param>
        /// <param name="autofocus">Whether the button should be focused initially. Defaults to false.</param>
        public CupertinoButton(
            Widget child,
            Action? onPressed = null,
            Action? onLongPress = null,
            Action<bool>? onFocusChange = null,
            EdgeInsets? padding = null,
            Color? color = null,
            Color? foregroundColor = null,
            Color? disabledColor = null,
            Size? minimumSize = null,
            double? pressedOpacity = null,
            double? borderRadius = null,
            Alignment? alignment = null,
            CupertinoButtonSize sizeStyle = CupertinoButtonSize.Large,
            bool autofocus = false
        )
        {
            var s = GetBackingStruct<CupertinoButtonStruct>();

            // Register callbacks and assign action IDs to struct
            s.onPressedAction = RegisterCallback(onPressed);
            s.onLongPressAction = RegisterCallback(onLongPress);
            s.onFocusChangeAction = RegisterCallback(onFocusChange);

            // Assign child widget
            s.child = GetWidgetHandle(child);

            // Assign padding if provided
            if (padding.HasValue)
            {
                s.HasPadding = 1;
                s.paddingLeft = padding.Value.Left;
                s.paddingTop = padding.Value.Top;
                s.paddingRight = padding.Value.Right;
                s.paddingBottom = padding.Value.Bottom;
            }

            // Assign color if provided
            if (color.HasValue)
            {
                s.HasColor = 1;
                s.color = color.Value.Value;
            }

            // Assign foreground color if provided
            if (foregroundColor.HasValue)
            {
                s.HasForegroundColor = 1;
                s.foregroundColor = foregroundColor.Value.Value;
            }

            // Assign disabled color if provided
            if (disabledColor.HasValue)
            {
                s.HasDisabledColor = 1;
                s.disabledColor = disabledColor.Value.Value;
            }

            // Assign minimum size if provided
            if (minimumSize.HasValue)
            {
                s.HasMinimumSize = 1;
                s.minimumSizeWidth = minimumSize.Value.Width;
                s.minimumSizeHeight = minimumSize.Value.Height;
            }

            // Assign pressed opacity if provided
            if (pressedOpacity.HasValue)
            {
                s.HaspressedOpacity = 1;
                s.pressedOpacity = pressedOpacity.Value;
            }

            // Assign border radius if provided
            if (borderRadius.HasValue)
            {
                s.HasBorderRadius = 1;
                s.borderRadius = borderRadius.Value;
            }

            // Assign alignment if provided
            if (alignment != null)
            {
                s.HasAlignment = 1;
                s.alignmentX = alignment.X;
                s.alignmentY = alignment.Y;
            }

            // Assign simple properties
            s.sizeStyle = sizeStyle;
            s.autofocus = autofocus;
            s.isFilled = false;
        }

        /// <summary>
        /// Creates a filled CupertinoButton.
        /// Creates an iOS-style button with a filled background. The background color is
        /// derived from the color argument. If color is not provided, it uses the primary
        /// color from CupertinoTheme.
        /// </summary>
        /// <param name="child">The widget below this widget in the tree, typically the button's label.</param>
        /// <param name="onPressed">Called when the button is tapped or otherwise activated. If null, the button will be disabled.</param>
        /// <param name="onLongPress">Called when the button is long-pressed.</param>
        /// <param name="onFocusChange">Handler called when the focus changes.</param>
        /// <param name="padding">The amount of space to surround the child inside the bounds of the button.</param>
        /// <param name="color">The color of the button's background.</param>
        /// <param name="foregroundColor">The color of the button's foreground (text and icons).</param>
        /// <param name="disabledColor">The color of the button's background when the button is disabled.</param>
        /// <param name="minimumSize">Minimum size of the button.</param>
        /// <param name="pressedOpacity">The opacity that the button will fade to when it is pressed. Defaults to 0.4.</param>
        /// <param name="borderRadius">The radius of the button's corners when it has a background color.</param>
        /// <param name="alignment">The alignment of the button's child. Defaults to Alignment.center.</param>
        /// <param name="sizeStyle">The size style of the button. Defaults to CupertinoButtonSize.Large.</param>
        /// <param name="autofocus">Whether the button should be focused initially. Defaults to false.</param>
        /// <returns>A filled CupertinoButton instance.</returns>
        public static CupertinoButton Filled(
            Widget child,
            Action? onPressed = null,
            Action? onLongPress = null,
            Action<bool>? onFocusChange = null,
            EdgeInsets? padding = null,
            Color? color = null,
            Color? foregroundColor = null,
            Color? disabledColor = null,
            Size? minimumSize = null,
            double? pressedOpacity = null,
            double? borderRadius = null,
            Alignment? alignment = null,
            CupertinoButtonSize sizeStyle = CupertinoButtonSize.Large,
            bool autofocus = false
        )
        {
            var button = new CupertinoButton(
                child: child,
                onPressed: onPressed,
                onLongPress: onLongPress,
                onFocusChange: onFocusChange,
                padding: padding,
                color: color,
                foregroundColor: foregroundColor,
                disabledColor: disabledColor,
                minimumSize: minimumSize,
                pressedOpacity: pressedOpacity,
                borderRadius: borderRadius,
                alignment: alignment,
                sizeStyle: sizeStyle,
                autofocus: autofocus
            );

            // Mark this as a filled button
            var s = button.GetBackingStruct<CupertinoButtonStruct>();
            s.isFilled = true;

            return button;
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new CupertinoButtonStruct();
    }
}
