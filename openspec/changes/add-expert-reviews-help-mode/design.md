# Technical Design: Expert Reviews Help Mode

**Change ID:** `add-expert-reviews-help-mode`  
**Version:** 1.0  
**Last Updated:** 2025-12-09

## Architecture Overview

This design extends the existing eShop microservices architecture to support customer reviews integrated with AI-powered product recommendations. The solution maintains the architectural principles of the existing system while adding review capabilities to the Catalog domain.

```
┌──────────────────────────────────────────────────────────────────┐
│                          WebApp (Blazor)                         │
│  ┌────────────────────┐         ┌──────────────────────────┐   │
│  │   Standard Chat    │◄────────►   Expert Mode Chat       │   │
│  │   (ChatState)      │         │  (ExpertChatState)       │   │
│  └────────┬───────────┘         └──────────┬───────────────┘   │
│           │                                  │                    │
│           │  ┌──────────────────────────────┼─────────────┐     │
│           │  │      Review Functions:       │             │     │
│           │  │  - GetProductReviews()       │             │     │
│           └──┤  - GetReviewSummary()        │             │     │
│              │  - SearchReviewsByTopic()    │             │     │
│              │  - CompareProductsByReviews()│             │     │
│              └──────────────┬───────────────┘             │     │
└─────────────────────────────┼─────────────────────────────┼─────┘
                              │                             │
                              ▼                             ▼
                    ┌──────────────────┐         ┌─────────────────┐
                    │  ReviewService   │         │ CatalogService  │
                    │   (HTTP Client)  │         │ (HTTP Client)   │
                    └─────────┬────────┘         └────────┬────────┘
                              │                           │
                              ▼                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Catalog.API                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                      ReviewsApi                          │  │
│  │  GET    /items/{id}/reviews                             │  │
│  │  POST   /items/{id}/reviews                             │  │
│  │  PUT    /reviews/{id}                                   │  │
│  │  POST   /reviews/{id}/helpful                           │  │
│  │  GET    /items/{id}/review-summary                      │  │
│  └──────────────────────┬───────────────────────────────────┘  │
│                         │                                       │
│  ┌──────────────────────▼───────────────────────────────────┐  │
│  │              ReviewRepository                           │  │
│  │  - GetByProductIdAsync()                                │  │
│  │  - SearchReviewsBySemanticsAsync()                      │  │
│  │  - GetReviewSummaryAsync()                              │  │
│  └──────────────────────┬───────────────────────────────────┘  │
│                         │                                       │
│  ┌──────────────────────▼───────────────────────────────────┐  │
│  │                  CatalogContext                         │  │
│  │  DbSet<ProductReview>                                   │  │
│  │  DbSet<ReviewHelpfulness>                               │  │
│  │  DbSet<VerifiedPurchase>                                │  │
│  └──────────────────────┬───────────────────────────────────┘  │
│                         │                                       │
└─────────────────────────┼───────────────────────────────────────┘
                          │
                          ▼
              ┌──────────────────────┐
              │  PostgreSQL + pgvector│
              │  - catalogdb          │
              │    • ProductReviews   │
              │    • ReviewHelpfulness│
              │    • VerifiedPurchase │
              └──────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                      Ordering.API                              │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │   OrderShippedDomainEventHandler                         │ │
│  │   → Publishes OrderShippedIntegrationEvent               │ │
│  └──────────────────────┬───────────────────────────────────┘ │
└─────────────────────────┼──────────────────────────────────────┘
                          │
                          ▼
                   ┌─────────────┐
                   │  RabbitMQ   │
                   │  Event Bus  │
                   └──────┬──────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Catalog.API (Subscriber)                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  OrderShippedIntegrationEventHandler                     │  │
│  │  → Stores VerifiedPurchase record                        │  │
│  │  → Publishes ReviewRequestIntegrationEvent (7 days)      │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                    AI Infrastructure                           │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │   OpenAI / Ollama                                        │ │
│  │   - Chat Completions (gpt-4.1-mini)                      │ │
│  │   - Text Embeddings (text-embedding-3-small)             │ │
│  └──────────────────────────────────────────────────────────┘ │
│                           ▲                                    │
│                           │                                    │
│  ┌────────────────────────┴─────────────────────────────────┐ │
│  │   CatalogAI Service                                      │ │
│  │   - GenerateReviewEmbeddingAsync()                       │ │
│  │   - SearchReviewsBySemanticsAsync()                      │ │
│  └──────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
```

## Database Design

### ProductReviews Table

```sql
CREATE TABLE "ProductReviews" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INTEGER NOT NULL,
    "UserId" TEXT NOT NULL,
    "OrderId" INTEGER NULL,
    "Rating" INTEGER NOT NULL CHECK ("Rating" >= 1 AND "Rating" <= 5),
    "Title" VARCHAR(200) NOT NULL,
    "Content" TEXT NOT NULL,
    "IsVerifiedPurchase" BOOLEAN NOT NULL DEFAULT FALSE,
    "HelpfulCount" INTEGER NOT NULL DEFAULT 0,
    "NotHelpfulCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Embedding" vector(384),  -- pgvector type for semantic search
    
    CONSTRAINT "FK_ProductReviews_CatalogItems" 
        FOREIGN KEY ("ProductId") REFERENCES "CatalogItems"("Id") ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX "IX_ProductReviews_ProductId" ON "ProductReviews"("ProductId");
CREATE INDEX "IX_ProductReviews_UserId" ON "ProductReviews"("UserId");
CREATE INDEX "IX_ProductReviews_Rating" ON "ProductReviews"("Rating");
CREATE INDEX "IX_ProductReviews_IsVerifiedPurchase" ON "ProductReviews"("IsVerifiedPurchase");
CREATE INDEX "IX_ProductReviews_CreatedAt_Desc" ON "ProductReviews"("CreatedAt" DESC);
CREATE INDEX "IX_ProductReviews_ProductId_Rating_CreatedAt" 
    ON "ProductReviews"("ProductId", "Rating", "CreatedAt" DESC);

-- Vector similarity search index (using HNSW algorithm)
CREATE INDEX ON "ProductReviews" USING hnsw ("Embedding" vector_cosine_ops);
```

### ReviewHelpfulness Table

```sql
CREATE TABLE "ReviewHelpfulness" (
    "Id" SERIAL PRIMARY KEY,
    "ReviewId" INTEGER NOT NULL,
    "UserId" TEXT NOT NULL,
    "IsHelpful" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT "FK_ReviewHelpfulness_ProductReviews" 
        FOREIGN KEY ("ReviewId") REFERENCES "ProductReviews"("Id") ON DELETE CASCADE,
    
    CONSTRAINT "UQ_ReviewHelpfulness_ReviewId_UserId" 
        UNIQUE ("ReviewId", "UserId")
);

CREATE INDEX "IX_ReviewHelpfulness_ReviewId" ON "ReviewHelpfulness"("ReviewId");
CREATE INDEX "IX_ReviewHelpfulness_UserId" ON "ReviewHelpfulness"("UserId");
```

### VerifiedPurchase Table

```sql
CREATE TABLE "VerifiedPurchase" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "OrderId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "ShipDate" TIMESTAMP NOT NULL,
    "ReviewSubmitted" BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT "UQ_VerifiedPurchase_UserId_OrderId_ProductId" 
        UNIQUE ("UserId", "OrderId", "ProductId")
);

CREATE INDEX "IX_VerifiedPurchase_UserId_ProductId" 
    ON "VerifiedPurchase"("UserId", "ProductId");
CREATE INDEX "IX_VerifiedPurchase_OrderId" ON "VerifiedPurchase"("OrderId");
```

## Domain Models

### ProductReview Entity

```csharp
// Location: src/Catalog.API/Model/ProductReview.cs

using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace eShop.Catalog.API.Model;

public class ProductReview
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public CatalogItem? Product { get; set; }
    
    [Required]
    public string UserId { get; set; } = default!;
    
    public int? OrderId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Title { get; set; } = default!;
    
    [Required]
    [StringLength(5000, MinimumLength = 50)]
    public string Content { get; set; } = default!;
    
    public bool IsVerifiedPurchase { get; set; }
    
    public int HelpfulCount { get; set; }
    
    public int NotHelpfulCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public Vector? Embedding { get; set; }
    
    public double GetHelpfulnessScore()
    {
        var totalVotes = HelpfulCount + NotHelpfulCount;
        return totalVotes == 0 ? 0.5 : (double)HelpfulCount / totalVotes;
    }
    
    public string GetAbbreviatedUserId()
    {
        // Return first 2 and last 2 chars for privacy: "ab...yz"
        if (UserId.Length <= 6) return UserId.Substring(0, 2) + "...";
        return UserId.Substring(0, 2) + "..." + UserId.Substring(UserId.Length - 2);
    }
}
```

### DTOs

```csharp
// Location: src/Catalog.API/Model/ReviewDtos.cs

namespace eShop.Catalog.API.Model;

public record CreateReviewRequest(
    int ProductId,
    int Rating,
    string Title,
    string Content,
    int? OrderId = null
);

public record UpdateReviewRequest(
    int Rating,
    string Title,
    string Content
);

public record ReviewResponse(
    int Id,
    int ProductId,
    string UserId,
    int Rating,
    string Title,
    string Content,
    bool IsVerifiedPurchase,
    int HelpfulCount,
    int NotHelpfulCount,
    double HelpfulnessScore,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ReviewSummary(
    int ProductId,
    int TotalReviews,
    double AverageRating,
    Dictionary<int, int> RatingDistribution, // { 5: 120, 4: 45, 3: 10, 2: 3, 1: 2 }
    int VerifiedPurchaseCount,
    List<string> CommonThemes, // AI-extracted themes
    List<ReviewResponse> TopReviews // Top 3 most helpful
);

public record ReviewListResponse(
    List<ReviewResponse> Reviews,
    int TotalCount,
    int PageNumber,
    int PageSize
);
```

## AI Integration Design

### Review Embedding Strategy

**Objective:** Enable semantic search across review content to answer natural language queries like "reviews mentioning durability" or "what do people say about fit?"

**Implementation:**
1. **Embedding Generation:** When a review is created/updated, generate embedding from `Title + " " + Content`
2. **Model:** Use existing `text-embedding-3-small` (384 dimensions) for consistency with catalog embeddings
3. **Storage:** Store in `Embedding` column as pgvector type
4. **Search:** Use cosine similarity for semantic matching

```csharp
// Location: src/Catalog.API/Services/CatalogAI.cs (extended)

public async ValueTask<Vector?> GetReviewEmbeddingAsync(ProductReview review)
{
    if (!IsEnabled) return null;
    
    string text = $"{review.Title} {review.Content}";
    return await GetEmbeddingAsync(text);
}

public async Task<List<ProductReview>> SearchReviewsBySemanticsAsync(
    string query, 
    int? productId = null, 
    int maxResults = 10)
{
    var queryEmbedding = await GetEmbeddingAsync(query);
    if (queryEmbedding == null) return new List<ProductReview>();
    
    // Use pgvector cosine distance operator <=>
    var reviews = _context.ProductReviews
        .Where(r => productId == null || r.ProductId == productId)
        .OrderBy(r => r.Embedding!.CosineDistance(queryEmbedding))
        .Take(maxResults);
    
    return await reviews.ToListAsync();
}
```

### Expert Mode System Prompt

```csharp
// Location: src/WebApp/Components/Chatbot/ExpertChatState.cs

private const string ExpertSystemPrompt = """
You are an expert AdventureWorks product advisor with access to comprehensive 
customer reviews and ratings. Your primary goal is to help customers make 
informed purchase decisions based on real user experiences.

CORE PRINCIPLES:
1. Always ground recommendations in actual review data - never fabricate review content
2. Cite specific review insights using format: "Customers mention..." or "Verified buyers report..."
3. Consider rating distribution, not just averages (e.g., 4.2★ with 90% 5-star is stronger than 4.5★ with split ratings)
4. Present balanced view: highlight both strengths and common concerns from reviews
5. Match products to customer needs based on review feedback about use cases
6. When comparing products, use review-based differentiators (what do customers say differently?)
7. If reviews are insufficient (<5 reviews), acknowledge limitation and suggest alternatives

AVAILABLE FUNCTIONS:
- GetProductReviews(productId, filter): Get detailed reviews for a product
- GetReviewSummary(productId): Get aggregate stats and themes
- SearchReviewsByTopic(topic, productId?): Find reviews mentioning specific aspects
- CompareProductsByReviews(productIds[]): Compare products based on review insights

RESPONSE FORMAT:
- Start with clear recommendation
- Support with review-based evidence
- Include rating (★★★★★) and review count
- Quote specific review excerpts when relevant (use quotes)
- End with any important caveats from reviews

EXAMPLE INTERACTION:
User: "I'm looking for a snowboard for steep terrain"
1. Call SearchReviewsByTopic("steep terrain backcountry powder")
2. Call GetReviewSummary() for top matching products
3. Respond: "Based on 47 customer reviews, the Gravitator Peak Rider (4.8★, 89 reviews) 
   is highly recommended for steep terrain. Verified buyers specifically mention 
   'excellent edge hold on steep slopes' and 'confidence-inspiring on challenging runs.'
   92% of advanced riders gave it 5 stars. However, 3 reviews note it may be too 
   aggressive for beginners."

You NEVER respond about topics unrelated to AdventureWorks products. Stay focused 
on helping customers find the right gear based on community feedback.
""";
```

### AI Function Implementations

```csharp
// Location: src/WebApp/Components/Chatbot/ExpertChatState.cs

[Description("Gets detailed customer reviews for a specific product")]
private async Task<string> GetProductReviews(
    [Description("The product ID to get reviews for")] int productId,
    [Description("Optional filter: 'verified', 'recent', 'critical' (1-3 stars), 'positive' (4-5 stars)")] string? filter = null)
{
    try
    {
        var reviews = await _reviewService.GetReviews(productId, filter);
        
        // Format for AI consumption
        var formatted = new
        {
            ProductId = productId,
            TotalReviews = reviews.TotalCount,
            Reviews = reviews.Reviews.Select(r => new
            {
                Rating = r.Rating,
                Title = r.Title,
                Excerpt = r.Content.Length > 200 ? r.Content.Substring(0, 200) + "..." : r.Content,
                IsVerified = r.IsVerifiedPurchase,
                Helpfulness = r.HelpfulnessScore,
                Date = r.CreatedAt.ToString("yyyy-MM-dd")
            })
        };
        
        return JsonSerializer.Serialize(formatted);
    }
    catch (Exception e)
    {
        return Error(e, "Unable to retrieve product reviews.");
    }
}

[Description("Gets an AI-generated summary of reviews including ratings, themes, and key insights")]
private async Task<string> GetReviewSummary(
    [Description("The product ID to summarize reviews for")] int productId)
{
    try
    {
        var summary = await _reviewService.GetReviewSummary(productId);
        return JsonSerializer.Serialize(summary);
    }
    catch (Exception e)
    {
        return Error(e, "Unable to get review summary.");
    }
}

[Description("Searches reviews for mentions of specific topics, features, or concerns")]
private async Task<string> SearchReviewsByTopic(
    [Description("The topic to search for, e.g., 'durability', 'fit', 'beginner friendly'")] string topic,
    [Description("Optional: limit search to specific product ID")] int? productId = null)
{
    try
    {
        var reviews = await _reviewService.SearchReviews(topic, productId);
        
        var formatted = reviews.Reviews.Select(r => new
        {
            ProductId = r.ProductId,
            Rating = r.Rating,
            Title = r.Title,
            RelevantExcerpt = ExtractRelevantExcerpt(r.Content, topic),
            IsVerified = r.IsVerifiedPurchase
        });
        
        return JsonSerializer.Serialize(formatted);
    }
    catch (Exception e)
    {
        return Error(e, "Unable to search reviews.");
    }
}

[Description("Compares multiple products based on customer review insights")]
private async Task<string> CompareProductsByReviews(
    [Description("Array of product IDs to compare")] int[] productIds)
{
    try
    {
        var comparisons = new List<object>();
        
        foreach (var productId in productIds.Take(4)) // Limit to 4 products
        {
            var summary = await _reviewService.GetReviewSummary(productId);
            var product = await _catalogService.GetCatalogItem(productId);
            
            comparisons.Add(new
            {
                ProductId = productId,
                ProductName = product?.Name,
                AverageRating = summary.AverageRating,
                ReviewCount = summary.TotalReviews,
                VerifiedReviews = summary.VerifiedPurchaseCount,
                CommonPraise = summary.CommonThemes.Where(t => t.StartsWith("+")),
                CommonConcerns = summary.CommonThemes.Where(t => t.StartsWith("-"))
            });
        }
        
        return JsonSerializer.Serialize(new { Comparison = comparisons });
    }
    catch (Exception e)
    {
        return Error(e, "Unable to compare products.");
    }
}

private string ExtractRelevantExcerpt(string content, string topic)
{
    // Find sentence containing topic (case-insensitive)
    var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
    var relevant = sentences.FirstOrDefault(s => s.Contains(topic, StringComparison.OrdinalIgnoreCase));
    return relevant?.Trim() ?? content.Substring(0, Math.Min(150, content.Length));
}
```

## Integration Event Flow

### Verified Purchase Tracking

```
Order Shipped → Ordering.API
    ↓
OrderShippedDomainEvent raised
    ↓
OrderStatusChangedToShippedIntegrationEvent published to RabbitMQ
    ↓
Catalog.API subscribes and handles event
    ↓
Store VerifiedPurchase record: (UserId, OrderId, ProductId, ShipDate)
    ↓
Schedule delayed ReviewRequestIntegrationEvent (7 days)
    ↓
Email service sends review request to customer
```

### Integration Event Schema Extension Plan

**Issue:** The existing `OrderStatusChangedToShippedIntegrationEvent` lacks the required properties for verified purchase tracking. Current event has only: `OrderId`, `OrderStatus`, `BuyerName`, `BuyerIdentityGuid`. We need to add: `BuyerEmail` and `OrderItems` collection.

**Impact:** This is a breaking change to the integration event contract, affecting all subscribers (WebApp, Webhooks.API).

**Required Changes:**

**Step 1: Extend Integration Event (Ordering.API)**
```csharp
// Location: src/Ordering.API/Application/IntegrationEvents/Events/OrderStatusChangedToShippedIntegrationEvent.cs

public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; }
    public OrderStatus OrderStatus { get; }
    public string BuyerName { get; }
    public string BuyerIdentityGuid { get; }
    
    // NEW: Add buyer email for review notifications
    public string BuyerEmail { get; }
    
    // NEW: Add order items for verified purchase tracking
    public IReadOnlyCollection<OrderItemInfo> OrderItems { get; }

    public OrderStatusChangedToShippedIntegrationEvent(
        int orderId, 
        OrderStatus orderStatus, 
        string buyerName, 
        string buyerIdentityGuid,
        string buyerEmail,
        IEnumerable<OrderItemInfo> orderItems)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
        BuyerEmail = buyerEmail;
        OrderItems = orderItems.ToList().AsReadOnly();
    }
}

// NEW: DTO for order item information in integration events
public record OrderItemInfo(int ProductId, string ProductName, int Units, decimal UnitPrice);
```

**Step 2: Update Domain Event Handler (Ordering.API)**
```csharp
// Location: src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs

public class OrderShippedDomainEventHandler : INotificationHandler<OrderShippedDomainEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IIntegrationEventLogService _integrationEventLogService;
    private readonly ILogger<OrderShippedDomainEventHandler> _logger;

    public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order {OrderId} shipped", domainEvent.Order.Id);

        // Fetch full order with items
        var order = await _orderRepository.GetAsync(domainEvent.Order.Id);
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value);

        // UPDATED: Include email and order items in integration event
        var orderItems = order.OrderItems.Select(item => new OrderItemInfo(
            item.ProductId,
            item.GetProductName(),
            item.GetUnits(),
            item.GetUnitPrice()
        )).ToList();

        var integrationEvent = new OrderStatusChangedToShippedIntegrationEvent(
            order.Id,
            order.OrderStatus,
            buyer.Name,
            buyer.IdentityGuid,
            buyer.Email,  // NEW: Add email
            orderItems    // NEW: Add order items
        );

        await _integrationEventLogService.AddAndSaveEventAsync(integrationEvent);
    }
}
```

**Step 3: Verify Buyer Entity Has Email (Ordering.Domain)**
```csharp
// Location: src/Ordering.Domain/AggregatesModel/BuyerAggregate/Buyer.cs

// Verify the Buyer entity has an Email property. If not, add:
public class Buyer : Entity, IAggregateRoot
{
    public string IdentityGuid { get; private set; }
    public string Name { get; private set; }
    
    // ADD IF MISSING:
    public string Email { get; private set; }
    
    // Update constructor to include email if necessary
}
```

**Step 4: Create Integration Event Handler (Catalog.API)**
```csharp
// Location: src/Catalog.API/IntegrationEvents/OrderStatusChangedToShippedIntegrationEventHandler.cs

public class OrderStatusChangedToShippedIntegrationEventHandler 
    : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    private readonly CatalogContext _context;
    private readonly ILogger<OrderStatusChangedToShippedIntegrationEventHandler> _logger;
    
    public OrderStatusChangedToShippedIntegrationEventHandler(
        CatalogContext context,
        ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "Handling order shipped event for Order {OrderId} with {ItemCount} items",
            @event.OrderId, 
            @event.OrderItems.Count);
        
        // Store verified purchase records for all items in the order
        foreach (var orderItem in @event.OrderItems)
        {
            // Check if product exists in catalog
            var productExists = await _context.CatalogItems
                .AnyAsync(p => p.Id == orderItem.ProductId);
            
            if (!productExists)
            {
                _logger.LogWarning(
                    "Product {ProductId} from Order {OrderId} not found in catalog",
                    orderItem.ProductId,
                    @event.OrderId);
                continue;
            }
            
            var verifiedPurchase = new VerifiedPurchase
            {
                UserId = @event.BuyerIdentityGuid,
                OrderId = @event.OrderId,
                ProductId = orderItem.ProductId,
                ShipDate = DateTime.UtcNow,
                ReviewSubmitted = false
            };
            
            _context.VerifiedPurchase.Add(verifiedPurchase);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation(
            "Stored {Count} verified purchase records for Order {OrderId}",
            @event.OrderItems.Count,
            @event.OrderId);
        
        // TODO: Implement delayed review request (see delayed messaging section)
        // For now, we just track the verified purchase
    }
}
```

**Step 5: Register Event Handler (Catalog.API)**
```csharp
// Location: src/Catalog.API/Program.cs

// Add to event bus subscription section:
eventBus.Subscribe<OrderStatusChangedToShippedIntegrationEvent, 
                   OrderStatusChangedToShippedIntegrationEventHandler>();
```

**Step 6: Update Existing Subscribers**

Check if these services consume the event and update handlers if needed:
- `src/WebApp/Application/IntegrationEvents/` - Update any handlers
- `src/Webhooks.API/IntegrationEvents/` - Update any handlers

Most existing handlers likely only use `OrderId` and `BuyerName`, so they should continue working (backward compatible addition of new properties).

**Deployment Strategy:**
1. Deploy updated Ordering.API with extended event schema
2. Deploy Catalog.API with new handler
3. Update any other subscribers
4. Existing events in flight will fail gracefully if old schema - acceptable for non-critical review feature

**Estimated Effort:** 4-6 hours
- Event schema extension: 30 minutes
- Domain event handler update: 1 hour
- Buyer entity verification/update: 30 minutes
- Integration event handler implementation: 2 hours
- Testing and validation: 1-2 hours

**Alternative Approach (If Breaking Change Unacceptable):**

If modifying the existing event is too risky, create a new event:
```csharp
public record OrderShippedWithDetailsIntegrationEvent : IntegrationEvent
{
    // Include all required properties from the start
}
```

Publish both events from the domain event handler during transition period, then deprecate old event after migration complete. This doubles the effort but provides zero-downtime migration.

## Performance Considerations

### Caching Strategy

**Review Summaries (High Read, Low Write):**
- Cache in Redis with 5-minute TTL
- Key format: `review-summary:{productId}`
- Invalidate on new review submission
- Reduces database load for popular products

**Embedding Generation (Write-Heavy):**
- Asynchronous background processing
- Queue reviews for embedding generation
- Don't block review submission on embedding
- Batch process embeddings (up to 100 at a time)

```csharp
// Location: src/Catalog.API/Services/ReviewEmbeddingService.cs

public class ReviewEmbeddingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingEmbeddings(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }
    
    private async Task ProcessPendingEmbeddings(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        var catalogAI = scope.ServiceProvider.GetRequiredService<ICatalogAI>();
        
        // Get reviews without embeddings
        var reviewsNeedingEmbeddings = await context.ProductReviews
            .Where(r => r.Embedding == null)
            .Take(100) // Batch size
            .ToListAsync(cancellationToken);
        
        if (!reviewsNeedingEmbeddings.Any()) return;
        
        foreach (var review in reviewsNeedingEmbeddings)
        {
            try
            {
                review.Embedding = await catalogAI.GetReviewEmbeddingAsync(review);
            }
            catch (Exception ex)
            {
                // Log but continue with other reviews
                _logger.LogError(ex, "Failed to generate embedding for review {ReviewId}", review.Id);
            }
        }
        
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Generated embeddings for {Count} reviews", reviewsNeedingEmbeddings.Count);
    }
}
```

### Database Query Optimization

**Composite Indexes:**
```sql
-- Frequently used query: Get recent verified reviews for a product
CREATE INDEX "IX_ProductReviews_ProductId_Verified_CreatedAt" 
    ON "ProductReviews"("ProductId", "IsVerifiedPurchase", "CreatedAt" DESC)
    WHERE "IsVerifiedPurchase" = TRUE;

-- For rating filtering
CREATE INDEX "IX_ProductReviews_ProductId_Rating" 
    ON "ProductReviews"("ProductId", "Rating");
```

**Review Summary Materialized View (Optional):**
```sql
CREATE MATERIALIZED VIEW "ProductReviewSummary" AS
SELECT 
    "ProductId",
    COUNT(*) as "TotalReviews",
    AVG("Rating") as "AverageRating",
    SUM(CASE WHEN "IsVerifiedPurchase" THEN 1 ELSE 0 END) as "VerifiedCount",
    SUM(CASE WHEN "Rating" = 5 THEN 1 ELSE 0 END) as "FiveStarCount",
    SUM(CASE WHEN "Rating" = 4 THEN 1 ELSE 0 END) as "FourStarCount",
    SUM(CASE WHEN "Rating" = 3 THEN 1 ELSE 0 END) as "ThreeStarCount",
    SUM(CASE WHEN "Rating" = 2 THEN 1 ELSE 0 END) as "TwoStarCount",
    SUM(CASE WHEN "Rating" = 1 THEN 1 ELSE 0 END) as "OneStarCount"
FROM "ProductReviews"
GROUP BY "ProductId";

-- Refresh periodically or on review submission
REFRESH MATERIALIZED VIEW CONCURRENTLY "ProductReviewSummary";
```

## Security Considerations

### Authentication & Authorization

**Review Submission:**
- Requires authenticated user (JWT Bearer token)
- User ID extracted from token claims

**Review Editing:**
- Only review author can edit/delete their own reviews
- Implemented via policy: `[Authorize(Policy = "ReviewOwner")]`

**Verified Purchase:**
- Automatically set based on `VerifiedPurchase` table lookup
- Cannot be manually set by user
- Prevents fake verified reviews

**Helpfulness Voting:**
- Requires authentication
- One vote per user per review (enforced by unique constraint)
- Users cannot vote on their own reviews

```csharp
// Location: src/Catalog.API/Apis/ReviewsApi.cs

public static async Task<Results<Ok, UnauthorizedHttpResult>> UpdateReview(
    int id,
    UpdateReviewRequest request,
    HttpContext httpContext,
    ReviewRepository repository)
{
    var userId = httpContext.User.FindFirst("sub")?.Value;
    if (userId == null) return TypedResults.Unauthorized();
    
    var review = await repository.GetByIdAsync(id);
    if (review == null || review.UserId != userId)
    {
        return TypedResults.Unauthorized(); // Don't reveal if review exists
    }
    
    // Update review
    review.Rating = request.Rating;
    review.Title = request.Title;
    review.Content = request.Content;
    review.UpdatedAt = DateTime.UtcNow;
    review.Embedding = null; // Trigger re-generation
    
    await repository.UpdateAsync(review);
    return TypedResults.Ok();
}
```

### Data Privacy

**Personally Identifiable Information (PII):**
- User IDs stored but never displayed in full (use abbreviated format)
- Email addresses not stored with reviews (only for notifications)
- No full names in reviews (users can choose display names)

**Content Moderation:**
- Reviews are user-generated content - implement abuse reporting
- AI-powered profanity detection (optional)
- Manual review queue for flagged content

## Monitoring & Observability

### Key Metrics (OpenTelemetry)

```csharp
// Custom metrics to track

private static readonly Counter<long> ReviewsSubmittedCounter = 
    Meter.CreateCounter<long>("reviews.submitted", "count");

private static readonly Histogram<double> EmbeddingGenerationDuration = 
    Meter.CreateHistogram<double>("review.embedding.duration", "ms");

private static readonly Counter<long> ExpertModeActivations = 
    Meter.CreateCounter<long>("chatbot.expertmode.activations", "count");

private static readonly Histogram<int> ReviewRetrievalSize = 
    Meter.CreateHistogram<int>("review.retrieval.size", "reviews");
```

**Dashboards to Create:**
1. Review submission rate over time
2. Expert Mode adoption rate
3. AI function call frequency (which functions are most used)
4. Review sentiment distribution per product
5. Embedding generation queue length and processing time

### Health Checks

```csharp
// Location: src/Catalog.API/HealthChecks/ReviewSystemHealthCheck.cs

public class ReviewSystemHealthCheck : IHealthCheck
{
    private readonly CatalogContext _context;
    private readonly ICatalogAI _catalogAI;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity
            var reviewCount = await _context.ProductReviews.CountAsync(cancellationToken);
            
            // Check embedding service availability
            var embeddingServiceHealthy = _catalogAI.IsEnabled;
            
            // Check for reviews needing embeddings (queue health)
            var pendingEmbeddings = await _context.ProductReviews
                .CountAsync(r => r.Embedding == null, cancellationToken);
            
            var data = new Dictionary<string, object>
            {
                { "total_reviews", reviewCount },
                { "embedding_service_available", embeddingServiceHealthy },
                { "pending_embeddings", pendingEmbeddings }
            };
            
            if (pendingEmbeddings > 1000)
            {
                return HealthCheckResult.Degraded(
                    "Embedding queue is backed up",
                    data: data
                );
            }
            
            return HealthCheckResult.Healthy("Review system operational", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Review system unavailable", ex);
        }
    }
}
```

## Testing Strategy

### Unit Tests

**ReviewRepository Tests:**
- Test pagination, filtering, sorting
- Test semantic search with mock embeddings
- Test helpfulness score calculation

**ExpertChatState Tests:**
- Mock ReviewService responses
- Verify AI function registration
- Test function invocations return expected JSON

### Integration Tests

**ReviewsApi Endpoint Tests:**
- Test full CRUD lifecycle
- Test authentication/authorization flows
- Test verified purchase validation

**Event Handler Tests:**
- Mock OrderShippedIntegrationEvent
- Verify VerifiedPurchase record created
- Verify ReviewRequestIntegrationEvent published

### E2E Tests (Playwright)

```typescript
// Location: e2e/ExpertModeReviewTest.spec.ts

test('expert mode recommends based on reviews', async ({ page }) => {
  await page.goto('/');
  
  // Open chatbot
  await page.click('.show-chatbot');
  
  // Enable Expert Mode
  await page.click('[data-testid="expert-mode-toggle"]');
  await expect(page.locator('.expert-mode-badge')).toBeVisible();
  
  // Ask for recommendation
  await page.fill('.chatbot-input textarea', 'What's the best snowboard for beginners?');
  await page.click('.chatbot-input button[type="submit"]');
  
  // Wait for AI response
  await page.waitForSelector('.message-assistant', { timeout: 10000 });
  
  // Verify response includes review citations
  const response = await page.locator('.message-assistant').last().textContent();
  expect(response).toContain('★'); // Star rating
  expect(response).toContain('review'); // Mentions reviews
  expect(response).toMatch(/\d+\s+reviews?/); // Review count
});
```

## Migration Path

### Phase 1: Database Only (Week 1)
- Deploy database schema
- No UI changes
- Allows review data to be seeded independently

### Phase 2: API Endpoints (Week 2-3)
- Deploy review APIs
- Background embedding service
- Internal testing with Postman/curl

### Phase 3: Review Submission UI (Week 4)
- Deploy review form
- Start collecting real user reviews
- Expert Mode not yet visible

### Phase 4: Expert Mode (Week 5-6)
- Deploy Expert Mode chatbot
- Feature flag enabled for 10% of users initially
- Monitor performance and AI quality

### Phase 5: Full Rollout (Week 7-8)
- Increase feature flag to 100%
- Collect feedback and iterate
- Optimize based on usage patterns

## Rollback Strategy

**If critical issues arise:**

1. **Disable Expert Mode:** Set feature flag to 0% - chatbot remains functional
2. **Disable Review Submissions:** Set read-only mode on review endpoints
3. **Database Rollback:** Migrations are reversible - drop new tables if needed
4. **AI Function Removal:** Remove review functions from ChatOptions - existing chat works

**Data Safety:**
- All review data persisted in separate tables
- No modification to existing CatalogItems
- Can operate in degraded mode (reviews stored but not displayed)

## Future Enhancements

1. **Review Photos:** Allow image uploads with reviews
2. **AI-Generated Review Summaries:** Show on product pages
3. **Review Trends:** "Recent reviews mention X more frequently"
4. **Comparative Shopping:** AI-generated comparison tables based on reviews
5. **Review Quality Scores:** ML model to detect helpful vs. unhelpful reviews
6. **Multi-Language Reviews:** Translation and language-specific analysis

---

**Document Status:** Ready for Review  
**Next Steps:** Validation, approval, task assignment
