import { apiRequest } from '@/shared/api/api-client';

import type { Overview } from '@/entities/overview/model/types';

export function getOverview(signal?: AbortSignal) {
  return apiRequest<Overview>('/v1/overview', { signal });
}
