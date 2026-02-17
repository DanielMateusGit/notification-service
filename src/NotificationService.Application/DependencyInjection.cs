using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Behaviors;

namespace NotificationService.Application;

/// <summary>
/// Estensione per registrare i servizi dell'Application Layer nel DI container
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Registra MediatR e scansiona questo assembly per trovare tutti gli Handler
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Registra tutti i Validators da questo assembly
        services.AddValidatorsFromAssembly(assembly);

        // Registra i Pipeline Behaviors (ordine = ordine di esecuzione)
        // 1. Validation - prima valida l'input
        // 2. Logging - poi logga la request
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
