using System.Collections.Generic;
using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// Single-page MAUI host for the FlutterSharp sample.
/// Keeps one FlutterView alive and swaps the displayed Flutter widget snapshot from MAUI state.
/// </summary>
public partial class MainPage : ContentPage
{
	private DemoMode _mode = DemoMode.Overview;
	private int _count;
	private readonly List<string> _items = new()
	{
		"Welcome to FlutterSharp!",
		"This list is driven by MAUI state.",
		"Add or clear items below."
	};

	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshDemo();
	}

	private void OnOverviewClicked(object? sender, EventArgs e)
	{
		_mode = DemoMode.Overview;
		RefreshDemo();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		_mode = DemoMode.Counter;
		RefreshDemo();
	}

	private void OnListClicked(object? sender, EventArgs e)
	{
		_mode = DemoMode.List;
		RefreshDemo();
	}

	private void OnIncrementClicked(object? sender, EventArgs e)
	{
		_count++;
		RefreshDemo();
	}

	private void OnDecrementClicked(object? sender, EventArgs e)
	{
		_count--;
		RefreshDemo();
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		_count = 0;
		RefreshDemo();
	}

	private void OnAddItemClicked(object? sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(itemEntry.Text))
		{
			return;
		}

		_items.Add(itemEntry.Text.Trim());
		itemEntry.Text = string.Empty;
		RefreshDemo();
	}

	private void OnClearClicked(object? sender, EventArgs e)
	{
		_items.Clear();
		RefreshDemo();
	}

	private void RefreshDemo()
	{
		counterControls.IsVisible = _mode == DemoMode.Counter;
		listControls.IsVisible = _mode == DemoMode.List;

		modeDescriptionLabel.Text = _mode switch
		{
			DemoMode.Overview => "Overview mode shows a simple Flutter layout hosted inside MAUI.",
			DemoMode.Counter => $"Counter mode rebuilds Flutter from MAUI state. Current value: {_count}.",
			DemoMode.List => $"List mode rebuilds Flutter from MAUI state. Items: {_items.Count}.",
			_ => string.Empty
		};

		UpdateModeButtons();
		flutterView.Widget = BuildCurrentWidget();
	}

	private void UpdateModeButtons()
	{
		SetButtonState(overviewButton, _mode == DemoMode.Overview);
		SetButtonState(counterButton, _mode == DemoMode.Counter);
		SetButtonState(listButton, _mode == DemoMode.List);
	}

	private static void SetButtonState(Button button, bool isActive)
	{
		button.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
		button.Opacity = isActive ? 1.0 : 0.7;
	}

	private Widget BuildCurrentWidget() => _mode switch
	{
		DemoMode.Overview => new OverviewWidget(),
		DemoMode.Counter => new MainCounterDisplayWidget(_count),
		DemoMode.List => new MainListDisplayWidget(_items),
		_ => new OverviewWidget()
	};

	private enum DemoMode
	{
		Overview,
		Counter,
		List
	}
}

public sealed class OverviewWidget : StatelessWidget
{
	public override Widget Build() =>
		new Center(child: new Column
		{
			new Text("Hello from FlutterSharp!"),
			new SizedBox(height: 20),
			new Text("This MAUI sample keeps one FlutterView alive."),
			new SizedBox(height: 20),
			new Text("Use the buttons above to swap Flutter content."),
			new SizedBox(height: 30),
			new Row
			{
				new Text("Overview"),
				new SizedBox(width: 10),
				new Text("Counter"),
				new SizedBox(width: 10),
				new Text("List"),
			},
		});
}

public sealed class MainCounterDisplayWidget : StatelessWidget
{
	private readonly int _count;

	public MainCounterDisplayWidget(int count)
	{
		_count = count;
	}

	public override Widget Build() =>
		new Center(child: new Column
		{
			new Text($"Count: {_count}"),
			new SizedBox(height: 20),
			new Text("Use the MAUI controls below to change the count."),
			new SizedBox(height: 10),
			new Text(_count switch
			{
				0 => "Try pressing + or -",
				> 0 => $"Positive! {_count} is greater than zero",
				< 0 => $"Negative! {_count} is less than zero",
			}),
		});
}

public sealed class MainListDisplayWidget : StatelessWidget
{
	private readonly List<string> _items;

	public MainListDisplayWidget(IEnumerable<string> items)
	{
		_items = new List<string>(items);
	}

	public override Widget Build()
	{
		if (_items.Count == 0)
		{
			return new Center(child: new Column
			{
				new Text("No items yet"),
				new SizedBox(height: 10),
				new Text("Use the MAUI entry field below to add items."),
			});
		}

		var column = new Column();
		column.Add(new Text($"Items ({_items.Count})"));
		column.Add(new SizedBox(height: 10));

		for (var index = 0; index < _items.Count; index++)
		{
			column.Add(new Row
			{
				new Text($"{index + 1}. "),
				new Expanded(child: new Text(_items[index])),
			});
			column.Add(new SizedBox(height: 8));
		}

		return new Center(child: column);
	}
}
