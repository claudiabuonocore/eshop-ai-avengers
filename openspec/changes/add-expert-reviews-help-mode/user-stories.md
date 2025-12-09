# User Stories: Expert Reviews Help Mode

**Change ID:** `add-expert-reviews-help-mode`  
**Version:** 1.0  
**Last Updated:** 2025-12-09

## Priority Legend

- **MVP:** Minimum Viable Product - essential for first release
- **Post-MVP:** Valuable enhancements after initial launch
- **Future:** Nice to have features for later iterations

---

## MVP Scope Decision

**Core Value Proposition:** Enable AI chatbot to give product recommendations based on real customer reviews.

**What's IN the MVP:**
- Customers can submit basic reviews (rating + text)
- Reviews display on product pages with ratings
- Expert Mode chatbot queries and cites reviews
- Semantic search to find relevant review content

**What's OUT of the MVP (Post-MVP or Future):**
- Verified purchase badges (requires complex integration event changes)
- Review request emails (requires delayed messaging infrastructure)
- Helpfulness voting (not essential for initial recommendations)
- Review editing/deletion (can moderate manually initially)
- Advanced filtering/sorting (pagination sufficient for MVP)
- Performance optimizations (can handle low initial volume)
- Review moderation queue (handle reports manually initially)

--- 

## Feature Epics

## Epic 1:

### US-1.1: Submit Product Review (MVP)
**As a** logged-in customer  
**I want to** write and submit a review with a rating and text  
**So that** I can share my experience with other customers

**Acceptance Criteria:**
- Can rate product 1-5 stars (required)
- Can write review title (10-200 characters, required)
- Can write review content (50-2000 characters, required)
- Basic form validation
- Success confirmation shown after submission
- Review appears on product page

**Tasks:** 4.1, 4.2, 4.3, 4.4, 1.9, 1.11

**Story Points:** 5

**MVP Notes:** Simplified - no character counter, reduced max length, no real-time validation

---

### ~~US-1.2: Verified Purchase Badge~~ (Post-MVP)
**Reason for deferral:** Requires OrderShipped integration event extension (breaking change, 4-6 hours). Can launch with all reviews treated equally, add verification later to build trust.

**Original Story Points:** 5

---

### ~~US-1.3: Review Request Email~~ (Post-MVP)
**Reason for deferral:** Requires delayed messaging infrastructure. Initial reviews can be seeded, organic submissions will come from engaged users finding the feature.

**Original Story Points:** 8

---

### ~~US-1.4: Edit My Review~~ (Future)
**Reason for deferral:** Users can contact support to edit reviews initially. Rare use case that doesn't block core value.

**Original Story Points:** 3

---

### ~~US-1.5: Delete My Review~~ (Future)
**Reason for deferral:** Users can contact support to delete reviews initially. Rare use case, can be handled manually.

**Original Story Points:** 2

---

## Epic 2: Review Discovery & Display

### US-2.1: View Product Reviews (MVP)
**As a** customer browsing products  
**I want to** see reviews and ratings for products  
**So that** I can make informed purchase decisions

**Acceptance Criteria:**
- Product detail page shows average rating (stars) and count
- Displays reviews in reverse chronological order (newest first)
- Simple pagination (20 per page)
- Reviews show: rating, title, content (truncated), date, user ID
- No reviews shows "Be the first to review" message

**Tasks:** 4.5, 4.6, 4.7, 1.9

**Story Points:** 5

**MVP Notes:** Simplified - removed "top reviews" logic, removed excerpt logic, simple list view

---

### ~~US-2.2: Filter and Sort Reviews~~ (Post-MVP)
**Reason for deferral:** Not essential for AI chatbot to function. Users can scroll through paginated list. Filtering/sorting adds complexity without blocking core value.

**Original Story Points:** 5

---

### ~~US-2.3: Mark Review as Helpful~~ (Post-MVP)
**Reason for deferral:** Helpfulness voting doesn't block MVP. Initial launch uses recency for display order. Can add voting later to surface quality content.

**Original Story Points:** 5

---

### ~~US-2.4: Review Summary Statistics~~ (Post-MVP)
**Reason for deferral:** Nice-to-have analytics. Basic count + average sufficient for MVP. Rating distribution chart is polish, not essential.

**Original Story Points:** 5

---

### ~~US-2.5: Anonymous Reviewer Privacy~~ (Future)
**Reason for deferral:** User IDs from authentication system are already abstracted. Can implement abbreviated display format post-launch if privacy concerns arise.

**Original Story Points:** 1

---

## Epic 3: AI-Powered Expert Mode

### US-3.1: AI Chatbot Accesses Review Data (MVP)
**As a** customer using the chatbot  
**I want to** get product recommendations based on customer reviews  
**So that** I trust the advice comes from real experiences

**Acceptance Criteria:**
- Chatbot can query review data via AI functions
- AI responses cite review feedback naturally
- Shows ratings when discussing products
- Mentions review count to establish credibility
- System prompt instructs AI to ground recommendations in reviews

**Tasks:** 3.1, 3.2, 3.5, 3.6, 3.11, 3.12, 3.13

**Story Points:** 8

**MVP Notes:** Merged US-3.1 and US-3.2. No separate "Expert Mode" toggle - chatbot always uses reviews when available. Simpler UX, same core value.

---

### US-3.2: Search Reviews by Topic (MVP)
**As a** customer asking about specific product features  
**I want to** the chatbot to find reviews mentioning those features  
**So that** I can learn what customers say about aspects I care about

**Acceptance Criteria:**
- Can ask "what do reviews say about durability?"
- Semantic search finds relevant reviews (not just keyword matching)
- Returns review excerpts with ratings
- Handles synonyms (e.g., "sturdy" and "durable")

**Tasks:** 3.3, 1.14, 1.12, 1.13

**Story Points:** 8

**MVP Notes:** Core semantic search functionality, essential for value prop

---

### ~~US-3.3: Compare Products Based on Reviews~~ (Post-MVP)
**Reason for deferral:** Comparison is valuable but not essential for MVP. Users can ask about products individually. Comparison logic adds complexity.

**Original Story Points:** 8

---

### ~~US-3.4: Review Citation Formatting~~ (Post-MVP)
**Reason for deferral:** AI will naturally cite reviews in its responses. Fancy formatting with special UI components is polish, not essential for understanding.

**Original Story Points:** 3

---

### ~~US-3.5: Insufficient Reviews Acknowledgment~~ (Post-MVP)
**Reason for deferral:** Can be added to system prompt without dedicated story. AI will naturally hedge when data is limited.

**Original Story Points:** 2

---

## Technical Enablers (Non-User Stories)

### TE-1: Semantic Review Search Infrastructure (MVP)
**Technical Need:** Review content must be searchable by semantic meaning for AI to find relevant reviews

**Acceptance Criteria:**
- All reviews have vector embeddings generated
- Embeddings use text-embedding-3-small (384 dims)
- Search by cosine similarity
- Background service processes new reviews (asynchronous, best effort)
- pgvector index for performance

**Tasks:** 1.12, 1.13, 1.14, 1.1, 1.5

**Effort:** 13 points

**MVP Notes:** No SLA on embedding generation time. Can be slow initially.

---

### TE-2: Seed Review Data (MVP)
**Technical Need:** Database seeded with review data for demonstration

**Acceptance Criteria:**
- 200+ reviews across 15+ popular products
- Mix of ratings (around 4.0 average)
- Varied content covering common topics
- All reviews have embeddings generated

**Tasks:** 5.1, 5.2, 5.3

**Effort:** 5 points

**MVP Notes:** Reduced from 500 to 200 reviews. Enough to demonstrate functionality.

---

### TE-3: Database Migration Execution (MVP)
**Technical Need:** Database schema deployed safely

**Acceptance Criteria:**
- Migration tested in staging
- Rollback script available
- ProductReviews table with embedding column
- Basic indexes created

**Tasks:** 1.6, 7.4

**Effort:** 3 points

**MVP Notes:** Simplified migration, fewer indexes initially

---

### TE-4: Basic API Tests (MVP)
**Technical Need:** Core functionality tested to prevent regressions

**Acceptance Criteria:**
- Unit tests for ReviewRepository (basic CRUD)
- API endpoint tests for submit and get reviews
- Tests run in CI/CD pipeline

**Tasks:** 5.4, 5.5

**Effort:** 5 points

**MVP Notes:** Reduced test coverage target to 60%. Focus on happy paths.

---

### TE-5: Basic E2E Test (MVP)
**Technical Need:** End-to-end validation of core flow

**Acceptance Criteria:**
- Playwright test: submit review and see it displayed
- Playwright test: chatbot asks about reviews and gets response

**Tasks:** 5.7, 5.8

**Effort:** 3 points

**MVP Notes:** Just 2 critical paths, not exhaustive coverage

---

### ~~TE-6: Review System Health Monitoring~~ (Post-MVP)
**Reason for deferral:** Can monitor via standard application logs initially. Dedicated health checks are operational polish.

**Original Effort:** 3 points

---

### ~~TE-7: Review Performance Optimization~~ (Post-MVP)
**Reason for deferral:** Low traffic initially. Can optimize based on actual usage patterns after launch.

**Original Effort:** 8 points

---

### ~~TE-8: Integration Event Testing~~ (Post-MVP)
**Reason for deferral:** No integration events in MVP (no verified purchases). Not needed until post-MVP.

**Original Effort:** 5 points

---

### ~~TE-9: Feature Flag for Expert Mode~~ (Post-MVP)
**Reason for deferral:** No separate expert mode in MVP. Feature is on for everyone who has chatbot.

**Original Effort:** 3 points

---

### ~~TE-10: Review System Documentation~~ (Post-MVP)
**Reason for deferral:** Code comments sufficient for MVP. Comprehensive docs can be written after launch.

**Original Effort:** 5 points

---

### ~~TE-11: Review Quality Analytics~~ (Future)
**Reason for deferral:** Can gather data manually initially. Analytics dashboard is post-MVP enhancement.

**Original Effort:** 5 points

---

### ~~TE-12: AI Response Quality Monitoring~~ (Future)
**Reason for deferral:** Manual spot-checking sufficient for MVP. Systematic monitoring comes later.

**Original Effort:** 3 points

---

### ~~TE-13: Review Moderation Queue~~ (Future)
**Reason for deferral:** Handle abusive reviews manually via direct database updates. Low volume initially.

**Original Effort:** 8 points

---

## MVP Summary

### MVP User Stories: 4 Stories, 21 Story Points
1. **US-1.1:** Submit Product Review (5 pts)
2. **US-2.1:** View Product Reviews (5 pts)
3. **US-3.1:** AI Chatbot Accesses Review Data (8 pts)
4. **US-3.2:** Search Reviews by Topic (8 pts)

### MVP Technical Enablers: 5 Items, 29 Points
1. **TE-1:** Semantic Review Search Infrastructure (13 pts)
2. **TE-2:** Seed Review Data (5 pts)
3. **TE-3:** Database Migration Execution (3 pts)
4. **TE-4:** Basic API Tests (5 pts)
5. **TE-5:** Basic E2E Test (3 pts)

**MVP Total: 4 User Stories + 5 Technical Enablers = 50 Story Points**

---

### Post-MVP Enhancements: 10 Items, 61 Points
- US-1.2: Verified Purchase Badge (5 pts) - Trust building
- US-1.3: Review Request Email (8 pts) - User engagement
- US-2.2: Filter and Sort Reviews (5 pts) - Better UX
- US-2.3: Mark Review as Helpful (5 pts) - Quality surfacing
- US-2.4: Review Summary Statistics (5 pts) - Quick insights
- US-3.3: Compare Products Based on Reviews (8 pts) - Advanced AI
- US-3.4: Review Citation Formatting (3 pts) - Polish
- TE-6: Review System Health Monitoring (3 pts)
- TE-7: Review Performance Optimization (8 pts)
- TE-10: Review System Documentation (5 pts)
- TE-9: Feature Flag (3 pts)

---

### Future Enhancements: 9 Items, 39 Points
- US-1.4: Edit My Review (3 pts)
- US-1.5: Delete My Review (2 pts)
- US-2.5: Anonymous Reviewer Privacy (1 pt)
- US-3.5: Insufficient Reviews Acknowledgment (2 pts)
- TE-8: Integration Event Testing (5 pts)
- TE-11: Review Quality Analytics (5 pts)
- TE-12: AI Response Quality Monitoring (3 pts)
- TE-13: Review Moderation Queue (8 pts)

---

## Scope Reduction Summary

**Original Scope:** 20 user stories + 13 enablers = 190 story points  
**MVP Scope:** 4 user stories + 5 enablers = 50 story points  
**Reduction:** 74% reduction in scope

**Key MVP Decisions:**
- ✅ Keep: Core review submission, display, AI integration, semantic search
- ❌ Cut: Verified purchases (complex integration), email notifications, helpfulness voting, filtering/sorting
- ❌ Cut: Edit/delete (handle manually), comparison feature (nice-to-have), advanced UI polish
- ❌ Cut: Performance optimization (premature), comprehensive monitoring, extensive documentation

**Why This MVP Still Delivers Value:**
1. Customers can submit reviews ✅
2. Reviews appear on product pages ✅
3. AI chatbot gives recommendations based on reviews ✅
4. Semantic search finds relevant review content ✅

**What We're Trading Off:**
- Initial reviews won't have "verified purchase" badges (all treated equally)
- No automated email reminders to review (organic submissions only)
- No helpfulness voting (show most recent first)
- Basic UI (no advanced filtering)
- Manual moderation for abusive content

---

## MVP Sprint Planning

### Sprint 1: Foundation (1 week)
**User Stories:** US-1.1  
**Technical Enablers:** TE-3  
**Work:**
- Create database migration (ProductReviews table with pgvector)
- Implement ProductReview entity and ReviewRepository
- Create ReviewsApi endpoints (POST, GET)
- Basic user ID storage from auth claims

**Points:** 8 (5 user stories + 3 enablers)

---

### Sprint 2: Semantic Search Infrastructure (1 week)
**Technical Enablers:** TE-1, TE-2  
**Work:**
- Extend CatalogAI for review embeddings
- Create background service for embedding generation
- Implement semantic search in ReviewRepository
- Seed 200 sample reviews with embeddings
- Add pgvector indexes

**Points:** 18 (all enablers)

---

### Sprint 3: UI & AI Integration (1 week)
**User Stories:** US-2.1, US-3.1, US-3.2  
**Work:**
- Add review display to product pages
- Create review submission form
- Implement AI functions: GetProductReviews, SearchReviewsByTopic
- Update chatbot system prompt to use reviews
- Create ReviewService HTTP client

**Points:** 21 (all user stories)

---

### Sprint 4: Testing & Polish (0.5 week)
**Technical Enablers:** TE-4, TE-5  
**Work:**
- Write unit tests for ReviewRepository
- Write API functional tests
- Create 2 Playwright E2E tests
- Bug fixes and polish

**Points:** 8 (all enablers)

---

**Total MVP Delivery Time: 3.5 weeks (17.5 working days)**

Assuming 2-person team with 15 point velocity per week:
- Week 1: 9 points ✅
- Week 2: 18 points (pair on complex embedding logic) ✅
- Week 3: 21 points (parallel work on UI + AI) ✅
- Week 4: 8 points + launch prep ✅

---

## Post-MVP Roadmap

### Phase 2: Trust & Engagement (3 weeks)
**Focus:** Verified purchases, review requests, helpfulness voting  
**Stories:** US-1.2, US-1.3, US-2.3  
**Effort:** 18 points  
**Rationale:** Build trust with verified badges, drive more reviews via email

### Phase 3: Enhanced Discovery (2 weeks)
**Focus:** Filtering, sorting, summary statistics  
**Stories:** US-2.2, US-2.4  
**Effort:** 10 points  
**Rationale:** Improve review browsing experience

### Phase 4: Advanced AI (2 weeks)
**Focus:** Product comparison, citation formatting  
**Stories:** US-3.3, US-3.4  
**Effort:** 11 points  
**Rationale:** More sophisticated AI recommendations

### Phase 5: Operations & Scale (2 weeks)
**Focus:** Monitoring, performance, documentation  
**Enablers:** TE-6, TE-7, TE-10  
**Effort:** 16 points  
**Rationale:** Prepare for higher traffic, operational excellence

---

## Notes

- Story points estimated using Fibonacci sequence (1, 2, 3, 5, 8, 13)
- Assumes team velocity of ~20-25 points per sprint
- Dependencies between stories considered in sprint planning
- **User Stories** represent customer-facing features with business value
- **Technical Enablers** represent infrastructure, testing, and operational requirements necessary to support user stories
- P0 user stories must be completed for MVP launch
- P0 technical enablers are required to make P0 user stories functional
- P1 items significantly enhance user experience and trust
- P2/P3 items can be delivered post-launch based on user feedback and operational needs
