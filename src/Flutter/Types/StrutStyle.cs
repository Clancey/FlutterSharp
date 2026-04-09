using System.Collections.Generic;

namespace Flutter
{
	/// <summary>
	/// Defines the strut, which sets the minimum height a line can be relative to the baseline.
	/// </summary>
	public class StrutStyle
	{
		public string? FontFamily { get; set; }
		public List<string>? FontFamilyFallback { get; set; }
		public double? FontSize { get; set; }
		public double? Height { get; set; }
		public double? Leading { get; set; }
		public FontWeight? FontWeight { get; set; }
		public FontStyle? FontStyle { get; set; }
		public bool? ForceStrutHeight { get; set; }

		public StrutStyle(
			string? fontFamily = null,
			List<string>? fontFamilyFallback = null,
			double? fontSize = null,
			double? height = null,
			double? leading = null,
			FontWeight? fontWeight = null,
			FontStyle? fontStyle = null,
			bool? forceStrutHeight = null)
		{
			FontFamily = fontFamily;
			FontFamilyFallback = fontFamilyFallback;
			FontSize = fontSize;
			Height = height;
			Leading = leading;
			FontWeight = fontWeight;
			FontStyle = fontStyle;
			ForceStrutHeight = forceStrutHeight;
		}
	}

	/// <summary>
	/// The thickness of the glyphs used to draw the text.
	/// </summary>
	public enum FontWeight
	{
		W100 = 0,
		W200 = 1,
		W300 = 2,
		W400 = 3,
		W500 = 4,
		W600 = 5,
		W700 = 6,
		W800 = 7,
		W900 = 8,
		Normal = W400,
		Bold = W700
	}

	/// <summary>
	/// Whether to use the italic typeface variant or not.
	/// </summary>
	public enum FontStyle
	{
		Normal = 0,
		Italic = 1
	}
}
