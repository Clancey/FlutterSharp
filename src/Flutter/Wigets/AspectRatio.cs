using Flutter.Structs;
using System;
namespace Flutter {
	public class AspectRatio : SingleChildRenderObjectWidget {
		public AspectRatio(double? value = null) => GetBackingStruct<AspectRatioStruct>().Value = value;
		protected override FlutterObjectStruct CreateBackingStruct() => new AspectRatioStruct();
	}
}
