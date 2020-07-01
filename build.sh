#!/bin/bash
cd flutter_module
flutter clean
flutter build ios-framework --output=build
flutter build aar