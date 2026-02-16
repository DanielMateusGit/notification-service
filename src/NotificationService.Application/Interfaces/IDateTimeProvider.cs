namespace NotificationService.Application.Interfaces;

/// <summary>
/// Port: Provider per data/ora corrente
/// Permette di mockare il tempo nei test
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
