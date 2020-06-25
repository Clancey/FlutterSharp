using System;
using System.Collections;
using System.Collections.Generic;

namespace Flutter {
	public class TabBar : Widget, IEnumerable {

		public IList<Tab> Tabs {
			get => GetProperty<IList<Tab>> () ?? (Tabs = new List<Tab> ());
			set => SetProperty (value);
		}

		public void Add (Tab child)
		{
			if (child == null)
				return;
			Tabs.Add (child);
		}

		IEnumerator IEnumerable.GetEnumerator () => Tabs.GetEnumerator ();
	}
}
