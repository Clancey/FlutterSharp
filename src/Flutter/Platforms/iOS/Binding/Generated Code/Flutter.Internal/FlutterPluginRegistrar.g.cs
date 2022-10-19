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
	[Protocol (Name = "FlutterPluginRegistrar", WrapperType = typeof (FlutterPluginRegistrarWrapper))]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "RegisterViewFactory", Selector = "registerViewFactory:withId:", ParameterType = new Type [] { typeof (NSObject), typeof (string) }, ParameterByRef = new bool [] { false, false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "Publish", Selector = "publish:", ParameterType = new Type [] { typeof (NSObject) }, ParameterByRef = new bool [] { false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "LookupKeyForAsset", Selector = "lookupKeyForAsset:", ReturnType = typeof (string), ParameterType = new Type [] { typeof (string) }, ParameterByRef = new bool [] { false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "LookupKeyForAsset", Selector = "lookupKeyForAsset:fromPackage:", ReturnType = typeof (string), ParameterType = new Type [] { typeof (string), typeof (string) }, ParameterByRef = new bool [] { false, false })]
	[ProtocolMember (IsRequired = false, IsProperty = true, IsStatic = false, Name = "Messenger", Selector = "messenger", PropertyType = typeof (Flutter.Internal.FlutterBinaryMessenger), GetterSelector = "messenger", ArgumentSemantic = ArgumentSemantic.None)]
	public partial interface IFlutterPluginRegistrar : INativeObject, IDisposable
	{
	}
	public static partial class FlutterPluginRegistrar_Extensions {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static void RegisterViewFactory (this IFlutterPluginRegistrar This, NSObject factory, string factoryId)
		{
			var factory__handle__ = factory!.GetNonNullHandle (nameof (factory));
			if (factoryId is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (factoryId));
			var nsfactoryId = CFString.CreateNative (factoryId);
			global::ApiDefinition.Messaging.void_objc_msgSend_NativeHandle_NativeHandle (This.Handle, Selector.GetHandle ("registerViewFactory:withId:"), factory__handle__, nsfactoryId);
			CFString.ReleaseNative (nsfactoryId);
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static void Publish (this IFlutterPluginRegistrar This, NSObject value)
		{
			var value__handle__ = value!.GetNonNullHandle (nameof (value));
			global::ApiDefinition.Messaging.void_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("publish:"), value__handle__);
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static string LookupKeyForAsset (this IFlutterPluginRegistrar This, string asset)
		{
			if (asset is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (asset));
			var nsasset = CFString.CreateNative (asset);
			string? ret;
			ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("lookupKeyForAsset:"), nsasset))!;
			CFString.ReleaseNative (nsasset);
			return ret!;
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static string LookupKeyForAsset (this IFlutterPluginRegistrar This, string asset, string package)
		{
			if (asset is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (asset));
			if (package is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (package));
			var nsasset = CFString.CreateNative (asset);
			var nspackage = CFString.CreateNative (package);
			string? ret;
			ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle_NativeHandle (This.Handle, Selector.GetHandle ("lookupKeyForAsset:fromPackage:"), nsasset, nspackage))!;
			CFString.ReleaseNative (nsasset);
			CFString.ReleaseNative (nspackage);
			return ret!;
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static FlutterBinaryMessenger GetMessenger (this IFlutterPluginRegistrar This)
		{
			return  Runtime.GetNSObject<FlutterBinaryMessenger> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend (This.Handle, Selector.GetHandle ("messenger")))!;
		}
	}
	internal sealed class FlutterPluginRegistrarWrapper : BaseWrapper, IFlutterPluginRegistrar {
		[Preserve (Conditional = true)]
		public FlutterPluginRegistrarWrapper (NativeHandle handle, bool owns)
			: base (handle, owns)
		{
		}
	}
}
namespace Flutter.Internal {
	[Protocol()]
	[Register("FlutterPluginRegistrar", true)]
	public unsafe partial class FlutterPluginRegistrar : NSObject, IFlutterPluginRegistrar {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		static readonly NativeHandle class_ptr = Class.GetHandle ("FlutterPluginRegistrar");
		public override NativeHandle ClassHandle { get { return class_ptr; } }
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Export ("init")]
		public FlutterPluginRegistrar () : base (NSObjectFlag.Empty)
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
		protected FlutterPluginRegistrar (NSObjectFlag t) : base (t)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal FlutterPluginRegistrar (NativeHandle handle) : base (handle)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[Export ("lookupKeyForAsset:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual string LookupKeyForAsset (string asset)
		{
			if (asset is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (asset));
			var nsasset = CFString.CreateNative (asset);
			string? ret;
			if (IsDirectBinding) {
				ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("lookupKeyForAsset:"), nsasset))!;
			} else {
				ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("lookupKeyForAsset:"), nsasset))!;
			}
			CFString.ReleaseNative (nsasset);
			return ret!;
		}
		[Export ("lookupKeyForAsset:fromPackage:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual string LookupKeyForAsset (string asset, string package)
		{
			if (asset is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (asset));
			if (package is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (package));
			var nsasset = CFString.CreateNative (asset);
			var nspackage = CFString.CreateNative (package);
			string? ret;
			if (IsDirectBinding) {
				ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle_NativeHandle (this.Handle, Selector.GetHandle ("lookupKeyForAsset:fromPackage:"), nsasset, nspackage))!;
			} else {
				ret = CFString.FromHandle (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle_NativeHandle (this.SuperHandle, Selector.GetHandle ("lookupKeyForAsset:fromPackage:"), nsasset, nspackage))!;
			}
			CFString.ReleaseNative (nsasset);
			CFString.ReleaseNative (nspackage);
			return ret!;
		}
		[Export ("publish:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual void Publish (NSObject value)
		{
			var value__handle__ = value!.GetNonNullHandle (nameof (value));
			if (IsDirectBinding) {
				global::ApiDefinition.Messaging.void_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("publish:"), value__handle__);
			} else {
				global::ApiDefinition.Messaging.void_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("publish:"), value__handle__);
			}
		}
		[Export ("registerViewFactory:withId:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual void RegisterViewFactory (NSObject factory, string factoryId)
		{
			var factory__handle__ = factory!.GetNonNullHandle (nameof (factory));
			if (factoryId is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (factoryId));
			var nsfactoryId = CFString.CreateNative (factoryId);
			if (IsDirectBinding) {
				global::ApiDefinition.Messaging.void_objc_msgSend_NativeHandle_NativeHandle (this.Handle, Selector.GetHandle ("registerViewFactory:withId:"), factory__handle__, nsfactoryId);
			} else {
				global::ApiDefinition.Messaging.void_objc_msgSendSuper_NativeHandle_NativeHandle (this.SuperHandle, Selector.GetHandle ("registerViewFactory:withId:"), factory__handle__, nsfactoryId);
			}
			CFString.ReleaseNative (nsfactoryId);
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual FlutterBinaryMessenger Messenger {
			[Export ("messenger")]
			get {
				FlutterBinaryMessenger? ret;
				if (IsDirectBinding) {
					ret =  Runtime.GetNSObject<FlutterBinaryMessenger> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend (this.Handle, Selector.GetHandle ("messenger")))!;
				} else {
					ret =  Runtime.GetNSObject<FlutterBinaryMessenger> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper (this.SuperHandle, Selector.GetHandle ("messenger")))!;
				}
				return ret!;
			}
		}
	} /* class FlutterPluginRegistrar */
}
