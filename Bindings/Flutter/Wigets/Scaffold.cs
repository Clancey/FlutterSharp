using Flutter.Structs;
using System;
namespace Flutter {
	public class Scaffold : Widget {

		AppBar appbar;
		FloatingActionButton floatingActionButton;
		Drawer drawer;
		Widget body;
		public Scaffold(AppBar appbar = null, FloatingActionButton floatingActionButton = null, Drawer drawer = null, Widget body = null)
		{
			var s = GetBackingStruct<ScaffoldStruct>();
			s.AppBar = this.appbar =appbar;
			s.FloatingActionButton = this.floatingActionButton = floatingActionButton;
			s.Drawer = this.drawer = drawer;
			s.Body = this.body = body;
		}

		protected override FlutterObjectStruct CreateBackingStruct() => new ScaffoldStruct();
	}
}
