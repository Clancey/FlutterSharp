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
			Alignment = alignment;
			Padding = padding;
			Margin = margin;
			Color = color;
			Width = width;
			Height = height;
		}


		public Alignment? Alignment {
			get => GetProperty<Alignment?> ();
			set => SetProperty (value);
		}

		public EdgeInsetsGeometry? Padding {
			get => GetProperty<EdgeInsetsGeometry?> ();
			set => SetProperty (value);
		}

		public EdgeInsetsGeometry? Margin {
			get => GetProperty<EdgeInsetsGeometry?> ();
			set => SetProperty (value);
		}

		public Color? Color {
			get => GetProperty<Color?> ();
			set => SetProperty (value);
		}


		public double? Width {
			get => GetProperty<double?> ();
			set => SetProperty (value);
		}

		public double? Height {
			get => GetProperty<double?> ();
			set => SetProperty (value);
		}

	}
}
