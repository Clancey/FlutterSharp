using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlutterSharp.CodeGen.Models;

namespace FlutterSharp.CodeGen.Analysis
{
	/// <summary>
	/// Example usage of the DartAnalyzerHost class.
	/// This file demonstrates how to use the analyzer to extract Dart definitions.
	/// </summary>
	public static class DartAnalyzerHostExample
	{
		/// <summary>
		/// Example: Analyze a complete Flutter package.
		/// </summary>
		public static async Task AnalyzePackageExample()
		{
			var analyzer = new DartAnalyzerHost();

			// Validate that Dart tools are available
			await analyzer.ValidateDartToolsAsync();

			// Analyze a Flutter package
			var packagePath = "/path/to/flutter/package";
			var result = await analyzer.AnalyzePackageAsync(packagePath);

			Console.WriteLine($"Package: {result.Name} v{result.Version}");
			Console.WriteLine($"Widgets found: {result.Widgets.Count}");
			Console.WriteLine($"Types found: {result.Types.Count}");
			Console.WriteLine($"Enums found: {result.Enums.Count}");

			// Print widget details
			foreach (var widget in result.Widgets)
			{
				Console.WriteLine($"\nWidget: {widget.Name}");
				Console.WriteLine($"  Type: {widget.Type}");
				Console.WriteLine($"  Base Class: {widget.BaseClass}");
				Console.WriteLine($"  Properties: {widget.Properties.Count}");

				if (widget.HasSingleChild)
				{
					Console.WriteLine($"  Has single child: {widget.ChildPropertyName}");
				}

				if (widget.HasMultipleChildren)
				{
					Console.WriteLine($"  Has multiple children: {widget.ChildrenPropertyName}");
				}
			}
		}

		/// <summary>
		/// Example: Analyze with include/exclude filters.
		/// </summary>
		public static async Task AnalyzeWithFiltersExample()
		{
			var analyzer = new DartAnalyzerHost();

			var packagePath = "/path/to/flutter/package";

			// Only analyze specific widget types
			var includeTypes = new List<string> { "Container", "Padding", "Center" };

			// Exclude certain types
			var excludeTypes = new List<string> { "DeprecatedWidget" };

			var result = await analyzer.AnalyzePackageAsync(
				packagePath,
				includeTypes,
				excludeTypes);

			Console.WriteLine($"Filtered analysis found {result.Widgets.Count} widgets");
		}

		/// <summary>
		/// Example: Analyze a single Dart file.
		/// </summary>
		public static async Task AnalyzeSingleFileExample()
		{
			var analyzer = new DartAnalyzerHost();

			var filePath = "/path/to/flutter/package/lib/widgets/my_widget.dart";

			var result = await analyzer.AnalyzeFileAsync(filePath);

			Console.WriteLine($"File analysis complete for package: {result.Name}");
		}

		/// <summary>
		/// Example: Custom analyzer with custom timeout and script path.
		/// </summary>
		public static async Task CustomAnalyzerExample()
		{
			// Create analyzer with custom configuration
			var analyzer = new DartAnalyzerHost(
				analyzerScriptPath: "custom/path/to/analyzer/script.dart",
				timeoutSeconds: 600); // 10 minutes timeout

			await analyzer.ValidateDartToolsAsync();

			var packagePath = "/path/to/large/flutter/package";
			var result = await analyzer.AnalyzePackageAsync(packagePath);

			Console.WriteLine($"Analysis complete: {result}");
		}

		/// <summary>
		/// Example: Error handling.
		/// </summary>
		public static async Task ErrorHandlingExample()
		{
			var analyzer = new DartAnalyzerHost();

			try
			{
				// Validate tools first
				await analyzer.ValidateDartToolsAsync();
			}
			catch (DartToolsNotFoundException ex)
			{
				Console.WriteLine($"Dart tools not found: {ex.Message}");
				Console.WriteLine("Please install the Dart SDK: https://dart.dev/get-dart");
				return;
			}

			try
			{
				var packagePath = "/path/to/package";
				var result = await analyzer.AnalyzePackageAsync(packagePath);
				Console.WriteLine($"Success: {result.Name}");
			}
			catch (DartAnalyzerException ex)
			{
				Console.WriteLine($"Analyzer error: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner error: {ex.InnerException.Message}");
				}
			}
		}

		/// <summary>
		/// Example: Custom logging by extending DartAnalyzerHost.
		/// </summary>
		public class CustomDartAnalyzer : DartAnalyzerHost
		{
			private readonly Action<string> _logger;

			public CustomDartAnalyzer(Action<string> logger)
			{
				_logger = logger;
			}

			protected override void LogDebug(string message)
			{
				_logger($"DEBUG: {message}");
			}

			protected override void LogWarning(string message)
			{
				_logger($"WARN: {message}");
			}
		}

		/// <summary>
		/// Example: Using custom logger.
		/// </summary>
		public static async Task CustomLoggingExample()
		{
			// Integrate with your logging framework
			var analyzer = new CustomDartAnalyzer(msg =>
			{
				// Use your preferred logging framework here
				Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
			});

			var result = await analyzer.AnalyzePackageAsync("/path/to/package");
			Console.WriteLine($"Analyzed: {result.Name}");
		}

		/// <summary>
		/// Example: Processing analysis results.
		/// </summary>
		public static async Task ProcessResultsExample()
		{
			var analyzer = new DartAnalyzerHost();
			var result = await analyzer.AnalyzePackageAsync("/path/to/package");

			// Find all stateless widgets
			var statelessWidgets = result.Widgets.FindAll(w => w.Type == WidgetType.Stateless);
			Console.WriteLine($"Stateless widgets: {statelessWidgets.Count}");

			// Find all widgets with children
			var containerWidgets = result.Widgets.FindAll(w =>
				w.HasSingleChild || w.HasMultipleChildren);
			Console.WriteLine($"Container widgets: {containerWidgets.Count}");

			// Find all deprecated items
			var deprecatedWidgets = result.Widgets.FindAll(w => w.IsDeprecated);
			var deprecatedTypes = result.Types.FindAll(t => t.IsDeprecated);
			Console.WriteLine($"Deprecated items: {deprecatedWidgets.Count + deprecatedTypes.Count}");

			// Process each widget
			foreach (var widget in result.Widgets)
			{
				// Find required properties
				var requiredProps = widget.Properties.FindAll(p => p.IsRequired);

				// Find callback properties
				var callbacks = widget.Properties.FindAll(p => p.IsCallback);

				Console.WriteLine($"{widget.Name}: {requiredProps.Count} required, {callbacks.Count} callbacks");
			}

			// Process enums
			foreach (var enumDef in result.Enums)
			{
				Console.WriteLine($"Enum {enumDef.Name} has {enumDef.Values.Count} values");
				foreach (var value in enumDef.Values)
				{
					var deprecated = value.IsDeprecated ? " (deprecated)" : "";
					Console.WriteLine($"  - {value.Name}{deprecated}");
				}
			}
		}
	}
}
