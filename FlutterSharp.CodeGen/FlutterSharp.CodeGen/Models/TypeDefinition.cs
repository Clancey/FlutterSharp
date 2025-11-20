using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a custom type (class) definition from Dart.
	/// </summary>
	public record TypeDefinition
	{
		/// <summary>
		/// Gets the name of the type.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the namespace/library path for this type.
		/// </summary>
		public string Namespace { get; init; } = string.Empty;

		/// <summary>
		/// Gets the base class name, if any.
		/// </summary>
		public string? BaseClass { get; init; }

		/// <summary>
		/// Gets the list of interfaces this type implements.
		/// </summary>
		public List<string>? Interfaces { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is an abstract class.
		/// </summary>
		public bool IsAbstract { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is an immutable class.
		/// </summary>
		public bool IsImmutable { get; init; }

		/// <summary>
		/// Gets the properties of this type.
		/// </summary>
		public List<PropertyDefinition> Properties { get; init; } = new();

		/// <summary>
		/// Gets the constructors for this type.
		/// </summary>
		public List<ConstructorDefinition> Constructors { get; init; } = new();

		/// <summary>
		/// Gets the documentation comment for this type.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets the source library/package this type comes from.
		/// </summary>
		public string? SourceLibrary { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is deprecated.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Gets the deprecation message if this type is deprecated.
		/// </summary>
		public string? DeprecationMessage { get; init; }

		/// <summary>
		/// Gets the generic type parameters if this is a generic type.
		/// </summary>
		public List<string>? TypeParameters { get; init; }

		/// <summary>
		/// Gets additional metadata about this type.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Returns a string representation of this type definition.
		/// </summary>
		public override string ToString()
		{
			var abstractModifier = IsAbstract ? "abstract " : "";
			var baseClassInfo = BaseClass != null ? $" : {BaseClass}" : "";
			var propertyCount = Properties?.Count ?? 0;
			return $"{abstractModifier}class {Name}{baseClassInfo} ({propertyCount} properties)";
		}
	}
}
