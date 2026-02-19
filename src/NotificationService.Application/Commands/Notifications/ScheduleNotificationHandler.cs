using MediatR;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.ValueObjects;

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
        // 1. Crea il Recipient Value Object (validazione nel factory method)
        var recipient = Recipient.Create(command.Recipient, command.Channel);

        // 2. Crea l'entity usando il Domain (validazione nel costruttore)
        var notification = new Notification(
            recipient: recipient,
            content: command.Content,
            priority: command.Priority,
            subject: command.Subject
        );

        // 3. Schedula (logica di business nel Domain, solleva evento)
        notification.Schedule(command.ScheduledAt);

        // 4. Persisti
        await _repository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Ritorna l'ID creato
        return notification.Id;
    }
}
