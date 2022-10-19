using System;

using ObjCRuntime;
using Foundation;
using CoreGraphics;
using UIKit;

namespace Flutter.Internal;


[Protocol]
[BaseType(typeof(NSObject))]
interface FlutterMessageCodec
{
	// @required +(instancetype _Nonnull)sharedInstance;
	[Static]
	[Export("sharedInstance")]
	FlutterMessageCodec SharedInstance();

	// @required -(NSData * _Nullable)encode:(id _Nullable)message;
	//[Abstract]
	[Export("encode:")]
	[return: NullAllowed]
	NSData Encode([NullAllowed] NSObject message);

	// @required -(id _Nullable)decode:(NSData * _Nullable)message;
	//[Abstract]
	[Export("decode:")]
	[return: NullAllowed]
	NSObject Decode([NullAllowed] NSData message);
}
[BaseType(typeof(FlutterMessageCodec))]
interface FlutterStandardMessageCodec
{
	[Static]
	[Export("sharedInstance")]
	FlutterMessageCodec StandardSharedInstance { get; }
	// +(instancetype _Nonnull)codecWithReaderWriter:(FlutterStandardReaderWriter * _Nonnull)readerWriter;
	//[Static]
	//[Export("codecWithReaderWriter:")]
	//FlutterStandardMessageCodec CodecWithReaderWriter(FlutterStandardReaderWriter readerWriter);
}



[BaseType(typeof(FlutterMessageCodec))]
interface FlutterBinaryCodec
{
}

[Protocol]
interface FlutterPlatformViewFactory
{
	// @required -(NSObject<FlutterPlatformView> * _Nonnull)createWithFrame:(CGRect)frame viewIdentifier:(int64_t)viewId arguments:(id _Nullable)args;
	[Abstract]
	[Export("createWithFrame:viewIdentifier:arguments:")]
	FlutterPlatformView ViewIdentifier(CGRect frame, long viewId, [NullAllowed] NSObject args);

	// @optional -(NSObject<FlutterMessageCodec> * _Nonnull)createArgsCodec;
	[Export("createArgsCodec")]
	FlutterMessageCodec CreateArgsCodec { get; }
}
[Protocol]
[BaseType(typeof(NSObject))]
interface FlutterPlatformView
{
	// @required -(UIView * _Nonnull)view;
	[Abstract]
	[Export("view")]
	UIView View { get; }
}


[Protocol]
[BaseType(typeof(NSObject))]
interface FlutterPluginRegistrar
{
	// @required -(NSObject<FlutterBinaryMessenger> * _Nonnull)messenger;
	//[Abstract]
	[Export("messenger")]
	FlutterBinaryMessenger Messenger { get; }

	//// @required -(NSObject<FlutterTextureRegistry> * _Nonnull)textures;
	//[Abstract]
	//[Export("textures")]
	//FlutterTextureRegistry Textures { get; }

	// @required -(void)registerViewFactory:(NSObject<FlutterPlatformViewFactory> * _Nonnull)factory withId:(NSString * _Nonnull)factoryId;
	//[Abstract]
	[Export("registerViewFactory:withId:")]
	void RegisterViewFactory(NSObject factory, string factoryId);

	// @required -(void)registerViewFactory:(NSObject<FlutterPlatformViewFactory> * _Nonnull)factory withId:(NSString * _Nonnull)factoryId gestureRecognizersBlockingPolicy:(FlutterPlatformViewGestureRecognizersBlockingPolicy)gestureRecognizersBlockingPolicy;
	//[Abstract]
	//[Export("registerViewFactory:withId:gestureRecognizersBlockingPolicy:")]
	//void RegisterViewFactory(FlutterPlatformViewFactory factory, string factoryId, FlutterPlatformViewGestureRecognizersBlockingPolicy gestureRecognizersBlockingPolicy);

	// @required -(void)publish:(NSObject * _Nonnull)value;
	//[Abstract]
	[Export("publish:")]
	void Publish(NSObject value);

	// @required -(void)addMethodCallDelegate:(NSObject<FlutterPlugin> * _Nonnull)delegate channel:(FlutterMethodChannel * _Nonnull)channel;
	//[Abstract]
	//[Export("addMethodCallDelegate:channel:")]
	//void AddMethodCallDelegate(FlutterPlugin @delegate, FlutterMethodChannel channel);

	// @required -(void)addApplicationDelegate:(NSObject<FlutterPlugin> * _Nonnull)delegate;
	//[Abstract]
	//[Export("addApplicationDelegate:")]
	//void AddApplicationDelegate(FlutterPlugin @delegate);

	// @required -(NSString * _Nonnull)lookupKeyForAsset:(NSString * _Nonnull)asset;
	//[Abstract]
	[Export("lookupKeyForAsset:")]
	string LookupKeyForAsset(string asset);

	// @required -(NSString * _Nonnull)lookupKeyForAsset:(NSString * _Nonnull)asset fromPackage:(NSString * _Nonnull)package;
	//[Abstract]
	[Export("lookupKeyForAsset:fromPackage:")]
	string LookupKeyForAsset(string asset, string package);
}

[Protocol]
[BaseType(typeof(NSObject))]
interface FlutterPluginRegistry
{
	// @required -(NSObject<FlutterPluginRegistrar> * _Nullable)registrarForPlugin:(NSString * _Nonnull)pluginKey;
	//[Abstract]
	[Export("registrarForPlugin:")]
	[return: NullAllowed]
	FlutterPluginRegistrar RegistrarForPlugin(string pluginKey);

	// @required -(BOOL)hasPlugin:(NSString * _Nonnull)pluginKey;
	//[Abstract]
	[Export("hasPlugin:")]
	bool HasPlugin(string pluginKey);

	// @required -(NSObject * _Nullable)valuePublishedByPlugin:(NSString * _Nonnull)pluginKey;
	//[Abstract]
	[Export("valuePublishedByPlugin:")]
	[return: NullAllowed]
	NSObject ValuePublishedByPlugin(string pluginKey);
}

[DisableDefaultCtor]
[BaseType(typeof(UIViewController))]
interface FlutterViewController
{

	[Export("initWithEngine:nibName:bundle:")]
	IntPtr Constructor(FlutterEngine engine, [NullAllowed] string nibName, [NullAllowed] NSBundle bundle);

	//@property (weak, nonatomic, readonly) FlutterEngine* engine;
	[Export("engine")]
	FlutterEngine Engine { get; }

	[Export("pluginRegistry")]
	FlutterPluginRegistry PluginRegistry { get; }
}

// The first step to creating a binding is to add your native library ("libNativeLibrary.a")
// to the project by right-clicking (or Control-clicking) the folder containing this source
// file and clicking "Add files..." and then simply select the native library (or libraries)
// that you want to bind.
//
// When you do that, you'll notice that MonoDevelop generates a code-behind file for each
// native library which will contain a [LinkWith] attribute. VisualStudio auto-detects the
// architectures that the native library supports and fills in that information for you,
// however, it cannot auto-detect any Frameworks or other system libraries that the
// native library may depend on, so you'll need to fill in that information yourself.
//
// Once you've done that, you're ready to move on to binding the API...
//
//
// Here is where you'd define your API definition for the native Objective-C library.
//
// For example, to bind the following Objective-C class:
//
//     @interface Widget : NSObject {
//     }
//
// The C# binding would look like this:
//
//     [BaseType (typeof (NSObject))]
//     interface Widget {
//     }
//
// To bind Objective-C properties, such as:
//
//     @property (nonatomic, readwrite, assign) CGPoint center;
//
// You would add a property definition in the C# interface like so:
//
//     [Export ("center")]
//     CGPoint Center { get; set; }
//
// To bind an Objective-C method, such as:
//
//     -(void) doSomething:(NSObject *)object atIndex:(NSInteger)index;
//
// You would add a method definition to the C# interface like so:
//
//     [Export ("doSomething:atIndex:")]
//     void DoSomething (NSObject object, int index);
//
// Objective-C "constructors" such as:
//
//     -(id)initWithElmo:(ElmoMuppet *)elmo;
//
// Can be bound as:
//
//     [Export ("initWithElmo:")]
//     IntPtr Constructor (ElmoMuppet elmo);
//
// For more information, see https://aka.ms/ios-binding
//
[BaseType(typeof(FlutterPluginRegistry))]
interface FlutterEngine
{

	[Export("initWithName:project:")]
	IntPtr Constructor([NullAllowed] string name, [NullAllowed] NSObject project);

	[Export("initWithName:")]
	IntPtr Constructor([NullAllowed] string name);

	[Export("runWithEntrypoint:")]
	bool Run([NullAllowed] string entrypoint);

	[Export("binaryMessenger")]
	FlutterBinaryMessenger BinaryMessenger { get; }
}

[BaseType(typeof(NSObject))]
interface GeneratedPluginRegistrant
{
	[Static]
	[Export("registerWithRegistry:")]
	void Register(FlutterEngine engine);
}
//	@interface GeneratedPluginRegistrant : NSObject
//+ (void) registerWithRegistry:(NSObject<FlutterPluginRegistry>*) registry;
//	@end


[BaseType(typeof(NSObject))]
[Model]
interface FlutterBinaryMessenger
{

}
[BaseType(typeof(NSObject))]
interface FlutterMethodCall
{
	[Export("method")]
	string Method { get; }

	[Export("arguments")]
	NSObject Arguments { get; }
}
delegate void FlutterResult([NullAllowed] NSObject result);
delegate void FlutterMethodCallHandler(FlutterMethodCall call, [BlockCallback] FlutterResult result);
[BaseType(typeof(NSObject))]
interface FlutterMethodChannel
{

	[Export("methodChannelWithName:binaryMessenger:")]
	[Static]
	FlutterMethodChannel FromNameAndMessenger(string channelName, FlutterBinaryMessenger binaryMessenger);

	[Export("setMethodCallHandler:")]
	void SetMethodCaller([NullAllowed] FlutterMethodCallHandler handler);

	[Export("invokeMethod:arguments:")]
	void InvokeMethod(string method, [NullAllowed] NSObject arguments);
}
