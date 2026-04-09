using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for Consumer widget.
    /// </summary>
    /// <remarks>
    /// Consumer listens to a Provider and rebuilds when the value changes.
    /// The C# Consumer handles subscription to ChangeNotifier and calls
    /// its builder callback. This struct contains the built widget pointer
    /// which Dart reads to render the widget tree.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class ConsumerStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the built widget tree.
        /// This is populated by the C# Consumer builder callback.
        /// </summary>
        public IntPtr builtChild;
    }
}
