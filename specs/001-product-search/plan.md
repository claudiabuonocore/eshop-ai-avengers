# Implementation Plan: Product Search

**Branch**: `001-product-search` | **Date**: 2025-12-08 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-product-search/spec.md`

## Summary

Add user-facing search functionality to the eShop WebApp. The backend already supports text search (`GetItemsByName`) and semantic/AI search (`GetItemsBySemanticRelevance`) via Catalog.API. This feature focuses on **UI integration** - adding a search input, query parameter support, and optional search suggestions.

## Technical Context

**Language/Version**: C# 12 / .NET 10  
**Primary Dependencies**: Blazor (SSR + streaming), ASP.NET Core Minimal APIs  
**Storage**: PostgreSQL with pgvector (already configured)  
**Testing**: MSTest (unit), Playwright (E2E)  
**Target Platform**: Web (Blazor Server-Side Rendering)  
**Project Type**: Distributed microservices (WebApp + Catalog.API)  
**Performance Goals**: Search results < 500ms, suggestions < 200ms  
**Constraints**: Must work with existing Catalog.API endpoints  
**Scale/Scope**: Minimal new code - leveraging existing search APIs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Microservices Architecture | ✅ PASS | Search stays in Catalog.API bounded context |
| II. Domain-Driven Design | ✅ PASS | No new domain logic required |
| III. Event-Driven Communication | ✅ PASS | No cross-service events needed |
| IV. Infrastructure Standards | ✅ PASS | Uses existing PostgreSQL/pgvector |
| V. Code Quality | ✅ PASS | Will follow existing patterns |
| VI. Testing Strategy | ✅ PASS | Will add E2E test for search flow |

**Gate Result**: ✅ PASSED - No violations

## Project Structure

### Documentation (this feature)

```text
specs/001-product-search/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (existing API analysis)
├── data-model.md        # Phase 1 output (minimal - no new entities)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API additions if any)
└── tasks.md             # Phase 2 output
```

### Source Code (affected files)

```text
src/
├── WebAppComponents/
│   ├── Catalog/
│   │   ├── CatalogSearch.razor       # MODIFY: Add search input
│   │   └── CatalogSearch.razor.css   # MODIFY: Style search box
│   └── Services/
│       └── CatalogService.cs         # MODIFY: Add search with name param
├── WebApp/
│   └── Components/Pages/Catalog/
│       └── Catalog.razor             # MODIFY: Handle search query param
└── Catalog.API/
    └── Apis/CatalogApi.cs            # NO CHANGE: Already has search endpoints

tests/
└── e2e/
    └── SearchTest.spec.ts            # NEW: E2E search tests
```

**Structure Decision**: Minimal changes to existing structure. Primary work in WebAppComponents (shared UI) and WebApp (page routing).

## Existing API Analysis

### Already Implemented in Catalog.API

| Endpoint | Method | Purpose | Status |
|----------|--------|---------|--------|
| `/api/catalog/items?name={term}` | GET | Text search (prefix match) | ✅ EXISTS |
| `/api/catalog/items/withsemanticrelevance?text={term}` | GET | AI/vector search | ✅ EXISTS |
| `/api/catalog/items?brand={id}&type={id}` | GET | Filter by brand/type | ✅ EXISTS |

### Already Implemented in WebAppComponents

| Service Method | Purpose | Status |
|----------------|---------|--------|
| `GetCatalogItems(page, size, brand, type)` | Get filtered items | ✅ EXISTS |
| `GetCatalogItemsWithSemanticRelevance(page, size, text)` | AI search | ✅ EXISTS |

### Missing (To Implement)

| Component | What's Missing |
|-----------|----------------|
| CatalogSearch.razor | Text input field for search |
| Catalog.razor | Query parameter `?q=` handling |
| CatalogService | Combined text + filter search |

## Implementation Phases

### Phase 1: Basic Text Search (P1)

1. **Add search input to CatalogSearch.razor**
   - Text input with search icon
   - Form submission navigates with `?q=` parameter
   - Clear button to reset search

2. **Update Catalog.razor**
   - Add `[SupplyParameterFromQuery(Name = "q")]` parameter
   - Pass search term to `CatalogService.GetCatalogItems()`

3. **Update CatalogService**
   - Modify `GetCatalogItems()` to accept optional `name` parameter
   - Call `/api/catalog/items?name={term}` when provided

### Phase 2: Combined Search + Filters (P2)

1. **Enable search + brand/type filter combination**
   - URL: `/?q=backpack&brand=1&type=2`
   - All filters applied server-side

2. **Display result count**
   - Show "X results for 'term'" header

### Phase 3: Search Suggestions (P3 - Optional)

1. **Add suggestions endpoint to Catalog.API**
   - New endpoint: `GET /api/catalog/items/suggestions?q={prefix}`
   - Returns top 5 matching product names

2. **Add autocomplete to search input**
   - Debounced API calls (300ms)
   - Dropdown showing suggestions

### Phase 4: AI-Powered Search (P4 - Optional)

1. **Toggle for semantic search**
   - Use existing `GetCatalogItemsWithSemanticRelevance` when AI enabled
   - Fallback to text search when AI unavailable

## API Contract (Phase 3 Addition)

```yaml
# New endpoint for suggestions
GET /api/catalog/items/suggestions
  parameters:
    - name: q
      in: query
      required: true
      schema:
        type: string
        minLength: 2
  responses:
    200:
      content:
        application/json:
          schema:
            type: array
            items:
              type: object
              properties:
                id: integer
                name: string
                brand: string
```

## Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|------------|
| Search performance on large catalog | Low | Existing index on Name column |
| Breaking existing filter functionality | Medium | E2E tests cover filter flows |
| AI search unavailable | Low | Graceful fallback to text search |

## Testing Strategy

| Test Type | Coverage |
|-----------|----------|
| E2E | Search flow: enter term → see results |
| E2E | Search + filter: term + brand → filtered results |
| E2E | Empty results: search non-existent term → friendly message |
| Unit | CatalogService handles null/empty search |

## Complexity Tracking

> No constitution violations - this section intentionally empty.

## Dependencies

- No new NuGet packages required
- No database migrations required
- No new infrastructure required

## Next Steps

Run `/speckit.tasks` to generate the task breakdown for implementation.
