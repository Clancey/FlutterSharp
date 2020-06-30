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

final mauiComponentStatesMaps = Map<String, Map<String, dynamic>>();

_MauiComponentState getMauiComponentState(String componentId) {
  if (!mauiComponentStates.containsKey(componentId)) {
    return null;
  }
  return mauiComponentStates[componentId];
}

void setMauiState(String componentId, Map<String, dynamic> map) {
  mauiComponentStatesMaps[componentId] = map;
  getMauiComponentState(componentId)?.updateMauiState(map);
}

Map<String, dynamic> getMauiState(String componentId) {
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

class FooBar extends Struct{
  @Int32()
  int first;
  @Int32()
  int second;
}

class _MauiRootRendererState extends State<MauiRootRenderer> {
  _MauiRootRendererState(Key key) {
    methodChannel.setMethodCallHandler((call) async {
      if(call.method =="intptr")
      {
          var i = int.parse(call.arguments);
          final pointer = Pointer<FooBar>.fromAddress(i);
          FooBar fooBar = pointer.ref;
          print(fooBar.second);
          
      }
      else
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
        final componentState = message['state'];
        setMauiState(componentId, componentState);
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
  Map _mauiState;
  String componentId;

  _MauiComponentState({this.componentId}) {
    _mauiState = getMauiState(componentId);
  }
  updateMauiState(Map mauiState) {
    if (mounted) {
      setState(() {
        _mauiState = mauiState;
      });
    } else {
      _mauiState = mauiState;
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_mauiState != null) {
      return DynamicWidgetBuilder.buildFromMap(_mauiState, context);
    } else {
      return SizedBox.shrink();
    }
  }
}
