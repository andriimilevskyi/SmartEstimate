import { useMutation } from '@tanstack/react-query';

import {
  addEstimateItem,
  deleteEstimateItem,
  duplicateEstimateItem,
  updateEstimateItem,
} from '@/entities/estimate/api/estimate-item-api';
import { updateEstimateStatus } from '@/entities/estimate/api/estimate-api';
import {
  addEstimateZone,
  deleteEstimateZone,
  reorderEstimateZones,
  updateEstimateZone,
} from '@/entities/estimate/api/estimate-zone-api';
import { estimateQueryKeys } from '@/entities/estimate/api/estimate-queries';
import {
  calculateLineItemTotal,
  roundMonetaryAmount,
} from '@/entities/estimate/model/calculations';
import type {
  AddEstimateItemPayload,
  Estimate,
  EstimateItemKind,
  EstimateStatus,
  UpdateEstimateItemPayload,
} from '@/entities/estimate/model/types';
import { queryClient } from '@/shared/config/query-client';
import type { Locale } from '@/shared/i18n/types';
import { useTranslation } from '@/shared/i18n/use-translation';

interface UpdateEstimateItemVariables {
  itemId: string;
  payload: UpdateEstimateItemPayload;
}

interface UpdateEstimateZoneVariables {
  name: string;
  zoneId: string;
}

interface OptimisticEstimateContext {
  previousEstimate?: Estimate;
}

const updateEstimateItemInCache = (
  estimate: Estimate,
  kind: EstimateItemKind,
  itemId: string,
  payload: UpdateEstimateItemPayload,
): Estimate => {
  const updateItem = (item: (typeof estimate.workItems)[number]) =>
    item.id === itemId
      ? {
          ...item,
          ...payload,
          total: calculateLineItemTotal(payload.quantity, payload.unitPrice),
        }
      : item;
  const workItems = kind === 'work' ? estimate.workItems.map(updateItem) : estimate.workItems;
  const materialItems =
    kind === 'material' ? estimate.materialItems.map(updateItem) : estimate.materialItems;
  const totalLabor = roundMonetaryAmount(workItems.reduce((total, item) => total + item.total, 0));
  const totalMaterials = roundMonetaryAmount(
    materialItems.reduce((total, item) => total + item.total, 0),
  );

  return {
    ...estimate,
    grandTotal: roundMonetaryAmount(totalLabor + totalMaterials),
    materialItems,
    totalLabor,
    totalMaterials,
    workItems,
  };
};

const removeEstimateItemFromCache = (
  estimate: Estimate,
  kind: EstimateItemKind,
  itemId: string,
): Estimate => {
  const workItems =
    kind === 'work' ? estimate.workItems.filter((item) => item.id !== itemId) : estimate.workItems;
  const materialItems =
    kind === 'material'
      ? estimate.materialItems.filter((item) => item.id !== itemId)
      : estimate.materialItems;
  const totalLabor = roundMonetaryAmount(workItems.reduce((total, item) => total + item.total, 0));
  const totalMaterials = roundMonetaryAmount(
    materialItems.reduce((total, item) => total + item.total, 0),
  );

  return {
    ...estimate,
    grandTotal: roundMonetaryAmount(totalLabor + totalMaterials),
    materialItems,
    totalLabor,
    totalMaterials,
    workItems,
  };
};

const updateEstimateCache = (
  estimateId: string,
  locale: Locale,
  estimate: Estimate | undefined,
) => {
  if (estimate) {
    queryClient.setQueryData(estimateQueryKeys.detail(estimateId, locale), estimate);
  }

  void queryClient.invalidateQueries({ queryKey: [...estimateQueryKeys.all, 'list'] });
};

export function useAddEstimateItem(estimateId: string, kind: EstimateItemKind) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (payload: AddEstimateItemPayload) => addEstimateItem(estimateId, kind, payload),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useUpdateEstimateItem(estimateId: string, kind: EstimateItemKind) {
  const { locale } = useTranslation();

  return useMutation<Estimate, Error, UpdateEstimateItemVariables, OptimisticEstimateContext>({
    mutationFn: ({ itemId, payload }: UpdateEstimateItemVariables) =>
      updateEstimateItem(estimateId, kind, itemId, payload),
    onError: (_error, _variables, context) => {
      if (context?.previousEstimate) {
        queryClient.setQueryData(estimateQueryKeys.detail(estimateId, locale), context.previousEstimate);
      }
    },
    onMutate: async ({ itemId, payload }) => {
      await queryClient.cancelQueries({ queryKey: estimateQueryKeys.detail(estimateId, locale) });
      const previousEstimate = queryClient.getQueryData<Estimate>(
        estimateQueryKeys.detail(estimateId, locale),
      );

      if (previousEstimate) {
        queryClient.setQueryData(
          estimateQueryKeys.detail(estimateId, locale),
          updateEstimateItemInCache(previousEstimate, kind, itemId, payload),
        );
      }

      return { previousEstimate };
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.detail(estimateId, locale) });
      void queryClient.invalidateQueries({ queryKey: [...estimateQueryKeys.all, 'list'] });
    },
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useDeleteEstimateItem(estimateId: string, kind: EstimateItemKind) {
  const { locale } = useTranslation();

  return useMutation<void, Error, string, OptimisticEstimateContext>({
    mutationFn: (itemId: string) => deleteEstimateItem(estimateId, kind, itemId),
    onError: (_error, _itemId, context) => {
      if (context?.previousEstimate) {
        queryClient.setQueryData(estimateQueryKeys.detail(estimateId, locale), context.previousEstimate);
      }
    },
    onMutate: async (itemId) => {
      await queryClient.cancelQueries({ queryKey: estimateQueryKeys.detail(estimateId, locale) });
      const previousEstimate = queryClient.getQueryData<Estimate>(
        estimateQueryKeys.detail(estimateId, locale),
      );

      if (previousEstimate) {
        queryClient.setQueryData(
          estimateQueryKeys.detail(estimateId, locale),
          removeEstimateItemFromCache(previousEstimate, kind, itemId),
        );
      }

      return { previousEstimate };
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.detail(estimateId, locale) });
      void queryClient.invalidateQueries({ queryKey: [...estimateQueryKeys.all, 'list'] });
    },
  });
}

export function useDuplicateEstimateItem(estimateId: string, kind: EstimateItemKind) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (itemId: string) => duplicateEstimateItem(estimateId, kind, itemId),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useAddEstimateZone(estimateId: string) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (name: string) => addEstimateZone(estimateId, name),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useUpdateEstimateZone(estimateId: string) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: ({ name, zoneId }: UpdateEstimateZoneVariables) =>
      updateEstimateZone(estimateId, zoneId, name),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useReorderEstimateZones(estimateId: string) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (zoneIds: string[]) => reorderEstimateZones(estimateId, zoneIds),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useDeleteEstimateZone(estimateId: string) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (zoneId: string) => deleteEstimateZone(estimateId, zoneId),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}

export function useUpdateEstimateStatus(estimateId: string) {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (status: EstimateStatus) => updateEstimateStatus(estimateId, status),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, locale, estimate);
    },
  });
}
