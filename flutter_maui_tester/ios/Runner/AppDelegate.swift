import UIKit
import Flutter
import HttpSwift
//import Swifter

enum ChannelName {
  static let battery = "com.Microsoft.FlutterSharp/Messages"
}


@UIApplicationMain
@objc class AppDelegate: FlutterAppDelegate {
    
  override func application(
    _ application: UIApplication,
    didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?
  ) -> Bool {
    
    
    let controller : FlutterViewController = window?.rootViewController as! FlutterViewController
  
    let batteryChannel = FlutterMethodChannel(name: ChannelName.battery,
                                              binaryMessenger: controller.binaryMessenger);
    batteryChannel.setMethodCallHandler { (call, result) in
        
    }
    let server = Server()
    server.post("/Update") { (request) -> Response in
         if let string = String(bytes: request.body, encoding: .utf8) {
            //self.sendBatteryStateEvent(json:string);
            batteryChannel.invokeMethod("test", arguments: string);
            //batteryChannel.invokeMethod("my/super/test",arguments: request.body)
                print(string)
            } else {
                print("not a valid UTF-8 sequence")
            }
        return .ok("");
    }
    server.get("/hello/{id}") { request in
        print(request.queryParams["state"] ?? "")
        return .ok(request.routeParams["id"]!)
    }
    do {
        try server.run()
        print("Listening on port 8080")
    }catch {
            print("Error starting server");
    
        }
    
    
    
    
    
    GeneratedPluginRegistrant.register(with: self)
    return super.application(application, didFinishLaunchingWithOptions: launchOptions)
  }

}
