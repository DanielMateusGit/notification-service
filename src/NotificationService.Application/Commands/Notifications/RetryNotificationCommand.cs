using MediatR;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Command: Ritenta l'invio di una notifica fallita
/// </summary>
public record RetryNotificationCommand(Guid NotificationId) : IRequest<bool>;
