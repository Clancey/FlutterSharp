#!/bin/bash

flutterCommand=`which flutter`
flutterBin=${flutterCommand%/flutter}
engineVersion="$flutterBin/internal/engine.version"
version=`cat $engineVersion`

echo "Using Flutter $version"
flutter_embedding_debug="https://storage.googleapis.com/download.flutter.io/io/flutter/flutter_embedding_debug/1.0.0-$version/flutter_embedding_debug-1.0.0-$version.jar"

cd flutter_module

# echo "Cleaning Flutter"
# flutter clean
echo "Building Flutter iOS"
flutter build ios-framework --output=build --no-profile
echo "Building Flutter Android AAR"
flutter build aar --no-profile
echo "Building Flutter Android APK"
flutter build apk

echo "Downloading Flutter embedding debug"
curl -o  build/flutter_embedding_debug.jar $flutter_embedding_debug

dotnet build ../src/Flutter.Bindings/Flutter.Bindings.csproj