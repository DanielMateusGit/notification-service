# ADR-001: Adozione di Clean Architecture

## Status

**Accepted** - 2026-02-02

## Context

Stiamo costruendo un **Notification Service** che dovrà:
- Durare anni in produzione
- Essere mantenuto da team diversi
- Supportare multiple canali (email, SMS, push, webhook)
- Essere testabile senza dipendenze esterne
- Permettere di cambiare provider (es. da SendGrid a Mailgun) senza riscrivere la business logic

Abbiamo bisogno di una struttura che:
1. Separi chiaramente le responsabilità
2. Renda il codice testabile in isolamento
3. Permetta di sostituire componenti infrastrutturali
4. Sia comprensibile per nuovi sviluppatori
5. Scali con la complessità del progetto

## Decision

Adottiamo **Clean Architecture** (Robert C. Martin) con la seguente struttura:

```
src/
├── NotificationService.Domain/           # Entità, Value Objects, Regole business
├── NotificationService.Application/      # Use cases, Interfacce (Ports)
├── NotificationService.Infrastructure/   # Implementazioni (DB, Email, SMS)
└── NotificationService.Api/              # Controllers, Middleware, DI
```

### Dependency Rule

Le dipendenze puntano **verso il centro**:

```
Api → Application → Domain
         ↑
Infrastructure
```

- **Domain** non dipende da nulla
- **Application** dipende solo da Domain, definisce interfacce
- **Infrastructure** implementa le interfacce di Application
- **Api** orchestra tutto e configura Dependency Injection

## Alternatives Considered

### 1. Traditional N-Layer (Controller → Service → Repository)

```
Controllers → Services → Repositories → Database
```

**Pro:**
- Più semplice da capire inizialmente
- Meno boilerplate

**Contro:**
- Business logic spesso finisce nei Controller o nei Service
- Dipendenze verso il database ovunque
- Difficile testare senza database reale
- Cambiare database = modificare molti file

**Motivo del rifiuto:** Per un sistema che deve durare anni e cambiare provider, le dipendenze nascoste diventano un problema serio.

---

### 2. Vertical Slice Architecture

```
Features/
├── SendNotification/
│   ├── SendNotificationCommand.cs
│   ├── SendNotificationHandler.cs
│   └── SendNotificationEndpoint.cs
├── GetNotification/
└── ...
```

**Pro:**
- Codice di una feature tutto insieme
- Facile trovare tutto ciò che riguarda una funzionalità

**Contro:**
- Può portare a duplicazione tra feature
- Meno chiara la separazione delle responsabilità
- Le regole di dominio possono essere duplicate

**Motivo del rifiuto:** Per un sistema con regole di business chiare (retry logic, template rendering, delivery tracking), Clean Architecture offre migliore riuso e coerenza.

---

### 3. Minimal API senza struttura

**Pro:**
- Veloce da iniziare
- Meno file

**Contro:**
- Non scala con la complessità
- Diventa spaghetti code rapidamente
- Impossibile testare in isolamento

**Motivo del rifiuto:** Non adatto per un sistema enterprise che dovrà gestire 10k+ notifiche al minuto.

## Consequences

### Positive

- **Testabilità:** Domain e Application testabili senza database/servizi esterni
- **Sostituibilità:** Possiamo cambiare da SendGrid a Mailgun modificando solo Infrastructure
- **Manutenibilità:** Chiaro dove mettere ogni tipo di codice
- **Onboarding:** Nuovi dev capiscono velocemente la struttura
- **Longevità:** Il core business è protetto da cambiamenti tecnologici

### Negative

- **Boilerplate iniziale:** Più file e cartelle rispetto a un approccio minimal
- **Curva di apprendimento:** Dev junior potrebbero non conoscere il pattern
- **Over-engineering per casi semplici:** Per un CRUD banale sarebbe eccessivo

### Neutral

- **Interfacce ovunque:** Ogni dipendenza esterna richiede un'interfaccia in Application e un'implementazione in Infrastructure. È più lavoro ma forza il disaccoppiamento.

## References

- [The Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Clean Architecture book](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)
- Progetto: https://github.com/DanielMateusGit/notification-service

---

*Autore: Dan*
*Data: 2026-02-02*
