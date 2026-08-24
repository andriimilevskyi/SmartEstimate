import { useQuery } from '@tanstack/react-query';

import { getOverview } from '@/entities/overview/api/overview-api';

export const overviewQueryKeys = {
  all: ['overview'] as const,
};

export function useOverviewQuery() {
  return useQuery({
    queryFn: ({ signal }) => getOverview(signal),
    queryKey: overviewQueryKeys.all,
  });
}
