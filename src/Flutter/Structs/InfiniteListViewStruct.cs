using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for InfiniteListView widget.
    /// Matches the Dart InfiniteListViewStruct in flutter_sharp_structs.dart
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class InfiniteListViewStruct : WidgetStruct
    {
        /// <summary>
        /// Number of items in the list (including loading indicator if present).
        /// </summary>
        public int itemCount;

        /// <summary>
        /// The ScrollController ID for this list.
        /// </summary>
        public IntPtr controllerId;

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
