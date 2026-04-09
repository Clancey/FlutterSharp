namespace Flutter
{
	/// <summary>
	/// Represents the severity level of a memory warning.
	/// Used to inform Flutter and C# code about system memory pressure.
	/// </summary>
	public enum MemoryWarningLevel
	{
		/// <summary>
		/// Low memory pressure. Consider releasing non-essential caches.
		/// </summary>
		Low,

		/// <summary>
		/// Moderate memory pressure. Release caches and temporary data.
		/// </summary>
		Medium,

		/// <summary>
		/// High memory pressure. Release all non-essential resources immediately.
		/// </summary>
		High,

		/// <summary>
		/// Critical memory pressure. The app may be terminated soon.
		/// Release everything possible to avoid termination.
		/// </summary>
		Critical
	}
}
