#nullable enable
using System;
using System.Text.Json;
using Flutter.Internal;
using Flutter.Structs;

namespace Flutter.Widgets
{
	/// <summary>
	/// A scrollable grid that builds items on demand.
	/// Similar to Flutter's GridView.builder constructor.
	/// </summary>
	/// <remarks>
	/// The itemBuilder callback is invoked asynchronously when Dart needs to
	/// display an item. The returned widget is sent back to Dart for rendering.
	/// </remarks>
	public class GridViewBuilder : Widget
	{
		private Func<int, Widget>? _itemBuilder;
		private int _itemCount;
		private int _crossAxisCount;
		private double _mainAxisSpacing;
		private double _crossAxisSpacing;
		private double _childAspectRatio;

		/// <summary>
		/// Creates a GridViewBuilder widget.
		/// </summary>
		/// <param name="itemCount">Total number of items in the grid</param>
		/// <param name="crossAxisCount">Number of columns in the grid</param>
		/// <param name="itemBuilder">Callback to build widget for each index</param>
		/// <param name="mainAxisSpacing">Spacing between rows (default 0.0)</param>
		/// <param name="crossAxisSpacing">Spacing between columns (default 0.0)</param>
		/// <param name="childAspectRatio">Width to height ratio of each item (default 1.0)</param>
		public GridViewBuilder(
			int itemCount,
			int crossAxisCount,
			Func<int, Widget> itemBuilder,
			double mainAxisSpacing = 0.0,
			double crossAxisSpacing = 0.0,
			double childAspectRatio = 1.0)
		{
			_itemCount = itemCount;
			_crossAxisCount = crossAxisCount;
			_itemBuilder = itemBuilder ?? throw new ArgumentNullException(nameof(itemBuilder));
			_mainAxisSpacing = mainAxisSpacing;
			_crossAxisSpacing = crossAxisSpacing;
			_childAspectRatio = childAspectRatio;

			// Track this widget so events can be routed to it
			FlutterManager.TrackWidget(this);

			// Initialize the backing struct
			var s = GetBackingStruct<GridViewBuilderStruct>();
			s.itemCount = _itemCount;
			s.crossAxisCount = _crossAxisCount;
			s.mainAxisSpacing = _mainAxisSpacing;
			s.crossAxisSpacing = _crossAxisSpacing;
			s.childAspectRatio = _childAspectRatio;
			s.Id = Id;
		}

		/// <summary>
		/// Number of items in the grid
		/// </summary>
		public int ItemCount
		{
			get => _itemCount;
			set
			{
				_itemCount = value;
				if (backingStruct is GridViewBuilderStruct s)
				{
					s.itemCount = value;
				}
			}
		}

		/// <summary>
		/// Number of columns in the grid
		/// </summary>
		public int CrossAxisCount
		{
			get => _crossAxisCount;
			set
			{
				_crossAxisCount = value;
				if (backingStruct is GridViewBuilderStruct s)
				{
					s.crossAxisCount = value;
				}
			}
		}

		/// <summary>
		/// Spacing between rows
		/// </summary>
		public double MainAxisSpacing
		{
			get => _mainAxisSpacing;
			set
			{
				_mainAxisSpacing = value;
				if (backingStruct is GridViewBuilderStruct s)
				{
					s.mainAxisSpacing = value;
				}
			}
		}

		/// <summary>
		/// Spacing between columns
		/// </summary>
		public double CrossAxisSpacing
		{
			get => _crossAxisSpacing;
			set
			{
				_crossAxisSpacing = value;
				if (backingStruct is GridViewBuilderStruct s)
				{
					s.crossAxisSpacing = value;
				}
			}
		}

		/// <summary>
		/// Width to height ratio of each item
		/// </summary>
		public double ChildAspectRatio
		{
			get => _childAspectRatio;
			set
			{
				_childAspectRatio = value;
				if (backingStruct is GridViewBuilderStruct s)
				{
					s.childAspectRatio = value;
				}
			}
		}

		/// <summary>
		/// Callback to build widget for each index
		/// </summary>
		public Func<int, Widget>? ItemBuilder
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
						Console.WriteLine($"GridViewBuilder: Index {index} out of range (0-{_itemCount - 1})");
						callback("0"); // Return null pointer
						return;
					}

					// Build the widget for this index
					var widget = _itemBuilder(index);
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
					Console.WriteLine($"GridViewBuilder: Error building item at index: {ex}");
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
			var s = new GridViewBuilderStruct();
			s.itemCount = _itemCount;
			s.crossAxisCount = _crossAxisCount;
			s.mainAxisSpacing = _mainAxisSpacing;
			s.crossAxisSpacing = _crossAxisSpacing;
			s.childAspectRatio = _childAspectRatio;
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
