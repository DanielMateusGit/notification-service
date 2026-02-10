namespace NotificationService.Domain.Enums;

/// <summary>
/// Canale di invio della notifica
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Notifica via email
    /// </summary>
    Email = 0,

    /// <summary>
    /// Notifica via SMS
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Notifica push (mobile/web)
    /// </summary>
    Push = 2,

    /// <summary>
    /// Notifica via webhook HTTP
    /// </summary>
    Webhook = 3
}
