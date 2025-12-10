# Data Model: Product Search

## Existing Entities (No Changes Required)

The search feature leverages existing data models. No new entities or schema changes required.

### CatalogItem (Existing)

```
CatalogItem
├── Id: int (PK)
├── Name: string (INDEXED - used for search)
├── Description: string
├── Price: decimal
├── PictureFileName: string
├── CatalogTypeId: int (FK)
├── CatalogBrandId: int (FK)
├── AvailableStock: int
├── Embedding: Vector (pgvector - for semantic search)
└── Relationships
    ├── CatalogType (navigation)
    └── CatalogBrand (navigation)
```

### CatalogBrand (Existing)

```
CatalogBrand
├── Id: int (PK)
└── Brand: string
```

### CatalogItemType (Existing)

```
CatalogItemType
├── Id: int (PK)
└── Type: string
```

## Search Query Flow

```
User Input          →    URL Parameter    →    API Query
─────────────────────────────────────────────────────────
"backpack"          →    ?q=backpack      →    WHERE Name LIKE 'backpack%'
"backpack" + brand  →    ?q=backpack&brand=1  →  WHERE Name LIKE 'backpack%' AND CatalogBrandId = 1
```

## New DTOs (View Models Only)

### SearchSuggestion (Phase 3)

For the suggestions endpoint response:

```csharp
public record SearchSuggestion(int Id, string Name, string? Brand);
```

This is a projection from `CatalogItem`, not a new database entity.

## Index Verification

Verify the following index exists or should be created:

```sql
-- Check for existing index
SELECT indexname, indexdef 
FROM pg_indexes 
WHERE tablename = 'CatalogItems';

-- Create if missing (migration)
CREATE INDEX IF NOT EXISTS IX_CatalogItems_Name 
ON catalog."CatalogItems" (Name);
```

## Vector Search (Existing)

The semantic search uses the `Embedding` column (pgvector):

```csharp
// Existing implementation in CatalogApi.cs
.OrderBy(c => c.Embedding!.CosineDistance(vector))
```

No changes required - this already works when AI is enabled.

