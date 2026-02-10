namespace NotificationService.Domain.Enums;

/// <summary>
/// Priorità della notifica (determina numero di retry)
/// Business Rule: Higher priority = more retries
/// </summary>
public enum Priority
{
    /// <summary>
    /// Bassa priorità - 2 retry
    /// </summary>
    Low = 0,

    /// <summary>
    /// Priorità normale - 3 retry
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Alta priorità - 5 retry
    /// </summary>
    High = 2,

    /// <summary>
    /// Priorità critica - 10 retry
    /// </summary>
    Critical = 3
}
