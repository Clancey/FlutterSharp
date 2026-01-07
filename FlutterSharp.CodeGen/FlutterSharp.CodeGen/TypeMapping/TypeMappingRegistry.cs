using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Central registry for all type mappings between Dart, C#, and FFI types.
	/// </summary>
	public class TypeMappingRegistry
	{
		private readonly Dictionary<string, TypeMapping> _dartTypeMappings = new();
		private readonly Dictionary<string, TypeMapping> _csharpTypeMappings = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeMappingRegistry"/> class.
		/// </summary>
		public TypeMappingRegistry()
		{
			RegisterDefaultMappings();
		}

		/// <summary>
		/// Registers a type mapping.
		/// </summary>
		/// <param name="mapping">The type mapping to register.</param>
		public void RegisterMapping(TypeMapping mapping)
		{
			if (mapping == null)
			{
				throw new ArgumentNullException(nameof(mapping));
			}

			_dartTypeMappings[mapping.DartType] = mapping;
			_csharpTypeMappings[mapping.CSharpType] = mapping;
		}

		/// <summary>
		/// Gets a type mapping by Dart type name.
		/// </summary>
		/// <param name="dartType">The Dart type name.</param>
		/// <returns>The type mapping, or null if not found.</returns>
		public TypeMapping? GetMapping(string dartType)
		{
			if (string.IsNullOrWhiteSpace(dartType))
			{
				return null;
			}

			_dartTypeMappings.TryGetValue(dartType, out var mapping);
			return mapping;
		}

		/// <summary>
		/// Gets a type mapping by C# type name.
		/// </summary>
		/// <param name="csharpType">The C# type name.</param>
		/// <returns>The type mapping, or null if not found.</returns>
		public TypeMapping? GetMappingByCSharpType(string csharpType)
		{
			if (string.IsNullOrWhiteSpace(csharpType))
			{
				return null;
			}

			_csharpTypeMappings.TryGetValue(csharpType, out var mapping);
			return mapping;
		}

		/// <summary>
		/// Checks if a Dart type has a mapping.
		/// </summary>
		/// <param name="dartType">The Dart type name.</param>
		/// <returns>True if a mapping exists, false otherwise.</returns>
		public bool HasMapping(string dartType)
		{
			return !string.IsNullOrWhiteSpace(dartType) && _dartTypeMappings.ContainsKey(dartType);
		}

		/// <summary>
		/// Gets all registered type mappings.
		/// </summary>
		/// <returns>An enumerable of all type mappings.</returns>
		public IEnumerable<TypeMapping> GetAllMappings()
		{
			return _dartTypeMappings.Values;
		}

		/// <summary>
		/// Gets all widget type mappings.
		/// </summary>
		/// <returns>An enumerable of widget type mappings.</returns>
		public IEnumerable<TypeMapping> GetWidgetMappings()
		{
			return _dartTypeMappings.Values.Where(m => m.IsWidget);
		}

		/// <summary>
		/// Gets all primitive type mappings.
		/// </summary>
		/// <returns>An enumerable of primitive type mappings.</returns>
		public IEnumerable<TypeMapping> GetPrimitiveMappings()
		{
			return _dartTypeMappings.Values.Where(m => m.IsPrimitive);
		}

		/// <summary>
		/// Gets all collection type mappings.
		/// </summary>
		/// <returns>An enumerable of collection type mappings.</returns>
		public IEnumerable<TypeMapping> GetCollectionMappings()
		{
			return _dartTypeMappings.Values.Where(m => m.IsCollection);
		}

		/// <summary>
		/// Clears all registered mappings.
		/// </summary>
		public void Clear()
		{
			_dartTypeMappings.Clear();
			_csharpTypeMappings.Clear();
		}

		/// <summary>
		/// Registers all default type mappings for common Flutter/Dart types.
		/// </summary>
		private void RegisterDefaultMappings()
		{
			// Primitive types
			RegisterPrimitiveTypes();

			// Flutter core types
			RegisterFlutterCoreTypes();

			// Flutter widget types
			RegisterFlutterWidgetTypes();

			// Flutter material types
			RegisterFlutterMaterialTypes();

			// Flutter painting types
			RegisterFlutterPaintingTypes();

			// Flutter rendering types
			RegisterFlutterRenderingTypes();

			// Collection types
			RegisterCollectionTypes();

			// Function/callback types
			RegisterCallbackTypes();

			// Common third-party types
			RegisterThirdPartyTypes();

			// Additional Flutter SDK types
			RegisterAdditionalFlutterTypes();
		}

		private void RegisterPrimitiveTypes()
		{
			// String
			RegisterMapping(new TypeMapping
			{
				DartType = "String",
				CSharpType = "string",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseString",
				IsPrimitive = true,
				IsNullable = false,
				CSharpToDartConversion = "{value}.ToNativeUtf8()",
				DartToCSharpConversion = "Marshal.PtrToStringUTF8({value})",
				Metadata = new Dictionary<string, object> { ["Description"] = "UTF-8 string type" }
			});

			// int
			RegisterMapping(new TypeMapping
			{
				DartType = "int",
				CSharpType = "int",
				DartStructType = "Int32",
				DartParserFunction = "parseInt",
				IsPrimitive = true,
				IsNullable = false,
				CSharpToDartConversion = "{value}",
				DartToCSharpConversion = "{value}",
				Metadata = new Dictionary<string, object> { ["FfiSize"] = 4 }
			});

			// double
			RegisterMapping(new TypeMapping
			{
				DartType = "double",
				CSharpType = "double",
				DartStructType = "Double",
				DartParserFunction = "parseDouble",
				IsPrimitive = true,
				IsNullable = false,
				CSharpToDartConversion = "{value}",
				DartToCSharpConversion = "{value}",
				Metadata = new Dictionary<string, object> { ["FfiSize"] = 8 }
			});

			// bool - represented as int in FFI structs with @Int8() annotation
			RegisterMapping(new TypeMapping
			{
				DartType = "bool",
				CSharpType = "bool",
				DartStructType = "Int8",
				DartParserFunction = "parseBool",
				IsPrimitive = true,
				IsNullable = false,
				CSharpToDartConversion = "({value} ? 1 : 0)",
				DartToCSharpConversion = "({value} != 0)",
				Metadata = new Dictionary<string, object> { ["FfiSize"] = 1 }
			});

			// num (Dart's number type)
			RegisterMapping(new TypeMapping
			{
				DartType = "num",
				CSharpType = "double",
				DartStructType = "Double",
				DartParserFunction = "parseNum",
				IsPrimitive = true,
				IsNullable = false
			});

			// dynamic
			RegisterMapping(new TypeMapping
			{
				DartType = "dynamic",
				CSharpType = "object",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseDynamic",
				IsPrimitive = false,
				IsNullable = true,
				RequiresCustomMarshalling = true
			});

			// Object
			RegisterMapping(new TypeMapping
			{
				DartType = "Object",
				CSharpType = "object",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseObject",
				IsPrimitive = false,
				IsNullable = false,
				RequiresCustomMarshalling = true
			});

			// void
			RegisterMapping(new TypeMapping
			{
				DartType = "void",
				CSharpType = "void",
				DartStructType = "Void",
				IsPrimitive = true,
				IsNullable = false
			});
		}

		private void RegisterFlutterCoreTypes()
		{
			// Key
			RegisterMapping(new TypeMapping
			{
				DartType = "Key",
				CSharpType = "Key",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseKey",
				IsWidget = false,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Duration
			RegisterMapping(new TypeMapping
			{
				DartType = "Duration",
				CSharpType = "TimeSpan",
				DartStructType = "Int64",
				DartParserFunction = "parseDuration",
				IsPrimitive = false,
				Package = "dart:core",
				CSharpToDartConversion = "{value}.TotalMilliseconds",
				DartToCSharpConversion = "TimeSpan.FromMilliseconds({value})"
			});

			// DateTime
			RegisterMapping(new TypeMapping
			{
				DartType = "DateTime",
				CSharpType = "DateTime",
				DartStructType = "Int64",
				DartParserFunction = "parseDateTime",
				IsPrimitive = false,
				Package = "dart:core",
				CSharpToDartConversion = "{value}.ToUniversalTime().Ticks",
				DartToCSharpConversion = "new DateTime({value}, DateTimeKind.Utc)"
			});
		}

		private void RegisterFlutterWidgetTypes()
		{
			// Widget (base class)
			RegisterMapping(new TypeMapping
			{
				DartType = "Widget",
				CSharpType = "Widget",
				DartStructType = "Pointer<Void>",
				DartParserFunction = null,  // Widget properties are handled by template child/children logic
				IsWidget = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// StatelessWidget
			RegisterMapping(new TypeMapping
			{
				DartType = "StatelessWidget",
				CSharpType = "StatelessWidget",
				DartStructType = "Pointer<Void>",
				DartParserFunction = null,  // Widget properties are handled by template child/children logic
				IsWidget = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// StatefulWidget
			RegisterMapping(new TypeMapping
			{
				DartType = "StatefulWidget",
				CSharpType = "StatefulWidget",
				DartStructType = "Pointer<Void>",
				DartParserFunction = null,  // Widget properties are handled by template child/children logic
				IsWidget = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// PreferredSizeWidget
			RegisterMapping(new TypeMapping
			{
				DartType = "PreferredSizeWidget",
				CSharpType = "PreferredSizeWidget",
				DartStructType = "Pointer<Void>",
				DartParserFunction = null,  // Widget properties are handled by template child/children logic
				IsWidget = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// BuildContext
			RegisterMapping(new TypeMapping
			{
				DartType = "BuildContext",
				CSharpType = "BuildContext",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBuildContext",
				IsWidget = false,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterFlutterMaterialTypes()
		{
			// Color
			RegisterMapping(new TypeMapping
			{
				DartType = "Color",
				CSharpType = "Color",
				DartStructType = "Uint32",
				DartParserFunction = "parseColor",
				IsPrimitive = false,
				Package = "dart:ui",
				CSharpToDartConversion = "{value}.Value",
				DartToCSharpConversion = "new Color({value})"
			});

			// MaterialColor
			RegisterMapping(new TypeMapping
			{
				DartType = "MaterialColor",
				CSharpType = "MaterialColor",
				DartStructType = "Uint32",
				DartParserFunction = "parseMaterialColor",
				IsPrimitive = false,
				Package = "flutter/material"
			});

			// MaterialStateProperty<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "MaterialStateProperty",
				CSharpType = "MaterialStateProperty",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseMaterialStateProperty",
				IsGeneric = true,
				Package = "flutter/material",
				RequiresCustomMarshalling = true
			});

			// ThemeData
			RegisterMapping(new TypeMapping
			{
				DartType = "ThemeData",
				CSharpType = "ThemeData",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseThemeData",
				Package = "flutter/material",
				RequiresCustomMarshalling = true
			});

			// IconData
			RegisterMapping(new TypeMapping
			{
				DartType = "IconData",
				CSharpType = "IconData",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseIconData",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterFlutterPaintingTypes()
		{
			// EdgeInsets
			RegisterMapping(new TypeMapping
			{
				DartType = "EdgeInsets",
				CSharpType = "EdgeInsets",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseEdgeInsets",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// EdgeInsetsGeometry
			RegisterMapping(new TypeMapping
			{
				DartType = "EdgeInsetsGeometry",
				CSharpType = "EdgeInsetsGeometry",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseEdgeInsetsGeometry",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// Alignment
			RegisterMapping(new TypeMapping
			{
				DartType = "Alignment",
				CSharpType = "Alignment",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseAlignment",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// AlignmentGeometry
			RegisterMapping(new TypeMapping
			{
				DartType = "AlignmentGeometry",
				CSharpType = "AlignmentGeometry",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseAlignmentGeometry",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// BorderRadius
			RegisterMapping(new TypeMapping
			{
				DartType = "BorderRadius",
				CSharpType = "BorderRadius",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBorderRadius",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// BorderRadiusGeometry
			RegisterMapping(new TypeMapping
			{
				DartType = "BorderRadiusGeometry",
				CSharpType = "BorderRadiusGeometry",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBorderRadiusGeometry",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// BoxDecoration
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxDecoration",
				CSharpType = "BoxDecoration",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBoxDecoration",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// Decoration
			RegisterMapping(new TypeMapping
			{
				DartType = "Decoration",
				CSharpType = "Decoration",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseDecoration",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// BoxConstraints
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxConstraints",
				CSharpType = "BoxConstraints",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBoxConstraints",
				IsPrimitive = false,
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			// TextStyle
			RegisterMapping(new TypeMapping
			{
				DartType = "TextStyle",
				CSharpType = "TextStyle",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextStyle",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// Gradient
			RegisterMapping(new TypeMapping
			{
				DartType = "Gradient",
				CSharpType = "Gradient",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseGradient",
				IsPrimitive = false,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// ImageProvider
			RegisterMapping(new TypeMapping
			{
				DartType = "ImageProvider",
				CSharpType = "ImageProvider",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseImageProvider",
				IsGeneric = true,
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterFlutterRenderingTypes()
		{
			// Size
			RegisterMapping(new TypeMapping
			{
				DartType = "Size",
				CSharpType = "Size",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSize",
				IsPrimitive = false,
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// Offset
			RegisterMapping(new TypeMapping
			{
				DartType = "Offset",
				CSharpType = "Offset",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseOffset",
				IsPrimitive = false,
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// Rect
			RegisterMapping(new TypeMapping
			{
				DartType = "Rect",
				CSharpType = "Rect",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRect",
				IsPrimitive = false,
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// Radius
			RegisterMapping(new TypeMapping
			{
				DartType = "Radius",
				CSharpType = "Radius",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRadius",
				IsPrimitive = false,
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// Matrix4
			RegisterMapping(new TypeMapping
			{
				DartType = "Matrix4",
				CSharpType = "Matrix4",
				DartStructType = "Pointer<Matrix4Struct>",
				DartParserFunction = "parseMatrix4",
				IsPrimitive = false,
				Package = "package:vector_math/vector_math_64.dart",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterCollectionTypes()
		{
			// List<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "List",
				CSharpType = "List",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseList",
				IsCollection = true,
				IsGeneric = true,
				Package = "dart:core",
				RequiresCustomMarshalling = true
			});

			// Set<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "Set",
				CSharpType = "HashSet",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSet",
				IsCollection = true,
				IsGeneric = true,
				Package = "dart:core",
				RequiresCustomMarshalling = true
			});

			// Map<K, V>
			RegisterMapping(new TypeMapping
			{
				DartType = "Map",
				CSharpType = "Dictionary",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseMap",
				IsCollection = true,
				IsGeneric = true,
				Package = "dart:core",
				RequiresCustomMarshalling = true
			});

			// Iterable<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "Iterable",
				CSharpType = "IEnumerable",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseIterable",
				IsCollection = true,
				IsGeneric = true,
				Package = "dart:core",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterCallbackTypes()
		{
			// All callbacks use Pointer<Utf8> to store an action ID string.
			// The action ID is dispatched to C# via method channel, not called as a native function.
			// This is the unified callback pattern for FlutterSharp interop.

			// VoidCallback
			RegisterMapping(new TypeMapping
			{
				DartType = "VoidCallback",
				CSharpType = "Action",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseVoidCallback",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// ValueChanged<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "ValueChanged",
				CSharpType = "Action",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseValueChanged",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// ValueSetter<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "ValueSetter",
				CSharpType = "Action",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseValueSetter",
				IsGeneric = true,
				Package = "flutter/foundation",
				RequiresCustomMarshalling = true
			});

			// ValueGetter<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "ValueGetter",
				CSharpType = "Func",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseValueGetter",
				IsGeneric = true,
				Package = "flutter/foundation",
				RequiresCustomMarshalling = true
			});

			// GestureTapCallback
			RegisterMapping(new TypeMapping
			{
				DartType = "GestureTapCallback",
				CSharpType = "Action",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseGestureTapCallback",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});
		}

		private void RegisterThirdPartyTypes()
		{
			// Common enum types that are typically registered separately
			// These are placeholders - actual enum mappings should be added dynamically

			// TextAlign
			RegisterMapping(new TypeMapping
			{
				DartType = "TextAlign",
				CSharpType = "TextAlign",
				DartStructType = "Int32",
				DartParserFunction = "parseTextAlign",
				IsEnum = true,
				Package = "dart:ui"
			});

			// MainAxisAlignment
			RegisterMapping(new TypeMapping
			{
				DartType = "MainAxisAlignment",
				CSharpType = "MainAxisAlignment",
				DartStructType = "Int32",
				DartParserFunction = "parseMainAxisAlignment",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// CrossAxisAlignment
			RegisterMapping(new TypeMapping
			{
				DartType = "CrossAxisAlignment",
				CSharpType = "CrossAxisAlignment",
				DartStructType = "Int32",
				DartParserFunction = "parseCrossAxisAlignment",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// MainAxisSize
			RegisterMapping(new TypeMapping
			{
				DartType = "MainAxisSize",
				CSharpType = "MainAxisSize",
				DartStructType = "Int32",
				DartParserFunction = "parseMainAxisSize",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// Axis
			RegisterMapping(new TypeMapping
			{
				DartType = "Axis",
				CSharpType = "Axis",
				DartStructType = "Int32",
				DartParserFunction = "parseAxis",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// VerticalDirection
			RegisterMapping(new TypeMapping
			{
				DartType = "VerticalDirection",
				CSharpType = "VerticalDirection",
				DartStructType = "Int32",
				DartParserFunction = "parseVerticalDirection",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// TextDirection
			RegisterMapping(new TypeMapping
			{
				DartType = "TextDirection",
				CSharpType = "TextDirection",
				DartStructType = "Int32",
				DartParserFunction = "parseTextDirection",
				IsEnum = true,
				Package = "dart:ui"
			});

			// FontWeight
			RegisterMapping(new TypeMapping
			{
				DartType = "FontWeight",
				CSharpType = "FontWeight",
				DartStructType = "Int32",
				DartParserFunction = "parseFontWeight",
				Package = "dart:ui"
			});

			// FontStyle
			RegisterMapping(new TypeMapping
			{
				DartType = "FontStyle",
				CSharpType = "FontStyle",
				DartStructType = "Int32",
				DartParserFunction = "parseFontStyle",
				IsEnum = true,
				Package = "dart:ui"
			});

			// Clip
			RegisterMapping(new TypeMapping
			{
				DartType = "Clip",
				CSharpType = "Clip",
				DartStructType = "Int32",
				DartParserFunction = "parseClip",
				IsEnum = true,
				Package = "dart:ui"
			});

			// BoxFit
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxFit",
				CSharpType = "BoxFit",
				DartStructType = "Int32",
				DartParserFunction = "parseBoxFit",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// BlendMode
			RegisterMapping(new TypeMapping
			{
				DartType = "BlendMode",
				CSharpType = "BlendMode",
				DartStructType = "Int32",
				DartParserFunction = "parseBlendMode",
				IsEnum = true,
				Package = "dart:ui"
			});
		}

		private void RegisterAdditionalFlutterTypes()
		{
			// Navigation types
			RegisterMapping(new TypeMapping
			{
				DartType = "Route",
				CSharpType = "Route",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRoute",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "PageRoute",
				CSharpType = "PageRoute",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parsePageRoute",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "NavigatorState",
				CSharpType = "NavigatorState",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseNavigatorState",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RouteSettings",
				CSharpType = "RouteSettings",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRouteSettings",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RouterDelegate",
				CSharpType = "RouterDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRouterDelegate",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RouterConfig",
				CSharpType = "RouterConfig",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRouterConfig",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RouteInformationParser",
				CSharpType = "RouteInformationParser",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRouteInformationParser",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "BackButtonDispatcher",
				CSharpType = "BackButtonDispatcher",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBackButtonDispatcher",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RouteInformationProvider",
				CSharpType = "RouteInformationProvider",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRouteInformationProvider",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "NavigatorObserver",
				CSharpType = "NavigatorObserver",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseNavigatorObserver",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "NavigationNotification",
				CSharpType = "NavigationNotification",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseNavigationNotification",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Localization types
			RegisterMapping(new TypeMapping
			{
				DartType = "Locale",
				CSharpType = "Locale",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseLocale",
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "LocalizationsDelegate",
				CSharpType = "LocalizationsDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseLocalizationsDelegate",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Shortcut/Intent types
			RegisterMapping(new TypeMapping
			{
				DartType = "ShortcutActivator",
				CSharpType = "ShortcutActivator",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseShortcutActivator",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "Intent",
				CSharpType = "Intent",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseIntent",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "Action",
				CSharpType = "FlutterAction",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFlutterAction",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// State/GlobalKey types
			RegisterMapping(new TypeMapping
			{
				DartType = "GlobalKey",
				CSharpType = "GlobalKey",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseGlobalKey",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "State",
				CSharpType = "State",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseState",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "StatefulWidget",
				CSharpType = "StatefulWidget",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseStatefulWidget",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Delegate types
			RegisterMapping(new TypeMapping
			{
				DartType = "SingleChildLayoutDelegate",
				CSharpType = "SingleChildLayoutDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSingleChildLayoutDelegate",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "MultiChildLayoutDelegate",
				CSharpType = "MultiChildLayoutDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseMultiChildLayoutDelegate",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "LayoutWidgetBuilder",
				CSharpType = "LayoutWidgetBuilder",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseLayoutWidgetBuilder",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollController",
				CSharpType = "ScrollController",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseScrollController",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollPhysics",
				CSharpType = "ScrollPhysics",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseScrollPhysics",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Gesture types
			RegisterMapping(new TypeMapping
			{
				DartType = "GestureRecognizer",
				CSharpType = "GestureRecognizer",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseGestureRecognizer",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "GestureRecognizerFactory",
				CSharpType = "GestureRecognizerFactory",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseGestureRecognizerFactory",
				IsGeneric = true,
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "DragStartBehavior",
				CSharpType = "DragStartBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseDragStartBehavior",
				IsEnum = true,
				Package = "flutter/gestures"
			});

			// Animation types
			RegisterMapping(new TypeMapping
			{
				DartType = "Animation",
				CSharpType = "Animation",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseAnimation",
				IsGeneric = true,
				Package = "flutter/animation",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "AnimationController",
				CSharpType = "AnimationController",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseAnimationController",
				Package = "flutter/animation",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "Curve",
				CSharpType = "Curve",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseCurve",
				Package = "flutter/animation",
				RequiresCustomMarshalling = true
			});

			// Text/Input types
			RegisterMapping(new TypeMapping
			{
				DartType = "TextEditingController",
				CSharpType = "TextEditingController",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextEditingController",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "FocusNode",
				CSharpType = "FocusNode",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFocusNode",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TextInputType",
				CSharpType = "TextInputType",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextInputType",
				Package = "flutter/services",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TextCapitalization",
				CSharpType = "TextCapitalization",
				DartStructType = "Int32",
				DartParserFunction = "parseTextCapitalization",
				IsEnum = true,
				Package = "flutter/services"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TextInputAction",
				CSharpType = "TextInputAction",
				DartStructType = "Int32",
				DartParserFunction = "parseTextInputAction",
				IsEnum = true,
				Package = "flutter/services"
			});

			// Material types
			RegisterMapping(new TypeMapping
			{
				DartType = "FloatingActionButtonLocation",
				CSharpType = "FloatingActionButtonLocation",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFloatingActionButtonLocation",
				Package = "flutter/material",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "FloatingActionButtonAnimator",
				CSharpType = "FloatingActionButtonAnimator",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFloatingActionButtonAnimator",
				Package = "flutter/material",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "SnackBarBehavior",
				CSharpType = "SnackBarBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseSnackBarBehavior",
				IsEnum = true,
				Package = "flutter/material"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "MaterialTapTargetSize",
				CSharpType = "MaterialTapTargetSize",
				DartStructType = "Int32",
				DartParserFunction = "parseMaterialTapTargetSize",
				IsEnum = true,
				Package = "flutter/material"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "Brightness",
				CSharpType = "Brightness",
				DartStructType = "Int32",
				DartParserFunction = "parseBrightness",
				IsEnum = true,
				Package = "dart:ui"
			});

			// Form/Validation types
			RegisterMapping(new TypeMapping
			{
				DartType = "FormFieldValidator",
				CSharpType = "FormFieldValidator",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFormFieldValidator",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "FormFieldSetter",
				CSharpType = "FormFieldSetter",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFormFieldSetter",
				IsGeneric = true,
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "AutovalidateMode",
				CSharpType = "AutovalidateMode",
				DartStructType = "Int32",
				DartParserFunction = "parseAutovalidateMode",
				IsEnum = true,
				Package = "flutter/widgets"
			});

			// Rendering types
			RegisterMapping(new TypeMapping
			{
				DartType = "RenderObject",
				CSharpType = "RenderObject",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRenderObject",
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "RenderBox",
				CSharpType = "RenderBox",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseRenderBox",
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "HitTestBehavior",
				CSharpType = "HitTestBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseHitTestBehavior",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "StackFit",
				CSharpType = "StackFit",
				DartStructType = "Int32",
				DartParserFunction = "parseStackFit",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "WrapAlignment",
				CSharpType = "WrapAlignment",
				DartStructType = "Int32",
				DartParserFunction = "parseWrapAlignment",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "WrapCrossAlignment",
				CSharpType = "WrapCrossAlignment",
				DartStructType = "Int32",
				DartParserFunction = "parseWrapCrossAlignment",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// Overflow/Clipping types
			// Note: "Overflow" is deprecated in Flutter, use TextOverflow instead
			RegisterMapping(new TypeMapping
			{
				DartType = "TextOverflow",
				CSharpType = "TextOverflow",
				DartStructType = "Int32",
				DartParserFunction = "parseTextOverflow",
				IsEnum = true,
				Package = "dart:ui"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TextBaseline",
				CSharpType = "TextBaseline",
				DartStructType = "Int32",
				DartParserFunction = "parseTextBaseline",
				IsEnum = true,
				Package = "dart:ui"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TextWidthBasis",
				CSharpType = "TextWidthBasis",
				DartStructType = "Int32",
				DartParserFunction = "parseTextWidthBasis",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// Image types
			RegisterMapping(new TypeMapping
			{
				DartType = "ImageRepeat",
				CSharpType = "ImageRepeat",
				DartStructType = "Int32",
				DartParserFunction = "parseImageRepeat",
				IsEnum = true,
				Package = "flutter/painting"
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "FilterQuality",
				CSharpType = "FilterQuality",
				DartStructType = "Int32",
				DartParserFunction = "parseFilterQuality",
				IsEnum = true,
				Package = "dart:ui"
			});

			// Scroll types
			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollViewKeyboardDismissBehavior",
				CSharpType = "ScrollViewKeyboardDismissBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseScrollViewKeyboardDismissBehavior",
				IsEnum = true,
				Package = "flutter/widgets"
			});

			// Sliver types
			RegisterMapping(new TypeMapping
			{
				DartType = "SliverChildDelegate",
				CSharpType = "SliverChildDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSliverChildDelegate",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Semantics types
			RegisterMapping(new TypeMapping
			{
				DartType = "SemanticsProperties",
				CSharpType = "SemanticsProperties",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSemanticsProperties",
				Package = "flutter/semantics",
				RequiresCustomMarshalling = true
			});

			// Hero types
			RegisterMapping(new TypeMapping
			{
				DartType = "HeroFlightDirection",
				CSharpType = "HeroFlightDirection",
				DartStructType = "Int32",
				DartParserFunction = "parseHeroFlightDirection",
				IsEnum = true,
				Package = "flutter/widgets"
			});

			// Notification types
			RegisterMapping(new TypeMapping
			{
				DartType = "Notification",
				CSharpType = "Notification",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseNotification",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// Tooltip types
			RegisterMapping(new TypeMapping
			{
				DartType = "TooltipTriggerMode",
				CSharpType = "TooltipTriggerMode",
				DartStructType = "Int32",
				DartParserFunction = "parseTooltipTriggerMode",
				IsEnum = true,
				Package = "flutter/material"
			});

			// Type (for Type references in Dart)
			RegisterMapping(new TypeMapping
			{
				DartType = "Type",
				CSharpType = "Type",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseType",
				Package = "dart:core",
				RequiresCustomMarshalling = true
			});

			// Key event types
			RegisterMapping(new TypeMapping
			{
				DartType = "KeyEvent",
				CSharpType = "KeyEvent",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseKeyEvent",
				Package = "flutter/services",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "KeyEventResult",
				CSharpType = "KeyEventResult",
				DartStructType = "Int32",
				DartParserFunction = "parseKeyEventResult",
				IsEnum = true,
				Package = "flutter/services"
			});

			// Pointer event types
			RegisterMapping(new TypeMapping
			{
				DartType = "PointerEvent",
				CSharpType = "PointerEvent",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parsePointerEvent",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TapUpDetails",
				CSharpType = "TapUpDetails",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTapUpDetails",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "TapDownDetails",
				CSharpType = "TapDownDetails",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTapDownDetails",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "DragUpdateDetails",
				CSharpType = "DragUpdateDetails",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseDragUpdateDetails",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "DragEndDetails",
				CSharpType = "DragEndDetails",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseDragEndDetails",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			RegisterMapping(new TypeMapping
			{
				DartType = "DragStartDetails",
				CSharpType = "DragStartDetails",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseDragStartDetails",
				Package = "flutter/gestures",
				RequiresCustomMarshalling = true
			});

			// Input formatter types
			RegisterMapping(new TypeMapping
			{
				DartType = "TextInputFormatter",
				CSharpType = "TextInputFormatter",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextInputFormatter",
				Package = "flutter/services",
				RequiresCustomMarshalling = true
			});

			// Selection types
			RegisterMapping(new TypeMapping
			{
				DartType = "TextSelection",
				CSharpType = "TextSelection",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextSelection",
				Package = "flutter/services",
				RequiresCustomMarshalling = true
			});

			// Device types
			RegisterMapping(new TypeMapping
			{
				DartType = "PointerDeviceKind",
				CSharpType = "PointerDeviceKind",
				DartStructType = "Int32",
				DartParserFunction = "parsePointerDeviceKind",
				IsEnum = true,
				Package = "dart:ui"
			});

			// Key types
			RegisterMapping(new TypeMapping
			{
				DartType = "Key",
				CSharpType = "string",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseString",
				Package = "flutter/foundation"
			});

			// BuildContext - Flutter build context
			RegisterMapping(new TypeMapping
			{
				DartType = "BuildContext",
				CSharpType = "BuildContext",
				DartStructType = "IntPtr",
				DartParserFunction = "parseBuildContext",
				Package = "flutter/widgets"
			});

			// FocusNode - Focus management
			RegisterMapping(new TypeMapping
			{
				DartType = "FocusNode",
				CSharpType = "FocusNode",
				DartStructType = "IntPtr",
				DartParserFunction = "parseFocusNode",
				Package = "flutter/widgets"
			});

			// ScrollController - Scroll control
			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollController",
				CSharpType = "ScrollController",
				DartStructType = "IntPtr",
				DartParserFunction = "parseScrollController",
				Package = "flutter/widgets"
			});

			// ScrollPhysics - Scroll physics
			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollPhysics",
				CSharpType = "ScrollPhysics",
				DartStructType = "IntPtr",
				DartParserFunction = "parseScrollPhysics",
				Package = "flutter/widgets"
			});

			// ScrollBehavior - Scroll behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollBehavior",
				CSharpType = "ScrollBehavior",
				DartStructType = "IntPtr",
				DartParserFunction = "parseScrollBehavior",
				Package = "flutter/widgets"
			});

			// RouteSettings - Navigation route settings
			RegisterMapping(new TypeMapping
			{
				DartType = "RouteSettings",
				CSharpType = "RouteSettings",
				DartStructType = "IntPtr",
				DartParserFunction = "parseRouteSettings",
				Package = "flutter/widgets"
			});

			// Intent - Shortcut/action intent
			RegisterMapping(new TypeMapping
			{
				DartType = "Intent",
				CSharpType = "Intent",
				DartStructType = "IntPtr",
				DartParserFunction = "parseIntent",
				Package = "flutter/widgets"
			});

			// StackTrace - Dart stack trace
			RegisterMapping(new TypeMapping
			{
				DartType = "StackTrace",
				CSharpType = "StackTrace",
				DartStructType = "IntPtr",
				DartParserFunction = "parseStackTrace",
				Package = "dart:core"
			});

			// ScrollViewKeyboardDismissBehavior - Enum for keyboard dismiss behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "ScrollViewKeyboardDismissBehavior",
				CSharpType = "ScrollViewKeyboardDismissBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseScrollViewKeyboardDismissBehavior",
				IsEnum = true,
				Package = "flutter/widgets"
			});

			// DragStartBehavior - Enum for drag start behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "DragStartBehavior",
				CSharpType = "DragStartBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseDragStartBehavior",
				IsEnum = true,
				Package = "flutter/gestures"
			});

			// Clip - Enum for clipping behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "Clip",
				CSharpType = "Clip",
				DartStructType = "Int32",
				DartParserFunction = "parseClip",
				IsEnum = true,
				Package = "dart:ui"
			});

			// HitTestBehavior - Enum for hit test behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "HitTestBehavior",
				CSharpType = "HitTestBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parseHitTestBehavior",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// ImageRepeat - Enum for image repeat
			RegisterMapping(new TypeMapping
			{
				DartType = "ImageRepeat",
				CSharpType = "ImageRepeat",
				DartStructType = "Int32",
				DartParserFunction = "parseImageRepeat",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// FilterQuality - Enum for filter quality
			RegisterMapping(new TypeMapping
			{
				DartType = "FilterQuality",
				CSharpType = "FilterQuality",
				DartStructType = "Int32",
				DartParserFunction = "parseFilterQuality",
				IsEnum = true,
				Package = "dart:ui"
			});

			// BlendMode - Enum for blend mode
			RegisterMapping(new TypeMapping
			{
				DartType = "BlendMode",
				CSharpType = "BlendMode",
				DartStructType = "Int32",
				DartParserFunction = "parseBlendMode",
				IsEnum = true,
				Package = "dart:ui"
			});

			// BoxFit - Enum for box fit
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxFit",
				CSharpType = "BoxFit",
				DartStructType = "Int32",
				DartParserFunction = "parseBoxFit",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// ImageProvider - Image provider base class
			RegisterMapping(new TypeMapping
			{
				DartType = "ImageProvider",
				CSharpType = "ImageProvider",
				DartStructType = "IntPtr",
				DartParserFunction = "parseImageProvider",
				Package = "flutter/painting"
			});

			// Animation - Animation base class
			RegisterMapping(new TypeMapping
			{
				DartType = "Animation",
				CSharpType = "Animation",
				DartStructType = "IntPtr",
				DartParserFunction = "parseAnimation",
				Package = "flutter/animation"
			});

			// Listenable - Listenable interface
			RegisterMapping(new TypeMapping
			{
				DartType = "Listenable",
				CSharpType = "Listenable",
				DartStructType = "IntPtr",
				DartParserFunction = "parseListenable",
				Package = "flutter/foundation"
			});

			// ValueListenable - Value listenable
			RegisterMapping(new TypeMapping
			{
				DartType = "ValueListenable",
				CSharpType = "ValueListenable",
				DartStructType = "IntPtr",
				DartParserFunction = "parseValueListenable",
				Package = "flutter/foundation"
			});

			// FlexFit - Enum for flexible widget fit behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "FlexFit",
				CSharpType = "FlexFit",
				DartStructType = "Int32",
				DartParserFunction = "parseFlexFit",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// BoxHeightStyle - Enum for text selection height style
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxHeightStyle",
				CSharpType = "BoxHeightStyle",
				DartStructType = "Int32",
				DartParserFunction = "parseBoxHeightStyle",
				IsEnum = true,
				Package = "dart:ui"
			});

			// BoxWidthStyle - Enum for text selection width style
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxWidthStyle",
				CSharpType = "BoxWidthStyle",
				DartStructType = "Int32",
				DartParserFunction = "parseBoxWidthStyle",
				IsEnum = true,
				Package = "dart:ui"
			});

			// BoxShape - Enum for decoration shape
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxShape",
				CSharpType = "BoxShape",
				DartStructType = "Int32",
				DartParserFunction = "parseBoxShape",
				IsEnum = true,
				Package = "flutter/painting"
			});

			// BoxBorder - Base class for box border
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxBorder",
				CSharpType = "BoxBorder",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseBoxBorder",
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// StrutStyle - Text strut style
			RegisterMapping(new TypeMapping
			{
				DartType = "StrutStyle",
				CSharpType = "StrutStyle",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseStrutStyle",
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// TextHeightBehavior - Text height behavior
			RegisterMapping(new TypeMapping
			{
				DartType = "TextHeightBehavior",
				CSharpType = "TextHeightBehavior",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTextHeightBehavior",
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// InlineSpan - Base class for text spans
			RegisterMapping(new TypeMapping
			{
				DartType = "InlineSpan",
				CSharpType = "InlineSpan",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseInlineSpan",
				Package = "flutter/painting",
				RequiresCustomMarshalling = true
			});

			// SliverGridDelegate - Grid delegate for slivers
			RegisterMapping(new TypeMapping
			{
				DartType = "SliverGridDelegate",
				CSharpType = "SliverGridDelegate",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseSliverGridDelegate",
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			// LayerLink - Layer link for composited transforms
			RegisterMapping(new TypeMapping
			{
				DartType = "LayerLink",
				CSharpType = "LayerLink",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseLayerLink",
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			// ImageFilter - Image filter for visual effects
			RegisterMapping(new TypeMapping
			{
				DartType = "ImageFilter",
				CSharpType = "ImageFilter",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseImageFilter",
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// ColorFilter - Color filter for visual effects
			RegisterMapping(new TypeMapping
			{
				DartType = "ColorFilter",
				CSharpType = "ColorFilter",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseColorFilter",
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// AssetBundle - Asset bundle for resource loading
			RegisterMapping(new TypeMapping
			{
				DartType = "AssetBundle",
				CSharpType = "AssetBundle",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseAssetBundle",
				Package = "flutter/services",
				RequiresCustomMarshalling = true
			});

			// FlutterView - Flutter view
			RegisterMapping(new TypeMapping
			{
				DartType = "FlutterView",
				CSharpType = "FlutterView",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseFlutterView",
				Package = "dart:ui",
				RequiresCustomMarshalling = true
			});

			// PlatformViewHitTestBehavior - Enum for platform view hit testing
			RegisterMapping(new TypeMapping
			{
				DartType = "PlatformViewHitTestBehavior",
				CSharpType = "PlatformViewHitTestBehavior",
				DartStructType = "Int32",
				DartParserFunction = "parsePlatformViewHitTestBehavior",
				IsEnum = true,
				Package = "flutter/rendering"
			});

			// BoxConstraintsTransform - Transform function type for constraints
			RegisterMapping(new TypeMapping
			{
				DartType = "BoxConstraintsTransform",
				CSharpType = "Func<BoxConstraints, BoxConstraints>",
				DartStructType = "Pointer<Utf8>",
				DartParserFunction = "parseBoxConstraintsTransform",
				Package = "flutter/rendering",
				RequiresCustomMarshalling = true
			});

			// Tween - Animation tween
			RegisterMapping(new TypeMapping
			{
				DartType = "Tween",
				CSharpType = "Tween",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseTween",
				IsGeneric = true,
				Package = "flutter/animation",
				RequiresCustomMarshalling = true
			});
		}
	}
}
