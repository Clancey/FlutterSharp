using System;
using Flutter.Internal;

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
