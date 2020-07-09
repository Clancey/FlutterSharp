#!/bin/bash
cd FlutterCore
ssh pi@192.168.1.10 'cd ~/flutter/flutter-pi && make'
rsync -zvr pi@192.168.1.10:/home/pi/flutter/flutter-pi/out/libFlutter.so ../FlutterBindingPi/libFlutter.so
dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
rsync -zvr ./bin/Debug/netcoreapp3.1/linux-arm/publish/ pi@192.168.1.10:/home/pi/flutter_sharp/
#rsync -zvr ../FlutterBindingPi/libFlutter.so pi@192.168.1.10:/home/pi/flutter_sharp/libFlutter.so
cd ../../flutter_module
flutter build bundle
rsync -zvr ./build/flutter_assets root@192.168.1.10:/opt/flutter_sharp/flutter_assets