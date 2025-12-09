# Implementation Tasks: Expert Reviews Help Mode

**Change ID:** `add-expert-reviews-help-mode`  
**Status:** Not Started

## Phase 1: Foundation - Review Data Infrastructure

### Database & Domain Model
- [ ] **Task 1.1:** Create EF Core migration for ProductReviews table
  - Columns: Id, ProductId, UserId, OrderId (nullable), Rating, Title, Content
  - Columns: IsVerifiedPurchase, HelpfulCount, NotHelpfulCount, CreatedAt, UpdatedAt
  - Embedding column using pgvector type
  - Indexes: ProductId, UserId, Rating, IsVerifiedPurchase, CreatedAt (DESC)
  - Foreign key to CatalogItems

- [ ] **Task 1.2:** Create ReviewHelpfulness tracking table
  - Columns: Id, ReviewId, UserId, IsHelpful (bool), CreatedAt
  - Unique constraint on (ReviewId, UserId)
  - Foreign key to ProductReviews

- [ ] **Task 1.3:** Implement ProductReview entity in Catalog.Domain
  - Location: `src/Catalog.API/Model/ProductReview.cs`
  - Properties matching database schema
  - Validation: Rating 1-5, Content max 5000 chars, Title max 200 chars
  - Method: `CalculateHelpfulnessScore()` returning ratio

- [ ] **Task 1.4:** Implement ReviewHelpfulness entity
  - Location: `src/Catalog.API/Model/ReviewHelpfulness.cs`
  - Simple vote tracking entity

- [ ] **Task 1.5:** Update CatalogContext DbContext
  - Add `DbSet<ProductReview> ProductReviews`
  - Add `DbSet<ReviewHelpfulness> ReviewHelpfulness`
  - Configure entity relationships and indexes
  - Configure pgvector column for embeddings

- [ ] **Task 1.6:** Create and test database migration
  - Run migration in development environment
  - Verify indexes created correctly
  - Test rollback scenario

### Repository Layer
- [ ] **Task 1.7:** Create IReviewRepository interface
  - Methods: GetByProductIdAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync
  - Methods: GetReviewSummaryAsync, SearchReviewsAsync, RecordHelpfulnessAsync

- [ ] **Task 1.8:** Implement ReviewRepository
  - Location: `src/Catalog.API/Repositories/ReviewRepository.cs`
  - Implement all interface methods with EF Core
  - Include pagination support
  - Include filtering (rating, verified, date range)

### API Endpoints
- [ ] **Task 1.9:** Create ReviewsApi endpoint group
  - Location: `src/Catalog.API/Apis/ReviewsApi.cs`
  - Endpoint: `GET /api/catalog/items/{id}/reviews` - paginated list
  - Endpoint: `POST /api/catalog/items/{id}/reviews` - create review (authenticated)
  - Endpoint: `PUT /api/catalog/reviews/{id}` - update review (owner only)
  - Endpoint: `DELETE /api/catalog/reviews/{id}` - delete review (owner only)
  - Endpoint: `POST /api/catalog/reviews/{id}/helpful` - mark helpful (authenticated)
  - All endpoints use [FromHeader] for user authentication

- [ ] **Task 1.10:** Add authorization policies for reviews
  - User can only edit/delete own reviews
  - Helpfulness voting requires authentication
  - Verified purchase flag set automatically based on OrderId

- [ ] **Task 1.11:** Implement review DTOs
  - Location: `src/Catalog.API/Model/ReviewDtos.cs`
  - CreateReviewRequest, UpdateReviewRequest, ReviewResponse, ReviewListResponse
  - ReviewSummary (aggregate stats)

### AI Embeddings Integration
- [ ] **Task 1.12:** Extend CatalogAI service for review embeddings
  - Add method: `GetReviewEmbeddingAsync(ProductReview review)`
  - Use existing IEmbeddingGenerator
  - Combine Title + Content for embedding text

- [ ] **Task 1.13:** Create background service for review embedding generation
  - Location: `src/Catalog.API/Services/ReviewEmbeddingService.cs`
  - Process new reviews without embeddings on startup and periodic intervals
  - Log progress and errors

- [ ] **Task 1.14:** Implement semantic review search
  - Add method to ReviewRepository: `SearchReviewsBySemanticsAsync(string query, productId)`
  - Use pgvector distance functions
  - Return reviews ranked by relevance

## Phase 2: Ordering Integration

### Verified Purchase Tracking
- [ ] **Task 2.1:** Add integration event listener in Catalog.API
  - Subscribe to `OrderShippedIntegrationEvent`
  - Store mapping of UserId -> OrderId -> ProductIds for verification
  - Location: `src/Catalog.API/IntegrationEvents/OrderShippedIntegrationEventHandler.cs`

- [ ] **Task 2.2:** Create VerifiedPurchase tracking table
  - Columns: UserId, OrderId, ProductId, ShipDate
  - Used to validate IsVerifiedPurchase when creating reviews

- [ ] **Task 2.3:** Implement verified purchase validation
  - Check if user purchased product before allowing verified flag
  - Add validation in review creation endpoint

### Review Request Notifications
- [ ] **Task 2.4:** Create ReviewRequestIntegrationEvent
  - Location: `src/Catalog.API/IntegrationEvents/Events/ReviewRequestIntegrationEvent.cs`
  - Properties: UserId, OrderId, ProductIds[], CustomerEmail

- [ ] **Task 2.5:** Implement review request email template
  - HTML email with product images and direct review links
  - Location: email template configuration

- [ ] **Task 2.6:** Update OrderShippedDomainEventHandler
  - Publish ReviewRequestIntegrationEvent 7 days after shipping
  - Use delayed message pattern (if supported) or scheduled job

## Phase 3: Chatbot Expert Mode

### Review Analysis Functions
- [ ] **Task 3.1:** Implement GetProductReviews AI function
  - Location: `src/WebApp/Components/Chatbot/ChatState.cs`
  - Method: `GetProductReviews(int productId, string? filter)`
  - Returns JSON with reviews, ratings, key themes
  - Add to ChatOptions.Tools

- [ ] **Task 3.2:** Implement GetReviewSummary AI function
  - Method: `GetReviewSummary(int productId)`
  - Aggregates: avg rating, count, rating distribution, common themes
  - Use semantic clustering to identify themes
  - Return structured summary for AI to interpret

- [ ] **Task 3.3:** Implement SearchReviewsByTopic AI function
  - Method: `SearchReviewsByTopic(string topic, int? productId)`
  - Examples: "durability", "fit", "beginner friendly"
  - Use semantic search on review embeddings
  - Return relevant review excerpts

- [ ] **Task 3.4:** Implement CompareProductsByReviews AI function
  - Method: `CompareProductsByReviews(int[] productIds)`
  - Returns comparative analysis: ratings, pros/cons from reviews
  - Identify differentiating factors mentioned in reviews

### Expert Mode Chatbot State
- [ ] **Task 3.5:** Create ExpertChatState class
  - Location: `src/WebApp/Components/Chatbot/ExpertChatState.cs`
  - Inherit from or extend ChatState
  - Enhanced system prompt emphasizing review usage
  - Register all review-related AI functions

- [ ] **Task 3.6:** Update system prompt for expert mode
  ```
  System: You are an expert AdventureWorks product advisor with access to 
  comprehensive customer reviews and ratings. Your recommendations must be 
  grounded in actual customer feedback. When discussing products:
  
  1. Always check review summaries before making recommendations
  2. Cite specific review insights ("customers mention...", "verified buyers report...")
  3. Consider rating distribution, not just averages
  4. Highlight both strengths and common concerns
  5. Match products to customer needs based on review feedback
  6. Compare products using review-based differentiators
  
  Never fabricate review content. If reviews are insufficient, acknowledge it.
  ```

- [ ] **Task 3.7:** Add review citation formatting
  - Format AI responses to clearly distinguish review citations
  - Example: "★★★★★ 'Great for beginners' - Verified Buyer"
  - Add review IDs for potential click-through

### UI Components
- [ ] **Task 3.8:** Create Expert Mode toggle component
  - Location: `src/WebApp/Components/Chatbot/ExpertModeToggle.razor`
  - Simple switch: "Standard Mode" | "Expert Mode"
  - State persisted in session storage
  - Info tooltip explaining expert mode benefits

- [ ] **Task 3.9:** Update Chatbot.razor to support mode switching
  - Instantiate ExpertChatState when Expert Mode enabled
  - Show mode indicator badge in chatbot header
  - Clear/preserve message history on mode switch (decide which)

- [ ] **Task 3.10:** Create inline review display component
  - Location: `src/WebApp/Components/Chatbot/ReviewHighlight.razor`
  - Shows rating stars, excerpt, verified badge
  - Appears when AI cites specific reviews
  - Click to expand full review

### Review Service Client
- [ ] **Task 3.11:** Create IReviewService in WebApp
  - Location: `src/WebApp/Services/IReviewService.cs`
  - Methods: GetReviews, GetSummary, SearchReviews, SubmitReview

- [ ] **Task 3.12:** Implement ReviewService HTTP client
  - Location: `src/WebApp/Services/ReviewService.cs`
  - Call Catalog.API review endpoints
  - Handle authentication headers
  - Implement caching for summaries (5 min TTL)

- [ ] **Task 3.13:** Register ReviewService in DI
  - Update `src/WebApp/Extensions/Extensions.cs`
  - Configure HttpClient with base address

## Phase 4: Review Submission UI

### Review Form
- [ ] **Task 4.1:** Create ReviewSubmission page
  - Location: `src/WebApp/Components/Pages/ReviewSubmission.razor`
  - Route: `/review/{productId}` with optional `orderId` query param
  - Pre-populate product info and verified status

- [ ] **Task 4.2:** Implement star rating input component
  - Location: `src/WebApp/Components/Shared/StarRating.razor`
  - Interactive 1-5 star selection
  - Visual feedback on hover
  - Reusable component

- [ ] **Task 4.3:** Create review form validation
  - Required: Rating, Title (10-200 chars), Content (50-5000 chars)
  - Client-side and server-side validation
  - Character counter for Title and Content

- [ ] **Task 4.4:** Implement review submission flow
  - Call ReviewService.SubmitReview
  - Show success confirmation
  - Redirect to product page or order history
  - Handle errors gracefully

- [ ] **Task 4.5:** Add review display to product detail pages
  - Show aggregate rating and review count
  - Display top 3-5 reviews with "See all" link
  - Filter controls (rating, verified, helpful)

### Product Page Enhancements
- [ ] **Task 4.6:** Add review summary to catalog item cards
  - Show star rating and review count
  - Location: Update existing catalog item component
  - Fetch aggregate data efficiently (cache or eager load)

- [ ] **Task 4.7:** Create product reviews page/section
  - Location: `src/WebApp/Components/Pages/ProductReviews.razor` or section in existing page
  - Paginated review list
  - Sorting: Most Recent, Highest Rating, Most Helpful
  - Filtering: By rating, verified only

## Phase 5: Data Seeding & Testing

### Sample Data
- [ ] **Task 5.1:** Create review seed data JSON
  - Location: `src/Catalog.API/Setup/reviews.json`
  - 500+ realistic reviews across 20+ products
  - Variety of ratings (normal distribution around 4.2)
  - Mix of verified and unverified
  - Diverse topics: fit, quality, durability, value, performance

- [ ] **Task 5.2:** Implement ReviewContextSeed
  - Location: `src/Catalog.API/Infrastructure/ReviewContextSeed.cs`
  - Similar to CatalogContextSeed
  - Load reviews.json and seed database
  - Generate embeddings for all seeded reviews

- [ ] **Task 5.3:** Update CatalogContextSeed to include reviews
  - Call ReviewContextSeed.SeedAsync after catalog items

### Unit Tests
- [ ] **Task 5.4:** ReviewRepository tests
  - Location: `tests/Catalog.UnitTests/ReviewRepositoryTests.cs` (create if needed)
  - Test GetByProductId, pagination, filtering
  - Test semantic search functionality
  - Test helpfulness recording

- [ ] **Task 5.5:** ReviewsApi endpoint tests
  - Location: `tests/Catalog.FunctionalTests/ReviewsApiTests.cs`
  - Test CRUD operations with authentication
  - Test authorization (can't edit others' reviews)
  - Test verified purchase validation

- [ ] **Task 5.6:** Expert ChatState tests
  - Location: `tests/WebApp.UnitTests/ExpertChatStateTests.cs` (create if needed)
  - Mock review service responses
  - Test AI function invocations
  - Verify review citations in responses

### E2E Tests
- [ ] **Task 5.7:** Playwright test: Submit review
  - Location: `e2e/SubmitReviewTest.spec.ts`
  - Navigate to product → Submit review → Verify display
  - Test authenticated flow

- [ ] **Task 5.8:** Playwright test: Expert mode interaction
  - Location: `e2e/ExpertModeTest.spec.ts`
  - Toggle Expert Mode → Ask for recommendations → Verify review citations
  - Test mode switching preserves context

- [ ] **Task 5.9:** Playwright test: Review helpfulness voting
  - Mark review helpful → Verify count updates

### Integration Tests
- [ ] **Task 5.10:** Test OrderShipped → Review request flow
  - Mock order shipping event
  - Verify review request event published
  - Verify verified purchase tracked

## Phase 6: Performance & Observability

### Optimization
- [ ] **Task 6.1:** Implement review summary caching
  - Cache aggregate review stats per product (Redis)
  - 5-minute TTL, invalidate on new review
  - Reduce database load for frequently viewed products

- [ ] **Task 6.2:** Optimize review embedding generation
  - Batch embedding generation (up to 100 reviews at once)
  - Background queue processing for non-blocking writes
  - Monitor OpenAI API usage and costs

- [ ] **Task 6.3:** Add database indexes for review queries
  - Index on (ProductId, Rating, CreatedAt DESC)
  - Index on (ProductId, IsVerifiedPurchase)
  - Index for embedding vector similarity search

### Monitoring
- [ ] **Task 6.4:** Add OpenTelemetry tracing for review operations
  - Instrument review submission endpoint
  - Instrument AI function calls in Expert Mode
  - Track embedding generation duration

- [ ] **Task 6.5:** Add custom metrics
  - Review submission rate
  - Expert Mode activation rate
  - Review-to-purchase conversion rate
  - Average helpfulness score per product

- [ ] **Task 6.6:** Create health check for review system
  - Verify database connectivity
  - Verify embedding generation service health
  - Add to ASP.NET Core health checks

## Phase 7: Documentation & Deployment

### Documentation
- [ ] **Task 7.1:** Update API documentation
  - Add review endpoints to Swagger/Scalar docs
  - Include request/response examples
  - Document authentication requirements

- [ ] **Task 7.2:** Create developer guide for review system
  - Location: `docs/ReviewSystem.md` (create if needed)
  - Architecture overview
  - How to extend with new review types
  - Embedding generation process

- [ ] **Task 7.3:** Update project.md with review capabilities
  - Add to Tech Stack and Architecture sections
  - Document review domain model
  - Update database per service list

### Deployment
- [ ] **Task 7.4:** Run database migrations in staging
  - Test migration on staging database
  - Verify data integrity
  - Monitor migration duration

- [ ] **Task 7.5:** Seed initial reviews in staging
  - Deploy with seed data enabled
  - Verify embeddings generated correctly
  - Test Expert Mode with real data

- [ ] **Task 7.6:** Deploy to production
  - Run production database migration
  - Seed initial reviews (or phase in gradually)
  - Monitor application health post-deployment

- [ ] **Task 7.7:** Configure feature flag for Expert Mode
  - Allow gradual rollout to users
  - A/B test Expert Mode default state
  - Monitor engagement metrics

## Phase 8: Post-Launch

### Monitoring & Iteration
- [ ] **Task 8.1:** Monitor review submission rate
  - Track first 30 days post-launch
  - Identify low-adoption products
  - Adjust review request timing if needed

- [ ] **Task 8.2:** Analyze Expert Mode usage patterns
  - User activation rate
  - Average session length in Expert Mode
  - Conversion rate comparison vs. Standard Mode

- [ ] **Task 8.3:** Review AI quality
  - Sample Expert Mode conversations
  - Verify review citations are accurate
  - Identify hallucination instances
  - Refine system prompts based on findings

- [ ] **Task 8.4:** Implement review moderation if spam detected
  - Add moderation queue
  - Flag suspicious reviews (spam keywords, extreme sentiment)
  - Admin interface for review approval

## Estimated Effort

| Phase | Tasks | Estimated Time | Priority |
|-------|-------|---------------|----------|
| Phase 1: Foundation | 1.1 - 1.14 | 2 weeks | Critical |
| Phase 2: Ordering Integration | 2.1 - 2.6 | 1 week | High |
| Phase 3: Chatbot Expert Mode | 3.1 - 3.13 | 1.5 weeks | Critical |
| Phase 4: Review Submission UI | 4.1 - 4.7 | 1 week | High |
| Phase 5: Data Seeding & Testing | 5.1 - 5.10 | 1 week | Critical |
| Phase 6: Performance & Observability | 6.1 - 6.6 | 0.5 weeks | Medium |
| Phase 7: Documentation & Deployment | 7.1 - 7.7 | 0.5 weeks | High |
| Phase 8: Post-Launch | 8.1 - 8.4 | Ongoing | Medium |

**Total Estimated Time:** 7-8 weeks

## Dependencies Between Tasks

```
Phase 1 (all) → Phase 2, Phase 3, Phase 4, Phase 5
Phase 2.1-2.3 → Phase 4 (verified purchase validation)
Phase 3.1-3.4 → Phase 3.5-3.7 (functions before state)
Phase 3.11-3.13 → Phase 3.1-3.4 (service before function usage)
Phase 1.1-1.6 → All other phases (database schema)
Phase 5.1-5.3 → Phase 5.7-5.9 (seed data before E2E tests)
Phase 1-5 → Phase 6 (optimization comes after core features)
Phase 1-6 → Phase 7 (deployment after implementation)
```

## Notes

- Tasks can be parallelized within constraints (e.g., UI work can start once API contracts defined)
- Phase 5 (Testing) should be ongoing throughout development, not just at end
- Phase 8 (Post-Launch) is continuous improvement based on real-world data
- Consider creating smaller PRs per task cluster rather than one massive PR
