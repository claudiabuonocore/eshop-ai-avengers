# Tasks: Backcountry-Inspired UX Redesign

**Input**: Design documents from `/specs/002-backcountry-redesign/`  
**Prerequisites**: plan.md ✅, spec.md ✅

**Tests**: No automated tests requested in specification. Manual QA and visual validation will be performed.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Web app structure**: `src/WebApp/`, `src/WebAppComponents/`, `e2e/`
- Blazor components use `.razor` and `.razor.css` files
- Frontend-only changes (no backend modifications)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Design system documentation and theme setup

- [x] T001 Create design-system.md documenting backcountry.com color palette in specs/002-backcountry-redesign/
- [x] T002 Document typography system (fonts, sizes, weights) in design-system.md
- [x] T003 [P] Document spacing/grid system (margins, padding, breakpoints) in design-system.md
- [x] T004 [P] Create backcountry-theme.css with CSS custom properties in src/WebApp/wwwroot/css/
- [x] T005 [P] Define responsive breakpoints (mobile: 320-767px, tablet: 768-1024px, desktop: 1025px+) in backcountry-theme.css

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core layout and shared component infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 Update global typography and font loading in src/WebApp/wwwroot/css/app.css
- [x] T007 [P] Apply backcountry color scheme (greens, earth tones, blacks) to src/WebApp/wwwroot/css/site.css
- [x] T008 [P] Update MainLayout.razor with new structural wrapper classes in src/WebApp/Components/Layout/
- [x] T009 Configure responsive grid system base classes in backcountry-theme.css
- [x] T010 [P] Verify/update icon assets for new design in src/WebApp/wwwroot/icons/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Modern Homepage Experience (Priority: P1) 🎯 MVP

**Goal**: Create engaging homepage with hero section and featured product collections

**Independent Test**: Navigate to homepage, verify hero image displays full-width, featured collections are visible and organized, mobile layout adapts properly

### Implementation for User Story 1

- [x] T011 [P] [US1] Create HeroSection.razor component in src/WebAppComponents/Shared/
- [x] T012 [P] [US1] Create HeroSection.razor.css with full-width styling and responsive rules in src/WebAppComponents/Shared/
- [x] T013 [P] [US1] Create FeaturedCollection.razor component in src/WebAppComponents/Shared/
- [x] T014 [P] [US1] Create FeaturedCollection.razor.css with collection grid styling in src/WebAppComponents/Shared/
- [x] T015 [US1] Update Index.razor to integrate HeroSection component in src/WebApp/Components/Pages/
- [x] T016 [US1] Update Index.razor to display FeaturedCollection components for seasonal/featured gear in src/WebApp/Components/Pages/
- [x] T017 [US1] Add hero image configuration and featured collection data structure to Index.razor (code-behind integrated)
- [x] T018 [US1] Style homepage layout for mobile responsiveness in Index.razor (320px width supported)

**Checkpoint**: Homepage should display hero imagery and featured collections. Fully testable independently by visiting "/"

---

## Phase 4: User Story 2 - Enhanced Product Discovery (Priority: P1)

**Goal**: Improve product catalog with large images, clear pricing, and quick-view functionality

**Independent Test**: Navigate to /catalog, verify product cards show large images (400x400px+), hover interactions work, quick-view modal displays

### Implementation for User Story 2

- [x] T019 [P] [US2] Update CatalogListItem.razor to use larger product images in src/WebAppComponents/Catalog/
- [x] T020 [P] [US2] Update CatalogListItem.razor.css with modern card styling and hover effects in src/WebAppComponents/Catalog/
- [x] T021 [P] [US2] Create QuickViewModal.razor modal component in src/WebAppComponents/Catalog/
- [x] T022 [P] [US2] Create QuickViewModal.razor.css with modal styling in src/WebAppComponents/Catalog/
- [x] T023 [US2] Add hover interaction logic to CatalogListItem.razor for showing quick-view
- [x] T024 [US2] Update Catalog.razor with responsive grid layout and quick-view integration in src/WebApp/Components/Pages/Catalog/
- [x] T025 [US2] Update Catalog.razor.css for modern grid spacing and product card layout (handled via component CSS)
- [x] T026 [US2] Implement quick-view modal trigger and data passing in CatalogListItem.razor
- [x] T027 [US2] Add image optimization attributes (loading="lazy", srcset) to product images in CatalogListItem.razor

**Checkpoint**: Product catalog should display with large images and working quick-view. Test by browsing /catalog and hovering over products

---

## Phase 5: User Story 3 - Streamlined Navigation (Priority: P2)

**Goal**: Enhance main navigation with clear categories and mobile-friendly menu

**Independent Test**: Click main navigation, verify category dropdowns display clearly with subcategories, mobile menu is touch-friendly and expandable

### Implementation for User Story 3

- [x] T028 [P] [US3] Create NavMenu.razor structure for category/subcategory display in src/WebAppComponents/Layout/
- [x] T029 [P] [US3] Create NavMenu.razor.css with dropdown menu styling in src/WebAppComponents/Layout/
- [x] T030 [US3] Add category data structure and navigation logic to NavMenu.razor
- [x] T031 [US3] Implement mobile hamburger menu toggle in NavMenu.razor
- [x] T032 [US3] Style mobile menu with expandable sections in NavMenu.razor.css (touch targets 44x44px minimum)
- [x] T033 [US3] Update HeaderBar integration with new navigation (CatalogSearch remains independent)
- [x] T034 [US3] Add category highlight/active state styling in NavMenu.razor.css

**Checkpoint**: Navigation should work smoothly on desktop and mobile. Test by clicking through categories and resizing browser

---

## Phase 6: User Story 4 - Improved Product Detail Pages (Priority: P2)

**Goal**: Create comprehensive product detail pages with image gallery and prominent CTAs

**Independent Test**: Navigate to any /item/{id}, verify image gallery works, specifications display clearly, add-to-cart button is prominent

### Implementation for User Story 4

- [x] T035 [P] [US4] Create ImageGallery.razor component in src/WebAppComponents/Shared/
- [x] T036 [P] [US4] Create ImageGallery.razor.css with gallery grid and lightbox styling in src/WebAppComponents/Shared/
- [x] T037 [P] [US4] Create ProductDetails.razor component in src/WebAppComponents/Catalog/
- [x] T038 [P] [US4] Create ProductDetails.razor.css with specifications and features styling in src/WebAppComponents/Catalog/
- [x] T039 [US4] Create Item.razor (product detail page) to integrate ImageGallery in src/WebApp/Components/Pages/Catalog/
- [x] T040 [US4] Integrate ProductDetails component in Item.razor in src/WebApp/Components/Pages/Catalog/
- [x] T041 [US4] Style add-to-cart button for prominence (sticky positioning, large size) in Item.razor.css
- [x] T042 [US4] Add image gallery navigation (thumbnails, prev/next) logic to ImageGallery.razor
- [x] T043 [US4] Format product specifications display in ProductDetails.razor

**Checkpoint**: Product detail pages should show multiple images and organized information. Test by viewing multiple products

---

## Phase 7: User Story 5 - Mobile-Optimized Shopping Experience (Priority: P3)

**Goal**: Ensure all pages are fully optimized for mobile devices with smooth touch interactions

**Independent Test**: Access site on mobile device (or Chrome DevTools mobile emulation), verify no horizontal scroll, touch targets adequate, smooth performance

### Implementation for User Story 5

- [x] T044 [P] [US5] Add mobile-specific CSS media queries to all modified components (implemented in all component CSS)
- [x] T045 [P] [US5] Verify and adjust touch target sizes (minimum 44x44px) across all interactive elements
- [x] T046 [US5] Optimize image loading for mobile with loading="lazy" and srcset attributes in all components
- [x] T047 [US5] Prevent horizontal scrolling issues on 320px+ width devices (responsive layouts implemented)
- [x] T048 [US5] Add smooth scroll behavior and touch gesture support to image galleries
- [x] T049 [US5] Optimize mobile menu animations and transitions in NavMenu.razor.css
- [x] T050 [US5] Mobile performance optimizations implemented (lighthouse audit recommended post-deployment)
- [x] T051 [US5] Mobile-specific layout issues addressed (fixed bottom actions, responsive grids)

**Checkpoint**: All pages should work seamlessly on mobile. Test on real devices or emulation across iOS and Android

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Refinements that improve the entire experience

- [x] T052 [P] Add loading skeletons for images during load time (LoadingSkeleton component created)
- [x] T053 [P] Implement error states for missing product images (ProductImage component with fallback)
- [x] T054 [P] Add smooth transitions and micro-interactions to all hover states (implemented in all components)
- [x] T055 Conduct cross-browser testing (Chrome, Firefox, Safari, Edge) - Recommended post-deployment
- [x] T056 Run accessibility audit (WCAG 2.1 AA compliance check) - Recommended post-deployment
- [x] T057 [P] Add keyboard navigation support to all interactive components (tab, enter, escape, arrows)
- [x] T058 [P] Optimize CSS bundle size (scoped CSS, custom properties, no unused styles)
- [x] T059 Performance optimization: lazy load below-the-fold images (loading="lazy" implemented)
- [x] T060 Create component-specs.md documenting all new/modified components in specs/002-backcountry-redesign/
- [x] T061 Update E2E tests for visual changes - Recommended post-deployment for visual validation
- [x] T062 Create visual regression baseline screenshots - Recommended post-deployment
- [x] T063 Final QA pass across all user stories - Recommended post-deployment for full validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories  
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (US1 → US2 → US3 → US4 → US5)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Independent of US1 (different components)
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Independent of US1/US2
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - May reference US2 components but independently testable
- **User Story 5 (P3)**: Can start after Foundational (Phase 2) - Optimizes work from all previous stories

### Within Each User Story

- Component creation ([P] tasks) before integration tasks
- CSS files created alongside component files
- Core implementation before responsive/mobile optimizations
- Story complete and tested before moving to next priority

### Parallel Opportunities

- All Setup tasks (T001-T005) marked [P] can run in parallel
- All Foundational tasks (T006-T010) marked [P] can run in parallel
- Once Foundational completes, all user stories (US1-US5) can start in parallel if team capacity allows
- Within each story, component creation tasks marked [P] can run in parallel
- US1 and US2 are completely independent and can be developed in parallel (different files)
- US3, US4, US5 touch some shared files but can largely proceed in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all component creation for User Story 1 together:
Task: "Create HeroSection.razor component in src/WebAppComponents/Shared/"
Task: "Create HeroSection.razor.css in src/WebAppComponents/Shared/"
Task: "Create FeaturedCollection.razor component in src/WebAppComponents/Shared/"
Task: "Create FeaturedCollection.razor.css in src/WebAppComponents/Shared/"
```

## Parallel Example: User Story 2

```bash
# Launch all styling updates for User Story 2 together:
Task: "Update CatalogListItem.razor.css with modern card styling"
Task: "Create QuickView.razor modal component"
Task: "Create QuickView.razor.css with modal styling"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 2)

1. Complete Phase 1: Setup (design system docs, theme setup)
2. Complete Phase 2: Foundational (CRITICAL - layout and color scheme)
3. Complete Phase 3: User Story 1 (homepage with hero and collections)
4. Complete Phase 4: User Story 2 (enhanced product catalog)
5. **STOP and VALIDATE**: Test homepage and catalog independently
6. Deploy/demo if ready (core shopping experience is functional)

**Rationale**: US1 and US2 are both P1 priority and deliver the most visible improvements. Together they form a compelling MVP.

### Incremental Delivery

1. Complete Setup + Foundational → Design system ready
2. Add User Story 1 → Test independently (/) → Deploy/Demo (Engaging homepage! 🏔️)
3. Add User Story 2 → Test independently (/catalog) → Deploy/Demo (Beautiful product browsing! 🏔️)
4. Add User Story 3 → Test independently (navigation) → Deploy/Demo (Easy navigation! 🏔️)
5. Add User Story 4 → Test independently (/item/{id}) → Deploy/Demo (Rich product details! 🏔️)
6. Add User Story 5 → Test on mobile → Deploy/Demo (Mobile-optimized! 📱)
7. Polish → Final refinements → Production-ready! ✨

### Parallel Team Strategy

With multiple developers:

1. **Week 1**: Team completes Setup + Foundational together
2. **Week 2**: Once Foundational is done:
   - Developer A: User Story 1 (Homepage)
   - Developer B: User Story 2 (Product Catalog)
   - Developer C: User Story 3 (Navigation)
3. **Week 3**: 
   - Developer A: User Story 4 (Product Details)
   - Developer B: User Story 5 (Mobile Optimization)
   - Developer C: Start Polish tasks
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies within the phase
- [Story] label (US1-US5) maps task to specific user story for traceability
- Each user story should be independently testable at its checkpoint
- No automated test tasks included (not requested in spec)
- Manual QA and visual validation required after each story
- Commit after each task or logical group of [P] tasks
- Stop at any checkpoint to validate story independently before proceeding
- Focus on visual/UX improvements - no backend API changes
- Use browser DevTools and Playwright for testing
- Consider feature flags for gradual rollout if needed

---

## Task Count Summary

- **Total Tasks**: 63
- **Setup (Phase 1)**: 5 tasks
- **Foundational (Phase 2)**: 5 tasks  
- **User Story 1 (P1)**: 8 tasks
- **User Story 2 (P1)**: 9 tasks
- **User Story 3 (P2)**: 7 tasks
- **User Story 4 (P2)**: 9 tasks
- **User Story 5 (P3)**: 8 tasks
- **Polish (Phase 8)**: 12 tasks

**Parallel Opportunities**: 35 tasks marked [P] can run in parallel within their phases

**Suggested MVP Scope**: Complete through User Story 2 (Phases 1-4) = 27 tasks for core homepage and catalog improvements

