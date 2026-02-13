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

2. **Start infrastructure** (PostgreSQL + Redis)
   ```bash
   docker-compose up -d
   ```

3. **Verify containers are running**
   ```bash
   docker ps
   ```
   You should see `notification-postgres` and `notification-redis` running.

4. **Run the API** (coming soon)
   ```bash
   cd src/NotificationService.Api
   dotnet run
   ```

### Configuration

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

See [.env.example](.env.example) for all available options.

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## API Reference

*Coming in Week 3*

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
| Event-Driven + Channels (W5-W8) | Planned |
| API + DevOps (W9-W12) | Planned |
| Cloud + Production (W13-W16) | Planned |

### Week 2 Deliverables
- ✅ Domain Model (Notification, Template, DeliveryAttempt)
- ✅ Value Objects (Recipient, EmailAddress, PhoneNumber, TemplateData)
- ✅ Domain Events (Scheduled, Sent, Failed)
- ✅ 178 unit tests (all passing)
- ✅ ADR-002: Rich Domain Model decision
- ✅ C4 Container Diagram

## License

MIT

---

Built with Clean Architecture principles as part of the Architect Quest learning path.
