// FlutterSharp Performance Overlay
// A visual debugging overlay displaying real-time rendering metrics

import 'dart:async';
import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'rendering_metrics.dart';

/// Manager for controlling the FlutterSharp performance overlay visibility.
class PerformanceOverlayManager {
  static final PerformanceOverlayManager _instance =
      PerformanceOverlayManager._internal();
  factory PerformanceOverlayManager() => _instance;
  PerformanceOverlayManager._internal();

  static PerformanceOverlayManager get instance => _instance;

  bool _isVisible = false;
  final _visibilityController = StreamController<bool>.broadcast();

  /// Whether the performance overlay is currently visible.
  bool get isVisible => _isVisible;

  /// Stream of visibility changes.
  Stream<bool> get visibilityStream => _visibilityController.stream;

  /// Shows the performance overlay.
  void show() {
    if (!_isVisible) {
      _isVisible = true;
      // Also enable metrics collection when showing overlay
      renderingMetrics.enable();
      _visibilityController.add(true);
    }
  }

  /// Hides the performance overlay.
  void hide() {
    if (_isVisible) {
      _isVisible = false;
      _visibilityController.add(false);
    }
  }

  /// Toggles the performance overlay visibility.
  void toggle() {
    if (_isVisible) {
      hide();
    } else {
      show();
    }
  }

  void dispose() {
    _visibilityController.close();
  }
}

/// Convenience getter for the performance overlay manager.
PerformanceOverlayManager get performanceOverlayManager =>
    PerformanceOverlayManager.instance;

/// A widget that displays real-time rendering performance metrics.
///
/// This overlay shows:
/// - Current FPS with color coding (green/yellow/red)
/// - Frame time percentiles (P50, P95, P99)
/// - Jank percentage and count
/// - Frame time graph visualization
///
/// Usage:
/// ```dart
/// FlutterSharpPerformanceOverlay(
///   child: YourApp(),
/// )
/// ```
class FlutterSharpPerformanceOverlay extends StatefulWidget {
  final Widget child;
  final OverlayPosition position;
  final bool showGraph;
  final bool compact;
  final double opacity;

  const FlutterSharpPerformanceOverlay({
    super.key,
    required this.child,
    this.position = OverlayPosition.topRight,
    this.showGraph = true,
    this.compact = false,
    this.opacity = 0.85,
  });

  @override
  State<FlutterSharpPerformanceOverlay> createState() =>
      _FlutterSharpPerformanceOverlayState();
}

enum OverlayPosition {
  topLeft,
  topRight,
  bottomLeft,
  bottomRight,
}

class _FlutterSharpPerformanceOverlayState
    extends State<FlutterSharpPerformanceOverlay> {
  Timer? _updateTimer;
  RenderingStats? _stats;
  List<FrameTimingInfo> _recentFrames = [];
  bool _isVisible = false;
  StreamSubscription<bool>? _visibilitySubscription;

  @override
  void initState() {
    super.initState();
    _isVisible = performanceOverlayManager.isVisible;
    _visibilitySubscription =
        performanceOverlayManager.visibilityStream.listen((visible) {
      if (mounted) {
        setState(() {
          _isVisible = visible;
          if (visible) {
            _startUpdates();
          } else {
            _stopUpdates();
          }
        });
      }
    });

    if (_isVisible) {
      _startUpdates();
    }
  }

  void _startUpdates() {
    _updateTimer?.cancel();
    _updateTimer = Timer.periodic(const Duration(milliseconds: 100), (_) {
      if (mounted && _isVisible) {
        setState(() {
          _stats = renderingMetrics.getStats();
          _recentFrames = renderingMetrics.getRecentFrames();
        });
      }
    });
  }

  void _stopUpdates() {
    _updateTimer?.cancel();
    _updateTimer = null;
  }

  @override
  void dispose() {
    _updateTimer?.cancel();
    _visibilitySubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        widget.child,
        if (_isVisible)
          Positioned(
            top: widget.position == OverlayPosition.topLeft ||
                    widget.position == OverlayPosition.topRight
                ? 8
                : null,
            bottom: widget.position == OverlayPosition.bottomLeft ||
                    widget.position == OverlayPosition.bottomRight
                ? 8
                : null,
            left: widget.position == OverlayPosition.topLeft ||
                    widget.position == OverlayPosition.bottomLeft
                ? 8
                : null,
            right: widget.position == OverlayPosition.topRight ||
                    widget.position == OverlayPosition.bottomRight
                ? 8
                : null,
            child: _PerformanceOverlayPanel(
              stats: _stats,
              recentFrames: _recentFrames,
              showGraph: widget.showGraph,
              compact: widget.compact,
              opacity: widget.opacity,
              onClose: () => performanceOverlayManager.hide(),
            ),
          ),
      ],
    );
  }
}

class _PerformanceOverlayPanel extends StatelessWidget {
  final RenderingStats? stats;
  final List<FrameTimingInfo> recentFrames;
  final bool showGraph;
  final bool compact;
  final double opacity;
  final VoidCallback onClose;

  const _PerformanceOverlayPanel({
    required this.stats,
    required this.recentFrames,
    required this.showGraph,
    required this.compact,
    required this.opacity,
    required this.onClose,
  });

  @override
  Widget build(BuildContext context) {
    return Material(
      type: MaterialType.transparency,
      child: Container(
        constraints: BoxConstraints(
          maxWidth: compact ? 160 : 220,
        ),
        decoration: BoxDecoration(
          color: Colors.black.withValues(alpha: opacity),
          borderRadius: BorderRadius.circular(8),
          border: Border.all(
            color: _getFpsColor(stats?.currentFps ?? 0).withValues(alpha: 0.5),
            width: 1,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.3),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            _buildHeader(),
            const Divider(height: 1, color: Colors.white24),
            Padding(
              padding: const EdgeInsets.all(8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildFpsSection(),
                  if (!compact) ...[
                    const SizedBox(height: 8),
                    _buildFrameTimeSection(),
                    const SizedBox(height: 8),
                    _buildJankSection(),
                  ],
                  if (showGraph && recentFrames.isNotEmpty) ...[
                    const SizedBox(height: 8),
                    _buildGraph(),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          const Row(
            children: [
              Icon(Icons.speed, size: 14, color: Colors.white70),
              SizedBox(width: 4),
              Text(
                'Performance',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 11,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          GestureDetector(
            onTap: onClose,
            child: const Icon(Icons.close, size: 14, color: Colors.white54),
          ),
        ],
      ),
    );
  }

  Widget _buildFpsSection() {
    final fps = stats?.currentFps ?? 0;
    final color = _getFpsColor(fps);

    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Row(
          children: [
            Container(
              width: 8,
              height: 8,
              decoration: BoxDecoration(
                color: color,
                shape: BoxShape.circle,
              ),
            ),
            const SizedBox(width: 6),
            const Text(
              'FPS',
              style: TextStyle(color: Colors.white70, fontSize: 11),
            ),
          ],
        ),
        Text(
          fps.toStringAsFixed(1),
          style: TextStyle(
            color: color,
            fontSize: compact ? 16 : 18,
            fontWeight: FontWeight.bold,
            fontFamily: 'monospace',
          ),
        ),
      ],
    );
  }

  Widget _buildFrameTimeSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Frame Time',
          style: TextStyle(color: Colors.white54, fontSize: 10),
        ),
        const SizedBox(height: 4),
        _buildMetricRow('Avg', stats?.averageFrameTimeMs ?? 0, 'ms'),
        _buildMetricRow('P95', stats?.p95FrameTimeMs ?? 0, 'ms'),
        _buildMetricRow('P99', stats?.p99FrameTimeMs ?? 0, 'ms'),
      ],
    );
  }

  Widget _buildJankSection() {
    final jankPct = stats?.jankPercentage ?? 0;
    final jankColor =
        jankPct > 5 ? Colors.red : (jankPct > 2 ? Colors.orange : Colors.green);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Jank',
          style: TextStyle(color: Colors.white54, fontSize: 10),
        ),
        const SizedBox(height: 4),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            const Text(
              'Percentage',
              style: TextStyle(color: Colors.white70, fontSize: 10),
            ),
            Text(
              '${jankPct.toStringAsFixed(2)}%',
              style: TextStyle(
                color: jankColor,
                fontSize: 11,
                fontWeight: FontWeight.bold,
                fontFamily: 'monospace',
              ),
            ),
          ],
        ),
        const SizedBox(height: 2),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            const Text(
              'Frames',
              style: TextStyle(color: Colors.white70, fontSize: 10),
            ),
            Text(
              '${stats?.jankFrames ?? 0} / ${stats?.totalFrames ?? 0}',
              style: const TextStyle(
                color: Colors.white,
                fontSize: 11,
                fontFamily: 'monospace',
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildMetricRow(String label, double value, String unit) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 1),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: const TextStyle(color: Colors.white70, fontSize: 10),
          ),
          Text(
            '${value.toStringAsFixed(2)} $unit',
            style: const TextStyle(
              color: Colors.white,
              fontSize: 11,
              fontFamily: 'monospace',
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildGraph() {
    return Container(
      height: 50,
      decoration: BoxDecoration(
        color: Colors.black26,
        borderRadius: BorderRadius.circular(4),
      ),
      child: CustomPaint(
        painter: _FrameTimeGraphPainter(
          frames: recentFrames,
          targetFps: stats?.targetFps ?? 60,
        ),
        size: const Size(double.infinity, 50),
      ),
    );
  }

  Color _getFpsColor(double fps) {
    final target = stats?.targetFps ?? 60;
    if (fps >= target * 0.95) return Colors.green;
    if (fps >= target * 0.7) return Colors.orange;
    return Colors.red;
  }
}

class _FrameTimeGraphPainter extends CustomPainter {
  final List<FrameTimingInfo> frames;
  final double targetFps;

  _FrameTimeGraphPainter({
    required this.frames,
    required this.targetFps,
  });

  @override
  void paint(Canvas canvas, Size size) {
    if (frames.isEmpty) return;

    final targetMs = 1000.0 / targetFps;
    final maxMs = targetMs * 3; // Show up to 3x target as max

    // Draw target line
    final targetPaint = Paint()
      ..color = Colors.green.withValues(alpha: 0.3)
      ..strokeWidth = 1;
    final targetY = size.height - (targetMs / maxMs * size.height);
    canvas.drawLine(
      Offset(0, targetY),
      Offset(size.width, targetY),
      targetPaint,
    );

    // Draw jank threshold line
    final jankPaint = Paint()
      ..color = Colors.red.withValues(alpha: 0.3)
      ..strokeWidth = 1;
    final jankMs = targetMs * 2;
    final jankY = size.height - (jankMs / maxMs * size.height);
    canvas.drawLine(
      Offset(0, jankY),
      Offset(size.width, jankY),
      jankPaint,
    );

    // Draw frame bars
    final barWidth = size.width / frames.length;
    for (int i = 0; i < frames.length; i++) {
      final frame = frames[i];
      final frameMs = frame.totalTime.inMicroseconds / 1000.0;
      final normalizedHeight = math.min(frameMs / maxMs, 1.0);
      final barHeight = normalizedHeight * size.height;

      final isJank = frame.isJank;
      final barColor = isJank ? Colors.red : Colors.green;

      final barPaint = Paint()
        ..color = barColor.withValues(alpha: 0.8)
        ..style = PaintingStyle.fill;

      canvas.drawRect(
        Rect.fromLTWH(
          i * barWidth,
          size.height - barHeight,
          barWidth - 1,
          barHeight,
        ),
        barPaint,
      );
    }
  }

  @override
  bool shouldRepaint(covariant _FrameTimeGraphPainter oldDelegate) {
    return frames != oldDelegate.frames;
  }
}

/// A simple floating action button to toggle the performance overlay.
///
/// Add this to your app to easily toggle the overlay visibility:
/// ```dart
/// Scaffold(
///   body: FlutterSharpPerformanceOverlay(child: YourContent()),
///   floatingActionButton: PerformanceOverlayToggle(),
/// )
/// ```
class PerformanceOverlayToggle extends StatefulWidget {
  final Widget? icon;
  final Color? backgroundColor;
  final Color? foregroundColor;

  const PerformanceOverlayToggle({
    super.key,
    this.icon,
    this.backgroundColor,
    this.foregroundColor,
  });

  @override
  State<PerformanceOverlayToggle> createState() =>
      _PerformanceOverlayToggleState();
}

class _PerformanceOverlayToggleState extends State<PerformanceOverlayToggle> {
  bool _isVisible = false;
  StreamSubscription<bool>? _subscription;

  @override
  void initState() {
    super.initState();
    _isVisible = performanceOverlayManager.isVisible;
    _subscription =
        performanceOverlayManager.visibilityStream.listen((visible) {
      if (mounted) {
        setState(() {
          _isVisible = visible;
        });
      }
    });
  }

  @override
  void dispose() {
    _subscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton.small(
      onPressed: () => performanceOverlayManager.toggle(),
      backgroundColor:
          widget.backgroundColor ?? (_isVisible ? Colors.green : Colors.grey),
      foregroundColor: widget.foregroundColor ?? Colors.white,
      child:
          widget.icon ?? Icon(_isVisible ? Icons.speed : Icons.speed_outlined),
    );
  }
}
