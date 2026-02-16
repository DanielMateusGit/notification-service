using MediatR;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Command: Cancella una notifica esistente
/// </summary>
public record CancelNotificationCommand(Guid NotificationId) : IRequest<bool>;
