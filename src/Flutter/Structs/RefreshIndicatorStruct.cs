using System;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Material;
using Flutter.Widgets;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for RefreshIndicator widget.
    /// A widget that supports the Material "swipe to refresh" idiom.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class RefreshIndicatorStruct : SingleChildRenderObjectWidgetStruct
    {
        // Has flag for nullable property: onRefresh
        public byte HasonRefresh { get; set; }

        // Callback field: onRefresh
        // Using action string pattern - Dart will dispatch action to C# via method channel
        IntPtr _onRefresh;

        /// <summary>
        /// Action identifier for onRefresh callback.
        /// This is a Future callback - Dart will wait for completion signal from C#.
        /// </summary>
        public string? onRefreshAction
        {
            get => GetString(_onRefresh);
            set { SetString(ref _onRefresh, value); HasonRefresh = (byte)(value != null ? 1 : 0); }
        }

        /// <summary>
        /// The distance from the child's top or bottom EdgeInsets.zero where
        /// the refresh indicator will settle. During the drag that exposes the refresh
        /// indicator, its actual displacement may significantly exceed this value.
        /// Default value is 40.0.
        /// </summary>
        public double displacement { get; set; }

        /// <summary>
        /// The offset where the progress indicator should appear from the top
        /// of the parent widget when the refresh is triggered.
        /// Default value is 0.0.
        /// </summary>
        public double edgeOffset { get; set; }

        /// <summary>
        /// Has flag for color property.
        /// </summary>
        public byte Hascolor { get; set; }

        /// <summary>
        /// The progress indicator's foreground color.
        /// </summary>
        public uint color { get; set; }

        /// <summary>
        /// Has flag for backgroundColor property.
        /// </summary>
        public byte HasbackgroundColor { get; set; }

        /// <summary>
        /// The progress indicator's background color.
        /// </summary>
        public uint backgroundColor { get; set; }

        /// <summary>
        /// The distance from the top of the viewport to place the progress indicator.
        /// Default value is null, which means the default position is used.
        /// </summary>
        public byte HasnotificationPredicate { get; set; }

        /// <summary>
        /// Defines how the trigger gesture is handled.
        /// </summary>
        public RefreshIndicatorTriggerMode triggerMode { get; set; }

        /// <summary>
        /// The thickness of the progress indicator's stroke.
        /// Default value is 2.5.
        /// </summary>
        public double strokeWidth { get; set; }

        /// <summary>
        /// The child scrollable widget that triggers the refresh.
        /// </summary>
        public IntPtr child { get; set; }
    }
}
