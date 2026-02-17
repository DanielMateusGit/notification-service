using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Query: Ottiene tutte le notifiche con un determinato status
/// </summary>
public record GetNotificationsByStatusQuery(NotificationStatus Status) : IRequest<IReadOnlyList<NotificationDto>>;
