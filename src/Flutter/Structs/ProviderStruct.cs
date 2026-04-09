using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for Provider widget.
    /// </summary>
    /// <remarks>
    /// Provider is a dependency injection widget that makes a value
    /// available to its descendants. The value itself is managed on the
    /// C# side - this struct only contains the child widget pointer.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class ProviderStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the child widget tree.
        /// </summary>
        public IntPtr child;
    }
}
