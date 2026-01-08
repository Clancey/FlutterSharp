import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'mauiRenderer.dart';

/// Manages ScrollController instances and their communication with C#.
///
/// This class creates and tracks ScrollController instances by their C# IDs,
/// listens to scroll position changes, and sends updates to C# via method channel.
class ScrollControllerManager {
  static final ScrollControllerManager _instance = ScrollControllerManager._internal();
  factory ScrollControllerManager() => _instance;
  ScrollControllerManager._internal();

  /// Map of controller IDs to their ScrollController instances
  final Map<String, _ManagedScrollController> _controllers = {};

  /// Gets or creates a ScrollController for the given ID.
  ///
  /// If a controller with this ID already exists, it is returned.
  /// Otherwise, a new controller is created and registered.
  ScrollController getController(String controllerId, {
    double initialScrollOffset = 0.0,
    bool keepScrollOffset = true,
    String? debugLabel,
  }) {
    if (_controllers.containsKey(controllerId)) {
      return _controllers[controllerId]!.controller;
    }

    final controller = ScrollController(
      initialScrollOffset: initialScrollOffset,
      keepScrollOffset: keepScrollOffset,
      debugLabel: debugLabel,
    );

    final managed = _ManagedScrollController(
      controllerId: controllerId,
      controller: controller,
      onUpdate: _sendScrollUpdate,
    );

    _controllers[controllerId] = managed;
    managed.startListening();

    // Notify C# that a controller was attached
    _sendScrollUpdate(controllerId, 'attach', controller.hasClients ? controller.offset : 0.0, null);

    return controller;
  }

  /// Disposes a controller by ID.
  void disposeController(String controllerId) {
    final managed = _controllers.remove(controllerId);
    if (managed != null) {
      managed.dispose();
      // Notify C# that a controller was detached
      _sendScrollUpdate(controllerId, 'detach', 0.0, null);
    }
  }

  /// Checks if a controller exists for the given ID.
  bool hasController(String controllerId) {
    return _controllers.containsKey(controllerId);
  }

  /// Handles a scroll command from C#.
  Future<void> handleScrollCommand(Map<String, dynamic> message) async {
    final controllerId = message['controllerId'] as String?;
    final command = message['command'] as String?;
    final offset = (message['offset'] as num?)?.toDouble() ?? 0.0;
    final durationMs = (message['durationMs'] as num?)?.toDouble();
    final curve = message['curve'] as String?;

    if (controllerId == null || command == null) {
      debugPrint('ScrollControllerManager: Invalid scroll command message');
      return;
    }

    final managed = _controllers[controllerId];
    if (managed == null || !managed.controller.hasClients) {
      debugPrint('ScrollControllerManager: Controller not found or has no clients: $controllerId');
      return;
    }

    final controller = managed.controller;

    switch (command) {
      case 'jumpTo':
        controller.jumpTo(offset);
        break;

      case 'animateTo':
        final duration = durationMs != null
            ? Duration(milliseconds: durationMs.toInt())
            : const Duration(milliseconds: 300);
        final animationCurve = _parseCurve(curve);
        await controller.animateTo(
          offset,
          duration: duration,
          curve: animationCurve,
        );
        break;

      default:
        debugPrint('ScrollControllerManager: Unknown command: $command');
    }
  }

  /// Sends a scroll update to C#.
  void _sendScrollUpdate(
    String controllerId,
    String eventType,
    double offset,
    ScrollMetrics? metrics,
  ) {
    final message = {
      'controllerId': controllerId,
      'eventType': eventType,
      'offset': offset,
      if (metrics != null) ...{
        'maxScrollExtent': metrics.maxScrollExtent,
        'minScrollExtent': metrics.minScrollExtent,
        'viewportDimension': metrics.viewportDimension,
        'hasClients': true,
        'axisDirection': metrics.axisDirection.index,
      },
    };

    try {
      methodChannel.invokeMethod('ScrollUpdate', json.encode(message));
    } catch (e) {
      debugPrint('ScrollControllerManager: Error sending scroll update: $e');
    }
  }

  /// Parses a curve name to a Curve object.
  Curve _parseCurve(String? curveName) {
    switch (curveName) {
      case 'linear':
        return Curves.linear;
      case 'easeIn':
        return Curves.easeIn;
      case 'easeOut':
        return Curves.easeOut;
      case 'easeInOut':
        return Curves.easeInOut;
      case 'fastOutSlowIn':
        return Curves.fastOutSlowIn;
      case 'bounceIn':
        return Curves.bounceIn;
      case 'bounceOut':
        return Curves.bounceOut;
      case 'bounceInOut':
        return Curves.bounceInOut;
      case 'elasticIn':
        return Curves.elasticIn;
      case 'elasticOut':
        return Curves.elasticOut;
      case 'elasticInOut':
        return Curves.elasticInOut;
      case 'decelerate':
        return Curves.decelerate;
      default:
        return Curves.easeInOut;
    }
  }

  /// Disposes all controllers.
  void disposeAll() {
    for (final managed in _controllers.values) {
      managed.dispose();
    }
    _controllers.clear();
  }
}

/// Wraps a ScrollController and listens to its position changes.
class _ManagedScrollController {
  final String controllerId;
  final ScrollController controller;
  final void Function(String, String, double, ScrollMetrics?) onUpdate;

  bool _isListening = false;
  bool _isScrolling = false;

  _ManagedScrollController({
    required this.controllerId,
    required this.controller,
    required this.onUpdate,
  });

  void startListening() {
    if (_isListening) return;
    _isListening = true;

    controller.addListener(_onScrollPositionChanged);
  }

  void _onScrollPositionChanged() {
    if (!controller.hasClients) return;

    final position = controller.position;

    // Determine event type based on scroll activity
    String eventType;
    if (position.isScrollingNotifier.value && !_isScrolling) {
      _isScrolling = true;
      eventType = 'scrollStart';
    } else if (!position.isScrollingNotifier.value && _isScrolling) {
      _isScrolling = false;
      eventType = 'scrollEnd';
    } else {
      eventType = 'scrollUpdate';
    }

    onUpdate(controllerId, eventType, position.pixels, position);
  }

  void dispose() {
    if (_isListening) {
      controller.removeListener(_onScrollPositionChanged);
      _isListening = false;
    }
    controller.dispose();
  }
}

/// Global instance for easy access
final scrollControllerManager = ScrollControllerManager();
