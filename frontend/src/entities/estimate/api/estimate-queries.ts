import { useQuery } from '@tanstack/react-query';

import { getEstimate, getEstimates } from '@/entities/estimate/api/estimate-api';

export const estimateQueryKeys = {
  all: ['estimates'] as const,
  detail: (id: string) => [...estimateQueryKeys.all, 'detail', id] as const,
  list: () => [...estimateQueryKeys.all, 'list'] as const,
};

export function useEstimateQuery(id: string) {
  return useQuery({
    enabled: id.length > 0,
    queryFn: ({ signal }) => getEstimate(id, signal),
    queryKey: estimateQueryKeys.detail(id),
  });
}

export function useEstimatesQuery() {
  return useQuery({
    queryFn: ({ signal }) => getEstimates(signal),
    queryKey: estimateQueryKeys.list(),
  });
}
