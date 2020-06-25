using System;
using Newtonsoft.Json;

namespace Flutter {
	public class AppBar : Widget {
		public AppBar(Widget title = null, Widget bottom = null)
		{
			Title = title;
			bottom = bottom;
		}
		public Widget Title {
			get => GetProperty<Widget>();
			set => SetProperty (value);
		}

		public Widget Bottom {
			get => GetProperty<Widget> ();
			set => SetProperty (value);
		}
	}
}
