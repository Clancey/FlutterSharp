using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents metadata about a Dart/Flutter package.
	/// </summary>
	public record PackageDefinition
	{
		/// <summary>
		/// Gets the name of the package.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the version of the package.
		/// </summary>
		public string Version { get; init; } = string.Empty;

		/// <summary>
		/// Gets the description of the package.
		/// </summary>
		public string? Description { get; init; }

		/// <summary>
		/// Gets the homepage URL for the package.
		/// </summary>
		public string? Homepage { get; init; }

		/// <summary>
		/// Gets the repository URL for the package.
		/// </summary>
		public string? Repository { get; init; }

		/// <summary>
		/// Gets the list of authors for this package.
		/// </summary>
		public List<string>? Authors { get; init; }

		/// <summary>
		/// Gets the license information for this package.
		/// </summary>
		public string? License { get; init; }

		/// <summary>
		/// Gets the widgets discovered in this package.
		/// </summary>
		public List<WidgetDefinition> Widgets { get; init; } = new();

		/// <summary>
		/// Gets the types (classes) discovered in this package.
		/// </summary>
		public List<TypeDefinition> Types { get; init; } = new();

		/// <summary>
		/// Gets the enums discovered in this package.
		/// </summary>
		public List<EnumDefinition> Enums { get; init; } = new();

		/// <summary>
		/// Gets the package dependencies.
		/// </summary>
		public Dictionary<string, string>? Dependencies { get; init; }

		/// <summary>
		/// Gets the root library path for this package.
		/// </summary>
		public string? RootLibrary { get; init; }

		/// <summary>
		/// Gets the path to the package on the file system.
		/// </summary>
		public string? PackagePath { get; init; }

		/// <summary>
		/// Gets additional metadata about this package.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Gets the Flutter SDK version this package was analyzed with.
		/// </summary>
		public string? FlutterSdkVersion { get; init; }

		/// <summary>
		/// Gets the Dart SDK version this package was analyzed with.
		/// </summary>
		public string? DartSdkVersion { get; init; }

		/// <summary>
		/// Gets the timestamp when this package was analyzed.
		/// </summary>
		public string? AnalysisTimestamp { get; init; }

		/// <summary>
		/// Returns a string representation of this package definition.
		/// </summary>
		public override string ToString()
		{
			var widgetCount = Widgets?.Count ?? 0;
			var typeCount = Types?.Count ?? 0;
			var enumCount = Enums?.Count ?? 0;
			return $"{Name} v{Version} ({widgetCount} widgets, {typeCount} types, {enumCount} enums)";
		}
	}
}
