using System;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.Widgets
{
    /// <summary>
    /// Controls a scrollable widget.
    /// </summary>
    /// <remarks>
    /// Scroll controllers are typically stored as member variables in State objects and are reused
    /// in each Build. A single scroll controller can be used to control multiple scrollable widgets,
    /// but some operations, such as reading the scroll offset, require the controller to be used
    /// with a single scrollable widget.
    ///
    /// ScrollController extends ChangeNotifier so you can listen to scroll position changes:
    /// <code>
    /// var controller = new ScrollController();
    /// controller.AddListener(() => Console.WriteLine($"Offset: {controller.Offset}"));
    /// </code>
    /// </remarks>
    public class ScrollController : ChangeNotifier
    {
        private double _offset;
        private double _initialScrollOffset;
        private bool _keepScrollOffset;
        private string _debugLabel;
        private bool _hasClients;
        private double _maxScrollExtent;
        private double _minScrollExtent;
        private double _viewportDimension;

        // Internal ID for tracking
        private readonly string _controllerId;

        // Infinite scrolling support
        private double _loadMoreThreshold = 200.0; // Default: trigger 200 pixels before end
        private bool _isLoadingMore = false;
        private Func<System.Threading.Tasks.Task>? _onLoadMore;

        /// <summary>
        /// Creates a new ScrollController.
        /// </summary>
        /// <param name="initialScrollOffset">The initial scroll offset.</param>
        /// <param name="keepScrollOffset">Whether to restore the scroll offset when the controller's scrollable is recreated.</param>
        /// <param name="debugLabel">A label for debugging.</param>
        public ScrollController(
            double initialScrollOffset = 0.0,
            bool keepScrollOffset = true,
            string debugLabel = null)
        {
            _initialScrollOffset = initialScrollOffset;
            _keepScrollOffset = keepScrollOffset;
            _debugLabel = debugLabel;
            _offset = initialScrollOffset;
            _controllerId = Guid.NewGuid().ToString();

            // Register with FlutterManager to receive updates from Dart
            FlutterManager.RegisterScrollController(this);
        }

        /// <summary>
        /// The initial scroll offset to use when the controller's scrollable is first created.
        /// </summary>
        public double InitialScrollOffset
        {
            get => _initialScrollOffset;
            set => _initialScrollOffset = value;
        }

        /// <summary>
        /// Whether to save and restore the scroll offset with PageStorage.
        /// </summary>
        public bool KeepScrollOffset
        {
            get => _keepScrollOffset;
            set => _keepScrollOffset = value;
        }

        /// <summary>
        /// A label used for debugging.
        /// </summary>
        public string DebugLabel
        {
            get => _debugLabel;
            set => _debugLabel = value;
        }

        /// <summary>
        /// The current scroll offset of the scrollable widget(s) that this controller controls.
        /// </summary>
        public double Offset
        {
            get => _offset;
            internal set
            {
                if (Math.Abs(_offset - value) > double.Epsilon)
                {
                    _offset = value;
                    NotifyListeners();
                }
            }
        }

        /// <summary>
        /// Whether any ScrollPosition objects are attached to this controller.
        /// </summary>
        public bool HasClients
        {
            get => _hasClients;
            internal set => _hasClients = value;
        }

        /// <summary>
        /// The maximum in-range value for the scroll offset.
        /// </summary>
        public double MaxScrollExtent
        {
            get => _maxScrollExtent;
            internal set => _maxScrollExtent = value;
        }

        /// <summary>
        /// The minimum in-range value for the scroll offset.
        /// </summary>
        public double MinScrollExtent
        {
            get => _minScrollExtent;
            internal set => _minScrollExtent = value;
        }

        /// <summary>
        /// The dimension of the viewport in the main axis.
        /// </summary>
        public double ViewportDimension
        {
            get => _viewportDimension;
            internal set => _viewportDimension = value;
        }

        /// <summary>
        /// Gets the unique identifier for this controller.
        /// </summary>
        internal string ControllerId => _controllerId;

        /// <summary>
        /// The distance from the bottom at which to trigger loading more content.
        /// Default is 200 pixels.
        /// </summary>
        /// <remarks>
        /// When the scroll position is within this distance from the maximum scroll extent,
        /// the OnLoadMore callback will be triggered.
        /// </remarks>
        public double LoadMoreThreshold
        {
            get => _loadMoreThreshold;
            set => _loadMoreThreshold = value > 0 ? value : 200.0;
        }

        /// <summary>
        /// Whether the controller is currently loading more content.
        /// </summary>
        /// <remarks>
        /// This is set to true when OnLoadMore is triggered and reset to false
        /// when the task completes. Use this to prevent duplicate load requests.
        /// </remarks>
        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            private set => _isLoadingMore = value;
        }

        /// <summary>
        /// Callback invoked when the scroll position is near the end (within LoadMoreThreshold).
        /// </summary>
        /// <remarks>
        /// Set this callback to load additional content when the user scrolls near the end.
        /// The callback should return a Task that completes when loading is done.
        /// Multiple simultaneous calls are prevented while IsLoadingMore is true.
        ///
        /// Example:
        /// <code>
        /// controller.OnLoadMore = async () => {
        ///     var moreItems = await FetchMoreItems(page++);
        ///     items.AddRange(moreItems);
        ///     listView.ItemCount = items.Count;
        /// };
        /// </code>
        /// </remarks>
        public Func<System.Threading.Tasks.Task>? OnLoadMore
        {
            get => _onLoadMore;
            set => _onLoadMore = value;
        }

        /// <summary>
        /// Event raised when more content needs to be loaded (user scrolled near end).
        /// </summary>
        /// <remarks>
        /// This is an alternative to the OnLoadMore callback for event-based patterns.
        /// Both can be used simultaneously but will fire independently.
        /// </remarks>
        public event Func<System.Threading.Tasks.Task>? LoadMoreRequested;

        /// <summary>
        /// Event raised when a ScrollPosition is attached to this controller.
        /// </summary>
        public event Action<ScrollController> OnAttach;

        /// <summary>
        /// Event raised when a ScrollPosition is detached from this controller.
        /// </summary>
        public event Action<ScrollController> OnDetach;

        /// <summary>
        /// Event raised when scrolling starts.
        /// </summary>
        public event Action<ScrollStartDetails> OnScrollStart;

        /// <summary>
        /// Event raised when the scroll position updates.
        /// </summary>
        public event Action<ScrollUpdateDetails> OnScrollUpdate;

        /// <summary>
        /// Event raised when scrolling ends.
        /// </summary>
        public event Action<ScrollEndDetails> OnScrollEnd;

        /// <summary>
        /// Jumps the scroll position to the given offset without animation.
        /// </summary>
        /// <param name="offset">The target scroll offset.</param>
        public void JumpTo(double offset)
        {
            if (!_hasClients)
                return;

            // Send scroll command to Dart
            FlutterManager.SendScrollCommand(_controllerId, "jumpTo", offset);
        }

        /// <summary>
        /// Animates the scroll position to the given offset.
        /// </summary>
        /// <param name="offset">The target scroll offset.</param>
        /// <param name="duration">The duration of the animation.</param>
        /// <param name="curve">The curve to use for the animation (optional).</param>
        public void AnimateTo(double offset, TimeSpan duration, string curve = "easeInOut")
        {
            if (!_hasClients)
                return;

            // Send animated scroll command to Dart
            FlutterManager.SendScrollCommand(_controllerId, "animateTo", offset, duration.TotalMilliseconds, curve);
        }

        /// <summary>
        /// Called when the Dart side notifies us of a scroll position update.
        /// </summary>
        internal void UpdateFromDart(double offset, double maxScrollExtent, double minScrollExtent, double viewportDimension, bool hasClients)
        {
            _maxScrollExtent = maxScrollExtent;
            _minScrollExtent = minScrollExtent;
            _viewportDimension = viewportDimension;
            _hasClients = hasClients;
            Offset = offset; // This will notify listeners if changed
        }

        /// <summary>
        /// Called when a ScrollPosition is attached on the Dart side.
        /// </summary>
        internal void NotifyAttach()
        {
            _hasClients = true;
            OnAttach?.Invoke(this);
        }

        /// <summary>
        /// Called when a ScrollPosition is detached on the Dart side.
        /// </summary>
        internal void NotifyDetach()
        {
            _hasClients = false;
            OnDetach?.Invoke(this);
        }

        /// <summary>
        /// Called when scrolling starts on the Dart side.
        /// </summary>
        internal void NotifyScrollStart(ScrollStartDetails details)
        {
            OnScrollStart?.Invoke(details);
        }

        /// <summary>
        /// Called when scroll position updates on the Dart side.
        /// </summary>
        internal void NotifyScrollUpdate(ScrollUpdateDetails details)
        {
            Offset = details.Offset;
            _maxScrollExtent = details.MaxScrollExtent;
            _minScrollExtent = details.MinScrollExtent;
            _viewportDimension = details.ViewportDimension;
            OnScrollUpdate?.Invoke(details);

            // Check if we need to load more content
            CheckAndTriggerLoadMore();
        }

        /// <summary>
        /// Checks if the scroll position is near the end and triggers loading more content.
        /// </summary>
        private async void CheckAndTriggerLoadMore()
        {
            // Don't trigger if already loading or no callback registered
            if (_isLoadingMore || (_onLoadMore == null && LoadMoreRequested == null))
                return;

            // Don't trigger if scroll extent is not valid
            if (_maxScrollExtent <= 0 || double.IsNaN(_maxScrollExtent) || double.IsInfinity(_maxScrollExtent))
                return;

            // Check if we're within the threshold of the end
            double distanceFromEnd = _maxScrollExtent - _offset;
            if (distanceFromEnd <= _loadMoreThreshold && distanceFromEnd >= 0)
            {
                _isLoadingMore = true;

                try
                {
                    // Invoke the callback
                    if (_onLoadMore != null)
                    {
                        await _onLoadMore();
                    }

                    // Invoke the event
                    if (LoadMoreRequested != null)
                    {
                        await LoadMoreRequested();
                    }
                }
                catch (Exception ex)
                {
                    FlutterSharpLogger.LogError(ex, "Error during OnLoadMore");
                }
                finally
                {
                    _isLoadingMore = false;
                }
            }
        }

        /// <summary>
        /// Manually triggers loading more content, regardless of scroll position.
        /// </summary>
        /// <remarks>
        /// Use this to programmatically trigger loading, for example after an initial data fetch.
        /// </remarks>
        public async System.Threading.Tasks.Task TriggerLoadMore()
        {
            if (_isLoadingMore)
                return;

            _isLoadingMore = true;

            try
            {
                if (_onLoadMore != null)
                {
                    await _onLoadMore();
                }

                if (LoadMoreRequested != null)
                {
                    await LoadMoreRequested();
                }
            }
            finally
            {
                _isLoadingMore = false;
            }
        }

        /// <summary>
        /// Resets the loading state, allowing new load-more triggers.
        /// </summary>
        /// <remarks>
        /// Call this if loading was cancelled or failed and you want to retry.
        /// </remarks>
        public void ResetLoadingState()
        {
            _isLoadingMore = false;
        }

        /// <summary>
        /// Called when scrolling ends on the Dart side.
        /// </summary>
        internal void NotifyScrollEnd(ScrollEndDetails details)
        {
            Offset = details.Offset;
            OnScrollEnd?.Invoke(details);
        }


        /// <summary>
        /// Disposes the controller and cleans up resources.
        /// </summary>
        public override void Dispose()
        {
            // Unregister from FlutterManager
            FlutterManager.UnregisterScrollController(_controllerId);

            base.Dispose();
        }

        /// <summary>
        /// Returns a string representation of this controller.
        /// </summary>
        public override string ToString()
        {
            var label = string.IsNullOrEmpty(_debugLabel) ? "" : $"({_debugLabel})";
            return $"ScrollController{label}(offset: {_offset:F1}, hasClients: {_hasClients})";
        }
    }

    /// <summary>
    /// Details for scroll start events.
    /// </summary>
    public class ScrollStartDetails
    {
        /// <summary>
        /// The scroll offset at the start of the scroll.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// The axis direction of the scroll.
        /// </summary>
        public AxisDirection AxisDirection { get; set; }
    }

    /// <summary>
    /// Details for scroll update events.
    /// </summary>
    public class ScrollUpdateDetails
    {
        /// <summary>
        /// The current scroll offset.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// The change in scroll offset since the last update.
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// The maximum scroll extent.
        /// </summary>
        public double MaxScrollExtent { get; set; }

        /// <summary>
        /// The minimum scroll extent.
        /// </summary>
        public double MinScrollExtent { get; set; }

        /// <summary>
        /// The dimension of the viewport.
        /// </summary>
        public double ViewportDimension { get; set; }

        /// <summary>
        /// The axis direction of the scroll.
        /// </summary>
        public AxisDirection AxisDirection { get; set; }
    }

    /// <summary>
    /// Details for scroll end events.
    /// </summary>
    public class ScrollEndDetails
    {
        /// <summary>
        /// The scroll offset at the end of the scroll.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// The velocity at the end of the scroll (in pixels per second).
        /// </summary>
        public double Velocity { get; set; }

        /// <summary>
        /// The axis direction of the scroll.
        /// </summary>
        public AxisDirection AxisDirection { get; set; }
    }

    /// <summary>
    /// The direction in which a scrollable axis is oriented.
    /// </summary>
    public enum AxisDirection
    {
        /// <summary>
        /// Zero is at the bottom and positive values are above it.
        /// </summary>
        Up = 0,

        /// <summary>
        /// Zero is on the left and positive values are to the right of it.
        /// </summary>
        Right = 1,

        /// <summary>
        /// Zero is at the top and positive values are below it.
        /// </summary>
        Down = 2,

        /// <summary>
        /// Zero is on the right and positive values are to the left of it.
        /// </summary>
        Left = 3
    }
}
