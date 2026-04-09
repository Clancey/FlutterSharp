using System;
using System.Collections.Generic;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Represents a mapping between Dart types, C# types, and FFI struct types.
	/// </summary>
	public record TypeMapping
	{
		/// <summary>
		/// Gets the Dart type name (e.g., "String", "int", "Widget", "List&lt;String&gt;").
		/// </summary>
		public string DartType { get; init; } = string.Empty;

		/// <summary>
		/// Gets the corresponding C# type name (e.g., "string", "int", "Widget", "List&lt;string&gt;").
		/// </summary>
		public string CSharpType { get; init; } = string.Empty;

		/// <summary>
		/// Gets the Dart FFI struct type used for marshalling (e.g., "Pointer&lt;Utf8&gt;", "Int32", "Pointer&lt;Void&gt;").
		/// </summary>
		public string DartStructType { get; init; } = string.Empty;

		/// <summary>
		/// Gets the Dart parser function name used to parse values from FFI (e.g., "parseString", "parseInt").
		/// This is the function that will be called on the Dart side to convert FFI types to Dart types.
		/// </summary>
		public string? DartParserFunction { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is nullable.
		/// </summary>
		public bool IsNullable { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is a collection (List, Set, Map, etc.).
		/// </summary>
		public bool IsCollection { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is a Widget or StatelessWidget.
		/// </summary>
		public bool IsWidget { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is an enum.
		/// </summary>
		public bool IsEnum { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is a primitive type (int, double, bool, String).
		/// </summary>
		public bool IsPrimitive { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type is a generic type.
		/// </summary>
		public bool IsGeneric { get; init; }

		/// <summary>
		/// Gets the generic type arguments if this is a generic type (e.g., ["String"] for List&lt;String&gt;).
		/// </summary>
		public List<string>? GenericArguments { get; init; }

		/// <summary>
		/// Gets a value indicating whether this type requires custom marshalling logic.
		/// </summary>
		public bool RequiresCustomMarshalling { get; init; }

		/// <summary>
		/// Gets the C# to Dart conversion expression template.
		/// Placeholders: {value} = the C# value, {type} = the type name.
		/// </summary>
		public string? CSharpToDartConversion { get; init; }

		/// <summary>
		/// Gets the Dart to C# conversion expression template.
		/// Placeholders: {value} = the Dart value, {type} = the type name.
		/// </summary>
		public string? DartToCSharpConversion { get; init; }

		/// <summary>
		/// Gets additional metadata about this type mapping.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is a custom user-defined type.
		/// </summary>
		public bool IsCustomType { get; init; }

		/// <summary>
		/// Gets the package/library this type belongs to (e.g., "flutter/material", "flutter/widgets").
		/// </summary>
		public string? Package { get; init; }

		/// <summary>
		/// Creates a non-nullable version of this type mapping.
		/// </summary>
		public TypeMapping AsNonNullable() => this with { IsNullable = false };

		/// <summary>
		/// Creates a nullable version of this type mapping.
		/// </summary>
		public TypeMapping AsNullable() => this with { IsNullable = true };

		/// <summary>
		/// Returns a string representation of this type mapping.
		/// </summary>
		public override string ToString()
		{
			var nullable = IsNullable ? "?" : "";
			var flags = new List<string>();
			if (IsWidget) flags.Add("Widget");
			if (IsCollection) flags.Add("Collection");
			if (IsEnum) flags.Add("Enum");
			if (IsPrimitive) flags.Add("Primitive");
			if (IsGeneric) flags.Add("Generic");

			var flagsStr = flags.Count > 0 ? $" [{string.Join(", ", flags)}]" : "";
			return $"{DartType}{nullable} -> {CSharpType}{nullable}{flagsStr}";
		}
	}
}
