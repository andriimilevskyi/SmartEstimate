using System.Diagnostics;
using SmartEstimate.Application.Estimates;
using SmartEstimate.Application.Estimates.GetEstimateById;
using SmartEstimate.Contracts.Common;
using SmartEstimate.Documents.EstimateDocuments;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Api.Endpoints.Estimates;

/// <summary>
/// Maps document generation endpoints for estimates.
/// </summary>
public static class EstimateDocumentEndpoints
{
    public static IEndpointRouteBuilder MapEstimateDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/v1/estimates")
            .WithTags("Estimate Documents");

        group.MapGet("/document-templates", GetTemplates)
            .WithName("GetEstimateDocumentTemplates")
            .WithSummary("Get estimate document templates")
            .Produces<ApiResponse<IReadOnlyCollection<DocumentTemplateDefinition>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}/documents/pdf", GetPdfAsync)
            .WithName("GetEstimatePdfDocument")
            .WithSummary("Generate an estimate PDF")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static IResult GetTemplates(
        IEnumerable<IEstimateDocumentRenderer> renderers,
        HttpContext httpContext,
        string locale = DocumentLocales.DefaultCode)
    {
        if (!DocumentLocales.TryParse(locale, out var documentLocale))
        {
            return ExpectedError(
                new Error("ValidationError", $"Unsupported document locale '{locale}'."),
                httpContext);
        }

        var templates = renderers
            .Where(renderer => renderer.Format == DocumentOutputFormat.Pdf)
            .SelectMany(renderer => renderer.GetTemplates(documentLocale))
            .ToArray();

        return Results.Ok(ApiResponse<IReadOnlyCollection<DocumentTemplateDefinition>>.FromData(templates));
    }

    private static async Task<IResult> GetPdfAsync(
        Guid id,
        GetEstimateByIdHandler handler,
        IEnumerable<IEstimateDocumentRenderer> renderers,
        IConfiguration configuration,
        IHostEnvironment environment,
        HttpContext httpContext,
        CancellationToken cancellationToken,
        string template = "full-estimate",
        string locale = DocumentLocales.DefaultCode,
        string disposition = "attachment")
    {
        var renderer = renderers.Single(candidate => candidate.Format == DocumentOutputFormat.Pdf);
        if (!DocumentLocales.TryParse(locale, out var documentLocale))
        {
            return ExpectedError(
                new Error("ValidationError", $"Unsupported document locale '{locale}'."),
                httpContext);
        }

        if (!TryResolveTemplate(template, renderer.GetTemplates(documentLocale), out var documentTemplate))
        {
            return ExpectedError(
                new Error("ValidationError", $"Unknown estimate document template '{template}'."),
                httpContext);
        }

        if (!TryResolveDisposition(disposition, out var inline))
        {
            return ExpectedError(
                new Error("ValidationError", $"Unknown content disposition '{disposition}'."),
                httpContext);
        }

        var result = await handler.HandleAsync(
            new GetEstimateByIdQuery(id),
            cancellationToken,
            EstimateApiLocale.FromDocumentLocale(documentLocale));
        if (!result.IsSuccess || result.Value is null)
        {
            return ExpectedError(result.Error, httpContext);
        }

        var document = renderer.Render(new EstimateDocumentRenderRequest(
            documentTemplate,
            MapEstimate(result.Value),
            GetCompanyProfile(configuration, environment),
            DateTimeOffset.UtcNow,
            documentLocale));

        return Results.File(
            document.Content,
            document.ContentType,
            inline ? null : document.FileName,
            enableRangeProcessing: true);
    }

    private static bool TryResolveTemplate(
        string code,
        IReadOnlyCollection<DocumentTemplateDefinition> templates,
        out EstimateDocumentTemplate template)
    {
        var normalized = code.Trim();
        foreach (var definition in templates)
        {
            if (string.Equals(definition.Code, normalized, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definition.Template.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                template = definition.Template;
                return true;
            }
        }

        template = EstimateDocumentTemplate.FullEstimate;
        return false;
    }

    private static bool TryResolveDisposition(string value, out bool inline)
    {
        var normalized = value.Trim();
        if (string.Equals(normalized, "inline", StringComparison.OrdinalIgnoreCase))
        {
            inline = true;
            return true;
        }

        if (string.Equals(normalized, "attachment", StringComparison.OrdinalIgnoreCase))
        {
            inline = false;
            return true;
        }

        inline = false;
        return false;
    }

    private static EstimateDocumentModel MapEstimate(EstimateDetailsResponse estimate)
    {
        var workItemsByZone = estimate.WorkItems
            .GroupBy(item => item.ZoneId)
            .ToDictionary(group => group.Key, group => group.Select(MapLineItem).ToArray());
        var materialItemsByZone = estimate.MaterialItems
            .GroupBy(item => item.ZoneId)
            .ToDictionary(group => group.Key, group => group.Select(MapLineItem).ToArray());
        var zones = estimate.Zones
            .OrderBy(zone => zone.SortOrder)
            .Select(zone => new EstimateDocumentZone(
                zone.Id,
                zone.Name,
                zone.TotalLabor,
                zone.TotalMaterials,
                zone.GrandTotal,
                workItemsByZone.GetValueOrDefault(zone.Id, []),
                materialItemsByZone.GetValueOrDefault(zone.Id, [])))
            .ToArray();

        return new EstimateDocumentModel(
            estimate.Id,
            estimate.EstimateNumber,
            estimate.Currency,
            estimate.BusinessContext.CustomerName,
            estimate.BusinessContext.CustomerPhone,
            estimate.BusinessContext.CustomerEmail,
            estimate.BusinessContext.Name,
            estimate.BusinessContext.ObjectType,
            estimate.BusinessContext.Address,
            estimate.BusinessContext.TotalArea,
            estimate.BusinessContext.Description,
            estimate.Notes,
            estimate.TotalLabor,
            estimate.TotalMaterials,
            estimate.GrandTotal,
            estimate.CreatedAt,
            zones);
    }

    private static EstimateDocumentLineItem MapLineItem(EstimateLineItemResponse item) => new(
        item.Name,
        item.Quantity,
        item.MeasurementUnit,
        item.UnitPrice,
        item.Total,
        item.Notes);

    private static DocumentCompanyProfile GetCompanyProfile(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var section = configuration.GetSection("Documents:DefaultCompany");
        var name = section["Name"];
        var contacts = section.GetSection("Contacts")
            .GetChildren()
            .Select(contact => contact.Value)
            .OfType<string>()
            .Where(contact => !string.IsNullOrWhiteSpace(contact))
            .ToArray();
        var logoPath = ResolveLogoPath(section["LogoPath"], environment);

        return new DocumentCompanyProfile(
            string.IsNullOrWhiteSpace(name) ? "SmartEstimate" : name.Trim(),
            contacts,
            logoPath);
    }

    private static string? ResolveLogoPath(string? logoPath, IHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        var normalized = logoPath.Trim();
        return Path.IsPathFullyQualified(normalized)
            ? normalized
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, normalized));
    }

    private static IResult ExpectedError(Error error, HttpContext httpContext)
    {
        var payload = ApiResponse<object>.FromError(new ApiError(
            error.Code,
            error.Message,
            Activity.Current?.Id ?? httpContext.TraceIdentifier));

        return Results.Json(payload, statusCode: GetStatusCode(error));
    }

    private static int GetStatusCode(Error error) => error.Code switch
    {
        "ValidationError" => StatusCodes.Status422UnprocessableEntity,
        "EstimateNotFound" => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status400BadRequest
    };

}
