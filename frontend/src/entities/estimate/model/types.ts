export type EstimateObjectType =
  | 'Apartment'
  | 'PrivateHouse'
  | 'CommercialSpace'
  | 'Office'
  | 'IndustrialSpace'
  | 'Other';

export interface EstimateZone {
  grandTotal: number;
  id: string;
  name: string;
  sortOrder: number;
  totalLabor: number;
  totalMaterials: number;
}

export interface EstimateLineItem {
  id: string;
  knowledgeItemId?: string | null;
  measurementUnit: string;
  name: string;
  notes: string | null;
  quantity: number;
  total: number;
  unitPrice: number;
  zoneId: string;
}

export interface EstimateSummary {
  createdAt: string;
  currency: string;
  estimateNumber: string;
  grandTotal: number;
  id: string;
  objectAddress: string | null;
  objectType: EstimateObjectType;
  totalLabor: number;
  totalMaterials: number;
  totalArea: number | null;
  updatedAt: string;
  version: number;
}

export interface Estimate extends EstimateSummary {
  materialItems: EstimateLineItem[];
  notes: string | null;
  workItems: EstimateLineItem[];
  zones: EstimateZone[];
}

export interface EstimateList {
  items: EstimateSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateEstimatePayload {
  currency: string;
  estimateNumber: string;
  materialItems?: CreateEstimateLineItem[];
  notes?: string;
  objectAddress?: string | null;
  objectType: EstimateObjectType;
  totalArea?: number | null;
  workItems?: CreateEstimateLineItem[];
  zones: string[];
}

export interface CreateEstimateLineItem {
  measurementUnit: string;
  name: string;
  notes?: string;
  quantity: number;
  unitPrice: number;
}

export type EstimateItemKind = 'work' | 'material';

export interface AddEstimateItemPayload {
  knowledgeItemId: string;
  notes?: string | null;
  quantity: number;
  unitPrice: number;
  zoneId: string;
}

export interface UpdateEstimateItemPayload {
  notes?: string | null;
  quantity: number;
  unitPrice: number;
}
