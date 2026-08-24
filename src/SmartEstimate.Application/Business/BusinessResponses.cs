using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Application.Business;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Note,
    bool IsArchived,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

public sealed record EstimateObjectResponse(
    Guid Id,
    Guid CustomerId,
    string Name,
    string ObjectType,
    string? Address,
    decimal? TotalArea,
    string? Description,
    bool IsArchived,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

public sealed record EstimateObjectDetailsResponse(
    Guid Id,
    CustomerResponse Customer,
    string Name,
    string ObjectType,
    string? Address,
    decimal? TotalArea,
    string? Description,
    bool IsArchived,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version,
    int EstimateCount);

public sealed record CustomerDetailsResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Note,
    bool IsArchived,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

public sealed record PagedBusinessResponse<TItem>(
    IReadOnlyCollection<TItem> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record OverviewEstimateCountsResponse(
    int Total,
    int Draft,
    int InProgress,
    int Sent,
    int Approved,
    int Completed);

public sealed record OverviewObjectSummaryResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Name,
    string ObjectType,
    string? Address,
    decimal? TotalArea,
    DateTimeOffset UpdatedAt);

public sealed record OverviewResponse(
    OverviewEstimateCountsResponse Estimates,
    IReadOnlyCollection<SmartEstimate.Application.Estimates.EstimateSummaryResponse> RecentEstimates,
    IReadOnlyCollection<OverviewObjectSummaryResponse> RecentObjects);

public sealed record CreateCustomerRequest(string Name, string? Phone, string? Email, string? Note);

public sealed record CreateEstimateObjectRequest(
    Guid CustomerId,
    string Name,
    EstimateObjectType ObjectType,
    string? Address,
    decimal? TotalArea,
    string? Description);
