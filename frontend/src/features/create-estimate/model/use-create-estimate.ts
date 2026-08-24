import { useMutation } from '@tanstack/react-query';

import { createEstimate } from '@/entities/estimate/api/estimate-api';
import { estimateQueryKeys } from '@/entities/estimate/api/estimate-queries';
import type { CreateEstimatePayload } from '@/entities/estimate/model/types';
import { queryClient } from '@/shared/config/query-client';
import { useTranslation } from '@/shared/i18n/use-translation';

export function useCreateEstimate() {
  const { locale } = useTranslation();

  return useMutation({
    mutationFn: (payload: CreateEstimatePayload) => createEstimate(payload),
    onSuccess: async (estimate) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: [...estimateQueryKeys.all, 'list'] }),
        queryClient.setQueryData(estimateQueryKeys.detail(estimate.id, locale), estimate),
      ]);
    },
  });
}
