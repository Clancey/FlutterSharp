using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// List demo page showing Flutter list rendering with MAUI input controls.
/// Demonstrates dynamic list updates from MAUI to Flutter.
/// </summary>
public partial class ListDemoPage : ContentPage
{
	private ListWidget? _listWidget;

	public ListDemoPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Create and set the list widget
		_listWidget = new ListWidget();
		flutterView.Widget = _listWidget;
	}

	private void OnAddItemClicked(object? sender, EventArgs e)
	{
		if (_listWidget != null && !string.IsNullOrWhiteSpace(itemEntry.Text))
		{
			_listWidget.AddItem(itemEntry.Text);
			itemEntry.Text = string.Empty;
		}
	}

	private void OnClearClicked(object? sender, EventArgs e)
	{
		_listWidget?.ClearItems();
	}
}

/// <summary>
/// A stateful Flutter widget that displays a dynamic list.
/// Items can be added or cleared via public methods.
/// </summary>
public class ListWidget : StatefulWidget
{
	private List<string> _items = new()
	{
		"Welcome to FlutterSharp!",
		"This is a dynamic list",
		"Add items using the entry below"
	};

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

	public void AddItem(string item)
	{
		SetState(() => _items.Add(item));
	}

	public void ClearItems()
	{
		SetState(() => _items.Clear());
	}
}
