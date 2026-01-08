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
	}
}
