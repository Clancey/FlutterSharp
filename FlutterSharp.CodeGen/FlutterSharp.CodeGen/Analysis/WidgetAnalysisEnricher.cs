using System;
using System.Collections.Generic;
using System.Linq;
using FlutterSharp.CodeGen.Models;
using FlutterSharp.CodeGen.TypeMapping;

namespace FlutterSharp.CodeGen.Analysis
{
	/// <summary>
	/// Enriches widget definitions with all information needed for code generation.
	/// This centralizes generation decisions so generators only need to format output.
	/// </summary>
	public class WidgetAnalysisEnricher
	{
		private readonly DartToCSharpMapper _dartToCSharpMapper;
		private readonly CSharpToDartFfiMapper _csharpToDartMapper;

		// Reserved field names from base structs
		private static readonly HashSet<string> ReservedFieldNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"handle", "managedHandle", "widgetType", "id"
		};

		public WidgetAnalysisEnricher(DartToCSharpMapper dartToCSharpMapper, CSharpToDartFfiMapper csharpToDartMapper)
		{
			_dartToCSharpMapper = dartToCSharpMapper ?? throw new ArgumentNullException(nameof(dartToCSharpMapper));
			_csharpToDartMapper = csharpToDartMapper ?? throw new ArgumentNullException(nameof(csharpToDartMapper));
		}

		/// <summary>
		/// Enriches a widget definition with all code generation metadata.
		/// </summary>
		public EnrichedWidgetDefinition Enrich(WidgetDefinition widget)
		{
			if (widget == null)
				throw new ArgumentNullException(nameof(widget));

			// Determine base class information
			var baseClassInfo = DetermineBaseClass(widget);

			// Filter and enrich properties
			var enrichedProperties = widget.Properties
				.Where(p => !ReservedFieldNames.Contains(p.Name))
				.Select(p => EnrichProperty(p))
				.ToList();

			// Add inherited child/children properties if needed but not present
			AddInheritedChildProperties(enrichedProperties, widget, baseClassInfo);

			// Separate into required and optional for constructor
			var requiredProperties = enrichedProperties.Where(p => p.IsRequired).ToList();
			var optionalProperties = enrichedProperties.Where(p => !p.IsRequired).ToList();

			// Determine which properties belong to base constructor vs derived constructor
			var (baseConstructorProperties, derivedConstructorProperties) =
				SeparateBaseAndDerivedProperties(enrichedProperties, baseClassInfo);

			// Determine HasSingleChild/HasMultipleChildren from base class if not already set
			var hasSingleChild = widget.HasSingleChild || IsSingleChildBaseClass(widget.BaseClass);
			var hasMultipleChildren = widget.HasMultipleChildren || IsMultiChildBaseClass(widget.BaseClass);

			return new EnrichedWidgetDefinition
			{
				// Original widget data
				Name = widget.Name,
				Namespace = widget.Namespace,
				Documentation = widget.Documentation,
				IsAbstract = widget.IsAbstract,
				IsDeprecated = widget.IsDeprecated,
				DeprecationMessage = widget.DeprecationMessage,
				TypeParameters = widget.TypeParameters ?? new List<string>(),

				// Base class information
				BaseClassName = baseClassInfo.CSharpBaseClass,
				DartBaseStruct = baseClassInfo.DartBaseStruct,
				BaseConstructorProperties = baseConstructorProperties,

				// Properties
				AllProperties = enrichedProperties,
				RequiredProperties = requiredProperties,
				OptionalProperties = optionalProperties,
				DerivedConstructorProperties = derivedConstructorProperties,

				// Struct naming
				StructName = $"{widget.Name}Struct",

				// Metadata - use computed values that consider base class
				HasSingleChild = hasSingleChild,
				HasMultipleChildren = hasMultipleChildren,
				ChildPropertyName = widget.ChildPropertyName ?? "child",
				ChildrenPropertyName = widget.ChildrenPropertyName ?? "children"
			};
		}

		/// <summary>
		/// Enriches a property with all type mapping and generation metadata.
		/// </summary>
		private EnrichedPropertyDefinition EnrichProperty(PropertyDefinition property)
		{
			// Map Dart type to C#
			var csharpType = !string.IsNullOrEmpty(property.CSharpType)
				? property.CSharpType
				: _dartToCSharpMapper.MapType(property.DartType);

			// Map C# type to Dart FFI type annotation (e.g., "Int32", "Double")
			var rawFfiType = _csharpToDartMapper.MapToFfiType(csharpType);
			var ffiAnnotation = GetFfiAnnotation(property, csharpType);

			// Convert FFI annotation types to actual Dart field types for struct declarations
			// In Dart FFI, struct fields use primitive types (int, double) with annotations
			// Pass the Dart type so we can determine typed pointers for known struct types
			var ffiType = ConvertFfiTypeToStructFieldType(rawFfiType, property.DartType);

			// Override FFI type for callbacks - they use Pointer<Utf8> for action string IDs
			// The action ID is dispatched to C# via method channel, not called as native function
			if (property.IsCallback)
			{
				ffiType = "Pointer<Utf8>";
				ffiAnnotation = ""; // No annotation needed for Pointer<Utf8>
			}

			// Determine backing field name
			var backingFieldName = $"_{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}";

			// Escape C# keywords
			var escapedName = EscapeCSharpKeyword(property.Name);
			var escapedBackingField = EscapeCSharpKeyword(backingFieldName);

			// Check if this is a generic type parameter
			var isGenericTypeParam = csharpType == "T" || csharpType == "T?" ||
			                         csharpType == "S" || csharpType == "S?";

			return new EnrichedPropertyDefinition
			{
				Name = escapedName,
				BackingFieldName = escapedBackingField,
				DartType = property.DartType,
				CSharpType = csharpType,
				FfiType = ffiType,
				FfiAnnotation = ffiAnnotation,
				IsRequired = property.IsRequired,
				IsNullable = property.IsNullable || ffiType.StartsWith("Pointer<"),
				DefaultValue = ConvertDartDefaultValueToCSharp(property.DefaultValue, csharpType),
				IsCallback = property.IsCallback,
				IsList = property.IsList,
				IsGenericTypeParam = isGenericTypeParam,
				Documentation = property.Documentation
			};
		}

		/// <summary>
		/// Determines base class information for the widget.
		/// </summary>
		private BaseClassInfo DetermineBaseClass(WidgetDefinition widget)
		{
			var dartBaseClass = widget.BaseClass;

			// Map Dart base class to C# base class
			var csharpBaseClass = MapDartBaseClassToCSharp(dartBaseClass, widget.Type);

			// Determine Dart base struct
			var dartBaseStruct = MapDartBaseClassToBaseStruct(dartBaseClass);

			return new BaseClassInfo
			{
				DartBaseClass = dartBaseClass,
				CSharpBaseClass = csharpBaseClass,
				DartBaseStruct = dartBaseStruct
			};
		}

		/// <summary>
		/// Maps Dart base class names to C# base class names.
		/// </summary>
		private string MapDartBaseClassToCSharp(string? dartBaseClass, WidgetType widgetType)
		{
			if (!string.IsNullOrEmpty(dartBaseClass))
			{
				// Direct mappings
				var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					["SingleChildRenderObjectWidget"] = "SingleChildRenderObjectWidget",
					["MultiChildRenderObjectWidget"] = "MultiChildRenderObjectWidget",
					["StatelessWidget"] = "StatelessWidget",
					["StatefulWidget"] = "StatefulWidget",
					["InheritedWidget"] = "InheritedWidget",
					["RenderObjectWidget"] = "RenderObjectWidget",
					["ProxyWidget"] = "ProxyWidget",
					["ParentDataWidget"] = "ParentDataWidget",
					["ImplicitlyAnimatedWidget"] = "ImplicitlyAnimatedWidget",
					["AnimatedWidget"] = "AnimatedWidget"
				};

				if (mappings.TryGetValue(dartBaseClass, out var mapped))
					return mapped;
			}

			// Fallback based on widget type
			return widgetType switch
			{
				WidgetType.SingleChildRenderObject => "SingleChildRenderObjectWidget",
				WidgetType.MultiChildRenderObject => "MultiChildRenderObjectWidget",
				WidgetType.Stateless => "StatelessWidget",
				WidgetType.Stateful => "StatefulWidget",
				_ => "Widget"
			};
		}

		/// <summary>
		/// Maps Dart base class to Dart FFI base struct name.
		/// </summary>
		private string MapDartBaseClassToBaseStruct(string? dartBaseClass)
		{
			if (string.IsNullOrEmpty(dartBaseClass))
				return "Struct";

			// Map to corresponding struct type
			var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["SingleChildRenderObjectWidget"] = "SingleChildRenderObjectWidgetStruct",
				["MultiChildRenderObjectWidget"] = "MultiChildRenderObjectWidgetStruct",
				["StatelessWidget"] = "StatelessWidgetStruct",
				["StatefulWidget"] = "StatefulWidgetStruct",
				["InheritedWidget"] = "InheritedWidgetStruct",
				["RenderObjectWidget"] = "RenderObjectWidgetStruct",
				["ProxyWidget"] = "ProxyWidgetStruct",
				["ParentDataWidget"] = "ParentDataWidgetStruct",
				["ImplicitlyAnimatedWidget"] = "ImplicitlyAnimatedWidgetStruct",
				["AnimatedWidget"] = "AnimatedWidgetStruct"
			};

			if (mappings.TryGetValue(dartBaseClass, out var mapped))
				return mapped;

			return "Struct";
		}

		/// <summary>
		/// Separates properties into those that belong to base constructor vs derived constructor.
		/// For now, all properties go to derived constructor. This can be enhanced later.
		/// </summary>
		private (List<EnrichedPropertyDefinition> baseProps, List<EnrichedPropertyDefinition> derivedProps)
			SeparateBaseAndDerivedProperties(List<EnrichedPropertyDefinition> properties, BaseClassInfo baseClassInfo)
		{
			// TODO: Implement logic to determine which properties belong to base class constructors
			// For now, all properties go to the derived class constructor
			return (new List<EnrichedPropertyDefinition>(), properties);
		}

		/// <summary>
		/// Checks if the base class is one that has a single child property.
		/// These are Flutter base classes that have a required child parameter in their constructor.
		/// </summary>
		private bool IsSingleChildBaseClass(string? baseClass)
		{
			if (string.IsNullOrEmpty(baseClass))
				return false;

			var singleChildBaseClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				// Render object widgets with single child
				"SingleChildRenderObjectWidget",
				// Proxy widgets (all have required child)
				"ProxyWidget",
				"InheritedWidget",
				"InheritedTheme",        // extends InheritedWidget
				"InheritedModel",        // extends InheritedWidget
				"InheritedNotifier",     // extends InheritedWidget
				"ParentDataWidget"
			};

			return singleChildBaseClasses.Contains(baseClass);
		}

		/// <summary>
		/// Checks if the base class is one that has a multiple children property.
		/// </summary>
		private bool IsMultiChildBaseClass(string? baseClass)
		{
			if (string.IsNullOrEmpty(baseClass))
				return false;

			var multiChildBaseClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"MultiChildRenderObjectWidget"
			};

			return multiChildBaseClasses.Contains(baseClass);
		}

		/// <summary>
		/// Adds inherited child/children properties if the widget inherits from a base class that has them
		/// but they're not already in the properties list.
		/// </summary>
		private void AddInheritedChildProperties(List<EnrichedPropertyDefinition> enrichedProperties, WidgetDefinition widget, BaseClassInfo baseClassInfo)
		{
			// Check if child property already exists
			var hasChildProperty = enrichedProperties.Any(p =>
				p.Name.Equals("child", StringComparison.OrdinalIgnoreCase));

			// Check if children property already exists
			var hasChildrenProperty = enrichedProperties.Any(p =>
				p.Name.Equals("children", StringComparison.OrdinalIgnoreCase));

			// Add child property if widget has single child and property doesn't exist
			if ((widget.HasSingleChild || IsSingleChildBaseClass(widget.BaseClass)) && !hasChildProperty)
			{
				var childPropertyName = widget.ChildPropertyName ?? "child";

				// Determine if child is required (it usually is for these base classes)
				var childIsRequired = IsSingleChildBaseClass(widget.BaseClass);

				enrichedProperties.Insert(0, new EnrichedPropertyDefinition
				{
					Name = childPropertyName,
					BackingFieldName = $"_{childPropertyName}",
					DartType = childIsRequired ? "Widget" : "Widget?",
					CSharpType = childIsRequired ? "Widget" : "Widget?",
					FfiType = "Pointer<WidgetStruct>",
					FfiAnnotation = "",
					IsRequired = childIsRequired,
					IsNullable = !childIsRequired,
					IsCallback = false,
					IsList = false,
					IsGenericTypeParam = false,
					Documentation = "The widget below this widget in the tree.\n\n{@macro flutter.widgets.ProxyWidget.child}"
				});
			}

			// Add children property if widget has multiple children and property doesn't exist
			if ((widget.HasMultipleChildren || IsMultiChildBaseClass(widget.BaseClass)) && !hasChildrenProperty)
			{
				var childrenPropertyName = widget.ChildrenPropertyName ?? "children";

				enrichedProperties.Insert(0, new EnrichedPropertyDefinition
				{
					Name = childrenPropertyName,
					BackingFieldName = $"_{childrenPropertyName}",
					DartType = "List<Widget>",
					CSharpType = "List<Widget>",
					FfiType = "Pointer<ChildrenStruct>",
					FfiAnnotation = "",
					IsRequired = true,
					IsNullable = false,
					IsCallback = false,
					IsList = true,
					IsGenericTypeParam = false,
					Documentation = "The widgets below this widget in the tree.\n\nIf this list is going to be mutated, it is usually wise to put a [Key] on each of the child widgets, so that the framework can match old configurations to new configurations and maintain the underlying render objects."
				});
			}
		}

		/// <summary>
		/// Converts Dart default values to C# syntax.
		/// </summary>
		private string? ConvertDartDefaultValueToCSharp(string? dartDefaultValue, string csharpType)
		{
			if (string.IsNullOrEmpty(dartDefaultValue))
				return null;

			// Handle Dart collection literals
			if (dartDefaultValue.Contains("<") && (dartDefaultValue.Contains("{}") || dartDefaultValue.Contains("[]")))
				return "null";

			// Handle Dart const Duration() - not compile-time constant in C#
			if (dartDefaultValue.Contains("Duration("))
				return "null";

			// Handle Dart const EdgeInsets
			if (dartDefaultValue.Contains("EdgeInsets"))
				return "null";

			// Handle Dart enums (e.g., Axis.horizontal)
			if (dartDefaultValue.Contains(".") && !dartDefaultValue.StartsWith("const"))
				return dartDefaultValue;

			// Handle common literals
			dartDefaultValue = dartDefaultValue
				.Replace("const ", "")
				.Replace("true", "true")
				.Replace("false", "false")
				.Replace("null", "null");

			// If it's a complex const constructor, return null
			if (dartDefaultValue.Contains("(") && dartDefaultValue.Contains(")"))
				return "null";

			return dartDefaultValue;
		}

		/// <summary>
		/// Escapes C# keywords by prefixing with @.
		/// </summary>
		private string EscapeCSharpKeyword(string name)
		{
			var keywords = new HashSet<string>
			{
				"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
				"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
				"enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
				"goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
				"namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
				"protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
				"stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try",
				"typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
				"volatile", "while"
			};

			return keywords.Contains(name.ToLowerInvariant()) ? $"@{name}" : name;
		}

		/// <summary>
		/// Converts FFI type names (Int8, Int32, Double, etc.) to Dart struct field types (int, double).
		/// In Dart FFI structs, fields use primitive types with annotations, not FFI type names directly.
		/// </summary>
		private string ConvertFfiTypeToStructFieldType(string ffiType, string? dartType = null)
		{
			var result = ffiType switch
			{
				// Integer types -> int
				"Int8" or "Int16" or "Int32" or "Int64" => "int",
				"Uint8" or "Uint16" or "Uint32" or "Uint64" => "int",

				// Floating point types -> double
				"Float" or "Double" => "double",

				// Boolean -> bool (but note: Dart FFI uses Int8 for bools with annotation)
				"Bool" => "int", // Actually represented as Int8 in FFI

				// IntPtr -> Pointer<Void>
				"IntPtr" => "Pointer<Void>",

				// Pointers and other types pass through unchanged
				_ => ffiType
			};

			// If the result is Pointer<Void>, check if this type should be a typed pointer to its own struct
			if (result == "Pointer<Void>" && !string.IsNullOrWhiteSpace(dartType))
			{
				var typedPointer = GetTypedPointerForDartType(dartType);
				if (typedPointer != null)
				{
					return typedPointer;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets a typed pointer type for known Dart types that should have struct representations.
		/// </summary>
		private string? GetTypedPointerForDartType(string dartType)
		{
			// Remove nullable marker and generic parameters for matching
			var baseType = dartType.TrimEnd('?').Split('<')[0].Trim();

			// Check if this type should have its own struct
			// These are Flutter/Dart types that have their own struct definitions
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
				return $"Pointer<{baseType}Struct>";
			}

			return null;
		}

		/// <summary>
		/// Gets the FFI annotation for a property (e.g., "@Int32()", "@Double()").
		/// </summary>
		private string? GetFfiAnnotation(PropertyDefinition property, string csharpType)
		{
			// Get the raw FFI type to determine annotation
			var rawFfiType = _csharpToDartMapper.MapToFfiType(csharpType);

			// Map raw FFI types to their annotations
			return rawFfiType switch
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
				"Bool" => "@Int8()", // Bool is represented as Int8 in FFI
				_ => null // Pointers and other types don't need annotations
			};
		}
	}

	/// <summary>
	/// Enriched widget definition with all generation metadata.
	/// </summary>
	public class EnrichedWidgetDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;
		public string? Documentation { get; set; }
		public bool IsAbstract { get; set; }
		public bool IsDeprecated { get; set; }
		public string? DeprecationMessage { get; set; }
		public List<string> TypeParameters { get; set; } = new();

		public string BaseClassName { get; set; } = "Widget";
		public string DartBaseStruct { get; set; } = "Struct";
		public List<EnrichedPropertyDefinition> BaseConstructorProperties { get; set; } = new();

		public List<EnrichedPropertyDefinition> AllProperties { get; set; } = new();
		public List<EnrichedPropertyDefinition> RequiredProperties { get; set; } = new();
		public List<EnrichedPropertyDefinition> OptionalProperties { get; set; } = new();
		public List<EnrichedPropertyDefinition> DerivedConstructorProperties { get; set; } = new();

		public string StructName { get; set; } = string.Empty;

		public bool HasSingleChild { get; set; }
		public bool HasMultipleChildren { get; set; }
		public string ChildPropertyName { get; set; } = "child";
		public string ChildrenPropertyName { get; set; } = "children";
	}

	/// <summary>
	/// Enriched property definition with all generation metadata.
	/// </summary>
	public class EnrichedPropertyDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string BackingFieldName { get; set; } = string.Empty;
		public string DartType { get; set; } = string.Empty;
		public string CSharpType { get; set; } = string.Empty;
		public string FfiType { get; set; } = string.Empty;
		public string? FfiAnnotation { get; set; }
		public bool IsRequired { get; set; }
		public bool IsNullable { get; set; }
		public string? DefaultValue { get; set; }
		public bool IsCallback { get; set; }
		public bool IsList { get; set; }
		public bool IsGenericTypeParam { get; set; }
		public string? Documentation { get; set; }
	}

	/// <summary>
	/// Base class information for a widget.
	/// </summary>
	public class BaseClassInfo
	{
		public string? DartBaseClass { get; set; }
		public string CSharpBaseClass { get; set; } = "Widget";
		public string DartBaseStruct { get; set; } = "Struct";
	}
}
