# ADR-003: Database Strategy - PostgreSQL con Entity Framework Core

## Status

**Accepted** - 2026-02-19

## Context

Il Notification Service necessita di un database per persistere:

- **Notifications**: con stato, scheduling, retry tracking
- **Templates**: con versioning e placeholder management
- **DeliveryAttempts**: storico di ogni tentativo di invio

Dobbiamo decidere:
1. **Quale database** usare (SQL vs NoSQL, quale vendor)
2. **Quale ORM/data access** usare (.NET)
3. **Come gestire le migrations** (schema evolution)

### Requisiti

- **ACID transactions**: le operazioni su Notification devono essere atomiche
- **Query complesse**: ricerca per status, scheduled_at, channel
- **Relazioni**: Notification → DeliveryAttempts (1:N)
- **Sviluppo locale facile**: deve funzionare su Mac/Windows/Linux
- **Scalabilità futura**: possibilità di replica read/write
- **Owned Types**: supporto per Value Objects (Recipient)

## Decision

Adottiamo **PostgreSQL** come database con **Entity Framework Core** come ORM.

### Stack Scelto

```
┌─────────────────────────────────────────────────────────┐
│  Application Layer                                      │
│  └── INotificationRepository (interfaccia)             │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                   │
│  ├── PostgresNotificationRepository                    │
│  ├── AppDbContext (EF Core)                            │
│  └── Entity Configurations (Fluent API)                │
├─────────────────────────────────────────────────────────┤
│  Database                                               │
│  └── PostgreSQL 16                                      │
└─────────────────────────────────────────────────────────┘
```

### Configurazione Chiave

```csharp
// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
}

// Owned Type per Value Object
builder.OwnsOne(n => n.Recipient, recipient =>
{
    recipient.Property(r => r.Value).HasColumnName("recipient_value");
    recipient.Property(r => r.Channel).HasColumnName("recipient_channel");
});
```

## Alternatives Considered

### 1. SQL Server

**Pro:**
- Integrazione nativa con .NET e Azure
- Tooling maturo (SSMS)
- Familiar per team Microsoft-stack

**Contro:**
- **Licenze costose** per produzione (o Azure SQL)
- Più pesante per sviluppo locale
- Lock-in Microsoft

**Motivo del rifiuto:** PostgreSQL è gratuito, performante, e altrettanto supportato da EF Core.

### 2. MongoDB (NoSQL)

**Pro:**
- Schema flessibile
- Scalabilità orizzontale nativa
- Buono per documenti nested

**Contro:**
- **No ACID transactions** (o limitato)
- **Query relazionali complesse** più difficili
- Diverso paradigma per team SQL-oriented
- EF Core support limitato

**Motivo del rifiuto:** Il nostro modello è relazionale (Notification → DeliveryAttempts) e richiede transazioni ACID.

### 3. SQLite

**Pro:**
- Zero setup
- File-based, perfetto per test
- Leggero

**Contro:**
- **Non adatto a produzione** multi-utente
- Funzionalità limitate (no concurrent writes)
- Differenze sottili con PostgreSQL

**Motivo del rifiuto:** Ottimo per testing locale, ma vogliamo lo stesso database in sviluppo e produzione.

### 4. Dapper (Micro-ORM)

**Pro:**
- Performance superiore (raw SQL)
- Controllo totale sulle query
- Leggero

**Contro:**
- **Nessun supporto Owned Types** nativamente
- Mapping manuale per Value Objects
- Migrations manuali
- Più codice boilerplate

**Motivo del rifiuto:** EF Core con Owned Types supporta meglio il nostro Rich Domain Model con Value Objects.

## Consequences

### Positive

- **Owned Types**: `Recipient` Value Object mappato automaticamente a colonne
- **Code-First Migrations**: schema evolution tracciata in Git
- **LINQ queries**: type-safe, refactorable
- **Change Tracking**: EF Core traccia modifiche automaticamente
- **Testcontainers**: integration tests con PostgreSQL reale
- **Gratuito**: nessun costo di licenza

### Negative

- **Overhead EF Core**: più lento di raw SQL per query semplici
- **Learning curve**: Fluent API, migrations, owned types
- **Docker richiesto**: per sviluppo locale serve PostgreSQL container
- **N+1 potenziale**: bisogna usare `.Include()` correttamente

### Neutral

- **Connection pooling**: gestito da Npgsql, configurazione necessaria per produzione
- **Migrations in CI/CD**: bisogna automatizzare `dotnet ef database update`

## Implementation Details

### Pacchetti NuGet

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10" />
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=notification_service;Username=notification;Password=..."
  }
}
```

### Migrations Create

```bash
dotnet ef migrations add MigrationName \
  --project src/NotificationService.Infrastructure \
  --startup-project src/NotificationService.Api
```

### Testing Strategy

- **Unit tests**: Mock repository con NSubstitute
- **Integration tests**: Testcontainers con PostgreSQL reale

## References

- [EF Core with PostgreSQL - Official Docs](https://www.npgsql.org/efcore/)
- [Owned Entity Types - EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- ADR-002: Rich Domain Model - Value Objects che usiamo
- ADR-001: Clean Architecture - Repository pattern

---

*Autore: Dan*
*Data: 2026-02-19*
