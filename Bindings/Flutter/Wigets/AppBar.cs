using System;
using Flutter.Structs;
using Newtonsoft.Json;

namespace Flutter {

	public class AppBar : Widget {
		protected override FlutterObjectStruct CreateBackingStruct() => new AppBarStruct();
		Widget title;
		Widget bottom;
		public AppBar(Widget title = null, Widget bottom = null)
		{
			var backingStruct = GetBackingStruct<AppBarStruct>();
			backingStruct.Title = this.title =  title;
			backingStruct.Bottom = this.bottom = bottom;
		}
	}
}
