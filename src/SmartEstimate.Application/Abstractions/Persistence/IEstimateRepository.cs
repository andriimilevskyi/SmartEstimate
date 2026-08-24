using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Application.Abstractions.Persistence;

/// <summary>
/// Persistence abstraction for the Estimate aggregate.
/// </summary>
public interface IEstimateRepository
{
    Task AddAsync(Estimate estimate, CancellationToken cancellationToken);

    Task<bool> ExistsByNumberAsync(EstimateNumber number, CancellationToken cancellationToken);

    Task<Estimate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Estimate?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken);

    Task<EstimatePage> GetPageAsync(EstimateListQuery query, CancellationToken cancellationToken);

    Task<bool> ExistsForObjectAsync(Guid objectId, CancellationToken cancellationToken);

    Task RemoveAsync(Estimate estimate, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// A page of Estimate aggregates returned by persistence.
/// </summary>
public sealed record EstimatePage(IReadOnlyCollection<Estimate> Items, int TotalCount);

public sealed record EstimateListQuery(
    int Page,
    int PageSize,
    string? Search = null,
    EstimateStatus? Status = null,
    Guid? CustomerId = null,
    Guid? ObjectId = null);
