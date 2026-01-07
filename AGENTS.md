# FlutterSharp Agent Configuration

## Core Principle: Monolithic Scheduler with Subagents

FlutterSharp uses a single autonomous agent running in a bash loop. The primary context window operates as a **scheduler** that spawns subagents for specific tasks. This extends effective context while maintaining a single source of truth.

## Agent Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Primary Agent (Scheduler)                 │
│                                                              │
│  - Reads PROMPT.md each loop                                │
│  - Loads IMPLEMENTATION_PLAN.md                             │
│  - Selects ONE task per loop                                │
│  - Spawns subagents for work                                │
│  - Updates plan after completion                            │
│  - Commits when tests pass                                  │
└─────────────────────────────────────────────────────────────┘
           │                    │                    │
           ▼                    ▼                    ▼
    ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
    │  Explore    │     │  Implement  │     │  Build/Test │
    │  Subagent   │     │  Subagent   │     │  Subagent   │
    │  (parallel) │     │  (parallel) │     │  (serial)   │
    └─────────────┘     └─────────────┘     └─────────────┘
```

## Subagent Types and Usage

### 1. Explore Agent
**Purpose**: Search and understand the codebase before making changes.

**When to use**:
- Before implementing ANY feature
- When unsure if something is already implemented
- To find related code patterns
- To understand existing architecture

**Parallelism**: Up to 5 concurrent explore agents

```
CRITICAL: Never assume code doesn't exist. Always search first.
```

**Example tasks**:
- "Find all usages of CallbackRegistry"
- "Search for existing Clip enum definitions"
- "Find how other widgets handle nullable properties"
- "Locate all type mapping code"

### 2. Code Generation Agent
**Purpose**: Work on the code generation system.

**Subagent type**: `csharp-pro` or `dart-expert`

**When to use**:
- Modifying Scriban templates
- Updating type mappings
- Fixing generator logic
- Adding new generation features

**Key files**:
- `FlutterSharp.CodeGen/` - C# generator
- `FlutterSharp.CodeGen/Templates/` - Scriban templates
- `FlutterSharp.CodeGen/TypeMapping/` - Type mappings

### 3. C# Widget Agent
**Purpose**: Fix or enhance generated C# widgets.

**Subagent type**: `csharp-pro`

**When to use**:
- Fixing compilation errors in widgets
- Adding manual widget implementations
- Updating widget patterns

**Key files**:
- `src/Flutter/Widgets/` - Generated widgets
- `src/Flutter/Structs/` - FFI structs
- `src/Flutter/Enums/` - Generated enums

### 4. Dart Parser Agent
**Purpose**: Work on Dart-side parsing and widget building.

**Subagent type**: `dart-expert` or `flutter-expert`

**When to use**:
- Fixing Dart parser generation
- Updating MauiRenderer
- Adding widget parsers

**Key files**:
- `flutter_module/lib/` - Dart code
- `flutter_module/lib/parsers/` - Widget parsers
- `flutter_module/lib/maui_flutter.dart` - Main entry

### 5. Build/Test Agent
**Purpose**: Run builds and tests to verify changes.

**Subagent type**: `debugger`

**CRITICAL**: Only ONE build/test agent at a time.

**When to use**:
- After making code changes
- To verify compilation
- To run tests
- To check for regressions

**Commands**:
```bash
# Build C# project
dotnet build src/Flutter/Flutter.csproj

# Build code generator
dotnet build FlutterSharp.CodeGen/FlutterSharp.CodeGen/FlutterSharp.CodeGen.csproj

# Run code generator
dotnet run --project FlutterSharp.CodeGen/FlutterSharp.CodeGen/FlutterSharp.CodeGen.csproj

# Analyze Dart code
cd flutter_module && dart analyze
```

## Agent Rules

### Rule 1: One Task Per Loop
```
The scheduler picks ONE task from IMPLEMENTATION_PLAN.md per loop iteration.
Do not attempt multiple unrelated tasks in a single loop.
```

### Rule 2: Search Before Implementing
```
Before implementing anything, spawn an Explore agent to:
1. Check if it already exists
2. Find similar patterns
3. Understand the context

Never assume something isn't implemented.
```

### Rule 3: Parallel Exploration, Serial Building
```
Exploration: Up to 5 parallel subagents
Implementation: Up to 3 parallel subagents
Build/Test: Exactly 1 subagent (serial)
```

### Rule 4: No Placeholders
```
Every implementation must be complete and functional.
No TODO comments in generated code.
No placeholder implementations.
No "will implement later" code.
```

### Rule 5: Tests as Backpressure
```
The build/test agent provides backpressure:
- If tests fail, fix before proceeding
- If build fails, fix before proceeding
- Don't move to next task until current passes
```

### Rule 6: Commit When Green
```
After tests pass:
1. Stage changed files
2. Commit with descriptive message
3. Reference the task from IMPLEMENTATION_PLAN.md
```

### Rule 7: Update the Plan
```
After completing a task:
1. Mark task complete in IMPLEMENTATION_PLAN.md
2. Add any new discovered tasks
3. Re-prioritize if needed
```

## Task Selection Algorithm

```
1. Read IMPLEMENTATION_PLAN.md
2. Find first incomplete task with:
   - Status: "pending" or "in_progress"
   - No blocking dependencies
   - Highest priority
3. If task is blocked:
   - Find and complete blocking task first
4. Execute selected task
5. Update plan
6. Loop
```

## Subagent Communication

### Spawning a Subagent
```
Use Task tool with appropriate subagent_type:
- subagent_type: "Explore" - For searching/understanding
- subagent_type: "csharp-pro" - For C# implementation
- subagent_type: "dart-expert" - For Dart implementation
- subagent_type: "debugger" - For build/test issues
```

### Subagent Prompt Template
```
Context:
- Working on: [task description]
- Current file: [file path]
- Goal: [specific goal]

Instructions:
[specific instructions]

Constraints:
- Do not modify files outside scope
- Report findings back
- Be thorough but focused
```

## Error Handling

### Build Errors
1. Spawn debugger agent to analyze error
2. Spawn explore agent to find similar working code
3. Implement fix
4. Re-run build

### Test Failures
1. Spawn debugger agent to understand failure
2. Fix the issue (not the test, unless test is wrong)
3. Re-run tests
4. Ensure all tests pass

### Stuck Agent
If no progress after 3 attempts:
1. Document the blocker in IMPLEMENTATION_PLAN.md
2. Move to next non-blocked task
3. Return to blocked task later with fresh context

## Context Management

### What to Load Each Loop
1. `PROMPT.md` - Main instructions
2. `IMPLEMENTATION_PLAN.md` - Current tasks
3. Relevant spec file for current task
4. Minimal code context (let subagents explore)

### What Subagents Should Load
1. Task-specific context only
2. Relevant source files
3. Related test files
4. Type mapping if needed

### Context Efficiency
```
Primary agent: Scheduler logic + task selection
Subagents: Detailed implementation work
This maximizes effective context across the system.
```

## Self-Improvement

The agent can improve its own operation by:

1. **Updating AGENTS.md**: Add new patterns discovered
2. **Updating PROMPT.md**: Refine instructions
3. **Updating IMPLEMENTATION_PLAN.md**: Better task breakdown
4. **Updating specs**: Clarify unclear areas

```
Self-improvement changes should be committed separately
with clear commit messages explaining the improvement.
```

## Monitoring

### Signs of Good Operation
- Tasks completing in order
- Build staying green
- Commits happening regularly
- Plan being updated

### Signs of Problems
- Same task attempted repeatedly
- Build errors not being fixed
- No commits for extended period
- Subagents timing out

## Example Workflow

```
Loop 1:
  - Read PROMPT.md, IMPLEMENTATION_PLAN.md
  - Select task: "Add Clip enum"
  - Spawn Explore agent: "Find all Clip usage in Flutter SDK"
  - Spawn Explore agent: "Find existing enum generation code"
  - Review findings
  - Spawn csharp-pro agent: "Generate Clip enum"
  - Spawn build agent: "Verify compilation"
  - Build passes
  - Commit: "Add Clip enum to fix 40 compilation errors"
  - Update IMPLEMENTATION_PLAN.md

Loop 2:
  - Read PROMPT.md, IMPLEMENTATION_PLAN.md
  - Select task: "Fix TextOverflow enum values"
  - [continue...]
```

## See Also

- [PROMPT.md](./PROMPT.md) - Main loop prompt
- [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) - Current task list
- [ARCHITECTURE.md](./ARCHITECTURE.md) - System architecture
- [CODE-GENERATION.md](./CODE-GENERATION.md) - Code generation details
