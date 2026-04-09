using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a constructor for a widget or class.
	/// </summary>
	public record ConstructorDefinition
	{
		/// <summary>
		/// Gets the name of the constructor (empty for default constructor).
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets a value indicating whether this is a const constructor.
		/// </summary>
		public bool IsConst { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is a factory constructor.
		/// </summary>
		public bool IsFactory { get; init; }

		/// <summary>
		/// Gets the parameters for this constructor.
		/// </summary>
		public List<PropertyDefinition> Parameters { get; init; } = new();

		/// <summary>
		/// Gets the documentation comment for this constructor.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets a value indicating whether this constructor is deprecated.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Gets the deprecation message if this constructor is deprecated.
		/// </summary>
		public string? DeprecationMessage { get; init; }

		/// <summary>
		/// Gets additional metadata about this constructor.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Gets the full constructor name including the class name.
		/// </summary>
		public string? FullName { get; init; }

		/// <summary>
		/// Returns a string representation of this constructor definition.
		/// </summary>
		public override string ToString()
		{
			var constModifier = IsConst ? "const " : "";
			var factoryModifier = IsFactory ? "factory " : "";
			var paramCount = Parameters?.Count ?? 0;
			var name = string.IsNullOrEmpty(Name) ? "default" : Name;
			return $"{constModifier}{factoryModifier}{FullName ?? name}({paramCount} parameters)";
		}
	}
}
