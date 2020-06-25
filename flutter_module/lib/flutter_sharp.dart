import 'dart:async';

import 'package:flutter/services.dart';

class FlutterSharp {
  static const MethodChannel _channel =
      const MethodChannel('flutter_sharp');

  static Future<String> get platformVersion async {
    final String version = await _channel.invokeMethod('getPlatformVersion');
    return version;
  }
}
