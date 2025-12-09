# Data Classification and Sensitive Field Documentation

## Overview

This document describes the data classification framework implemented in the eShop application to support data minimization, PII redaction, and safe logging practices as required by FedRAMP compliance (PL-2, MP-5, AU-2).

## Data Classification Levels

The eShop application uses four levels of data classification to categorize sensitive information:

### 1. PII (Personally Identifiable Information)
**Classification Level:** `DataClassification.PII`

Information that can be used to identify, contact, or locate a specific individual.

**Handling Requirements:**
- Must be masked or redacted in application logs
- Subject to data retention and deletion policies
- Should be minimized in data collection
- Requires appropriate access controls

**Examples:**
- User names (first name, last name)
- Email addresses (inherited from IdentityUser)
- Phone numbers (inherited from IdentityUser)
- Physical addresses (street, city, state, country, postal code)
- User identifiers
- Buyer/Customer IDs

### 2. Financial
**Classification Level:** `DataClassification.Financial`

Financial or payment-related information that requires strict protection.

**Handling Requirements:**
- Must be masked or fully redacted in logs (never log in plain text)
- Requires encryption at rest and in transit
- Subject to PCI-DSS compliance requirements
- Must have the highest level of access controls
- Should be purged according to strict retention policies

**Examples:**
- Credit card numbers
- Card security codes (CVV/CVC)
- Card expiration dates
- Bank account information

### 3. Credential
**Classification Level:** `DataClassification.Credential`

Authentication credentials and security tokens.

**Handling Requirements:**
- Must NEVER be logged or displayed
- Must be stored using secure hashing (passwords) or encryption (tokens)
- Should be rotated regularly
- Subject to immediate revocation when compromised
- Requires the strictest access controls

**Examples:**
- Passwords (not tagged as they should never be stored in plain text)
- API keys
- Authentication tokens
- Webhook authentication tokens

### 4. Regulated
**Classification Level:** `DataClassification.Regulated`

Data subject to specific regulatory compliance requirements.

**Handling Requirements:**
- Must be handled according to applicable regulatory frameworks (FedRAMP, GDPR, HIPAA, etc.)
- Subject to audit logging requirements
- May require data residency controls
- Needs special handling during data transfer and storage

**Examples:**
- Any data covered by specific regulations
- Data with geographic restrictions
- Healthcare information (if applicable)
- Government-regulated data

## Tagged Entities and Fields

### Identity.API - ApplicationUser

Located in: `src/Identity.API/Models/ApplicationUser.cs`

| Property | Classification | Notes |
|----------|---------------|-------|
| CardNumber | Financial | Credit card number - must be masked in logs |
| SecurityNumber | Financial | Card security code - must never appear in logs |
| Expiration | Financial | Card expiration date |
| CardHolderName | PII | Cardholder name |
| Street | PII | Street address |
| City | PII | City |
| State | PII | State or province |
| Country | PII | Country |
| ZipCode | PII | Postal/ZIP code |
| Name | PII | User first name |
| LastName | PII | User last name |
| Email | PII | (Inherited from IdentityUser) |
| PhoneNumber | PII | (Inherited from IdentityUser) |

### Ordering.Domain - Buyer

Located in: `src/Ordering.Domain/AggregatesModel/BuyerAggregate/Buyer.cs`

| Property | Classification | Notes |
|----------|---------------|-------|
| IdentityGuid | PII | User identity GUID |
| Name | PII | Buyer name |

### Ordering.Domain - PaymentMethod

Located in: `src/Ordering.Domain/AggregatesModel/BuyerAggregate/PaymentMethod.cs`

| Field | Classification | Notes |
|-------|---------------|-------|
| _cardNumber | Financial | Credit card number - must be masked in logs |
| _securityNumber | Financial | Card security code - must never appear in logs |
| _cardHolderName | PII | Cardholder name |
| _expiration | Financial | Card expiration date |

### Ordering.Domain - Address

Located in: `src/Ordering.Domain/AggregatesModel/OrderAggregate/Address.cs`

| Property | Classification | Notes |
|----------|---------------|-------|
| Street | PII | Street address |
| City | PII | City |
| State | PII | State or province |
| Country | PII | Country |
| ZipCode | PII | Postal/ZIP code |

### Basket.API - CustomerBasket

Located in: `src/Basket.API/Model/CustomerBasket.cs`

| Property | Classification | Notes |
|----------|---------------|-------|
| BuyerId | PII | Buyer/Customer identifier |

### Webhooks.API - WebhookSubscription

Located in: `src/Webhooks.API/Model/WebhookSubscription.cs`

| Property | Classification | Notes |
|----------|---------------|-------|
| Token | Credential | Webhook authentication token - must never be logged |
| UserId | PII | User identifier |

## Implementation Details

### SensitiveDataAttribute

The `SensitiveDataAttribute` is a custom attribute used to mark properties and fields containing sensitive data. This attribute can be used by logging frameworks, serialization libraries, and other infrastructure code to automatically handle sensitive data appropriately.

**Location:** `src/Shared/SensitiveDataAttribute.cs`

**Usage:**
```csharp
[SensitiveData(DataClassification.PII, Notes = "User email address")]
public string Email { get; set; }

[SensitiveData(DataClassification.Financial, Notes = "Must never appear in logs")]
public string CardNumber { get; set; }

[SensitiveData(DataClassification.Credential, Notes = "Authentication token")]
public string Token { get; set; }
```

### Recommended Handling Practices

#### For Logging
1. **Financial and Credential data:** Never log these values
2. **PII data:** Mask or redact (e.g., show only last 4 digits of phone, first letter of name)
3. **Regulated data:** Follow specific regulatory requirements

#### For Storage
1. **Credential data:** Hash passwords, encrypt tokens
2. **Financial data:** Encrypt at rest, use tokenization when possible
3. **PII data:** Encrypt sensitive PII, implement proper access controls

#### For Data Retention
1. Define retention periods for each classification level
2. Implement automated deletion after retention period expires
3. Provide user-initiated deletion capabilities (right to be forgotten)
4. Maintain audit logs of sensitive data access and deletion

#### For Data Minimization
1. Only collect sensitive data that is absolutely necessary
2. Avoid storing sensitive data in logs, caches, or temporary storage
3. Redact sensitive data when displaying in UI or debug output
4. Consider tokenization or pseudonymization for analytics

## Compliance Mapping

### FedRAMP Controls

- **PL-2 (System Security Plan):** This classification system supports the security plan by identifying sensitive data elements
- **MP-5 (Media Protection):** Classification drives appropriate media protection controls for data at rest
- **AU-2 (Audit Events):** Sensitive data is identified to ensure proper audit logging without exposing sensitive information

### Additional Compliance Considerations

- **GDPR:** PII classification supports GDPR Article 5 (data minimization) and Article 17 (right to erasure)
- **PCI-DSS:** Financial classification aligns with PCI-DSS requirements for cardholder data protection
- **SOC 2:** Classification supports SOC 2 security principle for protecting sensitive information

## Future Enhancements

1. **Automatic Masking:** Implement logging interceptors that automatically mask sensitive data based on the attribute
2. **Data Loss Prevention (DLP):** Integrate with DLP tools to monitor and prevent unauthorized sensitive data exposure
3. **Compliance Reporting:** Generate reports showing all sensitive data elements and their handling
4. **Runtime Validation:** Add runtime checks to ensure sensitive data is not being logged or exposed inappropriately
5. **Data Discovery:** Automated tools to discover and tag additional sensitive fields
6. **Encryption Policies:** Automatic encryption enforcement for fields marked as Financial or Credential

## References

- [FedRAMP Security Controls](https://www.fedramp.gov/assets/resources/documents/FedRAMP_Security_Controls_Baseline.xlsx)
- [NIST Special Publication 800-53](https://csrc.nist.gov/publications/detail/sp/800-53/rev-5/final)
- [GDPR Data Protection Principles](https://gdpr-info.eu/art-5-gdpr/)
- [PCI DSS Requirements](https://www.pcisecuritystandards.org/document_library)

## Contact

For questions or concerns about data classification or sensitive data handling, please contact the security team or create an issue in the repository.
