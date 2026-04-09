import 'dart:async';
import 'dart:convert';
import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';

/// MAUI navigation types
enum MauiNavigationType {
  push,
  pop,
  shell,
  modal,
  popToRoot,
  replace,
  unknown,
}

/// Represents a MAUI navigation event
class MauiNavigationEvent {
  final String fromRoute;
  final String toRoute;
  final MauiNavigationType navigationType;
  final String? source;

  MauiNavigationEvent({
    required this.fromRoute,
    required this.toRoute,
    required this.navigationType,
    this.source,
  });
}

/// Bridge between Flutter and MAUI navigation systems.
/// Allows Flutter widgets to request MAUI navigation and receive navigation events.
class MauiNavigationBridge {
  MauiNavigationBridge._();

  static final MauiNavigationBridge _instance = MauiNavigationBridge._();

  /// Singleton instance
  static MauiNavigationBridge get instance => _instance;

  /// Method channel for communication with C#
  static const MethodChannel _channel =
      MethodChannel('com.Microsoft.FlutterSharp/Messages');

  /// Stream controller for navigation events
  final _navigatingController =
      StreamController<MauiNavigationEvent>.broadcast();
  final _navigatedController =
      StreamController<MauiNavigationEvent>.broadcast();

  /// Current MAUI route
  String _currentRoute = '';

  /// Gets the current MAUI route
  String get currentRoute => _currentRoute;

  /// Stream of navigation starting events
  Stream<MauiNavigationEvent> get onNavigating => _navigatingController.stream;

  /// Stream of navigation completed events
  Stream<MauiNavigationEvent> get onNavigated => _navigatedController.stream;

  /// List of navigation event listeners
  final List<void Function(MauiNavigationEvent)> _navigatedListeners = [];
  final List<void Function(MauiNavigationEvent)> _navigatingListeners = [];

  /// Add a listener for navigation completed events
  void addNavigatedListener(void Function(MauiNavigationEvent) listener) {
    _navigatedListeners.add(listener);
  }

  /// Remove a navigation completed listener
  void removeNavigatedListener(void Function(MauiNavigationEvent) listener) {
    _navigatedListeners.remove(listener);
  }

  /// Add a listener for navigation starting events
  void addNavigatingListener(void Function(MauiNavigationEvent) listener) {
    _navigatingListeners.add(listener);
  }

  /// Remove a navigation starting listener
  void removeNavigatingListener(void Function(MauiNavigationEvent) listener) {
    _navigatingListeners.remove(listener);
  }

  /// Handles navigation starting message from C#
  void handleNavigating({
    required String from,
    required String to,
    required String navigationType,
  }) {
    final event = MauiNavigationEvent(
      fromRoute: from,
      toRoute: to,
      navigationType: _parseNavigationType(navigationType),
    );

    _navigatingController.add(event);

    for (final listener in _navigatingListeners) {
      try {
        listener(event);
      } catch (e) {
        debugPrint('Error in navigation listener: $e');
      }
    }
  }

  /// Handles navigation completed message from C#
  void handleNavigated({
    required String from,
    required String to,
    required String navigationType,
    required String source,
  }) {
    _currentRoute = to;

    final event = MauiNavigationEvent(
      fromRoute: from,
      toRoute: to,
      navigationType: _parseNavigationType(navigationType),
      source: source,
    );

    _navigatedController.add(event);

    for (final listener in _navigatedListeners) {
      try {
        listener(event);
      } catch (e) {
        debugPrint('Error in navigation listener: $e');
      }
    }
  }

  /// Parse navigation type string to enum
  MauiNavigationType _parseNavigationType(String type) {
    switch (type.toLowerCase()) {
      case 'push':
        return MauiNavigationType.push;
      case 'pop':
        return MauiNavigationType.pop;
      case 'shell':
        return MauiNavigationType.shell;
      case 'modal':
        return MauiNavigationType.modal;
      case 'poptoroot':
        return MauiNavigationType.popToRoot;
      case 'replace':
        return MauiNavigationType.replace;
      default:
        return MauiNavigationType.unknown;
    }
  }

  /// Request MAUI to push a page by route name
  Future<bool> push(String route, {bool animate = true}) async {
    return await _sendNavigationRequest('push', route, animate: animate);
  }

  /// Request MAUI to pop the current page
  Future<bool> pop({bool animate = true}) async {
    return await _sendNavigationRequest('pop', '', animate: animate);
  }

  /// Request MAUI to pop to the root page
  Future<bool> popToRoot({bool animate = true}) async {
    return await _sendNavigationRequest('popToRoot', '', animate: animate);
  }

  /// Request MAUI to push a modal page
  Future<bool> pushModal(String route, {bool animate = true}) async {
    return await _sendNavigationRequest('pushModal', route, animate: animate);
  }

  /// Request MAUI to pop a modal page
  Future<bool> popModal({bool animate = true}) async {
    return await _sendNavigationRequest('popModal', '', animate: animate);
  }

  /// Request MAUI Shell navigation to a route
  Future<bool> goTo(String route, {bool animate = true}) async {
    return await _sendNavigationRequest('goto', route,
        animate: animate, useShellNavigation: true);
  }

  /// Request MAUI to go back (Shell ".." navigation)
  Future<bool> goBack({bool animate = true}) async {
    return await _sendNavigationRequest('goBack', '', animate: animate);
  }

  /// Send a navigation request to MAUI
  Future<bool> _sendNavigationRequest(
    String action,
    String route, {
    bool animate = true,
    bool useShellNavigation = false,
    Map<String, dynamic>? parameters,
  }) async {
    try {
      final request = {
        'action': action,
        'route': route,
        'animate': animate,
        'useShellNavigation': useShellNavigation,
        if (parameters != null) 'parameters': parameters,
      };

      final event = {
        'componentId': '0',
        'eventName': 'MauiNavigation',
        'data': jsonEncode(request),
      };

      final response = await _channel.invokeMethod('Event', jsonEncode(event));

      if (response is String) {
        try {
          final result = jsonDecode(response) as Map<String, dynamic>;
          return result['success'] == true;
        } catch (e) {
          debugPrint('Error parsing navigation response: $e');
          return false;
        }
      }

      return true;
    } catch (e) {
      debugPrint('Error sending navigation request: $e');
      return false;
    }
  }

  /// Dispose resources
  void dispose() {
    _navigatingController.close();
    _navigatedController.close();
    _navigatedListeners.clear();
    _navigatingListeners.clear();
  }
}

/// Convenience extension for easy access to MAUI navigation
extension MauiNavigation on Object {
  /// Get the MAUI navigation bridge instance
  MauiNavigationBridge get mauiNav => MauiNavigationBridge.instance;
}

/// Widget that rebuilds when MAUI navigation occurs
class MauiNavigationListener extends StatefulWidget {
  final Widget child;
  final void Function(MauiNavigationEvent)? onNavigating;
  final void Function(MauiNavigationEvent)? onNavigated;

  const MauiNavigationListener({
    Key? key,
    required this.child,
    this.onNavigating,
    this.onNavigated,
  }) : super(key: key);

  @override
  State<MauiNavigationListener> createState() => _MauiNavigationListenerState();
}

class _MauiNavigationListenerState extends State<MauiNavigationListener> {
  @override
  void initState() {
    super.initState();
    if (widget.onNavigating != null) {
      MauiNavigationBridge.instance.addNavigatingListener(widget.onNavigating!);
    }
    if (widget.onNavigated != null) {
      MauiNavigationBridge.instance.addNavigatedListener(widget.onNavigated!);
    }
  }

  @override
  void dispose() {
    if (widget.onNavigating != null) {
      MauiNavigationBridge.instance
          .removeNavigatingListener(widget.onNavigating!);
    }
    if (widget.onNavigated != null) {
      MauiNavigationBridge.instance
          .removeNavigatedListener(widget.onNavigated!);
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => widget.child;
}
