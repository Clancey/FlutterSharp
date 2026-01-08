using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
    /// <summary>
    /// FFI struct for Selector widget.
    /// </summary>
    /// <remarks>
    /// Selector is an optimized Consumer that only rebuilds when a selected
    /// subset of the provider value changes. The C# Selector handles the
    /// selection and comparison logic. This struct contains the built widget
    /// pointer which Dart reads to render the widget tree.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal class SelectorStruct : WidgetStruct
    {
        /// <summary>
        /// Pointer to the built widget tree.
        /// This is populated by the C# Selector builder callback.
        /// </summary>
        public IntPtr builtChild;
    }
}
