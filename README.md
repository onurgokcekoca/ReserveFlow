# ReserveFlow

A hotel reservation backend built as a **modular monolith** on .NET 10. It exists to work through real backend problems end to end: module boundaries, data consistency, concurrency, reliable messaging and integration testing.

> Status: in active development. Catalog, Inventory, Booking, Search and Notifications modules are wired up and running. Authentication is not implemented yet.

## Architecture

```text
ReserveFlow/
│
│  ASP.NET Core 10 · Modular Monolith · Clean Architecture · DDD · CQRS
│
├── 🌐 Api                       → Composition root, Minimal API endpoints, ProblemDetails, Swagger
│
├── 📦 Modules
│   ├── 🏨 Catalog               → Properties and room types; Redis cache-aside on reads; outbox
│   ├── 📊 Inventory             → Room availability per date range; optimistic concurrency
│   ├── 📅 Booking               → Holds → confirm / expire as domain rules; outbox
│   ├── 🔍 Search                → RabbitMQ consumer → Elasticsearch index; search queries
│   └── 🔔 Notifications         → RabbitMQ consumer with retry and dead-letter queues
│
│   Each module: Domain → Application → Infrastructure → Presentation
│   Own PostgreSQL schema and DbContext. Modules talk only through ports and integration events.
│
├── 🧩 Shared
│   ├── Kernel                   → Base Entity, Result<T>, Error, domain events
│   ├── Application              → ICurrentUser, messaging and pagination abstractions
│   ├── Infrastructure           → RabbitMQ connection provider and publisher
│   └── Presentation             → Result → HTTP response mapping
│
├── ⚙️ Infrastructure
│   ├── PostgreSQL 17            → One schema per module, EF Core migrations
│   ├── RabbitMQ                 → Integration events between modules
│   ├── Outbox / Inbox           → Events saved with the data; idempotent consumers
│   ├── Redis                    → Catalog read cache
│   ├── Elasticsearch            → Property search index
│   └── BackgroundService        → Outbox publishers, consumers, hold expiration
│
└── 🧪 Tests
    ├── Catalog.UnitTests        → Domain rules
    ├── Inventory.UnitTests      → Domain rules
    └── IntegrationTests         → WebApplicationFactory + Testcontainers (PostgreSQL, RabbitMQ, Elasticsearch)
```

### Event flow

```text
  API request ──► Catalog / Booking module
                        │  same transaction
                        ▼
                PostgreSQL + Outbox table
                        │
                        ▼
                Outbox Publisher (BackgroundService)
                        │
                        ▼
                     RabbitMQ
              ┌─────────┴─────────┐
              ▼                   ▼
      Search Consumer     Notifications Consumer
              │                   │
              ▼                   ▼
        Elasticsearch        Notifications DB
```

Catalog events keep the search index in sync. Booking events drive notifications. The notifications consumer is idempotent through an inbox table and retries through delay and dead-letter queues.

## Getting started

Requirements: .NET 10 SDK, Docker Desktop.

```bash
git clone https://github.com/onurgokcekoca/ReserveFlow.git
cd ReserveFlow

# 1. Local infrastructure credentials (git-ignored)
cp .env.example .env

# 2. Start everything, including the API
docker compose up -d --build
```

The API is then available at `http://localhost:8085/swagger`. RabbitMQ management UI is at `http://localhost:15672`, Kibana at `http://localhost:5601`. All ports are bound to localhost only.

### Running the API from Visual Studio / `dotnet run`

Start only the infrastructure, then store the connection secrets with user-secrets (they are intentionally not in `appsettings.Development.json`):

```bash
docker compose up -d postgres rabbitmq redis elasticsearch

cd src/Api/ReserveFlow.Api
dotnet user-secrets set "ConnectionStrings:CatalogConnection"      "Host=localhost;Port=5432;Database=reserveflow;Username=reserveflow;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "ConnectionStrings:InventoryConnection"    "Host=localhost;Port=5432;Database=reserveflow;Username=reserveflow;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "ConnectionStrings:BookingConnection"      "Host=localhost;Port=5432;Database=reserveflow;Username=reserveflow;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "ConnectionStrings:NotificationConnection" "Host=localhost;Port=5432;Database=reserveflow;Username=reserveflow;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "RabbitMq:Password" "<RABBITMQ_PASSWORD>"

dotnet run
```

### Tests

```bash
dotnet test
```

Integration tests spin up PostgreSQL, RabbitMQ and Elasticsearch with Testcontainers, so Docker must be running.

## Design decisions

- **Modular monolith, not microservices.** One deployable, one database server, but each module owns its schema and DbContext and never references another module's project. The seams are there if a module ever needs to move out.
- **Outbox instead of publishing inside the request.** Integration events are written in the same transaction as the business data and published by a background worker, so a broker outage cannot lose an event.
- **Optimistic concurrency on inventory.** Conflicting holds surface as HTTP 409 rather than silently overbooking.
- **Compensation over distributed transactions.** When a cross-module step fails, the originating module undoes its own change.
- **Real databases in tests.** Integration tests run against containers, not in-memory providers.

## Known limitations

- No authentication yet. A fixed development user is registered in the Development environment only; the API refuses to start in any other environment until authentication exists.
- Elasticsearch runs with security disabled for local development.
- Payment and identity modules are not started.
