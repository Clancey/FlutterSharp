# FlutterSharp Roadmap

## Current Status

### Completed Features

| Feature | Status | Notes |
|---------|--------|-------|
| Code generation architecture | ✅ Complete | Scriban templates, Dart analyzer |
| C# widget generation | ✅ Complete | 400+ widgets generated |
| C# struct generation | ✅ Complete | FFI-compatible structs |
| Dart parser generation | ✅ Complete | Widget builders from structs |
| Dart struct generation | ✅ Complete | Matching FFI structs |
| Enum generation | ✅ Complete | 100+ enums |
| Type mapping system | ✅ Complete | 500+ type mappings |
| Basic interop protocol | ✅ Complete | Memory sharing, MethodChannel |
| Widget tree rendering | ✅ Complete | Recursive widget building |

### Known Issues

| Issue | Severity | Description |
|-------|----------|-------------|
| Missing Clip enum | High | ~40 compilation errors |
| Incomplete generics | Medium | HashSet<T>, some List<T> cases |
| TimeSpan defaults | Medium | Not compile-time constants |
| Builder callbacks | Medium | Synchronous invocation needed |
| State management | Low | Two-way binding incomplete |
| Animation controllers | Low | Stateful objects not supported |

## Phase 1: Compilation Fixes

**Goal**: Get the generated code to compile without errors.

### 1.1 Add Missing Enums

- [ ] Add `Clip` enum (hardest blocker - 40+ errors)
- [ ] Add any other missing enums from compilation errors
- [ ] Verify enum values match Dart source

### 1.2 Fix Type Mapping Issues

- [ ] Handle incomplete generic types (HashSet, Set)
- [ ] Fix default value issues for TimeSpan
- [ ] Handle nullable generic type parameters
- [ ] Review and fix delegate type mappings

### 1.3 Code Generation Improvements

- [ ] Add validation step before generation
- [ ] Generate placeholder types for missing mappings
- [ ] Add detailed error reporting for unmapped types
- [ ] Support for `// TODO:` markers in generated code

### 1.4 Testing

- [ ] Create test project that references all generated widgets
- [ ] Verify all widgets compile successfully
- [ ] Test basic widget instantiation
- [ ] Test property getters and setters

## Phase 2: Core Runtime

**Goal**: Working runtime with basic widget rendering.

### 2.1 FlutterManager Implementation

- [ ] Complete `FlutterManager` implementation
- [ ] Widget tracking with WeakDictionary
- [ ] Proper widget disposal
- [ ] Update message batching

### 2.2 Communication Layer

- [ ] Implement MethodChannel communication
- [ ] JSON message serialization
- [ ] Error handling for failed messages
- [ ] Timeout handling for stale messages

### 2.3 Dart Runtime

- [ ] Parser registry initialization
- [ ] MauiRenderer message handling
- [ ] MauiComponent state management
- [ ] Error widget for parse failures

### 2.4 Memory Management

- [ ] GCHandle lifecycle management
- [ ] String pointer cleanup
- [ ] Children array cleanup
- [ ] Memory leak detection

### 2.5 Testing

- [ ] End-to-end render test
- [ ] Widget update test
- [ ] Widget disposal test
- [ ] Memory leak test

## Phase 3: Callback System

**Goal**: Full bidirectional event handling.

### 3.1 Callback Registry

- [ ] Complete CallbackRegistry implementation
- [ ] Action registration
- [ ] Typed callback support
- [ ] Callback cleanup on dispose

### 3.2 Event Flow

- [ ] VoidCallback (tap, press)
- [ ] ValueChanged<T> callbacks
- [ ] Complex event data serialization
- [ ] Event routing to correct widget

### 3.3 Builder Callbacks

- [ ] Synchronous builder invocation
- [ ] ListView.builder support
- [ ] GridView.builder support
- [ ] IndexedWidgetBuilder support

### 3.4 Form Callbacks

- [ ] TextField onChanged
- [ ] TextField onSubmitted
- [ ] Checkbox onChanged
- [ ] Switch onChanged
- [ ] FormField validation

### 3.5 Testing

- [ ] Tap callback test
- [ ] Value change callback test
- [ ] Builder callback test
- [ ] Form interaction test

## Phase 4: Widget Coverage

**Goal**: Comprehensive widget support.

### 4.1 Core Widgets

- [ ] Container (complete)
- [ ] Text (complete)
- [ ] Column/Row (complete)
- [ ] Stack (complete)
- [ ] ListView
- [ ] GridView
- [ ] SingleChildScrollView

### 4.2 Input Widgets

- [ ] TextField
- [ ] Checkbox
- [ ] Radio
- [ ] Switch
- [ ] Slider
- [ ] DropdownButton

### 4.3 Button Widgets

- [ ] ElevatedButton
- [ ] TextButton
- [ ] OutlinedButton
- [ ] IconButton
- [ ] FloatingActionButton

### 4.4 Layout Widgets

- [ ] Padding
- [ ] Align
- [ ] Center
- [ ] Expanded
- [ ] Flexible
- [ ] SizedBox
- [ ] AspectRatio

### 4.5 Material Widgets

- [ ] Scaffold
- [ ] AppBar
- [ ] BottomNavigationBar
- [ ] Drawer
- [ ] Card
- [ ] ListTile

### 4.6 Cupertino Widgets

- [ ] CupertinoButton
- [ ] CupertinoTextField
- [ ] CupertinoNavigationBar
- [ ] CupertinoTabBar

## Phase 5: Advanced Features

**Goal**: Production-ready features.

### 5.1 State Management

- [ ] Two-way data binding
- [ ] State synchronization
- [ ] Incremental updates
- [ ] Optimistic updates

### 5.2 Navigation

- [ ] Navigator support
- [ ] Route management
- [ ] Page transitions
- [ ] Deep linking

### 5.3 Animation

- [ ] AnimationController bindings
- [ ] Implicit animations
- [ ] Explicit animations
- [ ] Transition widgets

### 5.4 Theming

- [ ] ThemeData support
- [ ] Dark mode
- [ ] Custom themes
- [ ] Dynamic theming

### 5.5 Scrolling

- [ ] ScrollController
- [ ] Scroll position tracking
- [ ] Infinite scrolling
- [ ] Pull-to-refresh

## Phase 6: Platform Integration

**Goal**: Full .NET MAUI integration.

### 6.1 MAUI Integration

- [ ] FlutterView MAUI component
- [ ] Platform initialization
- [ ] Lifecycle management
- [ ] Sizing and layout

### 6.2 Platform-Specific

- [ ] iOS implementation
- [ ] Android implementation
- [ ] Windows implementation
- [ ] macOS implementation

### 6.3 Performance

- [ ] Benchmark suite
- [ ] Memory profiling
- [ ] Rendering performance
- [ ] Message throughput

### 6.4 Developer Experience

- [ ] Hot reload integration
- [ ] Error overlay
- [ ] Debug logging
- [ ] Performance overlay

## Phase 7: Package Ecosystem

**Goal**: Support third-party Flutter packages.

### 7.1 Package Generation

- [ ] pub.dev package support
- [ ] Local package support
- [ ] Package versioning
- [ ] Dependency resolution

### 7.2 Popular Packages

- [ ] provider
- [ ] riverpod
- [ ] bloc
- [ ] get
- [ ] dio

### 7.3 Custom Packages

- [ ] Custom widget registration
- [ ] Manual binding support
- [ ] Package templates
- [ ] Documentation generator

## Phase 8: Documentation and Tooling

**Goal**: Production documentation and tooling.

### 8.1 Documentation

- [ ] API documentation
- [ ] Getting started guide
- [ ] Widget catalog
- [ ] Migration guide

### 8.2 Tooling

- [ ] VS/VS Code extension
- [ ] CLI improvements
- [ ] Widget inspector
- [ ] Code snippets

### 8.3 Samples

- [ ] Basic sample app
- [ ] Todo app
- [ ] E-commerce app
- [ ] Social app

## Success Metrics

### Phase 1 Success

- [ ] Zero compilation errors
- [ ] All generated widgets compile
- [ ] Basic tests pass

### Phase 2 Success

- [ ] Simple widget renders in Flutter
- [ ] Widget updates work
- [ ] Memory is properly managed

### Phase 3 Success

- [ ] Tap events work
- [ ] Form inputs work
- [ ] List builders work

### Phase 4 Success

- [ ] 80% widget coverage
- [ ] Common patterns work
- [ ] Performance acceptable

### Phase 5 Success

- [ ] State management works
- [ ] Navigation works
- [ ] Animations work

### Phase 6 Success

- [ ] Runs on iOS
- [ ] Runs on Android
- [ ] Runs on Windows/macOS

### Phase 7 Success

- [ ] 5+ packages supported
- [ ] Custom packages work
- [ ] Package versioning works

### Phase 8 Success

- [ ] Complete documentation
- [ ] IDE integration
- [ ] 3+ sample apps

## Technical Debt

### High Priority

1. **Type system gaps**: Some Dart types don't map cleanly to C#
2. **Builder callbacks**: Need synchronous channel support
3. **Generic constraints**: Type parameter handling incomplete

### Medium Priority

1. **Default values**: Not all Dart defaults are compile-time constants in C#
2. **Documentation**: XML docs could be more complete
3. **Error messages**: Could be more helpful

### Low Priority

1. **Code style**: Generated code style could match C# conventions better
2. **Optimizations**: Some inefficient patterns in generated code
3. **Refactoring**: Some duplicate logic in generators

## Future Considerations

### Potential Features

- **Hot Reload Integration**: Sync with Flutter hot reload
- **Design Tools**: Visual widget editor
- **Testing Tools**: Widget testing framework
- **AI Integration**: AI-assisted widget building

### Architecture Evolution

- **Native FFI**: Direct FFI without MethodChannel
- **Shared Memory Pool**: Pre-allocated memory for widgets
- **Binary Protocol**: Replace JSON with binary for performance
- **Code Sharing**: Share business logic between C# and Dart

### Ecosystem Growth

- **Community Packages**: Accept community contributions
- **Enterprise Support**: Enterprise licensing and support
- **Certification**: FlutterSharp certification program
- **Conferences**: Speak at .NET and Flutter conferences

## Version Milestones

| Version | Target | Focus |
|---------|--------|-------|
| 0.1.0 | Phase 1-2 | Compiles and renders |
| 0.2.0 | Phase 3 | Callbacks work |
| 0.5.0 | Phase 4 | Good widget coverage |
| 0.8.0 | Phase 5 | Advanced features |
| 1.0.0 | Phase 6 | Production ready |
| 1.5.0 | Phase 7 | Package ecosystem |
| 2.0.0 | Phase 8 | Full ecosystem |

## Contributing

### Areas Needing Help

1. **Type Mapping**: Adding more Dart → C# type mappings
2. **Widget Testing**: Testing generated widgets
3. **Documentation**: Writing guides and tutorials
4. **Platform Ports**: iOS, Android, Windows, macOS testing

### How to Contribute

1. Pick an issue from the roadmap
2. Create a feature branch
3. Implement the feature
4. Add tests
5. Submit PR

## See Also

- [OVERVIEW.md](./OVERVIEW.md) - Project overview
- [ARCHITECTURE.md](./ARCHITECTURE.md) - System architecture
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Code generation
