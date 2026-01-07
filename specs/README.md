# FlutterSharp Specifications

This directory contains the complete technical specification for FlutterSharp - a C#/.NET MAUI to Dart/Flutter interoperability layer.

## Specification Documents

| Document | Description |
|----------|-------------|
| [OVERVIEW.md](./OVERVIEW.md) | Project overview, goals, and core concepts |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | System architecture, components, and data flow |
| [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) | FFI memory sharing protocol and message format |
| [CODE-GENERATION.md](./CODE-GENERATION.md) | Code generation pipeline and templates |
| [TYPE-MAPPING.md](./TYPE-MAPPING.md) | Dart to C# type mappings and FFI types |
| [WIDGET-BINDING.md](./WIDGET-BINDING.md) | C# widget binding patterns and lifecycle |
| [CALLBACKS-EVENTS.md](./CALLBACKS-EVENTS.md) | Callback registration and event handling |
| [ROADMAP.md](./ROADMAP.md) | Implementation phases and future work |

## Autonomous Agent Documents

| Document | Description |
|----------|-------------|
| [AGENTS.md](./AGENTS.md) | Agent configuration and subagent usage patterns |
| [PROMPT.md](./PROMPT.md) | Main loop prompt for autonomous execution |
| [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) | Active task list (fix_plan.md equivalent) |

## Quick Links

### For Developers Using FlutterSharp

- Start with [OVERVIEW.md](./OVERVIEW.md) to understand what FlutterSharp does
- Read [WIDGET-BINDING.md](./WIDGET-BINDING.md) to learn how to use widgets
- See [CALLBACKS-EVENTS.md](./CALLBACKS-EVENTS.md) for handling user interactions

### For Contributors

- Read [ARCHITECTURE.md](./ARCHITECTURE.md) to understand the system design
- Study [CODE-GENERATION.md](./CODE-GENERATION.md) to work on the generator
- Check [TYPE-MAPPING.md](./TYPE-MAPPING.md) when adding type support
- Review [ROADMAP.md](./ROADMAP.md) to find areas to contribute

### For Deep Technical Understanding

- [INTEROP-PROTOCOL.md](./INTEROP-PROTOCOL.md) explains the FFI protocol in detail
- [TYPE-MAPPING.md](./TYPE-MAPPING.md) covers all type conversions
- [ARCHITECTURE.md](./ARCHITECTURE.md) describes memory management

### For Autonomous Agent Operation

- [PROMPT.md](./PROMPT.md) is loaded each loop iteration
- [AGENTS.md](./AGENTS.md) defines subagent patterns
- [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) tracks current tasks

## Key Concepts

### Memory-Shared FFI

FlutterSharp uses a novel approach where C# and Dart share the same memory:

```
C# Widget → C# Struct (pinned) → Pointer → Dart reads struct → Flutter Widget
```

No serialization is needed for struct data - Dart reads directly from C# memory.

### Code Generation

The generator analyzes Flutter packages and produces:

- **C# Widgets**: Type-safe wrapper classes
- **C# Structs**: FFI-compatible memory layouts
- **Dart Structs**: Matching FFI definitions
- **Dart Parsers**: Widget builders from structs

### Bidirectional Communication

- **C# → Dart**: Widget updates via memory pointers
- **Dart → C#**: Events via MethodChannel with action IDs

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET MAUI Application                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   C# Widget API (Container, Text, Column, etc.)       │  │
│  └───────────────────────────────────────────────────────┘  │
│                            │                                │
│                            ▼                                │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   FlutterManager + CallbackRegistry                   │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────│────────────────────────────────┘
                             │ MethodChannel + Memory Pointers
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Flutter Engine                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   MauiRenderer + DynamicWidgetBuilder                 │  │
│  └───────────────────────────────────────────────────────┘  │
│                            │                                │
│                            ▼                                │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   Native Flutter Widget Tree                          │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Current Status

- ✅ Code generation architecture complete
- ✅ 400+ widgets generated
- ✅ Type mapping system working
- ⚠️ Some compilation errors to fix
- 🔄 Runtime integration in progress

See [ROADMAP.md](./ROADMAP.md) for detailed status and next steps.

## Version

Specification Version: 1.0
Last Updated: January 2025
