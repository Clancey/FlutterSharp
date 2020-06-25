using System;
namespace Flutter {
	public class TextField : Widget {

		public string Hint {
			get => GetProperty<string> ();
			set => SetProperty (value);
		}

		public string Text {
			get => GetProperty<string> ();
			set => SetProperty (value);
		}
		public Action<string> OnInput {
			get => GetProperty<Action<string>> ();
			set => SetProperty (value);
		}

		public Action<string> OnChange {
			get => GetProperty<Action<string>> ();
			set => SetProperty (value);
		}
	}
}
