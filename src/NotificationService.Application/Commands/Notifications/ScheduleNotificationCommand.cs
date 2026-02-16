using MediatR;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Command: Schedula una nuova notifica per invio futuro
/// </summary>
public record ScheduleNotificationCommand(
    string Recipient,
    NotificationChannel Channel,
    string Content,
    DateTime ScheduledAt,
    Priority Priority = Priority.Normal,
    string? Subject = null
) : IRequest<Guid>;
