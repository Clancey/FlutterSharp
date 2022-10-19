using System;
using UIKit;
using CoreGraphics;
namespace Flutter;

public partial class PlatformView
{
	public PlatformView(Func<CGRect,UIView> createView)
	{
		this.CreateView = createView;
	}
	public Func<CGRect,UIView> CreateView { get; private set; }
}

