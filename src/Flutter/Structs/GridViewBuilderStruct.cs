using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{
	/// <summary>
	/// FFI struct for GridViewBuilder widget.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class GridViewBuilderStruct : WidgetStruct
	{
		/// <summary>
		/// Number of items in the grid
		/// </summary>
		public int itemCount;

		/// <summary>
		/// Number of columns in the grid (crossAxisCount)
		/// </summary>
		public int crossAxisCount;

		/// <summary>
		/// Main axis spacing between items
		/// </summary>
		public double mainAxisSpacing;

		/// <summary>
		/// Cross axis spacing between items
		/// </summary>
		public double crossAxisSpacing;

		/// <summary>
		/// Child aspect ratio (width / height)
		/// </summary>
		public double childAspectRatio;
	}
}
