using Microsoft.Extensions.DependencyInjection;
using SmartEstimate.Documents.EstimateDocuments;
using SmartEstimate.Documents.EstimateDocuments.Pdf;

namespace SmartEstimate.Documents;

/// <summary>
/// Registers document rendering services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDocuments(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDocumentTextProvider, DocumentTextProvider>();
        services.AddSingleton<IEstimateDocumentRenderer, PdfEstimateDocumentRenderer>();

        return services;
    }
}
