# FlutterSharp Implementation Plan

This is the active task list for autonomous agent execution. The agent selects ONE task per loop, completes it, and updates this file.

## Status Key

| Status | Meaning |
|--------|---------|
| `pending` | Not started |
| `in_progress` | Currently being worked on |
| `completed` | Done and verified |
| `blocked` | Cannot proceed, see notes |

## Current Build Status

**Last checked**: 2026-01-07
**C# compilation errors**: 50 (regression - needs investigation)
**Dart analysis errors**: 853 (reduced from 1448 → 42% reduction)

---

## Phase 1: Compilation Fixes

**Goal**: Get all generated code to compile without errors.

### 1.1 Missing Enums (HIGH PRIORITY)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| E001 | Add `Clip` enum to src/Flutter/Enums/ | completed | Already existed, verified working |
| E002 | Verify TextOverflow enum has correct values | pending | Check against Flutter source |
| E003 | Audit all enums for missing values | completed | No missing enums blocking C# build |
| E004 | Add any other missing enums from build errors | completed | Not needed - build passes |

### 1.2 Type Mapping Fixes (HIGH PRIORITY)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| T001 | Fix HashSet<T> type mapping | pending | Map to ISet<T> or HashSet<T> |
| T002 | Fix Set<T> type mapping | pending | Map to ISet<T> |
| T003 | Handle incomplete generic types | pending | Search for "Object" fallbacks |
| T004 | Fix TimeSpan default value issues | pending | TimeSpan not compile-time constant |
| T005 | Fix nullable generic type parameters | pending | T? where T is already nullable |
| T006 | Review delegate type mappings | pending | Ensure Action/Func are correct |

### 1.3 Widget Compilation Fixes (MEDIUM PRIORITY)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| W001 | Run dotnet build and capture all errors | completed | 0 errors as of 2026-01-06 |
| W002 | Fix widgets with Clip property errors | completed | Not needed - Clip enum worked |
| W003 | Fix widgets with missing type errors | completed | Fixed base class constructors |
| W004 | Fix widgets with default value errors | completed | Fixed by making constructors parameterless |
| W005 | Fix widgets with callback type errors | completed | Fixed FlutterManager SendEvent type conversion |
| W006 | Fix AnimatedWidget base class constructor | completed | Made parameterless |
| W007 | Fix ImplicitlyAnimatedWidget base class constructor | completed | Made parameterless |
| W008 | Fix Viewport base class constructor | completed | Made parameterless |
| W009 | Add Widget.Id property | completed | For widget tracking |
| W010 | Add Widget.SendEvent method | completed | For event handling |

### 1.4 Dart Compilation Fixes (MEDIUM PRIORITY)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| D001 | Run dart analyze on flutter_module | completed | Baseline: 3684 errors, now 1448 |
| D002 | Fix Dart parser syntax errors | completed | Fixed ToCamelCase, added stub parsers, 376→0 undefined_method |
| D003 | Fix Dart struct definition errors | completed | Removed invalid interface classes, fixed template |
| D004 | Ensure parsers match C# structs | pending | Field order, types |
| D005 | Remove duplicate nested directories | completed | lib/structs/structs/ and lib/structs/parsers/ removed |
| D006 | Fix FFI struct field type annotations | completed | Changed Int8/Double to int/double with @Int8()/@Double() |
| D007 | Fix argument_type_not_assignable errors | pending | 273 remaining type conversion issues |
| D008 | Fix undefined_getter errors | pending | 139 missing struct field accessors |

### 1.5 Code Generator Fixes (LOW PRIORITY)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| G001 | Add validation before generation | pending | Catch issues earlier |
| G002 | Improve error reporting for unmapped types | pending | Better messages |
| G003 | Add Clip enum to manual types | pending | If not auto-generated |

---

## Phase 2: Core Runtime

**Goal**: Basic widget rendering working end-to-end.

### 2.1 C# Runtime (BLOCKED - needs Phase 1)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| R001 | Complete FlutterManager implementation | pending | Blocked by Phase 1 |
| R002 | Implement widget tracking dictionary | pending | |
| R003 | Implement widget disposal | pending | |
| R004 | Implement update batching | pending | |

### 2.2 Communication (BLOCKED - needs Phase 1)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| C001 | Implement MethodChannel setup | pending | |
| C002 | Implement message serialization | pending | |
| C003 | Implement error handling | pending | |
| C004 | Implement timeout handling | pending | |

### 2.3 Dart Runtime (BLOCKED - needs Phase 1)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| DR001 | Initialize parser registry | pending | |
| DR002 | Implement MauiRenderer | pending | |
| DR003 | Implement MauiComponent | pending | |
| DR004 | Add error widget for failures | pending | |

### 2.4 Memory Management (BLOCKED - needs Phase 1)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| M001 | Implement GCHandle lifecycle | pending | |
| M002 | Implement string pointer cleanup | pending | |
| M003 | Implement children array cleanup | pending | |
| M004 | Add memory leak detection | pending | |

---

## Phase 3: Callback System

**Goal**: Full bidirectional event handling.

### 3.1 Callback Registry (BLOCKED - needs Phase 2)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| CB001 | Complete CallbackRegistry | pending | |
| CB002 | Implement action registration | pending | |
| CB003 | Implement typed callbacks | pending | |
| CB004 | Implement callback cleanup | pending | |

### 3.2 Event Types (BLOCKED - needs CB001)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| EV001 | Implement VoidCallback | pending | |
| EV002 | Implement ValueChanged<T> | pending | |
| EV003 | Implement complex event data | pending | |
| EV004 | Implement event routing | pending | |

---

## Immediate Next Actions

When starting a new loop, work on these in order:

1. ~~**E001** - Add Clip enum~~ ✅ DONE
2. ~~**W001** - Run build to get full error list~~ ✅ DONE (0 errors!)
3. ~~**D001** - Run dart analyze and get baseline error count~~ ✅ DONE (1448 errors)
4. ~~**D003** - Fix Dart struct definition errors~~ ✅ DONE
5. ~~**D005** - Remove duplicate nested directories~~ ✅ DONE
6. ~~**D006** - Fix FFI struct field type annotations~~ ✅ DONE
7. ~~**D002** - Fix undefined_method errors (370) - add missing parse methods~~ ✅ DONE (376→0)
8. **D007** - Fix argument_type_not_assignable errors (302) - type conversions
9. **D008** - Fix undefined_getter errors (139) - struct field accessors

---

## Blocked Tasks Log

| Task ID | Blocked By | Date Blocked | Notes |
|---------|------------|--------------|-------|
| (none yet) | | | |

---

## Completed Tasks Log

| Task ID | Completed Date | Commit Hash | Notes |
|---------|---------------|-------------|-------|
| E001 | 2026-01-06 | a68aa86 | Clip enum already existed |
| E003 | 2026-01-06 | a68aa86 | No missing enums |
| E004 | 2026-01-06 | a68aa86 | Not needed |
| W001-W010 | 2026-01-06 | a68aa86 | C# compilation fully fixed |
| D001 | 2026-01-07 | pending | Baseline 3684 errors, reduced to 1448 |
| D003 | 2026-01-07 | pending | Removed interface classes, fixed template |
| D005 | 2026-01-07 | pending | Removed duplicate nested directories |
| D006 | 2026-01-07 | pending | Fixed FFI struct field types |
| D002 | 2026-01-07 | 4b5d8b8 | Fixed ToCamelCase in generators, added stub parsers |

---

## Discovery Notes

Add notes here when exploring the codebase:

### Enum Findings
- Clip enum needed: none (0), hardEdge (1), antiAlias (2), antiAliasWithSaveLayer (3)
- src/Flutter/Enums/Clip.cs exists but may be incomplete

### Type Mapping Findings
- (Add findings here as you explore)

### Build Error Patterns
- Base class constructors with required params caused errors in all derived classes
- Solution: Make abstract base class constructors parameterless
- FlutterManager used `object` type for event data, needed string conversion
- Dart parsers have many issues with required named parameters and type mismatches

### Dart FFI Struct Findings (2026-01-07)
- Interface classes (`class IWidgetStruct { external ... }`) are invalid - `external` is only for FFI structs
- Solution: Use `abstract class` with `Pointer get handle;` getters instead
- FFI struct fields must be primitive types (`int`, `double`) with annotations (`@Int8()`, `@Double()`)
- The template was generating `external Int8 field;` instead of `@Int8() external int field;`
- Duplicate directories (`lib/structs/structs/`, `lib/structs/parsers/`) were adding ~1100 errors
- Remaining error categories (after D002 fix - 853 total):
  - 302 argument_type_not_assignable - FFI pointer vs Dart type mismatches
  - 194 missing_required_argument - widgets with required params
  - 139 undefined_getter - missing struct field accessors
  - 101 undefined_named_parameter - wrong param names
  - 0 undefined_method - FIXED by adding stub parsers
  - 0 invalid_field_type_in_struct - FIXED by ToCamelCase @ stripping

---

## How to Update This File

After completing a task:

1. Change status from `pending` → `completed`
2. Add completion date and commit hash to Completed Tasks Log
3. Remove from Immediate Next Actions if applicable
4. Add any newly discovered tasks
5. Update dependency notes if needed

After hitting a blocker:

1. Change status to `blocked`
2. Add entry to Blocked Tasks Log
3. Add blocker details to Notes column
4. Select different non-blocked task

---

## Success Criteria

### Phase 1 Complete When:
- [ ] `dart analyze flutter_module` has no errors (~2282 remaining)
- [ ] `dotnet build src/Flutter/Flutter.csproj` 
- [ ] All generated C# widgets compile 

### Phase 2 Complete When:
- [ ] Simple Text widget renders in Flutter
- [ ] Widget updates work
- [ ] Memory is properly managed

### Phase 3 Complete When:
- [ ] Tap events work end-to-end
- [ ] TextField onChanged works
- [ ] ListView.builder callbacks work

---

## Reference

- [AGENTS.md](../AGENTS.md) - How to use subagents
- [PROMPT.md](../PROMPT.md) - Main loop behavior
- [ROADMAP.md](./ROADMAP.md) - Full project roadmap
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) - Type mapping reference
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Generator details
