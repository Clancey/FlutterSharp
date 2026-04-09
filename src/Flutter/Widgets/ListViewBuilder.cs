#nullable enable
using System;
using System.Text.Json;
using Flutter.Internal;
using Flutter.Logging;
using Flutter.Structs;

namespace Flutter.Widgets
{
	/// <summary>
	/// A scrollable list that builds items on demand.
	/// Similar to Flutter's ListView.builder constructor.
	/// </summary>
	/// <remarks>
	/// The itemBuilder callback is invoked asynchronously when Dart needs to
	/// display an item. The returned widget is sent back to Dart for rendering.
	/// </remarks>
	public class ListViewBuilder : Widget
	{
		private IndexedWidgetBuilder? _itemBuilder;
		private int _itemCount;

		/// <summary>
		/// Creates a ListViewBuilder widget.
		/// </summary>
		/// <param name="itemCount">Total number of items in the list</param>
		/// <param name="itemBuilder">Callback to build widget for each index</param>
		public ListViewBuilder(int itemCount, IndexedWidgetBuilder itemBuilder)
		{
			_itemCount = itemCount;
			_itemBuilder = itemBuilder ?? throw new ArgumentNullException(nameof(itemBuilder));

			// Track this widget so events can be routed to it
			FlutterManager.TrackWidget(this);

			// Initialize the backing struct
			var s = GetBackingStruct<ListViewBuilderStruct>();
			s.itemCount = _itemCount;
			s.Id = Id; // Set the ID so Dart can identify this widget
		}

		/// <summary>
		/// Number of items in the list
		/// </summary>
		public int ItemCount
		{
			get => _itemCount;
			set
			{
				_itemCount = value;
				var s = GetBackingStruct<ListViewBuilderStruct>();
				s.itemCount = value;
			}
		}

		/// <summary>
		/// Callback to build widget for each index
		/// </summary>
		public IndexedWidgetBuilder? ItemBuilder
		{
			get => _itemBuilder;
			set => _itemBuilder = value;
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
					else
					{
						// Try parsing as JSON if it's wrapped
						try
						{
							var jsonDoc = JsonDocument.Parse(data);
							if (jsonDoc.RootElement.TryGetInt32(out int jsonIndex))
							{
								index = jsonIndex;
							}
						}
						catch
						{
							// Use 0 as fallback
						}
					}

					// Validate index is in range
					if (index < 0 || index >= _itemCount)
					{
						FlutterSharpLogger.LogWarning("Index {Index} out of range (0-{MaxIndex})", index, _itemCount - 1);
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
					var ptr = (IntPtr)widget;
					callback(ptr.ToString());
				}
				catch (Exception ex)
				{
					FlutterSharpLogger.LogError(ex, "Error building list item");
					callback("0"); // Return null pointer on error
				}
			}
			else
			{
				base.SendEvent(eventName, data, callback);
			}
		}

		protected override FlutterObjectStruct CreateBackingStruct()
		{
			var s = new ListViewBuilderStruct();
			s.itemCount = _itemCount;
			s.Id = Id;
			return s;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FlutterManager.UntrackWidget(this);
			}
			base.Dispose(disposing);
		}
	}
}
