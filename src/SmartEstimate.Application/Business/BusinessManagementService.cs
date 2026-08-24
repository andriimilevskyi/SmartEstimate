using FluentValidation;
using SmartEstimate.Application.Abstractions.Persistence;
using SmartEstimate.Application.Business.Abstractions;
using SmartEstimate.Application.Estimates;
using SmartEstimate.Domain.Customers;
using SmartEstimate.Domain.Estimates;
using SmartEstimate.Domain.Objects;
using SmartEstimate.Shared.Primitives;

namespace SmartEstimate.Application.Business;

public sealed class BusinessManagementService(
    ICustomerRepository customers,
    IEstimateObjectRepository objects,
    IEstimateRepository estimates,
    IValidator<CreateCustomerRequest> customerValidator,
    IValidator<CreateEstimateObjectRequest> objectValidator,
    EstimateResponseFactory estimateResponseFactory)
{
    public async Task<Result<OverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var totalEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1), cancellationToken);
        var draftEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1, Status: EstimateStatus.Draft), cancellationToken);
        var inProgressEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1, Status: EstimateStatus.InProgress), cancellationToken);
        var sentEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1, Status: EstimateStatus.Sent), cancellationToken);
        var approvedEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1, Status: EstimateStatus.Approved), cancellationToken);
        var completedEstimates = await estimates.GetPageAsync(new EstimateListQuery(1, 1, Status: EstimateStatus.Completed), cancellationToken);
        var recentEstimatesPage = await estimates.GetPageAsync(new EstimateListQuery(1, 5), cancellationToken);
        var recentEstimates = await estimateResponseFactory.CreateSummariesAsync(recentEstimatesPage.Items, cancellationToken);
        var recentObjects = await objects.ListRecentAsync(5, cancellationToken);
        var recentCustomers = await customers.GetByIdsAsync(
            recentObjects.Select(estimateObject => estimateObject.CustomerId).Distinct().ToArray(),
            cancellationToken);
        var customersById = recentCustomers.ToDictionary(customer => customer.Id);

        return Result<OverviewResponse>.Success(new OverviewResponse(
            new OverviewEstimateCountsResponse(
                totalEstimates.TotalCount,
                draftEstimates.TotalCount,
                inProgressEstimates.TotalCount,
                sentEstimates.TotalCount,
                approvedEstimates.TotalCount,
                completedEstimates.TotalCount),
            recentEstimates,
            recentObjects.Select(estimateObject =>
            {
                var customerName = customersById.TryGetValue(estimateObject.CustomerId, out var customer)
                    ? customer.Name
                    : string.Empty;

                return new OverviewObjectSummaryResponse(
                    estimateObject.Id,
                    estimateObject.CustomerId,
                    customerName,
                    estimateObject.Name,
                    estimateObject.ObjectType.ToString(),
                    estimateObject.Address,
                    estimateObject.TotalArea,
                    estimateObject.UpdatedAt);
            }).ToArray()));
    }

    public async Task<Result<PagedBusinessResponse<CustomerResponse>>> GetCustomersAsync(CustomerListQuery query, CancellationToken cancellationToken)
    {
        var records = await customers.ListAsync(query, cancellationToken);
        var count = await customers.CountAsync(query, cancellationToken);
        return Result<PagedBusinessResponse<CustomerResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<CustomerDetailsResponse>> GetCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDetailsResponse>.Failure(BusinessErrors.CustomerNotFound(id));
        }

        return Result<CustomerDetailsResponse>.Success(new CustomerDetailsResponse(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.Note,
            customer.IsArchived,
            customer.ArchivedAt,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.Version));
    }

    public async Task<Result<CustomerResponse>> UpdateCustomerAsync(Guid id, CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerResponse>.Failure(BusinessErrors.CustomerNotFound(id));
        }

        var validation = await customerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CustomerResponse>.Failure(BusinessErrors.Validation(validation));
        }

        customer.Update(request.Name, request.Phone, request.Email, request.Note, DateTimeOffset.UtcNow);
        await customers.SaveChangesAsync(cancellationToken);
        return Result<CustomerResponse>.Success(Map(customer));
    }

    public async Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var validation = await customerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CustomerResponse>.Failure(BusinessErrors.Validation(validation));
        }

        var customer = Customer.Create(Guid.NewGuid(), request.Name, request.Phone, request.Email, request.Note, DateTimeOffset.UtcNow);
        await customers.AddAsync(customer, cancellationToken);
        await customers.SaveChangesAsync(cancellationToken);
        return Result<CustomerResponse>.Success(Map(customer));
    }

    public async Task<Result<PagedBusinessResponse<EstimateObjectResponse>>> GetObjectsAsync(EstimateObjectListQuery query, CancellationToken cancellationToken)
    {
        var records = await objects.ListAsync(query, cancellationToken);
        var count = await objects.CountAsync(query, cancellationToken);
        return Result<PagedBusinessResponse<EstimateObjectResponse>>.Success(new(records.Select(Map).ToArray(), query.Page, query.PageSize, count));
    }

    public async Task<Result<EstimateObjectResponse>> CreateObjectAsync(CreateEstimateObjectRequest request, CancellationToken cancellationToken)
    {
        var validation = await objectValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.Validation(validation));
        }

        if (await customers.GetByIdAsync(request.CustomerId, cancellationToken) is null)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.CustomerNotFound(request.CustomerId));
        }

        var estimateObject = EstimateObject.Create(
            Guid.NewGuid(),
            request.CustomerId,
            request.Name,
            request.ObjectType,
            request.Address,
            request.TotalArea,
            request.Description,
            DateTimeOffset.UtcNow);

        await objects.AddAsync(estimateObject, cancellationToken);
        await objects.SaveChangesAsync(cancellationToken);
        return Result<EstimateObjectResponse>.Success(Map(estimateObject));
    }

    public async Task<Result<EstimateObjectResponse>> UpdateObjectAsync(Guid id, CreateEstimateObjectRequest request, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(id, cancellationToken);
        if (estimateObject is null)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.ObjectNotFound(id));
        }

        var validation = await objectValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.Validation(validation));
        }

        if (await customers.GetByIdAsync(request.CustomerId, cancellationToken) is null)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.CustomerNotFound(request.CustomerId));
        }

        estimateObject.Update(
            request.Name,
            request.ObjectType,
            request.Address,
            request.TotalArea,
            request.Description,
            DateTimeOffset.UtcNow);

        await objects.SaveChangesAsync(cancellationToken);
        return Result<EstimateObjectResponse>.Success(Map(estimateObject));
    }

    public async Task<Result<EstimateObjectDetailsResponse>> GetObjectAsync(Guid id, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(id, cancellationToken);
        if (estimateObject is null)
        {
            return Result<EstimateObjectDetailsResponse>.Failure(BusinessErrors.ObjectNotFound(id));
        }

        var customer = await customers.GetByIdAsync(estimateObject.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<EstimateObjectDetailsResponse>.Failure(BusinessErrors.CustomerNotFound(estimateObject.CustomerId));
        }

        var estimatePage = await estimates.GetPageAsync(new EstimateListQuery(1, 1, ObjectId: id), cancellationToken);
        return Result<EstimateObjectDetailsResponse>.Success(new EstimateObjectDetailsResponse(
            estimateObject.Id,
            Map(customer),
            estimateObject.Name,
            estimateObject.ObjectType.ToString(),
            estimateObject.Address,
            estimateObject.TotalArea,
            estimateObject.Description,
            estimateObject.IsArchived,
            estimateObject.ArchivedAt,
            estimateObject.CreatedAt,
            estimateObject.UpdatedAt,
            estimateObject.Version,
            estimatePage.TotalCount));
    }

    public async Task<Result<CustomerResponse>> ArchiveCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerResponse>.Failure(BusinessErrors.CustomerNotFound(id));
        }

        customer.Archive(DateTimeOffset.UtcNow);
        await customers.SaveChangesAsync(cancellationToken);
        return Result<CustomerResponse>.Success(Map(customer));
    }

    public async Task<Result<CustomerResponse>> RestoreCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerResponse>.Failure(BusinessErrors.CustomerNotFound(id));
        }

        customer.Restore(DateTimeOffset.UtcNow);
        await customers.SaveChangesAsync(cancellationToken);
        return Result<CustomerResponse>.Success(Map(customer));
    }

    public async Task<Result> DeleteCustomerPermanentlyAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(BusinessErrors.CustomerNotFound(id));
        }

        if (await objects.ExistsForCustomerAsync(id, cancellationToken))
        {
            return Result.Failure(BusinessErrors.CustomerHasObjects());
        }

        await customers.RemoveAsync(customer, cancellationToken);
        await customers.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<EstimateObjectResponse>> ArchiveObjectAsync(Guid id, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(id, cancellationToken);
        if (estimateObject is null)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.ObjectNotFound(id));
        }

        estimateObject.Archive(DateTimeOffset.UtcNow);
        await objects.SaveChangesAsync(cancellationToken);
        return Result<EstimateObjectResponse>.Success(Map(estimateObject));
    }

    public async Task<Result<EstimateObjectResponse>> RestoreObjectAsync(Guid id, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(id, cancellationToken);
        if (estimateObject is null)
        {
            return Result<EstimateObjectResponse>.Failure(BusinessErrors.ObjectNotFound(id));
        }

        estimateObject.Restore(DateTimeOffset.UtcNow);
        await objects.SaveChangesAsync(cancellationToken);
        return Result<EstimateObjectResponse>.Success(Map(estimateObject));
    }

    public async Task<Result> DeleteObjectPermanentlyAsync(Guid id, CancellationToken cancellationToken)
    {
        var estimateObject = await objects.GetByIdAsync(id, cancellationToken);
        if (estimateObject is null)
        {
            return Result.Failure(BusinessErrors.ObjectNotFound(id));
        }

        if (await estimates.ExistsForObjectAsync(id, cancellationToken))
        {
            return Result.Failure(BusinessErrors.ObjectHasEstimates());
        }

        await objects.RemoveAsync(estimateObject, cancellationToken);
        await objects.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public static CustomerResponse Map(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Phone,
        customer.Email,
        customer.Note,
        customer.IsArchived,
        customer.ArchivedAt,
        customer.CreatedAt,
        customer.UpdatedAt,
        customer.Version);

    public static EstimateObjectResponse Map(EstimateObject estimateObject) => new(
        estimateObject.Id,
        estimateObject.CustomerId,
        estimateObject.Name,
        estimateObject.ObjectType.ToString(),
        estimateObject.Address,
        estimateObject.TotalArea,
        estimateObject.Description,
        estimateObject.IsArchived,
        estimateObject.ArchivedAt,
        estimateObject.CreatedAt,
        estimateObject.UpdatedAt,
        estimateObject.Version);
}
