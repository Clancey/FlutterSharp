using Flutter;
using Flutter.Widgets;
using Flutter.MAUI;

namespace FlutterSample.MAUI;

/// <summary>
/// Counter demo page showing MAUI-Flutter hybrid interaction.
/// MAUI buttons rebuild a Flutter widget from page state.
/// </summary>
public partial class CounterPage : ContentPage
{
	private int _count;

	public CounterPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshCounterWidget();
	}

	private void OnIncrementClicked(object? sender, EventArgs e)
	{
		_count++;
		RefreshCounterWidget();
	}

	private void OnDecrementClicked(object? sender, EventArgs e)
	{
		_count--;
		RefreshCounterWidget();
	}

	private void OnResetClicked(object? sender, EventArgs e)
	{
		_count = 0;
		RefreshCounterWidget();
	}

	private void RefreshCounterWidget()
	{
		flutterView.Widget = new CounterDisplayWidget(_count);
	}
}

/// <summary>
/// A Flutter widget snapshot for the current counter value.
/// </summary>
public sealed class CounterDisplayWidget : StatelessWidget
{
	private readonly int _count;

	public CounterDisplayWidget(int count)
	{
		_count = count;
	}

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
}
