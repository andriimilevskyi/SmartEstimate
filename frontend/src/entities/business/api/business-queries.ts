import { useQuery } from '@tanstack/react-query';

import { getCustomer, getCustomers, getObject, getObjects } from '@/entities/business/api/business-api';
import type { BusinessRecordStatus } from '@/entities/business/model/types';

export const businessQueryKeys = {
  all: ['business'] as const,
  customer: (id: string) => [...businessQueryKeys.all, 'customer', id] as const,
  customers: (search?: string, status: BusinessRecordStatus = 'active') =>
    [...businessQueryKeys.all, 'customers', search ?? '', status] as const,
  object: (id: string) => [...businessQueryKeys.all, 'object', id] as const,
  objects: (customerId?: string, search?: string, status: BusinessRecordStatus = 'active') =>
    [...businessQueryKeys.all, 'objects', customerId ?? 'all', search ?? '', status] as const,
};

export function useCustomersQuery(search?: string, status: BusinessRecordStatus = 'active') {
  return useQuery({
    queryFn: ({ signal }) => getCustomers(signal, search, status),
    queryKey: businessQueryKeys.customers(search, status),
  });
}

export function useCustomerQuery(id: string) {
  return useQuery({
    enabled: id.length > 0,
    queryFn: ({ signal }) => getCustomer(id, signal),
    queryKey: businessQueryKeys.customer(id),
  });
}

export function useObjectsQuery(customerId?: string, search?: string, status: BusinessRecordStatus = 'active') {
  return useQuery({
    queryFn: ({ signal }) => getObjects(signal, { customerId, search, status }),
    queryKey: businessQueryKeys.objects(customerId, search, status),
  });
}

export function useObjectQuery(id: string) {
  return useQuery({
    enabled: id.length > 0,
    queryFn: ({ signal }) => getObject(id, signal),
    queryKey: businessQueryKeys.object(id),
  });
}
