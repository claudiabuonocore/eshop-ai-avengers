using eShop.Basket.API.Repositories;
using eShop.Basket.API.IntegrationEvents.EventHandling.Events;
using eShop.Shared;

namespace eShop.Basket.API.IntegrationEvents.EventHandling;

public class OrderStartedIntegrationEventHandler(
    IBasketRepository repository,
    ILogger<OrderStartedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({IntegrationEvent})", 
            @event.Id, SensitiveDataLogger.Redact(@event));

        await repository.DeleteBasketAsync(@event.UserId);
    }
}
