namespace eShop.Basket.API.Model;

public class CustomerBasket
{
    [SensitiveData(DataClassification.PII, Notes = "Buyer/Customer identifier")]
    public string BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = [];

    public CustomerBasket() { }

    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }
}
