import 'dart:async';
import 'dart:io' show Platform;
import 'dart:typed_data';
import 'dart:ffi';
import 'package:ffi/ffi.dart';

import 'package:flutter/foundation.dart'
    show debugDefaultTargetPlatformOverride;
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import 'maui_flutter.dart';
import 'state_notifier.dart';
import 'scroll_controller_manager.dart';
import 'parsers/refreshindicator_parser.dart' show completeAsyncCallback;

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

Future _raiseMauiEvent(int eventHandlerId, String type, Map eventArgs) async {
  await _invokeCallbackToDotNet({
    'raiseEvent': eventHandlerId,
    'type': type,
    'eventArgsJson': JsonEncoder().convert(eventArgs)
  });
}

const MethodChannel methodChannel =
    MethodChannel('com.Microsoft.FlutterSharp/Messages');

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
        if (call.arguments is String) {
          _onEvent(call.arguments as String);
        } else {
          setState(() {
            _debugMessage = "ERROR: Args not String (${call.arguments.runtimeType})";
          });
          debugPrint('[Dart] ERROR: Arguments is not a String!');
        }
      } catch (e, stackTrace) {
        setState(() {
          _debugMessage = "ERROR: $e";
        });
        debugPrint('[Dart] ERROR in method handler: $e');
        debugPrint('[Dart] Stack: $stackTrace');
      }
    });

    dotNetMessageChannel.setMessageHandler((bytes) async {
      if (bytes == null) {
        return ByteData(0);
      }
      final messageString = getStringFromBytes(bytes);
      _onEvent(messageString);
      return ByteData(0);
    });

    _invokeCallbackToDotNet(
        json.encode({'readyPlayer1': widget.key?.toString() ?? '0'}));
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
          debugPrint('[Dart] Component: $componentId, Address: 0x${ptr.toRadixString(16)}');
          final pointer = Pointer<FlutterObjectStruct>.fromAddress(ptr);
          debugPrint('[Dart] Struct at address: handle=0x${pointer.ref.handle.address.toRadixString(16)}, managedHandle=0x${pointer.ref.managedHandle.address.toRadixString(16)}, widgetType=${pointer.ref.widgetType}');
          setMauiState(componentId, pointer.ref);
          debugPrint('[Dart] State set successfully');
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
      default:
        print('Warning: Unknown message type: ${message['messageType']}');
    }
    } catch (e, stackTrace) {
      print('ERROR in _onEvent: $e');
      print('Stack trace: $stackTrace');
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

    if (componentId != null && mauiComponentStatesMaps.containsKey(componentId)) {
      // Optionally clear the component state if the entire component was disposed
      // For now we leave the state to allow for widget replacement
    }
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.yellow,
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
                style: TextStyle(fontSize: 16, color: Colors.white, fontWeight: FontWeight.bold),
              ),
            ),
            Expanded(child: MauiComponent(componentId: "0")),
          ],
        ),
      ),
    );
  }
}

class MauiComponent extends StatefulWidget {
  String componentId;

  MauiComponent({required this.componentId});

  _MauiComponentState createState() => mauiComponentStates[componentId] =
      new _MauiComponentState(componentId: componentId);
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
        debugPrint('[MauiComponent] Building with address: ${_address!.handle.address}');
        debugPrint('[MauiComponent] Widget type: ${_address!.widgetType}');
        var w = DynamicWidgetBuilder.buildFromMap(_address, context);
        debugPrint('[MauiComponent] Build result: $w');
        if (w == null) {
          return Text('Widget build returned null for type: ${_address!.widgetType}', style: TextStyle(color: Colors.red));
        }
        return w;
      } catch (e, stackTrace) {
        debugPrint('ERROR in _MauiComponentState.build: $e');
        debugPrint('Stack trace: $stackTrace');
        return Text('Error: $e', style: TextStyle(color: Colors.red));
      }
    } else {
      return Text('No address set (address is null)', style: TextStyle(color: Colors.orange));
    }
  }
}
