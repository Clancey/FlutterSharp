using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for InfiniteGridView widget.
    /// Matches the Dart InfiniteGridViewStruct in flutter_sharp_structs.dart
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class InfiniteGridViewStruct : WidgetStruct
    {
        /// <summary>
        /// Number of items in the grid (including loading indicator slots if present).
        /// </summary>
        public int itemCount;

        /// <summary>
        /// The ScrollController ID for this grid.
        /// </summary>
        public IntPtr controllerId;

        /// <summary>
        /// Number of items per row.
        /// </summary>
        public int crossAxisCount;

        /// <summary>
        /// Spacing between rows (in pixels).
        /// </summary>
        public double mainAxisSpacing;

        /// <summary>
        /// Spacing between columns (in pixels).
        /// </summary>
        public double crossAxisSpacing;

        /// <summary>
        /// Aspect ratio of each child.
        /// </summary>
        public double childAspectRatio;

        /// <summary>
        /// Whether there is a loading indicator at the end.
        /// </summary>
        public byte hasLoadingIndicator;

        /// <summary>
        /// Distance from end to trigger load more (in pixels).
        /// </summary>
        public double loadMoreThreshold;
    }
}
