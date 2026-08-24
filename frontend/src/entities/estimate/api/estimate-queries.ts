import { useQuery } from '@tanstack/react-query';

import { getEstimateDocumentTemplates } from '@/entities/estimate/api/estimate-document-api';
import {
  getEstimate,
  getEstimates,
  type EstimateListFilters,
} from '@/entities/estimate/api/estimate-api';
import type { Locale } from '@/shared/i18n/types';
import { useTranslation } from '@/shared/i18n/use-translation';

export const estimateQueryKeys = {
  all: ['estimates'] as const,
  documentTemplates: (locale: Locale) =>
    [...estimateQueryKeys.all, 'document-templates', locale] as const,
  detail: (id: string, locale: Locale) => [...estimateQueryKeys.all, 'detail', id, locale] as const,
  list: (filters?: EstimateListFilters) =>
    [
      ...estimateQueryKeys.all,
      'list',
      filters?.search ?? '',
      filters?.status ?? '',
      filters?.customerId ?? '',
      filters?.objectId ?? '',
    ] as const,
};

export function useEstimateQuery(id: string) {
  const { locale } = useTranslation();

  return useQuery({
    enabled: id.length > 0,
    queryFn: ({ signal }) => getEstimate(id, signal),
    queryKey: estimateQueryKeys.detail(id, locale),
  });
}

export function useEstimatesQuery(filters?: EstimateListFilters) {
  return useQuery({
    queryFn: ({ signal }) => getEstimates(signal, filters),
    queryKey: estimateQueryKeys.list(filters),
  });
}

export function useEstimateDocumentTemplatesQuery(locale: Locale) {
  return useQuery({
    queryFn: ({ signal }) => getEstimateDocumentTemplates(locale, signal),
    queryKey: estimateQueryKeys.documentTemplates(locale),
  });
}
