import { useMutation } from '@tanstack/react-query';

import { deleteEstimate } from '@/entities/estimate/api/estimate-api';
import { estimateQueryKeys } from '@/entities/estimate/api/estimate-queries';
import { queryClient } from '@/shared/config/query-client';

export function useDeleteEstimate() {
  return useMutation({
    mutationFn: deleteEstimate,
    onSuccess: async (_, id) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: estimateQueryKeys.all }),
        queryClient.invalidateQueries({ queryKey: [...estimateQueryKeys.all, 'detail', id] }),
      ]);
    },
  });
}
