using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a public Dart typedef definition.
	/// </summary>
	public record TypedefDefinition
	{
		/// <summary>
		/// Gets the typedef name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the namespace/library path for this typedef.
		/// </summary>
		public string Namespace { get; init; } = string.Empty;

		/// <summary>
		/// Gets the aliased Dart type expression.
		/// </summary>
		public string AliasedType { get; init; } = string.Empty;

		/// <summary>
		/// Gets the return type for function typedefs, when available.
		/// </summary>
		public string? ReturnType { get; init; }

		/// <summary>
		/// Gets the typedef parameters for function typedefs.
		/// </summary>
		public List<PropertyDefinition> Parameters { get; init; } = new();

		/// <summary>
		/// Gets a value indicating whether this typedef aliases a function type.
		/// </summary>
		public bool IsFunction { get; init; }

		/// <summary>
		/// Gets the documentation comment for this typedef.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets the source library/package this typedef comes from.
		/// </summary>
		public string? SourceLibrary { get; init; }

		/// <summary>
		/// Gets a value indicating whether this typedef is deprecated.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Gets the deprecation message if this typedef is deprecated.
		/// </summary>
		public string? DeprecationMessage { get; init; }

		/// <summary>
		/// Gets the generic type parameters if this typedef is generic.
		/// </summary>
		public List<string>? TypeParameters { get; init; }

		/// <summary>
		/// Gets the referenced public type names used by this typedef surface.
		/// </summary>
		public List<string> ReferencedTypes { get; init; } = new();

		/// <summary>
		/// Gets additional metadata about this typedef.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }
	}
}
