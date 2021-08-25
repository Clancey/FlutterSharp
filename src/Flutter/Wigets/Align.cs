using Flutter.Structs;
using System;
namespace Flutter {
	
	public class Align : SingleChildRenderObjectWidget {
		public Align(Alignment? alignment = null, double? widthFactor = null, double? heightFactor = null)
		{
			var backingStruct = GetBackingStruct<AlignStruct>();
			backingStruct.Alignment = alignment;
			backingStruct.WidthFactor = widthFactor;
			backingStruct.HeightFactor = heightFactor;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new AlignStruct();
	}
}
