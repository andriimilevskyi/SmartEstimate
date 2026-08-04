import { apiRequest } from '@/shared/api/api-client';

import type {
  CreateEstimatePayload,
  Estimate,
  EstimateList,
  EstimateSummary,
} from '@/entities/estimate/model/types';

const estimatesPath = '/v1/estimates';

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

export function getEstimate(id: string, signal?: AbortSignal) {
  return apiRequest<Estimate>(`${estimatesPath}/${id}`, { signal });
}

export async function getEstimates(signal?: AbortSignal): Promise<EstimateList> {
  const data = await apiRequest<EstimateList | EstimateSummary[]>(
    `${estimatesPath}?page=1&pageSize=20`,
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
