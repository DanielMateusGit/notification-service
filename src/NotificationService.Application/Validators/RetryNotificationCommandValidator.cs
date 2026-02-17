using FluentValidation;
using NotificationService.Application.Commands.Notifications;

namespace NotificationService.Application.Validators;

/// <summary>
/// Validator per RetryNotificationCommand
/// Valida solo che il Guid non sia vuoto (input validation)
/// L'esistenza e lo stato della notifica sono verificati nell'Handler (business logic)
/// </summary>
public class RetryNotificationCommandValidator : AbstractValidator<RetryNotificationCommand>
{
    public RetryNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("NotificationId is required");
    }
}
