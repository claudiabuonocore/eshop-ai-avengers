# Tasks: Product Search

**Input**: Design documents from `/specs/001-product-search/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: E2E tests included as specified in plan.md (Playwright)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Includes exact file paths in descriptions

## Path Conventions

Based on plan.md structure:
- **WebAppComponents**: `src/WebAppComponents/` (shared UI components)
- **WebApp**: `src/WebApp/` (main web application)
- **Catalog.API**: `src/Catalog.API/` (backend API)
- **E2E Tests**: `e2e/` (Playwright tests)

---

## Phase 1: Setup

**Purpose**: Verify existing infrastructure and prepare for changes

- [x] T001 Verify Catalog.API search endpoints are functional by testing `GET /api/catalog/items?name=test`
- [x] T002 [P] Review existing CatalogSearch.razor component in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [x] T003 [P] Review existing CatalogService.cs in `src/WebAppComponents/Services/CatalogService.cs`
- [x] T004 [P] Review ICatalogService interface in `src/WebAppComponents/Services/ICatalogService.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Update shared service interface to support search parameter

**⚠️ CRITICAL**: Must complete before any user story UI work

- [x] T005 Update ICatalogService interface to add optional `name` parameter to `GetCatalogItems()` in `src/WebAppComponents/Services/ICatalogService.cs`
- [x] T006 Update CatalogService implementation to pass `name` parameter to API call in `src/WebAppComponents/Services/CatalogService.cs`

**Checkpoint**: Service layer ready - UI implementation can begin

---

## Phase 3: User Story 1 - Basic Text Search (Priority: P1) 🎯 MVP

**Goal**: Customers can search for products by typing keywords in a search box

**Independent Test**: Enter "backpack" in search box → see products with "backpack" in name

### E2E Test for User Story 1

- [x] T007 [US1] Create E2E test for basic search flow in `e2e/SearchTest.spec.ts`

### Implementation for User Story 1

- [x] T008 [P] [US1] Add search input HTML to CatalogSearch component in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [x] T009 [P] [US1] Add search input CSS styles in `src/WebAppComponents/Catalog/CatalogSearch.razor.css`
- [x] T010 [US1] Add SearchTerm parameter to CatalogSearch component in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [x] T011 [US1] Add query parameter binding `[SupplyParameterFromQuery(Name = "q")]` in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [x] T012 [US1] Pass SearchTerm from Catalog page to CatalogSearch component in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [x] T013 [US1] Update GetCatalogItems call to include search term in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [x] T014 [US1] Handle empty search results with friendly message in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [x] T014.5 [US1] Add highlight styling for matching search terms in product cards in `src/WebAppComponents/Catalog/CatalogListItem.razor`
- [x] T015 [US1] Add clear search button functionality in `src/WebAppComponents/Catalog/CatalogSearch.razor`

**Checkpoint**: Basic search functional - users can search by keyword

---

## Phase 4: User Story 2 - Search with Filters (Priority: P2)

**Goal**: Customers can combine search with brand/type filters

**Independent Test**: Search "jacket", select brand filter → see only jackets from that brand

### E2E Test for User Story 2

- [ ] T016 [US2] Add E2E test for search + filter combination in `e2e/SearchTest.spec.ts`

### Implementation for User Story 2

- [ ] T017 [US2] Ensure search term preserved when filter is applied in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T018 [US2] Update filter links to preserve `q` parameter in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T019 [US2] Add result count display header in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [ ] T020 [US2] Style result count header in `src/WebApp/Components/Pages/Catalog/Catalog.razor.css`

**Checkpoint**: Search + filters work together seamlessly

---

## Phase 5: User Story 3 - Search Suggestions (Priority: P3)

**Goal**: Customers see autocomplete suggestions as they type

**Independent Test**: Type "back" → see dropdown with "Backpack", "Backpacker Pro", etc.

### Backend for User Story 3

- [ ] T021 [US3] Add suggestions endpoint to Catalog.API in `src/Catalog.API/Apis/CatalogApi.cs`
- [ ] T022 [US3] Add GetSuggestions method to ICatalogService in `src/WebAppComponents/Services/ICatalogService.cs`
- [ ] T023 [US3] Implement GetSuggestions in CatalogService in `src/WebAppComponents/Services/CatalogService.cs`

### Frontend for User Story 3

- [ ] T024 [US3] Add suggestions dropdown HTML to search input in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T025 [US3] Add suggestions dropdown CSS in `src/WebAppComponents/Catalog/CatalogSearch.razor.css`
- [ ] T026 [US3] Implement debounced input handler for suggestions in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T027 [US3] Handle suggestion click navigation in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T028 [US3] Add keyboard navigation for suggestions (arrow keys, enter) in `src/WebAppComponents/Catalog/CatalogSearch.razor`

**Checkpoint**: Autocomplete suggestions enhance search UX

---

## Phase 6: User Story 4 - AI-Powered Semantic Search (Priority: P4)

**Goal**: Customers can search using natural language queries

**Independent Test**: Search "something to carry my laptop" → see laptop bags/backpacks

### Implementation for User Story 4

- [ ] T029 [US4] Add semantic search toggle/option to UI in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T030 [US4] Implement semantic search service call (using existing `GetCatalogItemsWithSemanticRelevance`) in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T031 [US4] Add graceful fallback when AI service unavailable in `src/WebAppComponents/Services/CatalogService.cs`
- [ ] T032 [US4] Display indicator when using AI search vs text search in `src/WebAppComponents/Catalog/CatalogSearch.razor`

**Checkpoint**: AI search provides enhanced discovery

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T033 [P] Add search analytics logging (anonymized) in `src/WebApp/Components/Pages/Catalog/Catalog.razor`
- [ ] T034 [P] Add input sanitization for search queries in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T035 [P] Add search query length validation (max 200 chars) in `src/WebAppComponents/Catalog/CatalogSearch.razor`
- [ ] T036 [P] Update HybridApp CatalogSearch if shared component doesn't auto-sync in `src/HybridApp/Components/Pages/Catalog/CatalogSearch.razor`
- [ ] T037 Run quickstart.md validation to verify all scenarios work
- [ ] T038 Update feature documentation with final implementation notes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verify existing infrastructure
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Story 1-4 (Phases 3-6)**: All depend on Foundational completion
  - US1 (P1): No dependencies on other stories - **MVP**
  - US2 (P2): No dependencies on other stories (can parallelize with US1)
  - US3 (P3): No dependencies on US1/US2 (can parallelize)
  - US4 (P4): No dependencies on US1-US3 (can parallelize)
- **Polish (Phase 7)**: Depends on desired user stories being complete

### Within Each User Story

- E2E test task → Implementation tasks (test-first for verification)
- UI component changes can parallelize where marked [P]
- Service changes before UI integration

### Parallel Opportunities

**Within Phase 1 (Setup)**:
```
T002, T003, T004 can run in parallel (different files)
```

**Within Phase 3 (US1)**:
```
T008 (HTML), T009 (CSS) can run in parallel
```

**Across User Stories (after Phase 2)**:
```
US1, US2, US3, US4 can all be worked on by different developers simultaneously
```

---

## Implementation Strategy

### MVP First (User Story 1 Only) ⭐ RECOMMENDED

1. Complete Phase 1: Setup (verify existing APIs)
2. Complete Phase 2: Foundational (service layer update)
3. Complete Phase 3: User Story 1 (basic search)
4. **STOP and VALIDATE**: Test search independently
5. Deploy/demo MVP

### Incremental Delivery

| Increment | User Stories | Value Delivered |
|-----------|--------------|-----------------|
| MVP | US1 | Basic keyword search |
| +1 | US1 + US2 | Search with filters |
| +2 | US1 + US2 + US3 | Autocomplete suggestions |
| Full | US1-US4 | AI-powered semantic search |

---

## Summary

| Metric | Count |
|--------|-------|
| Total Tasks | 39 |
| Setup Tasks | 4 |
| Foundational Tasks | 2 |
| US1 Tasks | 10 |
| US2 Tasks | 5 |
| US3 Tasks | 8 |
| US4 Tasks | 4 |
| Polish Tasks | 6 |
| Parallelizable Tasks | 14 |

**MVP Scope**: Phases 1-3 (16 tasks) delivers basic search functionality

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable after completion
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Backend APIs already exist - this is primarily UI work

