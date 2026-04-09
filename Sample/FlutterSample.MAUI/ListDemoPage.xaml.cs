using System.Collections.Generic;
using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// List demo page showing Flutter list rendering with MAUI input controls.
/// Demonstrates dynamic list updates by rebuilding Flutter content from MAUI state.
/// </summary>
public partial class ListDemoPage : ContentPage
{
	private readonly List<string> _items = new()
	{
		"Welcome to FlutterSharp!",
		"This is a dynamic list",
		"Add items using the entry below"
	};

	public ListDemoPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshListWidget();
	}

	private void OnAddItemClicked(object? sender, EventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(itemEntry.Text))
		{
			_items.Add(itemEntry.Text.Trim());
			itemEntry.Text = string.Empty;
			RefreshListWidget();
		}
	}

	private void OnClearClicked(object? sender, EventArgs e)
	{
		_items.Clear();
		RefreshListWidget();
	}

	private void RefreshListWidget()
	{
		flutterView.Widget = new ListDisplayWidget(_items);
	}
}

/// <summary>
/// A Flutter widget snapshot for the current list contents.
/// </summary>
public sealed class ListDisplayWidget : StatelessWidget
{
	private readonly List<string> _items;

	public ListDisplayWidget(IEnumerable<string> items)
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
				new Text("Use the entry field below to add items"),
			});
		}

		// Build a Column with all list items
		var column = new Column();

		// Add a title
		column.Add(new Text($"Items ({_items.Count})"));
		column.Add(new SizedBox(height: 10));

		// Add each item
		for (int i = 0; i < _items.Count; i++)
		{
			column.Add(new Row
			{
				new Text($"{i + 1}. "),
				new Expanded(child: new Text(_items[i])),
			});
			column.Add(new SizedBox(height: 8));
		}

		return new Center(child: column);
	}
}
