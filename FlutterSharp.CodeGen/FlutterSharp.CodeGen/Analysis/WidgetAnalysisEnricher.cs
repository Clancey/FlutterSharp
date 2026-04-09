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
		private readonly Action<string>? _logWarning;
		private readonly HashSet<string> _dartStructTypes;

		// Reserved field names from base structs
		private static readonly HashSet<string> ReservedFieldNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"handle", "managedHandle", "widgetType", "id"
		};

		// Debug-only parameter names that should be excluded from the public API
		private static readonly HashSet<string> DebugParameterNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"debugTypicalAncestorWidgetClass",
			"debugLabel",
			"debugOwnerBuildScopeNotified"
		};

		// Complex C# types that cannot be marshaled via FFI and should be optional
		// These are reference types or complex structs that require special marshaling
		private static readonly HashSet<string> ComplexUnmarshalableTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			// Flutter geometry types
			"AlignmentGeometry", "Alignment", "AlignmentDirectional",
			"EdgeInsetsGeometry", "EdgeInsets", "EdgeInsetsDirectional",
			"BorderRadiusGeometry", "BorderRadius", "BorderRadiusDirectional",
			"BoxConstraints", "Constraints",
			"Matrix4",
			// Flutter painting types
			"Decoration", "BoxDecoration", "ShapeDecoration",
			"ImageProvider", "ImageProvider<Object>",
			"Color", "Gradient", "LinearGradient", "RadialGradient", "SweepGradient",
			"Shadow", "BoxShadow",
			"Border", "BoxBorder", "ShapeBorder", "OutlinedBorder",
			"TextStyle", "StrutStyle",
			// Flutter animation types
			"Curve", "ParametricCurve", "Curves",
			"Animation", "Animation<T>", "Listenable",
			"AnimationController", "AnimationBehavior",
			// Flutter foundation types
			"Key", "GlobalKey", "LocalKey", "ValueKey", "ObjectKey", "UniqueKey",
			// Flutter widget types
			"BuildContext", "State",
			// Flutter rendering types
			"CustomClipper", "CustomClipper<T>", "CustomClipper<Rect>", "CustomClipper<Path>",
			"CustomPainter", "LayerLink", "Ticker", "TickerProvider",
			// Flutter services types
			"ImageFilter", "ImageFilter<T>", "ColorFilter",
			"AssetBundle", "Locale", "LocalizationsDelegate",
			// Controller types
			"ScrollController", "TextEditingController", "FocusNode", "FocusScopeNode",
			"PageController", "TabController", "AnimationController",
			// Delegate types
			"SliverChildDelegate", "SliverChildBuilderDelegate", "SliverChildListDelegate",
			"SliverGridDelegate", "GridDelegate", "TableColumnWidth",
			// Other complex types
			"ThemeData", "IconThemeData", "TextTheme",
			"MaterialStateProperty", "MaterialStateProperty<T>",
			"ValueListenable", "ValueNotifier",
			"SelectionRegistrar", "SelectionControls",
			"RouteSettings", "Route", "Navigator",
			// System.Type is also not marshalable
			"Type", "System.Type"
		};

		public WidgetAnalysisEnricher(
			DartToCSharpMapper dartToCSharpMapper,
			CSharpToDartFfiMapper csharpToDartMapper,
			Action<string>? logWarning = null,
			IEnumerable<string>? dartStructTypes = null)
		{
			_dartToCSharpMapper = dartToCSharpMapper ?? throw new ArgumentNullException(nameof(dartToCSharpMapper));
			_csharpToDartMapper = csharpToDartMapper ?? throw new ArgumentNullException(nameof(csharpToDartMapper));
			_logWarning = logWarning;
			_dartStructTypes = new HashSet<string>(dartStructTypes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase)
			{
				"Widget"
			};
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

			// Filter and enrich properties (pass widget name for context-aware type mapping)
			// Also filter out debug-only parameters
			var enrichedProperties = widget.Properties
				.Where(p => !ReservedFieldNames.Contains(p.Name))
				.Where(p => !DebugParameterNames.Contains(p.Name))
				.Select(p => EnrichProperty(p, widget.Name))
				.ToList();

			// Add inherited child/children properties if needed but not present
			AddInheritedChildProperties(enrichedProperties, widget, baseClassInfo);

			// Add inherited properties from base classes (duration, curve, animation, etc.)
			AddInheritedBaseClassProperties(enrichedProperties, widget.BaseClass);

			// Update IsRequired flag for properties that should be optional
			// This is important because the flag is used later during code generation
			foreach (var p in enrichedProperties)
			{
				if (p.IsRequired && ShouldBeOptionalParameter(p))
				{
					p.IsRequired = false;
				}
			}

			// Separate into required and optional for constructor
			// Properties should be optional if:
			// - Complex types that can't be marshaled
			// - Enum types (can use defaults)
			// - Types with known default values
			// - Dart nullable types
			var requiredProperties = enrichedProperties
				.Where(p => p.IsRequired)
				.ToList();
			var optionalProperties = enrichedProperties
				.Where(p => !p.IsRequired)
				.ToList();

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
				SourceLibrary = widget.SourceLibrary,
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
		/// <param name="property">The property definition to enrich.</param>
		/// <param name="widgetName">The widget name for context-aware type mapping of ambiguous parameters.</param>
		private EnrichedPropertyDefinition EnrichProperty(PropertyDefinition property, string widgetName)
		{
			// Map Dart type to C#
			// Pass property name and widget name for context-aware type inference when DartType is "InvalidType"
			var csharpType = !string.IsNullOrEmpty(property.CSharpType)
				? property.CSharpType
				: _dartToCSharpMapper.MapType(property.DartType, property.Name, widgetName);

			// Check if the Dart type has a mapping and log warning if not
			if (!_dartToCSharpMapper.HasMapping(property.DartType))
			{
				_logWarning?.Invoke($"Dart type '{property.DartType}' in {widgetName}.{property.Name} has no mapping");
			}

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

			// Determine constructor parameter name (camelCase, no underscore prefix)
			// This was previously a backing field name with underscore, but we now use clean parameter names
			var parameterName = $"{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}";

			// Escape C# keywords with @ prefix
			var escapedName = EscapeCSharpKeyword(property.Name);
			var escapedParameter = EscapeCSharpKeyword(parameterName);

			// Check if this is a generic type parameter
			var isGenericTypeParam = csharpType == "T" || csharpType == "T?" ||
			                         csharpType == "S" || csharpType == "S?";

			// Check if this is an enum type
			var isEnum = _dartToCSharpMapper.IsEnum(property.DartType);

			// Track the original Dart nullability for both struct and parser generation
			// CRITICAL FIX (D004): Both C# and Dart structs must use the same IsNullable logic
			// to ensure FFI field layouts match exactly. Pointer types do NOT automatically get "has" flags.
			var originalIsNullable = property.IsNullable;

			return new EnrichedPropertyDefinition
			{
				Name = escapedName,
				BackingFieldName = escapedParameter,
				DartType = property.DartType,
				CSharpType = csharpType,
				FfiType = ffiType,
				FfiAnnotation = ffiAnnotation,
				IsRequired = property.IsRequired,
				// Use original nullability for struct generation - must match C# struct layout
				IsNullable = originalIsNullable,
				// Track original Flutter nullability separately for parser generation
				IsDartNullable = originalIsNullable,
				DefaultValue = ConvertDartDefaultValueToCSharp(property.DefaultValue, csharpType),
				RawDefaultValue = property.DefaultValue,
				IsCallback = property.IsCallback,
				IsList = property.IsList,
				IsGenericTypeParam = isGenericTypeParam,
				IsEnum = isEnum,
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
			var dartBaseStruct = MapDartBaseClassToBaseStruct(
				dartBaseClass,
				widget.Type,
				widget.HasSingleChild,
				widget.HasMultipleChildren);

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
				// Direct mappings for base classes that exist in both Dart and C#
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
					["AnimatedWidget"] = "AnimatedWidget",
					// Intermediate base classes that should map to their parent base class
					["Flexible"] = "ParentDataWidget",           // Flexible extends ParentDataWidget
					["Positioned"] = "ParentDataWidget",         // Positioned extends ParentDataWidget
					["PositionedDirectional"] = "ParentDataWidget", // PositionedDirectional extends ParentDataWidget
					["LayoutId"] = "ParentDataWidget",           // LayoutId extends ParentDataWidget
					["InheritedTheme"] = "InheritedWidget",      // InheritedTheme extends InheritedWidget
					["ScrollView"] = "StatelessWidget",          // ScrollView extends StatelessWidget
					["BoxScrollView"] = "ScrollView"             // BoxScrollView extends ScrollView
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
		private string MapDartBaseClassToBaseStruct(
			string? dartBaseClass,
			WidgetType widgetType,
			bool hasSingleChild,
			bool hasMultipleChildren)
		{
			if (!string.IsNullOrEmpty(dartBaseClass))
			{
				// Any widget base struct needs the shared widget header fields to be emitted.
				var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					["SingleChildRenderObjectWidget"] = "SingleChildRenderObjectWidgetStruct",
					["MultiChildRenderObjectWidget"] = "MultiChildRenderObjectWidgetStruct",
					["StatelessWidget"] = "WidgetStruct",
					["StatefulWidget"] = "WidgetStruct",
					["InheritedWidget"] = "WidgetStruct",
					["RenderObjectWidget"] = "WidgetStruct",
					["ProxyWidget"] = "WidgetStruct",
					["ParentDataWidget"] = "WidgetStruct",
					["ImplicitlyAnimatedWidget"] = "WidgetStruct",
					["AnimatedWidget"] = "WidgetStruct",
					["Flexible"] = "WidgetStruct",
					["Positioned"] = "WidgetStruct",
					["PositionedDirectional"] = "WidgetStruct",
					["LayoutId"] = "WidgetStruct",
					["InheritedTheme"] = "WidgetStruct",
					["ScrollView"] = "WidgetStruct",
					["BoxScrollView"] = "WidgetStruct"
				};

				if (mappings.TryGetValue(dartBaseClass, out var mapped))
					return mapped;
			}

			if (hasMultipleChildren || widgetType == WidgetType.MultiChildRenderObject)
				return "MultiChildRenderObjectWidgetStruct";

			if (hasSingleChild || widgetType == WidgetType.SingleChildRenderObject)
				return "SingleChildRenderObjectWidgetStruct";

			return "WidgetStruct";
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
				"ParentDataWidget",
				// Intermediate classes that extend single-child base classes
				"Flexible",              // extends ParentDataWidget, has required child
				"Positioned",            // extends ParentDataWidget, has required child
				"PositionedDirectional", // extends ParentDataWidget, has required child
				"LayoutId"               // extends ParentDataWidget, has required child
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
		/// Gets inherited properties for a given base class.
		/// These are properties defined in the base class that subclasses must also pass.
		/// </summary>
		private List<EnrichedPropertyDefinition> GetInheritedBaseClassProperties(string? baseClass)
		{
			if (string.IsNullOrEmpty(baseClass))
				return new List<EnrichedPropertyDefinition>();

			var properties = new List<EnrichedPropertyDefinition>();

			// Properties inherited from ImplicitlyAnimatedWidget
			// Required: duration
			// Optional: curve, onEnd
			if (baseClass.Equals("ImplicitlyAnimatedWidget", StringComparison.OrdinalIgnoreCase))
			{
				// Duration is stored as microseconds (Int64) for simple FFI marshaling
				// parseDurationMicroseconds will convert to Duration in Dart
				properties.Add(new EnrichedPropertyDefinition
				{
					Name = "duration",
					BackingFieldName = "duration",
					DartType = "Duration",
					CSharpType = "long", // Microseconds as Int64
					FfiType = "int",
					FfiAnnotation = "@Int64()",
					IsRequired = true,
					IsNullable = false,
					IsCallback = false,
					IsList = false,
					IsGenericTypeParam = false,
					Documentation = "The duration over which to animate the parameters of this container (in microseconds)."
				});

				// Curve is optional - we skip it for now as it requires complex marshaling
				// The generated code will use the default Curves.linear

				properties.Add(new EnrichedPropertyDefinition
				{
					Name = "onEnd",
					BackingFieldName = "onEnd",
					DartType = "VoidCallback?",
					CSharpType = "Action?",
					FfiType = "Pointer<Utf8>",
					FfiAnnotation = "",
					IsRequired = false,
					IsNullable = true,
					IsCallback = true,
					IsList = false,
					IsGenericTypeParam = false,
					Documentation = "Called every time an animation completes."
				});
			}

			// Note: AnimatedWidget has a `listenable` getter, NOT a constructor parameter
			// Subclasses like FadeTransition, ScaleTransition, etc. have their own
			// animation-related parameters (animation, scale, turns, etc.)
			// Do NOT add listenable as an inherited property - it's an internal getter

			return properties;
		}

		/// <summary>
		/// Adds inherited properties from base classes if they're not already present.
		/// </summary>
		private void AddInheritedBaseClassProperties(List<EnrichedPropertyDefinition> enrichedProperties, string? baseClass)
		{
			var inheritedProperties = GetInheritedBaseClassProperties(baseClass);

			foreach (var inheritedProp in inheritedProperties)
			{
				// Check if already present
				var existingProp = enrichedProperties.FirstOrDefault(p => p.Name.Equals(inheritedProp.Name, StringComparison.OrdinalIgnoreCase));
				if (existingProp == null)
				{
					// Add if not present
					enrichedProperties.Add(inheritedProp);
				}
				else if (inheritedProp.IsCallback && !existingProp.IsCallback)
				{
					// Replace with inherited version if it has correct callback flag
					// The analyzer may not correctly identify callback typedefs like VoidCallback
					enrichedProperties.Remove(existingProp);
					enrichedProperties.Add(inheritedProp);
				}
			}
		}

		/// <summary>
		/// Adds inherited child/children properties if the widget inherits from a base class that has them
		/// but they're not already in the properties list.
		/// </summary>
		private void AddInheritedChildProperties(List<EnrichedPropertyDefinition> enrichedProperties, WidgetDefinition widget, BaseClassInfo baseClassInfo)
		{
			// Get the actual child/children property names for this widget
			// Some widgets use different names (e.g., "sliver" instead of "child", "slivers" instead of "children")
			var childPropertyName = widget.ChildPropertyName ?? "child";
			var childrenPropertyName = widget.ChildrenPropertyName ?? "children";

			// Check if child property already exists (using the widget's actual child property name)
			var hasChildProperty = enrichedProperties.Any(p =>
				p.Name.Equals("child", StringComparison.OrdinalIgnoreCase) ||
				p.Name.Equals(childPropertyName, StringComparison.OrdinalIgnoreCase));

			// Check if children property already exists (using the widget's actual children property name)
			var hasChildrenProperty = enrichedProperties.Any(p =>
				p.Name.Equals("children", StringComparison.OrdinalIgnoreCase) ||
				p.Name.Equals(childrenPropertyName, StringComparison.OrdinalIgnoreCase));

			// Add child property if widget has single child and property doesn't exist
			if ((widget.HasSingleChild || IsSingleChildBaseClass(widget.BaseClass)) && !hasChildProperty)
			{

				// Determine if child is required (it usually is for these base classes)
				var childIsRequired = IsSingleChildBaseClass(widget.BaseClass);

				enrichedProperties.Insert(0, new EnrichedPropertyDefinition
				{
					Name = childPropertyName,
					BackingFieldName = childPropertyName,
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
				enrichedProperties.Insert(0, new EnrichedPropertyDefinition
				{
					Name = childrenPropertyName,
					BackingFieldName = childrenPropertyName,
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
			if (LooksLikeDartCollectionLiteral(dartDefaultValue))
				return "null";

			// Common Flutter dimension constants that should become literal numeric defaults in C#.
			if (string.Equals(dartDefaultValue, "kMinInteractiveDimension", StringComparison.Ordinal))
				return "48.0";

			if (string.Equals(dartDefaultValue, "kMinInteractiveDimensionCupertino", StringComparison.Ordinal))
				return "44.0";

			// Handle Dart const Duration() - not compile-time constant in C#
			if (dartDefaultValue.Contains("Duration("))
				return "null";

			// Handle Dart const EdgeInsets
			if (dartDefaultValue.Contains("EdgeInsets"))
				return "null";

			// Handle Dart enums (e.g., Axis.horizontal -> Axis.Horizontal)
			if (dartDefaultValue.Contains(".") && !dartDefaultValue.StartsWith("const"))
			{
				// Parse and convert enum value from camelCase to PascalCase
				var dotIndex = dartDefaultValue.LastIndexOf('.');
				if (dotIndex > 0 && dotIndex < dartDefaultValue.Length - 1)
				{
					var enumType = dartDefaultValue.Substring(0, dotIndex);
					var enumValue = dartDefaultValue.Substring(dotIndex + 1);
					var pascalValue = ToPascalCase(enumValue);
					return $"{enumType}.{pascalValue}";
				}
				return dartDefaultValue;
			}

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

		private static bool LooksLikeDartCollectionLiteral(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return false;

			var trimmed = value.Trim();
			if (trimmed.StartsWith("const ", StringComparison.Ordinal))
			{
				trimmed = trimmed.Substring("const ".Length).TrimStart();
			}

			if ((trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal)) ||
			    (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal)))
			{
				return true;
			}

			if (trimmed.StartsWith("<", StringComparison.Ordinal))
			{
				var typeCloseIndex = trimmed.IndexOf('>');
				if (typeCloseIndex > 0 && typeCloseIndex + 1 < trimmed.Length)
				{
					var opener = trimmed[typeCloseIndex + 1];
					if (opener == '[' || opener == '{')
						return true;
				}
			}

			return false;
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
		/// Converts a camel case name to Pascal case for C# enum values.
		/// </summary>
		private string ToPascalCase(string camelCase)
		{
			if (string.IsNullOrEmpty(camelCase))
				return camelCase;

			// If already starts with uppercase, preserve as-is
			if (char.IsUpper(camelCase[0]))
				return camelCase;

			// For single-letter or very short strings, preserve as-is
			if (camelCase.Length <= 2 && camelCase.All(char.IsLetter))
				return camelCase;

			// Normal case: convert first character to uppercase
			return char.ToUpperInvariant(camelCase[0]) + camelCase.Substring(1);
		}

		/// <summary>
		/// Converts FFI type names (Int8, Int32, Double, etc.) to Dart struct field types (int, double).
		/// In Dart FFI structs, fields use primitive types with annotations, not FFI type names directly.
		/// </summary>
		private string ConvertFfiTypeToStructFieldType(string ffiType, string? dartType = null)
		{
			if (!string.IsNullOrWhiteSpace(dartType))
			{
				var trimmedDartType = dartType.Trim();
				if (trimmedDartType.StartsWith("List<", StringComparison.OrdinalIgnoreCase) &&
					trimmedDartType.Contains("Widget", StringComparison.Ordinal))
				{
					return "Pointer<ChildrenStruct>";
				}
			}

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

			if (_dartStructTypes.Contains(baseType))
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

		/// <summary>
		/// Determines if a property should be optional in the C# constructor even if marked required in Dart.
		/// This provides a better developer experience by allowing sensible defaults.
		/// </summary>
		private bool ShouldBeOptionalParameter(EnrichedPropertyDefinition property)
		{
			// Complex unmarshalable types should always be optional
			if (IsComplexUnmarshalableType(property.CSharpType))
				return true;

			// Enum types should be optional - they can use default enum values
			if (property.IsEnum)
				return true;

			// If it has a known default value (from Dart or our mappings), it's optional
			if (!string.IsNullOrEmpty(property.DefaultValue))
				return true;

			// Dart nullable types should be optional
			if (property.IsDartNullable)
				return true;

			// Types that are "object" should be optional (can't have proper defaults)
			if (property.CSharpType == "object" || property.CSharpType == "Object")
				return true;

			// Specific parameter names that are known to have sane defaults or be optional
			// These are parameters where Flutter uses sensible defaults even if marked required
			var optionalByName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"textSpan", "textScaler", "textHeightBehavior",
				"key", "semanticsLabel", "semanticsIdentifier",
				"overflow" // TextOverflow - defaults to Clip
			};
			if (optionalByName.Contains(property.Name))
				return true;

			// Also check C# type directly for known enum types
			// (some enum types may not be detected via Dart type analysis)
			var knownEnumTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"TextOverflow", "TextAlign", "TextDirection", "TextWidthBasis",
				"MainAxisAlignment", "CrossAxisAlignment", "MainAxisSize",
				"VerticalDirection", "Axis", "Clip", "StackFit", "BoxFit"
			};
			var baseType = property.CSharpType?.TrimEnd('?') ?? "";
			if (knownEnumTypes.Contains(baseType))
				return true;

			return false;
		}

		/// <summary>
		/// Checks if a C# type is a complex type that cannot be marshaled via FFI.
		/// These types should be treated as optional even if marked required in Dart.
		/// </summary>
		private bool IsComplexUnmarshalableType(string? csharpType)
		{
			if (string.IsNullOrEmpty(csharpType))
				return false;

			// Remove nullable marker
			var baseType = csharpType.TrimEnd('?');

			// Remove generic type arguments for matching
			var genericIndex = baseType.IndexOf('<');
			if (genericIndex > 0)
				baseType = baseType.Substring(0, genericIndex);

			// Check against known complex types
			if (ComplexUnmarshalableTypes.Contains(baseType))
				return true;

			// Also check if it's an Action or Func delegate (callbacks are complex)
			if (baseType.StartsWith("Action") || baseType.StartsWith("Func"))
				return true;

			// Check for generic delegates and controllers
			if (baseType.EndsWith("Callback") || baseType.EndsWith("Builder") ||
			    baseType.EndsWith("Listener") || baseType.EndsWith("Handler") ||
			    baseType.EndsWith("Controller") || baseType.EndsWith("Delegate"))
				return true;

			return false;
		}
	}

	/// <summary>
	/// Enriched widget definition with all generation metadata.
	/// </summary>
	public class EnrichedWidgetDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;
		public string? SourceLibrary { get; set; }
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
		/// <summary>
		/// The original Dart nullability (from the Flutter widget parameter).
		/// This is separate from IsNullable which includes pointer types for FFI.
		/// Used by parser generation to determine if null is a valid value.
		/// </summary>
		public bool IsDartNullable { get; set; }
		public string? DefaultValue { get; set; }
		public string? RawDefaultValue { get; set; }
		public bool IsCallback { get; set; }
		public bool IsList { get; set; }
		public bool IsGenericTypeParam { get; set; }
		public bool IsEnum { get; set; }
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
