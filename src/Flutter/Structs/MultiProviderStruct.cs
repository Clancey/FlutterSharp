using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for MultiProvider widget.
    /// </summary>
    /// <remarks>
    /// MultiProvider composes multiple providers into a nested widget tree.
    /// The C# MultiProvider handles building the nested structure. This
    /// struct contains the pointer to the outermost built widget.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class MultiProviderStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the built widget tree (the outermost provider).
        /// </summary>
        public IntPtr builtChild;
    }
}
