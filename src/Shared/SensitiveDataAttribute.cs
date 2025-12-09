namespace eShop.Shared;

/// <summary>
/// Marks a property as containing sensitive data that requires special handling for logging,
/// masking, and retention policies according to data classification and compliance requirements.
/// </summary>
/// <remarks>
/// This attribute is used to identify Personally Identifiable Information (PII) and other
/// sensitive data that must be handled according to FedRAMP requirements (PL-2, MP-5, AU-2).
/// Fields marked with this attribute should be:
/// - Masked or redacted in logs
/// - Subject to data retention and deletion policies
/// - Handled according to data minimization principles
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Gets the classification level of the sensitive data.
    /// </summary>
    public DataClassification Classification { get; }

    /// <summary>
    /// Gets additional notes about the sensitivity or handling requirements.
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// Initializes a new instance of the SensitiveDataAttribute.
    /// </summary>
    /// <param name="classification">The data classification level.</param>
    public SensitiveDataAttribute(DataClassification classification)
    {
        Classification = classification;
    }
}

/// <summary>
/// Defines data classification levels for sensitive information.
/// </summary>
public enum DataClassification
{
    /// <summary>
    /// Personally Identifiable Information (PII) such as names, email addresses, phone numbers.
    /// Requires masking in logs and subject to retention policies.
    /// </summary>
    PII = 1,

    /// <summary>
    /// Financial or payment information such as credit card numbers, security codes, account details.
    /// Requires strict masking and encryption. Must never appear in logs.
    /// </summary>
    Financial = 2,

    /// <summary>
    /// Authentication credentials such as passwords, tokens, API keys.
    /// Must never be logged or stored in plain text.
    /// </summary>
    Credential = 3,

    /// <summary>
    /// Regulated data subject to specific compliance requirements (e.g., FedRAMP, GDPR).
    /// Requires special handling according to regulatory frameworks.
    /// </summary>
    Regulated = 4
}
