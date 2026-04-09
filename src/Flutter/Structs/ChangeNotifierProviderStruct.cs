using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for ChangeNotifierProvider widget.
    /// </summary>
    /// <remarks>
    /// ChangeNotifierProvider manages a ChangeNotifier and notifies
    /// consumers when it changes. The ChangeNotifier itself is managed
    /// on the C# side - this struct only contains the child widget pointer.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class ChangeNotifierProviderStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the child widget tree.
        /// </summary>
        public IntPtr child;
    }
}
