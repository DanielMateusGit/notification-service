namespace NotificationService.Application.DTOs;

/// <summary>
/// DTO per trasferire dati di Notification fuori dall'Application Layer
/// </summary>
public record NotificationDto(
    Guid Id,
    string Recipient,
    string Channel,
    string Content,
    string? Subject,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? ScheduledAt,
    DateTime? SentAt
);
