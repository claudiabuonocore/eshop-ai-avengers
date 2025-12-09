namespace eShop.Identity.API.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [SensitiveData(DataClassification.Financial, Notes = "Credit card number - must be masked in logs")]
        public string CardNumber { get; set; }
        [Required]
        [SensitiveData(DataClassification.Financial, Notes = "Card security code - must never appear in logs")]
        public string SecurityNumber { get; set; }
        [Required]
        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        [SensitiveData(DataClassification.Financial, Notes = "Card expiration date")]
        public string Expiration { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "Cardholder name")]
        public string CardHolderName { get; set; }
        public int CardType { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "Street address")]
        public string Street { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "City")]
        public string City { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "State or province")]
        public string State { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "Country")]
        public string Country { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "Postal/ZIP code")]
        public string ZipCode { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "User first name")]
        public string Name { get; set; }
        [Required]
        [SensitiveData(DataClassification.PII, Notes = "User last name")]
        public string LastName { get; set; }
    }
}
