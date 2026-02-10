using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Entity: Tentativo di consegna
/// Rappresenta un singolo tentativo di invio di una notifica (immutabile dopo completamento)
/// </summary>
public class DeliveryAttempt
{
    // ===== PROPERTIES (State) =====

    /// <summary>
    /// Identificatore univoco del tentativo
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID della notifica associata
    /// </summary>
    public Guid NotificationId { get; private set; }

    /// <summary>
    /// Numero progressivo del tentativo (1, 2, 3, ...)
    /// </summary>
    public int AttemptNumber { get; private set; }

    /// <summary>
    /// Stato del tentativo
    /// </summary>
    public DeliveryStatus Status { get; private set; }

    /// <summary>
    /// Messaggio di errore (solo se Failed)
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Timestamp di inizio tentativo
    /// </summary>
    public DateTime AttemptedAt { get; private set; }

    /// <summary>
    /// Timestamp di completamento (Success o Failed)
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    // ===== CONSTRUCTOR =====

    /// <summary>
    /// Crea un nuovo tentativo di consegna
    /// </summary>
    /// <param name="notificationId">ID della notifica</param>
    /// <param name="attemptNumber">Numero del tentativo (deve essere >= 1)</param>
    /// <exception cref="ArgumentException">Se i parametri non sono validi</exception>
    public DeliveryAttempt(Guid notificationId, int attemptNumber)
    {
        // VALIDAZIONI
        if (notificationId == Guid.Empty)
            throw new ArgumentException("NotificationId cannot be empty", nameof(notificationId));

        if (attemptNumber < 1)
            throw new ArgumentException("AttemptNumber must be at least 1", nameof(attemptNumber));

        // INIZIALIZZAZIONE
        Id = Guid.NewGuid();
        NotificationId = notificationId;
        AttemptNumber = attemptNumber;
        Status = DeliveryStatus.InProgress;
        AttemptedAt = DateTime.UtcNow;
    }

    // ===== BEHAVIORS (Business Rules) =====

    /// <summary>
    /// Marca il tentativo come completato con successo
    /// Business Rule: Può essere completato solo se InProgress
    /// </summary>
    /// <exception cref="InvalidOperationException">Se non è InProgress</exception>
    public void MarkAsSuccess()
    {
        if (Status != DeliveryStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot mark attempt as success when status is {Status}. Must be InProgress.");

        Status = DeliveryStatus.Success;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca il tentativo come fallito
    /// Business Rule: Può fallire solo se InProgress
    /// </summary>
    /// <param name="errorMessage">Messaggio di errore (obbligatorio)</param>
    /// <exception cref="InvalidOperationException">Se non è InProgress</exception>
    /// <exception cref="ArgumentException">Se errorMessage è vuoto</exception>
    public void MarkAsFailed(string errorMessage)
    {
        if (Status != DeliveryStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot mark attempt as failed when status is {Status}. Must be InProgress.");

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        Status = DeliveryStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se il tentativo è ancora in corso
    /// </summary>
    public bool IsInProgress => Status == DeliveryStatus.InProgress;

    /// <summary>
    /// Verifica se il tentativo è completato (success o failed)
    /// </summary>
    public bool IsCompleted => Status != DeliveryStatus.InProgress;

    /// <summary>
    /// Calcola la durata del tentativo (se completato)
    /// </summary>
    /// <returns>Durata o null se ancora in corso</returns>
    public TimeSpan? GetDuration()
    {
        if (CompletedAt == null)
            return null;

        return CompletedAt.Value - AttemptedAt;
    }
}
