using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Represents detailed information about an unmapped type encounter.
	/// </summary>
	public record UnmappedTypeInfo
	{
		/// <summary>Gets the original Dart type that could not be mapped.</summary>
		public string DartType { get; init; } = string.Empty;

		/// <summary>Gets the parameter name where this type was encountered, if any.</summary>
		public string? ParameterName { get; init; }

		/// <summary>Gets the widget name where this type was encountered, if any.</summary>
		public string? WidgetName { get; init; }

		/// <summary>Gets the C# type that was returned as fallback.</summary>
		public string FallbackType { get; init; } = string.Empty;

		/// <summary>Gets the reason why mapping failed.</summary>
		public string Reason { get; init; } = string.Empty;

		/// <summary>Gets a suggested solution for mapping this type.</summary>
		public string? Suggestion { get; init; }

		public override string ToString()
		{
			var context = !string.IsNullOrEmpty(WidgetName)
				? $"{WidgetName}.{ParameterName}"
				: ParameterName ?? "(unknown context)";
			return $"'{DartType}' in {context} -> {FallbackType}: {Reason}";
		}
	}

	/// <summary>
	/// Maps Dart types to their corresponding C# types.
	/// </summary>
	public class DartToCSharpMapper
	{
		private readonly TypeMappingRegistry _registry;
		private static readonly Regex GenericTypeRegex = new(@"^(\w+)<(.+)>$", RegexOptions.Compiled);

		/// <summary>
		/// Gets the collection of unmapped types encountered during mapping.
		/// Call ClearUnmappedTypes() to reset this collection.
		/// </summary>
		public List<UnmappedTypeInfo> UnmappedTypes { get; } = new();

		/// <summary>
		/// Gets or sets whether to track unmapped types. Defaults to true.
		/// </summary>
		public bool TrackUnmappedTypes { get; set; } = true;

		/// <summary>
		/// Maps (WidgetName, ParameterName) to their expected types for AMBIGUOUS parameters.
		/// These are parameters that have different types depending on which widget they appear in.
		/// Format: "WidgetName.parameterName" -> "C#Type"
		/// </summary>
		private static readonly Dictionary<string, string> WidgetSpecificParameterTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			// fit parameter - different enum types in different widgets
			["Flexible.fit"] = "FlexFit",
			["Expanded.fit"] = "FlexFit",  // Expanded extends Flexible
			["Spacer.fit"] = "FlexFit",    // Spacer wraps Expanded
			["Stack.fit"] = "StackFit",
			["IndexedStack.fit"] = "StackFit",
			["IndexedStack.sizing"] = "StackFit",  // sizing parameter also uses StackFit
			["Image.fit"] = "BoxFit",
			["FittedBox.fit"] = "BoxFit",
			["RawImage.fit"] = "BoxFit",
			["FadeInImage.fit"] = "BoxFit",
			["DecorationImage.fit"] = "BoxFit",
			["OverflowBox.fit"] = "OverflowBoxFit",

			// direction parameter - Axis vs TextDirection
			["Wrap.direction"] = "Axis",
			["ListBody.mainAxis"] = "Axis",
			["Flex.direction"] = "Axis",
			["Row.direction"] = "Axis",
			["Column.direction"] = "Axis",
			["TwoDimensionalScrollView.mainAxis"] = "Axis",
			["Scrollable.axis"] = "Axis",

			// alignment parameters - widget-specific enum types
			["Wrap.alignment"] = "WrapAlignment",
			["Wrap.runAlignment"] = "WrapAlignment",
			["Wrap.crossAxisAlignment"] = "WrapCrossAlignment",
			["Table.defaultVerticalAlignment"] = "TableCellVerticalAlignment",

			// position parameter - DecorationPosition enum
			["DecoratedBox.position"] = "DecorationPosition",
			["DecoratedBoxTransition.position"] = "DecorationPosition",
			["DecoratedSliver.position"] = "DecorationPosition",

			// behavior parameter - HitTestBehavior vs PlatformViewHitTestBehavior
			["Listener.behavior"] = "HitTestBehavior",
			["MetaData.behavior"] = "HitTestBehavior",
			["TapRegion.behavior"] = "HitTestBehavior",
			["TextFieldTapRegion.behavior"] = "HitTestBehavior",
			["GestureDetector.behavior"] = "HitTestBehavior",
			["RawGestureDetector.behavior"] = "HitTestBehavior",
			["MouseRegion.hitTestBehavior"] = "HitTestBehavior",
			// Platform view widgets use PlatformViewHitTestBehavior
			["AndroidView.hitTestBehavior"] = "PlatformViewHitTestBehavior",
			["AndroidViewSurface.hitTestBehavior"] = "PlatformViewHitTestBehavior",
			["UiKitView.hitTestBehavior"] = "PlatformViewHitTestBehavior",
			["PlatformViewSurface.hitTestBehavior"] = "PlatformViewHitTestBehavior",
			["HtmlElementView.hitTestBehavior"] = "PlatformViewHitTestBehavior",
			// Regular scroll views use HitTestBehavior
			["ListWheelScrollView.hitTestBehavior"] = "HitTestBehavior",
			["PageView.hitTestBehavior"] = "HitTestBehavior",
			["SingleChildScrollView.hitTestBehavior"] = "HitTestBehavior",
			["TwoDimensionalScrollView.hitTestBehavior"] = "HitTestBehavior",

			// overflow parameter - usually TextOverflow for text-related widgets
			["DefaultTextStyle.overflow"] = "TextOverflow",
			["DefaultTextStyleTransition.overflow"] = "TextOverflow",
			["RichText.overflow"] = "TextOverflow",
			["Text.overflow"] = "TextOverflow",
			["AnimatedDefaultTextStyle.overflow"] = "TextOverflow",

			// crossAxisAlignment - CrossAxisAlignment for Flex-based widgets, WrapCrossAlignment for Wrap
			["Flex.crossAxisAlignment"] = "CrossAxisAlignment",
			["Row.crossAxisAlignment"] = "CrossAxisAlignment",
			["Column.crossAxisAlignment"] = "CrossAxisAlignment",

			// TwoDimensionalScrollView specific
			["TwoDimensionalScrollView.mainAxis"] = "Axis",
			["TwoDimensionalScrollView.diagonalDragBehavior"] = "DiagonalDragBehavior",
		};

		/// <summary>
		/// Maps UNAMBIGUOUS parameter names to their expected types when type resolution fails.
		/// Only includes names that have a unique meaning across all widgets.
		/// AMBIGUOUS names (alignment, direction, fit, crossAxisAlignment, shape) are excluded
		/// because they can refer to different types in different widgets.
		/// </summary>
		private static readonly Dictionary<string, string> ParameterNameToType = new(StringComparer.OrdinalIgnoreCase)
		{
			// Unambiguous Flutter layout enums - these always mean the same thing
			["mainAxisAlignment"] = "MainAxisAlignment",
			["mainAxisSize"] = "MainAxisSize",
			["verticalDirection"] = "VerticalDirection",
			["textBaseline"] = "TextBaseline",
			["flexFit"] = "FlexFit",
			["filterQuality"] = "FilterQuality",
			["blendMode"] = "BlendMode",
			["stackFit"] = "StackFit",
			["textOverflow"] = "TextOverflow",
			["textAlign"] = "TextAlign",
			["textWidthBasis"] = "TextWidthBasis",
			["selectionHeightStyle"] = "BoxHeightStyle",
			["selectionWidthStyle"] = "BoxWidthStyle",
			["scrollDirection"] = "Axis",
			["dragStartBehavior"] = "DragStartBehavior",
			["keyboardDismissBehavior"] = "ScrollViewKeyboardDismissBehavior",
			["clipBehavior"] = "Clip",
			["textDirection"] = "TextDirection",
			["baselineType"] = "TextBaseline",

			// AMBIGUOUS - intentionally excluded:
			// ["fit"] - FlexFit, BoxFit, or StackFit depending on widget
			// ["alignment"] - AlignmentGeometry, WrapAlignment, TableCellVerticalAlignment, etc.
			// ["crossAxisAlignment"] - CrossAxisAlignment or WrapCrossAlignment
			// ["axis"] - Axis enum but also ScrollController.axis
			// ["direction"] - Axis, TextDirection, or VerticalDirection
			// ["shape"] - BoxShape, ShapeBorder, OutlinedBorder, etc.
			// ["overflow"] - TextOverflow or Overflow (deprecated)
			// ["hitTestBehavior"] - HitTestBehavior or PlatformViewHitTestBehavior
			// ["behavior"] - HitTestBehavior, DragBehavior, etc.

			// Unambiguous geometric types
			["padding"] = "EdgeInsetsGeometry",
			["margin"] = "EdgeInsetsGeometry",
			["foregroundDecoration"] = "Decoration",
			["constraints"] = "BoxConstraints",
			["transform"] = "Matrix4",
			["transformAlignment"] = "AlignmentGeometry",
			["borderRadius"] = "BorderRadiusGeometry",
			["border"] = "BoxBorder",
			["gradient"] = "Gradient",
			["curve"] = "Curve",
			["duration"] = "TimeSpan",
			["textStyle"] = "TextStyle",
			["focusNode"] = "FocusNode",
			["physics"] = "ScrollPhysics",

			// Ambiguous - excluded:
			// ["decoration"] - Decoration or BoxDecoration or ShapeDecoration
			// ["style"] - TextStyle, ButtonStyle, IconButtonThemeData, etc.
			// ["offset"] - Offset or int (for SliverList index offset)
			// ["size"] - Size or double (for fontSize)
			// ["rect"] - Rect or BorderRadius
			// ["controller"] - ScrollController, TextEditingController, AnimationController, etc.
			// ["image"] - ImageProvider, DecorationImage, AssetImage

			// Common children - unambiguous
			["child"] = "Widget",
			["sliver"] = "Widget",
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="DartToCSharpMapper"/> class.
		/// </summary>
		/// <param name="registry">The type mapping registry to use.</param>
		public DartToCSharpMapper(TypeMappingRegistry registry)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}

		/// <summary>
		/// Clears the collection of unmapped types.
		/// </summary>
		public void ClearUnmappedTypes()
		{
			UnmappedTypes.Clear();
		}

		/// <summary>
		/// Gets a summary report of unmapped types grouped by category.
		/// </summary>
		public UnmappedTypesReport GetUnmappedTypesReport()
		{
			return new UnmappedTypesReport(UnmappedTypes);
		}

		/// <summary>
		/// Maps a Dart type string to its corresponding C# type.
		/// </summary>
		/// <param name="dartType">The Dart type string (e.g., "String", "int?", "List&lt;Widget&gt;").</param>
		/// <returns>The mapped C# type string.</returns>
		public string MapType(string dartType)
		{
			return MapType(dartType, null);
		}

		/// <summary>
		/// Maps a Dart type string to its corresponding C# type, with optional parameter name inference.
		/// </summary>
		/// <param name="dartType">The Dart type string (e.g., "String", "int?", "List&lt;Widget&gt;").</param>
		/// <param name="parameterName">Optional parameter name used for type inference when dartType is "InvalidType".</param>
		/// <returns>The mapped C# type string.</returns>
		public string MapType(string dartType, string? parameterName)
		{
			return MapType(dartType, parameterName, null);
		}

		/// <summary>
		/// Maps a Dart type string to its corresponding C# type, with widget-context-aware parameter name inference.
		/// </summary>
		/// <param name="dartType">The Dart type string (e.g., "String", "int?", "List&lt;Widget&gt;").</param>
		/// <param name="parameterName">Optional parameter name used for type inference when dartType is "InvalidType".</param>
		/// <param name="widgetName">Optional widget name for widget-specific type inference of ambiguous parameters.</param>
		/// <returns>The mapped C# type string.</returns>
		public string MapType(string dartType, string? parameterName, string? widgetName)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				throw new ArgumentException("Dart type cannot be null or empty.", nameof(dartType));
			}

			// Handle nullable types
			var isNullable = dartType.EndsWith("?");
			var cleanType = isNullable ? dartType.TrimEnd('?') : dartType;

			// FIRST: Check for widget-specific type overrides (even when we have a valid Dart type)
			// This handles cases where the Dart analyzer returns a type that differs from the actual widget type
			// (e.g., Dart may resolve hitTestBehavior to PlatformViewHitTestBehavior when it should be HitTestBehavior)
			if (!string.IsNullOrEmpty(widgetName) && !string.IsNullOrEmpty(parameterName))
			{
				var key = $"{widgetName}.{parameterName}";
				if (WidgetSpecificParameterTypes.TryGetValue(key, out var widgetSpecificType))
				{
					return ApplyNullability(widgetSpecificType, isNullable);
				}
			}

			// Handle InvalidType - try to infer from parameter name (with widget context if available)
			if (cleanType == "InvalidType")
			{
				if (!string.IsNullOrEmpty(parameterName))
				{
					var inferredType = InferTypeFromParameterName(parameterName, widgetName);
					if (inferredType != null)
					{
						// Track successful inference from InvalidType
						if (TrackUnmappedTypes)
						{
							UnmappedTypes.Add(new UnmappedTypeInfo
							{
								DartType = dartType,
								ParameterName = parameterName,
								WidgetName = widgetName,
								FallbackType = inferredType,
								Reason = "Dart analyzer returned InvalidType; type inferred from parameter name",
								Suggestion = $"Consider adding explicit mapping for '{parameterName}' pattern"
							});
						}
						return ApplyNullability(inferredType, isNullable);
					}
				}

				// If inference failed, return IntPtr as a safe fallback for FFI compatibility
				// Using IntPtr instead of 'object' because structs must be blittable for FFI pinning
				if (TrackUnmappedTypes)
				{
					UnmappedTypes.Add(new UnmappedTypeInfo
					{
						DartType = dartType,
						ParameterName = parameterName,
						WidgetName = widgetName,
						FallbackType = "IntPtr",
						Reason = "Dart analyzer returned InvalidType and no parameter name inference was possible",
						Suggestion = !string.IsNullOrEmpty(parameterName)
							? $"Add '{parameterName}' to ParameterNameToType dictionary or add widget-specific mapping '{widgetName}.{parameterName}'"
							: "Provide parameter name for type inference"
					});
				}
				return "IntPtr";
			}

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

				// If the Dart type is generic but missing type arguments, apply default generic args.
				if (mapping.IsGeneric && !cleanType.Contains("<"))
				{
					csharpType = ApplyDefaultGenericArguments(cleanType, csharpType);
				}

				return isNullable && !mapping.IsNullable
					? ApplyNullability(csharpType, true)
					: csharpType;
			}

			// Handle generic types
			var genericMatch = GenericTypeRegex.Match(cleanType);
			if (genericMatch.Success)
			{
				var baseType = genericMatch.Groups[1].Value;
				var typeArgs = ParseGenericArguments(genericMatch.Groups[2].Value);
				if (typeArgs.Count == 0 || typeArgs.All(string.IsNullOrWhiteSpace))
				{
					typeArgs = GetDefaultGenericArgumentPlaceholders(baseType);
				}
				else
				{
					typeArgs = typeArgs
						.Select(arg => string.IsNullOrWhiteSpace(arg) ? "Object" : arg)
						.ToList();
				}
				var mappedArgs = typeArgs.Select(t => MapType(t, null)).ToList();

				// Map the base type
				var baseMapping = _registry.GetMapping(baseType);
				if (baseMapping != null)
				{
					var mappedBase = baseMapping.CSharpType;
					if (!baseMapping.IsGeneric)
					{
						return ApplyNullability(mappedBase, isNullable);
					}
					var result = $"{mappedBase}<{string.Join(", ", mappedArgs)}>";
					return ApplyNullability(result, isNullable);
				}

				// Fallback for unknown generic types
				if (TrackUnmappedTypes)
				{
					UnmappedTypes.Add(new UnmappedTypeInfo
					{
						DartType = dartType,
						ParameterName = parameterName,
						WidgetName = widgetName,
						FallbackType = "object",
						Reason = $"Unknown generic base type '{baseType}'; defaulting to object",
						Suggestion = $"Add explicit mapping for generic base type '{baseType}' if strong typing is required"
					});
				}
				return ApplyNullability("object", isNullable);
			}

			// Handle callback types - map to Action
			if (IsCallbackType(cleanType))
			{
				return ApplyNullability("Action", isNullable);
			}

			// Fallback: when no mapping is found, default to 'object' to avoid referencing missing types at generation time.
			if (TrackUnmappedTypes)
			{
				UnmappedTypes.Add(new UnmappedTypeInfo
				{
					DartType = dartType,
					FallbackType = "object",
					Reason = "No mapping found; defaulting to object"
				});
			}
			return ApplyNullability("object", isNullable);
		}

		private static string ApplyDefaultGenericArguments(string dartBaseType, string csharpBaseType)
		{
			var arity = GetDefaultGenericArity(dartBaseType, csharpBaseType);
			if (arity <= 0)
			{
				return csharpBaseType;
			}

			var args = Enumerable.Repeat("object", arity);
			return $"{csharpBaseType}<{string.Join(", ", args)}>";
		}

		private static string ApplyNullability(string csharpType, bool isNullable)
		{
			if (!isNullable || csharpType.EndsWith("?"))
			{
				return csharpType;
			}

			return $"{csharpType}?";
		}

		private static List<string> GetDefaultGenericArgumentPlaceholders(string dartBaseType)
		{
			var arity = GetDefaultGenericArity(dartBaseType, dartBaseType);
			return Enumerable.Repeat("Object", arity).ToList();
		}

		private static int GetDefaultGenericArity(string dartBaseType, string csharpBaseType)
		{
			if (string.Equals(csharpBaseType, "Dictionary", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(dartBaseType, "Map", StringComparison.OrdinalIgnoreCase))
			{
				return 2;
			}

			return 1;
		}

		/// <summary>
		/// Infers a C# type from a parameter name when type resolution fails.
		/// </summary>
		/// <param name="parameterName">The parameter name to infer type from.</param>
		/// <returns>The inferred C# type, or null if no inference could be made.</returns>
		public static string? InferTypeFromParameterName(string parameterName)
		{
			return InferTypeFromParameterName(parameterName, null);
		}

		/// <summary>
		/// Infers a C# type from a parameter name when type resolution fails,
		/// with optional widget-context-aware inference for ambiguous parameters.
		/// </summary>
		/// <param name="parameterName">The parameter name to infer type from.</param>
		/// <param name="widgetName">Optional widget name for context-aware inference.</param>
		/// <returns>The inferred C# type, or null if no inference could be made.</returns>
		public static string? InferTypeFromParameterName(string parameterName, string? widgetName)
		{
			if (string.IsNullOrEmpty(parameterName))
			{
				return null;
			}

			// Try widget-specific type mapping first (for ambiguous parameters like fit, direction, behavior)
			if (!string.IsNullOrEmpty(widgetName))
			{
				var key = $"{widgetName}.{parameterName}";
				if (WidgetSpecificParameterTypes.TryGetValue(key, out var widgetSpecificType))
				{
					return widgetSpecificType;
				}
			}

			// Try exact match for unambiguous parameters
			if (ParameterNameToType.TryGetValue(parameterName, out var exactType))
			{
				return exactType;
			}

			// Try suffix matching for common patterns
			var lowerName = parameterName.ToLowerInvariant();

			if (lowerName.EndsWith("color"))
			{
				return "Color";
			}
			if (lowerName.EndsWith("alignment") && !lowerName.Contains("main") && !lowerName.Contains("cross"))
			{
				return "AlignmentGeometry";
			}
			if (lowerName.EndsWith("padding"))
			{
				return "EdgeInsetsGeometry";
			}
			if (lowerName.EndsWith("margin"))
			{
				return "EdgeInsetsGeometry";
			}
			if (lowerName.EndsWith("radius"))
			{
				return "BorderRadiusGeometry";
			}
			if (lowerName.EndsWith("decoration"))
			{
				return "Decoration";
			}
			if (lowerName.EndsWith("constraints"))
			{
				return "BoxConstraints";
			}
			if (lowerName.EndsWith("style") && lowerName.Contains("text"))
			{
				return "TextStyle";
			}
			if (lowerName.EndsWith("callback") || lowerName.StartsWith("on"))
			{
				return "Action";
			}

			return null;
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

			// Check for specific named builder patterns before generating generic delegate
			// This provides better type safety and IntelliSense in generated code
			var signature = $"{mappedReturnType}({string.Join(", ", paramList)})";

			// WidgetBuilder: Widget Function(BuildContext)
			if (signature == "Widget(BuildContext)")
			{
				return "Func<BuildContext, Widget>";
			}

			// IndexedWidgetBuilder: Widget Function(BuildContext, int)
			if (signature == "Widget(BuildContext, int)")
			{
				return "Func<BuildContext, int, Widget>";
			}

			// NullableIndexedWidgetBuilder: Widget? Function(BuildContext, int)
			if (signature == "Widget?(BuildContext, int)")
			{
				return "Func<BuildContext, int, Widget?>";
			}

			// TransitionBuilder: Widget Function(BuildContext, Widget?)
			if (signature == "Widget(BuildContext, Widget?)")
			{
				return "Func<BuildContext, Widget?, Widget>";
			}

			// AnimatedWidgetBuilder: Widget Function(BuildContext, Animation<double>)
			// Note: Animation<double> typically maps to object for now
			if (signature == "Widget(BuildContext, object)")
			{
				return "Func<BuildContext, object, Widget>";
			}

			// Generate appropriate C# delegate type for other cases
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
		/// Determines if a Dart type is a callback type based on naming convention.
		/// Callback types in Dart typically end with "Callback" (e.g., GestureTapCallback, VoidCallback).
		/// </summary>
		/// <param name="dartType">The Dart type to check.</param>
		/// <returns>True if the type appears to be a callback type, false otherwise.</returns>
		private static bool IsCallbackType(string dartType)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				return false;
			}

			// Common Flutter callback type suffixes
			return dartType.EndsWith("Callback") ||
			       dartType.EndsWith("Builder") ||
			       dartType.EndsWith("Listener") ||
			       dartType == "VoidCallback" ||
			       dartType == "ValueChanged" ||
			       dartType == "ValueGetter" ||
			       dartType == "ValueSetter";
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

	/// <summary>
	/// Provides a summary report of unmapped types grouped by category.
	/// </summary>
	public class UnmappedTypesReport
	{
		/// <summary>
		/// Gets all unmapped type entries.
		/// </summary>
		public IReadOnlyList<UnmappedTypeInfo> AllEntries { get; }

		/// <summary>
		/// Gets entries where types were inferred from parameter names.
		/// </summary>
		public IReadOnlyList<UnmappedTypeInfo> InferredTypes { get; }

		/// <summary>
		/// Gets entries where types could not be mapped and fell back to IntPtr.
		/// </summary>
		public IReadOnlyList<UnmappedTypeInfo> FailedMappings { get; }

		/// <summary>
		/// Gets entries grouped by widget name.
		/// </summary>
		public IReadOnlyDictionary<string, List<UnmappedTypeInfo>> ByWidget { get; }

		/// <summary>
		/// Gets entries grouped by the original Dart type.
		/// </summary>
		public IReadOnlyDictionary<string, List<UnmappedTypeInfo>> ByDartType { get; }

		/// <summary>
		/// Gets entries grouped by the parameter name.
		/// </summary>
		public IReadOnlyDictionary<string, List<UnmappedTypeInfo>> ByParameterName { get; }

		/// <summary>
		/// Gets the total count of unmapped types.
		/// </summary>
		public int TotalCount => AllEntries.Count;

		/// <summary>
		/// Gets the count of types that were successfully inferred.
		/// </summary>
		public int InferredCount => InferredTypes.Count;

		/// <summary>
		/// Gets the count of types that failed to map (returned IntPtr).
		/// </summary>
		public int FailedCount => FailedMappings.Count;

		public UnmappedTypesReport(IEnumerable<UnmappedTypeInfo> entries)
		{
			var allEntries = entries.ToList();
			AllEntries = allEntries;

			InferredTypes = allEntries
				.Where(e => e.FallbackType != "IntPtr")
				.ToList();

			FailedMappings = allEntries
				.Where(e => e.FallbackType == "IntPtr")
				.ToList();

			ByWidget = allEntries
				.Where(e => !string.IsNullOrEmpty(e.WidgetName))
				.GroupBy(e => e.WidgetName!)
				.ToDictionary(g => g.Key, g => g.ToList());

			ByDartType = allEntries
				.GroupBy(e => e.DartType)
				.ToDictionary(g => g.Key, g => g.ToList());

			ByParameterName = allEntries
				.Where(e => !string.IsNullOrEmpty(e.ParameterName))
				.GroupBy(e => e.ParameterName!)
				.ToDictionary(g => g.Key, g => g.ToList());
		}

		/// <summary>
		/// Gets suggestions for improving type mappings based on the patterns found.
		/// </summary>
		public IEnumerable<string> GetSuggestions()
		{
			var suggestions = new List<string>();

			// Find most common unmapped parameter names
			var frequentParams = ByParameterName
				.Where(kvp => kvp.Value.Count >= 3 && kvp.Value.All(v => v.FallbackType == "IntPtr"))
				.OrderByDescending(kvp => kvp.Value.Count)
				.Take(10);

			foreach (var param in frequentParams)
			{
				suggestions.Add($"Add parameter name mapping: '{param.Key}' (appears {param.Value.Count} times). " +
					$"Widgets: {string.Join(", ", param.Value.Select(v => v.WidgetName).Distinct().Take(5))}");
			}

			// Find widgets with many unmapped types
			var problematicWidgets = ByWidget
				.Where(kvp => kvp.Value.Count >= 5)
				.OrderByDescending(kvp => kvp.Value.Count)
				.Take(5);

			foreach (var widget in problematicWidgets)
			{
				suggestions.Add($"Consider excluding widget '{widget.Key}' from generation ({widget.Value.Count} unmapped types)");
			}

			return suggestions;
		}

		/// <summary>
		/// Returns a formatted string summary of the report.
		/// </summary>
		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"Unmapped Types Report: {TotalCount} total ({InferredCount} inferred, {FailedCount} failed)");

			if (FailedCount > 0)
			{
				sb.AppendLine();
				sb.AppendLine("=== Failed Mappings (IntPtr fallback) ===");
				foreach (var entry in FailedMappings.Take(20))
				{
					sb.AppendLine($"  - {entry}");
					if (!string.IsNullOrEmpty(entry.Suggestion))
					{
						sb.AppendLine($"    Suggestion: {entry.Suggestion}");
					}
				}
				if (FailedCount > 20)
				{
					sb.AppendLine($"  ... and {FailedCount - 20} more");
				}
			}

			var suggestions = GetSuggestions().ToList();
			if (suggestions.Any())
			{
				sb.AppendLine();
				sb.AppendLine("=== Suggestions ===");
				foreach (var suggestion in suggestions)
				{
					sb.AppendLine($"  * {suggestion}");
				}
			}

			return sb.ToString();
		}
	}
}
