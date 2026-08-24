using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Application.Knowledge.Abstractions;
using SmartEstimate.Application.Pricing.Abstractions;
using SmartEstimate.Infrastructure.Persistence;
using SmartEstimate.Infrastructure.Persistence.Repositories;

namespace SmartEstimate.Infrastructure;

/// <summary>
/// Registers infrastructure adapters owned by the modular monolith.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<SmartEstimateDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IEstimateRepository, EstimateRepository>();
        services.AddScoped<BusinessRepository>();
        services.AddScoped<ICustomerRepository>(serviceProvider => serviceProvider.GetRequiredService<BusinessRepository>());
        services.AddScoped<IEstimateObjectRepository>(serviceProvider => serviceProvider.GetRequiredService<BusinessRepository>());
        services.AddScoped<KnowledgeRepository>();
        services.AddScoped<ICategoryRepository>(serviceProvider => serviceProvider.GetRequiredService<KnowledgeRepository>());
        services.AddScoped<IConstructionWorkRepository>(serviceProvider => serviceProvider.GetRequiredService<KnowledgeRepository>());
        services.AddScoped<IMaterialRepository>(serviceProvider => serviceProvider.GetRequiredService<KnowledgeRepository>());
        services.AddScoped<IUnitRepository>(serviceProvider => serviceProvider.GetRequiredService<KnowledgeRepository>());
        services.AddScoped<ICatalogPriceRepository, PricingRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<SmartEstimateDbContext>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["database", "ready"]);

        return services;
    }
}
