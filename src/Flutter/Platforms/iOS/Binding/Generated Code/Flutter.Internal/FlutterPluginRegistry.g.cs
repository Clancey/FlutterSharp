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
	[Protocol (Name = "FlutterPluginRegistry", WrapperType = typeof (FlutterPluginRegistryWrapper))]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "RegistrarForPlugin", Selector = "registrarForPlugin:", ReturnType = typeof (Flutter.Internal.FlutterPluginRegistrar), ParameterType = new Type [] { typeof (string) }, ParameterByRef = new bool [] { false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "HasPlugin", Selector = "hasPlugin:", ReturnType = typeof (bool), ParameterType = new Type [] { typeof (string) }, ParameterByRef = new bool [] { false })]
	[ProtocolMember (IsRequired = false, IsProperty = false, IsStatic = false, Name = "ValuePublishedByPlugin", Selector = "valuePublishedByPlugin:", ReturnType = typeof (NSObject), ParameterType = new Type [] { typeof (string) }, ParameterByRef = new bool [] { false })]
	public partial interface IFlutterPluginRegistry : INativeObject, IDisposable
	{
	}
	public static partial class FlutterPluginRegistry_Extensions {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static FlutterPluginRegistrar? RegistrarForPlugin (this IFlutterPluginRegistry This, string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			FlutterPluginRegistrar? ret;
			ret =  Runtime.GetNSObject<FlutterPluginRegistrar> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("registrarForPlugin:"), nspluginKey))!;
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static bool HasPlugin (this IFlutterPluginRegistry This, string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			bool ret;
			ret = global::ApiDefinition.Messaging.bool_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("hasPlugin:"), nspluginKey);
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public static NSObject? ValuePublishedByPlugin (this IFlutterPluginRegistry This, string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			NSObject? ret;
			ret = Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (This.Handle, Selector.GetHandle ("valuePublishedByPlugin:"), nspluginKey))!;
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
	}
	internal sealed class FlutterPluginRegistryWrapper : BaseWrapper, IFlutterPluginRegistry {
		[Preserve (Conditional = true)]
		public FlutterPluginRegistryWrapper (NativeHandle handle, bool owns)
			: base (handle, owns)
		{
		}
	}
}
namespace Flutter.Internal {
	[Protocol()]
	[Register("FlutterPluginRegistry", true)]
	public unsafe partial class FlutterPluginRegistry : NSObject, IFlutterPluginRegistry {
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		static readonly NativeHandle class_ptr = Class.GetHandle ("FlutterPluginRegistry");
		public override NativeHandle ClassHandle { get { return class_ptr; } }
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Export ("init")]
		public FlutterPluginRegistry () : base (NSObjectFlag.Empty)
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
		protected FlutterPluginRegistry (NSObjectFlag t) : base (t)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal FlutterPluginRegistry (NativeHandle handle) : base (handle)
		{
			IsDirectBinding = GetType ().Assembly == global::ApiDefinition.Messaging.this_assembly;
		}

		[Export ("hasPlugin:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual bool HasPlugin (string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			bool ret;
			if (IsDirectBinding) {
				ret = global::ApiDefinition.Messaging.bool_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("hasPlugin:"), nspluginKey);
			} else {
				ret = global::ApiDefinition.Messaging.bool_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("hasPlugin:"), nspluginKey);
			}
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
		[Export ("registrarForPlugin:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual FlutterPluginRegistrar? RegistrarForPlugin (string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			FlutterPluginRegistrar? ret;
			if (IsDirectBinding) {
				ret =  Runtime.GetNSObject<FlutterPluginRegistrar> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("registrarForPlugin:"), nspluginKey))!;
			} else {
				ret =  Runtime.GetNSObject<FlutterPluginRegistrar> (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("registrarForPlugin:"), nspluginKey))!;
			}
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
		[Export ("valuePublishedByPlugin:")]
		[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual NSObject? ValuePublishedByPlugin (string pluginKey)
		{
			if (pluginKey is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (pluginKey));
			var nspluginKey = CFString.CreateNative (pluginKey);
			NSObject? ret;
			if (IsDirectBinding) {
				ret = Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSend_NativeHandle (this.Handle, Selector.GetHandle ("valuePublishedByPlugin:"), nspluginKey))!;
			} else {
				ret = Runtime.GetNSObject (global::ApiDefinition.Messaging.NativeHandle_objc_msgSendSuper_NativeHandle (this.SuperHandle, Selector.GetHandle ("valuePublishedByPlugin:"), nspluginKey))!;
			}
			CFString.ReleaseNative (nspluginKey);
			return ret!;
		}
	} /* class FlutterPluginRegistry */
}
