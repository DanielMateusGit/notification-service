using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Queries.Notifications;

/// <summary>
/// Handler: Gestisce GetNotificationByIdQuery
/// Legge una notifica e la mappa a DTO
/// </summary>
public class GetNotificationByIdHandler : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    private readonly INotificationRepository _repository;

    public GetNotificationByIdHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationDto?> Handle(GetNotificationByIdQuery query, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (notification is null)
            return null;

        // Mappa Entity → DTO (no entity fuori dall'Application Layer!)
        return new NotificationDto(
            Id: notification.Id,
            Recipient: notification.Recipient.Value,
            Channel: notification.Recipient.Channel.ToString(),
            Content: notification.Content,
            Subject: notification.Subject,
            Status: notification.Status.ToString(),
            Priority: notification.Priority.ToString(),
            CreatedAt: notification.CreatedAt,
            ScheduledAt: notification.ScheduledAt,
            SentAt: notification.SentAt
        );
    }
}
