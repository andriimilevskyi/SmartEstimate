using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimate.Application.Estimates;
using SmartEstimate.Application.Estimates.AddEstimateMaterialItem;
using SmartEstimate.Application.Estimates.AddEstimateZone;
using SmartEstimate.Application.Estimates.AddEstimateWorkItem;
using SmartEstimate.Application.Estimates.CreateEstimate;
using SmartEstimate.Application.Estimates.DeleteEstimate;
using SmartEstimate.Application.Estimates.DuplicateEstimateMaterialItem;
using SmartEstimate.Application.Estimates.DuplicateEstimateWorkItem;
using SmartEstimate.Application.Estimates.GetEstimateById;
using SmartEstimate.Application.Estimates.GetEstimates;
using SmartEstimate.Application.Estimates.RemoveEstimateMaterialItem;
using SmartEstimate.Application.Estimates.RemoveEstimateZone;
using SmartEstimate.Application.Estimates.RemoveEstimateWorkItem;
using SmartEstimate.Application.Estimates.ReorderEstimateZones;
using SmartEstimate.Application.Estimates.UpdateEstimateZone;
using SmartEstimate.Application.Estimates.UpdateEstimateMaterialItem;
using SmartEstimate.Application.Estimates.UpdateEstimateWorkItem;
using SmartEstimate.Application.Knowledge;

namespace SmartEstimate.Application;

/// <summary>
/// Registers application-layer dependencies. Business use cases are added as vertical slices.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        EstimateMappings.Register(TypeAdapterConfig.GlobalSettings);
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<CreateEstimateHandler>();
        services.AddScoped<GetEstimatesHandler>();
        services.AddScoped<GetEstimateByIdHandler>();
        services.AddScoped<DeleteEstimateHandler>();
        services.AddScoped<AddEstimateZoneHandler>();
        services.AddScoped<UpdateEstimateZoneHandler>();
        services.AddScoped<ReorderEstimateZonesHandler>();
        services.AddScoped<RemoveEstimateZoneHandler>();
        services.AddScoped<AddEstimateWorkItemHandler>();
        services.AddScoped<UpdateEstimateWorkItemHandler>();
        services.AddScoped<RemoveEstimateWorkItemHandler>();
        services.AddScoped<DuplicateEstimateWorkItemHandler>();
        services.AddScoped<AddEstimateMaterialItemHandler>();
        services.AddScoped<UpdateEstimateMaterialItemHandler>();
        services.AddScoped<RemoveEstimateMaterialItemHandler>();
        services.AddScoped<DuplicateEstimateMaterialItemHandler>();
        services.AddScoped<KnowledgeManagementService>();

        return services;
    }
}
