import 'dart:io';
import 'dart:convert';
import 'package:analyzer/dart/analysis/analysis_context.dart';
import 'package:analyzer/dart/analysis/analysis_context_collection.dart';
import 'package:analyzer/dart/analysis/results.dart';
import 'package:analyzer/dart/ast/ast.dart';
import 'package:analyzer/dart/ast/visitor.dart';
import 'package:analyzer/dart/element/element.dart';
import 'package:analyzer/dart/element/nullability_suffix.dart';
import 'package:analyzer/dart/element/type.dart';
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
  bool _isWidget(ClassElement element) {
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
  bool _extendsType(ClassElement element, String typeName) {
    var current = element;
    while (current.supertype != null) {
      if (current.supertype!.element.name == typeName) {
        return true;
      }
      final superElement = current.supertype!.element;
      if (superElement is! ClassElement) break;
      current = superElement;
    }
    return false;
  }

  /// Gets the immediate base class name
  String? _getBaseClassName(ClassElement element) {
    return element.supertype?.element.name;
  }

  /// Determines the widget type category
  String _getWidgetType(ClassElement element) {
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
    ClassDeclaration node,
    ClassElement element,
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
    ClassDeclaration node,
    ClassElement element,
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
    EnumDeclaration node,
    EnumElement element,
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
  List<Map<String, dynamic>> _extractConstructors(ClassElement element) {
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
  List<Map<String, dynamic>> _extractProperties(ClassElement element) {
    final properties = <Map<String, dynamic>>[];

    // Extract fields
    for (final field in element.fields) {
      // Skip private fields and static fields
      if (field.name.startsWith('_') || field.isStatic) continue;

      properties.add({
        'name': field.name,
        'dartType': _getTypeString(field.type),
        'isRequired': !_isNullable(field.type),
        'isNullable': _isNullable(field.type),
        'isNamed': true,
        'documentation': _getDocumentation(field),
        'isList': _isList(field.type),
        'isCallback': _isCallback(field.type),
        'typeArguments': _getTypeArguments(field.type),
      });
    }

    return properties;
  }

  /// Extracts a parameter definition
  Map<String, dynamic> _extractParameterDefinition(ParameterElement param) {
    return {
      'name': param.name,
      'dartType': _getTypeString(param.type),
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

  /// Gets the namespace/library path for an element
  String _getNamespace(Element element) {
    final library = element.library;
    if (library == null) return '';

    // Extract the library path relative to the package
    final libraryPath = library.source.fullName;
    final libIndex = libraryPath.indexOf('/lib/');
    if (libIndex == -1) return library.identifier;

    return libraryPath.substring(libIndex + 5).replaceAll('.dart', '').replaceAll('/', '.');
  }

  /// Gets documentation comment for an element
  String? _getDocumentation(Element element) {
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
  String? _getDeprecationMessage(Element element) {
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
  bool _isImmutable(ClassElement element) {
    return element.metadata.any((m) => m.element?.name == 'immutable');
  }

  /// Gets the type string representation
  String _getTypeString(DartType type) {
    return type.getDisplayString(withNullability: true);
  }

  /// Checks if a type is nullable
  bool _isNullable(DartType type) {
    return type.nullabilitySuffix == NullabilitySuffix.question;
  }

  /// Checks if a type is a list/collection
  bool _isList(DartType type) {
    if (type is! InterfaceType) return false;
    final name = type.element.name;
    return name == 'List' || name == 'Iterable' || name == 'Set';
  }

  /// Checks if a type is a callback/function
  bool _isCallback(DartType type) {
    return type is FunctionType;
  }

  /// Gets type arguments for generic types
  List<String>? _getTypeArguments(DartType type) {
    if (type is! InterfaceType) return null;
    if (type.typeArguments.isEmpty) return null;
    return type.typeArguments.map((t) => _getTypeString(t)).toList();
  }
}
