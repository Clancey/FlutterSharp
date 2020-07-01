#!/bin/bash
cd flutter_module
flutter clean
flutter build ios-framework -v --output=build
flutter build aar