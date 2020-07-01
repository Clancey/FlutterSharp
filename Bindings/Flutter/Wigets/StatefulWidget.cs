using System;
using Flutter.Internal;
using Newtonsoft.Json;

namespace Flutter {
	public abstract class StatefulWidget : SingleChildRenderObjectWidget {
		
		public abstract Widget Build ();
		public override void PrepareForSending()
		{
			base.PrepareForSending();
			if (Child == null)
				Child = Build ();
			Child?.PrepareForSending ();
		}

		public void SetState(Action setState)
		{
			setState ();
			Child = Build ();
			FlutterManager.SendState (this.Child,this.Id);
		}
		protected override string FlutterType => "StatefulWidget";
	}
}
