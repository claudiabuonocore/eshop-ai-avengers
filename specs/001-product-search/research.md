# Research: Product Search

## Existing Search Capabilities

### Decision: Leverage Existing Catalog.API Endpoints

**Rationale**: The backend already implements comprehensive search functionality:

1. **Text Search** (`GetItemsByName`, `GetAllItems` with name parameter)
   - Prefix matching: `WHERE Name.StartsWith(name)`
   - Already exposed at `/api/catalog/items?name={term}`

2. **Semantic/AI Search** (`GetItemsBySemanticRelevance`)
   - Uses pgvector for cosine distance similarity
   - Requires OpenAI or Ollama for embeddings
   - Falls back to text search when AI unavailable
   - Exposed at `/api/catalog/items/withsemanticrelevance?text={term}`

**Alternatives Considered**:
- Full-text search with PostgreSQL `tsvector` - Rejected: Would require schema changes, prefix search is sufficient for MVP
- Elasticsearch integration - Rejected: Over-engineering for current scale

## UI Implementation Approach

### Decision: Enhance CatalogSearch.razor Component

**Rationale**: 
- Component already handles brand/type filtering
- Adding search input maintains single responsibility for catalog filtering
- Shared via WebAppComponents for use in WebApp and HybridApp

**Alternatives Considered**:
- New separate SearchBar component - Rejected: Would require more integration points
- Search in HeaderBar - Rejected: CatalogSearch already positioned for filtering context

## URL Parameter Strategy

### Decision: Use `q` query parameter

**Rationale**:
- Standard convention (`?q=term`)
- Shareable URLs: `/?q=backpack&brand=1`
- Browser back button works naturally
- SEO-friendly if SSR

**Alternatives Considered**:
- `search` parameter - Rejected: `q` is more standard
- Hash-based routing - Rejected: Not SSR compatible

## Search Suggestions

### Decision: Server-side suggestions via new endpoint

**Rationale**:
- PostgreSQL `LIKE` query is efficient with index
- Server handles data filtering (no client-side catalog)
- Consistent with existing API patterns

**Implementation**:
```csharp
// New endpoint in CatalogApi.cs
api.MapGet("/items/suggestions", async (
    CatalogContext context,
    [Required, MinLength(2)] string q) =>
{
    var suggestions = await context.CatalogItems
        .Where(c => c.Name.StartsWith(q))
        .Take(5)
        .Select(c => new { c.Id, c.Name, Brand = c.CatalogBrand.Brand })
        .ToListAsync();
    return TypedResults.Ok(suggestions);
});
```

**Alternatives Considered**:
- Client-side filtering - Rejected: Would require loading all products
- Third-party autocomplete service - Rejected: Over-engineering

## Performance Considerations

### Findings

1. **Existing Index**: `CatalogItems` table likely has index on `Name` (verify)
2. **Query Optimization**: `StartsWith` translates to `LIKE 'term%'` which uses index
3. **Pagination**: Already implemented via `PaginationRequest`
4. **Caching**: Consider adding output caching for popular searches (future enhancement)

### Recommendation

No immediate performance concerns for MVP. Monitor query performance post-launch.

