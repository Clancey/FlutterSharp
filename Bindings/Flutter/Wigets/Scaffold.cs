using System;
namespace Flutter {
	public class Scaffold : Widget {

		public AppBar AppBar {
			get => GetProperty<AppBar> ();
			set => SetProperty (value);
		}
		public FloatingActionButton FloatingActionButton {
			get => GetProperty<FloatingActionButton> ();
			set => SetProperty (value);
		}
		public Drawer Drawer {
			get => GetProperty<Drawer> ();
			set => SetProperty (value);
		}
		public Widget Body {
			get => GetProperty<Widget> ();
			set => SetProperty (value);
		}
	}
}
