using System.Collections.Generic;
using System.Linq;

namespace FlutterSharp.CodeGen.Models
{
	/// <summary>
	/// Represents a Flutter widget definition extracted from Dart code.
	/// </summary>
	public record WidgetDefinition
	{
		/// <summary>
		/// Gets the name of the widget.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// Gets the namespace/library path for this widget.
		/// </summary>
		public string Namespace { get; init; } = string.Empty;

		/// <summary>
		/// Gets the base widget class (e.g., StatelessWidget, StatefulWidget, Widget).
		/// </summary>
		public string? BaseClass { get; init; }

		/// <summary>
		/// Gets a value indicating the widget type category.
		/// </summary>
		public WidgetType Type { get; init; }

		/// <summary>
		/// Gets the properties/parameters for this widget.
		/// </summary>
		public List<PropertyDefinition> Properties { get; init; } = new();

		/// <summary>
		/// Gets the constructors for this widget.
		/// </summary>
		public List<ConstructorDefinition> Constructors { get; init; } = new();

		/// <summary>
		/// Gets the documentation comment for this widget.
		/// </summary>
		public string? Documentation { get; init; }

		/// <summary>
		/// Gets the source library/package this widget comes from.
		/// </summary>
		public string? SourceLibrary { get; init; }

		/// <summary>
		/// Gets a value indicating whether this widget supports a single child.
		/// </summary>
		public bool HasSingleChild { get; init; }

		/// <summary>
		/// Gets a value indicating whether this widget supports multiple children.
		/// </summary>
		public bool HasMultipleChildren { get; init; }

		/// <summary>
		/// Gets the name of the child property if HasSingleChild is true.
		/// </summary>
		public string? ChildPropertyName { get; init; }

		/// <summary>
		/// Gets the name of the children property if HasMultipleChildren is true.
		/// </summary>
		public string? ChildrenPropertyName { get; init; }

		/// <summary>
		/// Gets a value indicating whether this widget is abstract.
		/// </summary>
		public bool IsAbstract { get; init; }

		/// <summary>
		/// Gets a value indicating whether this widget is deprecated.
		/// </summary>
		public bool IsDeprecated { get; init; }

		/// <summary>
		/// Gets the deprecation message if this widget is deprecated.
		/// </summary>
		public string? DeprecationMessage { get; init; }

		/// <summary>
		/// Gets the generic type parameters if this is a generic widget.
		/// </summary>
		public List<string>? TypeParameters { get; init; }

		/// <summary>
		/// Gets a value indicating whether this is a render object widget.
		/// </summary>
		public bool IsRenderObjectWidget { get; init; }

		/// <summary>
		/// Gets additional metadata about this widget.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; init; }

		/// <summary>
		/// Gets the Flutter package version this widget is from.
		/// </summary>
		public string? FlutterVersion { get; init; }

		/// <summary>
		/// Returns a string representation of this widget definition.
		/// </summary>
		public override string ToString()
		{
			var baseClassInfo = BaseClass != null ? $" : {BaseClass}" : "";
			var childInfo = HasSingleChild ? " (single child)" :
							HasMultipleChildren ? " (multiple children)" : "";
			var propertyCount = Properties?.Count ?? 0;
			return $"{Name}{baseClassInfo}{childInfo} ({propertyCount} properties)";
		}
	}

	/// <summary>
	/// Represents the category/type of a widget.
	/// </summary>
	public enum WidgetType
	{
		/// <summary>
		/// Unknown or unclassified widget type.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// A stateless widget.
		/// </summary>
		Stateless = 1,

		/// <summary>
		/// A stateful widget.
		/// </summary>
		Stateful = 2,

		/// <summary>
		/// A render object widget with a single child.
		/// </summary>
		SingleChildRenderObject = 3,

		/// <summary>
		/// A render object widget with multiple children.
		/// </summary>
		MultiChildRenderObject = 4,

		/// <summary>
		/// A proxy widget (like InheritedWidget).
		/// </summary>
		Proxy = 5,

		/// <summary>
		/// A leaf render object widget (no children).
		/// </summary>
		LeafRenderObject = 6,

		/// <summary>
		/// Base widget class.
		/// </summary>
		Widget = 7
	}
}
