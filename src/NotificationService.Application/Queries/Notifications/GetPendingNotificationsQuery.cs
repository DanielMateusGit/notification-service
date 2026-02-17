using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Query: Ottiene tutte le notifiche pending pronte per l'invio
/// (Status = Pending AND (non schedulata OR schedulata nel passato))
/// </summary>
public record GetPendingNotificationsQuery() : IRequest<IReadOnlyList<NotificationDto>>;
