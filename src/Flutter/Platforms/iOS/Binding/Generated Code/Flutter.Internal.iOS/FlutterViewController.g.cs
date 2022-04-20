//
// Auto-generated from generator.cs, do not edit
//
// We keep references to objects, so warning 414 is expected
#pragma warning disable 414
using System;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using UIKit;
using GLKit;
using Metal;
using CoreML;
using MapKit;
using Photos;
using ModelIO;
using Network;
using SceneKit;
using Contacts;
using Security;
using Messages;
using AudioUnit;
using CoreVideo;
using CoreMedia;
using QuickLook;
using CoreImage;
using SpriteKit;
using Foundation;
using CoreMotion;
using ObjCRuntime;
using AddressBook;
using MediaPlayer;
using GameplayKit;
using CoreGraphics;
using CoreLocation;
using AVFoundation;
using NewsstandKit;
using FileProvider;
using CoreAnimation;
using CoreFoundation;
using NetworkExtension;
#nullable enable
namespace Flutter.Internal.iOS {
	[Register("FlutterViewController", true)]
	public unsafe partial class FlutterViewController : global::UIKit.UIViewController {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		static readonly NativeHandle class_ptr = Class.GetHandle ("FlutterViewController");
		public override NativeHandle ClassHandle { get { return class_ptr; } }
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[DesignatedInitializer]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Export ("initWithCoder:")]
		public FlutterViewController (NSCoder coder) : base (NSObjectFlag.Empty)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
			if (IsDirectBinding) {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSend_IntPtr (this.Handle, Selector.GetHandle ("initWithCoder:"), coder.Handle), "initWithCoder:");
			} else {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSendSuper_IntPtr (this.SuperHandle, Selector.GetHandle ("initWithCoder:"), coder.Handle), "initWithCoder:");
			}
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected FlutterViewController (NSObjectFlag t) : base (t)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal FlutterViewController (IntPtr handle) : base (handle)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[Export ("initWithEngine:nibName:bundle:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public FlutterViewController (global::Flutter.Internal.FlutterEngine engine, string? nibName, NSBundle? bundle)
			: base (NSObjectFlag.Empty)
		{
			var engine__handle__ = engine.GetNonNullHandle (nameof (engine));
			var bundle__handle__ = bundle.GetHandle ();
			var nsnibName = CFString.CreateNative (nibName);
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
			if (IsDirectBinding) {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr (this.Handle, Selector.GetHandle ("initWithEngine:nibName:bundle:"), engine__handle__, nsnibName, bundle__handle__), "initWithEngine:nibName:bundle:");
			} else {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSendSuper_IntPtr_IntPtr_IntPtr (this.SuperHandle, Selector.GetHandle ("initWithEngine:nibName:bundle:"), engine__handle__, nsnibName, bundle__handle__), "initWithEngine:nibName:bundle:");
			}
			CFString.ReleaseNative (nsnibName);
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual global::Flutter.Internal.FlutterEngine Engine {
			[Export ("engine")]
			get {
				global::Flutter.Internal.FlutterEngine ret;
				if (IsDirectBinding) {
					ret =  Runtime.GetNSObject<global::Flutter.Internal.FlutterEngine> (global::ApiDefinition.Messaging.IntPtr_objc_msgSend (this.Handle, Selector.GetHandle ("engine")));
				} else {
					ret =  Runtime.GetNSObject<global::Flutter.Internal.FlutterEngine> (global::ApiDefinition.Messaging.IntPtr_objc_msgSendSuper (this.SuperHandle, Selector.GetHandle ("engine")));
				}
				return ret!;
			}
		}
	} /* class FlutterViewController */
}
