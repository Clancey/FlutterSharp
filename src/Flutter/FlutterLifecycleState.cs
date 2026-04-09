namespace Flutter
{
	/// <summary>
	/// Represents Flutter app lifecycle states for lifecycle management.
	/// Maps to Flutter's AppLifecycleState enum.
	/// </summary>
	public enum FlutterLifecycleState
	{
		/// <summary>
		/// App is in the foreground and receiving user input.
		/// This is the active state where the app is visible and interactive.
		/// </summary>
		Resumed,

		/// <summary>
		/// App is visible but not receiving user input.
		/// This state occurs during page transitions or when a dialog is shown.
		/// </summary>
		Inactive,

		/// <summary>
		/// App is hidden in the background.
		/// The app is not visible but still running.
		/// </summary>
		Paused,

		/// <summary>
		/// App is detached from any host view.
		/// This can occur when the engine is being destroyed.
		/// </summary>
		Detached
	}
}
