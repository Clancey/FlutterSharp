import 'dart:io';
import 'package:path/path.dart' as p;

extension PathCleansing on String {
  String CleansePath() {
    if (Platform.pathSeparator == '\\') return this;
    return this.replaceAll('\\', '/');
  }

  String NormalizePath() {
    return p.normalize(this);
  }
}

class Config {
  static const dartSdkEnvVariableName = "DART_SDK";
  static bool includeMethodImplementations = false;
  static bool includeConstructorImplementations = false;
  static bool includeFieldImplementations = false;
  static bool isTestbed = false;

  static String sourcePath =
      Config.isTestbed ? _testbedFlutterSourcePath : _flutterSourcePath;

  //TODO: Make this dynamic
  static String flutterRoot =
      Directory('..\\..\\flutter'.CleansePath()).absolute.path.NormalizePath();
  // Path to the flutter src directory
  static String _flutterSourcePath =
      Directory('${flutterRoot}\\packages\\flutter\\lib\\src'.CleansePath())
          .absolute
          .path
          .replaceAll('\\AST\\..'.CleansePath(), '')
          .CleansePath()
          .NormalizePath();

  // This is just a quick test bed, if you want to try out specific
  // Dart related functionality without running it on the whole transpiler
  static String _testbedFlutterSourcePath =
      Directory('..\\testbed'.CleansePath())
          .absolute
          .path
          .replaceAll('\\AST\\..'.CleansePath(), '')
          .CleansePath()
          .NormalizePath();

  static Future<bool> IsDartSdkPathAvailable = Directory(DartSdkPath).exists();

  // Absolute path to the dart-sdk directory
  static String DartSdkPath =
      Directory('${flutterRoot}\\bin\\cache\\dart-sdk'.CleansePath())
          .absolute
          .path
          .NormalizePath();

  // Root namespace the transpiled namespaces will start with
  static String rootNamespace = "FlutterSDK";

  // Imports that are replaced with .net system or mapping libraries
  static List<String> ignoredImports = <String>[]
    ..add("package:typed_data/typed_buffers.dart")
    ..add("package:collection/collection.dart")
    ..add("dart:ui")
    ..add("dart:async")
    ..add("dart:math")
    ..add("dart:collection")
    ..add("dart:developer")
    ..add("dart:io")
    ..add("dart:core")
    ..add("dart:typed_data")
    ..add("dart:_http")
    ..add("package:meta/meta.dart")
    ..add("dart:convert")
    ..add("dart:isolate")
    ..add("package:vector_math/vector_math_64.dart")
    ..add("package:typed_data/typed_buffers.dart;");
}
