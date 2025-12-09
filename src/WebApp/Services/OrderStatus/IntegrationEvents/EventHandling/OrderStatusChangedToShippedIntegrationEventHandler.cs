using eShop.EventBus.Abstractions;
using eShop.Shared;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

public class OrderStatusChangedToShippedIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({IntegrationEvent})", 
            @event.Id, SensitiveDataLogger.Redact(@event));
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
