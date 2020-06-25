using System;
using Flutter.Internal;
using Newtonsoft.Json;

namespace Flutter {
	public abstract class StatefulWidget : Widget {
		public Widget Child {
			get => GetProperty<Widget> ();
			private set => SetProperty (value);
		}
		public abstract Widget Build ();
		internal override void BeforeJSon ()
		{
			if (Child == null)
				Child = Build ();
		}

		public void SetState(Action setState)
		{
			setState ();
			Child = Build ();
			FlutterManager.SendState (this.Child,this.Id);
		}
		protected override string type => "StatefulWidget";
	}
}
