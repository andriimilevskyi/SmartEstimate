using Microsoft.EntityFrameworkCore;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Estimates.ValueObjects;

namespace SmartEstimate.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IEstimateRepository"/>.
/// </summary>
public sealed class EstimateRepository(SmartEstimateDbContext dbContext) : IEstimateRepository
{
    public async Task AddAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(estimate);

        await dbContext.Estimates.AddAsync(estimate, cancellationToken);
    }

    public Task<bool> ExistsByNumberAsync(EstimateNumber number, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(number);

        return dbContext.Estimates.AnyAsync(estimate => estimate.Number == number, cancellationToken);
    }

    public Task<Estimate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Estimates
            .Include(estimate => estimate.Zones)
            .Include(estimate => estimate.WorkItems)
            .Include(estimate => estimate.MaterialItems)
            .SingleOrDefaultAsync(estimate => estimate.Id == id, cancellationToken);

    public async Task<EstimatePage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Estimates
            .AsNoTracking()
            .OrderByDescending(estimate => estimate.CreatedAt)
            .ThenByDescending(estimate => estimate.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new EstimatePage(items, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
