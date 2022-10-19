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
using MetalPerformanceShadersGraph;
#nullable enable
#if !NET
using NativeHandle = System.IntPtr;
#endif
namespace Flutter.Internal {
	[Protocol (Name = "FlutterMessageCodec", WrapperType = typeof (FlutterMessageCodecWrapper))]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = true, Name = "SharedInstance", Selector = "sharedInstance", ReturnType = typeof (Flutter.Internal.FlutterMessageCodec))]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "Encode", Selector = "encode:", ReturnType = typeof (NSData), ParameterType = new Type [] { typeof (NSObject) }, ParameterByRef = new bool [] { false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "Decode", Selector = "decode:", ReturnType = typeof (NSObject), ParameterType = new Type [] { typeof (NSData) }, ParameterByRef = new bool [] { false })]
	public partial interface IFlutterMessageCodec : INativeObject, IDisposable
	{
	}
	public static partial class FlutterMessageCodec_Extensions {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static NSData? Encode (this IFlutterMessageCodec This, NSObject? message)
		{
			var message__handle__ = message.GetHandle ();
			return  Runtime.GetNSObject<NSData> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("encode:"), message__handle__))!;
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static NSObject? Decode (this IFlutterMessageCodec This, NSData? message)
		{
			var message__handle__ = message.GetHandle ();
			return Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("decode:"), message__handle__))!;
		}
	}
	internal sealed class FlutterMessageCodecWrapper : BaseWrapper, IFlutterMessageCodec {
		[Preserve (Conditional = true)]
		public FlutterMessageCodecWrapper (NativeHandle handle, bool owns)
			: base (handle, owns)
		{
		}
	}
}
namespace Flutter.Internal {
	[Protocol()]
	[Register("FlutterMessageCodec", true)]
	public unsafe partial class FlutterMessageCodec : NSObject, IFlutterMessageCodec {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		static readonly NativeHandle class_ptr = Class.GetHandle ("FlutterMessageCodec");
		public override NativeHandle ClassHandle { get { return class_ptr; } }
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Export ("init")]
		public FlutterMessageCodec () : base (NSObjectFlag.Empty)
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
		protected FlutterMessageCodec (NSObjectFlag t) : base (t)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal FlutterMessageCodec (NativeHandle handle) : base (handle)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[Export ("decode:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual NSObject? Decode (NSData? message)
		{
			var message__handle__ = message.GetHandle ();
			if (IsDirectBinding) {
				return Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("decode:"), message__handle__))!;
			} else {
				return Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("decode:"), message__handle__))!;
			}
		}
		[Export ("encode:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual NSData? Encode (NSObject? message)
		{
			var message__handle__ = message.GetHandle ();
			if (IsDirectBinding) {
				return  Runtime.GetNSObject<NSData> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("encode:"), message__handle__))!;
			} else {
				return  Runtime.GetNSObject<NSData> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("encode:"), message__handle__))!;
			}
		}
		[Export ("sharedInstance")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static FlutterMessageCodec SharedInstance ()
		{
			return  Runtime.GetNSObject<FlutterMessageCodec> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend (class_ptr, Selector.GetHandle ("sharedInstance")))!;
		}
	} /* class FlutterMessageCodec */
}
