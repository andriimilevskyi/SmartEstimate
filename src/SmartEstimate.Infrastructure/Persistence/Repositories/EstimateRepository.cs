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

    public Task<Estimate?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Estimates
            .IgnoreQueryFilters()
            .Include(estimate => estimate.Zones)
            .Include(estimate => estimate.WorkItems)
            .Include(estimate => estimate.MaterialItems)
            .SingleOrDefaultAsync(estimate => estimate.Id == id, cancellationToken);

    public async Task<EstimatePage> GetPageAsync(EstimateListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.Estimates.AsNoTracking();

        if (query.ObjectId is { } objectId)
        {
            source = source.Where(estimate => estimate.ObjectId == objectId);
        }

        if (query.CustomerId is { } customerId)
        {
            source =
                from estimate in source
                join estimateObject in dbContext.EstimateObjects.IgnoreQueryFilters().AsNoTracking() on estimate.ObjectId equals estimateObject.Id
                where estimateObject.CustomerId == customerId
                select estimate;
        }

        if (query.Status is { } status)
        {
            source = source.Where(estimate => estimate.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source =
                from estimate in source
                join estimateObject in dbContext.EstimateObjects.IgnoreQueryFilters().AsNoTracking() on estimate.ObjectId equals estimateObject.Id
                join customer in dbContext.Customers.IgnoreQueryFilters().AsNoTracking() on estimateObject.CustomerId equals customer.Id
                where EF.Functions.ILike(estimateObject.Name, term)
                    || (estimateObject.Address != null && EF.Functions.ILike(estimateObject.Address, term))
                    || EF.Functions.ILike(customer.Name, term)
                    || (customer.Phone != null && EF.Functions.ILike(customer.Phone, term))
                    || (customer.Email != null && EF.Functions.ILike(customer.Email, term))
                select estimate;
        }

        source = source
            .OrderByDescending(estimate => estimate.CreatedAt)
            .ThenByDescending(estimate => estimate.Id);

        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

        return new EstimatePage(items, totalCount);
    }

    public Task<bool> ExistsForObjectAsync(Guid objectId, CancellationToken cancellationToken) =>
        dbContext.Estimates
            .IgnoreQueryFilters()
            .AnyAsync(estimate => estimate.ObjectId == objectId, cancellationToken);

    public Task RemoveAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(estimate);

        dbContext.Estimates.Remove(estimate);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
