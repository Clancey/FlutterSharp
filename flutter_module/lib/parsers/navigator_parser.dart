// Manual parser for Navigator widget
// Part of FlutterSharp Phase 5 - Navigation

import 'dart:ffi';
import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Enum for route transition types matching C# RouteTransitionType
enum RouteTransitionType {
  none,
  material,
  cupertino,
  fade,
  slideBottom,
  slideRight,
  zoom,
}

/// Parser for Navigator widget.
///
/// Parses the Navigator FFI struct and builds a Flutter Navigator widget
/// that displays the current route's child widget with transition animations.
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

    // Parse clip behavior
    final clipBehavior = Clip.values[map.clipBehavior.clamp(0, Clip.values.length - 1)];

    // Parse callback action IDs
    final onRouteChangedAction = map.hasOnRouteChangedAction == 1
        ? parseString(map.onRouteChangedAction)
        : null;

    final onPopAction = map.hasOnPopAction == 1
        ? parseString(map.onPopAction)
        : null;

    // Parse dynamic route callback flags
    final hasOnGenerateRoute = map.hasOnGenerateRoute == 1;
    final hasOnUnknownRoute = map.hasOnUnknownRoute == 1;

    // Parse transition parameters
    final transitionType = RouteTransitionType.values[
        map.transitionType.clamp(0, RouteTransitionType.values.length - 1)];
    final transitionDurationMs = map.transitionDurationMs > 0 ? map.transitionDurationMs : 300;
    final reverseTransitionDurationMs = map.reverseTransitionDurationMs > 0 ? map.reverseTransitionDurationMs : 300;
    final fullscreenDialog = map.fullscreenDialog == 1;
    final isTransitioning = map.isTransitioning == 1;
    final isPopping = map.isPopping == 1;

    // Build the current route's child widget
    Widget? currentChild;
    if (map.currentChild.address != 0) {
      currentChild = DynamicWidgetBuilder.buildFromPointer(map.currentChild, buildContext);
    }

    // Build the previous route's child widget (for transitions)
    Widget? previousChild;
    if (map.previousChild.address != 0 && isTransitioning) {
      previousChild = DynamicWidgetBuilder.buildFromPointer(map.previousChild, buildContext);
    }

    // If we have a child, wrap it in Navigator structure with transitions
    if (currentChild != null) {
      return _NavigatorWrapper(
        key: ValueKey(navigatorId ?? id),
        currentRoute: currentRoute,
        currentChild: currentChild,
        previousChild: previousChild,
        onRouteChangedAction: onRouteChangedAction,
        onPopAction: onPopAction,
        clipBehavior: clipBehavior,
        transitionType: transitionType,
        transitionDurationMs: transitionDurationMs,
        reverseTransitionDurationMs: reverseTransitionDurationMs,
        fullscreenDialog: fullscreenDialog,
        isTransitioning: isTransitioning,
        isPopping: isPopping,
        hasOnGenerateRoute: hasOnGenerateRoute,
        hasOnUnknownRoute: hasOnUnknownRoute,
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

/// Internal wrapper widget that provides Navigator functionality with transition animations.
class _NavigatorWrapper extends StatefulWidget {
  final String currentRoute;
  final Widget currentChild;
  final Widget? previousChild;
  final String? onRouteChangedAction;
  final String? onPopAction;
  final Clip clipBehavior;
  final RouteTransitionType transitionType;
  final int transitionDurationMs;
  final int reverseTransitionDurationMs;
  final bool fullscreenDialog;
  final bool isTransitioning;
  final bool isPopping;
  /// Whether OnGenerateRoute callback is set on the C# side for dynamic route resolution.
  final bool hasOnGenerateRoute;
  /// Whether OnUnknownRoute callback is set on the C# side for fallback route handling.
  final bool hasOnUnknownRoute;

  const _NavigatorWrapper({
    super.key,
    required this.currentRoute,
    required this.currentChild,
    this.previousChild,
    this.onRouteChangedAction,
    this.onPopAction,
    this.clipBehavior = Clip.hardEdge,
    this.transitionType = RouteTransitionType.none,
    this.transitionDurationMs = 300,
    this.reverseTransitionDurationMs = 300,
    this.fullscreenDialog = false,
    this.isTransitioning = false,
    this.isPopping = false,
    this.hasOnGenerateRoute = false,
    this.hasOnUnknownRoute = false,
  });

  @override
  State<_NavigatorWrapper> createState() => _NavigatorWrapperState();
}

class _NavigatorWrapperState extends State<_NavigatorWrapper>
    with TickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<Offset> _slideAnimation;
  late Animation<double> _fadeAnimation;
  late Animation<double> _scaleAnimation;
  bool _animationComplete = true;

  @override
  void initState() {
    super.initState();
    _initAnimationController();
  }

  void _initAnimationController() {
    final duration = widget.isPopping
        ? Duration(milliseconds: widget.reverseTransitionDurationMs)
        : Duration(milliseconds: widget.transitionDurationMs);

    _controller = AnimationController(
      vsync: this,
      duration: duration,
    );

    _setupAnimations();

    // Start animation if transitioning
    if (widget.isTransitioning && widget.transitionType != RouteTransitionType.none) {
      _animationComplete = false;
      if (widget.isPopping) {
        _controller.reverse(from: 1.0).then((_) {
          setState(() => _animationComplete = true);
        });
      } else {
        _controller.forward(from: 0.0).then((_) {
          setState(() => _animationComplete = true);
        });
      }
    }
  }

  void _setupAnimations() {
    final curve = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeInOut,
      reverseCurve: Curves.easeInOut,
    );

    switch (widget.transitionType) {
      case RouteTransitionType.material:
        // Material: Slide up + fade
        _slideAnimation = Tween<Offset>(
          begin: const Offset(0.0, 0.25),
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.cupertino:
        // Cupertino: Slide from right
        _slideAnimation = Tween<Offset>(
          begin: const Offset(1.0, 0.0),
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.fade:
        // Fade only
        _slideAnimation = Tween<Offset>(
          begin: Offset.zero,
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.slideBottom:
        // Slide from bottom
        _slideAnimation = Tween<Offset>(
          begin: const Offset(0.0, 1.0),
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.slideRight:
        // Slide from right (similar to Cupertino but without parallax)
        _slideAnimation = Tween<Offset>(
          begin: const Offset(1.0, 0.0),
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.zoom:
        // Zoom from center
        _slideAnimation = Tween<Offset>(
          begin: Offset.zero,
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 0.8, end: 1.0).animate(curve);
        break;

      case RouteTransitionType.none:
      default:
        // No animation
        _slideAnimation = Tween<Offset>(
          begin: Offset.zero,
          end: Offset.zero,
        ).animate(curve);
        _fadeAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        _scaleAnimation = Tween<double>(begin: 1.0, end: 1.0).animate(curve);
        break;
    }
  }

  @override
  void didUpdateWidget(_NavigatorWrapper oldWidget) {
    super.didUpdateWidget(oldWidget);

    // If a new transition started, reinitialize animation
    if (widget.isTransitioning &&
        (widget.currentRoute != oldWidget.currentRoute ||
         widget.isPopping != oldWidget.isPopping)) {
      _controller.dispose();
      _initAnimationController();
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // Use PopScope to intercept back button
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
        child: _buildTransitionContent(),
      ),
    );
  }

  Widget _buildTransitionContent() {
    // If no transition or animation complete, just show current child
    if (widget.transitionType == RouteTransitionType.none ||
        (!widget.isTransitioning && _animationComplete)) {
      return widget.currentChild;
    }

    // Build animated transition
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return Stack(
          children: [
            // Previous child (slides out/fades out during pop)
            if (widget.previousChild != null && widget.isPopping)
              _buildAnimatedChild(widget.previousChild!, isIncoming: false),

            // Previous child (stays visible behind during push)
            if (widget.previousChild != null && !widget.isPopping)
              widget.previousChild!,

            // Current child (slides in/fades in during push)
            _buildAnimatedChild(widget.currentChild, isIncoming: !widget.isPopping),
          ],
        );
      },
    );
  }

  Widget _buildAnimatedChild(Widget child, {required bool isIncoming}) {
    // For incoming widget: animate from start to end
    // For outgoing widget: animate from end to start (reverse)
    Widget result = child;

    // Apply scale animation
    if (widget.transitionType == RouteTransitionType.zoom) {
      result = ScaleTransition(
        scale: _scaleAnimation,
        child: result,
      );
    }

    // Apply slide animation
    if (widget.transitionType == RouteTransitionType.material ||
        widget.transitionType == RouteTransitionType.cupertino ||
        widget.transitionType == RouteTransitionType.slideBottom ||
        widget.transitionType == RouteTransitionType.slideRight) {
      result = SlideTransition(
        position: _slideAnimation,
        child: result,
      );
    }

    // Apply fade animation
    if (widget.transitionType == RouteTransitionType.material ||
        widget.transitionType == RouteTransitionType.fade ||
        widget.transitionType == RouteTransitionType.zoom) {
      result = FadeTransition(
        opacity: _fadeAnimation,
        child: result,
      );
    }

    return result;
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
