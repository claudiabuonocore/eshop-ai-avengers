using eShop.Shared;

namespace eShop.Basket.UnitTests;

/// <summary>
/// Tests for SensitiveDataLogger to ensure PII and secrets are properly redacted from logs.
/// These tests guard against accidental logging of sensitive data in compliance with FedRAMP requirements.
/// </summary>
[TestClass]
public class SensitiveDataLoggerTests
{
    private class TestModel
    {
        public string PublicData { get; set; } = "public";

        [SensitiveData(DataClassification.PII)]
        public string PersonalInfo { get; set; } = "John Doe";

        [SensitiveData(DataClassification.Financial)]
        public string CreditCardNumber { get; set; } = "4532-1234-5678-9010";

        [SensitiveData(DataClassification.Credential)]
        public string ApiToken { get; set; } = "sk_test_abc123xyz789";
    }

    private class AddressModel
    {
        [SensitiveData(DataClassification.PII, Notes = "Street address")]
        public string Street { get; set; } = "123 Main St";

        [SensitiveData(DataClassification.PII, Notes = "City")]
        public string City { get; set; } = "Seattle";

        public string Country { get; set; } = "USA";
    }

    [TestMethod]
    public void Redact_FinancialData_ShouldBeCompletelyRedacted()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsTrue(redacted.Contains("CreditCardNumber = ***REDACTED***"), 
            "Financial data (credit card) must be completely redacted");
        Assert.IsFalse(redacted.Contains("4532-1234-5678-9010"), 
            "Raw credit card number must not appear in redacted output");
    }

    [TestMethod]
    public void Redact_CredentialData_ShouldBeCompletelyRedacted()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsTrue(redacted.Contains("ApiToken = ***REDACTED***"), 
            "Credential data (API token) must be completely redacted");
        Assert.IsFalse(redacted.Contains("sk_test_abc123xyz789"), 
            "Raw API token must not appear in redacted output");
    }

    [TestMethod]
    public void Redact_PIIData_ShouldBePartiallyMasked()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsTrue(redacted.Contains("PersonalInfo = Jo****"), 
            "PII data should be partially masked with first 2 characters visible");
        Assert.IsFalse(redacted.Contains("John Doe"), 
            "Full PII value must not appear in redacted output");
    }

    [TestMethod]
    public void Redact_PublicData_ShouldNotBeMasked()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsTrue(redacted.Contains("PublicData = \"public\""), 
            "Non-sensitive data should be logged as-is");
    }

    [TestMethod]
    public void Redact_NullObject_ShouldReturnNull()
    {
        // Act
        var redacted = SensitiveDataLogger.Redact(null);

        // Assert
        Assert.AreEqual("null", redacted);
    }

    [TestMethod]
    public void Redact_AddressWithPII_ShouldMaskPIIFields()
    {
        // Arrange
        var address = new AddressModel();

        // Act
        var redacted = SensitiveDataLogger.Redact(address);

        // Assert
        Assert.IsTrue(redacted.Contains("Street = 12****"), 
            "Street should be partially masked");
        Assert.IsTrue(redacted.Contains("City = Se****"), 
            "City should be partially masked");
        Assert.IsTrue(redacted.Contains("Country = \"USA\""), 
            "Non-sensitive country should not be masked");
        Assert.IsFalse(redacted.Contains("123 Main St"), 
            "Full street address must not appear");
        Assert.IsFalse(redacted.Contains("Seattle"), 
            "Full city name must not appear");
    }

    [TestMethod]
    public void ToSafeDictionary_ShouldRedactSensitiveFields()
    {
        // Arrange
        var model = new TestModel();

        // Act
        var dict = SensitiveDataLogger.ToSafeDictionary(model);

        // Assert
        Assert.AreEqual("***REDACTED***", dict["CreditCardNumber"], 
            "Financial data in dictionary should be redacted");
        Assert.AreEqual("***REDACTED***", dict["ApiToken"], 
            "Credential data in dictionary should be redacted");
        Assert.AreEqual("public", dict["PublicData"], 
            "Non-sensitive data in dictionary should not be redacted");
    }

    [TestMethod]
    public void ToSafeDictionary_NullObject_ShouldReturnEmptyDictionary()
    {
        // Act
        var dict = SensitiveDataLogger.ToSafeDictionary(null);

        // Assert
        Assert.IsNotNull(dict);
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    [DataRow("4532123456789010", "Financial data like credit cards")]
    [DataRow("sk_test_abc123xyz", "API tokens or secrets")]
    [DataRow("password123", "Passwords")]
    public void RegressionTest_RawSensitiveData_MustNotAppearInLogs(string sensitiveValue, string description)
    {
        // This test ensures that raw sensitive values never appear in log output
        // If this test fails, it means sensitive data is leaking into logs
        
        // Arrange
        var model = new TestModel
        {
            CreditCardNumber = sensitiveValue,
            ApiToken = sensitiveValue
        };

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsFalse(redacted.Contains(sensitiveValue), 
            $"SECURITY VIOLATION: {description} must not appear in logs. Found: {sensitiveValue}");
    }

    [TestMethod]
    public void Redact_ShortSensitiveValue_ShouldBeMasked()
    {
        // Arrange
        var model = new TestModel
        {
            PersonalInfo = "AB",
            CreditCardNumber = "12",
            ApiToken = "xyz"
        };

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert
        Assert.IsFalse(redacted.Contains("AB") && redacted.Contains("PersonalInfo"), 
            "Short PII values should be masked");
        Assert.IsTrue(redacted.Contains("***REDACTED***"), 
            "Financial and credential data should be completely redacted");
    }

    [TestMethod]
    public void Redact_EmptyOrNullSensitiveValues_ShouldHandleGracefully()
    {
        // Arrange
        var model = new TestModel
        {
            PersonalInfo = string.Empty,
            CreditCardNumber = null!,
            ApiToken = ""
        };

        // Act
        var redacted = SensitiveDataLogger.Redact(model);

        // Assert - Should not throw and should handle null/empty gracefully
        Assert.IsNotNull(redacted);
        Assert.IsTrue(redacted.Contains("PersonalInfo"));
        Assert.IsTrue(redacted.Contains("CreditCardNumber"));
        Assert.IsTrue(redacted.Contains("ApiToken"));
    }
}
