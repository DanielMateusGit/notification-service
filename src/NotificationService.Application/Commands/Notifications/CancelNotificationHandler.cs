using MediatR;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Handler: Gestisce CancelNotificationCommand
/// Cancella una notifica esistente (solo se Pending)
/// </summary>
public class CancelNotificationHandler : IRequestHandler<CancelNotificationCommand, bool>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CancelNotificationCommand command, CancellationToken cancellationToken)
    {
        // 1. Carica l'entity
        var notification = await _repository.GetByIdAsync(command.NotificationId, cancellationToken);

        if (notification is null)
            return false;

        // 2. Cancella (logica di business nel Domain)
        notification.Cancel();

        // 3. Persisti
        await _repository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
