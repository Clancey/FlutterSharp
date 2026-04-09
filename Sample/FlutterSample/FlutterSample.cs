using System;
using System.Collections.Generic;
using Flutter;
using Flutter.UI;
using Flutter.Widgets;
using Material = Flutter.Material;

namespace FlutterSample
{
	public class FlutterSampleApp : StatefulWidget
	{
		private int _actionCount;
		private string _lastAction = "Waiting for input";
		private readonly List<string> _items = new List<string>
		{
			"Pin a Dart parser fix",
			"Regenerate C# bindings",
			"Verify the iPhone simulator sample",
			"Close the warning backlog"
		};
		private bool _notificationsEnabled = true;
		private bool _diagnosticsEnabled;
		private double _priority = 40.0;

		private void RecordAction(string actionName)
		{
			SetState(() =>
			{
				_actionCount++;
				_lastAction = actionName;
			});
		}

		private void AddItem()
		{
			SetState(() => _items.Add($"Generated task {_items.Count + 1}"));
		}

		private void RemoveLast()
		{
			SetState(() =>
			{
				if (_items.Count > 0)
				{
					_items.RemoveAt(_items.Count - 1);
				}
			});
		}

		private void Promote(int index)
		{
			SetState(() =>
			{
				if (index <= 0 || index >= _items.Count)
				{
					return;
				}

				var item = _items[index];
				_items.RemoveAt(index);
				_items.Insert(0, item);
			});
		}

		private Widget BuildOverviewPage() =>
			new Column(crossAxisAlignment: CrossAxisAlignment.Start)
			{
				new Text("FlutterSharp renders a live Flutter widget tree from C#."),
				new SizedBox(height: 8),
				new Material.TextButton(
					new Text("Record overview action"),
					onPressed: () => RecordAction("Overview action")
				),
				new SizedBox(height: 8),
				new Text($"Actions triggered: {_actionCount}"),
				new Text($"Last action: {_lastAction}"),
				new SizedBox(height: 8),
				new Material.Card(child: new Column
				{
					new ListTile(
						title: new Text("Generated widgets"),
						subtitle: new Text("Tabs, cards, controls, and callbacks are live.")
					)
				})
			};

		private Widget BuildTaskPage()
		{
			var column = new Column(crossAxisAlignment: CrossAxisAlignment.Start);

			column.Add(new Text("Interactive task list"));
			column.Add(new SizedBox(height: 8));
			column.Add(new Material.TextButton(
				new Text("Add task"),
				onPressed: AddItem
			));
			column.Add(new Material.TextButton(
				new Text("Remove last task"),
				onPressed: RemoveLast
			));
			column.Add(new SizedBox(height: 8));
			column.Add(new Text($"Items in list: {_items.Count}"));
			column.Add(new Text("Tap a card to pin it to the top."));
			column.Add(new SizedBox(height: 8));

			var visibleCount = Math.Min(_items.Count, 2);
			for (var index = 0; index < visibleCount; index++)
			{
				var capturedIndex = index;
				column.Add(new Material.Card(
					child: new ListTile(
						title: new Text(_items[capturedIndex]),
						subtitle: new Text(capturedIndex == 0
							? "Pinned showcase item"
							: $"Tap to move \"{_items[capturedIndex]}\" to the top."),
						trailing: new Text($"#{capturedIndex + 1}"),
						onTap: () => Promote(capturedIndex)
					)
				));
				column.Add(new SizedBox(height: 8));
			}

			if (_items.Count > visibleCount)
			{
				column.Add(new Text($"+{_items.Count - visibleCount} more task(s) queued"));
			}

			return column;
		}

		private Widget BuildControlsPage() =>
			new Column(crossAxisAlignment: CrossAxisAlignment.Start)
			{
				new Material.Card(child: new Column
				{
					new ListTile(
						title: new Text("Enable notifications"),
						subtitle: new Text("Toggle a generated Switch widget."),
						trailing: new Switch(
							_notificationsEnabled,
							value => SetState(() => _notificationsEnabled = value)
						)
					),
					new ListTile(
						title: new Text("Show diagnostics"),
						subtitle: new Text("Toggle a generated Checkbox widget."),
						trailing: new Checkbox(
							_diagnosticsEnabled,
							value => SetState(() => _diagnosticsEnabled = value ?? false)
						)
					)
				}),
				new SizedBox(height: 12),
				new Text($"Priority: {Math.Round(_priority)}%"),
				new Slider(
					_priority,
					value => SetState(() => _priority = value),
					min: 0.0,
					max: 100.0,
					divisions: 10,
					label: $"{Math.Round(_priority)}"
				),
				new SizedBox(height: 12),
				new Text(
					$"Notifications: {(_notificationsEnabled ? "On" : "Off")} • Diagnostics: {(_diagnosticsEnabled ? "Visible" : "Hidden")}"
				)
			};

		public override Widget Build() =>
			new Material.MaterialApp(
				title: "FlutterSharp Demo",
				theme: new Material.ThemeData
				{
					ScaffoldBackgroundColor = 0xFFF6F1F8,
					AppBarBackgroundColor = 0xFFF6F1F8,
					CardColor = 0xFFFFFFFF
				},
				home: new Material.DefaultTabController(
					length: 3,
					child: new Material.Scaffold(
						appBar: new Material.AppBar(
							title: new Text("FlutterSharp Demo"),
							bottom: new Material.TabBar
							{
								new Material.Tab(text: "Overview"),
								new Material.Tab(text: "Tasks"),
								new Material.Tab(text: "Controls")
							}
						),
						drawer: new Material.Drawer(
							child: new Column
							{
								new Text("FlutterSharp"),
								new SizedBox(height: 12),
								new ListTile(
									title: new Text("Generator-backed widgets"),
									subtitle: new Text("The sample UI is built from generated C# bindings over Flutter widgets.")
								),
								new ListTile(
									title: new Text("Live callbacks"),
									subtitle: new Text("Button taps, list interactions, and toggles all update C# state.")
								),
								new ListTile(
									title: new Text("Reproducible iOS pipeline"),
									subtitle: new Text("The sample is using the scripted flutter_module + plugin framework build.")
								)
							}
						),
						body: new Material.TabBarView
						{
							BuildOverviewPage(),
							BuildTaskPage(),
							BuildControlsPage()
						}
					)
				)
			);
	}
}
