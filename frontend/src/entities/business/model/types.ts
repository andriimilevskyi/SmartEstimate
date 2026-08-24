import type { EstimateObjectType } from '@/entities/estimate/model/types';

export type BusinessRecordStatus = 'active' | 'archived' | 'all';

export interface Customer {
  archivedAt: string | null;
  createdAt: string;
  email: string | null;
  id: string;
  isArchived: boolean;
  name: string;
  note: string | null;
  phone: string | null;
  updatedAt: string;
  version: number;
}

export type CustomerDetails = Customer;

export interface EstimateObject {
  address: string | null;
  archivedAt: string | null;
  createdAt: string;
  customerId: string;
  description: string | null;
  id: string;
  isArchived: boolean;
  name: string;
  objectType: EstimateObjectType;
  totalArea: number | null;
  updatedAt: string;
  version: number;
}

export interface EstimateObjectDetails extends EstimateObject {
  customer: Customer;
  estimateCount: number;
}

export interface PagedBusinessResponse<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateCustomerPayload {
  email?: string | null;
  name: string;
  note?: string | null;
  phone?: string | null;
}

export type UpdateCustomerPayload = CreateCustomerPayload;

export interface CreateEstimateObjectPayload {
  address?: string | null;
  customerId: string;
  description?: string | null;
  name: string;
  objectType: EstimateObjectType;
  totalArea?: number | null;
}

export type UpdateEstimateObjectPayload = CreateEstimateObjectPayload;
