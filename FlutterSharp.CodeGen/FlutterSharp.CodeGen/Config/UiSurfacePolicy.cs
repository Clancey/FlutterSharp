using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Defines the allow/deny rules used to filter the raw analyzed graph down to the UI surface.
	/// </summary>
	public record UiSurfacePolicy
	{
		/// <summary>
		/// Gets the library prefixes considered eligible UI support surfaces.
		/// </summary>
		public List<string> UiLibraryPrefixes { get; init; } = new();

		/// <summary>
		/// Gets the denied library prefixes.
		/// </summary>
		public List<string> DeniedLibraryPrefixes { get; init; } = new();

		/// <summary>
		/// Gets explicitly allowed surface names.
		/// </summary>
		public List<string> AllowedNames { get; init; } = new();

		/// <summary>
		/// Gets the baseline support names that should always participate in the UI surface.
		/// </summary>
		public List<string> SeedNames { get; init; } = new();

		/// <summary>
		/// Gets explicitly denied surface names.
		/// </summary>
		public List<string> DeniedNames { get; init; } = new();

		/// <summary>
		/// Gets denied surface name suffixes.
		/// </summary>
		public List<string> DeniedNameSuffixes { get; init; } = new();

		/// <summary>
		/// Gets the supplemental type catalogs that should participate in reachability.
		/// </summary>
		public List<string> SupplementalTypeCatalogs { get; init; } = new();
	}
}
