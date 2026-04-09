// Manual parser for RefreshIndicator widget
// Part of FlutterSharp Phase 5 - Scrolling Widgets

import 'dart:async';
import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/refreshindicator_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for Material Design RefreshIndicator widget.
///
/// A widget that supports the Material "swipe to refresh" idiom.
/// When the child's Scrollable descendant overscrolls, an animated
/// circular progress indicator is faded into view.
class RefreshIndicatorParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map =
        Pointer<RefreshIndicatorStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for event dispatching
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse onRefresh callback action ID
    final onRefreshAction =
        map.hasOnRefresh == 1 ? parseString(map.onRefreshAction) : null;

    // Parse child widget
    final child = map.child.address != 0
        ? DynamicWidgetBuilder.buildFromPointer(
            map.child.cast<WidgetStruct>(), buildContext)
        : null;

    if (child == null) {
      debugPrint('RefreshIndicator: child is required');
      return null;
    }

    // Parse colors
    Color? color;
    if (map.hasColor == 1 && map.color != 0) {
      color = Color(map.color);
    }

    Color? backgroundColor;
    if (map.hasBackgroundColor == 1 && map.backgroundColor != 0) {
      backgroundColor = Color(map.backgroundColor);
    }

    // Parse trigger mode
    RefreshIndicatorTriggerMode triggerMode =
        RefreshIndicatorTriggerMode.onEdge;
    if (map.triggerMode >= 0 &&
        map.triggerMode < RefreshIndicatorTriggerMode.values.length) {
      triggerMode = RefreshIndicatorTriggerMode.values[map.triggerMode];
    }

    // Parse other properties with defaults
    final displacement = map.displacement > 0 ? map.displacement : 40.0;
    final edgeOffset = map.edgeOffset;
    final strokeWidth = map.strokeWidth > 0 ? map.strokeWidth : 2.5;

    return RefreshIndicator(
      key: ValueKey(id),
      child: child,
      onRefresh: onRefreshAction != null
          ? () => _createRefreshFuture(id, onRefreshAction)
          : () async {}, // Dummy future if no callback
      displacement: displacement,
      edgeOffset: edgeOffset,
      color: color,
      backgroundColor: backgroundColor,
      triggerMode: triggerMode,
      strokeWidth: strokeWidth,
    );
  }

  /// Creates a Future that completes when C# signals completion.
  Future<void> _createRefreshFuture(String widgetId, String actionId) {
    final completer = Completer<void>();

    // Register a listener for the async callback completion
    _asyncCallbackCompleters[widgetId] = completer;

    // Invoke the action on C# side
    raiseMauiEvent(actionId, "invoke", null);

    // Return the future that will complete when C# signals done
    // Add a timeout in case the C# side never responds
    return completer.future.timeout(
      const Duration(seconds: 60),
      onTimeout: () {
        _asyncCallbackCompleters.remove(widgetId);
        debugPrint(
            'RefreshIndicator: onRefresh timed out for widget $widgetId');
      },
    );
  }

  @override
  String get widgetName => "RefreshIndicator";
}

/// Map of widget IDs to their pending async callback completers.
final Map<String, Completer<void>> _asyncCallbackCompleters = {};

/// Called by mauiRenderer when C# signals that an async callback has completed.
void completeAsyncCallback(String widgetId) {
  final completer = _asyncCallbackCompleters.remove(widgetId);
  if (completer != null && !completer.isCompleted) {
    completer.complete();
  }
}
