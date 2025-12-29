# eShop Constitution

<!--
SYNC IMPACT REPORT
Version Change: 0.0.0 → 1.0.0 (Initial brownfield documentation)
Added Sections: All - initial constitution based on existing codebase analysis
Modified Principles: N/A (initial creation)
Templates Requiring Updates: ✅ None - templates are generic and compatible
-->

## Core Principles

### I. Microservices Architecture

The eShop application follows a microservices architecture pattern orchestrated by .NET Aspire. Each service MUST:
- Be independently deployable and scalable
- Own its data (database-per-service pattern)
- Communicate asynchronously via message broker (RabbitMQ) for cross-service operations
- Expose HTTP APIs for synchronous client interactions
- Define clear bounded contexts aligned with business domains (Catalog, Ordering, Basket, Identity, Webhooks)

### II. Domain-Driven Design (DDD)

The Ordering domain exemplifies DDD patterns that SHOULD be followed for complex business logic:
- **Aggregates**: Business entities with clear boundaries (e.g., Order as aggregate root)
- **Value Objects**: Immutable objects representing concepts without identity (e.g., Address)
- **Domain Events**: Events raised within aggregates to trigger side effects (e.g., OrderStartedDomainEvent)
- **Encapsulation**: Private collections exposed as read-only; mutations only through aggregate methods
- **Rich Domain Model**: Business logic lives in domain entities, not services

### III. Event-Driven Communication

Services MUST communicate asynchronously for operations that:
- Span multiple bounded contexts
- Require eventual consistency
- Need to trigger workflows in other services

Integration Events are the contract between services. Event handlers MUST:
- Be idempotent
- Handle failures gracefully
- Not block on synchronous calls to other services

### IV. Infrastructure Standards

The following infrastructure components are standardized:
- **Database**: PostgreSQL with Entity Framework Core (pgvector for AI/vector search capabilities)
- **Cache**: Redis for session state and caching (Basket service)
- **Message Broker**: RabbitMQ for integration events
- **Orchestration**: .NET Aspire for local development and service discovery
- **Identity**: OpenID Connect/OAuth2 via Identity.API

### V. Code Quality

All projects MUST:
- Treat warnings as errors (`TreatWarningsAsErrors=true`)
- Use implicit usings
- Include embedded debug symbols
- Follow consistent namespace conventions matching folder structure

### VI. Testing Strategy

Tests MUST follow the Arrange-Act-Assert pattern:
- **Unit Tests**: Test domain logic in isolation (MSTest framework)
- **Functional Tests**: Test API endpoints with test containers
- **E2E Tests**: Playwright tests for critical user journeys

## Architectural Constraints

### Service Boundaries

| Service | Responsibility | Data Store | Dependencies |
|---------|---------------|------------|--------------|
| Catalog.API | Product catalog management | PostgreSQL (catalogdb) | RabbitMQ |
| Basket.API | Shopping cart management | Redis | RabbitMQ, Identity |
| Ordering.API | Order processing & domain logic | PostgreSQL (orderingdb) | RabbitMQ, Identity |
| Identity.API | Authentication & authorization | PostgreSQL (identitydb) | - |
| Webhooks.API | External webhook subscriptions | PostgreSQL (webhooksdb) | RabbitMQ, Identity |
| OrderProcessor | Background order state machine | - | RabbitMQ, Ordering DB |
| PaymentProcessor | Payment simulation | - | RabbitMQ |

### API Design

- RESTful endpoints for CRUD operations
- gRPC for internal service-to-service calls where performance is critical (Basket)
- OpenAPI/Swagger documentation for all public APIs

## Development Workflow

### Local Development

Developers MUST use .NET Aspire AppHost for local development:
```bash
dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj
```

This ensures:
- Consistent infrastructure setup (databases, message broker, cache)
- Service discovery and dependency management
- Observable distributed tracing via Aspire dashboard

### Feature Development

New features SHOULD:
1. Define specifications before implementation
2. Consider impact on existing bounded contexts
3. Use integration events for cross-service communication
4. Include appropriate test coverage

## Governance

This constitution documents the existing architectural patterns of the eShop reference application. Amendments to this constitution require:
1. Clear justification for the change
2. Impact analysis on existing services
3. Migration plan if breaking changes are introduced
4. Documentation updates

Constitution violations SHOULD be flagged during code review and addressed before merging.

**Version**: 1.0.0 | **Ratified**: 2025-12-08 | **Last Amended**: 2025-12-08
