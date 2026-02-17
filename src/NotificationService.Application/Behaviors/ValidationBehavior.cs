using FluentValidation;
using MediatR;

namespace NotificationService.Application.Behaviors;

/// <summary>
/// Pipeline behavior che esegue la validazione PRIMA dell'handler.
/// Se la validazione fallisce, lancia ValidationException (mai raggiunge l'handler).
/// </summary>
/// <typeparam name="TRequest">Tipo della request (Command/Query)</typeparam>
/// <typeparam name="TResponse">Tipo della response</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se non ci sono validators per questa request, procedi
        if (!_validators.Any())
        {
            return await next();
        }

        // Crea il contesto di validazione
        var context = new ValidationContext<TRequest>(request);

        // Esegui tutti i validators e raccogli gli errori
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // Se ci sono errori, lancia ValidationException
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validazione OK, procedi all'handler
        return await next();
    }
}
