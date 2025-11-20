using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Maps C# types to Dart FFI struct types for marshalling across the FFI boundary.
	/// </summary>
	public class CSharpToDartFfiMapper
	{
		private readonly TypeMappingRegistry _registry;
		private static readonly Regex GenericTypeRegex = new(@"^(\w+)<(.+)>$", RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="CSharpToDartFfiMapper"/> class.
		/// </summary>
		/// <param name="registry">The type mapping registry to use.</param>
		public CSharpToDartFfiMapper(TypeMappingRegistry registry)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}

		/// <summary>
		/// Maps a C# type to its corresponding Dart FFI struct type.
		/// </summary>
		/// <param name="csharpType">The C# type string (e.g., "string", "int?", "List&lt;string&gt;").</param>
		/// <returns>The Dart FFI struct type string.</returns>
		public string MapToFfiType(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				throw new ArgumentException("C# type cannot be null or empty.", nameof(csharpType));
			}

			// Handle nullable types
			var isNullable = csharpType.EndsWith("?");
			var cleanType = isNullable ? csharpType.TrimEnd('?') : csharpType;

			// Try to find the mapping by C# type
			var mapping = _registry.GetMappingByCSharpType(cleanType);
			if (mapping != null)
			{
				return mapping.DartStructType;
			}

			// Handle generic types
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				var baseMapping = _registry.GetMappingByCSharpType(baseType);

				if (baseMapping != null)
				{
					// For collections, we typically use Pointer<Void> and handle marshalling separately
					return baseMapping.DartStructType;
				}
			}

			// Fallback for unknown types - use Pointer<Void>
			return "Pointer<Void>";
		}

		/// <summary>
		/// Gets the Dart parser function name for converting FFI types to Dart types.
		/// </summary>
		/// <param name="csharpType">The C# type string.</param>
		/// <returns>The parser function name, or null if not applicable.</returns>
		public string? GetParserFunction(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				return null;
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping != null)
			{
				return mapping.DartParserFunction;
			}

			// Handle generic types
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				var baseMapping = _registry.GetMappingByCSharpType(baseType);
				return baseMapping?.DartParserFunction;
			}

			return null;
		}

		/// <summary>
		/// Generates the C# to Dart FFI conversion code for a value.
		/// </summary>
		/// <param name="csharpType">The C# type.</param>
		/// <param name="valueName">The name of the variable to convert.</param>
		/// <returns>The conversion code expression.</returns>
		public string GetCSharpToDartConversion(string csharpType, string valueName)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				throw new ArgumentException("C# type cannot be null or empty.", nameof(csharpType));
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping?.CSharpToDartConversion != null)
			{
				return mapping.CSharpToDartConversion
					.Replace("{value}", valueName)
					.Replace("{type}", cleanType);
			}

			// Default conversions for common types
			return cleanType switch
			{
				"string" => $"{valueName}.ToNativeUtf8()",
				"int" => valueName,
				"double" => valueName,
				"bool" => $"({valueName} ? 1 : 0)",
				"float" => valueName,
				"long" => valueName,
				"byte" => valueName,
				"short" => valueName,
				_ => $"IntPtr.Zero /* TODO: Convert {cleanType} */"
			};
		}

		/// <summary>
		/// Generates the Dart FFI to C# conversion code for a value.
		/// </summary>
		/// <param name="csharpType">The target C# type.</param>
		/// <param name="valueName">The name of the FFI variable to convert.</param>
		/// <returns>The conversion code expression.</returns>
		public string GetDartToCSharpConversion(string csharpType, string valueName)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				throw new ArgumentException("C# type cannot be null or empty.", nameof(csharpType));
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping?.DartToCSharpConversion != null)
			{
				return mapping.DartToCSharpConversion
					.Replace("{value}", valueName)
					.Replace("{type}", cleanType);
			}

			// Default conversions for common types
			return cleanType switch
			{
				"string" => $"Marshal.PtrToStringUTF8({valueName})",
				"int" => valueName,
				"double" => valueName,
				"bool" => $"({valueName} != 0)",
				"float" => valueName,
				"long" => valueName,
				"byte" => valueName,
				"short" => valueName,
				_ => $"default({cleanType}) /* TODO: Convert from FFI */"
			};
		}

		/// <summary>
		/// Determines if a C# type requires custom marshalling logic.
		/// </summary>
		/// <param name="csharpType">The C# type to check.</param>
		/// <returns>True if custom marshalling is required, false otherwise.</returns>
		public bool RequiresCustomMarshalling(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				return false;
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping != null)
			{
				return mapping.RequiresCustomMarshalling;
			}

			// Generic types and complex types typically require custom marshalling
			return GenericTypeRegex.IsMatch(cleanType);
		}

		/// <summary>
		/// Gets the FFI type size in bytes, if known.
		/// </summary>
		/// <param name="csharpType">The C# type.</param>
		/// <returns>The size in bytes, or null if not known or variable.</returns>
		public int? GetFfiTypeSize(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				return null;
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping?.Metadata != null &&
				mapping.Metadata.TryGetValue("FfiSize", out var size) &&
				size is int sizeInt)
			{
				return sizeInt;
			}

			// Known sizes for primitive types
			return cleanType switch
			{
				"byte" => 1,
				"bool" => 1,
				"short" => 2,
				"int" => 4,
				"float" => 4,
				"long" => 8,
				"double" => 8,
				_ => null
			};
		}

		/// <summary>
		/// Determines if the type is passed by value or by reference in FFI.
		/// </summary>
		/// <param name="csharpType">The C# type.</param>
		/// <returns>True if passed by value, false if by reference/pointer.</returns>
		public bool IsPassedByValue(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				return false;
			}

			var cleanType = csharpType.TrimEnd('?');
			var mapping = _registry.GetMappingByCSharpType(cleanType);

			if (mapping != null)
			{
				return mapping.IsPrimitive && !mapping.IsCollection;
			}

			// Primitives are passed by value
			return cleanType switch
			{
				"byte" or "bool" or "short" or "int" or "float" or "long" or "double" => true,
				_ => false
			};
		}
	}
}
