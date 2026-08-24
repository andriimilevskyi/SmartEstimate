import { useMutation } from '@tanstack/react-query';

import { deleteEstimatePermanently } from '@/entities/estimate/api/estimate-api';
import { estimateQueryKeys } from '@/entities/estimate/api/estimate-queries';
import { queryClient } from '@/shared/config/query-client';

export function useDeleteEstimatePermanently() {
  return useMutation({
    mutationFn: deleteEstimatePermanently,
    onSuccess: async (_, id) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: estimateQueryKeys.all }),
        queryClient.removeQueries({ queryKey: [...estimateQueryKeys.all, 'detail', id] }),
      ]);
    },
  });
}
