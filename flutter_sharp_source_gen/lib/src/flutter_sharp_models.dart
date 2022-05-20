import 'dart:collection';

import 'package:analyzer/dart/element/element.dart';

class ModuleParsedData {
  Map<String, TypeInfo> types = {};
  HashSet<ClassInfo> classes = HashSet();
  Map<String, dynamic> toJson() => {
        'classes': classes.map((e) => e.toJson()).toList(),
        'types': types.values.map((e) => e.toJson()).toList(),
      };
}

class ClassInfo extends TypeInfo {
  List<ConstructorInfo> consturctors = [];
  ClassInfo(ClassElement element)
      : super(element.name, element.library.identifier);
  @override
  Map<String, dynamic> toJson() => {
        'name': name,
        'constructors': consturctors.map((e) => e.toJson()).toList(),
      };
}

class ConstructorInfo {
  String name;
  List<ParameterInfo> parameters = [];
  ConstructorInfo(this.name);
  Map<String, dynamic> toJson() => {
        'name': name,
        'parameters': parameters.map((e) => e.toJson()).toList(),
      };
}

class ParameterInfo {
  String name;
  bool required;
  late TypeInfo type;
  ParameterInfo(this.name, this.required);
  Map<String, dynamic> toJson() => {
        'name': name,
        'type': type.id,
      };
}

class TypeInfo {
  String name;
  String location;
  String get id => '$name - $location';
  TypeInfo(this.name, this.location);
  Map<String, dynamic> toJson() => {
        'name': name,
        'location': location,
      };
  @override
  int get hashCode => name.hashCode ^ location.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TypeInfo &&
          runtimeType == other.runtimeType &&
          name == other.name &&
          location == other.location;
}

class FunctionInfo extends TypeInfo {
  FunctionInfo(String name, String location) : super(name, location);
  TypeInfo? returnType;
  @override
  Map<String, dynamic> toJson() => {
        'name': name,
        'returnType': returnType?.toJson(),
      };
}
