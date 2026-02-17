using FluentValidation;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Validators;

/// <summary>
/// Validator per ScheduleNotificationCommand
/// Valida l'INPUT (forma), non la business logic
/// </summary>
public class ScheduleNotificationCommandValidator : AbstractValidator<ScheduleNotificationCommand>
{
    public ScheduleNotificationCommandValidator()
    {
        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Recipient is required")
            .MaximumLength(255).WithMessage("Recipient must not exceed 255 characters");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Invalid notification channel");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(4000).WithMessage("Content must not exceed 4000 characters");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be in the future");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority");

        RuleFor(x => x.Subject)
            .MaximumLength(255).WithMessage("Subject must not exceed 255 characters")
            .When(x => x.Subject != null);

        // Validazione condizionale per Email
        RuleFor(x => x.Recipient)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => x.Channel == NotificationChannel.Email);

        // Validazione condizionale per SMS (formato base telefono)
        RuleFor(x => x.Recipient)
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Invalid phone number format")
            .When(x => x.Channel == NotificationChannel.Sms);

        // Validazione condizionale per Webhook (URL valido)
        RuleFor(x => x.Recipient)
            .Must(BeValidUrl).WithMessage("Invalid webhook URL")
            .When(x => x.Channel == NotificationChannel.Webhook);

        // Email channel richiede Subject
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required for email notifications")
            .When(x => x.Channel == NotificationChannel.Email);
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
