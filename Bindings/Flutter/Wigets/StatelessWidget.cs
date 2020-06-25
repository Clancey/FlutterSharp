using System;
namespace Flutter {
	public abstract class StatelessWidget : Widget {
		Widget Child {
			get => GetProperty<Widget> ();
			set => SetProperty (value);
		}
		public abstract Widget Build ();
		internal override void BeforeJSon ()
		{
			if (Child == null)
				Child = Build ();
		}

		protected override string type => "StatefulWidget";
	}
}
