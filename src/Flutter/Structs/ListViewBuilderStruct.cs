using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
	/// <summary>
	/// FFI struct for ListViewBuilder widget.
	/// Matches the Dart ListViewBuilderStruct in flutter_sharp_structs.dart
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class ListViewBuilderStruct : WidgetStruct
	{
		/// <summary>
		/// Number of items in the list
		/// </summary>
		public int itemCount;
	}
}
