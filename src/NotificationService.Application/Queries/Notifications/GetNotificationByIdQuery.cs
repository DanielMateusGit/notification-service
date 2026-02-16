using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Query: Ottiene una notifica per ID
/// </summary>
public record GetNotificationByIdQuery(Guid Id) : IRequest<NotificationDto?>;
