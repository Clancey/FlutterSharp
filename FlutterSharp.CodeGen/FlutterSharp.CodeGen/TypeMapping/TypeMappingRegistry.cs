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

			// bool
			RegisterMapping(new TypeMapping
			{
				DartType = "bool",
				CSharpType = "bool",
				DartStructType = "Bool",
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
				DartParserFunction = "parseWidget",
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
				DartParserFunction = "parseWidget",
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
				DartParserFunction = "parseWidget",
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
				DartParserFunction = "parseWidget",
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
				DartStructType = "Pointer<Void>",
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
			// VoidCallback
			RegisterMapping(new TypeMapping
			{
				DartType = "VoidCallback",
				CSharpType = "Action",
				DartStructType = "Pointer<NativeFunction<Void Function()>>",
				DartParserFunction = "parseVoidCallback",
				Package = "flutter/widgets",
				RequiresCustomMarshalling = true
			});

			// ValueChanged<T>
			RegisterMapping(new TypeMapping
			{
				DartType = "ValueChanged",
				CSharpType = "Action",
				DartStructType = "Pointer<NativeFunction<Void Function(Pointer<Void>)>>",
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
				DartStructType = "Pointer<NativeFunction<Void Function(Pointer<Void>)>>",
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
				DartStructType = "Pointer<NativeFunction<Pointer<Void> Function()>>",
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
				DartStructType = "Pointer<NativeFunction<Void Function()>>",
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
	}
}
