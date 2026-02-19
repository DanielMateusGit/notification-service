# Notification Service

A centralized, multi-channel notification service built with .NET 8 and Clean Architecture.

## Features

- **Multi-channel delivery**: Email, SMS, Push notifications, Webhooks
- **Template engine**: Dynamic content with Scriban templates
- **Retry with backoff**: Automatic retry (1min → 5min → 15min)
- **Dead Letter Queue**: Failed notifications stored for analysis
- **Delivery tracking**: Real-time status (pending → sent → delivered/failed)
- **Scheduling**: Send notifications at a specific time

## Architecture

This project follows **Clean Architecture** principles with 4 layers:

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← HTTP endpoints
├─────────────────────────────────────────┤
│          Application Layer              │  ← Use cases, CQRS
├─────────────────────────────────────────┤
│          Infrastructure Layer           │  ← DB, external services
├─────────────────────────────────────────┤
│            Domain Layer                 │  ← Entities, business rules
└─────────────────────────────────────────┘
```

**C4 Diagrams:** [docs/architecture/](docs/architecture/)

**Architecture Decision Records:** [docs/adr/](docs/adr/)

## Domain Model

The domain layer contains the core business logic with **Rich Domain Model** approach (entities encapsulate behavior, not just data).

### Entities

```
┌─────────────────────────────────────────────────────────────────┐
│                         Notification                             │
├─────────────────────────────────────────────────────────────────┤
│ - Id: Guid                                                      │
│ - Recipient: Recipient (VO)                                     │
│ - Channel: NotificationChannel                                  │
│ - Content: string                                               │
│ - Status: NotificationStatus                                    │
│ - Priority: Priority                                            │
│ - ScheduledAt: DateTime?                                        │
│ - SentAt: DateTime?                                             │
├─────────────────────────────────────────────────────────────────┤
│ + Schedule()  → raises NotificationScheduledEvent               │
│ + Send()      → raises NotificationSentEvent                    │
│ + Fail()      → raises NotificationFailedEvent                  │
│ + Cancel()                                                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                          Template                                │
├─────────────────────────────────────────────────────────────────┤
│ - Id: Guid                                                      │
│ - Name: string                                                  │
│ - Channel: NotificationChannel                                  │
│ - Subject: string?                                              │
│ - Body: string                                                  │
│ - IsActive: bool                                                │
├─────────────────────────────────────────────────────────────────┤
│ + Render(TemplateData) → string                                 │
│ + Activate() / Deactivate()                                     │
│ + UpdateContent(subject, body)                                  │
│ + GetPlaceholders() → IEnumerable<string>                       │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                       DeliveryAttempt                            │
├─────────────────────────────────────────────────────────────────┤
│ - Id: Guid                                                      │
│ - NotificationId: Guid                                          │
│ - AttemptNumber: int                                            │
│ - Status: DeliveryStatus                                        │
│ - ErrorMessage: string?                                         │
│ - AttemptedAt: DateTime                                         │
│ - CompletedAt: DateTime?                                        │
├─────────────────────────────────────────────────────────────────┤
│ + MarkAsSuccess()                                               │
│ + MarkAsFailed(errorMessage)                                    │
│ + GetDuration() → TimeSpan?                                     │
└─────────────────────────────────────────────────────────────────┘
```

### Value Objects

| Value Object | Purpose | Validation |
|--------------|---------|------------|
| `Recipient` | Contact info for delivery | Channel-specific validation |
| `EmailAddress` | Valid email address | RFC 5322 format |
| `PhoneNumber` | Valid phone number | E.164 format |
| `TemplateData` | Key-value pairs for template rendering | Non-empty keys |

### Domain Events

Events are raised by entities to signal significant state changes:

| Event | Triggered When |
|-------|----------------|
| `NotificationScheduledEvent` | Notification is scheduled for delivery |
| `NotificationSentEvent` | Notification successfully sent |
| `NotificationFailedEvent` | Delivery failed (after all retries) |

Events are dispatched **after** `SaveChanges()` to ensure consistency.

## Application Layer

The Application Layer implements **CQRS pattern** with **MediatR** for request/response handling.

### Commands (Write Operations)

| Command | Purpose | Returns |
|---------|---------|---------|
| `ScheduleNotificationCommand` | Create and schedule a notification | `Guid` (new ID) |
| `CancelNotificationCommand` | Cancel a pending notification | `bool` |
| `RetryNotificationCommand` | Retry a failed notification | `bool` |

### Queries (Read Operations)

| Query | Purpose | Returns |
|-------|---------|---------|
| `GetNotificationByIdQuery` | Get single notification | `NotificationDto?` |
| `GetNotificationsByStatusQuery` | Get by status | `IReadOnlyList<NotificationDto>` |
| `GetPendingNotificationsQuery` | Get ready-to-send | `IReadOnlyList<NotificationDto>` |

### Pipeline Behaviors

MediatR pipeline processes requests in order:

```
Request → ValidationBehavior → LoggingBehavior → Handler → Response
              ↓ (if fails)
         ValidationException
```

| Behavior | Purpose |
|----------|---------|
| `ValidationBehavior` | Runs FluentValidation before handler |
| `LoggingBehavior` | Logs request/response for debugging |

### Validation (FluentValidation)

Input validation happens in the Application Layer, separate from domain rules:

```csharp
// Application Layer - Input validation (400 Bad Request)
RuleFor(x => x.Recipient).NotEmpty();
RuleFor(x => x.Recipient).EmailAddress().When(x => x.Channel == Email);

// Domain Layer - Business rules (handled in Entity)
if (Status != NotificationStatus.Pending)
    throw new InvalidOperationException("Cannot cancel...");
```

### Ports (Interfaces)

| Interface | Purpose |
|-----------|---------|
| `INotificationRepository` | Notification persistence |
| `ITemplateRepository` | Template persistence |
| `IUnitOfWork` | Transaction management |
| `IDateTimeProvider` | Testable time abstraction |

### Enums

| Enum | Values |
|------|--------|
| `NotificationStatus` | Pending, Scheduled, Sent, Failed, Cancelled |
| `NotificationChannel` | Email, SMS, Push, Webhook |
| `Priority` | Low, Normal, High, Critical |
| `DeliveryStatus` | InProgress, Success, Failed |

## Tech Stack

| Component | Technology |
|-----------|------------|
| Backend | .NET 8 |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Message Queue | Azure Service Bus |
| Containers | Docker |
| IaC | Terraform |
| CI/CD | GitHub Actions |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop/)
- [Docker Compose](https://docs.docker.com/compose/)

### Run locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/DanielMateusGit/notification-service.git
   cd notification-service
   ```

2. **Start PostgreSQL** (Docker)
   ```bash
   docker run -d \
     --name notification-postgres \
     -e POSTGRES_USER=notification \
     -e POSTGRES_PASSWORD=notification_dev \
     -e POSTGRES_DB=notification_service \
     -p 5432:5432 \
     postgres:16-alpine
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update \
     --project src/NotificationService.Infrastructure \
     --startup-project src/NotificationService.Api
   ```

4. **Run the API**
   ```bash
   cd src/NotificationService.Api
   dotnet run
   ```

### Database Setup (Manual)

If you prefer to manage PostgreSQL manually:

1. **Install PostgreSQL 16** (or use Docker as above)

2. **Create database and user**
   ```sql
   CREATE USER notification WITH PASSWORD 'notification_dev';
   CREATE DATABASE notification_service OWNER notification;
   GRANT ALL PRIVILEGES ON DATABASE notification_service TO notification;
   ```

3. **Connection string** (already configured in `appsettings.Development.json`)
   ```
   Host=localhost;Port=5432;Database=notification_service;Username=notification;Password=notification_dev
   ```

4. **Apply migrations**
   ```bash
   dotnet ef database update \
     --project src/NotificationService.Infrastructure \
     --startup-project src/NotificationService.Api
   ```

### Database Schema

After migrations, you'll have these tables:

| Table | Description |
|-------|-------------|
| `notifications` | Notification records with Recipient (Owned Type) |
| `templates` | Message templates with Scriban syntax |
| `delivery_attempts` | Delivery history for each notification |
| `__EFMigrationsHistory` | EF Core migration tracking |

### Configuration

Connection string is in `src/NotificationService.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=notification_service;Username=notification;Password=notification_dev"
  }
}
```

For production, use environment variables or a secrets manager.

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## API Reference

*Coming in Week 4 (Infrastructure Layer)*

```http
POST /api/notifications
Content-Type: application/json

{
  "channel": "email",
  "recipient": "user@example.com",
  "template": "order_confirmation",
  "data": {
    "name": "John",
    "order_id": "ORD-12345"
  }
}
```

## Project Status

| Phase | Status |
|-------|--------|
| Setup + Clean Architecture (W1-W2) | ✅ Complete |
| Application Layer (W3) | ✅ Complete |
| Infrastructure Layer (W4) | ✅ Complete |
| Event-Driven + Channels (W5-W8) | 🟢 Up Next |
| API + DevOps (W9-W12) | Planned |
| Cloud + Production (W13-W16) | Planned |

### Week 4 Deliverables
- ✅ EF Core with PostgreSQL (Npgsql)
- ✅ Entity Configurations (Fluent API)
- ✅ Recipient Value Object with Owned Types
- ✅ Repository implementations (PostgresNotificationRepository, PostgresTemplateRepository)
- ✅ UnitOfWork pattern
- ✅ Database migrations (InitialCreate, UseRecipientValueObject)
- ✅ Integration tests with Testcontainers (11 tests)
- ✅ ADR-003: Database Strategy
- ✅ 268 tests total (185 Domain + 72 Application + 11 Infrastructure)

### Week 3 Deliverables
- ✅ CQRS with MediatR (Commands + Queries)
- ✅ FluentValidation with ValidationBehavior
- ✅ Pipeline Behaviors (Validation + Logging)
- ✅ Repository interfaces (INotificationRepository, ITemplateRepository)

### Week 2 Deliverables
- ✅ Domain Model (Notification, Template, DeliveryAttempt)
- ✅ Value Objects (Recipient, EmailAddress, PhoneNumber, TemplateData)
- ✅ Domain Events (Scheduled, Sent, Failed)
- ✅ ADR-002: Rich Domain Model decision
- ✅ C4 Container Diagram

## License

MIT

---

Built with Clean Architecture principles as part of the Architect Quest learning path.
