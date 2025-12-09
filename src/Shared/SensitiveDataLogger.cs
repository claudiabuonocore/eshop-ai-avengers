using System.Reflection;
using System.Text;
using System.Text.Json;

namespace eShop.Shared;

/// <summary>
/// Provides logging utilities that automatically redact sensitive data marked with SensitiveDataAttribute.
/// </summary>
/// <remarks>
/// This helper ensures compliance with FedRAMP requirements (AU-2, AU-6, SC-13) by preventing
/// sensitive information from appearing in application logs. It uses reflection to identify
/// properties marked with SensitiveDataAttribute and masks Financial and Credential data.
/// </remarks>
public static class SensitiveDataLogger
{
    private const string RedactedText = "***REDACTED***";
    private const string MaskedPattern = "****";

    /// <summary>
    /// Creates a safe representation of an object for logging by redacting sensitive properties.
    /// </summary>
    /// <param name="obj">The object to prepare for logging</param>
    /// <returns>A string representation with sensitive data redacted</returns>
    public static string Redact(object obj)
    {
        if (obj == null)
        {
            return "null";
        }

        var type = obj.GetType();

        // Handle primitive types and strings
        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal))
        {
            return obj.ToString() ?? "null";
        }

        var sb = new StringBuilder();
        sb.Append($"{type.Name} {{ ");

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var first = true;

        foreach (var property in properties)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;

            var sensitiveAttr = property.GetCustomAttribute<SensitiveDataAttribute>();
            var propertyValue = property.GetValue(obj);

            sb.Append($"{property.Name} = ");

            if (sensitiveAttr != null)
            {
                sb.Append(MaskValue(propertyValue ?? (object)"null", sensitiveAttr.Classification));
            }
            else
            {
                sb.Append(FormatValue(propertyValue ?? (object)"null"));
            }
        }

        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>
    /// Masks a value based on its data classification level.
    /// </summary>
    /// <param name="value">The value to mask</param>
    /// <param name="classification">The data classification level</param>
    /// <returns>A masked representation of the value</returns>
    private static string MaskValue(object value, DataClassification classification)
    {
        if (value == null)
        {
            return "null";
        }

        // Financial and Credential data must be completely redacted
        if (classification == DataClassification.Financial || classification == DataClassification.Credential)
        {
            return RedactedText;
        }

        // For PII and Regulated data, apply partial masking for debugging purposes
        var stringValue = value.ToString() ?? string.Empty;
        
        if (string.IsNullOrEmpty(stringValue))
        {
            return "\"\"";
        }

        if (stringValue.Length <= 4)
        {
            return MaskedPattern;
        }

        // Show first 2 characters for context, mask the rest
        return $"{stringValue.Substring(0, 2)}{MaskedPattern}";
    }

    /// <summary>
    /// Formats a non-sensitive value for display.
    /// </summary>
    private static string FormatValue(object value)
    {
        if (value == null)
        {
            return "null";
        }

        if (value is string str)
        {
            return $"\"{str}\"";
        }

        if (value is DateTime dt)
        {
            return dt.ToString("o");
        }

        return value.ToString() ?? "null";
    }

    /// <summary>
    /// Checks if a property has sensitive data that should be redacted.
    /// </summary>
    /// <param name="property">The property to check</param>
    /// <returns>True if the property contains sensitive data requiring redaction</returns>
    public static bool IsSensitive(PropertyInfo property)
    {
        var sensitiveAttr = property.GetCustomAttribute<SensitiveDataAttribute>();
        return sensitiveAttr != null && 
               (sensitiveAttr.Classification == DataClassification.Financial || 
                sensitiveAttr.Classification == DataClassification.Credential);
    }

    /// <summary>
    /// Creates a dictionary with sensitive values redacted for structured logging.
    /// </summary>
    /// <param name="obj">The object to convert</param>
    /// <returns>A dictionary with sensitive data redacted</returns>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context
    public static Dictionary<string, object?> ToSafeDictionary(object obj)
    {
        var result = new Dictionary<string, object?>();
#pragma warning restore CS8632

        if (obj == null)
        {
            return result;
        }

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var sensitiveAttr = property.GetCustomAttribute<SensitiveDataAttribute>();
            var propertyValue = property.GetValue(obj);

            if (sensitiveAttr != null)
            {
                // For sensitive data, store the masked string representation
                result[property.Name] = propertyValue != null 
                    ? MaskValue(propertyValue, sensitiveAttr.Classification) 
                    : null;
            }
            else
            {
                // For non-sensitive data, store the actual value (preserving nulls)
                result[property.Name] = propertyValue;
            }
        }

        return result;
    }
}
