# DBA Analysis: eShop Reference Application

## Executive Summary

The eShop reference application is a microservices-based e-commerce platform built on .NET 10 and .NET Aspire. From a database perspective, it employs a **polyglot persistence** strategy with PostgreSQL as the primary relational database and Redis for caching/session management. The architecture follows **Database-per-Service** pattern, ensuring data isolation between microservices, with event-driven synchronization via RabbitMQ.

---

## 1. Data Architecture Overview

### 1.1 Polyglot Persistence Strategy

The solution implements multiple database technologies optimized for specific workloads:

| Database Technology | Purpose | Services Using It |
|---------------------|---------|-------------------|
| **PostgreSQL 16+** with pgvector | Relational storage, vector search | Catalog, Ordering, Identity, Webhooks |
| **Redis** | In-memory caching, session state | Basket (primary storage) |
| **RabbitMQ** | Message broker (data in transit) | All services (event bus) |

### 1.2 Database-per-Service Pattern

Each microservice owns its database schema, ensuring:
- **Data autonomy**: No cross-database joins or direct table access
- **Independent scaling**: Each database can be sized/optimized separately
- **Technology flexibility**: Services can evolve database technology independently
- **Fault isolation**: Database failure in one service doesn't affect others

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Catalog.API│     │ Ordering.API│     │ Identity.API│     │  Basket.API │
│             │     │             │     │             │     │             │
│   [PG DB]   │     │   [PG DB]   │     │   [PG DB]   │     │  [Redis]    │
│ catalogdb   │     │ orderingdb  │     │ identitydb  │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

---

## 2. PostgreSQL Implementation

### 2.1 Database Instances

The solution provisions a single PostgreSQL server instance (`postgres`) with multiple logical databases:

```csharp
// src/eShop.AppHost/Program.cs
var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");
var identityDb = postgres.AddDatabase("identitydb");
var orderDb = postgres.AddDatabase("orderingdb");
var webhooksDb = postgres.AddDatabase("webhooksdb");
```

**DBA Observations:**
- **Container Image**: Uses `ankane/pgvector` to support vector similarity search (AI embeddings)
- **Lifetime**: Persistent containers preserve data across restarts (development)
- **Multi-tenancy**: Single server, multiple databases (cost-effective for dev/staging)
- **Production Consideration**: Should migrate to separate server instances or managed services

### 2.2 Connection Management

**Framework**: Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 (via Aspire)

```csharp
// Catalog.API configuration
builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", 
    configureDbContextOptions: dbContextOptionsBuilder =>
    {
        dbContextOptionsBuilder.UseNpgsql(builder =>
        {
            builder.UseVector(); // pgvector extension
        });
    });

// Ordering.API configuration
services.AddDbContext<OrderingContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("orderingdb"));
});
```

**Connection Pooling:**
- Aspire automatically configures connection pooling via Npgsql
- Default pool size: 100 connections per app instance
- Recommend monitoring: `pg_stat_activity` for connection count
- **Production tuning**: Set `MaxPoolSize` based on pod count × expected concurrency

**DbContext Lifetime:**
- **Catalog**: Pooled DbContext (recommended)
- **Ordering**: **Not pooled** (due to constructor with IMediator dependency)
  ```csharp
  // Pooling is disabled because of the following error:
  // The DbContext of type 'OrderingContext' cannot be pooled because 
  // it does not have a public constructor accepting a single parameter 
  // of type DbContextOptions
  ```
  **⚠️ Performance Impact**: Each request creates a new OrderingContext instance

---

## 3. Database Schemas

### 3.1 Catalog Database (`catalogdb`)

**Purpose**: Product catalog management with AI-powered semantic search

**Tables:**
- `CatalogBrands` - Product brands (e.g., Azure, .NET)
- `CatalogTypes` - Product categories (e.g., T-Shirt, Mug)
- `CatalogItems` - Core product catalog with inventory
- `IntegrationEventLog` - Outbox pattern for event publishing

**Key Features:**

#### Vector Search (pgvector)
```csharp
public class CatalogItem
{
    // ...
    [JsonIgnore]
    public Vector? Embedding { get; set; } // Stores AI embeddings for semantic search
}
```

**Extension:**
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

**Use Case**: Users can search products using natural language (e.g., "comfortable shirts for summer") instead of exact keyword matches.

**Performance Consideration**: Vector similarity searches can be expensive. Recommend:
- **Index**: Create IVFFlat or HNSW index on `Embedding` column
- **Query Pattern**: Use `LIMIT` to restrict result sets
- **Monitoring**: Track query execution time in `pg_stat_statements`

#### Inventory Management
```csharp
public int RemoveStock(int quantityDesired)
{
    int removed = Math.Min(quantityDesired, this.AvailableStock);
    this.AvailableStock -= removed;
    return removed;
}
```

**Concurrency Control**: EF Core's optimistic concurrency (via `RowVersion`) prevents inventory oversell.

**Migrations:**
```
20231009153249_Initial.cs
20231018163051_RemoveHiLoAndIndexCatalogName.cs
20231026091140_Outbox.cs
```

---

### 3.2 Ordering Database (`orderingdb`)

**Purpose**: Order management and payment processing

**Schema**: `ordering` (explicitly set)

**Tables:**
- `Orders` - Order header (aggregate root)
- `OrderItems` - Line items (owned entities)
- `Buyers` - Customer information
- `PaymentMethods` - Stored payment methods
- `CardTypes` - Lookup table for credit card types
- `ClientRequests` - Idempotency tracking
- `IntegrationEventLog` - Outbox pattern

**Domain-Driven Design (DDD) Implementation:**

```csharp
public class Order : Entity, IAggregateRoot
{
    private readonly List<OrderItem> _orderItems;
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
    
    // Encapsulated behavior - only way to add items
    public void AddOrderItem(int productId, string productName, 
        decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        // Business logic encapsulated in aggregate
        var existingOrderForProduct = _orderItems
            .SingleOrDefault(o => o.ProductId == productId);
        
        if (existingOrderForProduct != null)
        {
            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.SetNewDiscount(discount);
            }
            existingOrderForProduct.AddUnits(units);
        }
        else
        {
            var orderItem = new OrderItem(productId, productName, 
                unitPrice, discount, pictureUrl, units);
            _orderItems.Add(orderItem);
        }
    }
}
```

**Data Integrity Patterns:**
- **Aggregate Root**: Order controls all OrderItems modifications
- **Value Objects**: `Address` stored as owned entity (EF Core 2.0+ feature)
- **Invariant Enforcement**: Business rules enforced in domain model, not database constraints

**Transaction Management:**
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
```

**⚠️ DBA Concern**: Business logic in code means fewer database-level constraints. Requires:
- Thorough application testing
- Regular data integrity audits
- Database backups with point-in-time recovery

---

### 3.3 Identity Database (`identitydb`)

**Purpose**: User authentication and authorization (IdentityServer/Duende)

**Tables** (ASP.NET Identity + IdentityServer):
- `AspNetUsers` - User accounts
- `AspNetRoles` - Roles
- `AspNetUserRoles` - User-role mapping
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - Authentication tokens
- `AspNetRoleClaims` - Role claims
- IdentityServer operational tables (clients, grants, etc.)

**Framework**: Duende.IdentityServer 7.3.1 + ASP.NET Identity

**Seeding Strategy:**
```csharp
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();
```

**Security Considerations:**
- **Password Storage**: ASP.NET Identity uses PBKDF2 with HMAC-SHA256 (100,000 iterations)
- **Personal Data**: Contains PII (email, phone) - ensure encryption at rest
- **GDPR Compliance**: Implement data retention policies and user data export/deletion
- **Connection String Security**: Store in Azure Key Vault or similar secret manager

**⚠️ Production Recommendations:**
- Enable Transparent Data Encryption (TDE)
- Implement column-level encryption for sensitive fields
- Regular backup with encrypted storage
- Monitor failed login attempts in `AspNetUserLogins`

---

### 3.4 Webhooks Database (`webhooksdb`)

**Purpose**: Webhook subscription management

**Tables:**
- `Subscriptions` - Webhook endpoints and event types

**Schema:**
```csharp
public class WebhookSubscription
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string DestUrl { get; set; }     // Target webhook URL
    public string Token { get; set; }       // Authentication token
    public int Type { get; set; }           // Event type (enum)
    public string UserId { get; set; }      // Owner
}
```

**Indexes:**
```csharp
eb.HasIndex(s => s.UserId);
eb.HasIndex(s => s.Type);
```

**Query Patterns**: Filter by UserId and Type for webhook delivery.

---

## 4. Redis Implementation

### 4.1 Basket Service Storage

**Purpose**: Shopping cart (ephemeral, high-performance)

**Data Model:**
```csharp
public class CustomerBasket
{
    public string BuyerId { get; set; }
    public List<BasketItem> Items { get; set; } = [];
}
```

**Storage Pattern**: Key-value store
- **Key**: `basket:{buyerId}`
- **Value**: JSON-serialized `CustomerBasket`
- **TTL**: Not explicitly set (should add expiration for inactive carts)

**Repository Implementation:**
```csharp
public class RedisBasketRepository : IBasketRepository
{
    private readonly IConnectionMultiplexer _redis;
    
    // CRUD operations using IConnectionMultiplexer
}
```

**⚠️ DBA Concerns:**

1. **Data Persistence**: Redis is configured as in-memory only
   - **Risk**: Cart data lost on Redis restart
   - **Recommendation**: Enable AOF (Append-Only File) or RDB snapshots for production

2. **Memory Management**:
   - No `maxmemory` policy configured
   - **Recommendation**: Set `maxmemory` with `allkeys-lru` eviction policy
   - Monitor memory usage via `INFO memory`

3. **Connection Pooling**:
   - `IConnectionMultiplexer` is singleton (correct pattern)
   - Default: Multiplexed single connection (efficient)

4. **Backup Strategy**:
   - Current: None (acceptable for development)
   - Production: Daily RDB snapshots + AOF for durability

---

## 5. Data Synchronization Strategy

### 5.1 Outbox Pattern (Transactional Messaging)

**Problem**: Ensuring database updates and event publishing happen atomically.

**Solution**: Store events in the same database transaction as business data.

**Implementation:**

#### IntegrationEventLog Table
```csharp
public class IntegrationEventLogEntry
{
    public Guid EventId { get; private set; }
    public string EventTypeName { get; private set; }
    public string Content { get; private set; }          // JSON serialized event
    public EventStateEnum State { get; set; }            // NotPublished, InProgress, Published, PublishedFailed
    public int TimesSent { get; set; }
    public DateTime CreationTime { get; private set; }
    public Guid TransactionId { get; private set; }     // Links to DB transaction
}
```

**Workflow:**

1. **Save Event in Transaction**:
   ```csharp
   public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
   {
       var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);
       _context.Database.UseTransaction(transaction.GetDbTransaction());
       _context.Set<IntegrationEventLogEntry>().Add(eventLogEntry);
       return _context.SaveChangesAsync();
   }
   ```

2. **Retrieve Pending Events**:
   ```csharp
   public async Task<IEnumerable<IntegrationEventLogEntry>> 
       RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
   {
       return await _context.Set<IntegrationEventLogEntry>()
           .Where(e => e.TransactionId == transactionId && 
                       e.State == EventStateEnum.NotPublished)
           .ToListAsync();
   }
   ```

3. **Mark as Published**:
   ```csharp
   public Task MarkEventAsPublishedAsync(Guid eventId)
   {
       var eventLogEntry = _context.Set<IntegrationEventLogEntry>()
           .Single(ie => ie.EventId == eventId);
       eventLogEntry.State = EventStateEnum.Published;
       return _context.SaveChangesAsync();
   }
   ```

**Applied in DbContext:**
```csharp
// Catalog.API
builder.UseIntegrationEventLogs(); // Extension method adds IntegrationEventLog table

// Ordering.API
modelBuilder.UseIntegrationEventLogs();
```

**⚠️ DBA Monitoring:**
- **Table Growth**: `IntegrationEventLog` can grow large
  - **Recommendation**: Archive/purge published events older than retention period (e.g., 30 days)
  - **Index**: On `State` and `CreationTime` for cleanup queries
- **Failed Events**: Monitor `State = PublishedFailed` for retry logic
- **Transaction Log Impact**: Large event payloads increase WAL size

---

### 5.2 Event-Driven Architecture (RabbitMQ)

**Message Broker**: RabbitMQ (Aspire.RabbitMQ.Client)

**Event Flow:**
```
[Catalog.API] --PriceChanged--> [RabbitMQ] --Subscribe--> [Webhooks.API]
[Ordering.API] --OrderStatusChanged--> [RabbitMQ] --Subscribe--> [Basket.API, Webhooks.API]
```

**Sample Events:**
- `OrderStatusChangedToPaidIntegrationEvent`
- `OrderStatusChangedToShippedIntegrationEvent`
- `ProductPriceChangedIntegrationEvent`
- `OrderStartedIntegrationEvent`

**Configuration:**
```csharp
builder.AddRabbitMqEventBus("eventbus")
    .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, 
                     OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, 
                     OrderStatusChangedToPaidIntegrationEventHandler>();
```

**Data Consistency Model**: **Eventual Consistency**
- Services don't share databases
- Data propagates asynchronously via events
- **CAP Theorem Trade-off**: Availability + Partition Tolerance over Consistency

**DBA Implications:**
- **No Distributed Transactions**: No 2PC or XA transactions
- **Idempotency Required**: Event handlers must handle duplicate messages
- **Compensating Actions**: Saga pattern for multi-service workflows

---

## 6. Migration Strategy

### 6.1 Entity Framework Core Migrations

**Migration Files Location:**
- Catalog: `src/Catalog.API/Infrastructure/Migrations/`
- Ordering: `src/Ordering.Infrastructure/Migrations/`
- Identity: `src/Identity.API/Data/Migrations/`
- Webhooks: `src/Webhooks.API/Migrations/`

**Migration Commands:**
```bash
# Catalog
dotnet ef migrations add --context CatalogContext [migration-name]

# Ordering
dotnet ef migrations add --startup-project Ordering.API \
    --context OrderingContext [migration-name]
```

### 6.2 Automatic Migration on Startup (Development)

```csharp
// Identity.API/Program.cs
// Apply database migration automatically. Note that this approach is not
// recommended for production scenarios. Consider generating SQL scripts from
// migrations instead.
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();
```

**MigrationService:**
```csharp
public static IServiceCollection AddMigration<TContext>(
    this IServiceCollection services, 
    Func<TContext, IServiceProvider, Task> seeder)
    where TContext : DbContext
{
    // Executes migrations + seeding on startup
}
```

**⚠️ Production Recommendations:**

**DO NOT** use automatic migrations in production. Instead:

1. **Generate SQL Scripts**:
   ```bash
   dotnet ef migrations script --idempotent --output migration.sql
   ```

2. **Review Scripts**:
   - Check for table locks (ALTER statements)
   - Verify data migration logic
   - Estimate downtime (if any)

3. **Apply via CI/CD Pipeline**:
   - Use Azure DevOps / GitHub Actions
   - Apply during maintenance window
   - Include rollback plan

4. **Zero-Downtime Migrations**:
   - **Expand-Contract Pattern**:
     1. Add new column (nullable)
     2. Deploy code to populate new column
     3. Backfill historical data
     4. Deploy code to use new column
     5. Remove old column

---

### 6.3 Database Seeding

**Catalog Seeding:**
```csharp
public class CatalogContextSeed : IDbSeeder<CatalogContext>
{
    public async Task SeedAsync(CatalogContext context)
    {
        // Workaround from https://github.com/npgsql/efcore.pg/issues/292
        context.Database.OpenConnection();
        ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypes();
        
        if (!context.CatalogItems.Any())
        {
            var sourcePath = Path.Combine(contentRootPath, "Setup", "catalog.json");
            var sourceJson = File.ReadAllText(sourcePath);
            var sourceItems = JsonSerializer.Deserialize<CatalogSourceEntry[]>(sourceJson);
            
            // Insert brands, types, items
            // Generate AI embeddings for descriptions
        }
    }
}
```

**Identity Seeding (UsersSeed):** Creates default test users.

**⚠️ Production Considerations:**
- **Disable Seeding**: Remove or gate behind feature flag
- **Reference Data**: Load via deployment scripts, not code
- **Performance**: Bulk inserts for large datasets (`COPY` command)

---

## 7. Performance & Optimization Guidance

### 7.1 Query Performance

#### Catalog.API
**Common Query Patterns:**
- Paginated product listings (with filters)
- Vector similarity search
- Stock availability checks

**Index Recommendations:**
```sql
-- Existing indexes from EF migrations
CREATE INDEX IF NOT EXISTS IX_CatalogItems_CatalogBrandId ON "CatalogItems"("CatalogBrandId");
CREATE INDEX IF NOT EXISTS IX_CatalogItems_CatalogTypeId ON "CatalogItems"("CatalogTypeId");
CREATE INDEX IF NOT EXISTS IX_CatalogItems_Name ON "CatalogItems"("Name");

-- Additional recommendations
CREATE INDEX IF NOT EXISTS IX_CatalogItems_AvailableStock 
    ON "CatalogItems"("AvailableStock") 
    WHERE "AvailableStock" > 0; -- Partial index for in-stock items

-- Vector search index (pgvector)
CREATE INDEX IF NOT EXISTS IX_CatalogItems_Embedding_ivfflat 
    ON "CatalogItems" USING ivfflat ("Embedding" vector_cosine_ops) 
    WITH (lists = 100);
```

**Query Tuning:**
- Use `AsNoTracking()` for read-only queries
- Batch inventory updates to reduce round trips
- Monitor slow queries via `pg_stat_statements`

---

#### Ordering.API
**Common Query Patterns:**
- Order history by buyer
- Order status updates
- Payment method lookups

**Index Recommendations:**
```sql
CREATE INDEX IF NOT EXISTS IX_Orders_BuyerId ON "ordering"."Orders"("BuyerId");
CREATE INDEX IF NOT EXISTS IX_Orders_OrderStatus ON "ordering"."Orders"("OrderStatus");
CREATE INDEX IF NOT EXISTS IX_Orders_OrderDate ON "ordering"."Orders"("OrderDate" DESC);
CREATE INDEX IF NOT EXISTS IX_OrderItems_ProductId ON "ordering"."OrderItems"("ProductId");
```

**Aggregate Performance:**
- Orders with many line items can cause N+1 queries
- Use `.Include(o => o.OrderItems)` for eager loading
- Consider read models/projections for reporting queries

---

### 7.2 Connection Pooling Tuning

**Current Configuration**: Defaults (via Aspire/Npgsql)

**Recommended Production Settings:**
```json
{
  "ConnectionStrings": {
    "catalogdb": "Host=postgres.example.com;Database=catalogdb;Username=app;Password=***;Maximum Pool Size=50;Minimum Pool Size=5;Connection Lifetime=300;Timeout=30;"
  }
}
```

**Sizing Guidelines:**
- **Max Pool Size** = (Expected concurrent requests per pod × 1.2) ÷ Number of pods
- **Min Pool Size** = Max Pool Size ÷ 5 (keep 20% warm)
- **Connection Lifetime** = 300 seconds (rotate connections to handle DNS changes)

**Monitor:**
```sql
-- Active connections by database
SELECT datname, count(*) 
FROM pg_stat_activity 
GROUP BY datname;

-- Connection pool exhaustion
-- Check application logs for "Timeout expired" errors
```

---

### 7.3 Transaction Isolation Levels

**Default**: `Read Committed` (PostgreSQL default)

**Ordering Domain (Critical Section):**
```csharp
// Explicit transaction for order placement
using var transaction = await _context.Database.BeginTransactionAsync(
    IsolationLevel.Serializable); // Highest isolation to prevent phantom reads

try 
{
    // Check inventory
    // Create order
    // Save integration event
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Trade-offs:**
- `Serializable`: Maximum consistency, higher lock contention
- `Read Committed`: Better concurrency, risk of non-repeatable reads
- Recommendation: Use `Serializable` only for critical business transactions (e.g., payment processing)

---

### 7.4 Redis Performance

**Current Setup**: Single Redis instance (no persistence)

**Production Optimizations:**

1. **Memory Eviction Policy**:
   ```redis
   CONFIG SET maxmemory 2gb
   CONFIG SET maxmemory-policy allkeys-lru
   ```

2. **Connection Multiplexing**: Already using `IConnectionMultiplexer` (correct pattern)

3. **Basket Expiration**:
   ```csharp
   // Add TTL to inactive baskets
   await database.StringSetAsync(key, json, TimeSpan.FromDays(7));
   ```

4. **Monitoring**:
   ```redis
   INFO memory        # Memory usage
   INFO stats         # Hit/miss ratio
   SLOWLOG GET 10     # Slow commands
   ```

---

## 8. Backup & Recovery Strategy

### 8.1 Development Environment
**Current**: None (containers are ephemeral but marked `ContainerLifetime.Persistent`)

### 8.2 Production Recommendations

#### PostgreSQL Backup

**Full Backup Strategy:**
```bash
# Daily full backup
pg_dump -h postgres.example.com -U app -Fc catalogdb > catalogdb_$(date +%Y%m%d).dump

# Point-in-Time Recovery (PITR)
# Enable WAL archiving in postgresql.conf:
wal_level = replica
archive_mode = on
archive_command = 'cp %p /backup/wal_archive/%f'
```

**Retention Policy:**
- Daily backups: 30 days
- Weekly backups: 90 days
- Monthly backups: 1 year

**Backup Testing:**
- Quarterly restore drills
- Validate backup integrity with `pg_restore --list`

#### Disaster Recovery Plan

**RTO (Recovery Time Objective)**: < 4 hours  
**RPO (Recovery Point Objective)**: < 15 minutes

**Steps:**
1. Restore latest full backup
2. Replay WAL logs since backup
3. Verify data integrity (row counts, checksums)
4. Reconnect application services

#### Redis Backup

**Strategy**: RDB snapshots + AOF

```redis
# redis.conf
save 900 1      # Save if 1 key changed in 15 minutes
save 300 10     # Save if 10 keys changed in 5 minutes
save 60 10000   # Save if 10k keys changed in 1 minute

appendonly yes
appendfsync everysec
```

**Recovery**: Load from RDB + replay AOF

---

## 9. Security Considerations

### 9.1 Database Access Control

**Principle of Least Privilege:**

```sql
-- Application user (no DDL permissions)
CREATE USER app_catalog WITH PASSWORD 'strong_password';
GRANT CONNECT ON DATABASE catalogdb TO app_catalog;
GRANT USAGE ON SCHEMA public TO app_catalog;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_catalog;

-- Migration user (DDL permissions)
CREATE USER migration_user WITH PASSWORD 'strong_password';
GRANT ALL PRIVILEGES ON DATABASE catalogdb TO migration_user;
```

**Current Issue**: Connection strings likely use superuser credentials.

---

### 9.2 Secrets Management

**Current**: `appsettings.json` (development only)

**Production**:
- Azure Key Vault (for Azure deployments)
- Kubernetes Secrets (for on-prem K8s)
- Environment variables (encrypted via platform)

**Example (Azure Key Vault)**:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

### 9.3 Data Protection

**Encryption at Rest:**
- PostgreSQL: TDE (Transparent Data Encryption) via disk encryption
- Redis: Encryption at rest (Azure Cache for Redis Enterprise tier)

**Encryption in Transit:**
- PostgreSQL: `sslmode=Require` in connection string
- Redis: TLS enabled via `ssl=true` parameter

**PII Data (Identity DB):**
- ASP.NET Identity hashes passwords (PBKDF2)
- Consider encrypting email/phone fields at application level
- Implement data masking for non-production environments

---

## 10. Monitoring & Observability

### 10.1 Key Metrics to Monitor

#### PostgreSQL
- **Connection Count**: `pg_stat_activity`
- **Query Performance**: `pg_stat_statements` (enable extension)
- **Cache Hit Ratio**: `pg_stat_database.blks_hit / (blks_hit + blks_read)`
- **Replication Lag**: `pg_stat_replication` (if using replicas)
- **Disk Usage**: `pg_database_size('catalogdb')`
- **Lock Contention**: `pg_locks`, `pg_stat_activity.wait_event`

**Alerting Thresholds:**
- Connection pool utilization > 80%
- Cache hit ratio < 90%
- Query duration > 5 seconds (P99)

---

#### Redis
- **Memory Usage**: `INFO memory` (used_memory vs maxmemory)
- **Eviction Rate**: `INFO stats` (evicted_keys)
- **Hit Ratio**: `keyspace_hits / (keyspace_hits + keyspace_misses)`
- **Latency**: `SLOWLOG` for commands > 10ms

**Alerting Thresholds:**
- Memory usage > 80%
- Hit ratio < 95%
- Eviction rate increasing

---

### 10.2 Observability Integration

**Aspire Dashboard**: http://localhost:19888
- View service health, logs, traces
- Does not persist metrics (in-memory only)

**Production**: Integrate with:
- **Azure Monitor** (for Azure deployments)
- **Prometheus + Grafana** (for on-prem/hybrid)
- **OpenTelemetry**: Already configured via .NET Aspire

**Database-Specific Dashboards:**
- PostgreSQL: pgAdmin, pgBadger (log analyzer)
- Redis: RedisInsight

---

## 11. Production Deployment Recommendations

### 11.1 Managed Services (Azure)

**Recommended Architecture:**
- **Azure Database for PostgreSQL - Flexible Server**
  - Zone-redundant high availability
  - Automated backups (35-day retention)
  - Point-in-time restore
  - Read replicas for scaling
  - Built-in PgBouncer for connection pooling

- **Azure Cache for Redis**
  - Premium tier for persistence (RDB + AOF)
  - Geo-replication for DR
  - VNet integration for security

**Cost Optimization:**
- Use Burstable tier (B1ms) for non-production environments
- General Purpose tier (D-series) for production
- Right-size based on actual usage metrics (start smaller and scale up)

---

### 11.2 Kubernetes Deployment

**StatefulSet Considerations:**
- PostgreSQL: Use StatefulSet with persistent volumes
- Redis: StatefulSet for AOF persistence

**Helm Charts:**
- Bitnami PostgreSQL Helm Chart (recommended)
- Bitnami Redis Helm Chart

**Init Containers:**
- Run EF migrations as init container (not main app container)
- Use Kubernetes Jobs for one-time migration tasks

**Resource Limits:**
```yaml
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "1Gi"
    cpu: "500m"
```

---

### 11.3 Schema Change Management

**Flyway or Liquibase** (Alternative to EF Migrations):
- Version-controlled SQL scripts
- Baseline existing databases
- Better for large teams with DBAs

**GitOps Workflow:**
1. Developer creates EF migration
2. Generate SQL script: `dotnet ef migrations script`
3. Commit script to Git
4. DBA reviews SQL in PR
5. CI/CD applies script to staging
6. Manual approval for production deployment

---

## 12. Scalability Considerations

### 12.1 Read Replicas

**Catalog Database (Read-Heavy):**
- Configure PostgreSQL read replicas for product browsing queries
- Use `CommandType.StoredProcedure` or connection string routing to read replicas

**Pattern:**
```csharp
// Write to primary
await _context.SaveChangesAsync();

// Read from replica (separate connection string)
var products = await _readOnlyContext.CatalogItems
    .AsNoTracking()
    .Where(p => p.AvailableStock > 0)
    .ToListAsync();
```

---

### 12.2 Sharding Strategy (Future)

**Not currently implemented**, but potential strategies:

**Catalog Database:**
- Shard by `CatalogBrandId` (if some brands have massive catalogs)
- Citus extension for PostgreSQL (distributed tables)

**Ordering Database:**
- Shard by `BuyerId` or geography
- Each shard = independent PostgreSQL instance

**Challenges:**
- Cross-shard queries require application-level aggregation
- Distributed transactions more complex

---

### 12.3 Caching Strategies

**Current**: Redis only for Basket.API

**Recommended Enhancements:**

1. **Distributed Cache for Catalog**:
   ```csharp
   // Cache popular products
   IDistributedCache cache;
   var cachedProduct = await cache.GetStringAsync($"product:{id}");
   if (cachedProduct == null)
   {
       var product = await _context.CatalogItems.FindAsync(id);
       await cache.SetStringAsync($"product:{id}", 
           JsonSerializer.Serialize(product), 
           new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
       return product;
   }
   return JsonSerializer.Deserialize<CatalogItem>(cachedProduct);
   ```

2. **Cache Invalidation**:
   - Subscribe to `ProductPriceChangedIntegrationEvent`
   - Remove cached product on update

3. **Response Caching**:
   - `[ResponseCache]` attribute for GET endpoints
   - Vary by query parameters (page, filter)

---

## 13. Cost Optimization

### 13.1 Development Environment

**Current Cost**: Minimal (local Docker containers)

**Optimization**: Use lightweight images
- PostgreSQL: `postgres:16-alpine` instead of full image
- Redis: `redis:7-alpine`

---

### 13.2 Production Environment (Azure)

**Database Sizing:**
| Service | Workload Profile | Recommended SKU | Est. Monthly Cost |
|---------|------------------|-----------------|-------------------|
| Catalog DB | Read-heavy, moderate writes | D4s (4 vCPU, 16 GB) | ~$300 |
| Ordering DB | Write-heavy, OLTP | D2s (2 vCPU, 8 GB) | ~$150 |
| Identity DB | Low volume | B2s (2 vCPU, 4 GB) | ~$60 |
| Webhooks DB | Low volume | B1ms (1 vCPU, 2 GB) | ~$30 |
| Redis Cache | Basket storage | P1 (6 GB, persistence) | ~$150 |
| **Total** |  |  | **~$690/month** |

**Cost Reduction Strategies:**
- Use Azure Reserved Instances (1-year commitment) = 30-40% savings
- Downscale non-production environments after hours (Automation Runbook)
- Use single PostgreSQL instance with multiple databases (dev/test)

---

## 14. Rollback Strategies

### 14.1 Application Rollback

**Blue-Green Deployment:**
- Deploy new version alongside old version
- Route 10% traffic to new version (canary)
- Monitor error rates, latency
- Roll forward or roll back based on metrics

**Database Compatibility:**
- New code must work with **old schema** and **new schema** (during migration window)
- Use feature flags to toggle new behavior

---

### 14.2 Schema Rollback

**Expand-Contract Pattern** (recap):
1. **Expand**: Add new column (nullable)
2. **Deploy Code**: Write to both old and new column
3. **Backfill**: Populate new column for existing rows
4. **Contract**: Remove old column after full rollout

**If Rollback Needed:**
- Keep old column until new version is proven stable (N+1 deployments)
- Revert application code (app continues using old column)

**Emergency Rollback:**
```sql
-- Example: Revert column rename
ALTER TABLE "Orders" RENAME COLUMN "NewColumnName" TO "OldColumnName";
```

---

## 15. Compliance & Auditing

### 15.1 GDPR / Data Privacy

**Personal Data Locations:**
- **Identity DB**: Email, phone number, username
- **Ordering DB**: Buyer name, address (value object)
- **Webhooks DB**: UserId (no PII)

**Required Capabilities:**
1. **Data Export**: Implement API endpoint to export user data (JSON format)
2. **Right to be Forgotten**: Soft delete or anonymize user data
   ```sql
   UPDATE "AspNetUsers" 
   SET "Email" = 'deleted_user@example.com', 
       "PhoneNumber" = NULL,
       "NormalizedEmail" = 'DELETED_USER@EXAMPLE.COM'
   WHERE "Id" = @userId;
   ```
3. **Audit Logging**: Track data access (enable PostgreSQL query logging)

---

### 15.2 Audit Trail

**Current**: Limited audit capabilities

**Recommendations:**

1. **Database-Level Auditing** (PostgreSQL):
   ```sql
   -- Enable query logging
   ALTER DATABASE catalogdb SET log_statement = 'mod'; -- Log INSERT/UPDATE/DELETE
   ALTER DATABASE catalogdb SET log_min_duration_statement = 1000; -- Log slow queries
   ```

2. **Application-Level Auditing**:
   - Use MediatR behaviors to log all commands
   - Store audit events in separate `AuditLog` table
   - Include: UserId, Action, EntityType, EntityId, Timestamp, Changes (JSON)

3. **Change Data Capture (CDC)**:
   - PostgreSQL: Use `pgaudit` extension
   - Capture schema changes, permission changes

---

## 16. Disaster Recovery Scenarios

### Scenario 1: Single Database Corruption

**Symptoms**: Corrupted blocks in `catalogdb`

**Recovery Steps:**
1. Identify corrupt table: `SELECT * FROM pg_stat_database WHERE datname='catalogdb';`
2. Restore from latest backup
3. Replay WAL logs to RPO
4. Restart Catalog.API pods

**Estimated Downtime**: 30 minutes

---

### Scenario 2: Complete PostgreSQL Server Failure

**Symptoms**: All databases unavailable

**Recovery Steps:**
1. Provision new Azure PostgreSQL Flexible Server
2. Restore all databases from backup
3. Update connection strings in Key Vault
4. Restart all API pods
5. Verify data integrity

**Estimated Downtime**: 2-4 hours (depending on database size)

---

### Scenario 3: Redis Data Loss

**Symptoms**: All shopping carts lost

**Impact**: Users lose cart contents (poor UX, but no data corruption)

**Recovery Steps:**
1. Restart Redis with persistence enabled (RDB/AOF)
2. No data recovery possible (Redis was in-memory only)
3. Users re-add items to cart

**Mitigation**: Enable Redis persistence in production

---

## 17. Testing Strategies

### 17.1 Integration Tests

**Current Setup**: Functional tests use Aspire test fixtures

```csharp
public sealed class CatalogApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
    
    public CatalogApiFixture()
    {
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres("CatalogDB")
            .WithImage("ankane/pgvector")
            .WithImageTag("latest");
        _app = appBuilder.Build();
    }
}
```

**DBA Recommendations:**
- **Isolated Test Databases**: Each test run uses fresh database
- **Schema Verification**: Test that migrations run successfully
- **Performance Tests**: Benchmark query execution time (threshold: <100ms for simple queries)

---

### 17.2 Database Migration Testing

**Checklist:**
1. **Forward Migration**: Apply migration to empty database
2. **Backward Compatibility**: New code works with old schema
3. **Data Migration**: Verify data transformations (e.g., column rename)
4. **Rollback Test**: Revert migration and ensure old code works
5. **Performance Test**: Measure migration execution time on production-sized data

**CI/CD Integration:**
```yaml
# GitHub Actions example
- name: Test EF Migrations
  run: |
    dotnet ef database update --context CatalogContext
    dotnet test --filter Category=Integration
```

---

## 18. Common Pitfalls & Gotchas

### 18.1 N+1 Query Problem

**Symptom**: Hundreds of queries for a single page load

**Example (Ordering.API)**:
```csharp
// Bad: N+1 queries
var orders = await _context.Orders.ToListAsync(); // 1 query
foreach (var order in orders)
{
    var items = order.OrderItems; // N queries (lazy loading disabled in EF Core by default)
}

// Good: Single query with Include
var orders = await _context.Orders
    .Include(o => o.OrderItems)
    .Include(o => o.Buyer)
    .ToListAsync();
```

---

### 18.2 Implicit Transactions

**EF Core Behavior**: `SaveChangesAsync()` wraps in transaction by default

**Problem**: Forgetting to include integration events in transaction

```csharp
// Bad: Event saved outside transaction
await _context.SaveChangesAsync(); // Transaction 1
await _integrationEventService.SaveEventAsync(event); // Transaction 2 (could fail)

// Good: Both in same transaction
using var transaction = await _context.Database.BeginTransactionAsync();
await _context.SaveChangesAsync();
await _integrationEventService.SaveEventAsync(event, transaction);
await transaction.CommitAsync();
```

---

### 18.3 PostgreSQL Case Sensitivity

**Gotcha**: Unquoted identifiers are case-insensitive, quoted identifiers are case-sensitive

```sql
-- These are the same table
SELECT * FROM orders;
SELECT * FROM Orders;
SELECT * FROM ORDERS;

-- This is a DIFFERENT table
SELECT * FROM "Orders";
```

**EF Core Default**: Generates quoted identifiers (e.g., `"CatalogItems"`)

**Recommendation**: Stick with EF defaults, avoid manual SQL with case-sensitive names

---

## 19. Future Enhancements (DBA Perspective)

### 19.1 Read Models (CQRS)

**Problem**: Reporting queries (e.g., "Top 10 selling products") are inefficient on transactional DB

**Solution**: Materialized views or separate read database

```sql
-- Materialized view example
CREATE MATERIALIZED VIEW catalog_search_view AS
SELECT 
    ci."Id",
    ci."Name",
    ci."Price",
    ci."AvailableStock",
    cb."Brand",
    ct."Type"
FROM "CatalogItems" ci
JOIN "CatalogBrands" cb ON ci."CatalogBrandId" = cb."Id"
JOIN "CatalogTypes" ct ON ci."CatalogTypeId" = ct."Id"
WHERE ci."AvailableStock" > 0;

-- Refresh periodically
REFRESH MATERIALIZED VIEW catalog_search_view;
```

---

### 19.2 Time-Series Data (Order History)

**Current**: Orders stored indefinitely in `orderingdb`

**Future**: Archive old orders to cold storage

```sql
-- Partition orders by year
CREATE TABLE "Orders_2024" PARTITION OF "Orders"
    FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
CREATE TABLE "Orders_2025" PARTITION OF "Orders"
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
```

**Archive Strategy**:
- Move orders older than 2 years to Azure Blob Storage (Parquet format)
- Keep recent orders in hot PostgreSQL storage

---

### 19.3 Multi-Tenancy

**Current**: Single-tenant architecture

**Future**: Support multiple customers (tenants)

**Database Strategies:**
1. **Shared Database, Shared Schema**: Add `TenantId` column to all tables
2. **Shared Database, Separate Schemas**: Each tenant has own schema (`tenant1.Orders`, `tenant2.Orders`)
3. **Database per Tenant**: Each tenant has dedicated database (best isolation, higher cost)

**Recommendation**: Start with Strategy 1 (shared schema), migrate to Strategy 3 as customer base grows.

---

## 20. Conclusion & Summary

### Key Takeaways for DBAs

1. **Architecture**: Microservices with database-per-service pattern
2. **Databases**: PostgreSQL (4 instances) + Redis (1 instance)
3. **Data Sync**: Outbox pattern + RabbitMQ for eventual consistency
4. **Migrations**: EF Core migrations (auto-applied in dev, scripted for prod)
5. **Performance**: Index recommendations provided, connection pooling configured
6. **Backup**: Requires production backup strategy (RDB + WAL archiving)
7. **Security**: Secrets in Key Vault, TLS/SSL required, least-privilege access
8. **Monitoring**: OpenTelemetry + Azure Monitor or Prometheus

### Immediate Action Items (Production Readiness)

- [ ] **Disable automatic migrations** in production code
- [ ] **Generate SQL scripts** for all migrations
- [ ] **Configure backup strategy** (daily full + WAL archiving)
- [ ] **Enable PostgreSQL extensions**: `pg_stat_statements`, `pgaudit`
- [ ] **Create read replicas** for Catalog database
- [ ] **Set Redis persistence** (AOF + RDB snapshots)
- [ ] **Implement connection pooling limits** (MaxPoolSize tuning)
- [ ] **Add vector indexes** for Catalog.Embedding column
- [ ] **Create monitoring dashboards** (Grafana or Azure Monitor)
- [ ] **Document rollback procedures** for each service

---

## Appendix A: Connection String Reference

### Development (Docker Compose)
```bash
# PostgreSQL (auto-configured by Aspire)
Host=localhost;Port=5432;Database=catalogdb;Username=postgres;Password=<auto-generated>

# Redis (auto-configured by Aspire)
localhost:6379
```

### Production (Azure)
```bash
# Azure Database for PostgreSQL
Host=eshop-postgres.postgres.database.azure.com;Port=5432;Database=catalogdb;Username=app@eshop-postgres;Password=<KeyVault>;Ssl Mode=Require;Trust Server Certificate=true

# Azure Cache for Redis
eshop-redis.redis.cache.windows.net:6380,password=<KeyVault>,ssl=True,abortConnect=False
```

---

## Appendix B: Useful Queries

### PostgreSQL Health Checks
```sql
-- Database size
SELECT pg_database.datname, 
       pg_size_pretty(pg_database_size(pg_database.datname)) AS size
FROM pg_database
WHERE datname IN ('catalogdb', 'orderingdb', 'identitydb', 'webhooksdb');

-- Table sizes
SELECT schemaname, tablename,
       pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC
LIMIT 20;

-- Active connections
SELECT datname, usename, application_name, state, count(*)
FROM pg_stat_activity
GROUP BY datname, usename, application_name, state;

-- Long-running queries
SELECT pid, usename, datname, query, state, 
       now() - query_start AS duration
FROM pg_stat_activity
WHERE state = 'active' AND query_start < now() - interval '5 seconds'
ORDER BY duration DESC;

-- Index usage
SELECT schemaname, tablename, indexname, idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
WHERE idx_scan = 0 AND schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY pg_relation_size(indexrelid) DESC;
```

---

## Appendix C: EF Core DbContext Configuration Summary

| Service | DbContext | Database | Connection Pooling | Schema |
|---------|-----------|----------|-------------------|--------|
| Catalog.API | CatalogContext | catalogdb | Yes (default) | public |
| Ordering.API | OrderingContext | orderingdb | No (IMediator dependency) | ordering |
| Identity.API | ApplicationDbContext | identitydb | Yes (default) | public |
| Webhooks.API | WebhooksContext | webhooksdb | Yes (default) | public |
| Basket.API | RedisBasketRepository | redis (N/A) | Yes (StackExchange.Redis) | N/A |

---

**Document Version**: 1.0  
**Last Updated**: December 8, 2025  
**Author**: DBA Mode Analysis  
**Target Audience**: Database Administrators, Site Reliability Engineers, Platform Engineers

---

*This document is a living guide. Update it as the architecture evolves.*
