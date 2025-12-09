using eShop.EventBus.Events;
using eShop.Shared;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; }
    public string OrderStatus { get; }
    
    [SensitiveData(DataClassification.PII, Notes = "Buyer name")]
    public string BuyerName { get; }
    
    [SensitiveData(DataClassification.PII, Notes = "Buyer identity GUID")]
    public string BuyerIdentityGuid { get; }

    public OrderStatusChangedToShippedIntegrationEvent(
        int orderId, string orderStatus, string buyerName, string buyerIdentityGuid)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}

