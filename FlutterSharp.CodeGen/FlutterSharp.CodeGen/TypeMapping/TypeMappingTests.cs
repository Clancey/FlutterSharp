using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.TypeMapping
{
	/// <summary>
	/// Unit tests for the type mapping system.
	/// These are example tests that can be adapted to your testing framework (xUnit, NUnit, MSTest).
	/// </summary>
	/// <remarks>
	/// To use these tests, add your preferred testing framework package and uncomment the attributes.
	/// Example for xUnit:
	///   - Add package: Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio
	///   - Uncomment [Fact] attributes
	/// </remarks>
	public class TypeMappingTests
	{
		// [Fact]
		public void TypeMapping_ShouldMapPrimitiveTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("string", mapper.MapType("String"));
			AssertEqual("int", mapper.MapType("int"));
			AssertEqual("double", mapper.MapType("double"));
			AssertEqual("bool", mapper.MapType("bool"));
		}

		// [Fact]
		public void TypeMapping_ShouldHandleNullableTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("int?", mapper.MapType("int?"));
			AssertEqual("string?", mapper.MapType("String?"));
			AssertEqual("double?", mapper.MapType("double?"));
		}

		// [Fact]
		public void TypeMapping_ShouldMapGenericTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("List<string>", mapper.MapType("List<String>"));
			AssertEqual("Dictionary<string, int>", mapper.MapType("Map<String, int>"));
			AssertEqual("HashSet<double>", mapper.MapType("Set<double>"));
		}

		// [Fact]
		public void TypeMapping_ShouldHandleNestedGenerics()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("List<List<string>>", mapper.MapType("List<List<String>>"));
			AssertEqual("Dictionary<string, List<int>>", mapper.MapType("Map<String, List<int>>"));
		}

		// [Fact]
		public void TypeMapping_ShouldMapWidgetTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("Widget", mapper.MapType("Widget"));
			AssertEqual("StatelessWidget", mapper.MapType("StatelessWidget"));
			AssertEqual("StatefulWidget", mapper.MapType("StatefulWidget"));
		}

		// [Fact]
		public void TypeMapping_ShouldIdentifyWidgets()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertTrue(mapper.IsWidget("Widget"));
			AssertTrue(mapper.IsWidget("StatelessWidget"));
			AssertFalse(mapper.IsWidget("String"));
			AssertFalse(mapper.IsWidget("int"));
		}

		// [Fact]
		public void TypeMapping_ShouldIdentifyCollections()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertTrue(mapper.IsCollection("List<String>"));
			AssertTrue(mapper.IsCollection("Map<String, int>"));
			AssertTrue(mapper.IsCollection("Set<double>"));
			AssertFalse(mapper.IsCollection("String"));
			AssertFalse(mapper.IsCollection("Widget"));
		}

		// [Fact]
		public void TypeMapping_ShouldIdentifyEnums()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertTrue(mapper.IsEnum("TextAlign"));
			AssertTrue(mapper.IsEnum("MainAxisAlignment"));
			AssertFalse(mapper.IsEnum("String"));
			AssertFalse(mapper.IsEnum("Widget"));
		}

		// [Fact]
		public void FfiMapper_ShouldMapToFfiTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act & Assert
			AssertEqual("Pointer<Utf8>", ffiMapper.MapToFfiType("string"));
			AssertEqual("Int32", ffiMapper.MapToFfiType("int"));
			AssertEqual("Double", ffiMapper.MapToFfiType("double"));
			AssertEqual("Bool", ffiMapper.MapToFfiType("bool"));
		}

		// [Fact]
		public void FfiMapper_ShouldProvideParserFunctions()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act & Assert
			AssertEqual("parseString", ffiMapper.GetParserFunction("string"));
			AssertEqual("parseInt", ffiMapper.GetParserFunction("int"));
			AssertEqual("parseDouble", ffiMapper.GetParserFunction("double"));
			AssertEqual("parseBool", ffiMapper.GetParserFunction("bool"));
		}

		// [Fact]
		public void FfiMapper_ShouldGenerateConversions()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act
			var stringConversion = ffiMapper.GetCSharpToDartConversion("string", "myValue");
			var intConversion = ffiMapper.GetCSharpToDartConversion("int", "count");
			var boolConversion = ffiMapper.GetCSharpToDartConversion("bool", "flag");

			// Assert
			AssertEqual("myValue.ToNativeUtf8()", stringConversion);
			AssertEqual("count", intConversion);
			AssertEqual("(flag ? 1 : 0)", boolConversion);
		}

		// [Fact]
		public void FfiMapper_ShouldIdentifyCustomMarshalling()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act & Assert
			AssertTrue(ffiMapper.RequiresCustomMarshalling("Widget"));
			AssertTrue(ffiMapper.RequiresCustomMarshalling("List<string>"));
			AssertFalse(ffiMapper.RequiresCustomMarshalling("int"));
			AssertFalse(ffiMapper.RequiresCustomMarshalling("string"));
		}

		// [Fact]
		public void FfiMapper_ShouldProvideTypeSizes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act & Assert
			AssertEqual(4, ffiMapper.GetFfiTypeSize("int"));
			AssertEqual(8, ffiMapper.GetFfiTypeSize("double"));
			AssertEqual(1, ffiMapper.GetFfiTypeSize("bool"));
			AssertEqual(1, ffiMapper.GetFfiTypeSize("byte"));
		}

		// [Fact]
		public void FfiMapper_ShouldIdentifyValueTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var ffiMapper = new CSharpToDartFfiMapper(registry);

			// Act & Assert
			AssertTrue(ffiMapper.IsPassedByValue("int"));
			AssertTrue(ffiMapper.IsPassedByValue("double"));
			AssertTrue(ffiMapper.IsPassedByValue("bool"));
			AssertFalse(ffiMapper.IsPassedByValue("string"));
			AssertFalse(ffiMapper.IsPassedByValue("Widget"));
		}

		// [Fact]
		public void Registry_ShouldAllowCustomMappings()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var customMapping = new TypeMapping
			{
				DartType = "CustomType",
				CSharpType = "CustomType",
				DartStructType = "Pointer<Void>",
				DartParserFunction = "parseCustomType",
				IsCustomType = true
			};

			// Act
			registry.RegisterMapping(customMapping);
			var retrieved = registry.GetMapping("CustomType");

			// Assert
			AssertNotNull(retrieved);
			AssertEqual("CustomType", retrieved?.DartType);
			AssertEqual("CustomType", retrieved?.CSharpType);
			AssertTrue(retrieved?.IsCustomType ?? false);
		}

		// [Fact]
		public void Registry_ShouldProvideTypedQueries()
		{
			// Arrange
			var registry = new TypeMappingRegistry();

			// Act
			var widgets = registry.GetWidgetMappings().ToList();
			var primitives = registry.GetPrimitiveMappings().ToList();
			var collections = registry.GetCollectionMappings().ToList();

			// Assert
			AssertTrue(widgets.Count > 0);
			AssertTrue(primitives.Count > 0);
			AssertTrue(collections.Count > 0);
			AssertTrue(widgets.All(m => m.IsWidget));
			AssertTrue(primitives.All(m => m.IsPrimitive));
			AssertTrue(collections.All(m => m.IsCollection));
		}

		// [Fact]
		public void TypeMapping_ShouldSupportNullableConversions()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapping = registry.GetMapping("String");

			// Act
			var nullable = mapping?.AsNullable();
			var nonNullable = nullable?.AsNonNullable();

			// Assert
			AssertNotNull(nullable);
			AssertTrue(nullable?.IsNullable ?? false);
			AssertNotNull(nonNullable);
			AssertFalse(nonNullable?.IsNullable ?? true);
		}

		// [Fact]
		public void TypeMapping_ShouldHandleFlutterTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("Color", mapper.MapType("Color"));
			AssertEqual("EdgeInsets", mapper.MapType("EdgeInsets"));
			AssertEqual("TextStyle", mapper.MapType("TextStyle"));
			AssertEqual("Alignment", mapper.MapType("Alignment"));
		}

		// [Fact]
		public void TypeMapping_ShouldHandleCallbackTypes()
		{
			// Arrange
			var registry = new TypeMappingRegistry();
			var mapper = new DartToCSharpMapper(registry);

			// Act & Assert
			AssertEqual("Action", mapper.MapType("VoidCallback"));
			AssertEqual("Action", mapper.MapType("ValueChanged"));
			AssertEqual("Func", mapper.MapType("ValueGetter"));
		}

		#region Helper Methods

		/// <summary>
		/// Asserts that two values are equal.
		/// Replace with your testing framework's assertion method.
		/// </summary>
		private static void AssertEqual<T>(T expected, T actual)
		{
			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				throw new Exception($"Expected '{expected}' but got '{actual}'");
			}
		}

		/// <summary>
		/// Asserts that a value is true.
		/// Replace with your testing framework's assertion method.
		/// </summary>
		private static void AssertTrue(bool condition)
		{
			if (!condition)
			{
				throw new Exception("Expected true but got false");
			}
		}

		/// <summary>
		/// Asserts that a value is false.
		/// Replace with your testing framework's assertion method.
		/// </summary>
		private static void AssertFalse(bool condition)
		{
			if (condition)
			{
				throw new Exception("Expected false but got true");
			}
		}

		/// <summary>
		/// Asserts that a value is not null.
		/// Replace with your testing framework's assertion method.
		/// </summary>
		private static void AssertNotNull(object? value)
		{
			if (value == null)
			{
				throw new Exception("Expected non-null value but got null");
			}
		}

		#endregion

		/// <summary>
		/// Runs all tests manually (for demonstration purposes).
		/// In a real test suite, your test runner would execute these.
		/// </summary>
		public static void RunAllTests()
		{
			var tests = new TypeMappingTests();
			var testMethods = typeof(TypeMappingTests)
				.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
				.Where(m => m.Name.StartsWith("TypeMapping_") || m.Name.StartsWith("FfiMapper_") || m.Name.StartsWith("Registry_"))
				.ToList();

			var passed = 0;
			var failed = 0;

			Console.WriteLine($"Running {testMethods.Count} tests...\n");

			foreach (var method in testMethods)
			{
				try
				{
					method.Invoke(tests, null);
					Console.WriteLine($"✓ {method.Name}");
					passed++;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"✗ {method.Name}");
					Console.WriteLine($"  Error: {ex.InnerException?.Message ?? ex.Message}");
					failed++;
				}
			}

			Console.WriteLine($"\nResults: {passed} passed, {failed} failed");
		}
	}
}
