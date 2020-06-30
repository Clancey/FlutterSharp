using System;
using Flutter.Structs;
using Newtonsoft.Json;

namespace Flutter {

	public class AppBar : Widget {
		protected override FlutterObjectStruct CreateBackingStruct() => new AppBarStruct();
		public AppBar(Widget title = null, Widget bottom = null)
		{
			var backingStruct = GetBackingStruct<AppBarStruct>();
			backingStruct.Title = title;
			backingStruct.Bottom = bottom;
		}
	}
}
