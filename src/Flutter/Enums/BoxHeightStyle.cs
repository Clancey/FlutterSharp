namespace Flutter.Enums
{
	/// <summary>
	/// Defines the way to treat the selection's rect height.
	/// </summary>
	public enum BoxHeightStyle
	{
		/// Provide tight bounding boxes that fit heights per run.
		Tight = 0,
		/// The box height is the largest of the heights in the line.
		Max = 1,
		/// The height of every box is the full strut height.
		Strut = 2,
		/// The height includes all the content regardless of baseline.
		IncludeLineSpacingMiddle = 3,
		IncludeLineSpacingTop = 4,
		IncludeLineSpacingBottom = 5
	}
}
