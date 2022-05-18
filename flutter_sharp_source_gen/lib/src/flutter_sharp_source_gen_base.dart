import 'dart:collection';
import 'dart:ffi';

import 'package:analyzer/dart/constant/value.dart';
import 'package:analyzer/dart/element/element.dart';
import 'package:analyzer/dart/element/type.dart';
import 'package:flutter_sharp/flutter_sharp.dart';
import 'package:source_gen/source_gen.dart';
import 'package:build/build.dart';
import 'dart:mirrors';
import 'dart:async';

Builder flutter_sharpGeneratorFactoryBuilder({String? header}) =>
    LibraryBuilder(CommentGenerator());

/// Generates a single-line comment for each class
class CommentGenerator extends GeneratorForAnnotation<GenerateAttribute> {
  final bool forClasses, forLibrary;

  const CommentGenerator({this.forClasses = true, this.forLibrary = true});

  // @override
  // Future<String> generate(LibraryReader library, _) async {
  //   final output = <String>[];
  //   if (forLibrary) {
  //     var name = library.element.name;
  //     if (name.isEmpty) {
  //       name = library.element.source.uri.pathSegments.last;
  //     }
  //     output.add('// Code for "$name"');
  //   }
  //   if (forClasses) {
  //     for (var classElement in library.allElements.whereType<ClassElement>()) {
  //       if (classElement.displayName.contains('GoodError')) {
  //         throw InvalidGenerationSourceError(
  //           "Don't use classes with the word 'Error' in the name",
  //           todo: 'Rename ${classElement.displayName} to something else.',
  //           element: classElement,
  //         );
  //       }
  //       output.add('// Code for "$classElement"');
  //     }
  //   }
  //   return output.join('\n');
  // }

  static HashSet<ClassElement> completedClasses = HashSet<ClassElement>();
  static Map<ClassElement, LibraryElement> classToLibrary =
      <ClassElement, LibraryElement>{};

  @override
  generateForAnnotatedElement(
      Element element, ConstantReader annotation, BuildStep buildStep) {
    final output = <String>[];

    List<ClassElement> classesToGenerate = [];
    try {
      var name =
          annotation.objectValue.type?.getDisplayString(withNullability: false);
      if (name == "GenerateAttribute") {
        bool generateForEntireLib = annotation.objectValue
                .getField('generateForEntireLib')
                ?.toBoolValue() ??
            false;
        var typeToGenerate =
            annotation.objectValue.getField('classType')?.toTypeValue();
        if (typeToGenerate != null) {
          var classElement = typeToGenerate.element as ClassElement;
          var lib = getLibraryElement(classElement, element);
          getAllSuperClasses(classElement, classesToGenerate);
          for (var c in classesToGenerate) {
            generateForClass(output, c, lib, buildStep);
          }
          print('Found GenerateAttribute annotation');
          if (generateForEntireLib) {}
        }
      }
    } catch (e) {
      print(e);
    }
    // if (forLibrary) {
    //   var name = element.name;
    //   if (name?.isEmpty ?? false) {
    //     name = element.source?.uri.pathSegments.last;
    //   }
    //   output.add('// Code for "$name"');
    // }
    return output.join('\n');
  }

  static LibraryElement getLibraryElement(
      ClassElement classElement, Element element) {
    if (classToLibrary.containsKey(classElement)) {
      return classToLibrary[classElement]!;
    }
    var lib = classElement.library;
    try {
      var allLibs = element.library?.importedLibraries;
      var foundLib = allLibs?.firstWhere(
          (element) => element.exports.any((e) => matches(e, classElement)));
      if (foundLib != null) {
        classToLibrary[classElement] = foundLib;
        return foundLib;
      }
    } catch (e) {
      print(e);
    }
    return lib;
  }

  static bool matches(ExportElement exportElement, ClassElement classElement) {
    var exportPath = exportElement.library.identifier.split('/')[0] +
        '/' +
        exportElement.uri.toString();
    return exportPath == classElement.library.identifier;
  }

  void generateForClass(List<String> output, ClassElement classElement,
      LibraryElement libraryElement, BuildStep buildStep) {
    var name = classElement.name;
    if (name.isEmpty) {
      name = classElement.source.uri.pathSegments.last;
    }
    output.add('// Struct for "$name"');
    generateStructForClass(output, classElement, libraryElement, buildStep);
    output.join('\n');
    generateParserForClass(output, classElement, libraryElement, buildStep);
    completedClasses.add(classElement);
  }

  void generateStructForClass(List<String> output, ClassElement classElement,
      LibraryElement libraryElement, BuildStep buildStep) {
    print(classElement.name);
    var constructorCount = classElement.constructors.length;
    HashSet<MapEntry<MapEntry<String, Type>, String>> properties = HashSet();
    if (constructorCount > 0) {
      properties.add(MapEntry(MapEntry("String", String), "constructor"));
    }
    for (var c in classElement.constructors) {
      for (var p in c.parameters) {
        var element = p.type.element;
        if (element is ClassElement) {
          properties.add(MapEntry(MapEntry(element.name, Pointer), p.name));
        } else {
          properties.add(MapEntry(
              MapEntry(element?.name ?? "", element.runtimeType), p.name));
        }
      }
    }
  }

  void generateParserForClass(List<String> output, ClassElement classElement,
      LibraryElement libraryElement, BuildStep buildStep) {}

  void generateLib() {}

  void getAllSuperClasses(ClassElement? element, List<ClassElement> classes) {
    var name = element?.displayName;
    if (element != null &&
        name != "Widget" &&
        name != "StatefulWidget" &&
        !completedClasses.contains(element)) {
      classes.add(element);
      getAllSuperClasses(element.supertype?.element as ClassElement, classes);
    }
  }
}

ClassMirror? getClassMirrorByName(String className) {
  if (className == null) {
    return null;
  }

  var index = className.lastIndexOf('.');
  var libname = '';
  var name = className;
  if (index > 0) {
    libname = className.substring(0, index);
    name = className.substring(index + 1);
  }

  LibraryMirror lib;
  var mirrors = currentMirrorSystem();
  if (libname.isEmpty) {
    try {
      var s = Symbol(name);
      for (var l in mirrors.libraries.values) {
        if (l.declarations.containsKey(s)) {
          return l.declarations[s] as ClassMirror;
        }
      }
    } catch (e) {
      print(e);
    }
  } else {
    lib = mirrors.findLibrary(Symbol(libname));
    var c = lib.declarations[Symbol(name)] as ClassMirror;
    return c;
  }
  return null;
}

// Runs for anything annotated as deprecated
class DeprecatedGeneratorForAnnotation
    extends GeneratorForAnnotation<Deprecated> {
  @override
  String generateForAnnotatedElement(Element element, _, __) =>
      '// "$element" is deprecated!';
}
