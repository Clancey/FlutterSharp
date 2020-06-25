using System;
using Newtonsoft.Json;

namespace Flutter {
	[JsonConverter (typeof (EdgeInsetsGeometryConverter))]
	public struct EdgeInsetsGeometry {
		public EdgeInsetsGeometry (double all)
		{
			Left = Top = Right = Bottom = all;
		}

		public EdgeInsetsGeometry (double left = 0, double top = 0, double right = 0, double bottom = 0)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public double Left { get; set; }
		public double Top { get; set; }
		public double Right { get; set; }
		public double Bottom { get; set; }
		public override string ToString () => $"{Left},{Top},{Right},{Bottom}";

		public static EdgeInsetsGeometry Parse (string input)
		{
			var values = input.Split (',');
			return new EdgeInsetsGeometry (double.Parse (values [0]), double.Parse (values [1]), double.Parse (values [2]), double.Parse (values [3]));
		}
	}
	public struct Alignment {
		public Alignment (double x, double y)
		{
			X = x;
			Y = y;
		}
		public double X { get; }
		public double Y { get; }

		/// The top left corner.
		public static readonly Alignment TopLeft = new Alignment (-1.0, -1.0);

		/// The center point along the top edge.
		public static readonly Alignment TopCenter = new Alignment (0.0, -1.0);

		/// The top right corner.
		public static readonly Alignment TopRight = new Alignment (1.0, -1.0);

		/// The center point along the left edge.
		public static readonly Alignment CenterLeft = new Alignment (-1.0, 0.0);

		/// The center point, both horizontally and vertically.
		public static readonly Alignment Center = new Alignment (0.0, 0.0);

		/// The center point along the right edge.
		public static readonly Alignment CenterRight = new Alignment (1.0, 0.0);

		/// The bottom left corner.
		public static readonly Alignment BottomLeft = new Alignment (-1.0, 1.0);

		/// The center point along the bottom edge.
		public static readonly Alignment BottomCenter = new Alignment (0.0, 1.0);

		/// The bottom right corner.
		public static readonly Alignment BottomRight = new Alignment (1.0, 1.0);

	}

	/// How the children should be placed along the main axis in a flex layout.
	///
	/// See also:
	///
	///  * [Column], [Row], and [Flex], the flex widgets.
	///  * [RenderFlex], the flex render object.
	public enum MainAxisAlignment {
		/// Place the children as close to the start of the main axis as possible.
		///
		/// If this value is used in a horizontal direction, a [TextDirection] must be
		/// available to determine if the start is the left or the right.
		///
		/// If this value is used in a vertical direction, a [VerticalDirection] must be
		/// available to determine if the start is the top or the bottom.
		Start,

		/// Place the children as close to the end of the main axis as possible.
		///
		/// If this value is used in a horizontal direction, a [TextDirection] must be
		/// available to determine if the end is the left or the right.
		///
		/// If this value is used in a vertical direction, a [VerticalDirection] must be
		/// available to determine if the end is the top or the bottom.
		End,

		/// Place the children as close to the middle of the main axis as possible.
		Center,

		/// Place the free space evenly between the children.
		SpaceBetween,

		/// Place the free space evenly between the children as well as half of that
		/// space before and after the first and last child.
		SpaceAround,

		/// Place the free space evenly between the children as well as before and
		/// after the first and last child.
		SpaceEvenly,
	}


	/// How the children should be placed along the cross axis in a flex layout.
	///
	/// See also:
	///
	///  * [Column], [Row], and [Flex], the flex widgets.
	///  * [RenderFlex], the flex render object.
	public enum CrossAxisAlignment {
		/// Place the children with their start edge aligned with the start side of
		/// the cross axis.
		///
		/// For example, in a column (a flex with a vertical axis) whose
		/// [TextDirection] is [TextDirection.ltr], this aligns the left edge of the
		/// children along the left edge of the column.
		///
		/// If this value is used in a horizontal direction, a [TextDirection] must be
		/// available to determine if the start is the left or the right.
		///
		/// If this value is used in a vertical direction, a [VerticalDirection] must be
		/// available to determine if the start is the top or the bottom.
		Start,

		/// Place the children as close to the end of the cross axis as possible.
		///
		/// For example, in a column (a flex with a vertical axis) whose
		/// [TextDirection] is [TextDirection.ltr], this aligns the right edge of the
		/// children along the right edge of the column.
		///
		/// If this value is used in a horizontal direction, a [TextDirection] must be
		/// available to determine if the end is the left or the right.
		///
		/// If this value is used in a vertical direction, a [VerticalDirection] must be
		/// available to determine if the end is the top or the bottom.
		End,

		/// Place the children so that their centers align with the middle of the
		/// cross axis.
		///
		/// This is the default cross-axis alignment.
		Center,

		/// Require the children to fill the cross axis.
		///
		/// This causes the constraints passed to the children to be tight in the
		/// cross axis.
		Stretch,

		/// Place the children along the cross axis such that their baselines match.
		///
		/// If the main axis is vertical, then this value is treated like [start]
		/// (since baselines are always horizontal).
		Baseline,
	}

	public readonly struct Color {
		public readonly byte Red;
		public readonly byte Green;
		public readonly byte Blue;
		public readonly byte Alpha;

		public Color (byte red, byte green, byte blue, byte alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		public static Color RGB (byte red, byte green, byte blue)
			=> new Color (red, green, blue, 255);

		public static Color RGBA (byte red, byte green, byte blue, byte alpha)
			=> new Color (red, green, blue, alpha);

		public uint ToARGB ()
		{
			return ((uint)Alpha << 24)
				| ((uint)Red << 16)
				| ((uint)Green << 8)
				| Blue;
		}
	}
}
