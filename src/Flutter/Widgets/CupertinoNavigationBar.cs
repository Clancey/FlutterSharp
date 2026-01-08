// FlutterSharp Manual Implementation
// CupertinoNavigationBar Widget

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
    /// An iOS-style navigation bar.
    ///
    /// The navigation bar is a toolbar that minimally consists of a widget, normally
    /// a page title, in the middle of the toolbar.
    ///
    /// It also supports a leading and trailing widget before and after the middle widget
    /// while keeping the middle widget centered.
    ///
    /// The leading widget will automatically be a back chevron icon button (or a close
    /// button in case of a fullscreen dialog) to navigate back to the previous route if
    /// none is provided and automaticallyImplyLeading is set to true.
    ///
    /// The middle widget will automatically be a title text from the current CupertinoPageRoute
    /// if none is provided and automaticallyImplyMiddle is set to true.
    ///
    /// It should be placed at top of the screen and automatically accounts for the OS's
    /// status bar.
    ///
    /// If the given backgroundColor's opacity is not 1.0 (which is the case by default),
    /// it will produce a blurring effect to the content behind it.
    ///
    /// See also:
    ///  * https://developer.apple.com/design/human-interface-guidelines/navigation-bars
    /// </summary>
    public class CupertinoNavigationBar : StatelessWidget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CupertinoNavigationBar"/> class.
        /// </summary>
        /// <param name="leading">Widget to place at the start of the navigation bar. Normally a back button or a cancel button.</param>
        /// <param name="automaticallyImplyLeading">Controls whether we should try to imply the leading widget if null. If true, a back button is automatically shown. Defaults to true.</param>
        /// <param name="automaticallyImplyMiddle">Controls whether we should try to imply the middle widget if null. If true, the title is automatically obtained from the current route. Defaults to true.</param>
        /// <param name="previousPageTitle">Manually specify the previous route's title when automaticallyImplyLeading is true.</param>
        /// <param name="middle">Widget to place in the middle of the navigation bar. Normally a title or a segmented control.</param>
        /// <param name="trailing">Widget to place at the end of the navigation bar. Normally additional actions taken on the page such as a search or edit function.</param>
        /// <param name="backgroundColor">The background color of the navigation bar. If it contains transparency, the tab bar will automatically produce a blurring effect to the content behind it.</param>
        /// <param name="brightness">The brightness of the navigation bar's background color.</param>
        /// <param name="padding">Padding for the contents of the navigation bar.</param>
        /// <param name="transitionBetweenRoutes">When true, this navigation bar will transition on top of the routes instead of inside them. Defaults to true.</param>
        /// <param name="automaticBackgroundVisibility">Whether the nav bar appears transparent when no content is scrolled under. Defaults to true.</param>
        /// <param name="enableBackgroundFilterBlur">Whether to enable the blur filter of the navigation bar. Defaults to true.</param>
        public CupertinoNavigationBar(
            Widget? leading = null,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyMiddle = true,
            string? previousPageTitle = null,
            Widget? middle = null,
            Widget? trailing = null,
            Color? backgroundColor = null,
            Brightness? brightness = null,
            EdgeInsets? padding = null,
            bool transitionBetweenRoutes = true,
            bool automaticBackgroundVisibility = true,
            bool enableBackgroundFilterBlur = true
        )
        {
            var s = GetBackingStruct<CupertinoNavigationBarStruct>();

            // Assign widget properties
            if (leading != null)
            {
                s.HasLeading = 1;
                s.leading = GetWidgetHandle(leading);
            }

            if (middle != null)
            {
                s.HasMiddle = 1;
                s.middle = GetWidgetHandle(middle);
            }

            if (trailing != null)
            {
                s.HasTrailing = 1;
                s.trailing = GetWidgetHandle(trailing);
            }

            // Assign string properties
            s.previousPageTitle = previousPageTitle;

            // Assign color if provided
            if (backgroundColor.HasValue)
            {
                s.HasBackgroundColor = 1;
                s.backgroundColor = backgroundColor.Value.Value;
            }

            // Assign brightness if provided
            if (brightness.HasValue)
            {
                s.HasBrightness = 1;
                s.brightness = (int)brightness.Value;
            }

            // Assign padding if provided
            if (padding.HasValue)
            {
                s.HasPadding = 1;
                s.paddingStart = padding.Value.Left;
                s.paddingTop = padding.Value.Top;
                s.paddingEnd = padding.Value.Right;
                s.paddingBottom = padding.Value.Bottom;
            }

            // Assign boolean properties
            s.automaticallyImplyLeading = automaticallyImplyLeading;
            s.automaticallyImplyMiddle = automaticallyImplyMiddle;
            s.transitionBetweenRoutes = transitionBetweenRoutes;
            s.automaticBackgroundVisibility = automaticBackgroundVisibility;
            s.enableBackgroundFilterBlur = enableBackgroundFilterBlur;
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new CupertinoNavigationBarStruct();
    }
}
