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
	/// Generates Dart FFI struct definitions from widget definitions.
	/// </summary>
	public class DartStructGenerator
	{
		private readonly CSharpToDartFfiMapper _typeMapper;
		private readonly Template _template;

		/// <summary>
		/// Initializes a new instance of the <see cref="DartStructGenerator"/> class.
		/// </summary>
		/// <param name="typeMapper">The type mapper to use for converting C# types to Dart FFI types.</param>
		public DartStructGenerator(CSharpToDartFfiMapper typeMapper)
		{
			_typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));

			// Load the Scriban template
			var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "DartStruct.scriban");
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
		/// Generates a Dart FFI struct definition from a widget definition.
		/// </summary>
		/// <param name="widget">The widget definition to generate the struct from.</param>
		/// <returns>The generated Dart FFI struct code.</returns>
	public string Generate(WidgetDefinition widget)
	{
		if (widget == null)
		{
			throw new ArgumentNullException(nameof(widget));
		}

		// Prepare the model for the template
		// Filter out base struct fields to avoid duplicates
		var reservedFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"handle", "managedHandle", "widgetType", "id"
		};

		var model = new
		{
			widget.Name,
			StructName = $"{widget.Name}Struct",
			BaseStruct = GetBaseStruct(widget),
			Properties = widget.Properties
				.Where(p => !reservedFieldNames.Contains(p.Name))
				.Select(p => new
				{
					p.Name,
					PropertyName = p.IsCallback ? ToCamelCase(p.Name) + "Action" : ToCamelCase(p.Name),
					DartType = p.DartType,
					CSharpType = p.CSharpType ?? "object",
					FfiType = GetFfiType(p),
					FfiAnnotation = GetFfiAnnotation(p),
					IsNullable = p.IsNullable || GetFfiType(p).StartsWith("Pointer<"),
					Documentation = FormatDocumentation(p.Documentation, "  ")
				}).ToList(),
				Documentation = FormatDocumentation(widget.Documentation),
				GeneratedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
				Imports = GetRequiredImports(widget)
			};


		// DEBUG: Log the first few properties to see what FfiType values are being passed
		if (widget.Name == "AnimatedContainer")
		{
			foreach (var prop in model.Properties.Take(3))
			{
				Console.WriteLine($"[DEBUG MODEL] Property '{prop.Name}': DartType='{prop.DartType}', FfiType='{prop.FfiType}'");
			}
		}
			// Render the template
			var result = _template.Render(model);
			return result;
		}

	/// <summary>
	/// Generates a Dart FFI struct definition from an enriched widget definition (new architecture).
	/// </summary>
	/// <param name="enrichedWidget">The enriched widget definition to generate the struct from.</param>
	/// <returns>The generated Dart FFI struct code.</returns>
	public string Generate(EnrichedWidgetDefinition enrichedWidget)
	{
		if (enrichedWidget == null)
		{
			throw new ArgumentNullException(nameof(enrichedWidget));
		}

		// Build model from enriched data
		var model = new
		{
			enrichedWidget.Name,
			StructName = enrichedWidget.StructName,
			BaseStruct = enrichedWidget.DartBaseStruct,
			Properties = enrichedWidget.AllProperties.Select(p => new
			{
				p.Name,
				PropertyName = p.IsCallback ? ToCamelCase(p.Name) + "Action" : ToCamelCase(p.Name),
				p.DartType,
				CSharpType = p.CSharpType,
				FfiType = p.FfiType,
				FfiAnnotation = p.FfiAnnotation ?? "",
				p.IsNullable,
				Documentation = FormatDocumentation(p.Documentation, "  ")
			}).ToList(),
			Documentation = FormatDocumentation(enrichedWidget.Documentation),
			GeneratedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
			Imports = GetRequiredImportsFromEnriched(enrichedWidget)
		};

		// Render the template
		var result = _template.Render(model);
		return result;
	}

		/// <summary>
		/// Gets the base struct class name for the widget.
		/// </summary>
	/// <summary>
	/// Gets the base struct class name for the widget.
	/// </summary>
	private string GetBaseStruct(WidgetDefinition widget)
	{
		if (widget.BaseClass != null)
		{
		// Known abstract base classes and Dart base types that won't have struct files generated
		var abstractBaseClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"Object",  // Dart base class - doesn't have a struct
			"AlignmentGeometry",  // Abstract geometry classes
			"BorderRadiusGeometry",
			"EdgeInsetsGeometry",
			"Gradient",  // Abstract gradient base class
			"Animation",  // Abstract animation base class
			"Decoration",  // Abstract decoration base class
			"Constraints",  // Abstract constraints base class
			"BoxBorder",  // Abstract box border base class
			"ParametricCurve",  // Abstract parametric curve base class
			"StatelessWidget",
			"StatefulWidget",
			"RenderObjectWidget",
			"ProxyWidget",
			"ParentDataWidget",
			"InheritedWidget",
			"LeafRenderObjectWidget",
			"SingleChildRenderObjectWidget",
			"MultiChildRenderObjectWidget",
			"SlottedRenderObjectWidget",
			"AnimatedWidget",
			"BoxScrollView",
			"ConstrainedLayoutBuilder",
			"ImplicitlyAnimatedWidget",
			"InheritedModel",
			"InheritedNotifier",
			"InheritedTheme",
			"ScrollView",
			"SliverMultiBoxAdaptorWidget",
			"StreamBuilderBase",
			"StatusTransitionWidget",
			"TwoDimensionalScrollView",
			"TwoDimensionalViewport",
			"UniqueWidget"
		};

			if (abstractBaseClasses.Contains(widget.BaseClass))
			{
				// Use WidgetStruct for abstract base classes
				return "WidgetStruct";
			}

			return $"{widget.BaseClass}Struct";
		}

		// Default base struct
		return "WidgetStruct";
	}

		/// <summary>
		/// Gets the FFI type for a property.
		/// </summary>
	private string GetFfiType(PropertyDefinition property)
	{
		// Handle callbacks - they use Pointer<Utf8> for action strings
		if (property.IsCallback)
		{
			return "Pointer<Utf8>";
		}

		// Use DartType as the source of truth
		var dartType = property.DartType;

		// Skip InvalidType - it means the Dart analyzer couldn't determine the type
		if (string.IsNullOrEmpty(dartType) || dartType.Equals("InvalidType", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine($"[WARNING] Property '{property.Name}' has invalid or missing type information. Using Pointer<Void> as fallback.");
			return "Pointer<Void>";
		}

		// Check for List<Widget> or similar collection types
		// These should map to Pointer<ChildrenStruct>
		if (dartType.StartsWith("List<", StringComparison.OrdinalIgnoreCase) && dartType.Contains("Widget"))
		{
			Console.WriteLine($"[DEBUG GetFfiType] List<Widget> detected: {dartType} -> Pointer<ChildrenStruct>");
			return "Pointer<ChildrenStruct>";
		}

		// If it's an enum, return int (will be annotated with @Int32() by GetFfiAnnotation)
		if (_typeMapper.IsEnum(dartType))
		{
			Console.WriteLine($"[DEBUG GetFfiType] Enum detected: {dartType} -> int (will use @Int32() annotation)");
			return "int";
		}

		// Map the Dart type to FFI type
		var structType = _typeMapper.MapDartTypeToFfiType(dartType);

		// DEBUG: Log for alignment property
		if (property.Name?.Equals("Alignment", StringComparison.OrdinalIgnoreCase) == true)
		{
			Console.WriteLine($"[DEBUG] Alignment property: DartType='{property.DartType}', structType='{structType}'");
		}

		// Convert FFI struct types to Dart types for struct fields
		// Annotations will specify the native representation
		var result = structType switch
		{
			"Int8" or "Int16" or "Int32" or "Int64" => "int",
			"Uint8" or "Uint16" or "Uint32" or "Uint64" => "int",
			"Float" or "Double" => "double",
			"Bool" => "bool",
			"IntPtr" => "Pointer<Void>",
			_ => structType
		};

		// If the result is Pointer<Void>, check if this type should be a typed pointer to its own struct
		if (result == "Pointer<Void>" && !string.IsNullOrWhiteSpace(dartType))
		{
			// Remove nullable marker and generic parameters for matching
			var baseType = dartType.TrimEnd('?').Split('<')[0].Trim();

			// Check if this type should have its own struct
			// These are Flutter/Dart types that have their own struct definitions
			// NOTE: Abstract types (AlignmentGeometry, EdgeInsetsGeometry, BorderRadiusGeometry, Gradient,
			// Animation, Decoration, Constraints, BoxBorder, ParametricCurve) will have interface-only struct files generated
			var knownStructTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Widget", "Alignment", "AlignmentGeometry", "EdgeInsets", "EdgeInsetsGeometry",
				"BorderRadius", "BorderRadiusGeometry", "BoxDecoration", "Decoration",
				"BoxConstraints", "Constraints", "TextStyle", "Gradient", "ImageProvider", "Size", "Offset",
				"Rect", "Radius", "Matrix4", "Key", "BuildContext", "ThemeData", "IconData",
				"Color", "MaterialColor", "MaterialStateProperty", "Duration", "DateTime",
				"Animation", "AnimationController", "Curve", "ParametricCurve", "FocusNode", "ScrollController",
				"ScrollPhysics", "TextEditingController", "GlobalKey", "State", "BoxBorder", "Border"
			};

			if (knownStructTypes.Contains(baseType))
			{
				Console.WriteLine($"[DEBUG GetFfiType] MATCH! Converting {dartType} (base: {baseType}) to Pointer<{baseType}Struct>");
				return $"Pointer<{baseType}Struct>";
			}
			else
			{
				Console.WriteLine($"[DEBUG GetFfiType] NO MATCH for {dartType} (base: {baseType})");
			}
		}

		return result;
	}

	/// <summary>
	/// Gets the FFI annotation for a property (e.g., @Int32(), @Double(), etc.).
	/// </summary>
	private string GetFfiAnnotation(PropertyDefinition property)
	{
		// Handle callbacks - Pointer<Utf8> doesn't need annotation
		if (property.IsCallback)
		{
			return "";
		}

		// Get the actual FFI type from the mapper using the Dart type
		var dartType = property.DartType;

		// Skip InvalidType
		if (string.IsNullOrEmpty(dartType) || dartType.Equals("InvalidType", StringComparison.OrdinalIgnoreCase))
		{
			return "";
		}

		var ffiType = _typeMapper.MapDartTypeToFfiType(dartType);

		// Map FFI types to their annotations
		return ffiType switch
		{
			"Int8" => "@Int8()",
			"Int16" => "@Int16()",
			"Int32" => "@Int32()",
			"Int64" => "@Int64()",
			"Uint8" => "@Uint8()",
			"Uint16" => "@Uint16()",
			"Uint32" => "@Uint32()",
			"Uint64" => "@Uint64()",
			"Float" => "@Float()",
			"Double" => "@Double()",
			"Bool" => "@Bool()",
			_ when ffiType.StartsWith("Pointer<") => "", // Pointers don't need annotations
			_ => ""
		};
	}

		/// <summary>
		/// Gets the required imports for the generated Dart struct.
		/// </summary>
		private List<string> GetRequiredImports(WidgetDefinition widget)
		{
			var imports = new HashSet<string>
			{
				"dart:ffi",
				"package:ffi/ffi.dart"
			};

			// Add imports based on property types
			foreach (var property in widget.Properties)
			{
				var ffiType = GetFfiType(property);

				// Add specific imports based on types
				if (ffiType.Contains("Utf8"))
				{
					imports.Add("package:ffi/ffi.dart");
				}

				// Check if property uses a Pointer to another struct type
				// Pattern: Pointer<SomeTypeStruct>
				if (ffiType.StartsWith("Pointer<") && ffiType.EndsWith("Struct>"))
				{
					// Extract the struct type name
					var structTypeName = ffiType.Substring(8, ffiType.Length - 9); // Remove "Pointer<" and ">"

					// Don't add import for base types or WidgetStruct (it's in the base file)
					// Object is a Dart base class that doesn't have a struct
					if (structTypeName != "WidgetStruct" && structTypeName != "Void" && structTypeName != "ObjectStruct" && structTypeName != "ChildrenStruct")
					{
						// Convert SomeTypeStruct -> sometype_struct.dart
						var typeName = structTypeName.Replace("Struct", "");
						var importFileName = typeName.ToLowerInvariant();
						imports.Add($"{importFileName}_struct.dart");
					}
				}
			}

			return imports.OrderBy(i => i).ToList();
		}

	/// <summary>
	/// Gets the required imports for the generated Dart struct from enriched widget data.
	/// </summary>
	private List<string> GetRequiredImportsFromEnriched(EnrichedWidgetDefinition widget)
	{
		var imports = new HashSet<string>
		{
			"dart:ffi",
			"package:ffi/ffi.dart"
		};

		// Add imports based on property types
		foreach (var property in widget.AllProperties)
		{
			var ffiType = property.FfiType;

			// Add specific imports based on types
			if (ffiType.Contains("Utf8"))
			{
				imports.Add("package:ffi/ffi.dart");
			}

			// Check if property uses a Pointer to another struct type
			// Pattern: Pointer<SomeTypeStruct>
			if (ffiType.StartsWith("Pointer<") && ffiType.EndsWith("Struct>"))
			{
				// Extract the struct type name
				var structTypeName = ffiType.Substring(8, ffiType.Length - 9); // Remove "Pointer<" and ">"

				// Don't add import for base types or WidgetStruct (it's in the base file)
				// Object is a Dart base class that doesn't have a struct
				if (structTypeName != "WidgetStruct" && structTypeName != "Void" && structTypeName != "ObjectStruct" && structTypeName != "ChildrenStruct")
				{
					// Convert SomeTypeStruct -> sometype_struct.dart
					var typeName = structTypeName.Replace("Struct", "");
					var importFileName = typeName.ToLowerInvariant();
					imports.Add($"{importFileName}_struct.dart");
				}
			}
		}

		return imports.OrderBy(i => i).ToList();
	}

	/// <summary>
	/// Formats documentation for Dart comments by prefixing each line with ///.
	/// </summary>
	private string FormatDocumentation(string documentation, string indent = "")
	{
		if (string.IsNullOrWhiteSpace(documentation))
		{
			return null;
		}

		var lines = documentation.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		var formattedLines = lines.Select(line =>
		{
			var trimmed = line.TrimStart();
			return string.IsNullOrWhiteSpace(trimmed) ? $"{indent}///" : $"{indent}/// {trimmed}";
		});
		return string.Join("\n", formattedLines);
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
		/// Gets the default template if the template file is not found.
		/// </summary>
		private string GetDefaultTemplate()
		{
			return @"// <auto-generated>
// This file was automatically generated on {{ generated_date }}
// DO NOT EDIT - Changes will be overwritten
// </auto-generated>

{{~ if imports
for import in imports ~}}
import '{{ import }}';
{{~ end
end ~}}

{{~ if documentation ~}}
/// {{ documentation }}
{{~ end ~}}
class {{ struct_name }} extends {{ base_struct }} {
{{~ for prop in properties ~}}
{{~ if prop.documentation ~}}
  /// {{ prop.documentation }}
{{~ end ~}}
  {{ prop.ffi_annotation }}
  external {{ prop.ffi_type }} {{ prop.property_name }};
{{~ end ~}}
}
";
		}
	}
}
