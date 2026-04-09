import 'dart:async';
import 'dart:ffi';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import 'maui_flutter.dart';
import 'state_notifier.dart';
import 'scroll_controller_manager.dart';
import 'parsers/refreshindicator_parser.dart' show completeAsyncCallback;
import 'maui_navigation.dart';
import 'error_overlay.dart';
import 'hot_reload_notification.dart';

import 'dart:convert';

String getStringFromBytes(ByteData data) {
  final list =
      data.buffer.asUint16List(data.offsetInBytes, data.lengthInBytes >> 1);
  return String.fromCharCodes(list);
}

final mauiComponentStates = Map<String, _MauiComponentState>();
const dotNetMessageChannel =
    const BasicMessageChannel('my/super/test', BinaryCodec());

final mauiComponentStatesMaps = Map<String, IFlutterObjectStruct>();

/// Tracks widget IDs that have been disposed on the C# side
final _disposedWidgetIds = Set<String>();

/// Checks if a widget has been disposed
bool isWidgetDisposed(String widgetId) => _disposedWidgetIds.contains(widgetId);

/// Clears the disposed widget tracking (call on app restart/reset)
void clearDisposedWidgets() => _disposedWidgetIds.clear();

_MauiComponentState? getMauiComponentState(String componentId) {
  if (!mauiComponentStates.containsKey(componentId)) {
    return null;
  }
  return mauiComponentStates[componentId];
}

void setMauiState(String componentId, IFlutterObjectStruct address) {
  mauiComponentStatesMaps[componentId] = address;
  getMauiComponentState(componentId)?.updateMauiState(address);
}

IFlutterObjectStruct? getMauiState(String componentId) {
  if (!mauiComponentStatesMaps.containsKey(componentId)) {
    return null;
  }
  return mauiComponentStatesMaps[componentId];
}

Future _invokeCallbackToDotNet(Object message) async {
  // var data = JsonUtf8Encoder().convert(message);
  // var buffer = Uint8List.fromList(data).buffer;
  // await dotNetMessageChannel.send(buffer.asByteData());
  await methodChannel.invokeMethod("ready", message);
}

/// Sends a Dart exception to C# for logging and handling.
/// This enables C# code to be notified of Dart-side errors.
///
/// [errorType] - Category of the exception (e.g., 'ParserError', 'BuildError', 'RuntimeError')
/// [message] - The exception message
/// [stackTrace] - Optional Dart stack trace
/// [widgetType] - Optional widget type context
/// [source] - Optional source method/function name
/// [handledLocally] - Whether the error was already displayed to the user in Dart
Future<void> sendExceptionToCSharp({
  required String errorType,
  required String message,
  StackTrace? stackTrace,
  String? widgetType,
  String? source,
  String? context,
  bool handledLocally = false,
}) async {
  try {
    final exceptionData = {
      'errorType': errorType,
      'message': message,
      'stackTrace': stackTrace?.toString(),
      'timestamp': DateTime.now().toUtc().toIso8601String(),
      'widgetType': widgetType,
      'source': source,
      'context': context,
      'handledLocally': handledLocally,
    };

    debugPrint('[Dart] Sending exception to C#: [$errorType] $message');
    await methodChannel.invokeMethod(
        'DartException', json.encode(exceptionData));
  } catch (e) {
    // Don't let exception sending cause more exceptions
    debugPrint('[Dart] Failed to send exception to C#: $e');
  }
}

/// Helper to send an exception object to C#.
Future<void> sendException(
  Object exception,
  StackTrace stackTrace, {
  required String errorType,
  String? widgetType,
  String? source,
  bool handledLocally = false,
}) async {
  await sendExceptionToCSharp(
    errorType: errorType,
    message: exception.toString(),
    stackTrace: stackTrace,
    widgetType: widgetType,
    source: source,
    handledLocally: handledLocally,
  );
}

const MethodChannel methodChannel =
    MethodChannel('com.Microsoft.FlutterSharp/Messages');

Future<void> invokeHandleAction(
  String? actionId, {
  String? widgetType,
  Map<String, dynamic>? args,
}) async {
  if (actionId == null || actionId.isEmpty) {
    return;
  }

  await methodChannel.invokeMethod(
    'HandleAction',
    json.encode({
      'actionId': actionId,
      'widgetType': widgetType ?? 'Unknown',
      ...?args,
    }),
  );
}

class MauiRootRenderer extends StatefulWidget {
  const MauiRootRenderer({Key? key}) : super(key: key);

  @override
  _MauiRootRendererState createState() => _MauiRootRendererState();
}

class _MauiRootRendererState extends State<MauiRootRenderer> {
  String _debugMessage = "Waiting for ready...";
  int _messageCount = 0;

  @override
  void initState() {
    super.initState();

    methodChannel.setMethodCallHandler((call) async {
      try {
        _messageCount++;
        setState(() {
          _debugMessage = "Received: ${call.method} (#$_messageCount)";
        });
        debugPrint('[Dart] Received method call: ${call.method}');
        debugPrint('[Dart] Arguments type: ${call.arguments.runtimeType}');
        debugPrint('[Dart] Arguments: ${call.arguments}');

        // Handle BackPressed directly - returns whether the back press was handled
        if (call.method == 'BackPressed') {
          final handled = await _handleBackPressed();
          debugPrint('[Dart] BackPressed handled: $handled');
          return handled;
        }

        if (call.arguments is String) {
          _onEvent(call.arguments as String);
        } else {
          setState(() {
            _debugMessage =
                "ERROR: Args not String (${call.arguments.runtimeType})";
          });
          debugPrint('[Dart] ERROR: Arguments is not a String!');
        }
      } catch (e, stackTrace) {
        setState(() {
          _debugMessage = "ERROR: $e";
        });
        debugPrint('[Dart] ERROR in method handler: $e');
        debugPrint('[Dart] Stack: $stackTrace');
        // Send exception to C# for logging/handling
        sendException(
          e,
          stackTrace,
          errorType: 'MethodChannelError',
          source: 'methodChannel.setMethodCallHandler',
          handledLocally: true,
        );
      }
    });

    dotNetMessageChannel.setMessageHandler((bytes) async {
      if (bytes == null) {
        return ByteData(0);
      }

      // Check if this is binary protocol data (starts with version byte)
      final data =
          bytes.buffer.asUint8List(bytes.offsetInBytes, bytes.lengthInBytes);
      if (BinaryProtocol.isEnabled &&
          data.length >= 2 &&
          data[0] == BinaryProtocol.protocolVersion) {
        _handleBinaryMessage(data);
      } else {
        // Legacy string-based message
        final messageString = getStringFromBytes(bytes);
        _onEvent(messageString);
      }
      return ByteData(0);
    });

    _invokeCallbackToDotNet(
        json.encode({'readyPlayer1': widget.key?.toString() ?? '0'}));
  }

  /// Handles binary protocol messages from C#.
  /// This is the high-performance path for widget updates and other frequent operations.
  void _handleBinaryMessage(Uint8List data) {
    try {
      final (version, messageType) = BinaryProtocol.decodeHeader(data);
      debugPrint('[Dart] Binary message: version=$version, type=$messageType');

      switch (messageType) {
        case MessageTypes.updateComponent:
          final (componentId, address) =
              BinaryProtocol.decodeUpdateMessage(data);
          debugPrint(
              '[Dart] Binary UpdateComponent: $componentId, addr=0x${address.toRadixString(16)}');
          final pointer = Pointer<FlutterObjectStruct>.fromAddress(address);
          setMauiState(componentId, pointer.ref);
          break;

        case MessageTypes.batchedUpdate:
          final updates = BinaryProtocol.decodeBatchedUpdate(data);
          debugPrint('[Dart] Binary BatchedUpdate: ${updates.length} updates');
          for (final (componentId, address) in updates) {
            try {
              final pointer = Pointer<FlutterObjectStruct>.fromAddress(address);
              setMauiState(componentId, pointer.ref);
            } catch (e) {
              debugPrint(
                  '[Dart] Error processing batched update for $componentId: $e');
            }
          }
          break;

        case MessageTypes.disposed:
          final widgetId = BinaryProtocol.decodeDisposedMessage(data);
          debugPrint('[Dart] Binary Disposed: $widgetId');
          _disposedWidgetIds.add(widgetId);
          break;

        case MessageTypes.error:
          final (message, stackTrace) = BinaryProtocol.decodeErrorMessage(data);
          debugPrint('[Dart] Binary Error: $message');
          ErrorOverlayManager.instance.showError(ErrorInfo(
            errorType: 'BinaryProtocolError',
            message: message,
            stackTrace: stackTrace,
          ));
          break;

        case MessageTypes.lifecycle:
          final state = BinaryProtocol.decodeLifecycleMessage(data);
          debugPrint('[Dart] Binary Lifecycle: $state');
          // Handle lifecycle state change (0=Resumed, 1=Inactive, 2=Paused, 3=Detached)
          break;

        case MessageTypes.scrollCommand:
          final (controllerId, command, offset, durationMs, curve) =
              BinaryProtocol.decodeScrollCommand(data);
          debugPrint(
              '[Dart] Binary ScrollCommand: $controllerId, cmd=$command, offset=$offset');
          ScrollControllerManager().handleScrollCommand({
            'controllerId': controllerId,
            'command': command == 0 ? 'jumpTo' : 'animateTo',
            'offset': offset,
            'durationMs': durationMs,
            'curve': curve,
          });
          break;

        case MessageTypes.stateNotify:
          final (notifierId, jsonValue, timestampTicks) =
              BinaryProtocol.decodeStateNotify(data);
          debugPrint('[Dart] Binary StateNotify: $notifierId');
          final value = jsonDecode(jsonValue);
          StateNotifier.handleStateChanged(notifierId, value);
          break;

        case MessageTypes.hotReload:
          final (success, widgetType, durationMs, error) =
              BinaryProtocol.decodeHotReloadNotification(data);
          debugPrint(
              '[Dart] Binary HotReload: success=$success, type=$widgetType');
          HotReloadNotificationManager.instance.showNotification(HotReloadInfo(
            success: success,
            widgetType: widgetType,
            duration: Duration(milliseconds: durationMs),
            errorMessage: error,
          ));
          break;

        case MessageTypes.asyncCallbackComplete:
          final callbackId = BinaryProtocol.decodeAsyncCallbackComplete(data);
          debugPrint('[Dart] Binary AsyncCallbackComplete: $callbackId');
          completeAsyncCallback(callbackId);
          break;

        default:
          debugPrint('[Dart] Unknown binary message type: $messageType');
      }
    } catch (e, stackTrace) {
      debugPrint('[Dart] Error handling binary message: $e');
      sendException(
        e,
        stackTrace,
        errorType: 'BinaryProtocolError',
        source: '_handleBinaryMessage',
        handledLocally: true,
      );
    }
  }

  void _onEvent(String json) {
    try {
      print('[Dart] _onEvent received: $json');
      final message = jsonDecode(json);
      print('[Dart] Decoded message type: ${message['messageType']}');
      switch (message['messageType']) {
        case 'UpdateComponent':
          debugPrint('[Dart] Processing UpdateComponent');
          final componentId = message['componentId'];
          int ptr = message['address'];
          debugPrint(
              '[Dart] Component: $componentId, Address: 0x${ptr.toRadixString(16)}');
          final pointer = Pointer<FlutterObjectStruct>.fromAddress(ptr);
          debugPrint(
              '[Dart] Struct at address: handle=0x${pointer.ref.handle.address.toRadixString(16)}, managedHandle=0x${pointer.ref.managedHandle.address.toRadixString(16)}, widgetType=${pointer.ref.widgetType}');
          setMauiState(componentId, pointer.ref);
          debugPrint('[Dart] State set successfully');
          break;
        case 'BatchedUpdate':
          _handleBatchedUpdate(message);
          break;
        case 'DisposedComponent':
          _handleDisposedComponent(message);
          break;
        case 'StateChanged':
          _handleStateChanged(message);
          break;
        case 'ScrollCommand':
          _handleScrollCommand(message);
          break;
        case 'AsyncCallbackComplete':
          _handleAsyncCallbackComplete(message);
          break;
        case 'MauiNavigating':
          _handleMauiNavigating(message);
          break;
        case 'MauiNavigated':
          _handleMauiNavigated(message);
          break;
        case 'Error':
          _handleError(message);
          break;
        case 'Lifecycle':
          _handleLifecycle(message);
          break;
        case 'MemoryWarning':
          _handleMemoryWarning(message);
          break;
        case 'HotReload':
          _handleHotReload(message);
          break;
        case 'EnableRenderingMetrics':
          _handleEnableRenderingMetrics(message);
          break;
        case 'ShowPerformanceOverlay':
          _handleShowPerformanceOverlay(message);
          break;
        case 'HidePerformanceOverlay':
          _handleHidePerformanceOverlay(message);
          break;
        case 'Invoke':
          _handleInvoke(message);
          break;
        default:
          print('Warning: Unknown message type: ${message['messageType']}');
      }
    } catch (e, stackTrace) {
      print('ERROR in _onEvent: $e');
      print('Stack trace: $stackTrace');
      // Send exception to C# before rethrowing
      sendException(
        e,
        stackTrace,
        errorType: 'MessageProcessingError',
        source: '_onEvent',
        handledLocally: false,
      );
      rethrow;
    }
  }

  /// Handles a widget disposal message from C#
  void _handleDisposedComponent(Map<String, dynamic> message) {
    final widgetId = message['widgetId'] as String?;
    final componentId = message['componentId'] as String?;

    if (widgetId != null) {
      // Clean up any tracked state for this widget
      _disposedWidgetIds.add(widgetId);
      print('Widget disposed: $widgetId');
    }

    if (componentId != null &&
        mauiComponentStatesMaps.containsKey(componentId)) {
      // Optionally clear the component state if the entire component was disposed
      // For now we leave the state to allow for widget replacement
    }
  }

  /// Handles a batched update message from C#.
  /// This processes multiple widget updates that were collected on the C# side
  /// within a single frame window, reducing MethodChannel overhead.
  void _handleBatchedUpdate(Map<String, dynamic> message) {
    final updates = message['updates'] as List<dynamic>?;
    if (updates == null || updates.isEmpty) {
      debugPrint('[Dart] BatchedUpdate: No updates in batch');
      return;
    }

    debugPrint(
        '[Dart] Processing BatchedUpdate with ${updates.length} updates');

    // Process all updates in the batch
    for (final update in updates) {
      try {
        final componentId = update['componentId'] as String?;
        final address = update['address'] as int?;

        if (componentId == null || address == null) {
          debugPrint(
              '[Dart] BatchedUpdate: Skipping update with missing componentId or address');
          continue;
        }

        final pointer = Pointer<FlutterObjectStruct>.fromAddress(address);
        debugPrint(
            '[Dart] BatchedUpdate: Processing component $componentId at 0x${address.toRadixString(16)}');
        setMauiState(componentId, pointer.ref);
      } catch (e, stackTrace) {
        debugPrint('[Dart] Error processing batched update: $e');
        debugPrint('Stack trace: $stackTrace');
        // Continue processing remaining updates even if one fails
      }
    }

    debugPrint(
        '[Dart] BatchedUpdate: Completed processing ${updates.length} updates');
  }

  /// Handles a state change notification from C#
  void _handleStateChanged(Map<String, dynamic> message) {
    final notifierId = message['notifierId'] as String?;
    final value = message['value'];

    if (notifierId != null) {
      // Update the Dart-side state value
      StateNotifier.handleStateChanged(notifierId, value);
      debugPrint('State changed for notifier: $notifierId, value: $value');
    }
  }

  /// Handles a scroll command from C#
  void _handleScrollCommand(Map<String, dynamic> message) {
    scrollControllerManager.handleScrollCommand(message);
  }

  /// Handles an async callback completion from C#
  void _handleAsyncCallbackComplete(Map<String, dynamic> message) {
    final widgetId = message['widgetId'] as String?;
    if (widgetId != null) {
      completeAsyncCallback(widgetId);
      debugPrint('Async callback completed for widget: $widgetId');
    }
  }

  /// Handles MAUI navigation starting event
  void _handleMauiNavigating(Map<String, dynamic> message) {
    MauiNavigationBridge.instance.handleNavigating(
      from: message['from'] as String? ?? '',
      to: message['to'] as String? ?? '',
      navigationType: message['navigationType'] as String? ?? '',
    );
  }

  /// Handles MAUI navigation completed event
  void _handleMauiNavigated(Map<String, dynamic> message) {
    MauiNavigationBridge.instance.handleNavigated(
      from: message['from'] as String? ?? '',
      to: message['to'] as String? ?? '',
      navigationType: message['navigationType'] as String? ?? '',
      source: message['source'] as String? ?? '',
    );
  }

  /// Handles error messages from C# to display in the error overlay
  void _handleError(Map<String, dynamic> message) {
    try {
      final errorInfo = ErrorInfo.fromJson(message);
      ErrorOverlayManager.instance.showError(errorInfo);
      debugPrint(
          'Error received from C#: [${errorInfo.errorType}] ${errorInfo.message}');
    } catch (e) {
      // Fallback if JSON parsing fails
      final errorType = message['errorType'] as String? ?? 'Error';
      final errorMessage = message['message'] as String? ?? 'Unknown error';
      ErrorOverlayManager.instance.showError(ErrorInfo(
        errorType: errorType,
        message: errorMessage,
      ));
      debugPrint('Error parsing error message: $e');
    }
  }

  /// Handles lifecycle state changes from C#
  void _handleLifecycle(Map<String, dynamic> message) {
    final state = message['state'] as String? ?? 'resumed';
    debugPrint('Lifecycle state changed: $state');
    // The lifecycle state is primarily informational for Flutter
    // Flutter handles its own lifecycle through WidgetsBindingObserver
    // This notification can be used for custom lifecycle handling if needed
    _LifecycleNotifier.instance.notifyStateChange(state);
  }

  /// Handles memory warning messages from C#
  void _handleMemoryWarning(Map<String, dynamic> message) {
    final level = message['level'] as String? ?? 'medium';
    final timestamp = message['timestamp'] as int? ?? 0;
    debugPrint('Memory warning received: level=$level, timestamp=$timestamp');

    // Notify memory warning listeners
    _MemoryWarningNotifier.instance.notifyMemoryWarning(level);

    // Release cached images
    imageCache.clear();
    imageCache.clearLiveImages();

    // Request Flutter to clear cached resources based on severity
    if (level == 'high' || level == 'critical') {
      debugPrint('Clearing image cache due to $level memory warning');
    }
  }

  /// Handles hot reload notifications from C#
  void _handleHotReload(Map<String, dynamic> message) {
    try {
      final info = HotReloadInfo.fromJson(message);
      HotReloadNotificationManager.instance.showNotification(info);
      debugPrint(
          'Hot reload notification received: success=${info.success}, widgetType=${info.widgetType}');
    } catch (e) {
      // Fallback if JSON parsing fails
      HotReloadNotificationManager.instance.showNotification(HotReloadInfo(
        success: true,
      ));
      debugPrint('Hot reload notification (fallback): $e');
    }
  }

  /// Handles enable/disable rendering metrics from C#
  void _handleEnableRenderingMetrics(Map<String, dynamic> message) {
    final enabled = message['enabled'] as bool? ?? false;
    final targetFps = (message['targetFps'] as num?)?.toDouble() ?? 60.0;

    if (enabled) {
      renderingMetrics.enable(targetFps: targetFps);
      debugPrint('Rendering metrics enabled (target FPS: $targetFps)');
    } else {
      renderingMetrics.disable();
      debugPrint('Rendering metrics disabled');
    }
  }

  /// Handles show performance overlay from C#
  void _handleShowPerformanceOverlay(Map<String, dynamic> message) {
    // Import performance overlay manager and show it
    performanceOverlayManager.show();
    debugPrint('Performance overlay shown from C#');
  }

  /// Handles hide performance overlay from C#
  void _handleHidePerformanceOverlay(Map<String, dynamic> message) {
    // Import performance overlay manager and hide it
    performanceOverlayManager.hide();
    debugPrint('Performance overlay hidden from C#');
  }

  /// Handles an invoke message from C# that expects a response.
  /// This is used for request/response style communication like widget inspector queries.
  Future<void> _handleInvoke(Map<String, dynamic> message) async {
    final requestId = message['requestId'] as int?;
    final method = message['method'] as String?;
    final arguments = message['arguments'];

    if (requestId == null || method == null) {
      debugPrint('[Dart] Invalid Invoke message: missing requestId or method');
      return;
    }

    debugPrint('[Dart] Invoke: $method (requestId: $requestId)');

    try {
      Object? result;

      // Route to appropriate handler based on method
      if (method.startsWith('inspector.')) {
        result = await _handleInspectorInvoke(method, arguments);
      } else {
        debugPrint('[Dart] Unknown invoke method: $method');
        result = null;
      }

      // Send response back to C#
      final response = {
        'messageType': 'InvokeResponse',
        'requestId': requestId,
        'result': result,
      };
      methodChannel.invokeMethod('InvokeResponse', jsonEncode(response));
    } catch (e, stackTrace) {
      debugPrint('[Dart] Error in invoke $method: $e');
      // Send error response
      final errorResponse = {
        'messageType': 'InvokeError',
        'requestId': requestId,
        'error': e.toString(),
      };
      methodChannel.invokeMethod('InvokeError', jsonEncode(errorResponse));
      sendException(
        e,
        stackTrace,
        errorType: 'InvokeError',
        source: '_handleInvoke:$method',
        handledLocally: false,
      );
    }
  }

  /// Handles widget inspector invoke methods.
  Future<Object?> _handleInspectorInvoke(
      String method, dynamic arguments) async {
    // Ensure inspector service is initialized
    widgetInspectorService.initialize();

    final args =
        arguments is Map<String, dynamic> ? arguments : <String, dynamic>{};

    switch (method) {
      case 'inspector.enable':
        widgetInspectorManager.enable();
        return true;
      case 'inspector.disable':
        widgetInspectorManager.disable();
        return true;
      case 'inspector.toggle':
        widgetInspectorManager.toggle();
        return widgetInspectorManager.isEnabled;
      case 'inspector.showOverlay':
        widgetInspectorManager.showInspectorOverlay();
        return true;
      case 'inspector.hideOverlay':
        widgetInspectorManager.hideInspectorOverlay();
        return true;
      case 'inspector.getWidgetTree':
        final depth = args['depth'] as int? ?? 10;
        return widgetInspectorService.getWidgetTreeJson(maxDepth: depth);
      case 'inspector.getSelectedWidget':
        return widgetInspectorService.getSelectedWidgetJson();
      case 'inspector.selectWidget':
        final widgetType = args['widgetType'] as String?;
        final hashCode = args['hashCode'] as int?;
        if (widgetType != null && hashCode != null) {
          widgetInspectorManager.selectByDebugInfo(widgetType, hashCode);
          return true;
        }
        return false;
      case 'inspector.getWidgetProperties':
        final hashCode = args['hashCode'] as int?;
        if (hashCode != null) {
          return widgetInspectorService.getWidgetPropertiesJson(hashCode);
        }
        return null;
      case 'inspector.getRenderObjectInfo':
        final hashCode = args['hashCode'] as int?;
        if (hashCode != null) {
          return widgetInspectorService.getRenderObjectInfoJson(hashCode);
        }
        return null;
      default:
        debugPrint('[Dart] Unknown inspector method: $method');
        return null;
    }
  }

  /// Handles the Android back button press from C#.
  /// Notifies all registered back button handlers and returns whether the back press was handled.
  /// This method is called when the Android back button is pressed and C# wants Flutter to handle it.
  Future<bool> _handleBackPressed() async {
    debugPrint('[Dart] _handleBackPressed called');

    // Notify all registered back button handlers
    final handled = await _BackButtonManager.instance.handleBackPressed();
    debugPrint('[Dart] Back button handled by registered handler: $handled');

    if (handled) {
      return true;
    }

    // If no handler handled it, try to pop the navigator if there is one
    final navigatorState = Navigator.maybeOf(context);
    if (navigatorState != null && navigatorState.canPop()) {
      navigatorState.pop();
      debugPrint('[Dart] Back button handled by Flutter Navigator.pop()');
      return true;
    }

    debugPrint('[Dart] Back button not handled');
    return false;
  }

  @override
  Widget build(BuildContext context) {
    return HotReloadNotificationOverlay(
      displayDuration: const Duration(seconds: 2),
      showSuccessNotifications: true,
      position: HotReloadNotificationPosition.bottom,
      child: ErrorOverlay(
        autoDismissDuration: const Duration(seconds: 8),
        child: Scaffold(
          backgroundColor: Theme.of(context).scaffoldBackgroundColor,
          body: SafeArea(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.start,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Container(
                  color: Colors.blue,
                  padding: EdgeInsets.all(8),
                  child: Text(
                    _debugMessage,
                    style: TextStyle(
                        fontSize: 16,
                        color: Colors.white,
                        fontWeight: FontWeight.bold),
                  ),
                ),
                Expanded(child: MauiComponent(componentId: "0")),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class MauiComponent extends StatefulWidget {
  final String componentId;

  MauiComponent({required this.componentId});

  @override
  _MauiComponentState createState() => mauiComponentStates[componentId] =
      _MauiComponentState(componentId: componentId);
}

class _MauiComponentState extends State<MauiComponent> {
  IFlutterObjectStruct? _address;
  String componentId;

  _MauiComponentState({required this.componentId}) {
    _address = getMauiState(componentId);
  }
  updateMauiState(IFlutterObjectStruct address) {
    if (mounted) {
      setState(() {
        _address = address;
      });
    } else {
      _address = address;
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_address != null) {
      try {
        debugPrint(
            '[MauiComponent] Building with address: ${_address!.handle.address}');
        debugPrint('[MauiComponent] Widget type: ${_address!.widgetType}');
        var w = DynamicWidgetBuilder.buildFromMap(_address, context);
        debugPrint('[MauiComponent] Build result: $w');
        if (w == null) {
          // Send error to overlay for visibility
          ErrorOverlayManager.instance.showError(ErrorInfo(
            errorType: 'WidgetParseError',
            message:
                'Widget build returned null for type: ${_address!.widgetType}',
            widgetType: _address!.widgetType.toString(),
          ));
          return Text(
              'Widget build returned null for type: ${_address!.widgetType}',
              style: TextStyle(color: Colors.red));
        }
        return w;
      } catch (e, stackTrace) {
        debugPrint('ERROR in _MauiComponentState.build: $e');
        debugPrint('Stack trace: $stackTrace');
        // Send error to overlay for visibility
        ErrorOverlayManager.instance.showError(ErrorInfo(
          errorType: 'WidgetParseError',
          message: '$e',
          stackTrace: stackTrace.toString(),
          widgetType: _address?.widgetType.toString(),
        ));
        // Send exception to C# for logging/handling
        sendException(
          e,
          stackTrace,
          errorType: 'WidgetBuildError',
          widgetType: _address?.widgetType.toString(),
          source: '_MauiComponentState.build',
          handledLocally: true,
        );
        return Text('Error: $e', style: TextStyle(color: Colors.red));
      }
    } else {
      return Text('No address set (address is null)',
          style: TextStyle(color: Colors.orange));
    }
  }
}

/// Notifies listeners about lifecycle state changes from C#.
/// Subscribe to this to react to app lifecycle changes from the C# side.
class _LifecycleNotifier {
  static final _LifecycleNotifier instance = _LifecycleNotifier._();
  _LifecycleNotifier._();

  final _listeners = <void Function(String state)>[];
  String _currentState = 'resumed';

  /// Gets the current lifecycle state.
  String get currentState => _currentState;

  /// Adds a listener for lifecycle state changes.
  void addListener(void Function(String state) listener) {
    _listeners.add(listener);
  }

  /// Removes a lifecycle state change listener.
  void removeListener(void Function(String state) listener) {
    _listeners.remove(listener);
  }

  /// Called internally to notify all listeners of a state change.
  void notifyStateChange(String state) {
    _currentState = state;
    for (final listener in _listeners) {
      listener(state);
    }
  }
}

/// Notifies listeners about memory warnings from C#.
/// Subscribe to this to handle memory cleanup when warnings are received.
class _MemoryWarningNotifier {
  static final _MemoryWarningNotifier instance = _MemoryWarningNotifier._();
  _MemoryWarningNotifier._();

  final _listeners = <void Function(String level)>[];

  /// Adds a listener for memory warnings.
  void addListener(void Function(String level) listener) {
    _listeners.add(listener);
  }

  /// Removes a memory warning listener.
  void removeListener(void Function(String level) listener) {
    _listeners.remove(listener);
  }

  /// Called internally to notify all listeners of a memory warning.
  void notifyMemoryWarning(String level) {
    debugPrint('Notifying ${_listeners.length} memory warning listeners');
    for (final listener in _listeners) {
      listener(level);
    }
  }
}

/// Public API for subscribing to lifecycle events from C#.
class LifecycleNotifier {
  /// Gets the singleton instance.
  static _LifecycleNotifier get instance => _LifecycleNotifier.instance;
}

/// Public API for subscribing to memory warning events from C#.
class MemoryWarningNotifier {
  /// Gets the singleton instance.
  static _MemoryWarningNotifier get instance => _MemoryWarningNotifier.instance;
}

/// Manages back button handlers for Android back button integration.
/// Widgets can register themselves as back button handlers to intercept
/// the Android back button press before it propagates.
class _BackButtonManager {
  static final _BackButtonManager instance = _BackButtonManager._();
  _BackButtonManager._();

  final List<Future<bool> Function()> _handlers = [];

  /// Registers a back button handler.
  /// The handler should return true if it handled the back press, false otherwise.
  /// Handlers are called in reverse order of registration (last registered first).
  void registerHandler(Future<bool> Function() handler) {
    _handlers.add(handler);
    debugPrint('Back button handler registered, total: ${_handlers.length}');
  }

  /// Unregisters a back button handler.
  void unregisterHandler(Future<bool> Function() handler) {
    _handlers.remove(handler);
    debugPrint('Back button handler unregistered, total: ${_handlers.length}');
  }

  /// Handles the back button press by notifying all registered handlers.
  /// Returns true if any handler handled the back press.
  Future<bool> handleBackPressed() async {
    debugPrint(
        'BackButtonManager.handleBackPressed called, handlers: ${_handlers.length}');

    // Call handlers in reverse order (most recently registered first)
    for (int i = _handlers.length - 1; i >= 0; i--) {
      try {
        final handled = await _handlers[i]();
        if (handled) {
          debugPrint('Back button handled by handler at index $i');
          return true;
        }
      } catch (e, stackTrace) {
        debugPrint('Error in back button handler: $e');
        // Send exception to C# for logging
        sendException(
          e,
          stackTrace,
          errorType: 'BackButtonHandlerError',
          source: 'BackButtonManager.handleBackPressed',
          handledLocally: true,
        );
      }
    }

    return false;
  }
}

/// Public API for back button management.
/// Widgets can use this to register/unregister back button handlers.
class BackButtonManager {
  /// Gets the singleton instance.
  static _BackButtonManager get instance => _BackButtonManager.instance;
}
