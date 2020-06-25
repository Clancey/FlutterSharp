using System;
namespace Flutter {
	public class Icon : Widget {
		public string CodePoint {
			get => GetProperty<string> ();
			set => SetProperty (value);
		}

		public string FontFamily {
			get => GetProperty<string> ();
			set => SetProperty (value);
		}
	}
}
