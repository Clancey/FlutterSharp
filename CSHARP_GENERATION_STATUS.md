# C# Code Generation - Status Update

**Date:** 2025-11-26
**Status:** ✅ **Dart Default Value Issues Fixed** - Major progress on C# compilation

## Summary

The C# code generator has been updated to convert Dart default values to C# syntax. The primary issue (Dart syntax in C# default parameters) has been resolved.

## Changes Made

### 1. Added Default Value Converter

**File:** `FlutterSharp.CodeGen/FlutterSharp.CodeGen/Generators/CSharp/CSharpWidgetGenerator.cs`

Added `ConvertDartDefaultValueToCSharp()` method that converts:
- `const Duration(milliseconds: 300)` → `TimeSpan.FromMilliseconds(300)`
- `const Duration(seconds: 1)` → `TimeSpan.FromSeconds(1)`
- `<String, WidgetBuilder>{}` → `null` (Dart map literals)
- `<NavigatorObserver>[]` → `null` (Dart list literals)
- `const EdgeInsets.all(8.0)` → `null` (complex constructors)

### 2. Generator Usage

To generate code with the correct output paths:

```bash
cd FlutterSharp.CodeGen/FlutterSharp.CodeGen
dotnet run -- generate \\
  --output-csharp ../../src/Flutter \\
  --output-dart ../../flutter_module/lib/generated
```

## Compilation Results

### Before Fixes
- **Errors:** 1000+ compilation errors
- **Primary Issue:** Dart syntax in C# default parameter values

### After Fixes
- **Errors:** 118 compilation errors (88% reduction!)
- **Warnings:** 870 warnings

## Remaining Issues

### 1. Missing `Clip` Enum (Most Common - ~40 errors)
**Error:** `The type or namespace name 'Clip' could not be found`

**Affected Files:**
- AnimatedPhysicalModelStruct.cs
- AnimatedSizeStruct.cs
- ClipOvalStruct.cs
- ClipPathStruct.cs
- ClipRectStruct.cs
- And many more...

**Solution Needed:** Generate or manually create the `Clip` enum type.

### 2. Generic Types Missing Type Arguments (~10 errors)
**Error:** `Using the generic type 'HashSet<T>' requires 1 type arguments`

**Examples:**
- `HashSet _gestureRecognizers` → Should be `HashSet<int>`
- `List _semanticsChildrenInTraversalOrder` → Should be `List<object>`

**Solution Needed:** Type mapper needs to handle incomplete generic types.

### 3. Non-Const TimeSpan Default Values (~30 errors)
**Error:** `Default parameter value for '_resizeDuration' must be a compile-time constant`

**Example:**
```csharp
TimeSpan? _resizeDuration = TimeSpan.FromMilliseconds(300)  // Not a compile-time constant!
```

**Solution Options:**
1. Use `null` as default and set in constructor body
2. Use milliseconds integer and convert to TimeSpan
3. Define static readonly TimeSpan constants

### 4. Generic Type Null Defaults (~5 errors)
**Error:** `A value of type '<null>' cannot be used as a default parameter because there are no standard conversions to type 'T'`

**Example:**
```csharp
T? _notifier = null  // Can't use null for unconstrained generic type parameter
```

**Solution:** Add `where T : class` constraint or don't use null default.

### 5. Other Type Mapping Issues (~33 errors)
Various issues with type conversions and mappings.

## Recommendations

### Immediate (To Get C# Compiling)

1. **Add Clip Enum** - Create FlutterEnums.cs with:
   ```csharp
   public enum Clip
   {
       None,
       HardEdge,
       AntiAlias,
       AntiAliasWithSaveLayer
   }
   ```

2. **Fix TimeSpan Defaults** - Update converter to use static readonly fields:
   ```csharp
   private static readonly TimeSpan DefaultResizeDuration = TimeSpan.FromMilliseconds(300);
   ```

3. **Fix Generic Type Arguments** - Update type mapper to provide concrete types for incomplete generics:
   - `HashSet` → `HashSet<int>` (for gesture recognizers)
   - `List` → `List<object>` (for unknown element types)

### Short Term (Improve Generator)

1. **Enum Generation** - Ensure all Flutter enums are generated
2. **Better Generic Handling** - Improve type mapper to handle partial generic types
3. **Const Default Values** - Generate code that uses compile-time constants or constructor initialization

### Long Term (Architectural)

1. **Complete Type Mapping** - Build comprehensive Dart → C# type mapping registry
2. **Enum Auto-Discovery** - Automatically find and generate all Flutter enums
3. **Default Value Strategy** - Standardize approach to default values (null vs constructor initialization)

## Success Metrics

✅ **88% Error Reduction** - From 1000+ to 118 errors
✅ **Dart Syntax Removed** - No more Dart collection literals in C#
✅ **Duration Mapping** - TimeSpan conversion working
⚠️ **Const Values** - TimeSpan not compile-time constant (needs different approach)
⚠️ **Missing Types** - Clip enum and other types need to be generated

## Next Steps

1. Add the `Clip` enum to resolve ~40 errors
2. Fix incomplete generic types (HashSet, List) to resolve ~10 errors
3. Change TimeSpan defaults to use null or static readonly to resolve ~30 errors
4. Address generic type parameter null defaults to resolve ~5 errors
5. Final cleanup of remaining type mapping issues

## Conclusion

The C# code generation is now **much closer to compilation**. The primary Dart syntax issues are resolved. The remaining errors are standard type definition and type mapping issues that can be systematically addressed.

**Estimated work to full compilation:** 2-4 hours of focused fixes
