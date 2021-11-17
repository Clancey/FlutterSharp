using Flutter.Structs;
using System;
namespace Flutter
{
	public class Text : Widget
	{
		public Text(string text = "", double? scaleFactor = null)
		{
			var s = GetBackingStruct<TextStruct>();
			s.Value = text;
			s.ScaleFactor = scaleFactor;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new TextStruct();
	}
}
