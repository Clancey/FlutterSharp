import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:flutter/widgets.dart';

import '../flutter_sharp_structs.dart';
import '../maui_flutter.dart';
import '../error_overlay.dart';
import '../generated/structs/errorboundary_struct.dart';

/// Parser for the ErrorBoundary widget.
class ErrorBoundaryParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<ErrorBoundaryStruct>.fromAddress(fos.handle.address).ref;

    // Parse child widget
    Widget? child = map.child.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(map.child, buildContext)
        : null;

    // Parse boolean flags
    bool showInOverlay = map.showInOverlay == 1;
    bool reportToNative = map.reportToNative == 1;

    // Parse optional widget type name
    String? widgetTypeName =
        map.hasWidgetTypeName == 1 && map.widgetTypeName.address != 0
            ? map.widgetTypeName.toDartString()
            : null;

    // Parse optional onError callback
    void Function(ErrorInfo)? onError;
    if (map.hasOnErrorAction == 1 && map.onErrorAction.address != 0) {
      String actionId = map.onErrorAction.toDartString();
      onError = (ErrorInfo error) {
        // Send error info back to C# via method channel
        _sendErrorToNative(actionId, error);
      };
    }

    // Build the ErrorBoundary widget
    return ErrorBoundary(
      showInOverlay: showInOverlay,
      reportToNative: reportToNative,
      widgetTypeName: widgetTypeName,
      onError: onError,
      child: child ?? const SizedBox.shrink(),
    );
  }

  /// Sends error information to C# via the callback mechanism.
  void _sendErrorToNative(String actionId, ErrorInfo error) {
    // Use the method channel to invoke the C# callback
    try {
      final errorJson = '''
{
  "errorType": "${_escapeJson(error.errorType)}",
  "message": "${_escapeJson(error.message)}",
  "stackTrace": ${error.stackTrace != null ? '"${_escapeJson(error.stackTrace!)}"' : 'null'},
  "widgetType": ${error.widgetType != null ? '"${_escapeJson(error.widgetType!)}"' : 'null'},
  "isRecoverable": ${error.isRecoverable}
}
''';
      // Invoke the action with the error JSON
      methodChannel.invokeMethod(
          'Action', '{"actionId": "$actionId", "args": $errorJson}');
    } catch (e) {
      debugPrint('[ErrorBoundaryParser] Failed to send error to native: $e');
    }
  }

  String _escapeJson(String value) {
    return value
        .replaceAll('\\', '\\\\')
        .replaceAll('"', '\\"')
        .replaceAll('\n', '\\n')
        .replaceAll('\r', '\\r')
        .replaceAll('\t', '\\t');
  }

  @override
  String get widgetName => "ErrorBoundary";
}
