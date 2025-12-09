# Implementation Plan: Epic 1 – Customer Review Submission (US-1.1)

**Change ID:** `add-expert-reviews-help-mode`  
**Scope:** MVP submission + display prerequisites  
**Goal:** Enable logged-in customers to submit a review (rating, title, content), with basic validation, and have it appear on product pages.

---

## Assumptions
- PostgreSQL + EF Core are used; `pgvector` arrives in TE-1 (not required for US-1.1).
- `ProductId` aligns to Catalog domain (commonly `int`); adjust if Catalog uses `Guid`.
- Use a dedicated `Reviews.API` to isolate review concerns; avoid changing `Catalog.API` for MVP.
- `UserId` sourced from auth claims (`sub` or `nameidentifier`), no PII beyond identifier.
- Web UI changes live in `src/WebApp` product detail page; simple Razor/Blazor form with server-side validation.

---

## Impacted Paths
- `src/Reviews.API/` (new service for reviews)
- `src/Shared/Reviews/` (shared DTOs/contracts)
- `src/WebApp/` (product detail review list + submission form; typed HTTP client)
- `eShop.slnx`, `eShop.Web.slnf` (solution updates)
- `src/eShop.AppHost/` (optional: local orchestration wiring if used)
- `tests/Reviews.*` and `e2e/*.spec.ts` (tests per TE-4, TE-5 once endpoints/UI exist)

---

## Step 1: Data Model & Migration
- **Entity:** `ProductReview`
  - `Id: Guid` (PK)
  - `ProductId: int` (or `Guid` per Catalog)
  - `UserId: string` (claim identifier)
  - `Rating: int` (1–5)
  - `Title: string` (10–200 chars)
  - `Content: string` (50–2000 chars)
  - `CreatedAt: DateTime` (UTC)
- **Indexes:** `IX_ProductReviews_ProductId`, `IX_ProductReviews_CreatedAt`.
- **DB Constraints:** `Rating BETWEEN 1 AND 5`, `Title NOT NULL`, `Content NOT NULL`, length via column types.
- **Files:**
  - `src/Reviews.API/Models/ProductReview.cs`
  - `src/Reviews.API/Infrastructure/ReviewsDbContext.cs`
  - `src/Reviews.API/Infrastructure/Migrations/*`
  - `src/Reviews.API/appsettings.json`
- **Program Wiring:** Register `AddDbContext<ReviewsDbContext>()` with Postgres.
- **Rollback:** EF down migration drops `ProductReviews` and indexes.

---

## Step 2: Repository & Service Layer
- **Interface:** `IReviewRepository`
  - `Task<ProductReview> AddAsync(ProductReview review)`
  - `Task<(IReadOnlyList<ProductReview> Items, int TotalCount)> GetByProductAsync(productId, page, pageSize)`
  - `Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(productId)`
- **Implementation:** `ReviewRepository` (EF Core).
- **Validation (server):** Rating ∈ [1..5]; Title length [10..200]; Content length [50..2000].
- **Files:**
  - `src/Reviews.API/Application/IReviewRepository.cs`
  - `src/Reviews.API/Application/ReviewRepository.cs`
- **Rollback:** Interface/implementation removable without cross-service impact.

---

## Step 3: API Endpoints (MVP)
- **POST** `/api/reviews`
  - Body: `ProductId`, `Rating`, `Title`, `Content`.
  - Auth required (`[Authorize]`); `UserId` from `HttpContext.User`.
  - Returns `201 Created` with review payload.
- **GET** `/api/reviews/{productId}`
  - Query: `page`, `pageSize` (defaults: `page=1`, `pageSize=20`).
  - Returns newest-first list.
- **GET** `/api/reviews/{productId}/aggregate`
  - Returns `{ averageRating, reviewCount }`.
- **Files:**
  - `src/Reviews.API/Controllers/ReviewsController.cs`
  - `src/Reviews.API/Program.cs` (DI registrations, minimal OpenAPI if present).
- **Rollback:** Remove controller; client calls fail gracefully if hidden behind a config flag.

---

## Step 4: Shared Contracts
- **DTOs:**
  - `SubmitReviewRequest { ProductId, Rating, Title, Content }`
  - `ReviewItemDto { Id, ProductId, UserId, Rating, Title, Content, CreatedAt }`
  - `ReviewAggregateDto { AverageRating, ReviewCount }`
  - `PagedResult<T> { Items, TotalCount, Page, PageSize }`
- **Files:** `src/Shared/Reviews/Contracts.cs`
- **Rollback:** Remove DTOs without affecting other services.

---

## Step 5: WebApp Integration (Display + Submission)
- **Display on Product Detail:**
  - Average rating (stars) + count; "Be the first to review" when none.
  - Newest-first list with simple pagination (20 per page).
- **Submission Form (auth only):**
  - Fields: Rating 1–5, Title, Content.
  - Basic client checks; show server validation errors.
  - Success confirmation; refresh list.
- **HTTP Client:** `ReviewService` (typed `HttpClient`)
  - `SubmitAsync(SubmitReviewRequest)`
  - `GetByProductAsync(productId, page, pageSize)`
  - `GetAggregateAsync(productId)`
- **Files:**
  - `src/WebApp/Services/ReviewService.cs`
  - `src/WebApp/Pages/ProductDetails.cshtml` + `.cshtml.cs` (or Blazor component under `WebAppComponents` consistent with repo patterns)
  - `src/WebApp/Program.cs` (typed client registration; `ReviewsApiUrl` config)
- **Rollback:** Hide UI via `REVIEWS_ENABLED=false` config; keep code dormant.

---

## Step 6: Auth & User Identity
- Extract `UserId` from claims (`sub` preferred; fallback `nameidentifier`).
- `[Authorize]` on POST; GET endpoints are public.
- Ensure `Identity.API` provides required claims; do not store names/emails in review.

---

## Step 7: Tests (Basic per MVP)
- **Unit:** `ReviewRepository` CRUD, ordering, pagination.
  - Path: `tests/Reviews.UnitTests/`
- **Functional API:** POST/GET/aggregate happy paths.
  - Path: `tests/Reviews.FunctionalTests/`
- **E2E (Playwright):** Submit review -> see displayed.
  - Path: `e2e/SubmitReviewTest.spec.ts`
- CI runs under existing pipeline (`ci.yml`).

---

## Step 8: Configuration & Wiring
- Create `src/Reviews.API/Reviews.API.csproj` referencing `eShop.ServiceDefaults`.
- Add `appsettings.json` with Postgres connection; secure via environment vars in local/staging.
- Include project in `eShop.slnx`; wire in `eShop.AppHost` if Aspire/AppHost is used.
- Add health checks and minimal OpenAPI consistent with project conventions.

---

## Validation Rules (MVP)
- **Rating:** required, integer 1–5.
- **Title:** required, 10–200 characters.
- **Content:** required, 50–2000 characters.
- **Errors:** return `400` with concise messages; client surfaces inline.

---

## Compliance & Security (FedRAMP Considerations)
- **PII Minimization:** store only `UserId` claim; no names/emails.
- **XSS Safety:** HTML-encode content on display; server sanitization where appropriate.
- **Logging Hygiene:** avoid logging review bodies; log IDs and outcomes.
- **Abuse Mitigation:** basic rate limiting on POST; request size limits.
- **Data Retention:** document retention policy post-MVP; support manual deletions by admins.

---

## Rollback Plan
- **DB:** Apply EF down migration to drop `ProductReviews` and indexes.
- **API:** Remove `Reviews.API` project and DI registrations.
- **WebApp:** Disable via `REVIEWS_ENABLED=false` flag; remove UI sections if permanent rollback.
- **Tests:** Skip or remove review-related tests from CI.

---

## Open Questions
- Confirm `ProductId` type (`int` vs `Guid`) from Catalog domain.
- Confirm UI stack (Razor Pages vs Blazor components) for product detail view.
- Confirm base URL discovery for `Reviews.API` in local/AppHost environments.

---

## Next Step
Proceed to Step 2: implement `IReviewRepository` and `ReviewRepository`, plus DTO contracts, to unblock API endpoints.
