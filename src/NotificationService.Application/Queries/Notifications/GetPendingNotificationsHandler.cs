using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Handler: Gestisce GetPendingNotificationsQuery
/// Legge tutte le notifiche pronte per l'invio
/// </summary>
public class GetPendingNotificationsHandler : IRequestHandler<GetPendingNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetPendingNotificationsHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetPendingNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetPendingAsync(cancellationToken);

        // Filtra solo quelle pronte per l'invio (IsReadyToSend)
        // Nota: in produzione questo filtro sarebbe nel repository/database
        var readyNotifications = notifications.Where(n => n.IsReadyToSend());

        // Mappa Entity → DTO
        return readyNotifications.Select(n => new NotificationDto(
            Id: n.Id,
            Recipient: n.Recipient.Value,
            Channel: n.Recipient.Channel.ToString(),
            Content: n.Content,
            Subject: n.Subject,
            Status: n.Status.ToString(),
            Priority: n.Priority.ToString(),
            CreatedAt: n.CreatedAt,
            ScheduledAt: n.ScheduledAt,
            SentAt: n.SentAt
        )).ToList();
    }
}
