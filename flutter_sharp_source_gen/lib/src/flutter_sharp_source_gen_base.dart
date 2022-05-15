import 'package:analyzer/dart/element/element.dart';
import 'package:source_gen/source_gen.dart';
import 'package:build/build.dart';

Builder flutter_sharpGeneratorFactoryBuilder({String? header}) =>
    new PartBuilder([new CommentGenerator()], ".simple_auth.dart",
        header: header);

/// Generates a single-line comment for each class
class CommentGenerator extends Generator {
  final bool forClasses, forLibrary;

  const CommentGenerator({this.forClasses = true, this.forLibrary = true});

  @override
  Future<String> generate(LibraryReader library, _) async {
    final output = <String>[];
    if (forLibrary) {
      var name = library.element.name;
      if (name.isEmpty) {
        name = library.element.source.uri.pathSegments.last;
      }
      output.add('// Code for "$name"');
    }
    if (forClasses) {
      for (var classElement in library.allElements.whereType<ClassElement>()) {
        if (classElement.displayName.contains('GoodError')) {
          throw InvalidGenerationSourceError(
            "Don't use classes with the word 'Error' in the name",
            todo: 'Rename ${classElement.displayName} to something else.',
            element: classElement,
          );
        }
        output.add('// Code for "$classElement"');
      }
    }
    return output.join('\n');
  }
}

// Runs for anything annotated as deprecated
class DeprecatedGeneratorForAnnotation
    extends GeneratorForAnnotation<Deprecated> {
  @override
  String generateForAnnotatedElement(Element element, _, __) =>
      '// "$element" is deprecated!';
}
