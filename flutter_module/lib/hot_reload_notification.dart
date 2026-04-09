import 'dart:async';
import 'package:flutter/material.dart';

/// Represents a hot reload notification from C#.
class HotReloadInfo {
  final DateTime timestamp;
  final String? widgetType;
  final int widgetsReloaded;
  final bool success;
  final String? errorMessage;
  final Duration? duration;

  HotReloadInfo({
    DateTime? timestamp,
    this.widgetType,
    this.widgetsReloaded = 1,
    this.success = true,
    this.errorMessage,
    this.duration,
  }) : timestamp = timestamp ?? DateTime.now();

  factory HotReloadInfo.fromJson(Map<String, dynamic> json) {
    DateTime? ts;
    if (json['timestamp'] != null) {
      try {
        ts = DateTime.parse(json['timestamp'] as String);
      } catch (_) {
        ts = DateTime.now();
      }
    }

    Duration? dur;
    if (json['durationMs'] != null) {
      dur = Duration(milliseconds: json['durationMs'] as int);
    }

    return HotReloadInfo(
      timestamp: ts,
      widgetType: json['widgetType'] as String?,
      widgetsReloaded: json['widgetsReloaded'] as int? ?? 1,
      success: json['success'] as bool? ?? true,
      errorMessage: json['errorMessage'] as String?,
      duration: dur,
    );
  }
}

/// Singleton manager for distributing hot reload notifications.
class HotReloadNotificationManager {
  static final HotReloadNotificationManager _instance =
      HotReloadNotificationManager._();
  static HotReloadNotificationManager get instance => _instance;

  HotReloadNotificationManager._();

  final List<void Function(HotReloadInfo)> _listeners = [];
  final List<HotReloadInfo> _reloadHistory = [];
  static const int _maxHistorySize = 20;

  /// Adds a listener for hot reload events.
  void addListener(void Function(HotReloadInfo) listener) {
    _listeners.add(listener);
  }

  /// Removes a hot reload listener.
  void removeListener(void Function(HotReloadInfo) listener) {
    _listeners.remove(listener);
  }

  /// Shows a hot reload notification to all registered listeners.
  void showNotification(HotReloadInfo info) {
    // Add to history
    _reloadHistory.add(info);
    if (_reloadHistory.length > _maxHistorySize) {
      _reloadHistory.removeAt(0);
    }

    // Notify listeners
    for (final listener in _listeners) {
      listener(info);
    }
  }

  /// Gets recent hot reload history.
  List<HotReloadInfo> get reloadHistory => List.unmodifiable(_reloadHistory);

  /// Clears reload history.
  void clearHistory() {
    _reloadHistory.clear();
  }

  /// Gets the count of successful reloads in the current session.
  int get successfulReloadCount =>
      _reloadHistory.where((r) => r.success).length;
}

/// A widget that displays hot reload notifications as a toast-like banner.
/// Wrap your root widget with this to show hot reload feedback from C#.
class HotReloadNotificationOverlay extends StatefulWidget {
  final Widget child;

  /// How long to show the notification. Default 2 seconds.
  final Duration displayDuration;

  /// Whether to show successful reloads. Set to false to only show errors.
  final bool showSuccessNotifications;

  /// Position of the notification.
  final HotReloadNotificationPosition position;

  const HotReloadNotificationOverlay({
    Key? key,
    required this.child,
    this.displayDuration = const Duration(seconds: 2),
    this.showSuccessNotifications = true,
    this.position = HotReloadNotificationPosition.bottom,
  }) : super(key: key);

  @override
  State<HotReloadNotificationOverlay> createState() =>
      _HotReloadNotificationOverlayState();
}

/// Position where the hot reload notification appears.
enum HotReloadNotificationPosition {
  top,
  bottom,
}

class _HotReloadNotificationOverlayState
    extends State<HotReloadNotificationOverlay>
    with SingleTickerProviderStateMixin {
  HotReloadInfo? _currentNotification;
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  Timer? _autoDismissTimer;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 250),
      vsync: this,
    );
    _fadeAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOut,
    ));

    final isTop = widget.position == HotReloadNotificationPosition.top;
    _slideAnimation = Tween<Offset>(
      begin: Offset(0, isTop ? -0.5 : 0.5),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutCubic,
    ));

    HotReloadNotificationManager.instance.addListener(_onNotificationReceived);
  }

  void _onNotificationReceived(HotReloadInfo info) {
    // Skip success notifications if configured
    if (info.success && !widget.showSuccessNotifications) {
      return;
    }

    // Cancel any existing timer
    _autoDismissTimer?.cancel();

    setState(() {
      _currentNotification = info;
    });

    _animationController.forward();

    // Auto-dismiss after duration
    _autoDismissTimer = Timer(widget.displayDuration, _dismissNotification);
  }

  void _dismissNotification() {
    _autoDismissTimer?.cancel();
    _animationController.reverse().then((_) {
      if (mounted) {
        setState(() {
          _currentNotification = null;
        });
      }
    });
  }

  @override
  void dispose() {
    _autoDismissTimer?.cancel();
    _animationController.dispose();
    HotReloadNotificationManager.instance
        .removeListener(_onNotificationReceived);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        widget.child,
        if (_currentNotification != null)
          Positioned(
            top:
                widget.position == HotReloadNotificationPosition.top ? 0 : null,
            bottom: widget.position == HotReloadNotificationPosition.bottom
                ? 0
                : null,
            left: 0,
            right: 0,
            child: SafeArea(
              child: SlideTransition(
                position: _slideAnimation,
                child: FadeTransition(
                  opacity: _fadeAnimation,
                  child: _buildNotificationBanner(_currentNotification!),
                ),
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildNotificationBanner(HotReloadInfo info) {
    final isSuccess = info.success;
    final backgroundColor = isSuccess
        ? const Color(0xFF2E7D32) // Green 800
        : Colors.orange.shade800;
    final icon = isSuccess ? Icons.refresh : Icons.warning_amber_rounded;

    return GestureDetector(
      onTap: _dismissNotification,
      child: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        child: Material(
          elevation: 6,
          borderRadius: BorderRadius.circular(12),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: backgroundColor,
              borderRadius: BorderRadius.circular(12),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.2),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                // Animated icon
                _HotReloadIcon(
                  icon: icon,
                  isSuccess: isSuccess,
                ),
                const SizedBox(width: 12),
                // Message
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        isSuccess ? 'Hot Reload' : 'Reload Failed',
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      if (!isSuccess && info.errorMessage != null)
                        Text(
                          info.errorMessage!,
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 12,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        )
                      else
                        Text(
                          _formatReloadMessage(info),
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 12,
                          ),
                        ),
                    ],
                  ),
                ),
                // Duration badge (if available)
                if (info.duration != null)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.black26,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      '${info.duration!.inMilliseconds}ms',
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 11,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatReloadMessage(HotReloadInfo info) {
    final count = HotReloadNotificationManager.instance.successfulReloadCount;
    if (info.widgetType != null) {
      return '${info.widgetType} updated (#$count)';
    }
    return 'Widget updated (#$count)';
  }
}

/// Animated hot reload icon that spins on success.
class _HotReloadIcon extends StatefulWidget {
  final IconData icon;
  final bool isSuccess;

  const _HotReloadIcon({
    required this.icon,
    required this.isSuccess,
  });

  @override
  State<_HotReloadIcon> createState() => _HotReloadIconState();
}

class _HotReloadIconState extends State<_HotReloadIcon>
    with SingleTickerProviderStateMixin {
  late AnimationController _rotationController;

  @override
  void initState() {
    super.initState();
    _rotationController = AnimationController(
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );
    if (widget.isSuccess) {
      _rotationController.forward();
    }
  }

  @override
  void didUpdateWidget(_HotReloadIcon oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.isSuccess && !oldWidget.isSuccess) {
      _rotationController.reset();
      _rotationController.forward();
    }
  }

  @override
  void dispose() {
    _rotationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return RotationTransition(
      turns: Tween(begin: 0.0, end: 1.0).animate(
        CurvedAnimation(
          parent: _rotationController,
          curve: Curves.easeOutBack,
        ),
      ),
      child: Icon(
        widget.icon,
        color: Colors.white,
        size: 24,
      ),
    );
  }
}

/// A compact inline widget for showing hot reload status.
/// Useful for development toolbars or debug panels.
class HotReloadStatusIndicator extends StatefulWidget {
  /// Whether to show the reload count badge.
  final bool showCount;

  const HotReloadStatusIndicator({
    Key? key,
    this.showCount = true,
  }) : super(key: key);

  @override
  State<HotReloadStatusIndicator> createState() =>
      _HotReloadStatusIndicatorState();
}

class _HotReloadStatusIndicatorState extends State<HotReloadStatusIndicator> {
  int _reloadCount = 0;

  @override
  void initState() {
    super.initState();
    _reloadCount = HotReloadNotificationManager.instance.successfulReloadCount;
    HotReloadNotificationManager.instance.addListener(_onReload);
  }

  void _onReload(HotReloadInfo info) {
    if (info.success) {
      setState(() {
        _reloadCount++;
      });
    }
  }

  @override
  void dispose() {
    HotReloadNotificationManager.instance.removeListener(_onReload);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.black12,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.refresh,
            size: 16,
            color: _reloadCount > 0 ? Colors.green : Colors.grey,
          ),
          if (widget.showCount) ...[
            const SizedBox(width: 4),
            Text(
              '$_reloadCount',
              style: TextStyle(
                fontSize: 12,
                color: _reloadCount > 0 ? Colors.green.shade700 : Colors.grey,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ],
      ),
    );
  }
}
