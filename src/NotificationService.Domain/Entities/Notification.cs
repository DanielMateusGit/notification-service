using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Entity: Notifica
/// Rappresenta una notifica da inviare attraverso un canale specifico
/// </summary>
public class Notification : Entity
{
    // ===== PROPERTIES (State) =====

    /// <summary>
    /// Identificatore univoco della notifica
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Destinatario della notifica (Value Object con validazione)
    /// </summary>
    public Recipient Recipient { get; private set; }

    /// <summary>
    /// Contenuto della notifica
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Subject (solo per Email)
    /// </summary>
    public string? Subject { get; private set; }

    /// <summary>
    /// Stato corrente della notifica
    /// </summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>
    /// Priorità (determina numero di retry)
    /// </summary>
    public Priority Priority { get; private set; }

    /// <summary>
    /// Data di creazione
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data di schedulazione (quando deve essere inviata)
    /// </summary>
    public DateTime? ScheduledAt { get; private set; }

    /// <summary>
    /// Data di invio effettivo
    /// </summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>
    /// Data di fallimento (dopo tutti i retry)
    /// </summary>
    public DateTime? FailedAt { get; private set; }

    /// <summary>
    /// Messaggio di errore (se fallita)
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // ===== CONSTRUCTOR =====

    // ===== CONSTRUCTORS =====

    /// <summary>
    /// Costruttore privato per EF Core (materializzazione da database)
    /// </summary>
    private Notification()
    {
        Recipient = null!; // EF Core imposterà il valore via Owned Type
        Content = string.Empty;
    }

    /// <summary>
    /// Crea una nuova notifica
    /// </summary>
    /// <param name="recipient">Destinatario (Value Object)</param>
    /// <param name="content">Contenuto del messaggio</param>
    /// <param name="priority">Priorità (default: Normal)</param>
    /// <param name="subject">Subject (opzionale, solo per Email)</param>
    /// <exception cref="ArgumentException">Se i parametri non sono validi</exception>
    /// <exception cref="ArgumentNullException">Se recipient è null</exception>
    public Notification(
        Recipient recipient,
        string content,
        Priority priority = Priority.Normal,
        string? subject = null)
    {
        // VALIDAZIONI (Business Rule: dati sempre validi)
        ArgumentNullException.ThrowIfNull(recipient);

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        if (recipient.Channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required for Email notifications", nameof(subject));

        // INIZIALIZZAZIONE
        Id = Guid.NewGuid();
        Recipient = recipient;
        Content = content;
        Subject = subject;
        Priority = priority;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // ===== BEHAVIORS (Business Rules) =====

    /// <summary>
    /// Schedula la notifica per un invio futuro
    /// Business Rule: Può essere schedulata solo se Pending
    /// </summary>
    /// <param name="scheduledAt">Data/ora di invio</param>
    /// <exception cref="InvalidOperationException">Se non è in stato Pending</exception>
    public void Schedule(DateTime scheduledAt)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot schedule notification in status {Status}. Must be Pending.");

        if (scheduledAt <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled time must be in the future", nameof(scheduledAt));

        ScheduledAt = scheduledAt;

        AddDomainEvent(new NotificationScheduledEvent(
            NotificationId: Id,
            Recipient: Recipient,
            ScheduledAt: scheduledAt
        ));
    }

    /// <summary>
    /// Marca la notifica come inviata
    /// Business Rule: Può essere inviata solo se Pending
    /// </summary>
    /// <exception cref="InvalidOperationException">Se non è in stato Pending</exception>
    public void Send()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException($"Cannot send notification in status {Status}. Must be Pending.");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationSentEvent(
            NotificationId: Id,
            Recipient: Recipient,
            SentAt: SentAt.Value
        ));
    }

    /// <summary>
    /// Marca la notifica come fallita
    /// Business Rule: Può fallire solo se Pending
    /// </summary>
    /// <param name="errorMessage">Messaggio di errore</param>
    /// <exception cref="InvalidOperationException">Se non è in stato Pending</exception>
    public void Fail(string errorMessage)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot fail notification in status {Status}. Must be Pending.");

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        Status = NotificationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        AddDomainEvent(new NotificationFailedEvent(
            NotificationId: Id,
            Recipient: Recipient,
            ErrorMessage: errorMessage,
            FailedAt: FailedAt.Value
        ));
    }

    /// <summary>
    /// Cancella la notifica
    /// Business Rule: Può essere cancellata solo se Pending
    /// </summary>
    /// <exception cref="InvalidOperationException">Se non è in stato Pending</exception>
    public void Cancel()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot cancel notification in status {Status}. Must be Pending.");

        Status = NotificationStatus.Cancelled;
    }

    /// <summary>
    /// Ritenta l'invio di una notifica fallita
    /// Business Rule: Può essere ritentata solo se Failed
    /// </summary>
    /// <exception cref="InvalidOperationException">Se non è in stato Failed</exception>
    public void Retry()
    {
        if (Status != NotificationStatus.Failed)
            throw new InvalidOperationException(
                $"Cannot retry notification in status {Status}. Must be Failed.");

        var previousError = ErrorMessage;

        Status = NotificationStatus.Pending;
        ErrorMessage = null;
        FailedAt = null;

        AddDomainEvent(new NotificationRetriedEvent(
            NotificationId: Id,
            Recipient: Recipient,
            PreviousError: previousError
        ));
    }

    /// <summary>
    /// Verifica se la notifica può essere ritentata
    /// Business Rule: Max retry dipende dalla Priority
    /// - Low: 2 retry
    /// - Normal: 3 retry
    /// - High: 5 retry
    /// - Critical: 10 retry
    /// </summary>
    /// <param name="attemptNumber">Numero del tentativo corrente</param>
    /// <returns>True se può essere ritentata</returns>
    public bool CanRetry(int attemptNumber)
    {
        var maxRetries = Priority switch
        {
            Priority.Low => 2,
            Priority.Normal => 3,
            Priority.High => 5,
            Priority.Critical => 10,
            _ => 3
        };

        return attemptNumber < maxRetries;
    }

    /// <summary>
    /// Verifica se la notifica è pronta per essere inviata
    /// </summary>
    /// <returns>True se Status = Pending e (non schedulata o schedulata nel passato)</returns>
    public bool IsReadyToSend()
    {
        if (Status != NotificationStatus.Pending)
            return false;

        // Se non è schedulata, è pronta
        if (ScheduledAt == null)
            return true;

        // Se è schedulata, verifica che il momento sia arrivato
        return ScheduledAt.Value <= DateTime.UtcNow;
    }
}
