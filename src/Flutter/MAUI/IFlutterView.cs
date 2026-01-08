using Flutter;

namespace Flutter.MAUI
{
	/// <summary>
	/// Interface for cross-platform FlutterView
	/// </summary>
	public interface IFlutterView : Microsoft.Maui.IView
	{
		/// <summary>
		/// Gets or sets the Flutter widget to display
		/// </summary>
		Widget? Widget { get; set; }

		/// <summary>
		/// Gets or sets the aspect ratio (width/height) to maintain for the Flutter content.
		/// When set, the view will size itself to maintain this ratio within its constraints.
		/// A value of 0 or negative means no aspect ratio constraint.
		/// </summary>
		double AspectRatio { get; set; }

		/// <summary>
		/// Gets or sets whether the Flutter view should fill the available space.
		/// When true (default), the view expands to fill its container.
		/// When false, the view sizes to its content or specified dimensions.
		/// </summary>
		bool FillAvailableSpace { get; set; }

		/// <summary>
		/// Notifies the view that the container size has changed.
		/// This triggers a re-measure and notifies the Flutter engine of the new size.
		/// </summary>
		void OnContainerSizeChanged(double width, double height);
	}
}
