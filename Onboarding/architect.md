# eShop Architecture Overview

## What It Is

**eShop** (formerly "AdventureWorks") is a **reference e-commerce application** built by Microsoft to demonstrate modern .NET architecture patterns and best practices. It's a learning resource showing how to build production-grade microservices applications.

## Core Architecture

**Technology Stack:**
- **.NET 10** (latest version)
- **.NET Aspire** - Cloud-native orchestration framework
- **Blazor Server** - For the web UI
- **PostgreSQL with pgvector** - Database with vector support for AI features
- **RabbitMQ** - Message bus for event-driven communication
- **Redis** - Caching
- **Docker** - Containerization

**Architecture Pattern:**
- **Microservices** - Independently deployable services
- **Event-driven** - Services communicate via RabbitMQ message bus
- **API Gateway** - YARP reverse proxy for mobile BFF (Backend for Frontend)
- **Service discovery** - Built into .NET Aspire

## Key Services

1. **Catalog.API** - Product catalog management
2. **Basket.API** - Shopping cart functionality
3. **Ordering.API** - Order processing
4. **Identity.API** - Authentication/authorization (Duende IdentityServer)
5. **Webhooks.API** - Webhook subscriptions and notifications
6. **WebApp** - Customer-facing Blazor web application
7. **OrderProcessor** - Background worker processing order events
8. **PaymentProcessor** - Background worker processing payments
9. **WebhookClient** - Demo client for webhook testing

## How It Works

**Service Orchestration:**
```
eShop.AppHost (Aspire) orchestrates all services
    ↓
Configures infrastructure (databases, messaging, caching)
    ↓
Wires up service dependencies and health checks
    ↓
Provides unified dashboard for monitoring
```

**Event Flow Example:**
```
User places order → Ordering.API → RabbitMQ event
    ↓                                    ↓
Identity validation            OrderProcessor consumes event
    ↓                                    ↓
Basket cleared                  PaymentProcessor triggered
    ↓                                    ↓
Webhooks notified              Order status updated
```

**Key Features:**
- Domain-driven design in Ordering service
- Integration event log pattern for reliable messaging
- gRPC for service-to-service communication
- OpenAPI/Swagger documentation
- Health checks and observability
- Optional AI integration (OpenAI/Azure OpenAI/Ollama) for product recommendations

## AI Agent Enhancements

This repository includes **custom AI agent personas** for GitHub Copilot:
- **Architect** - System design and planning
- **Engineer** - Implementation guidance
- **QE** - Testing strategies  
- **DBA** - Database optimization
- **DevOps** - CI/CD and deployment
- **Product Owner** - Feature prioritization

These are defined in `.github/chatmodes/` and now configured in your VS Code settings.

## Best Way to Learn

1. **Start here:** Run `dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj`
2. **Explore the Aspire Dashboard** - See service health, logs, traces
3. **Follow data flow:** Pick a feature (e.g., placing an order) and trace it through:
   - WebApp UI component
   - API calls
   - Event publishing
   - Background processing
4. **Read domain models:** `src/Ordering.Domain/` shows DDD patterns
5. **Check integration events:** `*/IntegrationEvents/` folders show service communication
6. **Review tests:** `tests/` folder has functional and unit tests

## Compliance Notes

The solution follows **FedRAMP considerations** per the Copilot rules - meaning care is taken around data handling, secrets management, and cloud deployment patterns suitable for government compliance.
