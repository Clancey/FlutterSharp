// FlutterSharp Manual Implementation
// CupertinoNavigationBar FFI Struct

using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Widgets;
using Flutter.Cupertino;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct representation of CupertinoNavigationBar widget.
    ///
    /// An iOS-style navigation bar.
    ///
    /// The navigation bar is a toolbar that minimally consists of a widget, normally
    /// a page title, in the middle of the toolbar.
    ///
    /// It also supports a leading and trailing widget before and after the middle widget
    /// while keeping the middle widget centered.
    ///
    /// See also:
    ///  * https://developer.apple.com/design/human-interface-guidelines/navigation-bars
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CupertinoNavigationBarStruct : WidgetStruct
    {
        // Has flag for nullable widget: leading
        public byte HasLeading { get; set; }

        /// <summary>
        /// Widget to place at the start of the navigation bar.
        /// Normally a back button for a normal page or a cancel button for full page dialogs.
        /// </summary>
        public IntPtr leading { get; set; }

        // Has flag for nullable widget: middle
        public byte HasMiddle { get; set; }

        /// <summary>
        /// Widget to place in the middle of the navigation bar.
        /// Normally a title or a segmented control.
        /// </summary>
        public IntPtr middle { get; set; }

        // Has flag for nullable widget: trailing
        public byte HasTrailing { get; set; }

        /// <summary>
        /// Widget to place at the end of the navigation bar.
        /// Normally additional actions taken on the page such as a search or edit function.
        /// </summary>
        public IntPtr trailing { get; set; }

        // Has flag for nullable string: previousPageTitle
        public byte HasPreviousPageTitle { get; set; }

        // String field: previousPageTitle
        IntPtr _previousPageTitle;

        /// <summary>
        /// Manually specify the previous route's title when automaticallyImplyLeading is true.
        /// </summary>
        public string? previousPageTitle
        {
            get => GetString(_previousPageTitle);
            set { SetString(ref _previousPageTitle, value); HasPreviousPageTitle = (byte)(value != null ? 1 : 0); }
        }

        /// <summary>
        /// The background color of the navigation bar.
        /// If it contains transparency, the tab bar will automatically produce
        /// a blurring effect to the content behind it.
        /// </summary>
        public byte HasBackgroundColor { get; set; }
        public uint backgroundColor { get; set; }

        /// <summary>
        /// The brightness of the navigation bar's background color.
        /// 0 = Brightness.dark, 1 = Brightness.light
        /// </summary>
        public byte HasBrightness { get; set; }
        public int brightness { get; set; }

        /// <summary>
        /// Padding for the contents of the navigation bar.
        /// Stored as: start, top, end, bottom (4 doubles) for EdgeInsetsDirectional
        /// </summary>
        public byte HasPadding { get; set; }
        public double paddingStart { get; set; }
        public double paddingTop { get; set; }
        public double paddingEnd { get; set; }
        public double paddingBottom { get; set; }

        /// <summary>
        /// Controls whether we should try to imply the leading widget if null.
        /// If true, a back button is automatically shown.
        /// Defaults to true.
        /// </summary>
        public bool automaticallyImplyLeading { get; set; }

        /// <summary>
        /// Controls whether we should try to imply the middle widget if null.
        /// If true, the title is automatically obtained from the current route.
        /// Defaults to true.
        /// </summary>
        public bool automaticallyImplyMiddle { get; set; }

        /// <summary>
        /// When true, this navigation bar will transition on top of the routes
        /// instead of inside them.
        /// Defaults to true.
        /// </summary>
        public bool transitionBetweenRoutes { get; set; }

        /// <summary>
        /// Whether the nav bar appears transparent when no content is scrolled under.
        /// Defaults to true.
        /// </summary>
        public bool automaticBackgroundVisibility { get; set; }

        /// <summary>
        /// Whether to enable the blur filter of the navigation bar.
        /// Defaults to true.
        /// </summary>
        public bool enableBackgroundFilterBlur { get; set; }
    }
}
