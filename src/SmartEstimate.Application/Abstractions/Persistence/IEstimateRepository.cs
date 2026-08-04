using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Application.Abstractions.Persistence;

/// <summary>
/// Persistence abstraction for the Estimate aggregate.
/// </summary>
public interface IEstimateRepository
{
    Task AddAsync(Estimate estimate, CancellationToken cancellationToken);

    Task<bool> ExistsByNumberAsync(EstimateNumber number, CancellationToken cancellationToken);

    Task<Estimate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<EstimatePage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// A page of Estimate aggregates returned by persistence.
/// </summary>
public sealed record EstimatePage(IReadOnlyCollection<Estimate> Items, int TotalCount);
