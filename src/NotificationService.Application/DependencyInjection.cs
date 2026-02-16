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
        // Registra MediatR e scansiona questo assembly per trovare tutti gli Handler
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Registra i Pipeline Behaviors (ordine = ordine di esecuzione)
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
