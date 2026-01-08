using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for ListenableBuilder widget.
    /// </summary>
    /// <remarks>
    /// This struct contains the built child widget pointer which is set
    /// by the C# ListenableBuilder when its builder callback is invoked.
    /// Dart reads this pointer to render the widget tree.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class ListenableBuilderStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the built widget tree.
        /// This is populated by calling the builder callback on the C# side.
        /// </summary>
        public IntPtr builtChild;
    }
}
