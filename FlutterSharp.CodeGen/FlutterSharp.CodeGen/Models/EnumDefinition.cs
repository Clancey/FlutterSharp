using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents an enum type definition from Dart.
	/// </summary>
	public record EnumDefinition
	{
		/// <summary>
		/// Gets the name of the enum.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the namespace/library path for this enum.
		/// </summary>
		public string Namespace { get; init; } = string.Empty;

		/// <summary>
		/// Gets the enum values.
		/// </summary>
		public List<EnumValueDefinition> Values { get; init; } = new();

		/// <summary>
		/// Gets the documentation comment for this enum.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets the source library/package this enum comes from.
		/// </summary>
		public string? SourceLibrary { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is a deprecated enum.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Gets additional metadata about this enum.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Returns a string representation of this enum definition.
		/// </summary>
		public override string ToString()
		{
			var valueCount = Values?.Count ?? 0;
			return $"enum {Name} ({valueCount} values)";
		}
	}

	/// <summary>
	/// Represents a single value in an enum.
	/// </summary>
	public record EnumValueDefinition
	{
		/// <summary>
		/// Gets the name of the enum value.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the integer value, if explicitly defined.
		/// </summary>
		public int? Value { get; init; }

		/// <summary>
		/// Gets the documentation comment for this enum value.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets a value indicating whether this enum value is deprecated.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Returns a string representation of this enum value.
		/// </summary>
		public override string ToString()
		{
			return Value.HasValue ? $"{Name} = {Value}" : Name;
		}
	}
}
