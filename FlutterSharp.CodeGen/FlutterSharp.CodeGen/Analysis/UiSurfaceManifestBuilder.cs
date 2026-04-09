using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using FlutterSharp.CodeGen.Config;
using FlutterSharp.CodeGen.Models;

namespace FlutterSharp.CodeGen.Analysis
{
	/// <summary>
	/// Builds a filtered UI surface manifest from the raw analyzed package model.
	/// </summary>
	public class UiSurfaceManifestBuilder
	{
		private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

		private static readonly HashSet<string> IgnoredTypeNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"bool", "double", "dynamic", "Function", "Future", "FutureOr", "int", "InvalidType", "Iterable", "List", "Map",
			"Never", "Null", "num", "Object", "Pointer", "Set", "String", "Utf8", "void",
			"Int8", "Int16", "Int32", "Int64", "Uint8", "Uint16", "Uint32", "Uint64", "Float", "Double",
			"Widget", "StatelessWidget", "StatefulWidget", "InheritedWidget", "RenderObjectWidget",
			"SingleChildRenderObjectWidget", "MultiChildRenderObjectWidget", "LeafRenderObjectWidget",
			"ProxyWidget", "ParentDataWidget",
			"T", "S", "U", "V", "K", "R"
		};

		private static readonly HashSet<string> PolymorphicSupportTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"AlignmentGeometry",
			"BorderRadiusGeometry",
			"BoxBorder",
			"Constraints",
			"Decoration",
			"EdgeInsetsGeometry",
			"Gradient"
		};

		private static readonly HashSet<string> SupportedParserCallbackTypes = new(StringComparer.Ordinal)
		{
			"VoidCallback",
			"GestureTapCallback",
			"GestureTapDownCallback",
			"GestureTapUpCallback",
			"GestureTapCancelCallback",
			"GestureTapMoveCallback",
			"GestureLongPressCallback",
			"GestureLongPressDownCallback",
			"GestureLongPressUpCallback",
			"GestureLongPressStartCallback",
			"GestureLongPressEndCallback",
			"GestureLongPressCancelCallback",
			"GestureLongPressMoveUpdateCallback",
			"GestureDragStartCallback",
			"GestureDragUpdateCallback",
			"GestureDragEndCallback",
			"GestureDragDownCallback",
			"GestureDragCancelCallback",
			"GestureScaleStartCallback",
			"GestureScaleUpdateCallback",
			"GestureScaleEndCallback",
			"GestureForcePressStartCallback",
			"GestureForcePressPeakCallback",
			"GestureForcePressUpdateCallback",
			"GestureForcePressEndCallback",
			"PointerDownEventListener",
			"PointerMoveEventListener",
			"PointerUpEventListener",
			"PointerCancelEventListener",
			"PointerEnterEventListener",
			"PointerExitEventListener",
			"PointerHoverEventListener",
			"PointerSignalEventListener",
			"PointerPanZoomStartEventListener",
			"PointerPanZoomUpdateEventListener",
			"PointerPanZoomEndEventListener",
			"PlatformViewCreatedCallback",
			"ShaderCallback"
		};

		private static readonly HashSet<string> ParserDeniedWidgetNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"AlignTransition", "DecoratedBoxTransition", "DefaultTextStyleTransition",
			"FadeTransition", "MatrixTransition", "PositionedTransition",
			"RelativePositionedTransition", "RotationTransition", "ScaleTransition",
			"SizeTransition", "SlideTransition", "SliverFadeTransition",
			"AnimatedBuilder", "AnimatedModalBarrier", "DualTransitionBuilder",
			"TweenAnimationBuilder", "ValueListenableBuilder",
			"EditableText", "FadeInImage", "FlutterLogo", "Image", "RawImage",
			"KeyboardListener", "RawKeyboardListener", "UndoHistory",
			"ListWheelViewport", "ShrinkWrappingViewport", "Viewport",
			"PrimaryScrollController", "WidgetsApp", "Table",
			"ImgElementPlatformView", "RawWebImage", "SystemContextMenu",
			"AndroidView", "HtmlElementView",
			"AnnotatedRegion", "SelectionContainer",
			"Icon", "ImageIcon", "Text", "RichText", "Dismissible",
			"LayoutId", "ListWheelScrollView", "PageView", "Semantics",
			"SliverConstrainedCrossAxis", "SliverSafeArea", "SliverVisibility", "Wrap",
			"ActionListener", "AnimatedGrid", "AnimatedList", "AnimatedSwitcher",
			"Builder", "ConstraintsTransformBox", "DraggableScrollableSheet",
			"Expansible", "Focus", "FocusScope", "LayoutBuilder", "ListenableBuilder",
			"MouseRegion", "NavigatorPopHandler", "NotificationListener",
			"OrientationBuilder", "OverlayPortal", "PlatformViewLink", "PopScope",
			"RawMenuAnchor", "ReorderableList", "ShaderMask",
			"SliverAnimatedGrid", "SliverAnimatedList", "SliverLayoutBuilder",
			"SliverReorderableList", "SliverVariedExtentList", "StatefulBuilder",
			"TapRegion", "TextFieldTapRegion", "TreeSliver", "WillPopScope",
			"SingleChildScrollView"
		};

		private static readonly Regex TypeTokenRegex = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

		private readonly Action<string>? _logWarning;

		public UiSurfaceManifestBuilder(Action<string>? logWarning = null)
		{
			_logWarning = logWarning;
		}

		/// <summary>
		/// Builds the manifest from the analyzed package and policy.
		/// </summary>
		public UiSurfaceManifest Build(PackageDefinition packageDefinition, UiSurfacePolicy policy, string policyPath)
		{
			if (packageDefinition == null)
			{
				throw new ArgumentNullException(nameof(packageDefinition));
			}

			if (policy == null)
			{
				throw new ArgumentNullException(nameof(policy));
			}

			var widgetsByName = packageDefinition.Widgets
				.GroupBy(w => w.Name, NameComparer)
				.ToDictionary(group => group.Key, SelectPreferredWidgetDefinition, NameComparer);

			var typesByName = packageDefinition.Types
				.GroupBy(t => t.Name, NameComparer)
				.ToDictionary(group => group.Key, SelectPreferredTypeDefinition, NameComparer);

			foreach (var supplementalType in LoadSupplementalTypes(policy))
			{
				typesByName[supplementalType.Name] = supplementalType;
			}

			var enumsByName = packageDefinition.Enums
				.GroupBy(e => e.Name, NameComparer)
				.ToDictionary(group => group.Key, SelectPreferredEnumDefinition, NameComparer);

			var typedefsByName = packageDefinition.Typedefs
				.GroupBy(t => t.Name, NameComparer)
				.ToDictionary(group => group.Key, SelectPreferredTypedefDefinition, NameComparer);

			var derivedTypesByBase = typesByName.Values
				.Where(type => !string.IsNullOrWhiteSpace(type.BaseClass))
				.GroupBy(type => type.BaseClass!, NameComparer)
				.ToDictionary(group => group.Key, group => group.ToList(), NameComparer);

			var manifestWidgets = new List<WidgetDefinition>();
			var manifestTypes = new Dictionary<string, TypeDefinition>(NameComparer);
			var manifestEnums = new Dictionary<string, EnumDefinition>(NameComparer);
			var manifestTypedefs = new Dictionary<string, TypedefDefinition>(NameComparer);
			var exclusions = new Dictionary<string, UiSurfaceExclusion>(NameComparer);
			var queuedNames = new HashSet<string>(NameComparer);
			var queue = new Queue<(string Name, string ReferencedBy)>();

			foreach (var widget in widgetsByName.Values.OrderBy(w => w.Name, NameComparer))
			{
				if (TryExcludeSurface(widget.Name, widget.SourceLibrary, UiSurfaceKind.Widget, policy, out var reason, out var policyRule))
				{
					AddExclusion(exclusions, widget.Name, UiSurfaceKind.Widget, widget.SourceLibrary, reason!, null, policyRule);
					continue;
				}

				if (HasUnresolvedAnalyzerTypes(widget))
				{
					AddExclusion(exclusions, widget.Name, UiSurfaceKind.Widget, widget.SourceLibrary, "Contains unresolved analyzer type information.", null, "analyzer");
					continue;
				}

				var normalizedWidget = ApplyWidgetGenerationTargets(EnsureReferencedTypes(widget));
				manifestWidgets.Add(normalizedWidget);

				foreach (var reference in normalizedWidget.ReferencedTypes)
				{
					Enqueue(reference, normalizedWidget.Name, queuedNames, queue);
				}
			}

			foreach (var seedName in policy.SeedNames)
			{
				Enqueue(seedName, "policy", queuedNames, queue);
			}

			while (queue.Count > 0)
			{
				var (name, referencedBy) = queue.Dequeue();

				if (enumsByName.TryGetValue(name, out var enumDefinition))
				{
					if (manifestEnums.ContainsKey(enumDefinition.Name))
					{
						continue;
					}

					if (TryExcludeSupportSurface(enumDefinition.Name, enumDefinition.SourceLibrary, UiSurfaceKind.Enum, policy, out var reason, out var policyRule))
					{
						AddExclusion(exclusions, enumDefinition.Name, UiSurfaceKind.Enum, enumDefinition.SourceLibrary, reason!, referencedBy, policyRule);
						continue;
					}

					manifestEnums[enumDefinition.Name] = enumDefinition;
					continue;
				}

				if (typesByName.TryGetValue(name, out var typeDefinition))
				{
					if (manifestTypes.ContainsKey(typeDefinition.Name))
					{
						continue;
					}

					if (HasUnresolvedAnalyzerTypes(typeDefinition))
					{
						AddExclusion(exclusions, typeDefinition.Name, UiSurfaceKind.Type, typeDefinition.SourceLibrary, "Contains unresolved analyzer type information.", referencedBy, "analyzer");
						continue;
					}

					var normalizedType = ApplyTypeGenerationTargets(EnsureReferencedTypes(typeDefinition));
					if (TryExcludeSupportType(normalizedType, policy, out var reason, out var policyRule))
					{
						AddExclusion(exclusions, normalizedType.Name, UiSurfaceKind.Type, normalizedType.SourceLibrary, reason!, referencedBy, policyRule);
						continue;
					}

					manifestTypes[normalizedType.Name] = normalizedType;
					foreach (var reference in normalizedType.ReferencedTypes)
					{
						Enqueue(reference, normalizedType.Name, queuedNames, queue);
					}

					if (PolymorphicSupportTypes.Contains(normalizedType.Name) &&
					    derivedTypesByBase.TryGetValue(normalizedType.Name, out var derivedTypes))
					{
						foreach (var derivedType in derivedTypes)
						{
							Enqueue(derivedType.Name, normalizedType.Name, queuedNames, queue);
						}
					}
					continue;
				}

				if (typedefsByName.TryGetValue(name, out var typedefDefinition))
				{
					if (manifestTypedefs.ContainsKey(typedefDefinition.Name))
					{
						continue;
					}

					if (HasUnresolvedAnalyzerTypes(typedefDefinition))
					{
						AddExclusion(exclusions, typedefDefinition.Name, UiSurfaceKind.Typedef, typedefDefinition.SourceLibrary, "Contains unresolved analyzer type information.", referencedBy, "analyzer");
						continue;
					}

					var normalizedTypedef = EnsureReferencedTypes(typedefDefinition);
					if (TryExcludeSupportSurface(normalizedTypedef.Name, normalizedTypedef.SourceLibrary, UiSurfaceKind.Typedef, policy, out var reason, out var policyRule))
					{
						AddExclusion(exclusions, normalizedTypedef.Name, UiSurfaceKind.Typedef, normalizedTypedef.SourceLibrary, reason!, referencedBy, policyRule);
						continue;
					}

					manifestTypedefs[normalizedTypedef.Name] = normalizedTypedef;
					foreach (var reference in normalizedTypedef.ReferencedTypes)
					{
						Enqueue(reference, normalizedTypedef.Name, queuedNames, queue);
					}
					continue;
				}

				if (!IgnoredTypeNames.Contains(name))
				{
					AddExclusion(exclusions, name, UiSurfaceKind.Type, null, "Referenced type has no analyzed or manual definition.", referencedBy, null);
				}
			}

			return new UiSurfaceManifest
			{
				Name = packageDefinition.Name,
				Version = packageDefinition.Version,
				Description = packageDefinition.Description,
				PackagePath = packageDefinition.PackagePath,
				RootLibrary = packageDefinition.RootLibrary,
				AnalysisTimestamp = packageDefinition.AnalysisTimestamp,
				ManifestTimestamp = DateTime.UtcNow.ToString("O"),
				PolicyPath = policyPath,
				Widgets = manifestWidgets.OrderBy(w => w.Name, NameComparer).ToList(),
				Types = manifestTypes.Values.OrderBy(t => t.Name, NameComparer).ToList(),
				Enums = manifestEnums.Values.OrderBy(e => e.Name, NameComparer).ToList(),
				Typedefs = manifestTypedefs.Values.OrderBy(t => t.Name, NameComparer).ToList(),
				Exclusions = exclusions.Values
					.OrderBy(exclusion => exclusion.Kind)
					.ThenBy(exclusion => exclusion.Name, NameComparer)
					.ToList()
			};
		}

		private IEnumerable<TypeDefinition> LoadSupplementalTypes(UiSurfacePolicy policy)
		{
			foreach (var catalogPath in policy.SupplementalTypeCatalogs)
			{
				var resolvedPath = ResolveSupplementalCatalogPath(catalogPath);
				if (resolvedPath == null)
				{
					_logWarning?.Invoke($"Supplemental UI type catalog not found: {catalogPath}");
					continue;
				}

				var json = File.ReadAllText(resolvedPath);
				var catalog = JsonSerializer.Deserialize<ManualTypeCatalog>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				if (catalog?.Types == null)
				{
					continue;
				}

				foreach (var manualType in catalog.Types)
				{
					yield return EnsureReferencedTypes(new TypeDefinition
					{
						Name = manualType.Name,
						Namespace = manualType.Namespace,
						Documentation = manualType.Documentation,
						Properties = manualType.Properties ?? new List<PropertyDefinition>(),
						Constructors = new List<ConstructorDefinition>(),
						IsAbstract = manualType.IsAbstract,
						SourceLibrary = manualType.SourceLibrary,
						IsSupplemental = true,
						GenerateCSharpStruct = manualType.GenerateCSharpStruct,
						GenerateDartStruct = manualType.GenerateDartStruct
					});
				}
			}
		}

		private static string? ResolveSupplementalCatalogPath(string relativePath)
		{
			var baseDirCandidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
			if (File.Exists(baseDirCandidate))
			{
				return baseDirCandidate;
			}

			var currentDirCandidate = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
			if (File.Exists(currentDirCandidate))
			{
				return currentDirCandidate;
			}

			for (var current = new DirectoryInfo(Directory.GetCurrentDirectory()); current != null; current = current.Parent)
			{
				var candidate = Path.Combine(current.FullName, relativePath);
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}

			return null;
		}

		private static void Enqueue(string? name, string referencedBy, HashSet<string> queuedNames, Queue<(string Name, string ReferencedBy)> queue)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return;
			}

			var cleanedName = name.Trim();
			if (IgnoredTypeNames.Contains(cleanedName))
			{
				return;
			}

			if (!queuedNames.Add(cleanedName))
			{
				return;
			}

			queue.Enqueue((cleanedName, referencedBy));
		}

		private static bool TryExcludeSurface(string name, string? sourceLibrary, UiSurfaceKind kind, UiSurfacePolicy policy, out string? reason, out string? policyRule)
		{
			if (policy.AllowedNames.Contains(name, NameComparer))
			{
				reason = null;
				policyRule = null;
				return false;
			}

			if (policy.DeniedNames.Contains(name, NameComparer))
			{
				reason = "Denied by explicit policy name.";
				policyRule = "deniedNames";
				return true;
			}

			if (policy.DeniedNameSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
			{
				reason = "Denied by policy name suffix.";
				policyRule = "deniedNameSuffixes";
				return true;
			}

			if (MatchesAnyPrefix(sourceLibrary, policy.DeniedLibraryPrefixes))
			{
				reason = "Denied by policy library prefix.";
				policyRule = "deniedLibraryPrefixes";
				return true;
			}

			reason = null;
			policyRule = null;
			return false;
		}

		private static bool TryExcludeSupportSurface(string name, string? sourceLibrary, UiSurfaceKind kind, UiSurfacePolicy policy, out string? reason, out string? policyRule)
		{
			if (TryExcludeSurface(name, sourceLibrary, kind, policy, out reason, out policyRule))
			{
				return true;
			}

			if (!policy.AllowedNames.Contains(name, NameComparer) &&
			    !MatchesAnyPrefix(sourceLibrary, policy.UiLibraryPrefixes))
			{
				reason = "Source library is outside the configured UI surface.";
				policyRule = "uiLibraryPrefixes";
				return true;
			}

			reason = null;
			policyRule = null;
			return false;
		}

		private static bool TryExcludeSupportType(TypeDefinition typeDefinition, UiSurfacePolicy policy, out string? reason, out string? policyRule)
		{
			if (TryExcludeSupportSurface(typeDefinition.Name, typeDefinition.SourceLibrary, UiSurfaceKind.Type, policy, out reason, out policyRule))
			{
				return true;
			}

			if (policy.AllowedNames.Contains(typeDefinition.Name, NameComparer))
			{
				reason = null;
				policyRule = null;
				return false;
			}

			if (typeDefinition.IsImmutable || typeDefinition.IsAbstract || typeDefinition.Properties.Count > 0)
			{
				reason = null;
				policyRule = null;
				return false;
			}

			reason = "Reachable type is not value-like or explicitly allowed.";
			policyRule = "uiSupportTypeHeuristic";
			return true;
		}

		private static WidgetDefinition ApplyWidgetGenerationTargets(WidgetDefinition widget)
		{
			var generateDartStruct = !widget.IsAbstract;
			var generateDartParser = generateDartStruct && !ShouldSkipWidgetParser(widget);

			return widget with
			{
				GenerateCSharpWidget = true,
				GenerateCSharpStruct = true,
				GenerateDartStruct = generateDartStruct,
				GenerateDartParser = generateDartParser
			};
		}

		private static TypeDefinition ApplyTypeGenerationTargets(TypeDefinition typeDefinition)
		{
			if (typeDefinition.IsSupplemental)
			{
				return typeDefinition;
			}

			return typeDefinition with
			{
				GenerateDartStruct = typeDefinition.GenerateDartStruct && !typeDefinition.IsAbstract
			};
		}

		private static bool ShouldSkipWidgetParser(WidgetDefinition widget)
		{
			if (ParserDeniedWidgetNames.Contains(widget.Name))
			{
				return true;
			}

			var sourceLibrary = widget.SourceLibrary ?? string.Empty;
			if (sourceLibrary.Contains("material", StringComparison.OrdinalIgnoreCase)
				|| sourceLibrary.Contains("cupertino", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return HasUnsupportedCallbackSignature(widget);
		}

		private static bool HasUnsupportedCallbackSignature(WidgetDefinition widget)
		{
			foreach (var property in widget.Properties)
			{
				if (!property.IsCallback)
				{
					continue;
				}

				var callbackType = (property.DartType ?? string.Empty).Trim();
				if (callbackType.EndsWith("?", StringComparison.Ordinal))
				{
					callbackType = callbackType.Substring(0, callbackType.Length - 1).Trim();
				}

				if (string.IsNullOrEmpty(callbackType))
				{
					return true;
				}

				if (callbackType.Contains("Function(", StringComparison.Ordinal))
				{
					return true;
				}

				if (callbackType.StartsWith("ValueChanged<", StringComparison.Ordinal)
					|| callbackType.StartsWith("FormFieldValidator", StringComparison.Ordinal))
				{
					continue;
				}

				if (!SupportedParserCallbackTypes.Contains(callbackType))
				{
					return true;
				}
			}

			return false;
		}

		private static bool MatchesAnyPrefix(string? value, IEnumerable<string> prefixes)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			return prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}

		private static WidgetDefinition EnsureReferencedTypes(WidgetDefinition widget)
		{
			var references = new HashSet<string>(widget.ReferencedTypes ?? new List<string>(), NameComparer);
			AddReferenceFromTypeName(references, widget.BaseClass);

			foreach (var property in widget.Properties)
			{
				AddReferenceFromProperty(references, property);
			}

			foreach (var constructor in widget.Constructors)
			{
				foreach (var parameter in constructor.Parameters)
				{
					AddReferenceFromProperty(references, parameter);
				}
			}

			return widget with
			{
				ReferencedTypes = references.OrderBy(reference => reference, NameComparer).ToList()
			};
		}

		private static bool HasUnresolvedAnalyzerTypes(WidgetDefinition widget)
		{
			return widget.Properties.Any(HasUnresolvedAnalyzerTypes)
				|| widget.Constructors.SelectMany(constructor => constructor.Parameters).Any(HasUnresolvedAnalyzerTypes);
		}

		private static TypeDefinition EnsureReferencedTypes(TypeDefinition typeDefinition)
		{
			var references = new HashSet<string>(typeDefinition.ReferencedTypes ?? new List<string>(), NameComparer);
			AddReferenceFromTypeName(references, typeDefinition.BaseClass);

			foreach (var interfaceName in typeDefinition.Interfaces ?? Enumerable.Empty<string>())
			{
				AddReferenceFromTypeName(references, interfaceName);
			}

			foreach (var property in typeDefinition.Properties)
			{
				AddReferenceFromProperty(references, property);
			}

			foreach (var constructor in typeDefinition.Constructors)
			{
				foreach (var parameter in constructor.Parameters)
				{
					AddReferenceFromProperty(references, parameter);
				}
			}

			return typeDefinition with
			{
				ReferencedTypes = references.OrderBy(reference => reference, NameComparer).ToList()
			};
		}

		private static bool HasUnresolvedAnalyzerTypes(TypeDefinition typeDefinition)
		{
			return ContainsInvalidType(typeDefinition.BaseClass)
				|| (typeDefinition.Interfaces?.Any(ContainsInvalidType) ?? false)
				|| typeDefinition.Properties.Any(HasUnresolvedAnalyzerTypes)
				|| typeDefinition.Constructors.SelectMany(constructor => constructor.Parameters).Any(HasUnresolvedAnalyzerTypes);
		}

		private static TypedefDefinition EnsureReferencedTypes(TypedefDefinition typedefDefinition)
		{
			var references = new HashSet<string>(typedefDefinition.ReferencedTypes ?? new List<string>(), NameComparer);
			AddReferenceFromTypeName(references, typedefDefinition.AliasedType);
			AddReferenceFromTypeName(references, typedefDefinition.ReturnType);

			foreach (var parameter in typedefDefinition.Parameters)
			{
				AddReferenceFromProperty(references, parameter);
			}

			return typedefDefinition with
			{
				ReferencedTypes = references.OrderBy(reference => reference, NameComparer).ToList()
			};
		}

		private static bool HasUnresolvedAnalyzerTypes(TypedefDefinition typedefDefinition)
		{
			return ContainsInvalidType(typedefDefinition.AliasedType)
				|| ContainsInvalidType(typedefDefinition.ReturnType)
				|| typedefDefinition.Parameters.Any(HasUnresolvedAnalyzerTypes);
		}

		private static bool HasUnresolvedAnalyzerTypes(PropertyDefinition property)
		{
			return ContainsInvalidType(property.DartType)
				|| (property.TypeArguments?.Any(ContainsInvalidType) ?? false);
		}

		private static bool ContainsInvalidType(string? typeName)
		{
			return !string.IsNullOrWhiteSpace(typeName)
				&& typeName.Contains("InvalidType", StringComparison.Ordinal);
		}

		private static void AddReferenceFromProperty(HashSet<string> references, PropertyDefinition property)
		{
			AddReferenceFromTypeName(references, property.DartType);

			foreach (var typeArgument in property.TypeArguments ?? Enumerable.Empty<string>())
			{
				AddReferenceFromTypeName(references, typeArgument);
			}
		}

		private static void AddReferenceFromTypeName(HashSet<string> references, string? typeName)
		{
			if (string.IsNullOrWhiteSpace(typeName))
			{
				return;
			}

			foreach (Match match in TypeTokenRegex.Matches(typeName))
			{
				var token = match.Value;
				if (IgnoredTypeNames.Contains(token))
				{
					continue;
				}

				references.Add(token);
			}
		}

		private static void AddExclusion(
			IDictionary<string, UiSurfaceExclusion> exclusions,
			string name,
			UiSurfaceKind kind,
			string? sourceLibrary,
			string reason,
			string? referencedBy,
			string? policyRule)
		{
			var key = $"{kind}:{name}:{reason}";
			if (exclusions.ContainsKey(key))
			{
				return;
			}

			exclusions[key] = new UiSurfaceExclusion
			{
				Name = name,
				Kind = kind,
				SourceLibrary = sourceLibrary,
				Reason = reason,
				ReferencedBy = referencedBy,
				PolicyRule = policyRule
			};
		}

		private static WidgetDefinition SelectPreferredWidgetDefinition(IGrouping<string, WidgetDefinition> definitions)
		{
			return definitions
				.OrderBy(widget => widget.IsAbstract ? 1 : 0)
				.ThenBy(widget => string.IsNullOrWhiteSpace(widget.SourceLibrary) ? 1 : 0)
				.ThenByDescending(widget => widget.Properties?.Count ?? 0)
				.First();
		}

		private static TypeDefinition SelectPreferredTypeDefinition(IGrouping<string, TypeDefinition> definitions)
		{
			return definitions
				.OrderBy(type => type.IsAbstract ? 1 : 0)
				.ThenBy(type => string.IsNullOrWhiteSpace(type.SourceLibrary) ? 1 : 0)
				.ThenByDescending(type => type.Properties?.Count ?? 0)
				.First();
		}

		private static EnumDefinition SelectPreferredEnumDefinition(IGrouping<string, EnumDefinition> definitions)
		{
			return definitions
				.OrderBy(enumDefinition => string.IsNullOrWhiteSpace(enumDefinition.SourceLibrary) ? 1 : 0)
				.ThenByDescending(enumDefinition => enumDefinition.Values?.Count ?? 0)
				.First();
		}

		private static TypedefDefinition SelectPreferredTypedefDefinition(IGrouping<string, TypedefDefinition> definitions)
		{
			return definitions
				.OrderBy(typedefDefinition => string.IsNullOrWhiteSpace(typedefDefinition.SourceLibrary) ? 1 : 0)
				.ThenByDescending(typedefDefinition => typedefDefinition.Parameters?.Count ?? 0)
				.First();
		}
	}
}
