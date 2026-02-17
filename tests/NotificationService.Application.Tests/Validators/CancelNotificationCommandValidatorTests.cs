using FluentValidation.TestHelper;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Application.Validators;

namespace NotificationService.Application.Tests.Validators;

public class CancelNotificationCommandValidatorTests
{
    private readonly CancelNotificationCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidNotificationId_ShouldPass()
    {
        // Arrange
        var command = new CancelNotificationCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldFail()
    {
        // Arrange
        var command = new CancelNotificationCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NotificationId)
            .WithErrorMessage("NotificationId is required");
    }
}
