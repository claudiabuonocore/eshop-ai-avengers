# Quickstart: Product Search Implementation

## Prerequisites

- .NET 10 SDK installed
- Docker/Colima running (for PostgreSQL, Redis, RabbitMQ)
- eShop running via Aspire

## Start Development Environment

```bash
# From project root
dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj
```

Open the Aspire dashboard URL shown in console output.

## Development Workflow

### 1. Modify CatalogSearch Component

Edit: `src/WebAppComponents/Catalog/CatalogSearch.razor`

Add search input above the filter sections:

```razor
<div class="catalog-search">
    <form method="get" class="search-form">
        <input type="text" 
               name="q" 
               placeholder="Search products..." 
               value="@SearchTerm" />
        <button type="submit">Search</button>
    </form>
    
    <!-- Existing filter sections -->
    ...
</div>

@code {
    [Parameter] public string? SearchTerm { get; set; }
    // ... existing code
}
```

### 2. Update Catalog Page

Edit: `src/WebApp/Components/Pages/Catalog/Catalog.razor`

Add search parameter:

```razor
@code {
    [SupplyParameterFromQuery(Name = "q")]
    public string? SearchTerm { get; set; }
    
    // Update OnInitializedAsync to pass SearchTerm
}
```

### 3. Test Changes

1. Navigate to http://localhost:5000 (WebApp)
2. Enter search term in new search box
3. Verify URL updates to `/?q=yourterm`
4. Verify results filter based on search

### 4. Add E2E Test

Create: `e2e/SearchTest.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test('search returns matching products', async ({ page }) => {
  await page.goto('/');
  await page.fill('input[name="q"]', 'backpack');
  await page.click('button[type="submit"]');
  
  await expect(page).toHaveURL(/q=backpack/);
  await expect(page.locator('.catalog-items')).toBeVisible();
});
```

## Key Files to Modify

| File | Change |
|------|--------|
| `src/WebAppComponents/Catalog/CatalogSearch.razor` | Add search input |
| `src/WebAppComponents/Catalog/CatalogSearch.razor.css` | Style search input |
| `src/WebApp/Components/Pages/Catalog/Catalog.razor` | Handle `q` parameter |
| `src/WebAppComponents/Services/CatalogService.cs` | Pass name to API |
| `e2e/SearchTest.spec.ts` | New E2E tests |

## API Endpoints Used

| Endpoint | Purpose |
|----------|---------|
| `GET /api/catalog/items?name={term}` | Text search |
| `GET /api/catalog/items?name={term}&brand={id}&type={id}` | Search + filters |

## Troubleshooting

**Search not working?**
- Check browser network tab for API calls
- Verify Catalog.API is running in Aspire dashboard
- Check API response in browser dev tools

**Results not updating?**
- Blazor SSR may need page refresh
- Check `@attribute [StreamRendering]` is present

