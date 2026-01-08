using System;
using System.Threading.Tasks;
using Flutter;
using Flutter.Enums;
using Flutter.Internal;
using Flutter.Material;
using Flutter.Structs;

namespace Flutter.Widgets
{
    /// <summary>
    /// A widget that supports the Material "swipe to refresh" idiom.
    /// </summary>
    /// <remarks>
    /// When the child's Scrollable descendant overscrolls, an animated circular
    /// progress indicator is faded into view. When the scroll ends, if the
    /// indicator has been dragged far enough for it to become completely opaque,
    /// the onRefresh callback is called. The callback is expected to update the
    /// scrollable's contents and then complete the Future it returns.
    ///
    /// The RefreshIndicator will remain visible until the Future completes.
    ///
    /// Example:
    /// <code>
    /// var listView = new ListView { ... };
    /// var refreshIndicator = new RefreshIndicator(
    ///     child: listView,
    ///     onRefresh: async () => {
    ///         await LoadDataAsync();
    ///     }
    /// );
    /// </code>
    /// </remarks>
    public class RefreshIndicator : StatefulWidget
    {
        private Func<Task>? _onRefresh;

        /// <summary>
        /// Initializes a new instance of the RefreshIndicator class.
        /// </summary>
        /// <param name="child">The scrollable child widget.</param>
        /// <param name="onRefresh">A function that's called when the user has dragged the refresh indicator far enough to trigger a refresh. Returns a Future that completes when the refresh is done.</param>
        /// <param name="displacement">The distance from the top where the refresh indicator will settle. Default is 40.0.</param>
        /// <param name="edgeOffset">The offset from the top for the progress indicator. Default is 0.0.</param>
        /// <param name="color">The progress indicator's foreground color.</param>
        /// <param name="backgroundColor">The progress indicator's background color.</param>
        /// <param name="triggerMode">Defines how the trigger gesture is handled.</param>
        /// <param name="strokeWidth">The thickness of the progress indicator's stroke. Default is 2.5.</param>
        public RefreshIndicator(
            Widget? child = null,
            Func<Task>? onRefresh = null,
            double displacement = 40.0,
            double edgeOffset = 0.0,
            uint? color = null,
            uint? backgroundColor = null,
            RefreshIndicatorTriggerMode triggerMode = RefreshIndicatorTriggerMode.OnEdge,
            double strokeWidth = 2.5)
        {
            _onRefresh = onRefresh;
            var s = GetBackingStruct<RefreshIndicatorStruct>();

            // Register the async callback
            // When Dart triggers this, it will wait for the Task to complete
            s.onRefreshAction = RegisterAsyncCallback(onRefresh);

            // Assign child widget
            s.child = GetWidgetHandle(child);

            // Assign properties
            s.displacement = displacement;
            s.edgeOffset = edgeOffset;
            s.strokeWidth = strokeWidth;
            s.triggerMode = triggerMode;

            if (color.HasValue)
            {
                s.color = color.Value;
                s.Hascolor = 1;
            }

            if (backgroundColor.HasValue)
            {
                s.backgroundColor = backgroundColor.Value;
                s.HasbackgroundColor = 1;
            }
        }

        /// <summary>
        /// Registers an async callback and returns its action ID.
        /// </summary>
        private string? RegisterAsyncCallback(Func<Task>? callback)
        {
            if (callback == null) return null;

            // Wrap the async callback to notify when complete
            Action wrappedAction = () =>
            {
                // Start the async operation
                var task = callback();

                // When complete, notify Dart
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"RefreshIndicator.onRefresh error: {t.Exception}");
                    }

                    // Signal completion to Dart
                    FlutterManager.SendAsyncCallbackComplete(Id);
                });
            };

            return RegisterCallback(wrappedAction);
        }

        protected override FlutterObjectStruct CreateBackingStruct() => new RefreshIndicatorStruct();
    }
}
