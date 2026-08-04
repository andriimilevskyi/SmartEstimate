import { apiRequest } from '@/shared/api/api-client';

import type {
  AddEstimateItemPayload,
  Estimate,
  EstimateItemKind,
  UpdateEstimateItemPayload,
} from '@/entities/estimate/model/types';

const getItemPath = (estimateId: string, kind: EstimateItemKind) =>
  `/v1/estimates/${estimateId}/${kind === 'work' ? 'work-items' : 'material-items'}`;

export function addEstimateItem(
  estimateId: string,
  kind: EstimateItemKind,
  payload: AddEstimateItemPayload,
) {
  const body =
    kind === 'work'
      ? {
          constructionWorkId: payload.knowledgeItemId,
          notes: payload.notes,
          quantity: payload.quantity,
          unitPrice: payload.unitPrice,
          zoneId: payload.zoneId,
        }
      : {
          materialId: payload.knowledgeItemId,
          notes: payload.notes,
          quantity: payload.quantity,
          unitPrice: payload.unitPrice,
          zoneId: payload.zoneId,
        };

  return apiRequest<Estimate>(getItemPath(estimateId, kind), {
    body: JSON.stringify(body),
    method: 'POST',
  });
}

export function deleteEstimateItem(estimateId: string, kind: EstimateItemKind, itemId: string) {
  return apiRequest<void>(`${getItemPath(estimateId, kind)}/${itemId}`, {
    method: 'DELETE',
  });
}

export function duplicateEstimateItem(estimateId: string, kind: EstimateItemKind, itemId: string) {
  return apiRequest<Estimate>(`${getItemPath(estimateId, kind)}/${itemId}/duplicate`, {
    method: 'POST',
  });
}

export function updateEstimateItem(
  estimateId: string,
  kind: EstimateItemKind,
  itemId: string,
  payload: UpdateEstimateItemPayload,
) {
  return apiRequest<Estimate>(`${getItemPath(estimateId, kind)}/${itemId}`, {
    body: JSON.stringify(payload),
    method: 'PATCH',
  });
}
