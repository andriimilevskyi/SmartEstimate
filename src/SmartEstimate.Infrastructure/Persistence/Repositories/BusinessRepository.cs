using Microsoft.EntityFrameworkCore;
using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Infrastructure.Persistence.Repositories;

public sealed class BusinessRepository(SmartEstimateDbContext dbContext) :
    ICustomerRepository,
    IEstimateObjectRepository
{
    Task<Customer?> ICustomerRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<Customer>> ICustomerRepository.GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Customer>();
        }

        return await dbContext.Customers
            .IgnoreQueryFilters()
            .Where(customer => ids.Contains(customer.Id))
            .ToArrayAsync(cancellationToken);
    }

    async Task<IReadOnlyCollection<Customer>> ICustomerRepository.ListAsync(CustomerListQuery query, CancellationToken cancellationToken) =>
        await CustomerQuery(query)
            .OrderBy(customer => customer.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

    Task<int> ICustomerRepository.CountAsync(CustomerListQuery query, CancellationToken cancellationToken) =>
        CustomerQuery(query).CountAsync(cancellationToken);

    Task ICustomerRepository.AddAsync(Customer customer, CancellationToken cancellationToken) =>
        dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();

    Task ICustomerRepository.RemoveAsync(Customer customer, CancellationToken cancellationToken)
    {
        dbContext.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    Task ICustomerRepository.SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    Task<EstimateObject?> IEstimateObjectRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.EstimateObjects
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(estimateObject => estimateObject.Id == id, cancellationToken);

    async Task<IReadOnlyCollection<EstimateObject>> IEstimateObjectRepository.GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<EstimateObject>();
        }

        return await dbContext.EstimateObjects
            .IgnoreQueryFilters()
            .Where(estimateObject => ids.Contains(estimateObject.Id))
            .ToArrayAsync(cancellationToken);
    }

    async Task<IReadOnlyCollection<EstimateObject>> IEstimateObjectRepository.ListAsync(EstimateObjectListQuery query, CancellationToken cancellationToken) =>
        await ObjectQuery(query)
            .OrderBy(estimateObject => estimateObject.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

    async Task<IReadOnlyCollection<EstimateObject>> IEstimateObjectRepository.ListRecentAsync(int count, CancellationToken cancellationToken) =>
        await dbContext.EstimateObjects
            .AsNoTracking()
            .OrderByDescending(estimateObject => estimateObject.UpdatedAt)
            .ThenByDescending(estimateObject => estimateObject.Id)
            .Take(count)
            .ToArrayAsync(cancellationToken);

    Task<int> IEstimateObjectRepository.CountAsync(EstimateObjectListQuery query, CancellationToken cancellationToken) =>
        ObjectQuery(query).CountAsync(cancellationToken);

    Task<bool> IEstimateObjectRepository.ExistsForCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        dbContext.EstimateObjects
            .IgnoreQueryFilters()
            .AnyAsync(estimateObject => estimateObject.CustomerId == customerId, cancellationToken);

    Task IEstimateObjectRepository.AddAsync(EstimateObject estimateObject, CancellationToken cancellationToken) =>
        dbContext.EstimateObjects.AddAsync(estimateObject, cancellationToken).AsTask();

    Task IEstimateObjectRepository.RemoveAsync(EstimateObject estimateObject, CancellationToken cancellationToken)
    {
        dbContext.EstimateObjects.Remove(estimateObject);
        return Task.CompletedTask;
    }

    Task IEstimateObjectRepository.SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Customer> CustomerQuery(CustomerListQuery query)
    {
        IQueryable<Customer> source = query.Status switch
        {
            BusinessRecordStatus.Archived => dbContext.Customers.IgnoreQueryFilters().Where(customer => customer.IsDeleted),
            BusinessRecordStatus.All => dbContext.Customers.IgnoreQueryFilters(),
            _ => dbContext.Customers
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(customer =>
                EF.Functions.ILike(customer.Name, term)
                || (customer.Phone != null && EF.Functions.ILike(customer.Phone, term))
                || (customer.Email != null && EF.Functions.ILike(customer.Email, term)));
        }

        return source;
    }

    private IQueryable<EstimateObject> ObjectQuery(EstimateObjectListQuery query)
    {
        IQueryable<EstimateObject> source = query.Status switch
        {
            BusinessRecordStatus.Archived => dbContext.EstimateObjects.IgnoreQueryFilters().Where(estimateObject => estimateObject.IsDeleted),
            BusinessRecordStatus.All => dbContext.EstimateObjects.IgnoreQueryFilters(),
            _ => dbContext.EstimateObjects
        };

        if (query.CustomerId is { } customerId)
        {
            source = source.Where(estimateObject => estimateObject.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source =
                from estimateObject in source
                join customer in dbContext.Customers.IgnoreQueryFilters() on estimateObject.CustomerId equals customer.Id
                where EF.Functions.ILike(estimateObject.Name, term)
                    || (estimateObject.Address != null && EF.Functions.ILike(estimateObject.Address, term))
                    || EF.Functions.ILike(customer.Name, term)
                select estimateObject;
        }

        return source;
    }
}
