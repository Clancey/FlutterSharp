using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Maps Dart types to their corresponding C# types.
	/// </summary>
	public class DartToCSharpMapper
	{
		private readonly TypeMappingRegistry _registry;
		private static readonly Regex GenericTypeRegex = new(@"^(\w+)<(.+)>$", RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="DartToCSharpMapper"/> class.
		/// </summary>
		/// <param name="registry">The type mapping registry to use.</param>
		public DartToCSharpMapper(TypeMappingRegistry registry)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}

		/// <summary>
		/// Maps a Dart type string to its corresponding C# type.
		/// </summary>
		/// <param name="dartType">The Dart type string (e.g., "String", "int?", "List&lt;Widget&gt;").</param>
		/// <returns>The mapped C# type string.</returns>
		public string MapType(string dartType)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				throw new ArgumentException("Dart type cannot be null or empty.", nameof(dartType));
			}

			// Handle nullable types
			var isNullable = dartType.EndsWith("?");
			var cleanType = isNullable ? dartType.TrimEnd('?') : dartType;

			// Handle Dart function signatures with named parameters (contains '{')
			// These cannot be directly mapped to C# and should be treated as Delegate
			if (cleanType.Contains(" Function(") && cleanType.Contains("{"))
			{
				return "Delegate";
			}

			// Handle simple Dart function signatures without named parameters
			if (cleanType.Contains(" Function("))
			{
				return MapFunctionSignature(cleanType);
			}

			// Try exact match first
			var mapping = _registry.GetMapping(cleanType);
			if (mapping != null)
			{
				var csharpType = mapping.CSharpType;
				return isNullable && !mapping.IsNullable ? $"{csharpType}?" : csharpType;
			}

			// Handle generic types
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				var typeArgs = ParseGenericArguments(genericMatch.Groups[2].Value);
				var mappedArgs = typeArgs.Select(MapType).ToList();

				// Map the base type
				var baseMapping = _registry.GetMapping(baseType);
				if (baseMapping != null)
				{
					var mappedBase = baseMapping.CSharpType;
					var result = $"{mappedBase}<{string.Join(", ", mappedArgs)}>";
					return isNullable ? $"{result}?" : result;
				}

				// Fallback for unknown generic types
				var result2 = $"{baseType}<{string.Join(", ", mappedArgs)}>";
				return isNullable ? $"{result2}?" : result2;
			}

			// Fallback: return the type as-is (might be a custom type)
			return isNullable ? $"{cleanType}?" : cleanType;
		}

		/// <summary>
		/// Maps a Dart function signature to a C# delegate type.
		/// </summary>
		/// <param name="dartFunctionSignature">The Dart function signature (e.g., "void Function(int)" or "Widget Function(BuildContext)").</param>
		/// <returns>The mapped C# delegate type.</returns>
		private string MapFunctionSignature(string dartFunctionSignature)
		{
			// Extract return type and parameters
			var functionMatch = Regex.Match(dartFunctionSignature, @"^(.+?)\s+Function\(([^)]*)\)$");
			if (!functionMatch.Success)
			{
				// Fallback to Delegate for complex signatures
				return "Delegate";
			}

			var returnType = functionMatch.Groups[1].Value.Trim();
			var parameters = functionMatch.Groups[2].Value.Trim();

			// Map return type
			var mappedReturnType = MapType(returnType);

			// Map parameters
			var paramList = string.IsNullOrWhiteSpace(parameters)
				? new List<string>()
				: parameters.Split(',').Select(p => MapType(p.Trim())).ToList();

			// Generate appropriate C# delegate type
			if (mappedReturnType == "void")
			{
				if (paramList.Count == 0)
				{
					return "Action";
				}
				return $"Action<{string.Join(", ", paramList)}>";
			}
			else
			{
				if (paramList.Count == 0)
				{
					return $"Func<{mappedReturnType}>";
				}
				paramList.Add(mappedReturnType); // Func<T1, T2, ..., TResult>
				return $"Func<{string.Join(", ", paramList)}>";
			}
		}

		/// <summary>
		/// Maps a Dart type to a C# type with full type mapping information.
		/// </summary>
		/// <param name="dartType">The Dart type string.</param>
		/// <returns>The type mapping, or null if not found.</returns>
		public TypeMapping? MapTypeWithInfo(string dartType)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				return null;
			}

			// Handle nullable types
			var isNullable = dartType.EndsWith("?");
			var cleanType = isNullable ? dartType.TrimEnd('?') : dartType;

			var mapping = _registry.GetMapping(cleanType);
			if (mapping == null)
			{
				return null;
			}

			return isNullable && !mapping.IsNullable ? mapping.AsNullable() : mapping;
		}

		/// <summary>
		/// Checks if a Dart type is a known mapped type.
		/// </summary>
		/// <param name="dartType">The Dart type to check.</param>
		/// <returns>True if the type has a mapping, false otherwise.</returns>
		public bool HasMapping(string dartType)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				return false;
			}

			var cleanType = dartType.TrimEnd('?');

			// Check for exact match
			if (_registry.HasMapping(cleanType))
			{
				return true;
			}

			// Check for generic types
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				return _registry.HasMapping(baseType);
			}

			return false;
		}

		/// <summary>
		/// Gets the C# namespace for a Dart type, if applicable.
		/// </summary>
		/// <param name="dartType">The Dart type.</param>
		/// <returns>The C# namespace, or null if not applicable.</returns>
		public string? GetCSharpNamespace(string dartType)
		{
			var mapping = MapTypeWithInfo(dartType);
			if (mapping?.Metadata != null && mapping.Metadata.TryGetValue("CSharpNamespace", out var ns))
			{
				return ns as string;
			}
			return null;
		}

		/// <summary>
		/// Determines if a Dart type is a widget type.
		/// </summary>
		/// <param name="dartType">The Dart type to check.</param>
		/// <returns>True if the type is a widget, false otherwise.</returns>
		public bool IsWidget(string dartType)
		{
			var mapping = MapTypeWithInfo(dartType);
			return mapping?.IsWidget ?? false;
		}

		/// <summary>
		/// Determines if a Dart type is a collection type.
		/// </summary>
		/// <param name="dartType">The Dart type to check.</param>
		/// <returns>True if the type is a collection, false otherwise.</returns>
		public bool IsCollection(string dartType)
		{
			var mapping = MapTypeWithInfo(dartType);
			if (mapping?.IsCollection ?? false)
			{
				return true;
			}

			// Check for generic collection types
			var cleanType = dartType.TrimEnd('?');
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				var baseMapping = _registry.GetMapping(baseType);
				return baseMapping?.IsCollection ?? false;
			}

			return false;
		}

		/// <summary>
		/// Determines if a Dart type is an enum type.
		/// </summary>
		/// <param name="dartType">The Dart type to check.</param>
		/// <returns>True if the type is an enum, false otherwise.</returns>
		public bool IsEnum(string dartType)
		{
			var mapping = MapTypeWithInfo(dartType);
			return mapping?.IsEnum ?? false;
		}

		/// <summary>
		/// Parses generic type arguments from a string.
		/// Handles nested generics like "Map&lt;String, List&lt;int&gt;&gt;".
		/// </summary>
		private List<string> ParseGenericArguments(string args)
		{
			var result = new List<string>();
			var current = string.Empty;
			var depth = 0;

			foreach (var ch in args)
			{
				if (ch == '<')
				{
					depth++;
					current += ch;
				}
				else if (ch == '>')
				{
					depth--;
					current += ch;
				}
				else if (ch == ',' && depth == 0)
				{
					result.Add(current.Trim());
					current = string.Empty;
				}
				else
				{
					current += ch;
				}
			}

			if (!string.IsNullOrWhiteSpace(current))
			{
				result.Add(current.Trim());
			}

			return result;
		}
	}
}
