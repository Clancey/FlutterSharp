// FlutterSharp Widget Inspector Service
// Provides widget tree inspection and debugging capabilities

import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'mauiRenderer.dart';

/// Manager for controlling the FlutterSharp widget inspector.
class WidgetInspectorManager {
  static final WidgetInspectorManager _instance =
      WidgetInspectorManager._internal();
  factory WidgetInspectorManager() => _instance;
  WidgetInspectorManager._internal();

  static WidgetInspectorManager get instance => _instance;

  bool _isEnabled = false;
  bool _showOverlay = false;
  Element? _selectedElement;
  final _selectionController = StreamController<Element?>.broadcast();
  final _enabledController = StreamController<bool>.broadcast();

  /// Whether the widget inspector is enabled.
  bool get isEnabled => _isEnabled;

  /// Whether the visual overlay is showing.
  bool get showOverlay => _showOverlay;

  /// The currently selected element.
  Element? get selectedElement => _selectedElement;

  /// Stream of selection changes.
  Stream<Element?> get selectionStream => _selectionController.stream;

  /// Stream of enabled state changes.
  Stream<bool> get enabledStream => _enabledController.stream;

  /// Enables the widget inspector.
  void enable() {
    if (!_isEnabled) {
      _isEnabled = true;
      _enabledController.add(true);
    }
  }

  /// Disables the widget inspector.
  void disable() {
    if (_isEnabled) {
      _isEnabled = false;
      _selectedElement = null;
      _selectionController.add(null);
      _enabledController.add(false);
    }
  }

  /// Toggles the widget inspector.
  void toggle() {
    if (_isEnabled) {
      disable();
    } else {
      enable();
    }
  }

  /// Shows the visual overlay.
  void showInspectorOverlay() {
    _showOverlay = true;
  }

  /// Hides the visual overlay.
  void hideInspectorOverlay() {
    _showOverlay = false;
    _selectedElement = null;
    _selectionController.add(null);
  }

  /// Selects an element for inspection.
  void selectElement(Element? element) {
    _selectedElement = element;
    _selectionController.add(element);
  }

  /// Selects a widget by its debug information string.
  void selectByDebugInfo(String widgetType, int hashCode) {
    final element = _findElementByTypeAndHash(widgetType, hashCode);
    selectElement(element);
  }

  Element? _findElementByTypeAndHash(String widgetType, int hashCode) {
    Element? found;
    void visitor(Element element) {
      if (found != null) return;
      if (element.widget.runtimeType.toString() == widgetType &&
          element.widget.hashCode == hashCode) {
        found = element;
        return;
      }
      element.visitChildren(visitor);
    }

    WidgetsBinding.instance.rootElement?.visitChildren(visitor);
    return found;
  }

  void dispose() {
    _selectionController.close();
    _enabledController.close();
  }
}

/// Convenience getter for the widget inspector manager.
WidgetInspectorManager get widgetInspectorManager =>
    WidgetInspectorManager.instance;

/// Service for extracting widget tree information and sending to C#.
class WidgetInspectorService {
  static final WidgetInspectorService _instance =
      WidgetInspectorService._internal();
  factory WidgetInspectorService() => _instance;
  WidgetInspectorService._internal();

  static WidgetInspectorService get instance => _instance;

  bool _isInitialized = false;

  /// Initializes the widget inspector service and registers method channel handlers.
  void initialize() {
    if (_isInitialized) return;
    _isInitialized = true;
    _registerMethodHandlers();
  }

  void _registerMethodHandlers() {
    methodChannel.setMethodCallHandler((call) async {
      switch (call.method) {
        case 'inspector.enable':
          widgetInspectorManager.enable();
          return true;
        case 'inspector.disable':
          widgetInspectorManager.disable();
          return true;
        case 'inspector.toggle':
          widgetInspectorManager.toggle();
          return widgetInspectorManager.isEnabled;
        case 'inspector.showOverlay':
          widgetInspectorManager.showInspectorOverlay();
          return true;
        case 'inspector.hideOverlay':
          widgetInspectorManager.hideInspectorOverlay();
          return true;
        case 'inspector.getWidgetTree':
          final depth = call.arguments?['depth'] as int? ?? 10;
          return getWidgetTreeJson(maxDepth: depth);
        case 'inspector.getSelectedWidget':
          return getSelectedWidgetJson();
        case 'inspector.selectWidget':
          final widgetType = call.arguments?['widgetType'] as String?;
          final hashCode = call.arguments?['hashCode'] as int?;
          if (widgetType != null && hashCode != null) {
            widgetInspectorManager.selectByDebugInfo(widgetType, hashCode);
            return true;
          }
          return false;
        case 'inspector.getWidgetProperties':
          final hashCode = call.arguments?['hashCode'] as int?;
          if (hashCode != null) {
            return getWidgetPropertiesJson(hashCode);
          }
          return null;
        case 'inspector.getRenderObjectInfo':
          final hashCode = call.arguments?['hashCode'] as int?;
          if (hashCode != null) {
            return getRenderObjectInfoJson(hashCode);
          }
          return null;
        default:
          // Let other handlers process the call
          return null;
      }
    });
  }

  /// Gets the widget tree as JSON.
  String getWidgetTreeJson({int maxDepth = 10}) {
    final rootElement = WidgetsBinding.instance.rootElement;
    if (rootElement == null) {
      return '{"error": "No root element"}';
    }

    final tree = _buildWidgetTreeNode(rootElement, 0, maxDepth);
    return jsonEncode(tree);
  }

  Map<String, dynamic> _buildWidgetTreeNode(
      Element element, int depth, int maxDepth) {
    final widget = element.widget;
    final renderObject = element.renderObject;

    final node = <String, dynamic>{
      'widgetType': widget.runtimeType.toString(),
      'hashCode': widget.hashCode,
      'key': widget.key?.toString(),
      'depth': depth,
    };

    // Add render object info if available
    if (renderObject != null) {
      node['hasRenderObject'] = true;
      if (renderObject is RenderBox && renderObject.hasSize) {
        node['size'] = {
          'width': renderObject.size.width,
          'height': renderObject.size.height,
        };
      }
    }

    // Add widget-specific properties
    node['properties'] = _extractWidgetProperties(widget);

    // Recursively add children
    if (depth < maxDepth) {
      final children = <Map<String, dynamic>>[];
      element.visitChildren((child) {
        children.add(_buildWidgetTreeNode(child, depth + 1, maxDepth));
      });
      if (children.isNotEmpty) {
        node['children'] = children;
      }
    }

    return node;
  }

  Map<String, dynamic> _extractWidgetProperties(Widget widget) {
    final properties = <String, dynamic>{};

    // Extract common properties based on widget type
    if (widget is Text) {
      properties['data'] = widget.data;
      properties['style'] = widget.style?.toString();
      properties['textAlign'] = widget.textAlign?.toString();
      properties['maxLines'] = widget.maxLines;
      properties['overflow'] = widget.overflow?.toString();
    } else if (widget is Container) {
      properties['color'] = widget.color?.toARGB32().toRadixString(16);
      properties['padding'] = widget.padding?.toString();
      properties['margin'] = widget.margin?.toString();
      properties['alignment'] = widget.alignment?.toString();
    } else if (widget is Padding) {
      properties['padding'] = widget.padding.toString();
    } else if (widget is SizedBox) {
      properties['width'] = widget.width;
      properties['height'] = widget.height;
    } else if (widget is Opacity) {
      properties['opacity'] = widget.opacity;
    } else if (widget is Align) {
      properties['alignment'] = widget.alignment.toString();
      properties['widthFactor'] = widget.widthFactor;
      properties['heightFactor'] = widget.heightFactor;
    } else if (widget is Center) {
      properties['widthFactor'] = widget.widthFactor;
      properties['heightFactor'] = widget.heightFactor;
    } else if (widget is Flex) {
      properties['direction'] = widget.direction.toString();
      properties['mainAxisAlignment'] = widget.mainAxisAlignment.toString();
      properties['crossAxisAlignment'] = widget.crossAxisAlignment.toString();
      properties['mainAxisSize'] = widget.mainAxisSize.toString();
    } else if (widget is Flexible) {
      properties['flex'] = widget.flex;
      properties['fit'] = widget.fit.toString();
    } else if (widget is Icon) {
      properties['icon'] = widget.icon?.codePoint.toRadixString(16);
      properties['size'] = widget.size;
      properties['color'] = widget.color?.toARGB32().toRadixString(16);
    } else if (widget is Image) {
      properties['image'] = widget.image.toString();
      properties['width'] = widget.width;
      properties['height'] = widget.height;
      properties['fit'] = widget.fit?.toString();
    } else if (widget is ClipRRect) {
      properties['borderRadius'] = widget.borderRadius.toString();
    } else if (widget is DecoratedBox) {
      properties['decoration'] = widget.decoration.toString();
    }

    return properties;
  }

  /// Gets the selected widget as JSON.
  String? getSelectedWidgetJson() {
    final element = widgetInspectorManager.selectedElement;
    if (element == null) return null;

    final info = _buildDetailedWidgetInfo(element);
    return jsonEncode(info);
  }

  Map<String, dynamic> _buildDetailedWidgetInfo(Element element) {
    final widget = element.widget;
    final renderObject = element.renderObject;

    final info = <String, dynamic>{
      'widgetType': widget.runtimeType.toString(),
      'hashCode': widget.hashCode,
      'key': widget.key?.toString(),
      'properties': _extractWidgetProperties(widget),
    };

    // Add render object details
    if (renderObject != null) {
      info['renderObject'] = _buildRenderObjectInfo(renderObject);
    }

    // Add parent chain
    final parentChain = <String>[];
    Element? parent = element;
    while (parent != null && parentChain.length < 10) {
      parentChain.add(parent.widget.runtimeType.toString());
      parent = _getParent(parent);
    }
    info['parentChain'] = parentChain;

    return info;
  }

  Element? _getParent(Element element) {
    Element? parent;
    element.visitAncestorElements((ancestor) {
      parent = ancestor;
      return false; // Stop after first parent
    });
    return parent;
  }

  Map<String, dynamic> _buildRenderObjectInfo(RenderObject renderObject) {
    final info = <String, dynamic>{
      'type': renderObject.runtimeType.toString(),
      'hashCode': renderObject.hashCode,
      'needsPaint': renderObject.debugNeedsPaint,
      'needsLayout': renderObject.debugNeedsLayout,
    };

    if (renderObject is RenderBox) {
      if (renderObject.hasSize) {
        info['size'] = {
          'width': renderObject.size.width,
          'height': renderObject.size.height,
        };
      }
      info['constraints'] = renderObject.constraints.toString();
    }

    // Add paint bounds if available
    try {
      final bounds = renderObject.paintBounds;
      info['paintBounds'] = {
        'left': bounds.left,
        'top': bounds.top,
        'width': bounds.width,
        'height': bounds.height,
      };
    } catch (_) {
      // Some render objects may not have paint bounds
    }

    return info;
  }

  /// Gets properties for a widget by its hash code.
  String? getWidgetPropertiesJson(int hashCode) {
    final element = _findElementByHash(hashCode);
    if (element == null) return null;

    final properties = _extractWidgetProperties(element.widget);
    return jsonEncode(properties);
  }

  /// Gets render object info for a widget by its hash code.
  String? getRenderObjectInfoJson(int hashCode) {
    final element = _findElementByHash(hashCode);
    if (element == null) return null;

    final renderObject = element.renderObject;
    if (renderObject == null) return null;

    final info = _buildRenderObjectInfo(renderObject);
    return jsonEncode(info);
  }

  Element? _findElementByHash(int hashCode) {
    Element? found;
    void visitor(Element element) {
      if (found != null) return;
      if (element.widget.hashCode == hashCode) {
        found = element;
        return;
      }
      element.visitChildren(visitor);
    }

    WidgetsBinding.instance.rootElement?.visitChildren(visitor);
    return found;
  }

  /// Sends the widget tree to C#.
  void sendWidgetTreeToCSharp() {
    final treeJson = getWidgetTreeJson();
    methodChannel.invokeMethod('widgetTree', treeJson);
  }

  /// Sends selection change to C#.
  void notifySelectionChanged() {
    final element = widgetInspectorManager.selectedElement;
    if (element != null) {
      final info = _buildDetailedWidgetInfo(element);
      methodChannel.invokeMethod('widgetSelected', jsonEncode(info));
    } else {
      methodChannel.invokeMethod('widgetSelected', null);
    }
  }
}

/// Convenience getter for the widget inspector service.
WidgetInspectorService get widgetInspectorService =>
    WidgetInspectorService.instance;

/// A widget that provides visual widget inspection overlay.
class WidgetInspectorOverlay extends StatefulWidget {
  final Widget child;
  final bool enabled;

  const WidgetInspectorOverlay({
    super.key,
    required this.child,
    this.enabled = false,
  });

  @override
  State<WidgetInspectorOverlay> createState() => _WidgetInspectorOverlayState();
}

class _WidgetInspectorOverlayState extends State<WidgetInspectorOverlay> {
  late StreamSubscription<Element?> _selectionSubscription;
  late StreamSubscription<bool> _enabledSubscription;
  bool _isEnabled = false;
  Element? _selectedElement;
  Rect? _highlightRect;

  @override
  void initState() {
    super.initState();
    _isEnabled = widget.enabled || widgetInspectorManager.isEnabled;

    _selectionSubscription =
        widgetInspectorManager.selectionStream.listen((element) {
      setState(() {
        _selectedElement = element;
        _highlightRect = _getElementRect(element);
      });
    });

    _enabledSubscription =
        widgetInspectorManager.enabledStream.listen((enabled) {
      setState(() {
        _isEnabled = enabled;
        if (!enabled) {
          _selectedElement = null;
          _highlightRect = null;
        }
      });
    });

    // Initialize the service
    widgetInspectorService.initialize();
  }

  @override
  void dispose() {
    _selectionSubscription.cancel();
    _enabledSubscription.cancel();
    super.dispose();
  }

  Rect? _getElementRect(Element? element) {
    if (element == null) return null;

    final renderObject = element.renderObject;
    if (renderObject is! RenderBox) return null;
    if (!renderObject.hasSize) return null;

    try {
      final offset = renderObject.localToGlobal(Offset.zero);
      return offset & renderObject.size;
    } catch (_) {
      return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        // Wrap child in gesture detector for selection when enabled
        if (_isEnabled && widgetInspectorManager.showOverlay)
          GestureDetector(
            behavior: HitTestBehavior.translucent,
            onTapDown: _handleTapDown,
            child: widget.child,
          )
        else
          widget.child,

        // Highlight overlay
        if (_isEnabled && _highlightRect != null)
          Positioned(
            left: _highlightRect!.left,
            top: _highlightRect!.top,
            width: _highlightRect!.width,
            height: _highlightRect!.height,
            child: IgnorePointer(
              child: Container(
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.blue, width: 2),
                  color: Colors.blue.withValues(alpha: 0.1),
                ),
              ),
            ),
          ),

        // Selection info panel
        if (_isEnabled &&
            _selectedElement != null &&
            widgetInspectorManager.showOverlay)
          Positioned(
            bottom: 16,
            left: 16,
            right: 16,
            child: _SelectionInfoPanel(element: _selectedElement!),
          ),
      ],
    );
  }

  void _handleTapDown(TapDownDetails details) {
    final element = _findElementAtPosition(details.globalPosition);
    widgetInspectorManager.selectElement(element);
    widgetInspectorService.notifySelectionChanged();
  }

  Element? _findElementAtPosition(Offset position) {
    Element? result;
    int maxDepth = 0;

    void visitor(Element element, int depth) {
      final renderObject = element.renderObject;
      if (renderObject is RenderBox && renderObject.hasSize) {
        try {
          final box = renderObject;
          final topLeft = box.localToGlobal(Offset.zero);
          final rect = topLeft & box.size;

          if (rect.contains(position)) {
            if (depth > maxDepth) {
              maxDepth = depth;
              result = element;
            }
          }
        } catch (_) {
          // Ignore errors from unattached render objects
        }
      }

      element.visitChildren((child) => visitor(child, depth + 1));
    }

    WidgetsBinding.instance.rootElement
        ?.visitChildren((child) => visitor(child, 0));
    return result;
  }
}

class _SelectionInfoPanel extends StatelessWidget {
  final Element element;

  const _SelectionInfoPanel({required this.element});

  @override
  Widget build(BuildContext context) {
    final widget = element.widget;
    final renderObject = element.renderObject;

    return Card(
      elevation: 8,
      color: Colors.grey[900],
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              widget.runtimeType.toString(),
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            if (widget.key != null) _InfoRow('Key', widget.key.toString()),
            _InfoRow('Hash', widget.hashCode.toString()),
            if (renderObject is RenderBox && renderObject.hasSize) ...[
              _InfoRow('Size',
                  '${renderObject.size.width.toStringAsFixed(1)} x ${renderObject.size.height.toStringAsFixed(1)}'),
            ],
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                TextButton(
                  onPressed: () {
                    widgetInspectorManager.selectElement(null);
                    widgetInspectorService.notifySelectionChanged();
                  },
                  child:
                      const Text('Clear', style: TextStyle(color: Colors.grey)),
                ),
                const SizedBox(width: 8),
                TextButton(
                  onPressed: () {
                    widgetInspectorManager.hideInspectorOverlay();
                  },
                  child:
                      const Text('Close', style: TextStyle(color: Colors.blue)),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label;
  final String value;

  const _InfoRow(this.label, this.value);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 60,
            child: Text(
              '$label:',
              style: TextStyle(color: Colors.grey[400], fontSize: 12),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(color: Colors.white, fontSize: 12),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }
}

/// A FAB widget for toggling the widget inspector.
class WidgetInspectorToggle extends StatefulWidget {
  final Widget child;

  const WidgetInspectorToggle({super.key, required this.child});

  @override
  State<WidgetInspectorToggle> createState() => _WidgetInspectorToggleState();
}

class _WidgetInspectorToggleState extends State<WidgetInspectorToggle> {
  late StreamSubscription<bool> _subscription;
  bool _isEnabled = false;

  @override
  void initState() {
    super.initState();
    _isEnabled = widgetInspectorManager.isEnabled;
    _subscription = widgetInspectorManager.enabledStream.listen((enabled) {
      setState(() => _isEnabled = enabled);
    });
  }

  @override
  void dispose() {
    _subscription.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        widget.child,
        Positioned(
          right: 16,
          bottom: 80, // Above performance overlay toggle if present
          child: FloatingActionButton.small(
            heroTag: 'widget_inspector_toggle',
            backgroundColor: _isEnabled ? Colors.blue : Colors.grey[700],
            onPressed: () {
              widgetInspectorManager.toggle();
              if (widgetInspectorManager.isEnabled) {
                widgetInspectorManager.showInspectorOverlay();
              }
            },
            child: const Icon(Icons.search, color: Colors.white, size: 20),
          ),
        ),
      ],
    );
  }
}
