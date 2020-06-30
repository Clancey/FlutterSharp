using Flutter.Structs;
using System;
namespace Flutter {
	public class Scaffold : Widget {

		public Scaffold(AppBar appbar = null, FloatingActionButton floatingActionButton = null, Drawer drawer = null, Widget body = null)
		{
			var s = GetBackingStruct<ScaffoldStruct>();
			s.AppBar = appbar;
			s.FloatingActionButton = floatingActionButton;
			s.Drawer = drawer;
			s.Body = body;
		}

		protected override FlutterObjectStruct CreateBackingStruct() => new ScaffoldStruct();
	}
}
