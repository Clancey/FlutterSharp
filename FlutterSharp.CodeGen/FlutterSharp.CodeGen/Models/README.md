# FlutterSharp Code Generator Models

This directory contains the model classes that represent the intermediate representation of Flutter widgets and types extracted from Dart code.

## Model Files

### Core Models

- **PackageDefinition.cs** - Represents metadata about a Dart/Flutter package including all discovered widgets, types, and enums.

- **WidgetDefinition.cs** - Represents a Flutter widget with all its properties, constructors, and metadata. Includes the `WidgetType` enum for categorizing widgets.

- **TypeDefinition.cs** - Represents a custom class/type definition from Dart that isn't a widget (e.g., EdgeInsets, Color, TextStyle).

- **EnumDefinition.cs** - Represents an enum type and its values (e.g., Alignment, Axis, CrossAxisAlignment).

### Supporting Models

- **PropertyDefinition.cs** - Represents a property or parameter in a widget, type, or constructor. Includes information about nullability, default values, and type mappings.

- **ConstructorDefinition.cs** - Represents constructor information including parameters, whether it's const/factory, and deprecation status.

## Model Hierarchy

```
PackageDefinition
├── Widgets (List<WidgetDefinition>)
│   ├── Properties (List<PropertyDefinition>)
│   └── Constructors (List<ConstructorDefinition>)
│       └── Parameters (List<PropertyDefinition>)
├── Types (List<TypeDefinition>)
│   ├── Properties (List<PropertyDefinition>)
│   └── Constructors (List<ConstructorDefinition>)
└── Enums (List<EnumDefinition>)
    └── Values (List<EnumValueDefinition>)
```

## Usage

These models are designed to be deserialized from JSON output produced by the Dart analyzer script and then used as input for the C# code generation templates (Scriban).

### Example JSON Structure

```json
{
  "name": "flutter",
  "version": "3.0.0",
  "widgets": [
    {
      "name": "Container",
      "namespace": "package:flutter/widgets.dart",
      "baseClass": "SingleChildRenderObjectWidget",
      "type": 3,
      "properties": [
        {
          "name": "alignment",
          "dartType": "AlignmentGeometry",
          "csharpType": "Alignment",
          "isNullable": true,
          "isNamed": true
        }
      ],
      "constructors": [
        {
          "name": "",
          "isConst": true,
          "parameters": [...]
        }
      ]
    }
  ]
}
```

## Features

- **Immutability**: All models use C# records for immutable value semantics
- **Nullable Reference Types**: Full support for nullable reference types
- **XML Documentation**: Comprehensive XML comments for IntelliSense
- **ToString Overrides**: Helpful debugging output for all models
- **Metadata Support**: Extensible metadata dictionaries for custom attributes

## Design Principles

1. **Separation of Concerns**: Models only represent data, no logic
2. **Type Safety**: Strongly typed properties with appropriate nullability
3. **Extensibility**: Metadata dictionaries allow for future extensions
4. **Documentation**: Rich metadata about documentation, deprecation, etc.
5. **Debugging**: ToString() overrides provide clear, readable output
