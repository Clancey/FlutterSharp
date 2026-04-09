import 'dart:typed_data';
import 'dart:convert';

/// Binary protocol for high-performance message encoding/decoding between C# and Dart.
/// Mirrors the C# BinaryProtocol class for interoperability.
class BinaryProtocol {
  /// Protocol version for compatibility checking.
  static const int protocolVersion = 1;

  /// Whether binary protocol is enabled.
  static bool isEnabled = false;

  // Statistics
  static int _totalBytesSent = 0;
  static int _totalBytesReceived = 0;
  static int _messagesSent = 0;
  static int _messagesReceived = 0;

  /// Gets protocol statistics.
  static BinaryProtocolStats getStats() => BinaryProtocolStats(
        totalBytesSent: _totalBytesSent,
        totalBytesReceived: _totalBytesReceived,
        messagesSent: _messagesSent,
        messagesReceived: _messagesReceived,
        isEnabled: isEnabled,
      );

  /// Resets protocol statistics.
  static void resetStats() {
    _totalBytesSent = 0;
    _totalBytesReceived = 0;
    _messagesSent = 0;
    _messagesReceived = 0;
  }

  /// Decodes the message header (version and type).
  static (int version, int messageType) decodeHeader(Uint8List data) {
    if (data.length < 2) {
      throw Exception('Binary message too short for header');
    }
    return (data[0], data[1]);
  }

  /// Decodes an UpdateComponent message from binary.
  /// Returns (componentId, address).
  static (String componentId, int address) decodeUpdateMessage(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final componentIdLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final componentId =
        utf8.decode(data.sublist(offset, offset + componentIdLen));
    offset += componentIdLen;
    final address = byteData.getInt64(offset, Endian.little);

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return (componentId, address);
  }

  /// Decodes a BatchedUpdate message from binary.
  /// Returns list of (componentId, address) tuples.
  static List<(String componentId, int address)> decodeBatchedUpdate(
      Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final count = byteData.getInt32(offset, Endian.little);
    offset += 4;

    final updates = <(String, int)>[];
    for (var i = 0; i < count; i++) {
      final componentIdLen = byteData.getUint16(offset, Endian.little);
      offset += 2;
      final componentId =
          utf8.decode(data.sublist(offset, offset + componentIdLen));
      offset += componentIdLen;
      final address = byteData.getInt64(offset, Endian.little);
      offset += 8;
      updates.add((componentId, address));
    }

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return updates;
  }

  /// Decodes a DisposedComponent message from binary.
  /// Returns widgetId.
  static String decodeDisposedMessage(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final widgetIdLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final widgetId = utf8.decode(data.sublist(offset, offset + widgetIdLen));

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return widgetId;
  }

  /// Decodes an Error message from binary.
  /// Returns (message, stackTrace).
  static (String message, String? stackTrace) decodeErrorMessage(
      Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final messageLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final message = utf8.decode(data.sublist(offset, offset + messageLen));
    offset += messageLen;

    final stackTraceLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final stackTrace = stackTraceLen > 0
        ? utf8.decode(data.sublist(offset, offset + stackTraceLen))
        : null;

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return (message, stackTrace);
  }

  /// Decodes a Lifecycle message from binary.
  /// Returns state as int (0=Resumed, 1=Inactive, 2=Paused, 3=Detached).
  static int decodeLifecycleMessage(Uint8List data) {
    _totalBytesReceived += data.length;
    _messagesReceived++;
    return data[2];
  }

  /// Decodes a ScrollCommand message from binary.
  /// Returns (controllerId, command, offset, durationMs, curve).
  static (
    String controllerId,
    int command,
    double offset,
    int durationMs,
    String? curve
  ) decodeScrollCommand(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final controllerIdLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final controllerId =
        utf8.decode(data.sublist(offset, offset + controllerIdLen));
    offset += controllerIdLen;

    final command = data[offset++];
    final scrollOffset = byteData.getFloat64(offset, Endian.little);
    offset += 8;
    final durationMs = byteData.getInt32(offset, Endian.little);
    offset += 4;

    final curveLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final curve = curveLen > 0
        ? utf8.decode(data.sublist(offset, offset + curveLen))
        : null;

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return (controllerId, command, scrollOffset, durationMs, curve);
  }

  /// Decodes a StateNotify message from binary.
  /// Returns (notifierId, jsonValue, timestampTicks).
  static (String notifierId, String jsonValue, int timestampTicks)
      decodeStateNotify(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final notifierIdLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final notifierId =
        utf8.decode(data.sublist(offset, offset + notifierIdLen));
    offset += notifierIdLen;

    final valueLen = byteData.getInt32(offset, Endian.little);
    offset += 4;
    final jsonValue = utf8.decode(data.sublist(offset, offset + valueLen));
    offset += valueLen;

    final timestampTicks = byteData.getInt64(offset, Endian.little);

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return (notifierId, jsonValue, timestampTicks);
  }

  /// Decodes a HotReload notification from binary.
  /// Returns (success, widgetType, durationMs, error).
  static (bool success, String widgetType, int durationMs, String? error)
      decodeHotReloadNotification(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final success = data[offset++] == 1;

    final widgetTypeLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final widgetType =
        utf8.decode(data.sublist(offset, offset + widgetTypeLen));
    offset += widgetTypeLen;

    final durationMs = byteData.getInt32(offset, Endian.little);
    offset += 4;

    final errorLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final error = errorLen > 0
        ? utf8.decode(data.sublist(offset, offset + errorLen))
        : null;

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return (success, widgetType, durationMs, error);
  }

  /// Decodes an AsyncCallbackComplete message from binary.
  /// Returns callbackId.
  static String decodeAsyncCallbackComplete(Uint8List data) {
    final byteData = ByteData.sublistView(data);
    var offset = 2; // Skip header

    final callbackIdLen = byteData.getUint16(offset, Endian.little);
    offset += 2;
    final callbackId =
        utf8.decode(data.sublist(offset, offset + callbackIdLen));

    _totalBytesReceived += data.length;
    _messagesReceived++;

    return callbackId;
  }

  // Encoding methods for Dart→C# messages

  /// Encodes a scroll update message to binary.
  /// Format: [Version:1][Type:1][ControllerIdLen:2][ControllerId:N][Offset:8][MaxExtent:8][ViewportDim:8][EventType:1]
  static Uint8List encodeScrollUpdate(
    String controllerId,
    double offset,
    double maxExtent,
    double viewportDimension,
    int eventType,
  ) {
    final controllerIdBytes = utf8.encode(controllerId);
    final buffer =
        ByteData(1 + 1 + 2 + controllerIdBytes.length + 8 + 8 + 8 + 1);
    var pos = 0;

    buffer.setUint8(pos++, protocolVersion);
    buffer.setUint8(pos++, MessageTypes.scrollUpdate);
    buffer.setUint16(pos, controllerIdBytes.length, Endian.little);
    pos += 2;

    final result = Uint8List.view(buffer.buffer);
    result.setRange(pos, pos + controllerIdBytes.length, controllerIdBytes);
    pos += controllerIdBytes.length;

    buffer.setFloat64(pos, offset, Endian.little);
    pos += 8;
    buffer.setFloat64(pos, maxExtent, Endian.little);
    pos += 8;
    buffer.setFloat64(pos, viewportDimension, Endian.little);
    pos += 8;
    buffer.setUint8(pos, eventType);

    _totalBytesSent += result.length;
    _messagesSent++;

    return result;
  }

  /// Encodes an event message to binary.
  /// Format: [Version:1][Type:1][EventNameLen:2][EventName:N][DataLen:4][Data:N][NeedsReturn:1]
  static Uint8List encodeEventMessage(
    String eventName,
    String jsonData,
    bool needsReturn,
  ) {
    final eventNameBytes = utf8.encode(eventName);
    final dataBytes = utf8.encode(jsonData);
    final buffer =
        ByteData(1 + 1 + 2 + eventNameBytes.length + 4 + dataBytes.length + 1);
    var pos = 0;

    buffer.setUint8(pos++, protocolVersion);
    buffer.setUint8(pos++, MessageTypes.event);
    buffer.setUint16(pos, eventNameBytes.length, Endian.little);
    pos += 2;

    final result = Uint8List.view(buffer.buffer);
    result.setRange(pos, pos + eventNameBytes.length, eventNameBytes);
    pos += eventNameBytes.length;

    buffer.setInt32(pos, dataBytes.length, Endian.little);
    pos += 4;
    result.setRange(pos, pos + dataBytes.length, dataBytes);
    pos += dataBytes.length;

    buffer.setUint8(pos, needsReturn ? 1 : 0);

    _totalBytesSent += result.length;
    _messagesSent++;

    return result;
  }
}

/// Message type identifiers matching C# BinaryProtocol.MessageTypes.
class MessageTypes {
  static const int updateComponent = 0x01;
  static const int batchedUpdate = 0x02;
  static const int event = 0x03;
  static const int stateNotify = 0x04;
  static const int disposed = 0x05;
  static const int error = 0x06;
  static const int lifecycle = 0x07;
  static const int memoryWarning = 0x08;
  static const int scrollUpdate = 0x09;
  static const int scrollCommand = 0x0A;
  static const int hotReload = 0x0B;
  static const int containerSize = 0x0C;
  static const int dartException = 0x0D;
  static const int partialUpdate = 0x0E;
  static const int asyncCallbackComplete = 0x0F;
}

/// Statistics about binary protocol usage.
class BinaryProtocolStats {
  final int totalBytesSent;
  final int totalBytesReceived;
  final int messagesSent;
  final int messagesReceived;
  final bool isEnabled;

  BinaryProtocolStats({
    required this.totalBytesSent,
    required this.totalBytesReceived,
    required this.messagesSent,
    required this.messagesReceived,
    required this.isEnabled,
  });

  double get averageMessageSize =>
      messagesSent > 0 ? totalBytesSent / messagesSent : 0;

  @override
  String toString() {
    return 'Binary Protocol: ${isEnabled ? "On" : "Off"}, '
        'Sent: $totalBytesSent bytes ($messagesSent msgs), '
        'Received: $totalBytesReceived bytes ($messagesReceived msgs), '
        'Avg Size: ${averageMessageSize.toStringAsFixed(1)} bytes';
  }
}
