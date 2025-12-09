using eShop.EventBus.Abstractions;
using eShop.Shared;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

public class OrderStatusChangedToStockConfirmedIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({IntegrationEvent})", 
            @event.Id, SensitiveDataLogger.Redact(@event));
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
