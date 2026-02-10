namespace NotificationService.Domain.Enums;

/// <summary>
/// Stato di un singolo tentativo di consegna
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// Tentativo in corso
    /// </summary>
    InProgress,

    /// <summary>
    /// Consegna riuscita
    /// </summary>
    Success,

    /// <summary>
    /// Consegna fallita
    /// </summary>
    Failed
}
