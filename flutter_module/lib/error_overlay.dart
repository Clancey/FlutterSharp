import 'dart:async';
import 'package:flutter/material.dart';

/// Represents an error message from C# to be displayed in the overlay.
class ErrorInfo {
  final String errorType;
  final String message;
  final String? stackTrace;
  final DateTime timestamp;
  final String? widgetType;
  final int? callbackId;
  final bool isRecoverable;

  ErrorInfo({
    required this.errorType,
    required this.message,
    this.stackTrace,
    DateTime? timestamp,
    this.widgetType,
    this.callbackId,
    this.isRecoverable = false,
  }) : timestamp = timestamp ?? DateTime.now();

  factory ErrorInfo.fromJson(Map<String, dynamic> json) {
    DateTime? ts;
    if (json['timestamp'] != null) {
      try {
        ts = DateTime.parse(json['timestamp'] as String);
      } catch (_) {
        ts = DateTime.now();
      }
    }

    return ErrorInfo(
      errorType: json['errorType'] as String? ?? 'Error',
      message: json['message'] as String? ?? 'Unknown error',
      stackTrace: json['stackTrace'] as String?,
      timestamp: ts,
      widgetType: json['widgetType'] as String?,
      callbackId: json['callbackId'] as int?,
      isRecoverable: json['isRecoverable'] as bool? ?? false,
    );
  }
}

/// Singleton manager for distributing errors to overlay listeners.
class ErrorOverlayManager {
  static final ErrorOverlayManager _instance = ErrorOverlayManager._();
  static ErrorOverlayManager get instance => _instance;

  ErrorOverlayManager._();

  final List<void Function(ErrorInfo)> _listeners = [];
  final List<ErrorInfo> _errorHistory = [];
  static const int _maxHistorySize = 50;

  /// Adds a listener for error events.
  void addListener(void Function(ErrorInfo) listener) {
    _listeners.add(listener);
  }

  /// Removes an error listener.
  void removeListener(void Function(ErrorInfo) listener) {
    _listeners.remove(listener);
  }

  /// Shows an error to all registered listeners.
  void showError(ErrorInfo error) {
    // Add to history
    _errorHistory.add(error);
    if (_errorHistory.length > _maxHistorySize) {
      _errorHistory.removeAt(0);
    }

    // Notify listeners
    for (final listener in _listeners) {
      listener(error);
    }
  }

  /// Gets recent error history.
  List<ErrorInfo> get errorHistory => List.unmodifiable(_errorHistory);

  /// Clears error history.
  void clearHistory() {
    _errorHistory.clear();
  }
}

/// A widget that wraps content and displays error overlays.
/// Wrap your root widget with this to show errors from C#.
class ErrorOverlay extends StatefulWidget {
  final Widget child;

  /// Auto-dismiss errors after this duration. Set to null to disable.
  final Duration? autoDismissDuration;

  /// Whether to show the stack trace by default.
  final bool expandStackTraceByDefault;

  const ErrorOverlay({
    Key? key,
    required this.child,
    this.autoDismissDuration = const Duration(seconds: 8),
    this.expandStackTraceByDefault = false,
  }) : super(key: key);

  @override
  State<ErrorOverlay> createState() => _ErrorOverlayState();
}

class _ErrorOverlayState extends State<ErrorOverlay>
    with SingleTickerProviderStateMixin {
  ErrorInfo? _currentError;
  late AnimationController _animationController;
  late Animation<Offset> _slideAnimation;
  Timer? _autoDismissTimer;
  bool _stackTraceExpanded = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );
    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, -1),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutCubic,
    ));

    _stackTraceExpanded = widget.expandStackTraceByDefault;
    ErrorOverlayManager.instance.addListener(_onErrorReceived);
  }

  void _onErrorReceived(ErrorInfo error) {
    // Cancel any existing auto-dismiss timer
    _autoDismissTimer?.cancel();

    setState(() {
      _currentError = error;
      _stackTraceExpanded = widget.expandStackTraceByDefault;
    });

    _animationController.forward();

    // Set up auto-dismiss
    if (widget.autoDismissDuration != null) {
      _autoDismissTimer = Timer(widget.autoDismissDuration!, _dismissError);
    }
  }

  void _dismissError() {
    _autoDismissTimer?.cancel();
    _animationController.reverse().then((_) {
      if (mounted) {
        setState(() {
          _currentError = null;
        });
      }
    });
  }

  @override
  void dispose() {
    _autoDismissTimer?.cancel();
    _animationController.dispose();
    ErrorOverlayManager.instance.removeListener(_onErrorReceived);
    super.dispose();
  }

  Color _getErrorColor(String errorType) {
    switch (errorType) {
      case 'CallbackError':
        return Colors.red.shade800;
      case 'WidgetParseError':
        return Colors.orange.shade800;
      case 'CommunicationError':
        return Colors.purple.shade800;
      case 'FrameworkError':
        return Colors.deepOrange.shade800;
      default:
        return Colors.red.shade900;
    }
  }

  IconData _getErrorIcon(String errorType) {
    switch (errorType) {
      case 'CallbackError':
        return Icons.error_outline;
      case 'WidgetParseError':
        return Icons.widgets_outlined;
      case 'CommunicationError':
        return Icons.sync_problem;
      case 'FrameworkError':
        return Icons.warning_amber_outlined;
      default:
        return Icons.error;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        widget.child,
        if (_currentError != null)
          Positioned(
            top: 0,
            left: 0,
            right: 0,
            child: SafeArea(
              child: SlideTransition(
                position: _slideAnimation,
                child: _buildErrorBanner(_currentError!),
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildErrorBanner(ErrorInfo error) {
    final errorColor = _getErrorColor(error.errorType);

    return Material(
      elevation: 8,
      child: Container(
        decoration: BoxDecoration(
          color: errorColor,
          border: Border(
            bottom: BorderSide(
              color: Colors.red.shade400,
              width: 2,
            ),
          ),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Header row
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 12, 8, 8),
              child: Row(
                children: [
                  Icon(
                    _getErrorIcon(error.errorType),
                    color: Colors.white,
                    size: 24,
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          error.errorType,
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 12,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          error.message,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 3,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close, color: Colors.white),
                    onPressed: _dismissError,
                    tooltip: 'Dismiss',
                  ),
                ],
              ),
            ),

            // Additional info row
            if (error.widgetType != null || error.callbackId != null)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Wrap(
                  spacing: 16,
                  children: [
                    if (error.widgetType != null)
                      _buildInfoChip('Widget', error.widgetType!),
                    if (error.callbackId != null)
                      _buildInfoChip('Callback', '#${error.callbackId}'),
                  ],
                ),
              ),

            // Stack trace section
            if (error.stackTrace != null && error.stackTrace!.isNotEmpty)
              _buildStackTraceSection(error.stackTrace!),

            // Bottom padding
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoChip(String label, String value) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.black26,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '$label: ',
            style: const TextStyle(
              color: Colors.white60,
              fontSize: 11,
            ),
          ),
          Text(
            value,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 11,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStackTraceSection(String stackTrace) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        InkWell(
          onTap: () {
            setState(() {
              _stackTraceExpanded = !_stackTraceExpanded;
            });
          },
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Row(
              children: [
                Icon(
                  _stackTraceExpanded ? Icons.expand_less : Icons.expand_more,
                  color: Colors.white70,
                  size: 20,
                ),
                const SizedBox(width: 4),
                const Text(
                  'Stack Trace',
                  style: TextStyle(
                    color: Colors.white70,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          ),
        ),
        if (_stackTraceExpanded)
          Container(
            width: double.infinity,
            margin: const EdgeInsets.symmetric(horizontal: 16),
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.black38,
              borderRadius: BorderRadius.circular(8),
            ),
            constraints: const BoxConstraints(maxHeight: 200),
            child: SingleChildScrollView(
              child: Text(
                stackTrace,
                style: const TextStyle(
                  color: Colors.white70,
                  fontSize: 10,
                  fontFamily: 'monospace',
                ),
              ),
            ),
          ),
      ],
    );
  }
}

/// A widget that catches errors in its child subtree and displays a fallback UI.
///
/// Unlike [ErrorOverlay] which shows toast-like notifications, [ErrorBoundary]
/// replaces its child with a fallback widget when an error occurs.
///
/// Example:
/// ```dart
/// ErrorBoundary(
///   fallbackBuilder: (error, retry) => Column(
///     children: [
///       Text('Something went wrong: ${error.message}'),
///       ElevatedButton(onPressed: retry, child: Text('Retry')),
///     ],
///   ),
///   child: MyWidget(),
/// )
/// ```
class ErrorBoundary extends StatefulWidget {
  /// The widget to wrap with error handling.
  final Widget child;

  /// Builder for the fallback UI when an error occurs.
  /// If null, a default error UI is shown.
  final Widget Function(ErrorInfo error, VoidCallback retry)? fallbackBuilder;

  /// Called when an error is caught.
  final void Function(ErrorInfo error)? onError;

  /// Whether to also show the error in the global [ErrorOverlay].
  final bool showInOverlay;

  /// Whether to send the error to C# for logging.
  final bool reportToNative;

  /// Optional widget type name for error reporting context.
  final String? widgetTypeName;

  const ErrorBoundary({
    Key? key,
    required this.child,
    this.fallbackBuilder,
    this.onError,
    this.showInOverlay = true,
    this.reportToNative = true,
    this.widgetTypeName,
  }) : super(key: key);

  @override
  State<ErrorBoundary> createState() => _ErrorBoundaryState();
}

class _ErrorBoundaryState extends State<ErrorBoundary> {
  ErrorInfo? _error;
  int _retryCount = 0;

  @override
  Widget build(BuildContext context) {
    if (_error != null) {
      return widget.fallbackBuilder != null
          ? widget.fallbackBuilder!(_error!, _retry)
          : _buildDefaultFallback(_error!);
    }

    // Provide ErrorBoundaryScope so children can programmatically report errors
    return ErrorBoundaryScope(
      reportError: _handleError,
      child: Builder(
        builder: (context) {
          try {
            return widget.child;
          } catch (error, stackTrace) {
            // Schedule error handling for next frame to avoid setState during build
            WidgetsBinding.instance.addPostFrameCallback((_) {
              _handleError(error, stackTrace);
            });
            return const SizedBox.shrink();
          }
        },
      ),
    );
  }

  void _handleError(Object error, StackTrace stackTrace) {
    if (_error != null) return; // Already handling an error

    final errorInfo = ErrorInfo(
      errorType: 'WidgetError',
      message: error.toString(),
      stackTrace: stackTrace.toString(),
      widgetType: widget.widgetTypeName,
      isRecoverable: true,
    );

    setState(() {
      _error = errorInfo;
    });

    // Notify callback
    widget.onError?.call(errorInfo);

    // Show in global overlay
    if (widget.showInOverlay) {
      ErrorOverlayManager.instance.showError(errorInfo);
    }

    // Log the error
    debugPrint('[ErrorBoundary] Caught error: $error');
    debugPrint('[ErrorBoundary] Stack: $stackTrace');
    if (widget.widgetTypeName != null) {
      debugPrint('[ErrorBoundary] Widget: ${widget.widgetTypeName}');
    }
  }

  void _retry() {
    setState(() {
      _error = null;
      _retryCount++;
      debugPrint('[ErrorBoundary] Retry attempt $_retryCount');
    });
  }

  Widget _buildDefaultFallback(ErrorInfo error) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        border: Border.all(color: Colors.red.shade300, width: 2),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.error_outline, color: Colors.red.shade700, size: 48),
          const SizedBox(height: 12),
          Text(
            'Something went wrong',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Colors.red.shade900,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            error.message,
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.red.shade800),
            maxLines: 3,
            overflow: TextOverflow.ellipsis,
          ),
          if (error.widgetType != null) ...[
            const SizedBox(height: 4),
            Text(
              'Widget: ${error.widgetType}',
              style: TextStyle(color: Colors.red.shade600, fontSize: 12),
            ),
          ],
          const SizedBox(height: 16),
          ElevatedButton.icon(
            onPressed: _retry,
            icon: const Icon(Icons.refresh),
            label: const Text('Retry'),
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red.shade700,
              foregroundColor: Colors.white,
            ),
          ),
        ],
      ),
    );
  }
}

/// Provides programmatic error reporting for ErrorBoundary widgets.
/// Use this to manually trigger an error in an ancestor ErrorBoundary.
class ErrorBoundaryScope extends InheritedWidget {
  final void Function(Object error, StackTrace stackTrace) reportError;

  const ErrorBoundaryScope({
    Key? key,
    required this.reportError,
    required Widget child,
  }) : super(key: key, child: child);

  /// Reports an error to the nearest ErrorBoundary ancestor.
  static void of(BuildContext context, Object error, [StackTrace? stackTrace]) {
    final scope =
        context.dependOnInheritedWidgetOfExactType<ErrorBoundaryScope>();
    if (scope != null) {
      scope.reportError(error, stackTrace ?? StackTrace.current);
    } else {
      // No ErrorBoundary found, just show in overlay
      ErrorOverlayManager.instance.showError(ErrorInfo(
        errorType: 'UnhandledError',
        message: error.toString(),
        stackTrace: stackTrace?.toString(),
      ));
    }
  }

  /// Returns true if there is an ErrorBoundary ancestor.
  static bool hasErrorBoundary(BuildContext context) {
    return context.dependOnInheritedWidgetOfExactType<ErrorBoundaryScope>() !=
        null;
  }

  @override
  bool updateShouldNotify(ErrorBoundaryScope oldWidget) => false;
}

/// A simpler error display widget for inline use.
class ErrorDisplay extends StatelessWidget {
  final ErrorInfo error;
  final VoidCallback? onDismiss;
  final VoidCallback? onRetry;

  const ErrorDisplay({
    Key? key,
    required this.error,
    this.onDismiss,
    this.onRetry,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        border: Border.all(color: Colors.red.shade200),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.error_outline, color: Colors.red.shade700),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  error.errorType,
                  style: TextStyle(
                    color: Colors.red.shade800,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              if (onDismiss != null)
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: onDismiss,
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(),
                ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            error.message,
            style: TextStyle(color: Colors.red.shade900),
          ),
          if (error.isRecoverable && onRetry != null) ...[
            const SizedBox(height: 12),
            TextButton.icon(
              onPressed: onRetry,
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
            ),
          ],
        ],
      ),
    );
  }
}
