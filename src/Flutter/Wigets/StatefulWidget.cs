using System;
using Flutter.HotReload;
using Flutter.Internal;
using Newtonsoft.Json;

namespace Flutter {
	public abstract class StatefulWidget : SingleChildRenderObjectWidget, IBuildableWidget
	{
		
		public abstract Widget Build ();
		
		public void SetState(Action setState)
		{
			setState ();
			Child?.Dispose ();
			Child = null;
			PrepareForSending();
			FlutterManager.SendState (this.Child,this.Id);
		}

		protected override string FlutterType => "StatefulWidget";
	}
}
