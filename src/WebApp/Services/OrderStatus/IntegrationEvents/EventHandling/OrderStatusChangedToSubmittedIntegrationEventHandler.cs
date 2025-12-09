using eShop.EventBus.Abstractions;
using eShop.Shared;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

public class OrderStatusChangedToSubmittedIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToSubmittedIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToSubmittedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({IntegrationEvent})", 
            @event.Id, SensitiveDataLogger.Redact(@event));
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
