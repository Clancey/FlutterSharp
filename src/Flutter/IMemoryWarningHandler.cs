using System;

namespace Flutter
{
	/// <summary>
	/// Interface for handling system memory warnings.
	/// Implement this interface to receive memory warning notifications and clean up resources.
	/// </summary>
	public interface IMemoryWarningHandler
	{
		/// <summary>
		/// Called when a memory warning is received from the system.
		/// Implementations should release caches and non-essential resources.
		/// </summary>
		/// <param name="level">The severity level of the memory warning</param>
		void OnMemoryWarning(MemoryWarningLevel level);
	}

	/// <summary>
	/// Event arguments for memory warning events.
	/// </summary>
	public class MemoryWarningEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the severity level of the memory warning.
		/// </summary>
		public MemoryWarningLevel Level { get; }

		/// <summary>
		/// Gets the timestamp when the warning was received.
		/// </summary>
		public DateTimeOffset Timestamp { get; }

		/// <summary>
		/// Gets or sets whether the warning has been handled.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Creates a new MemoryWarningEventArgs instance.
		/// </summary>
		/// <param name="level">The severity level of the memory warning</param>
		public MemoryWarningEventArgs(MemoryWarningLevel level)
		{
			Level = level;
			Timestamp = DateTimeOffset.UtcNow;
		}
	}
}
