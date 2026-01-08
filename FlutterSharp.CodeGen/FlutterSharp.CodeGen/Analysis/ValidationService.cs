using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FlutterSharp.CodeGen.Models;
using FlutterSharp.CodeGen.TypeMapping;

namespace FlutterSharp.CodeGen.Analysis;

/// <summary>
/// Provides validation services for code generation to catch issues before generation begins.
/// </summary>
public class ValidationService
{
	private readonly DartToCSharpMapper _typeMapper;
	private readonly Action<string>? _logWarning;
	private readonly Action<string>? _logInfo;

	// C# reserved keywords that need escaping
	private static readonly HashSet<string> CSharpReservedKeywords = new(StringComparer.Ordinal)
	{
		"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
		"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
		"enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
		"foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
		"long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
		"private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
		"short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
		"true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
		"virtual", "void", "volatile", "while"
	};

	// Contextual keywords that may need escaping in certain contexts
	private static readonly HashSet<string> CSharpContextualKeywords = new(StringComparer.Ordinal)
	{
		"add", "alias", "ascending", "async", "await", "by", "descending", "dynamic", "equals",
		"from", "get", "global", "group", "into", "join", "let", "nameof", "on", "orderby",
		"partial", "remove", "select", "set", "value", "var", "when", "where", "yield"
	};

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationService"/> class.
	/// </summary>
	/// <param name="typeMapper">The type mapper for validating type mappings.</param>
	/// <param name="logWarning">Optional callback for logging warnings.</param>
	/// <param name="logInfo">Optional callback for logging info messages.</param>
	public ValidationService(DartToCSharpMapper typeMapper, Action<string>? logWarning = null, Action<string>? logInfo = null)
	{
		_typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
		_logWarning = logWarning;
		_logInfo = logInfo;
	}

	/// <summary>
	/// Validates a package definition before code generation.
	/// </summary>
	/// <param name="package">The package definition to validate.</param>
	/// <returns>A validation result containing any errors and warnings.</returns>
	public ValidationResult ValidatePackage(PackageDefinition package)
	{
		var result = new ValidationResult();

		if (package == null)
		{
			result.Errors.Add(new ValidationError("Package", "Package definition is null"));
			return result;
		}

		// Validate package metadata
		if (string.IsNullOrWhiteSpace(package.Name))
		{
			result.Warnings.Add(new ValidationWarning("Package", "Package name is empty"));
		}

		// Validate widgets
		_logInfo?.Invoke($"Validating {package.Widgets.Count} widgets...");
		foreach (var widget in package.Widgets)
		{
			ValidateWidget(widget, result);
		}

		// Validate types
		_logInfo?.Invoke($"Validating {package.Types.Count} types...");
		foreach (var type in package.Types)
		{
			ValidateType(type, result);
		}

		// Validate enums
		_logInfo?.Invoke($"Validating {package.Enums.Count} enums...");
		foreach (var enumDef in package.Enums)
		{
			ValidateEnum(enumDef, result);
		}

		// Check for duplicate names
		ValidateDuplicateNames(package, result);

		return result;
	}

	/// <summary>
	/// Validates output paths are writable before generation begins.
	/// </summary>
	/// <param name="outputPaths">The output paths to validate.</param>
	/// <returns>A validation result containing any errors.</returns>
	public ValidationResult ValidateOutputPaths(params string[] outputPaths)
	{
		var result = new ValidationResult();

		foreach (var path in outputPaths)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				result.Errors.Add(new ValidationError("OutputPath", "Output path is null or empty"));
				continue;
			}

			try
			{
				// Try to create directory if it doesn't exist
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				// Test write access by creating and deleting a temp file
				var testFile = Path.Combine(path, $".validation_test_{Guid.NewGuid():N}");
				File.WriteAllText(testFile, "test");
				File.Delete(testFile);
			}
			catch (UnauthorizedAccessException)
			{
				result.Errors.Add(new ValidationError("OutputPath", $"No write access to output path: {path}"));
			}
			catch (IOException ex)
			{
				result.Errors.Add(new ValidationError("OutputPath", $"Cannot write to output path '{path}': {ex.Message}"));
			}
			catch (Exception ex)
			{
				result.Errors.Add(new ValidationError("OutputPath", $"Invalid output path '{path}': {ex.Message}"));
			}
		}

		return result;
	}

	/// <summary>
	/// Validates a single widget definition.
	/// </summary>
	private void ValidateWidget(WidgetDefinition widget, ValidationResult result)
	{
		var context = $"Widget '{widget.Name}'";

		// Required fields
		if (string.IsNullOrWhiteSpace(widget.Name))
		{
			result.Errors.Add(new ValidationError(context, "Widget name is empty"));
			return; // Can't continue without a name
		}

		// Validate name is a valid C# identifier
		if (!IsValidCSharpIdentifier(widget.Name))
		{
			result.Errors.Add(new ValidationError(context, $"Widget name '{widget.Name}' is not a valid C# identifier"));
		}

		// Check for reserved keywords
		if (CSharpReservedKeywords.Contains(widget.Name))
		{
			result.Warnings.Add(new ValidationWarning(context, $"Widget name '{widget.Name}' is a C# reserved keyword and will need escaping"));
		}

		// Validate widget type
		if (widget.Type == WidgetType.Unknown)
		{
			result.Warnings.Add(new ValidationWarning(context, "Widget type is Unknown, may generate incorrect base class"));
		}

		// Validate properties
		foreach (var property in widget.Properties)
		{
			ValidateProperty(property, context, result);
		}

		// Check for conflicting child properties
		if (widget.HasSingleChild && widget.HasMultipleChildren)
		{
			result.Warnings.Add(new ValidationWarning(context, "Widget has both HasSingleChild and HasMultipleChildren set to true"));
		}
	}

	/// <summary>
	/// Validates a single type definition.
	/// </summary>
	private void ValidateType(TypeDefinition type, ValidationResult result)
	{
		var context = $"Type '{type.Name}'";

		if (string.IsNullOrWhiteSpace(type.Name))
		{
			result.Errors.Add(new ValidationError(context, "Type name is empty"));
			return;
		}

		if (!IsValidCSharpIdentifier(type.Name))
		{
			result.Errors.Add(new ValidationError(context, $"Type name '{type.Name}' is not a valid C# identifier"));
		}

		if (CSharpReservedKeywords.Contains(type.Name))
		{
			result.Warnings.Add(new ValidationWarning(context, $"Type name '{type.Name}' is a C# reserved keyword"));
		}

		foreach (var property in type.Properties)
		{
			ValidateProperty(property, context, result);
		}
	}

	/// <summary>
	/// Validates a single enum definition.
	/// </summary>
	private void ValidateEnum(EnumDefinition enumDef, ValidationResult result)
	{
		var context = $"Enum '{enumDef.Name}'";

		if (string.IsNullOrWhiteSpace(enumDef.Name))
		{
			result.Errors.Add(new ValidationError(context, "Enum name is empty"));
			return;
		}

		if (!IsValidCSharpIdentifier(enumDef.Name))
		{
			result.Errors.Add(new ValidationError(context, $"Enum name '{enumDef.Name}' is not a valid C# identifier"));
		}

		if (enumDef.Values == null || enumDef.Values.Count == 0)
		{
			result.Warnings.Add(new ValidationWarning(context, "Enum has no values"));
			return;
		}

		foreach (var value in enumDef.Values)
		{
			if (string.IsNullOrWhiteSpace(value.Name))
			{
				result.Errors.Add(new ValidationError(context, "Enum value has empty name"));
				continue;
			}

			if (!IsValidCSharpIdentifier(value.Name))
			{
				result.Errors.Add(new ValidationError(context, $"Enum value '{value.Name}' is not a valid C# identifier"));
			}

			if (CSharpReservedKeywords.Contains(value.Name))
			{
				result.Warnings.Add(new ValidationWarning(context, $"Enum value '{value.Name}' is a C# reserved keyword"));
			}
		}
	}

	/// <summary>
	/// Validates a single property definition.
	/// </summary>
	private void ValidateProperty(PropertyDefinition property, string parentContext, ValidationResult result)
	{
		var context = $"{parentContext} -> Property '{property.Name}'";

		if (string.IsNullOrWhiteSpace(property.Name))
		{
			result.Errors.Add(new ValidationError(context, "Property name is empty"));
			return;
		}

		if (!IsValidCSharpIdentifier(property.Name))
		{
			// This is a warning, not error, because the generator can escape the name
			result.Warnings.Add(new ValidationWarning(context, $"Property name '{property.Name}' needs escaping for C#"));
		}

		if (string.IsNullOrWhiteSpace(property.DartType))
		{
			result.Warnings.Add(new ValidationWarning(context, "Property has empty Dart type"));
		}
		else
		{
			// Try to map the type and check if it maps to InvalidType
			var mappedType = _typeMapper.MapType(property.DartType, property.Name);
			if (mappedType == "InvalidType" || mappedType.Contains("InvalidType"))
			{
				result.Warnings.Add(new ValidationWarning(context, $"Dart type '{property.DartType}' maps to InvalidType"));
			}
		}
	}

	/// <summary>
	/// Checks for duplicate names that would cause conflicts.
	/// </summary>
	private void ValidateDuplicateNames(PackageDefinition package, ValidationResult result)
	{
		// Check for duplicate widget names
		var widgetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var widget in package.Widgets)
		{
			if (!widgetNames.Add(widget.Name))
			{
				result.Errors.Add(new ValidationError($"Widget '{widget.Name}'", "Duplicate widget name (case-insensitive)"));
			}
		}

		// Check for duplicate enum names
		var enumNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var enumDef in package.Enums)
		{
			if (!enumNames.Add(enumDef.Name))
			{
				result.Errors.Add(new ValidationError($"Enum '{enumDef.Name}'", "Duplicate enum name (case-insensitive)"));
			}
		}

		// Check for widget/enum name collisions
		foreach (var widget in package.Widgets)
		{
			if (enumNames.Contains(widget.Name))
			{
				result.Warnings.Add(new ValidationWarning($"Widget '{widget.Name}'", "Widget name conflicts with enum name"));
			}
		}
	}

	/// <summary>
	/// Checks if a string is a valid C# identifier.
	/// </summary>
	private static bool IsValidCSharpIdentifier(string name)
	{
		if (string.IsNullOrEmpty(name))
			return false;

		// Must start with letter or underscore
		if (!char.IsLetter(name[0]) && name[0] != '_')
			return false;

		// Rest must be letters, digits, or underscores
		for (int i = 1; i < name.Length; i++)
		{
			if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
				return false;
		}

		return true;
	}
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
	/// <summary>
	/// Gets the list of validation errors (blocking issues).
	/// </summary>
	public List<ValidationError> Errors { get; } = new();

	/// <summary>
	/// Gets the list of validation warnings (non-blocking issues).
	/// </summary>
	public List<ValidationWarning> Warnings { get; } = new();

	/// <summary>
	/// Gets a value indicating whether validation passed (no errors).
	/// </summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>
	/// Gets a value indicating whether there are any warnings.
	/// </summary>
	public bool HasWarnings => Warnings.Count > 0;

	/// <summary>
	/// Gets the total number of issues (errors + warnings).
	/// </summary>
	public int TotalIssues => Errors.Count + Warnings.Count;

	/// <summary>
	/// Merges another validation result into this one.
	/// </summary>
	public void Merge(ValidationResult other)
	{
		if (other == null) return;
		Errors.AddRange(other.Errors);
		Warnings.AddRange(other.Warnings);
	}

	/// <summary>
	/// Returns a summary of the validation result.
	/// </summary>
	public override string ToString()
	{
		return $"Validation: {Errors.Count} errors, {Warnings.Count} warnings";
	}
}

/// <summary>
/// Represents a validation error (blocking issue).
/// </summary>
public record ValidationError(string Context, string Message)
{
	public override string ToString() => $"ERROR [{Context}]: {Message}";
}

/// <summary>
/// Represents a validation warning (non-blocking issue).
/// </summary>
public record ValidationWarning(string Context, string Message)
{
	public override string ToString() => $"WARNING [{Context}]: {Message}";
}

/// <summary>
/// Exception thrown when validation fails with blocking errors.
/// </summary>
public class ValidationException : Exception
{
	/// <summary>
	/// Gets the validation result that caused this exception.
	/// </summary>
	public ValidationResult ValidationResult { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationException"/> class.
	/// </summary>
	public ValidationException(ValidationResult result)
		: base($"Validation failed with {result.Errors.Count} error(s)")
	{
		ValidationResult = result;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationException"/> class.
	/// </summary>
	public ValidationException(string message, ValidationResult result)
		: base(message)
	{
		ValidationResult = result;
	}
}
