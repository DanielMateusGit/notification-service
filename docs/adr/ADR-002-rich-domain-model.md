# ADR-002: Rich Domain Model vs Anemic Domain Model

## Status

**Accepted** - 2026-02-11

## Context

Nel Notification Service abbiamo entità con comportamento complesso:

- **Notification**: ha un ciclo di vita (Pending → Sent/Failed), regole su quando può essere inviata, retry logic
- **Template**: ha validazione di placeholders, versioning
- **DeliveryAttempt**: tracking di ogni tentativo con stato e metadata

Dobbiamo decidere **dove mettere la logica di business** relativa a queste entità:

1. **Nei Service** (Anemic Domain Model) - entità = contenitori di dati
2. **Nelle Entity** (Rich Domain Model) - entità = dati + comportamento

## Decision

Adottiamo un **Rich Domain Model** dove:

- Le entità incapsulano i propri **invarianti** e **regole di business**
- I **setter sono privati** - lo stato cambia solo tramite metodi con nomi significativi
- Le entità generano **Domain Events** per comunicare i cambiamenti
- I **Value Objects** garantiscono validità dei dati (EmailAddress, PhoneNumber, Recipient)

### Esempio Concreto

```csharp
// Rich Domain Model (il nostro approccio)
public class Notification : Entity
{
    public Recipient Recipient { get; private set; }      // Value Object, sempre valido
    public NotificationStatus Status { get; private set; } // Enum, type-safe

    private Notification() { }  // EF Core only

    public static Notification Schedule(Recipient recipient, ...)
    {
        // Validazione e invarianti QUI
        var notification = new Notification { ... };
        notification.AddDomainEvent(new NotificationScheduledEvent(...));
        return notification;
    }

    public void MarkAsSent()
    {
        // Regola di business: solo Pending può essere Sent
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Can only send pending notifications");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        AddDomainEvent(new NotificationSentEvent(Id, SentAt.Value));
    }

    public void MarkAsFailed(string reason, bool canRetry)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Can only fail pending notifications");

        Status = canRetry ? NotificationStatus.RetryScheduled : NotificationStatus.Failed;
        AddDomainEvent(new NotificationFailedEvent(Id, reason, canRetry));
    }
}
```

## Alternatives Considered

### 1. Anemic Domain Model

```csharp
// Entity = solo dati
public class Notification
{
    public Guid Id { get; set; }
    public string Recipient { get; set; }
    public string Status { get; set; }
    public DateTime? SentAt { get; set; }
}

// Logica nei Service
public class NotificationService
{
    public void Send(Notification notification)
    {
        if (notification.Status != "Pending")
            throw new Exception("Invalid status");

        notification.Status = "Sent";
        notification.SentAt = DateTime.UtcNow;
        _repository.Save(notification);
        _eventPublisher.Publish(new NotificationSentEvent(...));
    }
}
```

**Pro:**
- Più semplice inizialmente
- Entità "leggere" (solo POCO)
- Familiarità per chi viene da CRUD tradizionale

**Contro:**
- **Logica sparsa**: multiple classi possono modificare `Status` → chi è responsabile?
- **Validazione duplicata**: ogni Service deve validare le stesse regole
- **Stati invalidi possibili**: nulla impedisce `notification.Status = "InvalidState"`
- **Testing complesso**: devi mockare tutti i Service per testare le regole
- **Anti-pattern riconosciuto**: Martin Fowler lo definisce esplicitamente un anti-pattern

**Motivo del rifiuto:** Per un sistema con regole di dominio chiare (stati, transizioni, retry), l'Anemic Model porta a logica duplicata e difficile da mantenere.

---

### 2. Transaction Script

Tutta la logica in procedure step-by-step, senza entità vere.

**Pro:**
- Molto semplice per CRUD banali

**Contro:**
- Non scala con la complessità
- Zero riuso della logica
- Impossibile testare regole in isolamento

**Motivo del rifiuto:** Il Notification Service ha logica di retry, template rendering, multi-channel delivery - troppo complesso per Transaction Script.

## Consequences

### Positive

- **Entità sempre valide**: impossibile creare una Notification in stato inconsistente
- **Logica centralizzata**: "dove sta la regola per cambiare stato?" → nella Notification
- **Self-documenting**: `MarkAsSent()` è più chiaro di `notification.Status = "Sent"`
- **Testabilità**: posso testare le regole di dominio senza database o servizi
- **Domain Events**: side effects espliciti e disaccoppiati
- **Type safety**: `NotificationStatus.Sent` invece di magic string `"Sent"`

### Negative

- **Più codice iniziale**: costruttori privati, factory methods, property accessors
- **Curva di apprendimento**: dev abituati a CRUD devono cambiare mindset
- **EF Core friction**: richiede configurazione extra (private setters, backing fields)

### Neutral

- **Domain Events**: richiedono infrastruttura per il dispatch (MediatR o simile)
- **Value Objects**: più classi, ma garantiscono validità dei dati

## Implementation Details

### Value Objects Creati
- `EmailAddress` - valida formato email
- `PhoneNumber` - valida formato telefono
- `Recipient` - combina tipo canale + indirizzo

### Domain Events Creati
- `NotificationScheduledEvent` - quando viene creata
- `NotificationSentEvent` - quando viene inviata con successo
- `NotificationFailedEvent` - quando fallisce

### Entity Base Class
```csharp
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

## References

- [AnemicDomainModel - Martin Fowler](https://martinfowler.com/bliki/AnemicDomainModel.html) - Perché è un anti-pattern
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/) - Rich Domain Model originale
- [Implementing Domain-Driven Design - Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577) - Implementazione pratica
- ADR-001: Clean Architecture - Architettura che supporta questo approccio

---

*Autore: Dan*
*Data: 2026-02-11*
