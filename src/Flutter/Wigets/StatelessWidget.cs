using System;
namespace Flutter
{
	public abstract class StatelessWidget : SingleChildRenderObjectWidget, IBuildableWidget
	{
		public abstract Widget Build();

		public override void PrepareForSending()
		{
			base.PrepareForSending();
			if (Child == null)
			{
				Child = Build();
			}
			Child?.PrepareForSending();
		}

		protected override string FlutterType => "StatefulWidget";
	}
}
