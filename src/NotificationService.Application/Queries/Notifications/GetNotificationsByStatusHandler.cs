using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Handler: Gestisce GetNotificationsByStatusQuery
/// Legge tutte le notifiche con un determinato status
/// </summary>
public class GetNotificationsByStatusHandler : IRequestHandler<GetNotificationsByStatusQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsByStatusHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetByStatusAsync(query.Status, cancellationToken);

        // Mappa Entity → DTO
        return notifications.Select(n => new NotificationDto(
            Id: n.Id,
            Recipient: n.Recipient,
            Channel: n.Channel.ToString(),
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
