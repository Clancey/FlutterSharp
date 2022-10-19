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
	[Protocol (Name = "FlutterPlatformViewFactory", WrapperType = typeof (FlutterPlatformViewFactoryWrapper))]
	[ProtocolMember (IsRequired = true, IsProperty = false, IsStatic = false, Name = "ViewIdentifier", Selector = "createWithFrame:viewIdentifier:arguments:", ReturnType = typeof (Flutter.Internal.FlutterPlatformView), ParameterType = new Type [] { typeof (CGRect), typeof (long), typeof (NSObject) }, ParameterByRef = new bool [] { false, false, false })]
	[ProtocolMember (IsRequired = false, IsProperty = true, IsStatic = false, Name = "CreateArgsCodec", Selector = "createArgsCodec", PropertyType = typeof (Flutter.Internal.FlutterMessageCodec), GetterSelector = "createArgsCodec", ArgumentSemantic = ArgumentSemantic.None)]
	public partial interface IFlutterPlatformViewFactory : INativeObject, IDisposable
	{
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[Export ("createWithFrame:viewIdentifier:arguments:")]
		[Preserve (Conditional = true)]
		FlutterPlatformView ViewIdentifier (CGRect frame, long viewId, NSObject? args);
	}
	public static partial class FlutterPlatformViewFactory_Extensions {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static FlutterMessageCodec GetCreateArgsCodec (this IFlutterPlatformViewFactory This)
		{
			return  Runtime.GetNSObject<FlutterMessageCodec> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend (This.Handle, Selector.GetHandle ("createArgsCodec")))!;
		}
	}
	internal sealed class FlutterPlatformViewFactoryWrapper : BaseWrapper, IFlutterPlatformViewFactory {
		[Preserve (Conditional = true)]
		public FlutterPlatformViewFactoryWrapper (NativeHandle handle, bool owns)
			: base (handle, owns)
		{
		}
		[Export ("createWithFrame:viewIdentifier:arguments:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public FlutterPlatformView ViewIdentifier (CGRect frame, long viewId, NSObject? args)
		{
			var args__handle__ = args.GetHandle ();
			return  Runtime.GetNSObject<FlutterPlatformView> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_CGRect_Int64_NativeHandle (this.Handle, Selector.GetHandle ("createWithFrame:viewIdentifier:arguments:"), frame, viewId, args__handle__))!;
		}
	}
}
