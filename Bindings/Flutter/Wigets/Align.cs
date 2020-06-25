using System;
namespace Flutter {
	public class Align : SingleChildRenderObjectWidget {
		public Alignment? Alignment {
			get => GetProperty<Alignment?> ();
			set => SetProperty (value);
		}

		public double? WidthFactor {
			get => GetProperty<double?> ();
			set => SetProperty (value);
		}

		public double? HeightFactor {
			get => GetProperty<double?> ();
			set => SetProperty (value);
		}

	}
}
