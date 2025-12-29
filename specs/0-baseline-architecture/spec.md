# Baseline Specification: eShop Reference Application

**Feature Branch**: `0-baseline-architecture`  
**Created**: 2025-12-08  
**Status**: Baseline Documentation  
**Type**: Brownfield Analysis

## Overview

eShop is a reference .NET e-commerce application demonstrating modern microservices architecture patterns using .NET Aspire. This specification documents the existing system's current state, functionality, and architecture.

## System Architecture

### High-Level Architecture

The application follows a microservices architecture with:
- **7 Backend Services**: Each with single responsibility
- **Event-Driven Communication**: RabbitMQ for async messaging
- **Database-per-Service**: PostgreSQL instances for data isolation
- **Distributed Caching**: Redis for session and basket state
- **Centralized Identity**: OpenID Connect authentication

### Service Inventory

| Service | Type | Purpose | Port |
|---------|------|---------|------|
| WebApp | Blazor Server | Customer-facing storefront | HTTPS |
| Catalog.API | REST API | Product catalog management | HTTP/HTTPS |
| Basket.API | gRPC + REST | Shopping cart operations | HTTP |
| Ordering.API | REST API | Order lifecycle management | HTTP |
| Identity.API | OpenID Connect | Authentication & authorization | HTTPS |
| Webhooks.API | REST API | External webhook subscriptions | HTTP |
| OrderProcessor | Background Worker | Order state machine processing | N/A |
| PaymentProcessor | Background Worker | Payment simulation | N/A |

## User Scenarios & Testing *(existing functionality)*

### User Story 1 - Browse Product Catalog (Priority: P1)

Customers can browse the product catalog, filter by brand/type, and view product details.

**Why this priority**: Core e-commerce functionality - customers must be able to discover products.

**Independent Test**: Navigate to webapp, view catalog grid, filter products, view detail page.

**Acceptance Scenarios**:

1. **Given** a customer on the homepage, **When** they view the catalog, **Then** they see product cards with images, names, and prices
2. **Given** a customer viewing catalog, **When** they select a brand filter, **Then** only products from that brand are displayed
3. **Given** a customer viewing a product, **When** they click on it, **Then** they see full product details

---

### User Story 2 - Shopping Cart Management (Priority: P1)

Authenticated users can add products to their cart, modify quantities, and remove items.

**Why this priority**: Essential for purchase flow - users must manage their selections.

**Independent Test**: Login, add item to cart, view cart, modify quantity, remove item.

**Acceptance Scenarios**:

1. **Given** an authenticated user viewing a product, **When** they click "Add to Cart", **Then** the item is added and cart count updates
2. **Given** a user with items in cart, **When** they increase quantity, **Then** the cart total updates correctly
3. **Given** a user with items in cart, **When** they remove an item, **Then** it's removed and totals recalculate

---

### User Story 3 - Order Placement (Priority: P1)

Authenticated users can checkout their cart, provide shipping details, and complete an order.

**Why this priority**: Revenue-generating action - the core business transaction.

**Independent Test**: With items in cart, proceed to checkout, enter details, place order.

**Acceptance Scenarios**:

1. **Given** a user with cart items at checkout, **When** they enter shipping and payment details, **Then** they can submit the order
2. **Given** an order is placed, **When** stock is validated, **Then** the order moves to "Awaiting Validation" status
3. **Given** stock is confirmed, **When** payment processes, **Then** order status becomes "Paid"

---

### User Story 4 - Order History (Priority: P2)

Authenticated users can view their past orders and order details.

**Why this priority**: Customer service need - users must track their purchases.

**Independent Test**: Login, navigate to orders, view order list, click order for details.

**Acceptance Scenarios**:

1. **Given** an authenticated user, **When** they navigate to "My Orders", **Then** they see a list of their past orders
2. **Given** a user viewing orders, **When** they click an order, **Then** they see order items, status, and shipping details

---

### User Story 5 - User Authentication (Priority: P1)

Users can register, login, and manage their sessions securely.

**Why this priority**: Security foundation - required for personalized features.

**Independent Test**: Register new account, logout, login with credentials.

**Acceptance Scenarios**:

1. **Given** a visitor, **When** they register with valid details, **Then** an account is created and they're logged in
2. **Given** a registered user, **When** they login with correct credentials, **Then** they access authenticated features
3. **Given** a logged-in user, **When** they logout, **Then** their session ends and cart persists server-side

### Edge Cases

- What happens when a product is out of stock during checkout? → Order is cancelled with stock rejection message
- How does system handle payment failures? → Order remains in stock-confirmed state; retry possible
- What if user's session expires mid-checkout? → Cart is preserved in Redis; user re-authenticates to continue

## Requirements *(documented from existing implementation)*

### Functional Requirements

- **FR-001**: System MUST display product catalog with filtering by brand and type
- **FR-002**: System MUST support user registration and authentication via OpenID Connect
- **FR-003**: System MUST maintain shopping cart state per authenticated user in Redis
- **FR-004**: System MUST process orders through state machine (Submitted → AwaitingValidation → StockConfirmed → Paid → Shipped)
- **FR-005**: System MUST validate stock availability before confirming orders
- **FR-006**: System MUST publish integration events for cross-service communication
- **FR-007**: System MUST support webhook subscriptions for external integrations
- **FR-008**: System MUST provide order history for authenticated users
- **FR-009**: System MUST support AI-powered product search (optional OpenAI/Ollama integration)
- **FR-010**: System MUST expose mobile-friendly API via BFF (Backend for Frontend) pattern

### Non-Functional Requirements

- **NFR-001**: Services MUST be independently deployable
- **NFR-002**: System MUST handle eventual consistency between services
- **NFR-003**: All API endpoints MUST be documented via OpenAPI
- **NFR-004**: System MUST support distributed tracing via OpenTelemetry
- **NFR-005**: Database migrations MUST be handled by Ordering.API at startup

### Key Entities

- **Product**: Catalog item with name, description, price, brand, type, and image
- **CatalogBrand**: Product brand classification
- **CatalogType**: Product type/category classification
- **BasketItem**: Shopping cart line item with product reference and quantity
- **Order**: Aggregate root containing shipping address, order items, and status
- **OrderItem**: Line item within an order with product snapshot and pricing
- **Buyer**: Customer entity with payment methods
- **Address**: Value object for shipping information

## Success Criteria *(current system capabilities)*

### Measurable Outcomes

- **SC-001**: Users can browse catalog and complete checkout flow end-to-end
- **SC-002**: Order state transitions are reliable and auditable via domain events
- **SC-003**: Services start and communicate successfully via .NET Aspire orchestration
- **SC-004**: System supports local development with single command (`dotnet run --project src/eShop.AppHost`)
- **SC-005**: E2E tests (Playwright) pass for critical user journeys

## Technical Implementation Notes

### Integration Events (Cross-Service Communication)

| Event | Publisher | Subscribers |
|-------|-----------|-------------|
| OrderStatusChangedToAwaitingValidation | Ordering.API | Catalog.API |
| OrderStatusChangedToPaid | Ordering.API | Catalog.API |
| OrderStockConfirmed | Catalog.API | Ordering.API |
| OrderStockRejected | Catalog.API | Ordering.API |
| ProductPriceChanged | Catalog.API | Basket.API |

### Data Flow

1. **Catalog Browse**: WebApp → Catalog.API → PostgreSQL (catalogdb)
2. **Add to Cart**: WebApp → Basket.API → Redis
3. **Checkout**: WebApp → Ordering.API → PostgreSQL (orderingdb) → RabbitMQ → Catalog.API (stock check)
4. **Payment**: OrderProcessor ← RabbitMQ ← PaymentProcessor

### Infrastructure Dependencies

- PostgreSQL 15+ with pgvector extension
- Redis 7+
- RabbitMQ 3.12+
- .NET 9/10 SDK
- Docker/Colima for container orchestration

