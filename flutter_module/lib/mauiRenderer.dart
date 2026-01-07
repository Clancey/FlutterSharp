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
  @override
  void initState() {
    super.initState();

    methodChannel.setMethodCallHandler((call) async {
      _onEvent(call.arguments);
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
      final message = jsonDecode(json);
      switch (message['messageType']) {
        case 'UpdateComponent':
          final componentId = message['componentId'];
          int ptr = message['address'];
          final pointer = Pointer<FlutterObjectStruct>.fromAddress(ptr);
          setMauiState(componentId, pointer.ref);
          break;
        case 'DisposedComponent':
          _handleDisposedComponent(message);
          break;
      // case 'CreateDartObject':
      //   createAndStoreTrackedDartObject(message);
      //   break;
      // case 'ReleaseDartObject':
      //   releaseTrackedDartObject(message);
      //   break;
      // case 'TextEditingController_setValue':
      //   final controller = getTrackedDartObject<TextEditingController>(message['id']);
      //   controller.text = message['value'];
      //   break;
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

  @override
  Widget build(BuildContext context) {
    return MauiComponent(componentId: "0");
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
        var w = DynamicWidgetBuilder.buildFromMap(_address, context);
        return w ?? SizedBox.shrink();
      } catch (e, stackTrace) {
        print('ERROR in _MauiComponentState.build: $e');
        print('Stack trace: $stackTrace');
        return Text('Error: $e');
      }
    } else {
      return SizedBox.shrink();
    }
  }
}
