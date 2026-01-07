import 'dart:io';
import 'dart:convert';
import 'package:analyzer/dart/analysis/analysis_context.dart';
import 'package:analyzer/dart/analysis/analysis_context_collection.dart';
import 'package:analyzer/dart/analysis/results.dart';
import 'package:analyzer/dart/ast/ast.dart';
import 'package:analyzer/dart/ast/visitor.dart';
import 'package:path/path.dart' as path;
import 'package:yaml/yaml.dart';

/// Main entry point for the package scanner
void main(List<String> args) async {
  if (args.isEmpty) {
    stderr.writeln('Usage: dart run package_scanner.dart <package-path> [include1,include2] [exclude1,exclude2]');
    exit(1);
  }

  final packagePath = args[0];
  final includeList = args.length > 1 && args[1].isNotEmpty ? args[1].split(',') : <String>[];
  final excludeList = args.length > 2 && args[2].isNotEmpty ? args[2].split(',') : <String>[];

  try {
    final scanner = PackageScanner(packagePath, includeList, excludeList);
    final result = await scanner.scan();
    print(jsonEncode(result));
  } catch (e, stackTrace) {
    stderr.writeln('Error scanning package: $e');
    stderr.writeln(stackTrace);
    exit(1);
  }
}

/// Scanner for extracting widget, type, and enum definitions from a Dart package
class PackageScanner {
  final String packagePath;
  final List<String> includeList;
  final List<String> excludeList;

  PackageScanner(this.packagePath, this.includeList, this.excludeList);

  /// Scans the package and returns the extracted definitions
  Future<Map<String, dynamic>> scan() async {
    final packageInfo = await _readPackageInfo();
    final libPath = path.join(packagePath, 'lib');

    if (!Directory(libPath).existsSync()) {
      throw Exception('lib directory not found in package: $packagePath');
    }

    final collection = AnalysisContextCollection(
      includedPaths: [libPath],
    );

    final widgets = <Map<String, dynamic>>[];
    final types = <Map<String, dynamic>>[];
    final enums = <Map<String, dynamic>>[];

    for (final context in collection.contexts) {
      await _analyzeContext(context, widgets, types, enums);
    }

    return {
      'packagePath': packagePath,
      'name': packageInfo['name'],
      'version': packageInfo['version'],
      'description': packageInfo['description'],
      'widgets': widgets,
      'types': types,
      'enums': enums,
      'analysisTimestamp': DateTime.now().toIso8601String(),
    };
  }

  /// Reads package information from pubspec.yaml
  Future<Map<String, dynamic>> _readPackageInfo() async {
    final pubspecPath = path.join(packagePath, 'pubspec.yaml');
    if (!File(pubspecPath).existsSync()) {
      return {'name': 'unknown', 'version': '0.0.0'};
    }

    final pubspecContent = await File(pubspecPath).readAsString();
    final yaml = loadYaml(pubspecContent);

    return {
      'name': yaml['name'] ?? 'unknown',
      'version': yaml['version'] ?? '0.0.0',
      'description': yaml['description'],
      'homepage': yaml['homepage'],
      'repository': yaml['repository'],
    };
  }

  /// Analyzes an analysis context and extracts definitions
  Future<void> _analyzeContext(
    AnalysisContext context,
    List<Map<String, dynamic>> widgets,
    List<Map<String, dynamic>> types,
    List<Map<String, dynamic>> enums,
  ) async {
    for (final filePath in context.contextRoot.analyzedFiles()) {
      if (!filePath.endsWith('.dart')) continue;
      if (filePath.contains('/test/')) continue;
      if (filePath.contains('/.dart_tool/')) continue;

      try {
        final session = context.currentSession;
        final result = await session.getResolvedUnit(filePath);

        if (result is ResolvedUnitResult) {
          final visitor = DefinitionVisitor(
            filePath,
            packagePath,
            includeList,
            excludeList,
          );
          result.unit.accept(visitor);

          widgets.addAll(visitor.widgets);
          types.addAll(visitor.types);
          enums.addAll(visitor.enums);
        }
      } catch (e) {
        stderr.writeln('Warning: Error analyzing file $filePath: $e');
      }
    }
  }
}

/// AST visitor to extract widget, type, and enum definitions
class DefinitionVisitor extends RecursiveAstVisitor<void> {
  final String filePath;
  final String packagePath;
  final List<String> includeList;
  final List<String> excludeList;

  final widgets = <Map<String, dynamic>>[];
  final types = <Map<String, dynamic>>[];
  final enums = <Map<String, dynamic>>[];

  DefinitionVisitor(
    this.filePath,
    this.packagePath,
    this.includeList,
    this.excludeList,
  );

  @override
  void visitClassDeclaration(ClassDeclaration node) {
    final element = node.declaredElement;
    if (element == null) return;

    final className = element.name;

    // Check include/exclude filters
    if (!_shouldInclude(className)) return;

    if (_isWidget(element)) {
      widgets.add(_extractWidgetDefinition(node, element));
    } else {
      types.add(_extractTypeDefinition(node, element));
    }

    super.visitClassDeclaration(node);
  }

  @override
  void visitEnumDeclaration(EnumDeclaration node) {
    final element = node.declaredElement;
    if (element == null) return;

    final enumName = element.name;

    // Check include/exclude filters
    if (!_shouldInclude(enumName)) return;

    enums.add(_extractEnumDefinition(node, element));

    super.visitEnumDeclaration(node);
  }

  /// Checks if a name should be included based on include/exclude lists
  bool _shouldInclude(String name) {
    // If include list is specified, name must be in it
    if (includeList.isNotEmpty && !includeList.contains(name)) {
      return false;
    }

    // If exclude list is specified, name must not be in it
    if (excludeList.isNotEmpty && excludeList.contains(name)) {
      return false;
    }

    return true;
  }

  /// Determines if a class element is a widget
  bool _isWidget(element) {
    return _extendsType(element, 'Widget') ||
        _extendsType(element, 'StatelessWidget') ||
        _extendsType(element, 'StatefulWidget') ||
        _extendsType(element, 'RenderObjectWidget') ||
        _extendsType(element, 'SingleChildRenderObjectWidget') ||
        _extendsType(element, 'MultiChildRenderObjectWidget') ||
        _extendsType(element, 'LeafRenderObjectWidget') ||
        _extendsType(element, 'ProxyWidget') ||
        _extendsType(element, 'InheritedWidget');
  }

  /// Checks if a class extends a specific type
  bool _extendsType(element, String typeName) {
    var current = element;
    while (current.supertype != null) {
      if (current.supertype!.element.name == typeName) {
        return true;
      }
      final superElement = current.supertype!.element;
      if (superElement.runtimeType.toString().contains('ClassElement')) {
        current = superElement;
      } else {
        break;
      }
    }
    return false;
  }

  /// Gets the first public (non-private) base class name by traversing up the inheritance chain
  /// Private classes in Dart start with an underscore (_)
  String? _getBaseClassName(element) {
    var current = element.supertype;

    // Traverse up the inheritance chain until we find a public class
    while (current != null) {
      final className = current.element.name;

      // If the class name doesn't start with underscore, it's public
      if (!className.startsWith('_')) {
        return className;
      }

      // Move up to the next supertype
      if (current.element.runtimeType.toString().contains('ClassElement')) {
        current = current.element.supertype;
      } else {
        break;
      }
    }

    return null;
  }

  /// Determines the widget type category
  String _getWidgetType(element) {
    if (_extendsType(element, 'StatelessWidget')) return 'Stateless';
    if (_extendsType(element, 'StatefulWidget')) return 'Stateful';
    if (_extendsType(element, 'SingleChildRenderObjectWidget')) return 'SingleChildRenderObject';
    if (_extendsType(element, 'MultiChildRenderObjectWidget')) return 'MultiChildRenderObject';
    if (_extendsType(element, 'LeafRenderObjectWidget')) return 'LeafRenderObject';
    if (_extendsType(element, 'ProxyWidget')) return 'Proxy';
    return 'Widget';
  }

  /// Extracts widget definition from a class declaration
  Map<String, dynamic> _extractWidgetDefinition(
    node,
    element,
  ) {
    final constructors = _extractConstructors(element);
    final properties = _extractProperties(element);

    // Determine child properties
    final childProperty = properties.firstWhere(
      (p) => p['name'] == 'child',
      orElse: () => <String, dynamic>{},
    );
    final childrenProperty = properties.firstWhere(
      (p) => p['name'] == 'children',
      orElse: () => <String, dynamic>{},
    );

    final hasSingleChild = childProperty.isNotEmpty;
    final hasMultipleChildren = childrenProperty.isNotEmpty;

    return {
      'name': element.name,
      'namespace': _getNamespace(element),
      'baseClass': _getBaseClassName(element),
      'type': _getWidgetType(element),
      'properties': properties,
      'constructors': constructors,
      'documentation': _getDocumentation(element),
      'sourceLibrary': element.library.identifier,
      'hasSingleChild': hasSingleChild,
      'hasMultipleChildren': hasMultipleChildren,
      'childPropertyName': hasSingleChild ? 'child' : null,
      'childrenPropertyName': hasMultipleChildren ? 'children' : null,
      'isAbstract': element.isAbstract,
      'isDeprecated': element.hasDeprecated,
      'deprecationMessage': _getDeprecationMessage(element),
      'typeParameters': element.typeParameters.map((tp) => tp.name).toList(),
      'isRenderObjectWidget': _extendsType(element, 'RenderObjectWidget'),
    };
  }

  /// Extracts type definition from a class declaration
  Map<String, dynamic> _extractTypeDefinition(
    node,
    element,
  ) {
    return {
      'name': element.name,
      'namespace': _getNamespace(element),
      'baseClass': _getBaseClassName(element),
      'interfaces': element.interfaces.map((i) => i.element.name).toList(),
      'isAbstract': element.isAbstract,
      'isImmutable': _isImmutable(element),
      'properties': _extractProperties(element),
      'constructors': _extractConstructors(element),
      'documentation': _getDocumentation(element),
      'sourceLibrary': element.library.identifier,
      'isDeprecated': element.hasDeprecated,
      'deprecationMessage': _getDeprecationMessage(element),
      'typeParameters': element.typeParameters.map((tp) => tp.name).toList(),
    };
  }

  /// Extracts enum definition from an enum declaration
  Map<String, dynamic> _extractEnumDefinition(
    node,
    element,
  ) {
    final values = element.fields
        .where((f) => f.isEnumConstant)
        .map((f) => {
              'name': f.name,
              'documentation': _getDocumentation(f),
              'isDeprecated': f.hasDeprecated,
            })
        .toList();

    return {
      'name': element.name,
      'namespace': _getNamespace(element),
      'values': values,
      'documentation': _getDocumentation(element),
      'sourceLibrary': element.library.identifier,
      'isDeprecated': element.hasDeprecated,
    };
  }

  /// Extracts constructor definitions from a class element
  List<Map<String, dynamic>> _extractConstructors(element) {
    return element.constructors.map((constructor) {
      // Skip private constructors
      if (constructor.name.startsWith('_')) {
        return null;
      }

      final parameters = constructor.parameters.map((param) {
        return _extractParameterDefinition(param);
      }).toList();

      return {
        'name': constructor.name,
        'isConst': constructor.isConst,
        'isFactory': constructor.isFactory,
        'parameters': parameters,
        'documentation': _getDocumentation(constructor),
        'isDeprecated': constructor.hasDeprecated,
        'deprecationMessage': _getDeprecationMessage(constructor),
        'fullName': constructor.name.isEmpty
            ? element.name
            : '${element.name}.${constructor.name}',
      };
    }).whereType<Map<String, dynamic>>().toList();
  }

  /// Extracts property definitions from a class element
  /// This includes both public fields AND constructor parameters that aren't already fields.
  /// Many Flutter widgets have constructor parameters that are passed directly to super
  /// constructors without being stored as fields in the widget class itself.
  List<Map<String, dynamic>> _extractProperties(element) {
    final properties = <Map<String, dynamic>>[];
    final propertyNames = <String>{};

    // Build a map of default values from the unnamed constructor
    final defaultValues = <String, String?>{};
    dynamic unnamedConstructor;
    try {
      unnamedConstructor = element.constructors.firstWhere(
        (c) => c.name.isEmpty,
      );
    } catch (_) {
      // No unnamed constructor, try to use the first constructor if any
      if (element.constructors.isNotEmpty) {
        unnamedConstructor = element.constructors.first;
      }
    }

    if (unnamedConstructor != null) {
      for (final param in unnamedConstructor.parameters) {
        if (param.defaultValueCode != null) {
          defaultValues[param.name] = param.defaultValueCode;
        }
      }
    }

    // Extract fields
    for (final field in element.fields) {
      // Skip private fields and static fields
      if (field.name.startsWith('_') || field.isStatic) continue;

      var dartType = _getTypeString(field.type);

      // If we got InvalidType, try to infer from field name
      if (dartType == 'InvalidType' || dartType.contains('InvalidType')) {
        dartType = _inferTypeFromParameterName(field.name, dartType);
      }

      properties.add({
        'name': field.name,
        'dartType': dartType,
        'isRequired': !_isNullable(field.type),
        'isNullable': _isNullable(field.type),
        'isNamed': true,
        'defaultValue': defaultValues[field.name],
        'documentation': _getDocumentation(field),
        'isList': _isList(field.type),
        'isCallback': _isCallback(field.type),
        'typeArguments': _getTypeArguments(field.type),
      });
      propertyNames.add(field.name);
    }

    // Also extract constructor parameters that are NOT already fields
    // Many Flutter widgets have constructor-only parameters that need to be captured
    if (unnamedConstructor != null) {
      for (final param in unnamedConstructor.parameters) {
        // Skip if this parameter name already exists as a field
        if (propertyNames.contains(param.name)) continue;

        // Skip the 'key' parameter - it's handled separately by the base Widget class
        if (param.name == 'key') continue;

        var dartType = _getTypeString(param.type);

        // If we got InvalidType, try to infer from parameter name
        if (dartType == 'InvalidType' || dartType.contains('InvalidType')) {
          dartType = _inferTypeFromParameterName(param.name, dartType);
        }

        properties.add({
          'name': param.name,
          'dartType': dartType,
          'isRequired': param.isRequired,
          'isNullable': _isNullable(param.type),
          'isNamed': param.isNamed,
          'defaultValue': param.defaultValueCode,
          'documentation': _getDocumentation(param),
          'isList': _isList(param.type),
          'isCallback': _isCallback(param.type),
          'typeArguments': _getTypeArguments(param.type),
        });
        propertyNames.add(param.name);
      }
    }

    return properties;
  }

  /// Extracts a parameter definition
  Map<String, dynamic> _extractParameterDefinition(param) {
    var dartType = _getTypeString(param.type);

    // If we got InvalidType, try to infer from parameter name
    if (dartType == 'InvalidType' || dartType.contains('InvalidType')) {
      dartType = _inferTypeFromParameterName(param.name, dartType);
    }

    return {
      'name': param.name,
      'dartType': dartType,
      'isRequired': param.isRequired,
      'isNullable': _isNullable(param.type),
      'isNamed': param.isNamed,
      'defaultValue': param.defaultValueCode,
      'documentation': _getDocumentation(param),
      'isList': _isList(param.type),
      'isCallback': _isCallback(param.type),
      'typeArguments': _getTypeArguments(param.type),
    };
  }

  /// Infers a Dart type from a parameter name when type resolution fails
  String _inferTypeFromParameterName(String paramName, String fallback) {
    // Don't override if we already have valid List type information
    if (fallback.startsWith('List<') && !fallback.contains('InvalidType')) {
      return fallback;  // Keep List<Widget>, List<BoxShadow>, etc.
    }

    // Common parameter name patterns to type mappings
    final nameToType = {
      // Flutter layout enums (commonly unresolved by analyzer)
      'mainAxisAlignment': 'MainAxisAlignment',
      'crossAxisAlignment': 'CrossAxisAlignment',
      'mainAxisSize': 'MainAxisSize',
      'verticalDirection': 'VerticalDirection',
      'textBaseline': 'TextBaseline?',
      'flexFit': 'FlexFit',
      'fit': 'BoxFit',
      'filterQuality': 'FilterQuality',
      'blendMode': 'BlendMode',
      'stackFit': 'StackFit',
      'overflow': 'Overflow',
      'textOverflow': 'TextOverflow',
      'softWrap': 'bool',
      'maxLines': 'int?',
      'textAlign': 'TextAlign?',
      'textWidthBasis': 'TextWidthBasis',
      'selectionHeightStyle': 'BoxHeightStyle',
      'selectionWidthStyle': 'BoxWidthStyle',
      'strutStyle': 'StrutStyle?',
      'locale': 'Locale?',
      'semanticsLabel': 'String?',
      'axis': 'Axis',
      'scrollDirection': 'Axis',
      'reverse': 'bool',
      'primary': 'bool?',
      'shrinkWrap': 'bool',
      'physics': 'ScrollPhysics?',
      'cacheExtent': 'double?',
      'semanticChildCount': 'int?',
      'dragStartBehavior': 'DragStartBehavior',
      'keyboardDismissBehavior': 'ScrollViewKeyboardDismissBehavior',
      'restorationId': 'String?',
      'clipBehavior': 'Clip',
      // Alignment and spacing
      'alignment': 'AlignmentGeometry?',
      'padding': 'EdgeInsetsGeometry?',
      'margin': 'EdgeInsetsGeometry?',
      'decoration': 'Decoration?',
      'foregroundDecoration': 'Decoration?',
      'constraints': 'BoxConstraints?',
      'transform': 'Matrix4?',
      'transformAlignment': 'AlignmentGeometry?',
      'clipBehavior': 'Clip',
      'color': 'Color?',
      'backgroundColor': 'Color?',
      'foregroundColor': 'Color?',
      'shadowColor': 'Color?',
      'surfaceTintColor': 'Color?',
      'borderRadius': 'BorderRadiusGeometry?',
      'border': 'BoxBorder?',
      'shape': 'BoxShape',
      'gradient': 'Gradient?',
      'boxShadow': 'List<BoxShadow>?',
      'curve': 'Curve',
      'duration': 'Duration',
      'width': 'double?',
      'height': 'double?',
      'style': 'TextStyle?',
      'textStyle': 'TextStyle?',
      'icon': 'IconData?',
      'iconSize': 'double?',
      // Common list properties
      'children': 'List<Widget>',
      'actions': 'List<Widget>?',
      'tabs': 'List<Widget>',
      // Common required parameters for specific widgets
      'delegate': 'dynamic', // Generic delegate type - cannot be fully resolved
      'controller': 'dynamic', // Generic controller type
      'itemBuilder': 'dynamic', // Callback type
      'gridDelegate': 'SliverGridDelegate',
      'offset': 'Offset',
      'animation': 'Animation<double>',
      'opacity': 'Animation<double>',
      'child': 'Widget',
      'sliver': 'Widget',
      'slivers': 'List<Widget>',
      'filter': 'ImageFilter',
      'colorFilter': 'ColorFilter',
      'link': 'LayerLink',
      'image': 'ImageProvider',
      'placeholder': 'ImageProvider?',
      'bundle': 'AssetBundle',
      'viewType': 'String',
      'view': 'FlutterView',
      'views': 'List<FlutterView>',
      'textDirection': 'TextDirection',
      'textHeightBehavior': 'TextHeightBehavior',
      'value': 'dynamic', // For AnnotatedRegion, etc.
      'valueListenable': 'ValueListenable<dynamic>',
      'text': 'InlineSpan',
      'hitTestBehavior': 'PlatformViewHitTestBehavior',
      'gestureRecognizers': 'Set<Factory<OneSequenceGestureRecognizer>>',
      'translation': 'Offset',
      'tween': 'Tween<double>',
      'turns': 'Animation<double>',
      'size': 'Size',
      'rect': 'Rect',
      'tree': 'Widget', // For TreeSliver
      'listenable': 'Listenable',
      'builder': 'WidgetBuilder?', // For DualTransitionBuilder etc.
      'surfaceFactory': 'dynamic', // For PlatformViewSurface
      'cursorColor': 'Color',
      'backgroundCursorColor': 'Color',
      'focusNode': 'FocusNode',
      'selectionColor': 'Color?',
      'baselineType': 'TextBaseline',
      'constraintsTransform': 'BoxConstraintsTransform',
    };

    return nameToType[paramName] ?? fallback;
  }

  /// Gets the namespace/library path for an element
  String _getNamespace(element) {
    final library = element.library;
    if (library == null) return '';

    // Extract the library path relative to the package
    final libraryPath = library.source.fullName;
    final libIndex = libraryPath.indexOf('/lib/');
    if (libIndex == -1) return library.identifier;

    return libraryPath.substring(libIndex + 5).replaceAll('.dart', '').replaceAll('/', '.');
  }

  /// Gets documentation comment for an element
  String? _getDocumentation(element) {
    final docComment = element.documentationComment;
    if (docComment == null || docComment.isEmpty) return null;

    // Remove comment markers and clean up
    return docComment
        .split('\n')
        .map((line) => line.replaceFirst(RegExp(r'^\s*///\s?'), ''))
        .join('\n')
        .trim();
  }

  /// Gets deprecation message for an element
  String? _getDeprecationMessage(element) {
    final metadata = element.metadata;
    for (final annotation in metadata) {
      if (annotation.isDeprecated) {
        // Try to extract the deprecation message
        final source = annotation.toSource();
        // Simple pattern to extract deprecation message
        if (source.contains('(')) {
          final start = source.indexOf('(') + 1;
          final end = source.lastIndexOf(')');
          if (start > 0 && end > start) {
            var message = source.substring(start, end).trim();
            // Remove quotes if present
            if (message.startsWith('"') || message.startsWith("'")) {
              message = message.substring(1, message.length - 1);
            }
            return message;
          }
        }
      }
    }
    return null;
  }

  /// Checks if a class is immutable
  bool _isImmutable(element) {
    return element.metadata.any((m) => m.element?.name == 'immutable');
  }

  /// Gets the type string representation
  String _getTypeString(type) {
    final typeString = type.getDisplayString(withNullability: true);

    // Check if this is a List<Widget> or similar collection type
    // We want to preserve these even if they can't be fully resolved
    if (typeString.startsWith('List<') && typeString.contains('Widget')) {
      return typeString; // Keep List<Widget>, List<Widget>?, etc.
    }

    // If we got InvalidType, it means the type couldn't be resolved
    // This can happen with types from external packages or SDK
    // The type itself may still have useful information in its element
    if (typeString == 'InvalidType' || typeString.contains('InvalidType')) {
      // Try to get the type name from the element if available
      if (type.element != null && type.element.name != 'InvalidType') {
        final nullability = typeString.endsWith('?') ? '?' : '';
        return '${type.element.name}$nullability';
      }
    }

    return typeString;
  }

  /// Checks if a type is nullable
  bool _isNullable(type) {
    return _getTypeString(type).endsWith('?');
  }

  /// Checks if a type is a list/collection
  bool _isList(type) {
    try {
      final name = type.element?.name;
      return name == 'List' || name == 'Iterable' || name == 'Set';
    } catch (_) {
      return false;
    }
  }

  /// Checks if a type is a callback/function
  bool _isCallback(type) {
    return type.runtimeType.toString().contains('FunctionType');
  }

  /// Gets type arguments for generic types
  List<String>? _getTypeArguments(type) {
    try {
      if (type.typeArguments == null || type.typeArguments.isEmpty) return null;
      return type.typeArguments.map((t) => _getTypeString(t)).toList();
    } catch (_) {
      return null;
    }
  }
}
