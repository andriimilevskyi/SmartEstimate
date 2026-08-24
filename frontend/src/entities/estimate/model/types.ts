export type EstimateObjectType =
  | 'Apartment'
  | 'PrivateHouse'
  | 'CommercialSpace'
  | 'Office'
  | 'IndustrialSpace'
  | 'Other';

export type EstimateStatus =
  | 'Draft'
  | 'InProgress'
  | 'Sent'
  | 'Approved'
  | 'Completed'
  | 'Archived';

export interface EstimateBusinessContext {
  address: string | null;
  customerEmail: string | null;
  customerId: string;
  customerName: string;
  customerNote: string | null;
  customerPhone: string | null;
  description: string | null;
  id: string;
  name: string;
  objectType: EstimateObjectType;
  totalArea: number | null;
}

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
  isUnitPriceManuallyOverridden: boolean;
  knowledgeItemId?: string | null;
  measurementUnit: string;
  name: string;
  notes: string | null;
  nameSource: 'KnowledgeSnapshot' | 'Custom' | 'Legacy';
  priceCapturedAt: string | null;
  quantity: number;
  sourcePriceId: string | null;
  total: number;
  unitPrice: number;
  zoneId: string;
}

export interface EstimateSummary {
  createdAt: string;
  currency: string;
  deletedAt: string | null;
  estimateNumber: string;
  grandTotal: number;
  id: string;
  isDeleted: boolean;
  object: EstimateBusinessContext;
  status: EstimateStatus;
  totalLabor: number;
  totalMaterials: number;
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

export interface EstimateDocumentTemplate {
  code: string;
  description: string;
  format: 'Pdf';
  name: string;
  template: 'FullEstimate' | 'ShortEstimate' | 'CommercialProposal';
}

export interface CreateEstimatePayload {
  currency: string;
  estimateNumber: string;
  materialItems?: CreateEstimateLineItem[];
  notes?: string;
  objectId: string;
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
  unitPrice?: number | null;
  zoneId: string;
}

export interface UpdateEstimateItemPayload {
  notes?: string | null;
  quantity: number;
  unitPrice: number;
}
