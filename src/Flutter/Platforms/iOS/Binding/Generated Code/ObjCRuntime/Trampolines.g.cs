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
namespace ObjCRuntime {
	[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
	static partial class Trampolines {
		[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
		[UserDelegateType (typeof (global::Flutter.Internal.FlutterMethodCallHandler))]
		internal delegate void DFlutterMethodCallHandler (IntPtr block, NativeHandle call, NativeHandle result);
		//
		// This class bridges native block invocations that call into C#
		//
		static internal class SDFlutterMethodCallHandler {
			static internal readonly DFlutterMethodCallHandler Handler = Invoke;
			[Preserve (Conditional = true)]
			[global::System.Diagnostics.CodeAnalysis.DynamicDependency ("Handler")]
			[MonoPInvokeCallback (typeof (DFlutterMethodCallHandler))]
			static unsafe void Invoke (IntPtr block, NativeHandle call, NativeHandle result) {
				var descriptor = (BlockLiteral *) block;
				var del = (global::Flutter.Internal.FlutterMethodCallHandler) (descriptor->Target);
				if (del != null)
					del ( Runtime.GetNSObject<Flutter.Internal.FlutterMethodCall> (call)!, NIDFlutterResult.Create (result)!);
			}
		} /* class SDFlutterMethodCallHandler */
		internal sealed class NIDFlutterMethodCallHandler : TrampolineBlockBase {
			DFlutterMethodCallHandler invoker;
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			public unsafe NIDFlutterMethodCallHandler (BlockLiteral *block) : base (block)
			{
				invoker = block->GetDelegateForBlock<DFlutterMethodCallHandler> ();
			}
			[Preserve (Conditional=true)]
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			public unsafe static global::Flutter.Internal.FlutterMethodCallHandler? Create (IntPtr block)
			{
				if (block == IntPtr.Zero)
					return null;
				var del = (global::Flutter.Internal.FlutterMethodCallHandler) GetExistingManagedDelegate (block);
				return del ?? new NIDFlutterMethodCallHandler ((BlockLiteral *) block).Invoke;
			}
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			unsafe void Invoke (global::Flutter.Internal.FlutterMethodCall call, [BlockProxy (typeof (ObjCRuntime.Trampolines.NIDFlutterResult))]global::Flutter.Internal.FlutterResult result)
			{
				var call__handle__ = call.GetHandle ();
				if (result is null)
					ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (result));
				BlockLiteral *block_ptr_result;
				BlockLiteral block_result;
				block_result = new BlockLiteral ();
				block_ptr_result = &block_result;
				block_result.SetupBlockUnsafe (Trampolines.SDFlutterResult.Handler, result);
				invoker (BlockPointer, call__handle__, (IntPtr) block_ptr_result);
				block_ptr_result->CleanupBlock ();
			}
		} /* class NIDFlutterMethodCallHandler */
		[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
		[UserDelegateType (typeof (global::Flutter.Internal.FlutterResult))]
		internal delegate void DFlutterResult (IntPtr block, NativeHandle result);
		//
		// This class bridges native block invocations that call into C#
		//
		static internal class SDFlutterResult {
			static internal readonly DFlutterResult Handler = Invoke;
			[Preserve (Conditional = true)]
			[global::System.Diagnostics.CodeAnalysis.DynamicDependency ("Handler")]
			[MonoPInvokeCallback (typeof (DFlutterResult))]
			static unsafe void Invoke (IntPtr block, NativeHandle result) {
				var descriptor = (BlockLiteral *) block;
				var del = (global::Flutter.Internal.FlutterResult) (descriptor->Target);
				if (del != null)
					del ( Runtime.GetNSObject<NSObject> (result)!);
			}
		} /* class SDFlutterResult */
		internal sealed class NIDFlutterResult : TrampolineBlockBase {
			DFlutterResult invoker;
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			public unsafe NIDFlutterResult (BlockLiteral *block) : base (block)
			{
				invoker = block->GetDelegateForBlock<DFlutterResult> ();
			}
			[Preserve (Conditional=true)]
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			public unsafe static global::Flutter.Internal.FlutterResult? Create (IntPtr block)
			{
				if (block == IntPtr.Zero)
					return null;
				var del = (global::Flutter.Internal.FlutterResult) GetExistingManagedDelegate (block);
				return del ?? new NIDFlutterResult ((BlockLiteral *) block).Invoke;
			}
			[BindingImpl (BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
			unsafe void Invoke (NSObject? result)
			{
				var result__handle__ = result.GetHandle ();
				invoker (BlockPointer, result__handle__);
			}
		} /* class NIDFlutterResult */
	}
}
