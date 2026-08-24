import { apiRequest } from '@/shared/api/api-client';

import type {
  BusinessRecordStatus,
  UpdateCustomerPayload,
  UpdateEstimateObjectPayload,
  CreateCustomerPayload,
  CreateEstimateObjectPayload,
  Customer,
  CustomerDetails,
  EstimateObject,
  EstimateObjectDetails,
  PagedBusinessResponse,
} from '@/entities/business/model/types';

const customersPath = '/v1/customers';
const objectsPath = '/v1/objects';

const buildQuery = (options: {
  customerId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  status?: BusinessRecordStatus;
} = {}) => {
  const query = new URLSearchParams({
    page: String(options.page ?? 1),
    pageSize: String(options.pageSize ?? 100),
    status: options.status ?? 'active',
  });

  if (options.search) {
    query.set('search', options.search);
  }
  if (options.customerId) {
    query.set('customerId', options.customerId);
  }

  return query.toString();
};

export function getCustomers(signal?: AbortSignal, search?: string, status: BusinessRecordStatus = 'active') {
  return apiRequest<PagedBusinessResponse<Customer>>(`${customersPath}?${buildQuery({ search, status })}`, {
    signal,
  });
}

export function getCustomer(id: string, signal?: AbortSignal) {
  return apiRequest<CustomerDetails>(`${customersPath}/${id}`, { signal });
}

export function createCustomer(payload: CreateCustomerPayload) {
  return apiRequest<Customer>(customersPath, {
    body: JSON.stringify(payload),
    method: 'POST',
  });
}

export function updateCustomer(id: string, payload: UpdateCustomerPayload) {
  return apiRequest<Customer>(`${customersPath}/${id}`, {
    body: JSON.stringify(payload),
    method: 'PUT',
  });
}

export function archiveCustomer(id: string) {
  return apiRequest<Customer>(`${customersPath}/${id}/archive`, {
    method: 'PATCH',
  });
}

export function restoreCustomer(id: string) {
  return apiRequest<Customer>(`${customersPath}/${id}/restore`, {
    method: 'PATCH',
  });
}

export function deleteCustomerPermanently(id: string) {
  return apiRequest<void>(`${customersPath}/${id}`, {
    method: 'DELETE',
  });
}

export function getObjects(
  signal?: AbortSignal,
  options?: { customerId?: string; search?: string; status?: BusinessRecordStatus },
) {
  return apiRequest<PagedBusinessResponse<EstimateObject>>(
    `${objectsPath}?${buildQuery(options)}`,
    { signal },
  );
}

export function getObject(id: string, signal?: AbortSignal) {
  return apiRequest<EstimateObjectDetails>(`${objectsPath}/${id}`, { signal });
}

export function createObject(payload: CreateEstimateObjectPayload) {
  return apiRequest<EstimateObject>(objectsPath, {
    body: JSON.stringify(payload),
    method: 'POST',
  });
}

export function updateObject(id: string, payload: UpdateEstimateObjectPayload) {
  return apiRequest<EstimateObject>(`${objectsPath}/${id}`, {
    body: JSON.stringify(payload),
    method: 'PUT',
  });
}

export function archiveObject(id: string) {
  return apiRequest<EstimateObject>(`${objectsPath}/${id}/archive`, {
    method: 'PATCH',
  });
}

export function restoreObject(id: string) {
  return apiRequest<EstimateObject>(`${objectsPath}/${id}/restore`, {
    method: 'PATCH',
  });
}

export function deleteObjectPermanently(id: string) {
  return apiRequest<void>(`${objectsPath}/${id}`, {
    method: 'DELETE',
  });
}
