# RabbitMQ Integration Events - Technical Analysis

## Executive Summary

The eShop application uses RabbitMQ as a message broker for **event-driven communication** between microservices. There are **17+ distinct integration events** facilitating cross-service coordination, primarily for the **order lifecycle workflow**.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        RabbitMQ Exchange: "eshop_event_bus"                 │
│                              (Direct Exchange)                              │
└─────────────────────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│  Ordering.API │  │  Catalog.API  │  │  Basket.API   │  │    WebApp     │
│    (Queue)    │  │    (Queue)    │  │    (Queue)    │  │    (Queue)    │
└───────────────┘  └───────────────┘  └───────────────┘  └───────────────┘

┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│PaymentProcessor│ │OrderProcessor │  │ Webhooks.API  │
│    (Queue)    │  │    (Queue)    │  │    (Queue)    │
└───────────────┘  └───────────────┘  └───────────────┘
```

---

## Complete Event Handoff Matrix

### Order Lifecycle Flow (Happy Path)

| Step | Event | Publisher | Subscriber(s) | Action |
|------|-------|-----------|---------------|--------|
| 1 | `OrderStartedIntegrationEvent` | Ordering.API | Basket.API | Clears user's shopping basket |
| 2 | `OrderStatusChangedToAwaitingValidationIntegrationEvent` | Ordering.API | Catalog.API | Validates stock availability |
| 3a | `OrderStockConfirmedIntegrationEvent` | Catalog.API | Ordering.API | Updates order to StockConfirmed |
| 3b | `OrderStockRejectedIntegrationEvent` | Catalog.API | Ordering.API | Cancels order (insufficient stock) |
| 4 | `GracePeriodConfirmedIntegrationEvent` | OrderProcessor | Ordering.API | Triggers stock confirmation after grace period |
| 5 | `OrderStatusChangedToStockConfirmedIntegrationEvent` | Ordering.API | PaymentProcessor, WebApp | Triggers payment processing |
| 6a | `OrderPaymentSucceededIntegrationEvent` | PaymentProcessor | Ordering.API | Updates order to Paid status |
| 6b | `OrderPaymentFailedIntegrationEvent` | PaymentProcessor | Ordering.API | Handles payment failure |
| 7 | `OrderStatusChangedToPaidIntegrationEvent` | Ordering.API | Catalog.API, WebApp, Webhooks.API | Deducts inventory, notifies user |
| 8 | `OrderStatusChangedToShippedIntegrationEvent` | Ordering.API | WebApp, Webhooks.API | Notifies user of shipment |
| 9 | `OrderStatusChangedToCancelledIntegrationEvent` | Ordering.API | WebApp | Notifies user of cancellation |

---

## Detailed Handoff Flows

### 1. Order Submission → Basket Cleanup

```
┌─────────────┐    OrderStartedIntegrationEvent    ┌─────────────┐
│ Ordering.API│ ──────────────────────────────────►│ Basket.API  │
└─────────────┘                                    └─────────────┘
                                                         │
                                                         ▼
                                                   DeleteBasket()
```

**Source**: `src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler.cs`  
**Handler**: `src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHandler.cs`

---

### 2. Stock Validation Handoff

```
┌─────────────┐  OrderStatusChangedToAwaitingValidation  ┌─────────────┐
│ Ordering.API│ ────────────────────────────────────────►│ Catalog.API │
└─────────────┘                                          └─────────────┘
                                                               │
                                                   ┌───────────┴───────────┐
                                                   ▼                       ▼
                                          OrderStockConfirmed    OrderStockRejected
                                                   │                       │
                                                   └───────────┬───────────┘
                                                               ▼
                                                        ┌─────────────┐
                                                        │ Ordering.API│
                                                        └─────────────┘
```

**Logic**: Catalog.API checks `AvailableStock >= Units` for each item:
- **All items in stock** → `OrderStockConfirmedIntegrationEvent`
- **Any item out of stock** → `OrderStockRejectedIntegrationEvent`

---

### 3. Payment Processing Handoff

```
┌─────────────┐  OrderStatusChangedToStockConfirmed  ┌─────────────────┐
│ Ordering.API│ ───────────────────────────────────►│ PaymentProcessor│
└─────────────┘                                      └─────────────────┘
                                                            │
                                                ┌───────────┴───────────┐
                                                ▼                       ▼
                                      OrderPaymentSucceeded   OrderPaymentFailed
                                                │                       │
                                                └───────────┬───────────┘
                                                            ▼
                                                     ┌─────────────┐
                                                     │ Ordering.API│
                                                     └─────────────┘
```

**Note**: PaymentProcessor is a **simulated** payment gateway. Success/failure is controlled by `PaymentOptions.PaymentSucceeded` configuration.

---

### 4. Inventory Deduction (Post-Payment)

```
┌─────────────┐  OrderStatusChangedToPaid  ┌─────────────┐
│ Ordering.API│ ──────────────────────────►│ Catalog.API │
└─────────────┘                            └─────────────┘
                                                  │
                                                  ▼
                                         catalogItem.RemoveStock()
```

**Important**: Stock is deducted **after** payment, not during validation. This is a business decision documented in the handler.

---

### 5. User Notifications (WebApp)

```
┌─────────────┐                            ┌─────────────┐
│ Ordering.API│ ─────────────────────────► │   WebApp    │
└─────────────┘                            └─────────────┘
       │                                          │
       │ OrderStatusChangedTo:                    ▼
       │ • AwaitingValidation            OrderStatusNotificationService
       │ • StockConfirmed                 .NotifyOrderStatusChangedAsync()
       │ • Paid
       │ • Shipped
       │ • Cancelled
       │ • Submitted
```

WebApp subscribes to **6 order status events** to provide real-time UI updates.

---

### 6. External Webhooks

```
┌─────────────┐                            ┌─────────────┐
│ Ordering.API│ ─────────────────────────► │Webhooks.API │
└─────────────┘                            └─────────────┘
       │                                          │
       │ • OrderStatusChangedToPaid               ▼
       │ • OrderStatusChangedToShipped     retriever.GetSubscriptionsOfType()
       │ • ProductPriceChanged                    │
                                                  ▼
                                           sender.SendAll()
                                           (HTTP callbacks)
```

External systems can subscribe to webhooks for:
- `WebhookType.OrderShipped`
- `WebhookType.OrderPaid`
- Product price changes

---

### 7. Grace Period Background Processing

```
┌────────────────┐  GracePeriodConfirmedIntegrationEvent  ┌─────────────┐
│ OrderProcessor │ ──────────────────────────────────────►│ Ordering.API│
└────────────────┘                                        └─────────────┘
       │
       │ Background Service polls DB every N seconds
       │ for orders in "Submitted" status past grace period
       │
       ▼
 Triggers order to move from Submitted → AwaitingValidation
```

**Purpose**: Allows orders a "grace period" before stock validation begins (e.g., for order modifications).

---

## Event Reliability Features

### 1. Outbox Pattern

```csharp
// src/Ordering.API/Application/IntegrationEvents/OrderingIntegrationEventService.cs
await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
await _eventBus.PublishAsync(logEvt.IntegrationEvent);
await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
```

Events are persisted to an **integration event log** before publishing, ensuring at-least-once delivery.

### 2. Retry with Polly

```csharp
// src/EventBusRabbitMQ/RabbitMQEventBus.cs
private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value.RetryCount);
```

Configurable retry policy for transient failures.

### 3. OpenTelemetry Tracing

All RabbitMQ operations participate in distributed tracing via OpenTelemetry instrumentation.

---

## Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|------------|
| Message loss | High | Outbox pattern ensures persistence before publish |
| Duplicate processing | Medium | Handlers should be idempotent (most are) |
| Ordering issues | Medium | Events are processed sequentially per queue |
| Dead letters | High | **NOT IMPLEMENTED** - code comments note DLX should be added |
| Poison messages | High | No circuit breaker - bad messages will retry indefinitely |

---

## Compliance Considerations (FedRAMP)

| Requirement | Status | Notes |
|-------------|--------|-------|
| Audit logging | ✅ | All events logged with correlation IDs |
| Message encryption | ⚠️ | TLS in transit, but messages not encrypted at rest |
| Access control | ✅ | RabbitMQ credentials managed via Aspire secrets |
| Data retention | ⚠️ | Integration event log retained indefinitely |

---

## Rollback Considerations

If a RabbitMQ-related change needs rollback:

1. **Event schema changes**: Old consumers must handle both old and new formats (breaking change otherwise)
2. **New event types**: Existing services ignore unknown events (safe to rollback)
3. **Handler logic changes**: May leave orders in inconsistent states if mid-flight

**Recommendation**: Use versioned event types for breaking changes (e.g., `OrderStartedIntegrationEventV2`)

---

## Key Source Files

| File | Purpose |
|------|---------|
| `src/EventBusRabbitMQ/RabbitMQEventBus.cs` | Core RabbitMQ pub/sub implementation |
| `src/EventBus/Abstractions/IEventBus.cs` | Event bus interface |
| `src/EventBus/Events/IntegrationEvent.cs` | Base integration event class |
| `src/Ordering.API/Application/IntegrationEvents/` | Order-related events and handlers |
| `src/Catalog.API/IntegrationEvents/` | Catalog events and handlers |
| `src/IntegrationEventLogEF/` | Outbox pattern implementation |

---

*Generated: 2025-12-08*

