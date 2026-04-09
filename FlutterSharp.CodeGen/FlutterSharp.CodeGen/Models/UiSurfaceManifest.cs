using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents the filtered UI surface consumed by code generation.
	/// </summary>
	public record UiSurfaceManifest
	{
		/// <summary>
		/// Gets the package name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the package version.
		/// </summary>
		public string Version { get; init; } = string.Empty;

		/// <summary>
		/// Gets the package description.
		/// </summary>
		public string? Description { get; init; }

		/// <summary>
		/// Gets the package path.
		/// </summary>
		public string? PackagePath { get; init; }

		/// <summary>
		/// Gets the root library for the analyzed package.
		/// </summary>
		public string? RootLibrary { get; init; }

		/// <summary>
		/// Gets the raw analysis timestamp.
		/// </summary>
		public string? AnalysisTimestamp { get; init; }

		/// <summary>
		/// Gets the timestamp when the manifest was produced.
		/// </summary>
		public string? ManifestTimestamp { get; init; }

		/// <summary>
		/// Gets the source path to the UI surface policy file.
		/// </summary>
		public string? PolicyPath { get; init; }

		/// <summary>
		/// Gets the package widgets included in the UI surface.
		/// </summary>
		public List<WidgetDefinition> Widgets { get; init; } = new();

		/// <summary>
		/// Gets the support types included in the UI surface.
		/// </summary>
		public List<TypeDefinition> Types { get; init; } = new();

		/// <summary>
		/// Gets the enums included in the UI surface.
		/// </summary>
		public List<EnumDefinition> Enums { get; init; } = new();

		/// <summary>
		/// Gets the typedefs included in the UI surface.
		/// </summary>
		public List<TypedefDefinition> Typedefs { get; init; } = new();

		/// <summary>
		/// Gets surface-level exclusions recorded while building the manifest.
		/// </summary>
		public List<UiSurfaceExclusion> Exclusions { get; init; } = new();
	}

	/// <summary>
	/// Represents a filtered surface that was intentionally excluded.
	/// </summary>
	public record UiSurfaceExclusion
	{
		/// <summary>
		/// Gets the excluded surface name.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the excluded surface kind.
		/// </summary>
		public UiSurfaceKind Kind { get; init; }

		/// <summary>
		/// Gets the source library, when known.
		/// </summary>
		public string? SourceLibrary { get; init; }

		/// <summary>
		/// Gets the reason the surface was excluded.
		/// </summary>
		public string Reason { get; init; } = string.Empty;

		/// <summary>
		/// Gets the parent surface that referenced the excluded item, when applicable.
		/// </summary>
		public string? ReferencedBy { get; init; }

		/// <summary>
		/// Gets the originating policy rule, when applicable.
		/// </summary>
		public string? PolicyRule { get; init; }
	}

	/// <summary>
	/// Identifies a surface category in the UI manifest.
	/// </summary>
	public enum UiSurfaceKind
	{
		Widget = 0,
		Type = 1,
		Enum = 2,
		Typedef = 3
	}
}
