import type { EstimateObjectType } from '@/entities/estimate/model/types';
import type { EstimateSummary } from '@/entities/estimate/model/types';

export interface OverviewEstimateCounts {
  approved: number;
  completed: number;
  draft: number;
  inProgress: number;
  sent: number;
  total: number;
}

export interface OverviewObjectSummary {
  address: string | null;
  customerId: string;
  customerName: string;
  id: string;
  name: string;
  objectType: EstimateObjectType;
  totalArea: number | null;
  updatedAt: string;
}

export interface Overview {
  estimates: OverviewEstimateCounts;
  recentEstimates: EstimateSummary[];
  recentObjects: OverviewObjectSummary[];
}
