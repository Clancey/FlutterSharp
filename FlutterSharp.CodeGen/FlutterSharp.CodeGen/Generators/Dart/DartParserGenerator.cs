using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlutterSharp.CodeGen.Analysis;
using FlutterSharp.CodeGen.Models;
using FlutterSharp.CodeGen.TypeMapping;
using Scriban;

namespace FlutterSharp.CodeGen.Generators.Dart
{
	/// <summary>
	/// Generates Dart widget parser classes that convert FFI structs to Flutter widgets.
	/// </summary>
	public class DartParserGenerator
	{
		private readonly CSharpToDartFfiMapper _typeMapper;
		private readonly Template _template;
		private readonly Dictionary<string, Dictionary<string, string>> _structPropertyCache = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="DartParserGenerator"/> class.
		/// </summary>
		/// <param name="typeMapper">The type mapper to use for converting types.</param>
		public DartParserGenerator(CSharpToDartFfiMapper typeMapper)
		{
			_typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));

			// Load the Scriban template
			var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "DartParser.scriban");
			if (File.Exists(templatePath))
			{
				var templateContent = File.ReadAllText(templatePath);
				_template = Template.Parse(templateContent);
			}
			else
			{
				// Use inline template as fallback
				_template = Template.Parse(GetDefaultTemplate());
			}
		}

		/// <summary>
		/// Generates a Dart widget parser class from a widget definition.
		/// </summary>
		/// <param name="widget">The widget definition to generate the parser from.</param>
		/// <returns>The generated Dart parser code.</returns>
		public string Generate(WidgetDefinition widget)
		{
			if (widget == null)
			{
				throw new ArgumentNullException(nameof(widget));
			}

			// Prepare the model for the template
			// Remove leading underscore from parser name to make it public (Dart private members can't be exported)
			var publicName = widget.Name.TrimStart('_');

			// Filter out child/children properties - they're handled separately by the template
			var childPropName = widget.ChildPropertyName?.ToLowerInvariant();
			var childrenPropName = widget.ChildrenPropertyName?.ToLowerInvariant();
			var regularProperties = widget.Properties
				.Where(p => {
					var propName = p.Name.ToLowerInvariant();
					return propName != childPropName && propName != childrenPropName;
				})
				.ToList();

			// Find the child property to check if it's nullable
			PropertyDefinition? childProperty = null;
			bool childIsNullable = true; // Safe default
			if (widget.ChildPropertyName != null)
			{
				childProperty = widget.Properties.FirstOrDefault(p =>
					p.Name.Equals(widget.ChildPropertyName, StringComparison.OrdinalIgnoreCase));

				// Determine if child is nullable by checking if DartType ends with '?'
				if (childProperty != null && !string.IsNullOrEmpty(childProperty.DartType))
				{
					childIsNullable = childProperty.DartType.EndsWith("?");
				}
			}

			var model = new
			{
				widget.Name,
				ParserName = $"{publicName}Parser",
				StructName = $"{widget.Name}Struct",
				WidgetName = widget.Name,
				Properties = regularProperties.Select(p => {
					var isPointer = IsPointerTypeForWidget(p, widget.Name);
					var isPrimitive = IsPrimitiveType(p);
					var isEnum = IsEnumType(p);
					var parserFunc = GetParserFunction(p);
					var baseType = p.DartType?.TrimEnd('?') ?? "";
					var ffiType = _typeMapper.MapToFfiType(p.CSharpType ?? p.DartType);

					// Determine if this is a Color stored as a primitive
					var isColorPrimitive = baseType == "Color" && (ffiType == "Uint32" || ffiType == "Int32");

					// Debug logging
					if (widget.Name == "Container")
					{
						Console.WriteLine($"[{widget.Name} Property] {p.Name}: DartType={p.DartType}, IsNullable={p.IsNullable}, CSharpType={p.CSharpType}, IsPointer={isPointer}, IsPrimitive={isPrimitive}, IsEnum={isEnum}, IsColorPrimitive={isColorPrimitive}, Parser={parserFunc}");
					}

					return new
					{
						p.Name,
						PropertyName = ToCamelCase(p.Name),
						DartType = p.DartType,
						CSharpType = p.CSharpType ?? "object",
						ParserFunction = parserFunc,
						p.IsNullable,
						p.IsRequired,
						p.DefaultValue,
						HasDefaultValue = !string.IsNullOrWhiteSpace(p.DefaultValue),
						IsBool = p.DartType?.TrimEnd('?') == "bool",
						IsCallback = p.IsCallback,
						IsPointerVoid = _typeMapper.MapToFfiType(p.CSharpType ?? p.DartType) is "IntPtr" or "Pointer<Void>",
						IsPointerType = isPointer,
						IsPrimitiveType = isPrimitive,
						IsEnumType = isEnum,
						IsColorPrimitive = isColorPrimitive,
						Documentation = FormatDartDocumentation(p.Documentation)
					};
				}).ToList(),
				HasChildren = widget.HasSingleChild || widget.HasMultipleChildren,
				ChildPropertyName = widget.ChildPropertyName != null ? ToCamelCase(widget.ChildPropertyName) : null,
				child_is_nullable = childIsNullable,
				ChildrenPropertyName = widget.ChildrenPropertyName != null ? ToCamelCase(widget.ChildrenPropertyName) : null,
				Documentation = FormatDartDocumentation(widget.Documentation),
				GeneratedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
			};

			// Render the template
			var result = _template.Render(model);
			return result;
		}

	/// <summary>
	/// Generates a Dart widget parser class from an enriched widget definition (new architecture).
	/// </summary>
	/// <param name="enrichedWidget">The enriched widget definition to generate the parser from.</param>
	/// <returns>The generated Dart parser code.</returns>
	public string Generate(EnrichedWidgetDefinition enrichedWidget)
	{
		if (enrichedWidget == null)
		{
			throw new ArgumentNullException(nameof(enrichedWidget));
		}

		// Prepare the model for the template
		// Remove leading underscore from parser name to make it public (Dart private members can't be exported)
		var publicName = enrichedWidget.Name.TrimStart('_');

		// Filter out child/children properties - they're handled separately by the template
		var childPropName = enrichedWidget.ChildPropertyName?.ToLowerInvariant();
		var childrenPropName = enrichedWidget.ChildrenPropertyName?.ToLowerInvariant();
		var regularProperties = enrichedWidget.AllProperties
			.Where(p => {
				var propName = p.Name.ToLowerInvariant();
				return propName != childPropName && propName != childrenPropName;
			})
			.ToList();

		// Find the child property to check if it's nullable
		EnrichedPropertyDefinition? childProperty = null;
		bool childIsNullable = true; // Safe default
		if (enrichedWidget.ChildPropertyName != null)
		{
			childProperty = enrichedWidget.AllProperties.FirstOrDefault(p =>
				p.Name.Equals(enrichedWidget.ChildPropertyName, StringComparison.OrdinalIgnoreCase));

			// Determine if child is nullable
			if (childProperty != null)
			{
				childIsNullable = childProperty.IsNullable;
			}
		}

		var model = new
		{
			enrichedWidget.Name,
			ParserName = $"{publicName}Parser",
			StructName = enrichedWidget.StructName,
			WidgetName = enrichedWidget.Name,
			Properties = regularProperties.Select(p => {
				var isPointer = p.FfiType.StartsWith("Pointer<");
				var isPrimitive = IsPrimitiveTypeFfi(p.FfiType);
				var isEnum = p.FfiAnnotation?.Contains("Int32") == true;
				var parserFunc = GetParserFunctionFromEnriched(p);
				var baseType = p.DartType?.TrimEnd('?') ?? "";
				var ffiType = p.FfiType;

				// Determine if this is a Color stored as a primitive
				var isColorPrimitive = baseType == "Color" && (ffiType == "Uint32" || ffiType == "Int32" || ffiType == "int");

				return new
				{
					p.Name,
					PropertyName = p.IsCallback ? ToCamelCase(p.Name) + "Action" : ToCamelCase(p.Name),
					p.DartType,
					FfiType = ffiType,
					IsPointer = isPointer,
					IsPrimitive = isPrimitive,
					IsEnum = isEnum,
					IsColorPrimitive = isColorPrimitive,
					ParserFunction = parserFunc,
					p.IsNullable,
					p.IsCallback,
					Documentation = FormatDartDocumentation(p.Documentation)
				};
			}).ToList(),
			has_single_child = enrichedWidget.HasSingleChild,
			has_multiple_children = enrichedWidget.HasMultipleChildren,
			ChildPropertyName = enrichedWidget.ChildPropertyName != null ? ToCamelCase(enrichedWidget.ChildPropertyName) : null,
			child_is_nullable = childIsNullable,
			ChildrenPropertyName = enrichedWidget.ChildrenPropertyName != null ? ToCamelCase(enrichedWidget.ChildrenPropertyName) : null,
			Documentation = FormatDartDocumentation(enrichedWidget.Documentation),
			GeneratedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
		};

		// Render the template
		var result = _template.Render(model);
		return result;
	}

		/// <summary>
		/// Loads property type information from the generated struct file.
		/// </summary>
		private Dictionary<string, string> LoadStructPropertyTypes(string widgetName)
		{
			// Check cache first
			if (_structPropertyCache.TryGetValue(widgetName, out var cachedProperties))
			{
				return cachedProperties;
			}

			var propertyTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			// Construct path to struct file
			var structFileName = $"{widgetName.ToLowerInvariant()}_struct.dart";
			var structPath = Path.Combine(
				"/Users/clancey/Projects/FlutterSharp/flutter_module/lib/generated/structs",
				structFileName
			);

			if (!File.Exists(structPath))
			{
				// Cache empty result to avoid repeated file lookups
				_structPropertyCache[widgetName] = propertyTypes;
				return propertyTypes;
			}

			try
			{
				var content = File.ReadAllText(structPath);
				var lines = content.Split('\n');

				// Parse external field declarations
				// Format: external Pointer<SomeStruct> propertyName;
				//         external int propertyName;
				//         @Int32() external int propertyName;
				foreach (var line in lines)
				{
					var trimmed = line.Trim();

					// Skip lines that don't contain external declarations
					if (!trimmed.Contains("external"))
					{
						continue;
					}

					// Remove annotation if present (e.g., @Int32(), @Uint32())
					var workingLine = trimmed;
					if (workingLine.StartsWith("@"))
					{
						var externalIdx = workingLine.IndexOf("external");
						if (externalIdx > 0)
						{
							workingLine = workingLine.Substring(externalIdx);
						}
					}

					// Parse: external <Type> <propertyName>;
					var parts = workingLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 3 && parts[0] == "external")
					{
						var type = parts[1];
						var propertyName = parts[2].TrimEnd(';');

						// Handle generic types like Pointer<AlignmentGeometryStruct>
						if (type.Contains("<") && parts.Length > 3)
						{
							// Reconstruct the full type (e.g., "Pointer<AlignmentGeometryStruct>")
							var typeBuilder = new StringBuilder(type);
							for (int i = 2; i < parts.Length - 1; i++)
							{
								if (parts[i].Contains(">"))
								{
									typeBuilder.Append(parts[i]);
									propertyName = parts[i + 1].TrimEnd(';');
									break;
								}
								typeBuilder.Append(parts[i]);
							}
							type = typeBuilder.ToString();
						}

						propertyTypes[propertyName] = type;
					}
				}

				// Cache the results
				_structPropertyCache[widgetName] = propertyTypes;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Warning: Failed to parse struct file for {widgetName}: {ex.Message}");
				_structPropertyCache[widgetName] = propertyTypes;
			}

			return propertyTypes;
		}

		/// <summary>
		/// Determines if a property is a pointer type (Pointer<SomeStruct>) by reading the actual struct file.
		/// </summary>
		private bool IsPointerType(PropertyDefinition property)
		{
			// First, try to get the actual FFI type from the generated struct file
			var widgetName = property.CSharpType?.Split('.').LastOrDefault() ?? property.Name;

			// Try to determine widget name from context - this will be set by the caller
			// For now, we'll use a heuristic approach
			// This is a limitation - we need the widget context passed down
			// Let's try to extract it from the property's parent context if available

			// Since we don't have direct widget context, we'll use a fallback approach:
			// 1. Try to read from the type mapper first
			// 2. Check if it's a known complex type

			var typeToCheck = !string.IsNullOrWhiteSpace(property.CSharpType)
				? property.CSharpType
				: property.DartType;

			// Skip check if we don't have a valid type to map
			if (string.IsNullOrWhiteSpace(typeToCheck))
			{
				return false;
			}

			var ffiType = _typeMapper.MapToFfiType(typeToCheck);

			// Check if the FFI type is a pointer to a struct
			if (ffiType.StartsWith("Pointer<"))
			{
				// If it's explicitly Pointer<SomeStruct> (not Void), it's definitely a pointer
				if (ffiType != "Pointer<Void>")
				{
					return true;
				}

				// For Pointer<Void>, check if it's a known complex type that should be a pointer
				var dartBaseType = (property.DartType ?? "").TrimEnd('?');
				var parserFunc = GetParserFunction(property);

				// Complex types that have parsers and are PascalCase are likely pointers
				if (!string.IsNullOrEmpty(dartBaseType) &&
				    !IsPrimitiveTypeByName(dartBaseType) &&
				    parserFunc != null &&
				    char.IsUpper(dartBaseType[0]))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines if a property is a pointer type for a specific widget by reading the struct file.
		/// </summary>
		private bool IsPointerTypeForWidget(PropertyDefinition property, string widgetName)
		{
			// Load the struct properties for this widget
			var structProperties = LoadStructPropertyTypes(widgetName);

			// Look up the property type
			var propertyName = ToCamelCase(property.Name);
			if (structProperties.TryGetValue(propertyName, out var ffiType))
			{
				// Check if it's a Pointer type
				return ffiType.StartsWith("Pointer<");
			}

			// Fallback to the original heuristic-based approach
			return IsPointerType(property);
		}

		/// <summary>
		/// Helper to check if a type name is a primitive without property context.
		/// </summary>
		private bool IsPrimitiveTypeByName(string dartType)
		{
			var baseType = dartType.TrimEnd('?');
			return baseType == "String" || baseType == "int" || baseType == "double" ||
			       baseType == "bool" || baseType == "num";
		}

		/// <summary>
		/// Determines if a property is a primitive type (int, double, bool, String).
		/// </summary>
		private bool IsPrimitiveType(PropertyDefinition property)
		{
			var baseType = property.DartType?.TrimEnd('?') ?? "";
			return baseType == "String" || baseType == "int" || baseType == "double" ||
			       baseType == "bool" || baseType == "num";
		}

		/// <summary>
		/// Determines if a property is an enum type (stored as int in FFI).
		/// </summary>
		private bool IsEnumType(PropertyDefinition property)
		{
			var dartType = property.DartType?.TrimEnd('?') ?? "";

			// If Dart type is a primitive, it's not an enum
			if (dartType == "int" || dartType == "double" || dartType == "bool" || dartType == "String")
			{
				return false; // Actual primitive
			}

			// Try CSharpType first, then DartType
			var typeToCheck = !string.IsNullOrWhiteSpace(property.CSharpType)
				? property.CSharpType
				: property.DartType;

			// Skip check if we don't have a valid type to map
			if (string.IsNullOrWhiteSpace(typeToCheck))
			{
				return false;
			}

			var ffiType = _typeMapper.MapToFfiType(typeToCheck);
			// Enums are typically mapped to int32/uint32 in FFI
			return ffiType == "Int32" || ffiType == "Uint32" || ffiType == "Int8" || ffiType == "Uint8";
		}

	/// <summary>
	/// Determines if an FFI type is a primitive type.
	/// </summary>
	private bool IsPrimitiveTypeFfi(string ffiType)
	{
		// Check if it's a basic FFI primitive type or Dart primitive
		return ffiType == "int" || ffiType == "double" || ffiType == "bool" ||
		       ffiType == "Int8" || ffiType == "Int16" || ffiType == "Int32" || ffiType == "Int64" ||
		       ffiType == "Uint8" || ffiType == "Uint16" || ffiType == "Uint32" || ffiType == "Uint64" ||
		       ffiType == "Float" || ffiType == "Double";
	}

	/// <summary>
	/// Gets the parser function for an enriched property.
	/// </summary>
	private string GetParserFunctionFromEnriched(EnrichedPropertyDefinition property)
	{
		// Get base type without nullable suffix
		var baseType = property.DartType?.TrimEnd('?') ?? "";

		// Primitive types don't need parser functions
		if (baseType == "String" || baseType == "int" || baseType == "double" || baseType == "bool" || baseType == "num")
		{
			return null;
		}

		// Callbacks don't need parser functions (they're action strings)
		if (property.IsCallback)
		{
			return null;
		}

		// Enums don't need parser functions (they're int values)
		if (property.FfiAnnotation?.Contains("Int32") == true)
		{
			return null;
		}

		// Color has a special parser
		if (baseType == "Color")
		{
			return "parseColor";
		}

		// Default: use the type name to derive parser name
		// e.g., "EdgeInsets" -> "parseEdgeInsets"
		if (!string.IsNullOrEmpty(baseType))
		{
			return $"parse{baseType}";
		}

		return null;
	}

		/// <summary>
		/// Gets the parser function for a property.
		/// </summary>
		private string GetParserFunction(PropertyDefinition property)
		{
			var csharpType = property.CSharpType ?? property.DartType;

			// Get base type without nullable suffix
			var baseType = property.DartType?.TrimEnd('?') ?? "";

			// Primitive types don't need parser functions
			if (baseType == "String" || baseType == "int" || baseType == "double" || baseType == "bool" || baseType == "num")
			{
				return null;
			}

			// Check FFI type to determine if this is stored as a primitive
			var ffiType = _typeMapper.MapToFfiType(csharpType ?? property.DartType);

			// Special case: Color type stored as Uint32 primitive
			// Color should use direct constructor Color(value), not parseColor(ColorStruct)
			if (baseType == "Color")
			{
				if (ffiType == "Uint32" || ffiType == "Int32")
				{
					// Return null - Color will be handled specially in the template
					return null;
				}
			}

			// Special case: Enums stored as Int32/Uint32 primitives
			// These should use EnumType.values[index], not a parser function
			if (IsEnumType(property))
			{
				// Return null - Enums stored as primitives will be handled in the template
				return null;
			}

			var parserFunction = _typeMapper.GetParserFunction(csharpType);

			if (!string.IsNullOrEmpty(parserFunction))
			{
				return parserFunction;
			}

			// Try CSharpType as well
			if (!string.IsNullOrEmpty(property.CSharpType))
			{
				parserFunction = _typeMapper.GetParserFunction(property.CSharpType);
				if (!string.IsNullOrEmpty(parserFunction))
				{
					return parserFunction;
				}
			}

			// Try to infer from property name before giving up on Pointer<Void> types
			var propName = property.Name.ToLowerInvariant();
			if (propName.Contains("alignment"))
			{
				return "parseAlignment";
			}
			if (propName.Contains("color"))
			{
				// Only use parseColor if it's NOT a primitive (already handled above)
				if (ffiType != "Uint32" && ffiType != "Int32")
				{
					return "parseColor";
				}
			}
			if (propName.Contains("edge") || propName.Contains("padding") || propName.Contains("margin"))
			{
				return "parseEdgeInsets";
			}

			// Check if this property maps to Pointer<Void> - these should be passed through without parsing
			// This check comes AFTER property name inference so we don't lose known parser functions
			if (ffiType == "Pointer<Void>" || ffiType == "IntPtr")
			{
				// No parser function - will be passed through as-is
				return null;
			}

			// Default parser functions based on Dart type
			return property.DartType switch
			{
				_ when property.DartType?.StartsWith("List<") == true => "parseList",
				_ when property.DartType?.StartsWith("Map<") == true => "parseMap",
				_ when property.IsCallback => "parseCallback",
				_ => null  // Return null instead of "parseObject" - let template handle pass-through
			};
		}

		/// <summary>
		/// Gets the required imports for the generated Dart parser.
		/// </summary>
		private List<string> GetRequiredImports(WidgetDefinition widget)
		{
			var imports = new HashSet<string>
			{
				"dart:ffi",
				"package:ffi/ffi.dart",
				"package:flutter/widgets.dart"
			};

			// Add Flutter material import if needed
			if (widget.SourceLibrary != null && widget.SourceLibrary.Contains("material"))
			{
				imports.Add("package:flutter/material.dart");
			}

			// Add package import based on source library
			if (!string.IsNullOrEmpty(widget.SourceLibrary))
			{
				var package = widget.SourceLibrary;
				if (package.StartsWith("flutter/"))
				{
					imports.Add($"package:{package}.dart");
				}
			}

			// Add imports for specific property types
			foreach (var property in widget.Properties)
			{
				if (property.DartType.Contains("Color"))
				{
					imports.Add("package:flutter/material.dart");
				}
				else if (property.DartType.Contains("TextStyle") || property.DartType.Contains("EdgeInsets"))
				{
					imports.Add("package:flutter/painting.dart");
				}
			}

			return imports.OrderBy(i => i).ToList();
		}

		/// <summary>
		/// Converts a PascalCase string to camelCase.
		/// Also strips the @ prefix that C# uses for reserved word escaping (not valid in Dart).
		/// </summary>
		private string ToCamelCase(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			// Strip @ prefix (C# reserved word escaping) - not valid in Dart
			if (value.StartsWith("@"))
			{
				value = value.Substring(1);
			}

			if (char.IsLower(value[0]))
			{
				return value;
			}

			return char.ToLowerInvariant(value[0]) + value.Substring(1);
		}

		/// <summary>
		/// Formats multiline documentation for Dart by ensuring each line has a /// prefix.
		/// </summary>
		private string? FormatDartDocumentation(string? documentation)
		{
			if (string.IsNullOrWhiteSpace(documentation))
			{
				return documentation;
			}

			// Split by newlines and prefix each line with ///
			var lines = documentation.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			var formatted = new StringBuilder();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].TrimStart();
				// Remove existing /// prefix if present
				if (line.StartsWith("///"))
				{
					line = line.Substring(3).TrimStart();
				}

				formatted.Append(line);

				// Add newline with /// prefix for all but the last line
				if (i < lines.Length - 1)
				{
					formatted.AppendLine();
					formatted.Append("  /// ");
				}
			}

			return formatted.ToString();
		}

		/// <summary>
		/// Gets the default template if the template file is not found.
		/// </summary>
		private string GetDefaultTemplate()
		{
			return @"// <auto-generated>
// This file was automatically generated on {{ generated_date }}
// DO NOT EDIT - Changes will be overwritten
// </auto-generated>

import 'dart:ffi';
import 'package:flutter/widgets.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import '../utils.dart';
import '../generated_utility_parsers.dart';
import '../maui_flutter.dart';

{{~ if documentation ~}}
/// {{ documentation }}
{{~ end ~}}
class {{ parser_name }} extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<{{ struct_name }}>.fromAddress(fos.handle.address).ref;
    return {{ widget_name }}(
{{~ for prop in properties ~}}
{{~ if prop.is_required ~}}
      {{ prop.property_name }}: {{ prop.parser_function }}(map.{{ prop.property_name }}){{~ if !for.last || has_children ~}},{{~ end ~}}
{{~ else ~}}
{{~ if prop.is_nullable ~}}
      {{ prop.property_name }}: map.{{ prop.property_name }}.address != 0
        ? {{ prop.parser_function }}(map.{{ prop.property_name }})
        : null{{~ if !for.last || has_children ~}},{{~ end ~}}
{{~ else ~}}
      {{ prop.property_name }}: {{ prop.parser_function }}(map.{{ prop.property_name }}){{~ if !for.last || has_children ~}},{{~ end ~}}
{{~ end ~}}
{{~ end ~}}
{{~ end ~}}
{{~ if has_children ~}}
{{~ if child_property_name ~}}
{{~ if child_is_nullable ~}}
      {{ child_property_name }}: DynamicWidgetBuilder.buildFromPointer(map.{{ child_property_name }}, buildContext)
{{~ else ~}}
      {{ child_property_name }}: DynamicWidgetBuilder.buildFromPointerNotNull(map.{{ child_property_name }}, buildContext)
{{~ end ~}}
{{~ else if children_property_name ~}}
      {{ children_property_name }}: DynamicWidgetBuilder.buildWidgets(map.{{ children_property_name }}, buildContext)
{{~ end ~}}
{{~ end ~}}
    );
  }

  @override
  String get widgetName => ""{{ widget_name }}"";
}
";
		}
	}
}
