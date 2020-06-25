using System;

using ObjCRuntime;
using Foundation;
using UIKit;

namespace Flutter.Internal.iOS {
	[DisableDefaultCtor]
	[BaseType (typeof (UIViewController))]
	interface FlutterViewController {

		[Export ("initWithEngine:nibName:bundle:")]
		IntPtr Constructor (FlutterEngine engine, [NullAllowed] string nibName, [NullAllowed] NSBundle bundle);

		//@property (weak, nonatomic, readonly) FlutterEngine* engine;
		[Export ("engine")]
		FlutterEngine Engine { get; }
	}
}
namespace Flutter.Internal {
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
	[BaseType (typeof (NSObject))]
	interface FlutterEngine {

		[Export ("initWithName:project:")]
		IntPtr Constructor ([NullAllowed] string name, [NullAllowed] NSObject project);

		[Export ("runWithEntrypoint:")]
		bool Run ([NullAllowed] string entrypoint);

		[Export ("binaryMessenger")]
		FlutterBinaryMessenger BinaryMessenger { get;}
	}

	[BaseType(typeof(NSObject))]
	interface GeneratedPluginRegistrant {
		[Static]
		[Export ("registerWithRegistry:")]
		void Register (FlutterEngine engine);
	}
	//	@interface GeneratedPluginRegistrant : NSObject
	//+ (void) registerWithRegistry:(NSObject<FlutterPluginRegistry>*) registry;
	//	@end
	

	[BaseType (typeof (NSObject))]
	[Model]
	[Protocol]
	interface FlutterBinaryMessenger {

	}
	[BaseType(typeof(NSObject))]
	interface FlutterMethodCall {
		[Export("method")]
		string Method { get; }

		[Export("arguments")]
		NSObject Arguments { get; }
	}
	delegate void FlutterResult ([NullAllowed] NSObject result);
	delegate void FlutterMethodCallHandler (FlutterMethodCall call, [BlockCallback] FlutterResult result);
	[BaseType(typeof(NSObject))]
	interface FlutterMethodChannel {

		[Export ("methodChannelWithName:binaryMessenger:")]
		[Static]
		FlutterMethodChannel FromNameAndMessenger (string channelName, FlutterBinaryMessenger binaryMessenger);

		[Export ("setMethodCallHandler:")]
		void SetMethodCaller ([NullAllowed] FlutterMethodCallHandler handler);

		[Export ("invokeMethod:arguments:")]
		void InvokeMethod (string method, [NullAllowed] NSObject arguments);
	}


}

