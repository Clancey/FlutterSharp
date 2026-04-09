/// Rendering metrics tracking for FlutterSharp.
/// Collects frame timing data and reports to C# for performance analysis.

import 'dart:async';
import 'dart:collection';
import 'dart:convert';
import 'package:flutter/scheduler.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';

/// Method channel for communicating with C#
const MethodChannel _methodChannel =
    MethodChannel('com.Microsoft.FlutterSharp/Messages');

/// Singleton manager for rendering metrics collection.
class RenderingMetricsManager {
  static final RenderingMetricsManager _instance = RenderingMetricsManager._();
  static RenderingMetricsManager get instance => _instance;

  RenderingMetricsManager._();

  /// Whether metrics collection is enabled.
  bool _enabled = false;
  bool get enabled => _enabled;

  /// Target FPS for jank calculations.
  double _targetFps = 60.0;
  double get targetFps => _targetFps;
  set targetFps(double value) {
    _targetFps = value > 0 ? value : 60.0;
  }

  /// Threshold multiplier for jank detection.
  double _jankThresholdMultiplier = 2.0;

  /// Maximum recent frames to keep.
  static const int _maxRecentFrames = 120;

  /// Recent frame timings.
  final Queue<FrameTimingInfo> _recentFrames = Queue();

  /// Frame timing callback handle.
  void Function(List<FrameTiming>)? _frameTimingsCallback;

  /// Statistics
  int _totalFrameCount = 0;
  int _totalJankFrames = 0;
  Duration _totalBuildTime = Duration.zero;
  Duration _totalRasterTime = Duration.zero;
  Duration _worstBuildTime = Duration.zero;
  Duration _worstRasterTime = Duration.zero;

  /// Interval for sending metrics to C#.
  Timer? _reportTimer;
  static const Duration _reportInterval = Duration(seconds: 1);

  /// Enables rendering metrics collection.
  void enable({double? targetFps, double? jankThreshold}) {
    if (_enabled) return;

    if (targetFps != null) _targetFps = targetFps;
    if (jankThreshold != null) _jankThresholdMultiplier = jankThreshold;

    _enabled = true;
    _reset();

    // Register frame timing callback
    _frameTimingsCallback = _onFrameTimings;
    SchedulerBinding.instance.addTimingsCallback(_frameTimingsCallback!);

    // Start periodic reporting to C#
    _reportTimer = Timer.periodic(_reportInterval, (_) => _reportToCS());

    debugPrint('[RenderingMetrics] Enabled with target FPS: $_targetFps');
  }

  /// Disables rendering metrics collection.
  void disable() {
    if (!_enabled) return;

    _enabled = false;

    // Remove frame timing callback
    if (_frameTimingsCallback != null) {
      SchedulerBinding.instance.removeTimingsCallback(_frameTimingsCallback!);
      _frameTimingsCallback = null;
    }

    // Stop reporting
    _reportTimer?.cancel();
    _reportTimer = null;

    debugPrint('[RenderingMetrics] Disabled');
  }

  /// Resets all metrics.
  void _reset() {
    _recentFrames.clear();
    _totalFrameCount = 0;
    _totalJankFrames = 0;
    _totalBuildTime = Duration.zero;
    _totalRasterTime = Duration.zero;
    _worstBuildTime = Duration.zero;
    _worstRasterTime = Duration.zero;
  }

  /// Callback for frame timings from Flutter scheduler.
  void _onFrameTimings(List<FrameTiming> timings) {
    if (!_enabled) return;

    final jankThreshold = Duration(
      microseconds: (1000000 / _targetFps * _jankThresholdMultiplier).round(),
    );

    for (final timing in timings) {
      final buildDuration = timing.buildDuration;
      final rasterDuration = timing.rasterDuration;
      final totalFrameTime = timing.totalSpan;
      final isJank = totalFrameTime > jankThreshold;

      _totalFrameCount++;
      _totalBuildTime += buildDuration;
      _totalRasterTime += rasterDuration;

      if (buildDuration > _worstBuildTime) {
        _worstBuildTime = buildDuration;
      }
      if (rasterDuration > _worstRasterTime) {
        _worstRasterTime = rasterDuration;
      }
      if (isJank) {
        _totalJankFrames++;
      }

      final frameInfo = FrameTimingInfo(
        timestamp: DateTime.now(),
        frameNumber: _totalFrameCount,
        buildTime: buildDuration,
        rasterTime: rasterDuration,
        totalTime: totalFrameTime,
        isJank: isJank,
      );

      _recentFrames.addLast(frameInfo);
      while (_recentFrames.length > _maxRecentFrames) {
        _recentFrames.removeFirst();
      }

      if (isJank) {
        debugPrint('[RenderingMetrics] Jank detected: Frame #$_totalFrameCount '
            'took ${totalFrameTime.inMicroseconds / 1000}ms '
            '(build: ${buildDuration.inMicroseconds / 1000}ms, '
            'raster: ${rasterDuration.inMicroseconds / 1000}ms)');
      }
    }
  }

  /// Reports metrics to C# side.
  Future<void> _reportToCS() async {
    if (!_enabled || _recentFrames.isEmpty) return;

    try {
      final stats = getStats();
      final message = {
        'messageType': 'RenderingMetrics',
        'totalFrameCount': stats.totalFrameCount,
        'totalJankFrames': stats.totalJankFrames,
        'currentFps': stats.currentFps,
        'averageFps': stats.averageFps,
        'averageBuildTimeMs': stats.averageBuildTime.inMicroseconds / 1000,
        'averageRasterTimeMs': stats.averageRasterTime.inMicroseconds / 1000,
        'worstBuildTimeMs': _worstBuildTime.inMicroseconds / 1000,
        'worstRasterTimeMs': _worstRasterTime.inMicroseconds / 1000,
        'p50FrameTimeMs': stats.p50FrameTime.inMicroseconds / 1000,
        'p95FrameTimeMs': stats.p95FrameTime.inMicroseconds / 1000,
        'p99FrameTimeMs': stats.p99FrameTime.inMicroseconds / 1000,
        'jankPercentage': stats.jankPercentage,
      };

      await _methodChannel.invokeMethod(
          'RenderingMetrics', json.encode(message));
    } catch (e) {
      // Silently fail - don't disrupt rendering for metrics
      debugPrint('[RenderingMetrics] Failed to report to C#: $e');
    }
  }

  /// Gets the current FPS based on recent frames.
  double getCurrentFps() {
    if (_recentFrames.length < 2) return 0;

    final frames = _recentFrames.toList();
    final firstTime = frames.first.timestamp;
    final lastTime = frames.last.timestamp;
    final durationSeconds =
        lastTime.difference(firstTime).inMicroseconds / 1000000;

    if (durationSeconds <= 0) return 0;
    return (frames.length - 1) / durationSeconds;
  }

  /// Gets detailed statistics.
  RenderingStats getStats() {
    final stats = RenderingStats();
    stats.totalFrameCount = _totalFrameCount;
    stats.totalJankFrames = _totalJankFrames;
    stats.jankPercentage =
        _totalFrameCount > 0 ? (_totalJankFrames / _totalFrameCount) * 100 : 0;
    stats.currentFps = getCurrentFps();
    stats.targetFps = _targetFps;
    stats.averageFps = _totalFrameCount > 0
        ? _totalFrameCount /
            ((_totalBuildTime + _totalRasterTime).inMicroseconds / 1000000)
        : 0;
    stats.averageBuildTime = _totalFrameCount > 0
        ? Duration(
            microseconds: _totalBuildTime.inMicroseconds ~/ _totalFrameCount)
        : Duration.zero;
    stats.averageRasterTime = _totalFrameCount > 0
        ? Duration(
            microseconds: _totalRasterTime.inMicroseconds ~/ _totalFrameCount)
        : Duration.zero;

    // Calculate percentiles from recent frames
    if (_recentFrames.isNotEmpty) {
      final frameTimes =
          _recentFrames.map((f) => f.totalTime.inMicroseconds).toList()..sort();

      stats.p50FrameTime = Duration(
        microseconds: frameTimes[(frameTimes.length * 0.50).floor()],
      );
      stats.p95FrameTime = Duration(
        microseconds: frameTimes[(frameTimes.length * 0.95).floor()],
      );
      stats.p99FrameTime = Duration(
        microseconds: frameTimes[
            (frameTimes.length * 0.99).floor().clamp(0, frameTimes.length - 1)],
      );
    }

    return stats;
  }

  /// Gets recent frame data.
  List<FrameTimingInfo> getRecentFrames() => _recentFrames.toList();

  /// Generates a human-readable report.
  String generateReport() {
    final stats = getStats();
    final buffer = StringBuffer();

    buffer.writeln('=== Flutter Rendering Metrics Report ===');
    buffer.writeln();
    buffer.writeln('FPS Statistics:');
    buffer.writeln('  Current FPS: ${stats.currentFps.toStringAsFixed(1)}');
    buffer.writeln('  Average FPS: ${stats.averageFps.toStringAsFixed(1)}');
    buffer.writeln('  Target FPS: ${_targetFps.toStringAsFixed(0)}');
    buffer.writeln();
    buffer.writeln('Frame Statistics:');
    buffer.writeln('  Total Frames: ${stats.totalFrameCount}');
    buffer.writeln(
        '  Average Build Time: ${stats.averageBuildTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  Average Raster Time: ${stats.averageRasterTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  P50 Frame Time: ${stats.p50FrameTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  P95 Frame Time: ${stats.p95FrameTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  P99 Frame Time: ${stats.p99FrameTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  Worst Build Time: ${_worstBuildTime.inMicroseconds / 1000}ms');
    buffer.writeln(
        '  Worst Raster Time: ${_worstRasterTime.inMicroseconds / 1000}ms');
    buffer.writeln();
    buffer.writeln('Jank Statistics:');
    buffer.writeln('  Jank Frames: ${stats.totalJankFrames}');
    buffer.writeln(
        '  Jank Percentage: ${stats.jankPercentage.toStringAsFixed(2)}%');

    return buffer.toString();
  }
}

/// Information about a single frame.
class FrameTimingInfo {
  final DateTime timestamp;
  final int frameNumber;
  final Duration buildTime;
  final Duration rasterTime;
  final Duration totalTime;
  final bool isJank;

  FrameTimingInfo({
    required this.timestamp,
    required this.frameNumber,
    required this.buildTime,
    required this.rasterTime,
    required this.totalTime,
    required this.isJank,
  });
}

/// Rendering statistics.
class RenderingStats {
  int totalFrameCount = 0;
  int totalJankFrames = 0;
  double jankPercentage = 0;
  double currentFps = 0;
  double averageFps = 0;
  double targetFps = 60.0;
  Duration averageBuildTime = Duration.zero;
  Duration averageRasterTime = Duration.zero;
  Duration p50FrameTime = Duration.zero;
  Duration p95FrameTime = Duration.zero;
  Duration p99FrameTime = Duration.zero;

  // Convenience getters for the overlay
  int get totalFrames => totalFrameCount;
  int get jankFrames => totalJankFrames;
  double get averageFrameTimeMs =>
      (averageBuildTime.inMicroseconds + averageRasterTime.inMicroseconds) /
      1000.0;
  double get p50FrameTimeMs => p50FrameTime.inMicroseconds / 1000.0;
  double get p95FrameTimeMs => p95FrameTime.inMicroseconds / 1000.0;
  double get p99FrameTimeMs => p99FrameTime.inMicroseconds / 1000.0;
}

/// Global accessor for rendering metrics.
RenderingMetricsManager get renderingMetrics =>
    RenderingMetricsManager.instance;
