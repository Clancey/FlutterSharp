// Manual parser for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

import 'dart:ffi';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Navigator widget.
///
/// Parses the Navigator FFI struct and builds a Flutter Navigator widget
/// that displays the current route's child widget.
class NavigatorParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<NavigatorStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse initial route
    final initialRoute = map.hasInitialRoute == 1
        ? parseString(map.initialRoute) ?? '/'
        : '/';

    // Parse current route
    final currentRoute = map.hasCurrentRoute == 1
        ? parseString(map.currentRoute) ?? initialRoute
        : initialRoute;

    // Parse navigator ID
    final navigatorId = map.hasNavigatorId == 1
        ? parseString(map.navigatorId)
        : null;

    // Parse route names (pipe-separated) - currently unused but reserved for future use
    // final routeNamesStr = map.hasRouteNames == 1
    //     ? parseString(map.routeNames)
    //     : null;
    // final routeNames = routeNamesStr?.split('|') ?? [];

    // Parse maintain state - currently unused but reserved for future use
    // final maintainState = map.maintainState == 1;

    // Parse clip behavior
    final clipBehavior = Clip.values[map.clipBehavior.clamp(0, Clip.values.length - 1)];

    // Parse callback action IDs
    final onRouteChangedAction = map.hasOnRouteChangedAction == 1
        ? parseString(map.onRouteChangedAction)
        : null;

    final onPopAction = map.hasOnPopAction == 1
        ? parseString(map.onPopAction)
        : null;

    // Build the current route's child widget
    Widget? currentChild;
    if (map.currentChild.address != 0) {
      currentChild = DynamicWidgetBuilder.buildFromPointer(map.currentChild, buildContext);
    }

    // If we have a child, wrap it in a basic Navigator structure
    // This provides back button support and navigation context
    if (currentChild != null) {
      return _NavigatorWrapper(
        key: ValueKey(navigatorId ?? id),
        currentRoute: currentRoute,
        currentChild: currentChild,
        onRouteChangedAction: onRouteChangedAction,
        onPopAction: onPopAction,
        clipBehavior: clipBehavior,
      );
    }

    // Fallback: display error message
    return Center(
      child: Text(
        'Navigator: No route widget for "$currentRoute"',
        style: const TextStyle(color: Colors.red),
      ),
    );
  }

  @override
  String get widgetName => "Navigator";
}

/// Internal wrapper widget that provides Navigator functionality.
///
/// This widget wraps the current route's child and handles
/// back navigation events.
class _NavigatorWrapper extends StatefulWidget {
  final String currentRoute;
  final Widget currentChild;
  final String? onRouteChangedAction;
  final String? onPopAction;
  final Clip clipBehavior;

  const _NavigatorWrapper({
    super.key,
    required this.currentRoute,
    required this.currentChild,
    this.onRouteChangedAction,
    this.onPopAction,
    this.clipBehavior = Clip.hardEdge,
  });

  @override
  State<_NavigatorWrapper> createState() => _NavigatorWrapperState();
}

class _NavigatorWrapperState extends State<_NavigatorWrapper> {
  @override
  Widget build(BuildContext context) {
    // Use PopScope (formerly WillPopScope) to intercept back button
    return PopScope(
      canPop: false, // We handle pops manually via C#
      onPopInvokedWithResult: (didPop, result) async {
        if (didPop) return;

        // Notify C# that pop was requested
        if (widget.onPopAction != null) {
          await _invokePopAction(widget.onPopAction!, widget.currentRoute);
        }
      },
      child: ClipRect(
        clipBehavior: widget.clipBehavior,
        child: widget.currentChild,
      ),
    );
  }

  /// Invokes the pop action callback on the C# side
  Future<void> _invokePopAction(String actionId, String routeName) async {
    try {
      await methodChannel.invokeMethod('HandleAction', {
        'actionId': actionId,
        'widgetType': 'Navigator',
        'routeName': routeName,
      });
    } catch (e) {
      debugPrint('Error invoking pop action $actionId: $e');
    }
  }
}
