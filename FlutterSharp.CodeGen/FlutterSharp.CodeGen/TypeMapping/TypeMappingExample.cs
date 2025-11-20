using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Example class demonstrating the usage of the type mapping system.
	/// This class is for documentation and testing purposes.
	/// </summary>
	public static class TypeMappingExample
	{
		/// <summary>
		/// Demonstrates basic type mapping operations.
		/// </summary>
		public static void BasicExample()
		{
			Console.WriteLine("=== Basic Type Mapping Example ===\n");

			// Initialize the registry with default mappings
			var registry = new TypeMappingRegistry();
			var dartToCSharp = new DartToCSharpMapper(registry);
			var csharpToFfi = new CSharpToDartFfiMapper(registry);

			// Map primitive types
			var examples = new[]
			{
				"String", "int", "double", "bool", "int?", "String?",
				"List<String>", "Map<String, int>", "Widget",
				"Color", "EdgeInsets", "TextStyle"
			};

			foreach (var dartType in examples)
			{
				var csharpType = dartToCSharp.MapType(dartType);
				var ffiType = csharpToFfi.MapToFfiType(csharpType);
				var parser = csharpToFfi.GetParserFunction(csharpType);

				Console.WriteLine($"Dart: {dartType,-25} → C#: {csharpType,-25}");
				Console.WriteLine($"  FFI Type: {ffiType,-30} Parser: {parser ?? "N/A"}");
				Console.WriteLine();
			}
		}

		/// <summary>
		/// Demonstrates working with type mapping information.
		/// </summary>
		public static void DetailedMappingExample()
		{
			Console.WriteLine("=== Detailed Type Mapping Example ===\n");

			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Get detailed mapping information
			var typesToInspect = new[] { "String", "Widget", "List", "Color", "VoidCallback" };

			foreach (var dartType in typesToInspect)
			{
				var mapping = mapper.MapTypeWithInfo(dartType);
				if (mapping != null)
				{
					Console.WriteLine($"Type: {mapping.DartType}");
					Console.WriteLine($"  → C#: {mapping.CSharpType}");
					Console.WriteLine($"  → FFI: {mapping.DartStructType}");
					Console.WriteLine($"  → Parser: {mapping.DartParserFunction ?? "N/A"}");
					Console.WriteLine($"  → Flags: {GetTypeFlags(mapping)}");
					Console.WriteLine($"  → Package: {mapping.Package ?? "N/A"}");
					Console.WriteLine($"  → Custom Marshalling: {mapping.RequiresCustomMarshalling}");
					Console.WriteLine();
				}
			}
		}

		/// <summary>
		/// Demonstrates querying the registry.
		/// </summary>
		public static void QueryRegistryExample()
		{
			Console.WriteLine("=== Query Registry Example ===\n");

			var registry = new TypeMappingRegistry();

			// Get all widget types
			var widgets = registry.GetWidgetMappings();
			Console.WriteLine($"Widget Types ({widgets.Count()}):");
			foreach (var widget in widgets.Take(5))
			{
				Console.WriteLine($"  - {widget.DartType} ({widget.Package})");
			}
			Console.WriteLine();

			// Get all primitive types
			var primitives = registry.GetPrimitiveMappings();
			Console.WriteLine($"Primitive Types ({primitives.Count()}):");
			foreach (var primitive in primitives)
			{
				Console.WriteLine($"  - {primitive.DartType} → {primitive.CSharpType}");
			}
			Console.WriteLine();

			// Get all collection types
			var collections = registry.GetCollectionMappings();
			Console.WriteLine($"Collection Types ({collections.Count()}):");
			foreach (var collection in collections)
			{
				Console.WriteLine($"  - {collection.DartType} → {collection.CSharpType}");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates FFI conversion code generation.
		/// </summary>
		public static void FfiConversionExample()
		{
			Console.WriteLine("=== FFI Conversion Example ===\n");

			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			var conversions = new Dictionary<string, string>
			{
				["string"] = "myString",
				["int"] = "count",
				["double"] = "price",
				["bool"] = "isActive"
			};

			foreach (var (type, varName) in conversions)
			{
				Console.WriteLine($"Type: {type}, Variable: {varName}");

				var toFfi = ffiMapper.GetCSharpToDartConversion(type, varName);
				Console.WriteLine($"  C# → Dart FFI: {toFfi}");

				var fromFfi = ffiMapper.GetDartToCSharpConversion(type, $"{varName}Ptr");
				Console.WriteLine($"  Dart FFI → C#: {fromFfi}");

				var ffiType = ffiMapper.MapToFfiType(type);
				Console.WriteLine($"  FFI Type: {ffiType}");

				var size = ffiMapper.GetFfiTypeSize(type);
				if (size.HasValue)
				{
					Console.WriteLine($"  Size: {size.Value} bytes");
				}

				Console.WriteLine();
			}
		}

		/// <summary>
		/// Demonstrates registering custom type mappings.
		/// </summary>
		public static void CustomMappingExample()
		{
			Console.WriteLine("=== Custom Mapping Example ===\n");

			var registry = new TypeMappingRegistry();

			// Register a custom widget type
			registry.RegisterMapping(new TypeMapping
			{
				DartType = "MyCustomButton",
				CSharpType = "MyCustomButton",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseMyCustomButton",
				IsWidget = true,
				IsCustomType = true,
				Package = "my_app/widgets",
				RequiresCustomMarshalling = true,
				Metadata = new Dictionary<string, object>
				{
					["Author"] = "Your Name",
					["Version"] = "1.0.0",
					["Description"] = "A custom button widget"
				}
			});

			// Register a custom data type
			registry.RegisterMapping(new TypeMapping
			{
				DartType = "UserProfile",
				CSharpType = "UserProfile",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseUserProfile",
				IsCustomType = true,
				Package = "my_app/models",
				RequiresCustomMarshalling = true
			});

			// Use the custom mappings
			var mapper = new DartToCSharpMapper(registry);

			Console.WriteLine("Custom Widget:");
			var customWidget = mapper.MapTypeWithInfo("MyCustomButton");
			if (customWidget != null)
			{
				Console.WriteLine($"  {customWidget}");
				Console.WriteLine($"  Metadata: {string.Join(", ", customWidget.Metadata?.Select(kv => $"{kv.Key}={kv.Value}") ?? Array.Empty<string>())}");
			}
			Console.WriteLine();

			Console.WriteLine("Custom Data Type:");
			var customData = mapper.MapTypeWithInfo("UserProfile");
			if (customData != null)
			{
				Console.WriteLine($"  {customData}");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates handling complex generic types.
		/// </summary>
		public static void GenericTypeExample()
		{
			Console.WriteLine("=== Generic Type Example ===\n");

			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			var genericExamples = new[]
			{
				"List<String>",
				"List<Widget>",
				"Map<String, int>",
				"Map<String, List<Widget>>",
				"List<int?>",
				"ValueChanged<String>",
				"MaterialStateProperty<Color>",
				"ImageProvider<Object>"
			};

			foreach (var dartType in genericExamples)
			{
				var csharpType = mapper.MapType(dartType);
				var isCollection = mapper.IsCollection(dartType);

				Console.WriteLine($"Dart: {dartType}");
				Console.WriteLine($"  → C#: {csharpType}");
				Console.WriteLine($"  → Is Collection: {isCollection}");
				Console.WriteLine();
			}
		}

		/// <summary>
		/// Demonstrates type characteristic checks.
		/// </summary>
		public static void TypeCharacteristicsExample()
		{
			Console.WriteLine("=== Type Characteristics Example ===\n");

			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			var types = new[]
			{
				"Widget", "Container", "String", "int",
				"List<String>", "TextAlign", "VoidCallback"
			};

			foreach (var dartType in types)
			{
				Console.WriteLine($"Type: {dartType}");
				Console.WriteLine($"  Is Widget: {mapper.IsWidget(dartType)}");
				Console.WriteLine($"  Is Collection: {mapper.IsCollection(dartType)}");
				Console.WriteLine($"  Is Enum: {mapper.IsEnum(dartType)}");
				Console.WriteLine($"  Has Mapping: {mapper.HasMapping(dartType)}");
				Console.WriteLine();
			}
		}

		/// <summary>
		/// Runs all examples.
		/// </summary>
		public static void RunAllExamples()
		{
			BasicExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			DetailedMappingExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			QueryRegistryExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			FfiConversionExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			CustomMappingExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			GenericTypeExample();
			Console.WriteLine("\n" + new string('=', 60) + "\n");

			TypeCharacteristicsExample();
		}

		/// <summary>
		/// Gets a string representation of type flags.
		/// </summary>
		private static string GetTypeFlags(TypeMapping mapping)
		{
			var flags = new List<string>();

			if (mapping.IsPrimitive) flags.Add("Primitive");
			if (mapping.IsWidget) flags.Add("Widget");
			if (mapping.IsCollection) flags.Add("Collection");
			if (mapping.IsEnum) flags.Add("Enum");
			if (mapping.IsGeneric) flags.Add("Generic");
			if (mapping.IsNullable) flags.Add("Nullable");
			if (mapping.IsCustomType) flags.Add("Custom");
			if (mapping.RequiresCustomMarshalling) flags.Add("CustomMarshalling");

			return flags.Count > 0 ? string.Join(", ", flags) : "None";
		}
	}
}
