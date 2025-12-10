# E2E Tests for eShop Application

## Overview

This directory contains end-to-end tests for the eShop application using Playwright.

## Test Suites

### Core Functionality Tests
- **BrowseItemTest.spec.ts** - Tests for browsing and viewing product items
- **AddItemTest.spec.ts** - Tests for adding items to cart (requires login)
- **RemoveItemTest.spec.ts** - Tests for removing items from cart (requires login)
- **SearchTest.spec.ts** - Tests for product search functionality

### Backcountry Redesign Tests
- **BackcountryDesignTest.spec.ts** - Comprehensive tests for the backcountry UX redesign including:
  - **Mobile Responsiveness** (375px, 768px, 1440px viewports)
  - **Accessibility (A11y)** - WCAG 2.1 AA compliance
  - **Touch Target Sizes** - 44px minimum for mobile
  - **Keyboard Navigation**
  - **Color Contrast**
  - **ARIA Labels**
  - **Image Alt Text**
  - **Semantic HTML**
  - **No Horizontal Scroll**

## Running Tests

### Install Dependencies

```bash
npm install
```

This will install:
- `@playwright/test` - Playwright test framework
- `@axe-core/playwright` - Accessibility testing library
- `@types/node` - TypeScript definitions
- `dotenv` - Environment variable management

### Run All Tests

```bash
npx playwright test
```

### Run Specific Test Suite

```bash
# Run only backcountry redesign tests
npx playwright test BackcountryDesignTest

# Run only search tests
npx playwright test SearchTest

# Run only logged-in tests
npx playwright test --project="e2e tests logged in"

# Run only non-logged-in tests
npx playwright test --project="e2e tests without logged in"
```

### Run Tests with UI

```bash
npx playwright test --ui
```

### Run Tests in Debug Mode

```bash
npx playwright test --debug
```

### View Test Report

```bash
npx playwright show-report
```

## Test Configuration

Tests are configured in `playwright.config.ts`:

- **Base URL**: `http://localhost:5045` (Aspire dashboard URL)
- **Timeout**: 120 seconds (2 minutes) for local development
- **Retries**: 2 retries on CI, 0 locally
- **Browsers**: Chrome (default)
- **Projects**:
  - `setup` - Authentication setup
  - `e2e tests logged in` - Tests requiring authentication
  - `e2e tests without logged in` - Public tests

## Accessibility Testing

Accessibility tests use `@axe-core/playwright` to ensure WCAG 2.1 AA compliance:

```typescript
const accessibilityScanResults = await new AxeBuilder({ page })
  .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
  .analyze();

expect(accessibilityScanResults.violations).toEqual([]);
```

## Viewport Sizes Tested

### Mobile
- **iPhone SE**: 375x667px
- **Small Mobile**: 320x568px (smallest common viewport)

### Tablet
- **iPad**: 768x1024px

### Desktop
- **Standard Desktop**: 1440x900px

## What's Tested in Backcountry Redesign

### Mobile Responsiveness ✅
- Hamburger menu visibility and functionality
- Mobile search bar layout
- Hero section readability
- Product card grid (1 column)
- Touch target sizes (44px minimum)
- No horizontal scroll

### Desktop Features ✅
- Horizontal navigation menu
- Prominent header search bar
- Product hover effects (quick-view)
- Multi-column product grid (4 columns)
- Brands dropdown

### Tablet Adaptation ✅
- Compact navigation
- 2-3 column product grid
- Responsive typography

### Accessibility (A11y) ✅
- **WCAG 2.1 AA Compliance** - Automated axe scans
- **Keyboard Navigation** - Tab, Enter, Escape support
- **ARIA Labels** - All interactive elements labeled
- **Semantic HTML** - Proper heading hierarchy (h1 → h6)
- **Alt Text** - All images have descriptive alt attributes
- **Color Contrast** - Sufficient contrast ratios (4.5:1 minimum)
- **Focus Indicators** - Visible focus states on all interactive elements
- **Active States** - Current page indicated in navigation

### Search Functionality ✅
- Header search input and submission
- Search results display
- Results count and highlighting
- Clear search functionality

### Hero Section ✅
- Text readability with overlay
- White text on dark overlay (proper contrast)
- CTA button functionality
- Responsive typography scaling

### Product Cards ✅
- Price formatting ($XX.XX)
- Image aspect ratio (1:1)
- Add to cart button
- Quick-view on hover (desktop)

## Continuous Integration

Tests run automatically in CI with:
- 5-minute timeout for webServer startup
- 2 retries on failure
- Single worker (no parallelization)
- HTML report artifacts

## Troubleshooting

### App Not Starting
If tests fail because the app isn't starting:
1. Check that ports 5045 is available
2. Increase timeout in `playwright.config.ts`
3. Run app manually first: `dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj`

### Accessibility Violations
If a11y tests fail:
1. Check the HTML report for specific violations
2. Run `npx playwright test --debug` to inspect the page
3. Use browser DevTools Accessibility panel
4. Reference WCAG guidelines: https://www.w3.org/WAI/WCAG21/quickref/

### Flaky Tests
If tests are flaky:
1. Add explicit waits: `await page.waitForSelector()`
2. Wait for network idle: `await page.waitForLoadState('networkidle')`
3. Increase timeout for specific actions
4. Check for race conditions

## Contributing

When adding new tests:
1. Follow existing naming conventions (`*.spec.ts`)
2. Use descriptive test names
3. Group related tests with `test.describe()`
4. Add viewport specifications for responsive tests
5. Include accessibility checks where appropriate
6. Document any special setup requirements

