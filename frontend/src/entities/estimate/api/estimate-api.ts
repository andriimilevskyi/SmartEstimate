import { apiRequest } from '@/shared/api/api-client';

import type {
  CreateEstimatePayload,
  Estimate,
  EstimateList,
  EstimateStatus,
  EstimateSummary,
} from '@/entities/estimate/model/types';

const estimatesPath = '/v1/estimates';

export interface EstimateListFilters {
  customerId?: string;
  objectId?: string;
  search?: string;
  status?: EstimateStatus;
}

export function createEstimate(payload: CreateEstimatePayload) {
  return apiRequest<Estimate>(estimatesPath, {
    body: JSON.stringify(payload),
    method: 'POST',
  });
}

export function deleteEstimate(id: string) {
  return apiRequest<void>(`${estimatesPath}/${id}`, {
    method: 'DELETE',
  });
}

export function deleteEstimatePermanently(id: string) {
  return apiRequest<void>(`${estimatesPath}/${id}/permanent`, {
    method: 'DELETE',
  });
}

export function getEstimate(id: string, signal?: AbortSignal) {
  return apiRequest<Estimate>(`${estimatesPath}/${id}`, { signal });
}

export function updateEstimateStatus(id: string, status: EstimateStatus) {
  return apiRequest<Estimate>(`${estimatesPath}/${id}/status`, {
    body: JSON.stringify({ status }),
    method: 'PATCH',
  });
}

export async function getEstimates(
  signal?: AbortSignal,
  filters: EstimateListFilters = {},
): Promise<EstimateList> {
  const query = new URLSearchParams({ page: '1', pageSize: '20' });
  if (filters.search) {
    query.set('search', filters.search);
  }
  if (filters.status) {
    query.set('status', filters.status);
  }
  if (filters.customerId) {
    query.set('customerId', filters.customerId);
  }
  if (filters.objectId) {
    query.set('objectId', filters.objectId);
  }

  const data = await apiRequest<EstimateList | EstimateSummary[]>(
    `${estimatesPath}?${query}`,
    { signal },
  );

  if (Array.isArray(data)) {
    return {
      items: data,
      page: 1,
      pageSize: data.length,
      totalCount: data.length,
    };
  }

  return data;
}
