using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// Counter demo page showing MAUI-Flutter hybrid interaction.
/// MAUI buttons control a Flutter widget's state.
/// </summary>
public partial class CounterPage : ContentPage
{
	private CounterWidget? _counterWidget;

	public CounterPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Create and set the counter widget
		_counterWidget = new CounterWidget();
		flutterView.Widget = _counterWidget;
	}

	private void OnIncrementClicked(object? sender, EventArgs e)
	{
		if (_counterWidget != null)
		{
			_counterWidget.Increment();
		}
	}

	private void OnDecrementClicked(object? sender, EventArgs e)
	{
		if (_counterWidget != null)
		{
			_counterWidget.Decrement();
		}
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		if (_counterWidget != null)
		{
			_counterWidget.Reset();
		}
	}
}

/// <summary>
/// A stateful Flutter widget that displays a counter.
/// State is managed in C# and updated via SetState().
/// </summary>
public class CounterWidget : StatefulWidget
{
	private int _count = 0;

	public override Widget Build() =>
		new Center(child: new Column
		{
			new Text($"Count: {_count}"),
			new SizedBox(height: 20),
			new Text("Use the MAUI buttons below to change the count"),
			new SizedBox(height: 10),
			new Text(_count switch
			{
				0 => "Try pressing + or -",
				> 0 => $"Positive! {_count} is greater than zero",
				< 0 => $"Negative! {_count} is less than zero",
			}),
		});

	public void Increment()
	{
		SetState(() => _count++);
	}

	public void Decrement()
	{
		SetState(() => _count--);
	}

	public void Reset()
	{
		SetState(() => _count = 0);
	}
}
