using MediatR;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Commands.Notifications;

/// <summary>
/// Handler: Gestisce RetryNotificationCommand
/// Ritenta l'invio di una notifica fallita
/// </summary>
public class RetryNotificationHandler : IRequestHandler<RetryNotificationCommand, bool>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RetryNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RetryNotificationCommand command, CancellationToken cancellationToken)
    {
        // 1. Carica l'entity
        var notification = await _repository.GetByIdAsync(command.NotificationId, cancellationToken);

        if (notification is null)
            return false;

        // 2. Retry (logica di business nel Domain - verifica stato, solleva evento)
        notification.Retry();

        // 3. Persisti
        await _repository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
