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

final mauiComponentStatesMaps = Map<String, FlutterObjectStruct>();

_MauiComponentState getMauiComponentState(String componentId) {
  if (!mauiComponentStates.containsKey(componentId)) {
    return null;
  }
  return mauiComponentStates[componentId];
}

void setMauiState(String componentId, IFlutterObjectStruct address) {
  mauiComponentStatesMaps[componentId] = address;
  getMauiComponentState(componentId)?.updateMauiState(address);
}

FlutterObjectStruct getMauiState(String componentId) {
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
  _MauiRootRendererState createState() =>  _MauiRootRendererState(key);
}


class _MauiRootRendererState extends State<MauiRootRenderer> {
  _MauiRootRendererState(Key key) {
    methodChannel.setMethodCallHandler((call) async {
        _onEvent(call.arguments);
      // print(call.arguments);
    });
    dotNetMessageChannel.setMessageHandler((bytes) async {
      final messageString = getStringFromBytes(bytes);
      _onEvent(messageString);
      return null;
    });

    _invokeCallbackToDotNet({'ready': key.toString()}); // Arbitrary message
  }

  void _onEvent(String json) {
    final message = jsonDecode(json);
    switch (message['messageType']) {
      case 'UpdateComponent':
        final componentId = message['componentId'];
        int address = message['address'];
         final pointer = Pointer<FlutterObjectStruct>.fromAddress(address);
        setMauiState(componentId, pointer.ref);
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
        throw new Exception("Unknown message type: ${message['messageType']}");
    }
  }

  @override
  Widget build(BuildContext context) {
    return MauiComponent(componentId: "0");
  }
}

class MauiComponent extends StatefulWidget {
  String componentId;

  MauiComponent({this.componentId});

  _MauiComponentState createState() => mauiComponentStates[componentId] = new _MauiComponentState(componentId:componentId);
}

class _MauiComponentState extends State<MauiComponent> {
  FlutterObjectStruct _address;
  String componentId;

  _MauiComponentState({this.componentId}) {
    _address = getMauiState(componentId);
  }
  updateMauiState(FlutterObjectStruct address) {
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
      return DynamicWidgetBuilder.buildFromMap(_address, context);
    } else {
      return SizedBox.shrink();
    }
  }
}
