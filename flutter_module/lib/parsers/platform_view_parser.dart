import 'dart:ffi';
import 'dart:io';

import 'package:ffi/ffi.dart';
import 'package:flutter/services.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';

import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/widgets.dart';

class PlatformViewParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<WidgetStruct>.fromAddress(fos.handle.address).ref;
    const viewType = "FlutterSharpNativeView";
    final id = parseString(map.id);
    final Map<String, String> creationParams = <String, String>{"id": id};
    if (Platform.isAndroid) {
      return AndroidView(
        viewType: viewType,
        creationParams: id,
        creationParamsCodec: const StandardMessageCodec(),
        layoutDirection: TextDirection.ltr,
      );
    }
    if (Platform.isIOS) {
      return UiKitView(
        viewType: viewType,
        creationParams: id,
        creationParamsCodec: const StandardMessageCodec(),
        layoutDirection: TextDirection.ltr,
      );
    }
    return Text("PlatformViewParser: Unsupported platform");
  }

  @override
  String get widgetName => "PlatformView";
}
