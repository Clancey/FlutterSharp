using System;
using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a property or parameter in a widget or class.
	/// </summary>
	public record PropertyDefinition
	{
		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the Dart type of the property.
		/// </summary>
		public string DartType { get; init; } = string.Empty;

		/// <summary>
		/// Gets the C# type that this property should be mapped to.
		/// </summary>
		public string? CSharpType { get; init; }

		/// <summary>
		/// Gets a value indicating whether this property is required.
		/// </summary>
		public bool IsRequired { get; init; }

		/// <summary>
		/// Gets a value indicating whether this property is nullable.
		/// </summary>
		public bool IsNullable { get; init; }

		/// <summary>
		/// Gets a value indicating whether this property is named (vs positional).
		/// </summary>
		public bool IsNamed { get; init; }

		/// <summary>
		/// Gets the default value for this property, if any.
		/// </summary>
		public string? DefaultValue { get; init; }

		/// <summary>
		/// Gets the documentation comment for this property.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets a value indicating whether this property is a list/collection type.
		/// </summary>
		public bool IsList { get; init; }

		/// <summary>
		/// Gets a value indicating whether this property is a callback/function type.
		/// </summary>
		public bool IsCallback { get; init; }

		/// <summary>
		/// Gets the generic type arguments if this is a generic type.
		/// </summary>
		public List<string>? TypeArguments { get; init; }

		/// <summary>
		/// Gets additional metadata about this property.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Returns a string representation of this property definition.
		/// </summary>
		public override string ToString()
		{
			var nullable = IsNullable ? "?" : "";
			var required = IsRequired ? "required " : "";
			return $"{required}{DartType}{nullable} {Name}";
		}
	}
}
