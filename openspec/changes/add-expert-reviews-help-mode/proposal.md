# Change Proposal: Add Expert Reviews Help Mode

**Change ID:** `add-expert-reviews-help-mode`  
**Status:** Draft  
**Created:** 2025-12-09  
**Owner:** Product Team

## Overview

Enhance the existing AI-powered chatbot with an "Expert Help Mode" that provides personalized product recommendations and guidance based on aggregated customer reviews and ratings. This feature will leverage the existing AI infrastructure (OpenAI/Ollama) and vector embeddings to deliver contextual, review-driven insights to customers.

## Problem Statement

Currently, the AdventureWorks chatbot can:
- Search the catalog using semantic search
- Add items to cart
- Retrieve user information
- Show cart contents

However, customers lack access to peer experiences and expert-level guidance when making purchase decisions. They cannot:
- See what other customers think about products
- Get recommendations based on real user experiences
- Understand product suitability for their specific needs based on community feedback
- Compare products based on review sentiment and ratings

## Proposed Solution

Implement a comprehensive review system integrated with the chatbot's Expert Help Mode, consisting of:

### 1. **Review Data Infrastructure**
- Add review storage to the Catalog domain (since reviews are intrinsically linked to products)
- Create `ProductReview` entity with ratings, comments, reviewer metadata, and helpfulness scores
- Store review embeddings for semantic search capabilities
- Support verified purchase indicators

### 2. **Review Aggregation & Analysis**
- Calculate aggregate metrics per product (average rating, review count, sentiment distribution)
- Generate AI-powered review summaries highlighting common themes
- Create semantic embeddings of reviews for similarity matching
- Support filtering by rating, date, verified purchases, and helpfulness

### 3. **Expert Help Mode Integration**
- Add new chatbot mode toggle (Standard vs. Expert Mode)
- Enhance AI prompts with review-aware instructions
- Add new AI functions for review retrieval and analysis
- Enable contextual recommendations based on user queries + review data

### 4. **Review Management APIs**
- REST endpoints for submitting, retrieving, and managing reviews
- Integration with Ordering.API to enable verified purchase reviews
- Moderation workflow support for review quality

## Goals

**Primary Goals:**
1. Enable customers to make more informed purchase decisions through peer feedback
2. Increase conversion rates by building trust through social proof
3. Provide AI-powered, context-aware product recommendations based on real usage patterns
4. Enhance the chatbot from basic search assistant to expert shopping advisor

**Secondary Goals:**
1. Collect valuable customer feedback for product improvement
2. Identify trending products and common pain points
3. Create a competitive advantage through superior shopping assistance
4. Increase customer engagement and time-on-site

## Success Metrics

- **Engagement:** 40%+ of chatbot users enable Expert Help Mode within first session
- **Conversion:** 15%+ increase in add-to-cart rate for products recommended by expert mode
- **Review Adoption:** 25%+ of completed orders result in review submission within 30 days
- **Customer Satisfaction:** Average chatbot helpfulness rating increases from baseline by 20%
- **Review Coverage:** 80%+ of catalog items have at least 3 reviews within 6 months

## Technical Approach

### Architecture Components

1. **Domain Model Extension (Catalog.API)**
   ```
   ProductReview Entity:
   - Id, ProductId, UserId, OrderId (nullable)
   - Rating (1-5), Title, Content
   - IsVerifiedPurchase, Helpfulness scores
   - CreatedAt, UpdatedAt
   - Embedding (pgvector for semantic search)
   ```

2. **Database Schema**
   - New `ProductReviews` table in catalogdb
   - New `ReviewHelpfulness` table for vote tracking
   - Indexes on ProductId, Rating, IsVerifiedPurchase, CreatedAt

3. **API Endpoints (Catalog.API)**
   ```
   GET  /api/catalog/items/{id}/reviews
   POST /api/catalog/items/{id}/reviews
   PUT  /api/catalog/reviews/{id}
   POST /api/catalog/reviews/{id}/helpful
   GET  /api/catalog/items/{id}/review-summary
   ```

4. **Integration Events**
   - `OrderShippedIntegrationEvent` → trigger review request email
   - `ReviewSubmittedIntegrationEvent` → notify for moderation if needed

5. **Chatbot Enhancements (WebApp)**
   - New `ExpertChatState` with review-aware system prompts
   - New AI functions:
     - `GetProductReviews(productId, filterOptions)`
     - `GetReviewSummary(productId)`
     - `CompareProductsByReviews(productIds[])`
     - `RecommendByReviews(userQuery, preferences)`
   - UI toggle for Standard/Expert mode
   - Display review highlights inline with product results

### AI Integration Strategy

**Review Embeddings:**
- Generate embeddings for each review using existing text-embedding-3-small model
- Store in pgvector column alongside catalog item embeddings
- Enable semantic search across reviews: "show me reviews mentioning durability"

**Expert Mode Prompts:**
```
System: You are an expert AdventureWorks product advisor with access to 
thousands of customer reviews. When recommending products, consider:
- Aggregate ratings and review sentiment
- Common praise points and concerns from verified buyers
- Suitability for the customer's stated needs based on review feedback
- Comparison insights from reviews across similar products

Always cite specific review insights when making recommendations.
```

**AI Function Examples:**
- User: "Which snowboard is best for beginners?"
- AI calls: `SearchCatalog("beginner snowboard")` + `GetReviewSummaries(productIds)`
- AI responds: "The Daybird Shadow Black Snowboard (4.7★, 142 reviews) is highly 
  recommended for beginners. Verified buyers consistently praise its 'forgiving 
  flex' and 'stability at low speeds.' 89% of beginner-identified reviewers 
  rated it 5 stars."

## User Experience

### For Customers
1. **Product Discovery Flow:**
   - Browse catalog normally OR ask chatbot for recommendations
   - Toggle Expert Help Mode to get review-driven guidance
   - See AI-generated review summaries alongside products
   - Ask specific questions: "Is this good for advanced skiers?" → AI surfaces relevant reviews

2. **Review Submission Flow:**
   - Receive email 7 days after order ships: "Share your experience"
   - Click through to WebApp with pre-filled product context
   - Submit rating, title, detailed feedback
   - Mark specific aspects (fit, durability, value, performance)
   - Verification badge shows for confirmed purchases

3. **Expert Mode Interactions:**
   - User: "I need ski goggles that don't fog up"
   - AI: Searches reviews for "fog" mentions, ranks products by anti-fog sentiment
   - AI: "The AirStrider Pro Goggles have the best anti-fog performance according 
     to 47 reviewer mentions. 91% of users who mentioned fogging gave 5 stars..."

### For Administrators (Future)
- Review moderation dashboard
- Sentiment analysis reports
- Product improvement insights from review themes

## Implementation Phases

### Phase 1: Foundation (Weeks 1-2)
- [ ] Database schema and migrations
- [ ] ProductReview domain model
- [ ] Basic CRUD API endpoints
- [ ] Review embeddings generation

### Phase 2: Integration (Weeks 3-4)
- [ ] Integration with Ordering.API for verified purchases
- [ ] Email notifications for review requests
- [ ] Review submission UI in WebApp
- [ ] Initial review data seeding

### Phase 3: AI Enhancement (Weeks 5-6)
- [ ] Expert mode chatbot functions
- [ ] Review-aware system prompts
- [ ] AI-powered review summarization
- [ ] Semantic review search

### Phase 4: UX Polish (Week 7)
- [ ] Expert mode toggle UI
- [ ] Inline review display in chatbot
- [ ] Review submission form refinement
- [ ] Rating display on product cards

### Phase 5: Testing & Launch (Week 8)
- [ ] E2E testing with Playwright
- [ ] Load testing with sample review data
- [ ] A/B testing framework for Expert Mode
- [ ] Production deployment

## Dependencies

**Technical:**
- Existing AI infrastructure (OpenAI/Ollama configured)
- PostgreSQL with pgvector extension (already in use)
- Current chatbot implementation (ChatState.cs)
- Ordering.API for purchase verification

**Data:**
- Initial seed data: minimum 500 diverse reviews across top 20 products
- Review templates and moderation guidelines

**External:**
- Email service for review request notifications (existing webhooks infrastructure)

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Negative reviews deter purchases | High | Implement helpfulness voting; AI focuses on balanced summary |
| Spam/fake reviews | Medium | Require verified purchases; add moderation queue |
| AI hallucinating review content | High | Always ground AI responses in actual review text; add citations |
| Performance degradation with embeddings | Medium | Index optimization; caching for review summaries |
| Low review volume initially | Medium | Seed with realistic synthetic reviews; incentivize early reviewers |
| Privacy concerns with review data | Medium | Anonymize reviewer names; clear consent on submission |

## Open Questions

1. **Review Moderation:** Automated with AI sentiment analysis or manual review queue?
   - **Recommendation:** Hybrid - AI flags suspicious reviews for manual review

2. **Review Incentives:** Offer discounts/points for verified reviews?
   - **Recommendation:** Start without incentives; monitor adoption rate

3. **Expert Mode Default:** Should Expert Mode be on by default for all users?
   - **Recommendation:** Default OFF; A/B test to determine optimal default

4. **Review Editing:** Allow users to edit reviews after submission?
   - **Recommendation:** Yes, within 30 days; track edit history

5. **Review Photos:** Support image uploads with reviews?
   - **Recommendation:** Phase 2 feature; start text-only for MVP

## Out of Scope (Future Enhancements)

- Review photos/videos
- Question & Answer section separate from reviews
- Social sharing of reviews
- Influencer/expert reviewer badges
- Multi-language review translation
- Review sentiment trend charts
- Comparison tables generated from review data
- Review-based product recommendations on homepage

## Approval Requirements

- [ ] Technical Lead - Architecture review
- [ ] Product Owner - Feature scope confirmation
- [ ] AI/ML Lead - Embedding strategy and prompt design
- [ ] Security Team - Review data privacy compliance
- [ ] UX Designer - Expert mode interaction flows

## References

- Existing Chatbot: `/src/WebApp/Components/Chatbot/ChatState.cs`
- Catalog API: `/src/Catalog.API/Apis/CatalogApi.cs`
- Ordering Integration: `/src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs`
- AI Infrastructure: `/src/eShop.AppHost/Extensions.cs`
- Project Context: `/openspec/project.md`
