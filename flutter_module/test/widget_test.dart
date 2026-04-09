import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_module/mauiRenderer.dart' show methodChannel;
import 'package:flutter_module/maui_flutter.dart' show raiseMauiEvent;

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  final calls = <MethodCall>[];

  setUp(() {
    calls.clear();
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(methodChannel, (call) async {
      calls.add(call);
      return null;
    });
  });

  tearDown(() {
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(methodChannel, null);
  });

  test('action callbacks are routed through HandleAction with JSON payload', () async {
    await raiseMauiEvent('action_42', 'invoke', {'value': true});

    expect(calls, hasLength(1));
    expect(calls.single.method, 'HandleAction');
    expect(calls.single.arguments, isA<String>());

    final payload =
        json.decode(calls.single.arguments as String) as Map<String, dynamic>;
    expect(payload['actionId'], 'action_42');
    expect(payload['widgetType'], 'Unknown');
    expect(payload['value'], true);
  });

  test('non-action events keep using the legacy Event envelope', () async {
    await raiseMauiEvent('widget-1', 'OnChange', 'hello');

    expect(calls, hasLength(1));
    expect(calls.single.method, 'Event');
    expect(calls.single.arguments, isA<String>());

    final payload =
        json.decode(calls.single.arguments as String) as Map<String, dynamic>;
    expect(payload['componentId'], 'widget-1');
    expect(payload['eventName'], 'OnChange');
    expect(payload['data'], 'hello');
  });
}
