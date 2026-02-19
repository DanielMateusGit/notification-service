using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;

namespace NotificationService.Infrastructure;

/// <summary>
/// Estensione per registrare i servizi dell'Infrastructure Layer nel DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext con PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<INotificationRepository, PostgresNotificationRepository>();
        services.AddScoped<ITemplateRepository, PostgresTemplateRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // TODO: Week 5-6 - Aggiungi qui:
        // - Redis cache
        // - Email/SMS providers (SendGrid, Twilio)
        // - Background job services

        return services;
    }
}
