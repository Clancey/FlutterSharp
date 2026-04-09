#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flutter.Internal;
using Flutter.Logging;
using Flutter.Structs;

namespace Flutter.Widgets
{
    /// <summary>
    /// A scrollable list with built-in support for infinite scrolling.
    /// </summary>
    /// <remarks>
    /// InfiniteListView combines ListViewBuilder with ScrollController's load-more detection
    /// to provide a complete infinite scrolling solution. When the user scrolls near the end,
    /// the OnLoadMore callback is triggered to fetch more data.
    ///
    /// Example usage:
    /// <code>
    /// var items = new List&lt;string&gt;();
    /// var listView = new InfiniteListView(
    ///     itemBuilder: index => new Text(items[index]),
    ///     itemCount: items.Count,
    ///     onLoadMore: async () => {
    ///         var moreItems = await FetchNextPage();
    ///         items.AddRange(moreItems);
    ///         listView.ItemCount = items.Count;
    ///     }
    /// );
    /// </code>
    /// </remarks>
    public class InfiniteListView : Widget
    {
        private readonly ScrollController _scrollController;
        private IndexedWidgetBuilder? _itemBuilder;
        private int _itemCount;
        private Widget? _loadingIndicator;
        private bool _hasMoreData = true;
        private double _loadMoreThreshold = 200.0;
        private Func<Task>? _onLoadMore;

        /// <summary>
        /// Creates an InfiniteListView with the specified item builder and load-more callback.
        /// </summary>
        /// <param name="itemBuilder">Callback to build widget for each index.</param>
        /// <param name="itemCount">Initial number of items.</param>
        /// <param name="onLoadMore">Async callback to load more items when scrolling near the end.</param>
        /// <param name="loadMoreThreshold">Distance from end (in pixels) to trigger loading. Default is 200.</param>
        /// <param name="loadingIndicator">Optional widget to show at the bottom while loading.</param>
        /// <param name="scrollController">Optional ScrollController. If not provided, one will be created.</param>
        public InfiniteListView(
            IndexedWidgetBuilder itemBuilder,
            int itemCount = 0,
            Func<Task>? onLoadMore = null,
            double loadMoreThreshold = 200.0,
            Widget? loadingIndicator = null,
            ScrollController? scrollController = null)
        {
            _itemBuilder = itemBuilder ?? throw new ArgumentNullException(nameof(itemBuilder));
            _itemCount = itemCount;
            _onLoadMore = onLoadMore;
            _loadMoreThreshold = loadMoreThreshold;
            _loadingIndicator = loadingIndicator;

            // Create or use provided scroll controller
            _scrollController = scrollController ?? new ScrollController();
            _scrollController.LoadMoreThreshold = loadMoreThreshold;
            _scrollController.OnLoadMore = HandleLoadMore;

            // Track this widget for event routing
            FlutterManager.TrackWidget(this);

            // Initialize the backing struct
            InitializeBackingStruct();
        }

        private void InitializeBackingStruct()
        {
            var s = GetBackingStruct<InfiniteListViewStruct>();
            s.itemCount = _hasMoreData ? _itemCount + 1 : _itemCount; // +1 for loading indicator
            s.Id = Id;
            s.controllerId = SetString(s.controllerId, _scrollController.ControllerId);
            s.hasLoadingIndicator = (byte)(_loadingIndicator != null || _hasMoreData ? 1 : 0);
            s.loadMoreThreshold = _loadMoreThreshold;
        }

        /// <summary>
        /// The total number of items in the list (excluding loading indicator).
        /// </summary>
        public int ItemCount
        {
            get => _itemCount;
            set
            {
                if (_itemCount != value)
                {
                    _itemCount = value;
                    UpdateBackingStruct();
                }
            }
        }

        /// <summary>
        /// Whether there is more data to load.
        /// </summary>
        /// <remarks>
        /// Set this to false when the end of the data is reached to stop showing the loading indicator
        /// and prevent further load-more triggers.
        /// </remarks>
        public bool HasMoreData
        {
            get => _hasMoreData;
            set
            {
                if (_hasMoreData != value)
                {
                    _hasMoreData = value;
                    UpdateBackingStruct();
                }
            }
        }

        /// <summary>
        /// The distance from the end at which to trigger loading more.
        /// </summary>
        public double LoadMoreThreshold
        {
            get => _loadMoreThreshold;
            set
            {
                _loadMoreThreshold = value > 0 ? value : 200.0;
                _scrollController.LoadMoreThreshold = _loadMoreThreshold;
                UpdateBackingStruct();
            }
        }

        /// <summary>
        /// Whether more content is currently being loaded.
        /// </summary>
        public bool IsLoading => _scrollController.IsLoadingMore;

        /// <summary>
        /// Callback to build widget for each index.
        /// </summary>
        public IndexedWidgetBuilder? ItemBuilder
        {
            get => _itemBuilder;
            set => _itemBuilder = value;
        }

        /// <summary>
        /// Async callback to load more items.
        /// </summary>
        public Func<Task>? OnLoadMore
        {
            get => _onLoadMore;
            set => _onLoadMore = value;
        }

        /// <summary>
        /// Widget to show at the bottom while loading.
        /// </summary>
        public Widget? LoadingIndicator
        {
            get => _loadingIndicator;
            set
            {
                _loadingIndicator = value;
                UpdateBackingStruct();
            }
        }

        /// <summary>
        /// The ScrollController used by this list.
        /// </summary>
        public ScrollController Controller => _scrollController;

        private void UpdateBackingStruct()
        {
            var s = GetBackingStruct<InfiniteListViewStruct>();
            s.itemCount = _hasMoreData ? _itemCount + 1 : _itemCount;
            s.hasLoadingIndicator = (byte)(_loadingIndicator != null || _hasMoreData ? 1 : 0);
            s.loadMoreThreshold = _loadMoreThreshold;
        }

        private async Task HandleLoadMore()
        {
            if (!_hasMoreData || _onLoadMore == null)
                return;

            try
            {
                await _onLoadMore();
            }
            catch (Exception ex)
            {
                FlutterSharpLogger.LogError(ex, "Error during OnLoadMore");
            }
        }

        /// <summary>
        /// Handles events from Dart, including ItemBuilder requests.
        /// </summary>
        public override void SendEvent(string eventName, string data, Action<string>? callback = null)
        {
            if (eventName == "ItemBuilder" && _itemBuilder != null && callback != null)
            {
                try
                {
                    // Parse the index from the data
                    int index = 0;
                    if (int.TryParse(data, out int parsedIndex))
                    {
                        index = parsedIndex;
                    }

                    // Check if this is the loading indicator position
                    if (index >= _itemCount && _hasMoreData)
                    {
                        // Build loading indicator widget
                        var indicator = _loadingIndicator ?? CreateDefaultLoadingIndicator();
                        indicator.PrepareForSending();
                        callback(((IntPtr)indicator).ToString());
                        return;
                    }

                    // Validate index is in range
                    if (index < 0 || index >= _itemCount)
                    {
                        callback("0"); // Return null pointer
                        return;
                    }

                    // Create a BuildContext for the builder callback
                    var buildContext = new BuildContext
                    {
                        Widget = this
                    };

                    // Build the widget for this index
                    var widget = _itemBuilder(buildContext, index);
                    if (widget == null)
                    {
                        callback("0"); // Return null pointer
                        return;
                    }

                    // Prepare the widget for sending and return its pointer
                    widget.PrepareForSending();
                    callback(((IntPtr)widget).ToString());
                }
                catch (Exception ex)
                {
                    FlutterSharpLogger.LogError(ex, "Error building infinite list item");
                    callback("0"); // Return null pointer on error
                }
            }
            else
            {
                base.SendEvent(eventName, data, callback);
            }
        }

        private Widget CreateDefaultLoadingIndicator()
        {
            // Create a simple centered loading text
            // In a real implementation, this would be a CircularProgressIndicator
            return new Center(child: new Text("Loading..."));
        }

        protected override FlutterObjectStruct CreateBackingStruct()
        {
            var s = new InfiniteListViewStruct();
            s.itemCount = _hasMoreData ? _itemCount + 1 : _itemCount;
            s.Id = Id;
            s.controllerId = SetString(s.controllerId, _scrollController.ControllerId);
            s.hasLoadingIndicator = (byte)(_loadingIndicator != null || _hasMoreData ? 1 : 0);
            s.loadMoreThreshold = _loadMoreThreshold;
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlutterManager.UntrackWidget(this);
                // Only dispose the controller if we created it
                // (TODO: track this with a flag)
            }
            base.Dispose(disposing);
        }
    }
}
