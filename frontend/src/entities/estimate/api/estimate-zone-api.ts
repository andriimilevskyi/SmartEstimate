import { apiRequest } from '@/shared/api/api-client';

import type { Estimate } from '@/entities/estimate/model/types';

const estimatePath = (estimateId: string) => `/v1/estimates/${estimateId}`;

export function addEstimateZone(estimateId: string, name: string) {
  return apiRequest<Estimate>(`${estimatePath(estimateId)}/zones`, {
    body: JSON.stringify({ name }),
    method: 'POST',
  });
}

export function updateEstimateZone(estimateId: string, zoneId: string, name: string) {
  return apiRequest<Estimate>(`${estimatePath(estimateId)}/zones/${zoneId}`, {
    body: JSON.stringify({ name }),
    method: 'PATCH',
  });
}

export function reorderEstimateZones(estimateId: string, zoneIds: string[]) {
  return apiRequest<Estimate>(`${estimatePath(estimateId)}/zones/reorder`, {
    body: JSON.stringify({ zoneIds }),
    method: 'POST',
  });
}

export function deleteEstimateZone(estimateId: string, zoneId: string) {
  return apiRequest<Estimate>(`${estimatePath(estimateId)}/zones/${zoneId}`, {
    method: 'DELETE',
  });
}
