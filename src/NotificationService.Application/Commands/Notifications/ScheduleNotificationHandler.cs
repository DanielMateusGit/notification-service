using MediatR;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Handler: Gestisce ScheduleNotificationCommand
/// Crea una nuova notifica e la schedula per invio futuro
/// </summary>
public class ScheduleNotificationHandler : IRequestHandler<ScheduleNotificationCommand, Guid>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ScheduleNotificationCommand command, CancellationToken cancellationToken)
    {
        // 1. Crea l'entity usando il Domain (validazione nel costruttore)
        var notification = new Notification(
            recipient: command.Recipient,
            channel: command.Channel,
            content: command.Content,
            priority: command.Priority,
            subject: command.Subject
        );

        // 2. Schedula (logica di business nel Domain, solleva evento)
        notification.Schedule(command.ScheduledAt);

        // 3. Persisti
        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Ritorna l'ID creato
        return notification.Id;
    }
}
