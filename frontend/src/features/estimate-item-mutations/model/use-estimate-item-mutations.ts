import { useMutation } from '@tanstack/react-query';

import {
  addEstimateItem,
  deleteEstimateItem,
  duplicateEstimateItem,
  updateEstimateItem,
} from '@/entities/estimate/api/estimate-item-api';
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
  UpdateEstimateItemPayload,
} from '@/entities/estimate/model/types';
import { queryClient } from '@/shared/config/query-client';

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

const updateEstimateCache = (estimateId: string, estimate: Estimate | undefined) => {
  if (estimate) {
    queryClient.setQueryData(estimateQueryKeys.detail(estimateId), estimate);
  }

  void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.list() });
};

export function useAddEstimateItem(estimateId: string, kind: EstimateItemKind) {
  return useMutation({
    mutationFn: (payload: AddEstimateItemPayload) => addEstimateItem(estimateId, kind, payload),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useUpdateEstimateItem(estimateId: string, kind: EstimateItemKind) {
  return useMutation<Estimate, Error, UpdateEstimateItemVariables, OptimisticEstimateContext>({
    mutationFn: ({ itemId, payload }: UpdateEstimateItemVariables) =>
      updateEstimateItem(estimateId, kind, itemId, payload),
    onError: (_error, _variables, context) => {
      if (context?.previousEstimate) {
        queryClient.setQueryData(estimateQueryKeys.detail(estimateId), context.previousEstimate);
      }
    },
    onMutate: async ({ itemId, payload }) => {
      await queryClient.cancelQueries({ queryKey: estimateQueryKeys.detail(estimateId) });
      const previousEstimate = queryClient.getQueryData<Estimate>(
        estimateQueryKeys.detail(estimateId),
      );

      if (previousEstimate) {
        queryClient.setQueryData(
          estimateQueryKeys.detail(estimateId),
          updateEstimateItemInCache(previousEstimate, kind, itemId, payload),
        );
      }

      return { previousEstimate };
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.detail(estimateId) });
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.list() });
    },
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useDeleteEstimateItem(estimateId: string, kind: EstimateItemKind) {
  return useMutation<void, Error, string, OptimisticEstimateContext>({
    mutationFn: (itemId: string) => deleteEstimateItem(estimateId, kind, itemId),
    onError: (_error, _itemId, context) => {
      if (context?.previousEstimate) {
        queryClient.setQueryData(estimateQueryKeys.detail(estimateId), context.previousEstimate);
      }
    },
    onMutate: async (itemId) => {
      await queryClient.cancelQueries({ queryKey: estimateQueryKeys.detail(estimateId) });
      const previousEstimate = queryClient.getQueryData<Estimate>(
        estimateQueryKeys.detail(estimateId),
      );

      if (previousEstimate) {
        queryClient.setQueryData(
          estimateQueryKeys.detail(estimateId),
          removeEstimateItemFromCache(previousEstimate, kind, itemId),
        );
      }

      return { previousEstimate };
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.detail(estimateId) });
      void queryClient.invalidateQueries({ queryKey: estimateQueryKeys.list() });
    },
  });
}

export function useDuplicateEstimateItem(estimateId: string, kind: EstimateItemKind) {
  return useMutation({
    mutationFn: (itemId: string) => duplicateEstimateItem(estimateId, kind, itemId),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useAddEstimateZone(estimateId: string) {
  return useMutation({
    mutationFn: (name: string) => addEstimateZone(estimateId, name),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useUpdateEstimateZone(estimateId: string) {
  return useMutation({
    mutationFn: ({ name, zoneId }: UpdateEstimateZoneVariables) =>
      updateEstimateZone(estimateId, zoneId, name),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useReorderEstimateZones(estimateId: string) {
  return useMutation({
    mutationFn: (zoneIds: string[]) => reorderEstimateZones(estimateId, zoneIds),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}

export function useDeleteEstimateZone(estimateId: string) {
  return useMutation({
    mutationFn: (zoneId: string) => deleteEstimateZone(estimateId, zoneId),
    onSuccess: (estimate) => {
      updateEstimateCache(estimateId, estimate);
    },
  });
}
