# Project Context

## Purpose
eShop (AdventureWorks) is a reference .NET application implementing an e-commerce website using a microservices-based architecture built with .NET Aspire. This is a canonical example showcasing best practices for building cloud-native, distributed applications with .NET.

The project demonstrates:
- Microservices architecture with service-to-service communication
- Modern e-commerce workflows (catalog browsing, shopping cart, ordering, payments)
- Identity management and authentication
- Event-driven architecture using message queues
- AI integration capabilities (Azure OpenAI, Ollama)
- Container orchestration and cloud-native patterns

## Tech Stack

### Core Framework
- **.NET 10** (latest preview with rollForward to latestFeature)
- **C# 13** with modern language features
- **.NET Aspire 13.0.1** for cloud-native app orchestration

### Backend Technologies
- **ASP.NET Core 10.0** for web APIs and web applications
- **Blazor** for interactive web UI components
- **gRPC 2.71.0** for inter-service communication
- **Entity Framework Core 10.0** with PostgreSQL
- **Dapper 2.1.35** for performance-critical data access
- **MediatR 13.0.0** for CQRS pattern implementation
- **FluentValidation 12.0.0** for validation logic

### Data & Infrastructure
- **PostgreSQL** with **pgvector** extension for vector search (AI embeddings)
- **Redis** for distributed caching and basket storage
- **RabbitMQ** for asynchronous event bus messaging
- **Duende IdentityServer 7.3.2** for authentication/authorization

### AI & ML
- **Azure OpenAI** (via Aspire.Azure.AI.OpenAI)
- **Ollama** (via CommunityToolkit.Aspire.OllamaSharp) for local LLM support
- AI-powered chatbot for customer assistance

### Frontend
- **Blazor Server** and **Blazor WebAssembly** components
- **.NET MAUI** for cross-platform mobile apps (iOS, Android, Windows, macOS)
- **TypeScript** with **Playwright** for end-to-end testing

### DevOps & Observability
- **OpenTelemetry 1.12.0** for distributed tracing, metrics, and logging
- **Docker** for containerization
- **Scalar.AspNetCore 2.8.6** for API documentation
- **ASP.NET Core Health Checks** for monitoring

### API Standards
- **API Versioning 8.1.0** for versioned REST APIs
- **OpenAPI/Swagger** for API documentation
- **gRPC** for high-performance service-to-service calls

## Project Conventions

### Code Style
- **Namespace strategy**: Use file-scoped namespaces (e.g., `namespace eShop.Catalog.API;`)
- **Naming conventions**: 
  - PascalCase for classes, methods, properties
  - camelCase for private fields with underscore prefix (e.g., `_orderItems`)
  - Interfaces prefixed with `I` (e.g., `IRepository`)
- **Global usings**: Each project has a `GlobalUsings.cs` file for common namespaces
- **Minimal APIs**: Use endpoint mapping with extension methods (e.g., `MapOrdersApiV1()`)
- **Modern C# patterns**: Use records, pattern matching, nullable reference types

### Architecture Patterns

#### Microservices Architecture
- **Service boundaries**: Each service is independently deployable with its own database
- **Services**:
  - `Basket.API` - Shopping basket management
  - `Catalog.API` - Product catalog with AI-powered search
  - `Ordering.API` - Order processing and management
  - `Identity.API` - Authentication and user management
  - `Webhooks.API` - Webhook subscriptions and notifications
  - `WebApp` - Main Blazor web application
  - `OrderProcessor` & `PaymentProcessor` - Background workers

#### Domain-Driven Design (DDD)
- **Ordering domain** implements DDD patterns:
  - Aggregates (Order, Buyer)
  - Entities and Value Objects
  - Domain Events (e.g., `OrderStartedDomainEvent`)
  - Repository pattern (`IOrderRepository`, `IBuyerRepository`)
  - Unit of Work pattern
- **Private collections** for aggregate encapsulation (e.g., `_orderItems`)
- **Domain events** dispatched through MediatR

#### Event-Driven Architecture
- **Integration events** for cross-service communication via RabbitMQ
- **Domain events** for within-service consistency
- Event handlers in `IntegrationEvents` folders

#### CQRS Pattern
- Separation of commands and queries using MediatR
- Read models optimized separately from write models

#### Repository Pattern
- Database access abstracted through repositories
- Unit of Work for transaction management

### Testing Strategy

#### Unit Tests
- **MSTest 4.0.2** as the test framework (with Microsoft.Testing.Platform)
- Test projects: `Basket.UnitTests`, `Ordering.UnitTests`, `ClientApp.UnitTests`
- **NSubstitute 5.3.0** for mocking dependencies
- Tests validate business logic, domain models, and service behavior

#### Functional Tests
- **Aspire-based functional tests** that spin up test containers
- Test projects: `Catalog.FunctionalTests`, `Ordering.FunctionalTests`
- **Requires Docker** to run integration tests with real dependencies
- Uses `Microsoft.AspNetCore.Mvc.Testing` and `Microsoft.AspNetCore.TestHost`

#### End-to-End Tests
- **Playwright 1.42.1** with TypeScript for browser automation
- Tests located in `/e2e` directory
- Tests cover: browsing items, adding to cart, removing items, authentication flows
- Runs against `http://localhost:5045` (WebApp)
- Separate test configurations for authenticated vs. unauthenticated scenarios

### Git Workflow
- **Contribution principles**:
  - Focus on best practices and canonical .NET patterns
  - Selective about tools/libraries to maintain realism
  - Architectural changes require clear rationale
  - Performance improvements should include benchmarks
- **Issue labels**: Use "help wanted" and "good first issue" for new contributors
- **Pull requests**: Check for PR templates in `.github/PULL_REQUEST_TEMPLATE`
- **No separate issues for typos**: Small fixes can go directly to PRs

## Domain Context

### E-Commerce Domain
- **Catalog**: Products organized by type and brand (filters: All, AirStrider, B&R, Daybird, Gravitator)
- **Shopping experience**: Browse → Add to basket → Checkout → Order confirmation
- **User roles**: Anonymous browsing, authenticated purchasing
- **Order lifecycle**: Submitted → AwaitingValidation → StockConfirmed → Paid → Shipped → Cancelled
- **Payment methods**: Credit card processing with buyer association

### Business Entities
- **CatalogItem**: Products with images, descriptions, prices, stock
- **Order**: Contains order lines, status, buyer info, shipping address
- **Buyer**: Customer with payment methods
- **Basket**: Temporary shopping cart (Redis-backed)
- **Webhook**: Subscriptions for event notifications

### Adventure/Outdoor Sports Theme
- Product categories: Snowboards, ski goggles, winter sports equipment
- Brand names: AirStrider, B&R, Daybird, Gravitator
- Marketing focus: "Ready for a new adventure? Start the season with the latest in clothing and equipment."

## Important Constraints

### Technical Constraints
- **Docker required**: Application uses containers for Redis, RabbitMQ, PostgreSQL
- **HTTPS by default**: Production uses HTTPS; HTTP available via launch profile
- **Service discovery**: Services communicate via Aspire service discovery
- **Authentication**: JWT Bearer tokens from Identity.API required for protected endpoints
- **Database per service**: Each microservice owns its database (catalogdb, orderingdb, identitydb, webhooksdb, basket in Redis)

### Development Constraints
- **Visual Studio 2022 v17.10+** on Windows with ASP.NET workload
- **macOS with Rosetta 2** required for Apple Silicon (for grpc-tools)
- **.NET Aspire SDK** must be installed
- **Central package management**: Versions managed in `Directory.Packages.props`

### Architectural Constraints
- **No over-engineering**: Use realistic, canonical patterns only
- **Best practices first**: Code should represent industry standards
- **Selectivity**: Don't showcase every possible library/tool
- **Justification required**: Large refactoring needs clear rationale

## External Dependencies

### Required Services
- **Docker Desktop**: Must be running for local development
- **PostgreSQL with pgvector**: For catalog, ordering, identity, webhooks databases
- **Redis**: For basket storage and distributed caching
- **RabbitMQ**: For event bus messaging between services

### Optional AI Services
- **Azure OpenAI**: Configure in `eShop.AppHost/appsettings.json` with connection string
  - Set `useOpenAI = true` in `Program.cs` to enable
  - Requires API key or Azure subscription
- **Ollama**: Alternative local LLM option via CommunityToolkit

### Identity Provider
- **Duende IdentityServer**: Self-hosted identity provider
- Issues JWT tokens for service-to-service authentication
- Manages user registration, login, and OAuth2/OIDC flows

### Monitoring & Telemetry
- **OpenTelemetry exporters**: Send traces/metrics to observability platforms
- **Aspire Dashboard**: Built-in dashboard for monitoring services (login at `http://localhost:19888`)

### Build & Package Sources
- **NuGet.org**: Public package source
- **Unstable Aspire packages**: Version 13.0.0-preview.1.25560.3 for cutting-edge features
