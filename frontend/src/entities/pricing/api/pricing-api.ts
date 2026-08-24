import { apiRequest } from '@/shared/api/api-client';

import type {
  PriceHistoryResponse,
  PriceTargetType,
  PriceWriteRequest,
  PricingCatalogResponse,
  ResolvedPrice,
} from '@/entities/pricing/model/types';

const pricingPath = '/v1/pricing';

export interface PricingCatalogOptions {
  categoryId?: string;
  currency?: string;
  enabled?: boolean;
  missingOnly?: boolean;
  page?: number;
  pageSize?: number;
  regionCode?: string;
  search?: string;
  supplier?: string;
  targetType: PriceTargetType;
}

export function getPricingCatalog(options: PricingCatalogOptions, signal?: AbortSignal) {
  const query = new URLSearchParams(
    `targetType=${options.targetType}&page=${options.page ?? 1}&pageSize=${options.pageSize ?? 25}`,
  );

  if (options.search) query.set('search', options.search);
  if (options.categoryId) query.set('categoryId', options.categoryId);
  if (options.currency) query.set('currency', options.currency);
  if (options.supplier) query.set('supplier', options.supplier);
  if (options.regionCode) query.set('regionCode', options.regionCode);
  if (options.missingOnly) query.set('missingOnly', 'true');

  return apiRequest<PricingCatalogResponse>(`${pricingPath}/catalog?${query.toString()}`, {
    signal,
  });
}

export function createCatalogPrice(request: PriceWriteRequest) {
  return apiRequest(`${pricingPath}/prices`, {
    body: JSON.stringify(request),
    method: 'POST',
  });
}

export function updateCatalogPrice(priceId: string, request: PriceWriteRequest) {
  return apiRequest(`${pricingPath}/prices/${priceId}`, {
    body: JSON.stringify(request),
    method: 'PUT',
  });
}

export function archiveCatalogPrice(priceId: string) {
  return apiRequest<void>(`${pricingPath}/prices/${priceId}`, { method: 'DELETE' });
}

export function getPriceHistory(targetType: PriceTargetType, targetId: string, signal?: AbortSignal) {
  return apiRequest<PriceHistoryResponse>(`${pricingPath}/history/${targetType}/${targetId}`, {
    signal,
  });
}

export function resolveCatalogPrice(
  targetType: PriceTargetType,
  targetId: string,
  currency: string,
  signal?: AbortSignal,
) {
  const query = new URLSearchParams(`currency=${currency}`);
  return apiRequest<ResolvedPrice>(
    `${pricingPath}/resolve/${targetType}/${targetId}?${query.toString()}`,
    { signal },
  );
}
