using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Objects;

namespace SmartEstimate.Application.Business.Abstractions;

public enum BusinessRecordStatus
{
    Active,
    Archived,
    All
}

public sealed record CustomerListQuery(
    int Page,
    int PageSize,
    string? Search = null,
    BusinessRecordStatus Status = BusinessRecordStatus.Active);

public sealed record EstimateObjectListQuery(
    int Page,
    int PageSize,
    string? Search = null,
    Guid? CustomerId = null,
    BusinessRecordStatus Status = BusinessRecordStatus.Active);

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Customer>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Customer>> ListAsync(CustomerListQuery query, CancellationToken cancellationToken);
    Task<int> CountAsync(CustomerListQuery query, CancellationToken cancellationToken);
    Task RemoveAsync(Customer customer, CancellationToken cancellationToken);
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IEstimateObjectRepository
{
    Task<EstimateObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EstimateObject>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EstimateObject>> ListAsync(EstimateObjectListQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EstimateObject>> ListRecentAsync(int count, CancellationToken cancellationToken);
    Task<int> CountAsync(EstimateObjectListQuery query, CancellationToken cancellationToken);
    Task<bool> ExistsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task RemoveAsync(EstimateObject estimateObject, CancellationToken cancellationToken);
    Task AddAsync(EstimateObject estimateObject, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
