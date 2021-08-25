using Flutter.Structs;
using System;
namespace Flutter {
	public class Container : SingleChildRenderObjectWidget {
		public Container(Alignment? alignment = null,
			EdgeInsetsGeometry? padding = null,
			EdgeInsetsGeometry? margin = null,
			Color? color = null,
			double? width = null,
			double? height = null)
		{

			var backingStruct = GetBackingStruct<ContainerStruct>();
			backingStruct.Alignment = alignment;
			backingStruct.Padding = padding;
			backingStruct.Margin = margin;
			backingStruct.Color = color;
			backingStruct.Width = width;
			backingStruct.Height = height;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new ContainerStruct();
	}
}
