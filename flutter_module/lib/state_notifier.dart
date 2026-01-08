import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'mauiRenderer.dart';

/// Manages bidirectional state synchronization between Dart and C#.
///
/// This class provides:
/// - Registration of Dart-side listeners for C# notifiers
/// - Sending state changes from Dart to C#
/// - Handling state changes from C# to Dart
class StateNotifier {
  /// Registry of value notifiers by notifier ID
  static final Map<String, ValueNotifier<dynamic>> _notifiers = {};

  /// Registry of listeners to notify when state changes
  static final Map<String, List<VoidCallback>> _listeners = {};

  /// Last known values for each notifier (for change detection)
  static final Map<String, dynamic> _lastValues = {};

  /// Whether to suppress the next update to C# (to prevent loops)
  static final Set<String> _suppressNextUpdate = {};

  /// Creates or retrieves a ValueNotifier for the given notifier ID.
  /// This notifier will automatically sync with C#.
  static ValueNotifier<T> of<T>(String notifierId, {T? initialValue}) {
    if (_notifiers.containsKey(notifierId)) {
      return _notifiers[notifierId] as ValueNotifier<T>;
    }

    final notifier = ValueNotifier<T>(initialValue as T);
    _notifiers[notifierId] = notifier;
    _lastValues[notifierId] = initialValue;

    // Listen to local changes and sync to C#
    notifier.addListener(() {
      if (_suppressNextUpdate.contains(notifierId)) {
        _suppressNextUpdate.remove(notifierId);
        return;
      }
      _notifyDotNet(notifierId, notifier.value);
    });

    return notifier;
  }

  /// Registers a callback to be called when the notifier value changes.
  /// Returns a dispose function to remove the listener.
  static VoidCallback addListener(String notifierId, VoidCallback callback) {
    _listeners.putIfAbsent(notifierId, () => []);
    _listeners[notifierId]!.add(callback);

    return () {
      _listeners[notifierId]?.remove(callback);
    };
  }

  /// Gets the current value for a notifier, or null if not registered.
  static T? getValue<T>(String notifierId) {
    final notifier = _notifiers[notifierId];
    if (notifier == null) return null;
    return notifier.value as T?;
  }

  /// Sets the value for a notifier and syncs to C#.
  static void setValue<T>(String notifierId, T value, {String? sourceWidgetId}) {
    final notifier = _notifiers[notifierId];
    if (notifier != null) {
      notifier.value = value;
    } else {
      // Create a new notifier if it doesn't exist
      final newNotifier = of<T>(notifierId, initialValue: value);
      newNotifier.value = value;
    }

    // Notify C# of the change
    _notifyDotNet(notifierId, value, sourceWidgetId: sourceWidgetId);
  }

  /// Handles a state change notification from C#.
  /// This updates the Dart-side value without triggering a sync back to C#.
  static void handleStateChanged(String notifierId, dynamic value) {
    // Suppress the next update to prevent loops
    _suppressNextUpdate.add(notifierId);

    final notifier = _notifiers[notifierId];
    if (notifier != null) {
      notifier.value = value;
    } else {
      // Create a new notifier if it doesn't exist
      final newNotifier = ValueNotifier(value);
      _notifiers[notifierId] = newNotifier;
    }

    _lastValues[notifierId] = value;

    // Notify all registered listeners
    final listeners = _listeners[notifierId];
    if (listeners != null) {
      for (final listener in listeners) {
        try {
          listener();
        } catch (e) {
          debugPrint('StateNotifier: Error in listener for $notifierId: $e');
        }
      }
    }
  }

  /// Sends a state notification to C#.
  static Future<void> _notifyDotNet(String notifierId, dynamic value, {String? sourceWidgetId}) async {
    // Check if value actually changed
    if (_lastValues[notifierId] == value) {
      return;
    }
    _lastValues[notifierId] = value;

    try {
      final message = {
        'notifierId': notifierId,
        'value': value,
        'valueType': _getValueType(value),
        'timestamp': DateTime.now().millisecondsSinceEpoch,
        'sourceWidgetId': sourceWidgetId,
      };

      await methodChannel.invokeMethod('StateNotify', json.encode(message));
    } catch (e) {
      debugPrint('StateNotifier: Error notifying C#: $e');
    }
  }

  /// Gets a type name for serialization.
  static String _getValueType(dynamic value) {
    if (value == null) return 'null';
    if (value is String) return 'string';
    if (value is int) return 'int';
    if (value is double) return 'double';
    if (value is bool) return 'bool';
    if (value is List) return 'list';
    if (value is Map) return 'map';
    return value.runtimeType.toString();
  }

  /// Disposes a notifier and removes all listeners.
  static void dispose(String notifierId) {
    final notifier = _notifiers.remove(notifierId);
    notifier?.dispose();
    _listeners.remove(notifierId);
    _lastValues.remove(notifierId);
    _suppressNextUpdate.remove(notifierId);
  }

  /// Clears all state. Use for testing or app reset.
  static void clear() {
    for (final notifier in _notifiers.values) {
      notifier.dispose();
    }
    _notifiers.clear();
    _listeners.clear();
    _lastValues.clear();
    _suppressNextUpdate.clear();
  }
}

/// A widget that provides a bidirectional binding to a C# BidirectionalNotifier.
///
/// Usage:
/// ```dart
/// BidirectionalBinding<int>(
///   notifierId: 'counter_notifier_1',
///   initialValue: 0,
///   builder: (context, value, setValue) {
///     return Text('Count: $value');
///   },
/// )
/// ```
class BidirectionalBinding<T> extends StatefulWidget {
  /// The ID of the C# BidirectionalNotifier to bind to.
  final String notifierId;

  /// Initial value if the notifier hasn't been registered yet.
  final T? initialValue;

  /// Builder function that receives the current value and a setter.
  final Widget Function(BuildContext context, T? value, void Function(T) setValue) builder;

  /// Optional widget ID for tracking the source of changes.
  final String? widgetId;

  const BidirectionalBinding({
    Key? key,
    required this.notifierId,
    this.initialValue,
    required this.builder,
    this.widgetId,
  }) : super(key: key);

  @override
  State<BidirectionalBinding<T>> createState() => _BidirectionalBindingState<T>();
}

class _BidirectionalBindingState<T> extends State<BidirectionalBinding<T>> {
  late ValueNotifier<T?> _notifier;
  VoidCallback? _disposeListener;

  @override
  void initState() {
    super.initState();
    _notifier = StateNotifier.of<T?>(widget.notifierId, initialValue: widget.initialValue);

    // Listen for external changes
    _disposeListener = StateNotifier.addListener(widget.notifierId, () {
      if (mounted) {
        setState(() {});
      }
    });
  }

  @override
  void dispose() {
    _disposeListener?.call();
    super.dispose();
  }

  void _setValue(T value) {
    StateNotifier.setValue<T>(widget.notifierId, value, sourceWidgetId: widget.widgetId);
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder<T?>(
      valueListenable: _notifier,
      builder: (context, value, child) {
        return widget.builder(context, value, _setValue);
      },
    );
  }
}
