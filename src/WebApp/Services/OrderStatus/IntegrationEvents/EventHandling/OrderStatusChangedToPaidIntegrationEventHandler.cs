using eShop.EventBus.Abstractions;
using eShop.Shared;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

public class OrderStatusChangedToPaidIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({IntegrationEvent})", 
            @event.Id, SensitiveDataLogger.Redact(@event));
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
