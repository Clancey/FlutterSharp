using Flutter.Structs;
using System;
namespace Flutter {
	public class TextField : Widget {
		public TextField(string text = null, string hint = null)
		{
			var s = GetBackingStruct<TextFieldStruct>();
			s.Value = text;
			s.Hint = hint;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new TextFieldStruct();

		public Action<string> OnSubmitted {
			get => GetAction<Action<string>> ();
			set => SetAction (value);
		}

		public Action<string> OnChange {
			get => GetAction<Action<string>> ();
			set => SetAction (value);
		}
	}
}
