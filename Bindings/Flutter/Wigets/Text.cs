using System;
namespace Flutter {
	public class Text : Widget{
		public Text(string text = "")
		{
			Value = text;
		}
		public string Value {
			get => GetProperty<string> ();
			set => SetProperty (value);
		}

		public double? ScaleFactor {
			get => GetProperty<double?> ();
			set => SetProperty (value);
		}
	}
}
