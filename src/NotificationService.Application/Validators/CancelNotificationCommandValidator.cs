using FluentValidation;
using NotificationService.Application.Commands.Notifications;

namespace NotificationService.Application.Validators;

/// <summary>
/// Validator per CancelNotificationCommand
/// Valida solo che il Guid non sia vuoto (input validation)
/// L'esistenza della notifica è verificata nell'Handler (business logic)
/// </summary>
public class CancelNotificationCommandValidator : AbstractValidator<CancelNotificationCommand>
{
    public CancelNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("NotificationId is required");
    }
}
