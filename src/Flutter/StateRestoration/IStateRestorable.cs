using System;
using System.Collections.Generic;

namespace Flutter.StateRestoration
{
	/// <summary>
	/// Interface for widgets and components that support state restoration.
	/// Implementations can save their state to a dictionary and restore it later.
	/// </summary>
	public interface IStateRestorable
	{
		/// <summary>
		/// A unique identifier for this restorable object.
		/// Used to match saved state with the correct object during restoration.
		/// </summary>
		string RestorationId { get; }

		/// <summary>
		/// Saves the current state to a dictionary.
		/// </summary>
		/// <returns>Dictionary containing serializable state data</returns>
		Dictionary<string, object> SaveState();

		/// <summary>
		/// Restores state from a previously saved dictionary.
		/// </summary>
		/// <param name="state">Dictionary containing previously saved state</param>
		void RestoreState(Dictionary<string, object> state);
	}

	/// <summary>
	/// Marks a widget property as restorable.
	/// Properties with this attribute will be automatically saved/restored.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class RestorableAttribute : Attribute
	{
		/// <summary>
		/// Optional key override for the state dictionary.
		/// If not specified, the property name is used.
		/// </summary>
		public string Key { get; set; }

		public RestorableAttribute() { }

		public RestorableAttribute(string key)
		{
			Key = key;
		}
	}
}
