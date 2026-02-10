namespace NotificationService.Domain.Enums;

/// <summary>
/// Rappresenta lo stato del ciclo di vita di una notifica
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notifica creata ma non ancora inviata
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Notifica inviata con successo
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Notifica fallita dopo tutti i tentativi
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Notifica cancellata dall'utente
    /// </summary>
    Cancelled = 3
}
