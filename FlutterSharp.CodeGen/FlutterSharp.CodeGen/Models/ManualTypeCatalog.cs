using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a JSON catalog of manually curated UI support types.
	/// </summary>
	public class ManualTypeCatalog
	{
		/// <summary>
		/// Gets or sets the catalog types.
		/// </summary>
		public List<ManualTypeDefinition> Types { get; set; } = new();
	}

	/// <summary>
	/// Represents a manually curated support type definition.
	/// </summary>
	public class ManualTypeDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;
		public string? Documentation { get; set; }
		public List<PropertyDefinition> Properties { get; set; } = new();
		public bool IsAbstract { get; set; }
		public string? SourceLibrary { get; set; }
		public bool GenerateCSharpStruct { get; set; } = true;
		public bool GenerateDartStruct { get; set; } = true;
	}
}
