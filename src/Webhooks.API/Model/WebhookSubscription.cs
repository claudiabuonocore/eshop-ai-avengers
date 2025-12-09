namespace Webhooks.API.Model;

public class WebhookSubscription
{
    public int Id { get; set; }

    public WebhookType Type { get; set; }
    public DateTime Date { get; set; }
    [Required]
    public string DestUrl { get; set; }
    [SensitiveData(DataClassification.Credential, Notes = "Webhook authentication token - must never be logged")]
    public string Token { get; set; }
    [Required]
    [SensitiveData(DataClassification.PII, Notes = "User identifier")]
    public string UserId { get; set; }
}
