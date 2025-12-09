# Expert Reviews Help Mode - Feature Summary

## What Was Created

I've created a complete OpenSpec change proposal for adding an Expert Help Mode to the eShop chatbot that provides AI-powered product recommendations based on customer reviews. This package includes:

### 📋 Documents Created

1. **`proposal.md`** - High-level feature overview
   - Problem statement and proposed solution
   - Goals and success metrics
   - Technical approach and architecture
   - Implementation phases (8 weeks)
   - Risks, dependencies, and open questions

2. **`tasks.md`** - Detailed implementation checklist
   - 8 phases with 90+ granular tasks
   - Organized by: Database, API, AI Integration, UI, Testing, Deployment
   - Estimated effort and task dependencies
   - Ready for developer assignment

3. **`design.md`** - Deep technical specification
   - Complete architecture diagrams
   - Database schema with SQL DDL
   - Domain models and DTOs
   - AI integration strategy with code examples
   - Performance, security, and monitoring considerations

## Feature Overview

### What It Does

**Current State:**
- Chatbot can search products, add to cart, get user info

**New Capabilities:**
- ✅ Customers can submit product reviews with ratings
- ✅ Reviews are linked to verified purchases from orders
- ✅ AI chatbot gains "Expert Mode" with review-aware intelligence
- ✅ AI recommends products based on actual customer feedback
- ✅ Semantic search across reviews (e.g., "show reviews mentioning durability")
- ✅ AI compares products using review insights
- ✅ Review embeddings enable contextual understanding

### How It Works

```
Customer browses products
    ↓
Opens chatbot → Toggles "Expert Mode"
    ↓
Asks: "Which snowboard is best for beginners?"
    ↓
AI searches reviews semantically
    ↓
AI responds: "The Daybird Shadow Black (4.7★, 142 reviews) is highly 
recommended for beginners. Verified buyers mention 'forgiving flex' 
and 'stability at low speeds.' 89% of beginners gave 5 stars."
    ↓
Customer makes informed purchase
    ↓
Order ships → Receives review request email 7 days later
    ↓
Submits review → Powers future recommendations
```

## Key Technical Decisions

### Architecture
- **Reviews stored in Catalog.API** (reviews belong to products)
- **PostgreSQL + pgvector** for embedding storage (existing tech)
- **Integration with Ordering.API** for verified purchases
- **Event-driven** review requests via RabbitMQ

### AI Strategy
- **Existing OpenAI/Ollama infrastructure** reused
- **Text embeddings** for semantic review search (text-embedding-3-small)
- **Enhanced system prompts** for Expert Mode
- **New AI functions**: GetProductReviews, SearchReviewsByTopic, CompareProductsByReviews

### Data Model
- `ProductReviews` table: ratings, content, embeddings, verified flag
- `ReviewHelpfulness` table: vote tracking (helpful/not helpful)
- `VerifiedPurchase` table: maps orders to eligible reviewers

## Implementation Timeline

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| 1. Foundation | 2 weeks | Database, domain models, basic APIs |
| 2. Integration | 1 week | Order event handling, verified purchases |
| 3. AI Enhancement | 1.5 weeks | Expert mode, review functions |
| 4. UI Polish | 1 week | Review forms, mode toggle |
| 5. Testing | 1 week | Unit, integration, E2E tests |
| 6. Performance | 0.5 weeks | Caching, optimization |
| 7. Deployment | 0.5 weeks | Migration, seeding, rollout |
| 8. Post-Launch | Ongoing | Monitoring, iteration |

**Total:** 7-8 weeks

## Success Metrics

- 40%+ chatbot users enable Expert Mode
- 15%+ increase in add-to-cart rate for recommended products
- 25%+ of orders result in review submissions
- 80%+ catalog items have 3+ reviews within 6 months

## Dependencies

**Must Have:**
- OpenAI or Ollama configured (already exists)
- PostgreSQL with pgvector (already exists)
- Current chatbot implementation (already exists)

**Nice to Have:**
- Initial seed data (500+ reviews across top products)
- Email service for review requests

## Next Steps

### To Create an OpenSpec Plan

1. **Review the proposal**: Read through `proposal.md` to ensure it aligns with vision
2. **Validate**: Run `openspec validate add-expert-reviews-help-mode --strict`
3. **Iterate**: Address any questions in "Open Questions" section
4. **Approve**: Get stakeholder sign-off on proposal
5. **Implement**: Work through `tasks.md` sequentially

### To Customize

- **Adjust scope**: Remove phases if MVP needs to be smaller
- **Change AI provider**: Works with both OpenAI and Ollama
- **Modify review schema**: Add custom fields (e.g., photos, aspect ratings)
- **Alter timeline**: Compress or expand based on team size

## Questions Answered

**Q: How do reviews integrate with existing architecture?**
A: Reviews extend the Catalog domain with new tables. Event-driven integration with Ordering.API tracks verified purchases. No changes to existing Order/Catalog entities.

**Q: How does Expert Mode differ from Standard Mode?**
A: Expert Mode uses enhanced system prompts and adds 4 new AI functions for retrieving and analyzing review data. Standard mode continues to work exactly as before.

**Q: What's the AI strategy?**
A: Reviews get embeddings (just like catalog items). AI can semantically search reviews to answer natural language questions. AI functions provide structured review data to LLM for reasoning.

**Q: How are verified purchases enforced?**
A: When an order ships, an integration event stores UserId→OrderId→ProductId mapping. Review submission checks this table to set `IsVerifiedPurchase` flag automatically.

**Q: What about spam/fake reviews?**
A: MVP includes verified purchase badges, helpfulness voting, and ownership validation. Future: add moderation queue and AI-powered spam detection.

**Q: Performance impact?**
A: Embedding generation is async (background service). Review summaries cached in Redis. Database indexes optimized for common queries. Expect <100ms API response for review retrieval.

## File Structure Created

```
openspec/changes/add-expert-reviews-help-mode/
├── proposal.md      # Feature overview, goals, risks
├── tasks.md         # 90+ implementation tasks
├── design.md        # Technical architecture, code examples
└── README.md        # This file
```

## Ready to Proceed?

This specification is now ready for:
- ✅ Technical review by architects
- ✅ Approval by product owners
- ✅ Task assignment to developers
- ✅ Integration into sprint planning

The design maintains eShop's architectural principles:
- Microservices boundaries respected
- DDD patterns followed (reviews as entities in Catalog aggregate)
- Event-driven integration
- Existing AI infrastructure leveraged
- .NET 10 and C# 13 best practices
- OpenTelemetry observability
- Aspire orchestration compatible

---

**Created:** 2025-12-09  
**Change ID:** `add-expert-reviews-help-mode`  
**Estimated Effort:** 7-8 weeks (1-2 developers)  
**Status:** Ready for Approval
