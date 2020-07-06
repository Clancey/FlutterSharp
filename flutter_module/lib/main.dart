// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

import 'dart:async';
import 'dart:io' show Platform;
import 'dart:typed_data';

import 'package:flutter/foundation.dart'
    show debugDefaultTargetPlatformOverride;
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'mauiRenderer.dart';
import 'dart:convert';

void main() {
  // See https://github.com/flutter/flutter/wiki/Desktop-shells#target-platform-override
  debugDefaultTargetPlatformOverride = TargetPlatform.fuchsia;

  runApp(new MyApp());
}

String getStringFromBytes(ByteData data) {
  final list =
      data.buffer.asUint16List(data.offsetInBytes, data.lengthInBytes >> 1);
  return String.fromCharCodes(list);
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo1',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        // See https://github.com/flutter/flutter/wiki/Desktop-shells#fonts
        fontFamily: 'Roboto',
      ),
      home: MauiRootRenderer(),
    );
  }
}

// final blazorComponentStates = Map<int, _BlazorComponentState>();
// const dotNetMessageChannel = const BasicMessageChannel('my/super/test', BinaryCodec());

// _BlazorComponentState getOrCreateBlazorComponentState(int componentId) {
//   if (!blazorComponentStates.containsKey(componentId)) {
//     blazorComponentStates[componentId] = _BlazorComponentState();
//   }

//   return blazorComponentStates[componentId];
// }

// Future _invokeCallbackToDotNet(Object message) async {
//   var data = JsonUtf8Encoder().convert(message);
//   var buffer = Uint8List.fromList(data).buffer;
//   await dotNetMessageChannel.send(buffer.asByteData());
// }

// Future _raiseBlazorEvent(int eventHandlerId, String type, Map eventArgs) async {
//   await _invokeCallbackToDotNet({
//     'raiseEvent': eventHandlerId,
//     'type': type,
//     'eventArgsJson': JsonEncoder().convert(eventArgs)
//   });
// }

// Map<int, dynamic> _trackedDartObjects = <int, dynamic>{};

// void createAndStoreTrackedDartObject(Map message) {
//   final id = message['id'];
//   final instance = createDartObjectForTracking(message);
//   _trackedDartObjects[id] = instance;
// }

// dynamic createDartObjectForTracking(Map message) {
//   final args = message['args'];
//   switch (message['type']) {
//     case 'TextFieldController':
//       return TextEditingController(text: args['value']);
//     default:
//       throw new Exception("Unknown type for createDartObjectForTracking: '${message['type']}'");
//   }
// }

// T getTrackedDartObject<T>(int id) {
//   return _trackedDartObjects[id];
// }

// void releaseTrackedDartObject(Map message) {
//   final id = message['id'];
//   if (_trackedDartObjects.containsKey(id)) {
//     _trackedDartObjects.remove(id);
//   }
// }

// class BlazorRenderer extends StatefulWidget {
//   _BlazorRendererState createState() => _BlazorRendererState();
// }

// class _BlazorRendererState extends State<BlazorRenderer> {

//   static const MethodChannel methodChannel =
//       MethodChannel('samples.flutter.io/battery');
//   _BlazorRendererState() {
//      methodChannel.setMethodCallHandler((call) async {
//       _onEvent(call.arguments);
//      // print(call.arguments);
//     });
//     dotNetMessageChannel.setMessageHandler((bytes) async {
//       final messageString = getStringFromBytes(bytes);
//       _onEvent(messageString);
//       return null;
//     });

//     _invokeCallbackToDotNet({ 'ready': true }); // Arbitrary message
//   }

//   void _onEvent(json) {
//      final message = jsonDecode(json);
//       switch (message['messageType'])
//       {
//         case 'UpdateComponent':
//           final componentId = message['componentId'];
//           final componentState = message['state'];
//           getOrCreateBlazorComponentState(componentId).updateBlazorState(componentState);
//           break;
//         case 'CreateDartObject':
//           createAndStoreTrackedDartObject(message);
//           break;
//         case 'ReleaseDartObject':
//           releaseTrackedDartObject(message);
//           break;
//         case 'TextEditingController_setValue':
//           final controller = getTrackedDartObject<TextEditingController>(message['id']);
//           controller.text = message['value'];
//           break;
//         default:
//           throw new Exception("Unknown message type: ${message['messageType']}");
//       }
//   }

//   @override
//   Widget build(BuildContext context) {
//     return BlazorComponent(componentId: 0);
//   }
// }

// class BlazorComponent extends StatefulWidget {
//   int componentId;

//   BlazorComponent({ this.componentId });

//   _BlazorComponentState createState() => getOrCreateBlazorComponentState(componentId);
// }

// class _BlazorComponentState extends State<BlazorComponent> {
//   Map _blazorState;

//   updateBlazorState(Map blazorState) {
//     if (mounted) {
//       setState(() {
//         _blazorState = blazorState;
//       });
//     } else {
//       _blazorState = blazorState;
//     }
//   }

//   @override
//   Widget build(BuildContext context) {
//     if (_blazorState != null) {
//       return mapJsonData(_blazorState);
//     } else {
//       return SizedBox.shrink();
//     }
//   }

//   Widget mapJsonData(Map jsonData) {
//     if (jsonData == null) {
//       return null;
//     }

//     switch (jsonData['__type']) {
//       case 'BlazorComponent':
//         return BlazorComponent(componentId: jsonData['componentId']);
//       case 'AppBar':
//         return AppBar(title: mapJsonData(jsonData['title']), bottom: mapJsonData(jsonData['bottom']),);
//       case 'Center':
//         return Center(child: mapJsonData(jsonData['child']));
//       case 'Checkbox':
//         return Checkbox(
//           value: jsonData['value'],
//           onChanged: (value) {
//             if (jsonData['onchange'] > 0) {
//               _raiseBlazorEvent(jsonData['onchange'], 'change', { 'value': value });
//             }
//           },
//         );
//       case 'Column':
//         return Column(
//           mainAxisAlignment: MainAxisAlignment.values[int.parse(jsonData['mainAxisAlignment'])],
//           children: mapJsonArray(jsonData['children']),
//         );
//       case 'Container':
//         return Container(
//           child: mapJsonData(jsonData['child']),
//           padding: mapEdgeInsets(jsonData['padding']),
//           margin: mapEdgeInsets(jsonData['margin']),
//           decoration: mapBoxDecoraton(jsonData['decoration']),
//         );
//       case 'DefaultTabController':
//         return DefaultTabController(
//           length: int.parse(jsonData['length']),
//           child: mapJsonData(jsonData['child']));
//       case 'Drawer':
//         return Drawer(child: mapJsonData(jsonData['child']));
//       case 'Icon':
//         return Icon(IconData(int.parse(jsonData['codePoint']), fontFamily: jsonData['fontFamily']));
//       case 'Flexible':
//         return Flexible(child: mapJsonData(jsonData['child']));
//       case 'FloatingActionButton': {
//         final eventHandlerId = jsonData['onclick'];
//         return FloatingActionButton(
//           onPressed: () { _raiseBlazorEvent(eventHandlerId, 'mouse', null); },
//           tooltip: jsonData['tooltip'],
//           child: mapJsonData(jsonData['child']),
//           shape: mapShapeBorder(jsonData['shape']),
//         );
//       }
//       case 'ListView':
//         return ListView(
//           children: mapJsonArray(jsonData['children']),
//           padding: mapEdgeInsets(jsonData['padding']),
//         );
//       case 'Row':
//         return Row(
//           mainAxisAlignment: MainAxisAlignment.values[int.parse(jsonData['mainAxisAlignment'])],
//           children: mapJsonArray(jsonData['children']),
//         );
//       case 'Scaffold':
//         return Scaffold(
//           appBar: mapJsonData(jsonData['appBar']),
//           body: mapJsonData(jsonData['body']),
//           floatingActionButton: mapJsonData(jsonData['floatingActionButton']),
//           drawer: mapJsonData(jsonData['drawer']),
//         );
//       case 'Tab':
//         return Tab(child: mapJsonData(jsonData['child']));
//       case 'TabBar':
//         return TabBar(tabs: mapJsonArray(jsonData['tabs']),);
//       case 'TabBarView':
//         return TabBarView(children: mapJsonArray(jsonData['children']),);
//       case 'Text':
//         return Text(
//           jsonData['value'],
//           textScaleFactor: mapDouble(jsonData['scaleFactor']),
//           style: TextStyle(
//             color: mapColor(jsonData['color']),
//             decoration: mapBool(jsonData['lineThrough']) ? TextDecoration.lineThrough : null,
//           )
//         );
//       case 'TextField':
//         return SimpleTextField(
//           onInputEventHandlerId: jsonData['oninput'],
//           onSubmitEventHandlerId: jsonData['onchange'],
//           text: jsonData['value'],
//           decoration: InputDecoration(
//             border: OutlineInputBorder(),
//             hintText: jsonData['hint']
//           )
//         );
//       default:
//         return Text("Unknown widget type ${jsonData['__type']}");
//     }
//   }

//   Iterable<Widget> mapJsonArray(dynamic jsonArray) {
//     if (jsonArray == null) return null;
//     final listOfMaps = (jsonArray as List<dynamic>).whereType<Map>();
//     return listOfMaps.map(mapJsonData).toList();
//   }

//   EdgeInsets mapEdgeInsets(dynamic jsonArray) {
//     if (jsonArray == null) return null;
//     return EdgeInsets.fromLTRB(jsonArray[3], jsonArray[0], jsonArray[1], jsonArray[2]);
//   }

//   ShapeBorder mapShapeBorder(String value)
//   {
//     if (value == null)
//       return null;
//     switch (value) {
//       case 'Round': return CircleBorder();
//       case 'Square': return RoundedRectangleBorder();
//       default: throw new Exception('Unknown shape border value: $value');
//     }
//   }

//   double mapDouble(String value)
//   {
//     return value == null ? null : double.parse(value);
//   }

//   bool mapBool(String value)
//   {
//     return value == 'True';
//   }

//   BoxDecoration mapBoxDecoraton(dynamic jsonValue)
//   {
//     if (jsonValue == null)
//       return null;
//     return BoxDecoration(
//       color: mapColor(jsonValue['color']),
//       border: mapBorder(jsonValue['border'])
//     );
//   }

//   Border mapBorder(dynamic jsonValue) {
//     if (jsonValue == null)
//       return null;
//     return Border.all(
//       color: mapColor(jsonValue['color']),
//       width: jsonValue['width']);
//   }

//   Color mapColor(dynamic jsonValue) {
//     if (jsonValue == null)
//       return null;
//     return Color(jsonValue);
//   }
// }

// class SimpleTextField extends StatefulWidget {
//   SimpleTextField({ this.text, this.onInputEventHandlerId, this.onSubmitEventHandlerId, this.decoration }) {
//   }

//   String text;
//   int onInputEventHandlerId;
//   int onSubmitEventHandlerId;
//   InputDecoration decoration;

//   @override
//   _SimpleTextFieldState createState() => _SimpleTextFieldState();
// }

// class _SimpleTextFieldState extends State<SimpleTextField> {
//   TextEditingController controller;

//   @override
//   void initState() {
//     super.initState();
//     controller = TextEditingController(text: widget.text);
//   }

//   @override
//   void dispose() {
//     controller.dispose();
//     super.dispose();
//   }

//   @override
//   Widget build(BuildContext context) {
//     if (widget.text != controller.text) {
//      controller.text = widget.text ?? '';
//      FocusScope.of(context).requestFocus(new FocusNode());
//     }
//     return TextField(
//       controller: controller,
//       onChanged: (value) {
//         if (widget.onInputEventHandlerId > 0) {
//           _raiseBlazorEvent(widget.onInputEventHandlerId, 'change', { 'value': value });
//         }
//       },
//       onSubmitted: (value) {
//         if (widget.onSubmitEventHandlerId > 0) {
//           _raiseBlazorEvent(widget.onSubmitEventHandlerId, 'change', { 'value': value });
//         }
//       },
//       decoration: widget.decoration,
//     );
//   }
// }
