using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Internal;
using Flutter.Structs;
using Flutter.Widgets;
using Flutter.Enums;
using Flutter.UI;

namespace FlutterSample
{
	/// <summary>
	/// A counter page that demonstrates state management with StatefulWidget
	/// </summary>
	public class CounterPage : StatefulWidget
	{
		private int _count = 0;

		public override Widget Build() =>
			new Center(child: new Column
			{
				new Text($"You have pressed the button {_count} times"),
				new SizedBox(height: 20),
				new Text("Press + to increment"),
			});

		// TODO: Add FloatingActionButton when button callbacks are implemented
	}

	/// <summary>
	/// A simple list page that demonstrates basic layout
	/// </summary>
	public class ListPage : StatefulWidget
	{
		private List<string> _items = new List<string> { "Item 1", "Item 2", "Item 3" };

		public override Widget Build() =>
			new Column
			{
				new Text("Todo List", style: null),
				new Expanded(child: new Column
				{
					new Text(_items.Count > 0 ? _items[0] : "No items"),
					new Text(_items.Count > 1 ? _items[1] : ""),
					new Text(_items.Count > 2 ? _items[2] : ""),
				}),
			};
	}

	/// <summary>
	/// Main app entry point - demonstrates basic widget composition
	/// </summary>
	public class FlutterSampleApp : StatelessWidget
	{
		public override Widget Build() =>
			new Center(child: new Column
			{
				new Text("Hello from FlutterSharp!"),
				new SizedBox(height: 20),
				new Text("This is a .NET MAUI to Flutter interop demo"),
				new SizedBox(height: 20),
				new Row
				{
					new Text("Row item 1"),
					new SizedBox(width: 10),
					new Text("Row item 2"),
				},
			});
	}
}
