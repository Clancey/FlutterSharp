#!/bin/bash
version="1.0.0-241c87ad800beeab545ab867354d4683d5bfb6ce"
flutter_embedding_debug="https://storage.googleapis.com/download.flutter.io/io/flutter/flutter_embedding_debug/$version/flutter_embedding_debug-$version.jar"


cd flutter_module
echo "Cleaning Flutter"
flutter clean
echo "Building Flutter iOS"
flutter build ios-framework --output=build --debug
echo "Building Flutter Android AAR"
flutter build aar --debug
echo "Building Flutter Android APK"
flutter build apk --debug

echo "Downloading Flutter embedding debug"
curl -o  build/flutter_embedding_debug.jar $flutter_embedding_debug