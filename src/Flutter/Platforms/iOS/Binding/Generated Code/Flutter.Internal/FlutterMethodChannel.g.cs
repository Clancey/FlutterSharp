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
namespace Flutter.Internal {
	[Register("FlutterMethodChannel", true)]
	public unsafe partial class FlutterMethodChannel : NSObject {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		static readonly NativeHandle class_ptr = Class.GetHandle ("FlutterMethodChannel");
		public override NativeHandle ClassHandle { get { return class_ptr; } }
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Export ("init")]
		public FlutterMethodChannel () : base (NSObjectFlag.Empty)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
			if (IsDirectBinding) {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSend (this.Handle, global::ObjCRuntime.Selector.GetHandle ("init")), "init");
			} else {
				InitializeHandle (global::ApiDefinition.Messaging.IntPtr_objc_msgSendSuper (this.SuperHandle, global::ObjCRuntime.Selector.GetHandle ("init")), "init");
			}
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected FlutterMethodChannel (NSObjectFlag t) : base (t)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal FlutterMethodChannel (IntPtr handle) : base (handle)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[Export ("methodChannelWithName:binaryMessenger:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static FlutterMethodChannel FromNameAndMessenger (string channelName, FlutterBinaryMessenger binaryMessenger)
		{
			if (channelName is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (channelName));
			var binaryMessenger__handle__ = binaryMessenger.GetNonNullHandle (nameof (binaryMessenger));
			var nschannelName = CFString.CreateNative (channelName);
			FlutterMethodChannel ret;
			ret =  Runtime.GetNSObject<FlutterMethodChannel> (global::ApiDefinition.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr (class_ptr, Selector.GetHandle ("methodChannelWithName:binaryMessenger:"), nschannelName, binaryMessenger__handle__));
			CFString.ReleaseNative (nschannelName);
			return ret!;
		}
		[Export ("invokeMethod:arguments:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual void InvokeMethod (string method, NSObject? arguments)
		{
			if (method is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (method));
			var arguments__handle__ = arguments.GetHandle ();
			var nsmethod = CFString.CreateNative (method);
			if (IsDirectBinding) {
				global::ApiDefinition.Messaging.void_objc_msgSend_IntPtr_IntPtr (this.Handle, Selector.GetHandle ("invokeMethod:arguments:"), nsmethod, arguments__handle__);
			} else {
				global::ApiDefinition.Messaging.void_objc_msgSendSuper_IntPtr_IntPtr (this.SuperHandle, Selector.GetHandle ("invokeMethod:arguments:"), nsmethod, arguments__handle__);
			}
			CFString.ReleaseNative (nsmethod);
		}
		[Export ("setMethodCallHandler:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public unsafe virtual void SetMethodCaller ([BlockProxy (typeof (ObjCRuntime.Trampolines.NIDFlutterMethodCallHandler))]FlutterMethodCallHandler? handler)
		{
			BlockLiteral *block_ptr_handler;
			BlockLiteral block_handler;
			if (handler is null){
				block_ptr_handler = null;
			} else {
				block_handler = new BlockLiteral ();
				block_ptr_handler = &block_handler;
				block_handler.SetupBlockUnsafe (Trampolines.SDFlutterMethodCallHandler.Handler, handler);
			}
			if (IsDirectBinding) {
				global::ApiDefinition.Messaging.void_objc_msgSend_IntPtr (this.Handle, Selector.GetHandle ("setMethodCallHandler:"), (IntPtr) block_ptr_handler);
			} else {
				global::ApiDefinition.Messaging.void_objc_msgSendSuper_IntPtr (this.SuperHandle, Selector.GetHandle ("setMethodCallHandler:"), (IntPtr) block_ptr_handler);
			}
			if (block_ptr_handler != null)
				block_ptr_handler->CleanupBlock ();
		}
	} /* class FlutterMethodChannel */
}
