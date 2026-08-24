import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import {
  archiveCatalogPrice,
  createCatalogPrice,
  getPriceHistory,
  getPricingCatalog,
  resolveCatalogPrice,
  updateCatalogPrice,
  type PricingCatalogOptions,
} from '@/entities/pricing/api/pricing-api';
import type { PriceTargetType, PriceWriteRequest } from '@/entities/pricing/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import type { Locale } from '@/shared/i18n/types';

export const pricingQueryKeys = {
  all: ['pricing'] as const,
  catalog: (options: PricingCatalogOptions, locale: Locale) =>
    [...pricingQueryKeys.all, 'catalog', locale, options] as const,
  history: (targetType: PriceTargetType, targetId: string) =>
    [...pricingQueryKeys.all, 'history', targetType, targetId] as const,
  resolve: (targetType: PriceTargetType, targetId: string, currency: string) =>
    [...pricingQueryKeys.all, 'resolve', targetType, targetId, currency] as const,
};

export function usePricingCatalogQuery(options: PricingCatalogOptions) {
  const { locale } = useTranslation();

  return useQuery({
    enabled: options.enabled ?? true,
    queryFn: ({ signal }) => getPricingCatalog(options, signal),
    queryKey: pricingQueryKeys.catalog(options, locale),
  });
}

export function usePriceHistoryQuery(
  targetType: PriceTargetType,
  targetId: string,
  enabled = true,
) {
  return useQuery({
    enabled: enabled && targetId.length > 0,
    queryFn: ({ signal }) => getPriceHistory(targetType, targetId, signal),
    queryKey: pricingQueryKeys.history(targetType, targetId),
  });
}

export function useResolvePriceQuery(
  targetType: PriceTargetType,
  targetId: string,
  currency: string,
  enabled = true,
) {
  return useQuery({
    enabled: enabled && targetId.length > 0 && currency.length === 3,
    queryFn: ({ signal }) => resolveCatalogPrice(targetType, targetId, currency, signal),
    queryKey: pricingQueryKeys.resolve(targetType, targetId, currency),
    retry: false,
  });
}

export function useCreatePriceMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: PriceWriteRequest) => createCatalogPrice(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: pricingQueryKeys.all });
    },
  });
}

export function useUpdatePriceMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ priceId, request }: { priceId: string; request: PriceWriteRequest }) =>
      updateCatalogPrice(priceId, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: pricingQueryKeys.all });
    },
  });
}

export function useArchivePriceMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (priceId: string) => archiveCatalogPrice(priceId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: pricingQueryKeys.all });
    },
  });
}
