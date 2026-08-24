export type PriceTargetType = 'Material' | 'ConstructionWork';
export type PriceSourceType =
  | 'Manual'
  | 'Import'
  | 'SupplierIntegration'
  | 'MarketReference'
  | 'AiRecommendation';
export type PriceStatus = 'Active' | 'Archived';

export interface PriceSummary {
  archivedAt: string | null;
  amount: number;
  createdAt: string;
  currency: string;
  effectiveFrom: string;
  effectiveUntil: string | null;
  id: string;
  notes: string | null;
  regionCode: string | null;
  sourceType: PriceSourceType;
  status: PriceStatus;
  supplierId: string | null;
  supplierName: string | null;
  updatedAt: string;
  version: number;
}

export interface PricingCatalogItem {
  categoryId: string | null;
  categoryName: string | null;
  currentPrice: PriceSummary | null;
  name: string;
  targetId: string;
  targetType: PriceTargetType;
  unitId: string;
  unitSymbol: string;
}

export interface PricingCatalogResponse {
  items: PricingCatalogItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PriceWriteRequest {
  amount: number;
  currency: string;
  effectiveFrom: string;
  notes?: string | null;
  regionCode?: string | null;
  sourceType: PriceSourceType;
  supplierId?: string | null;
  supplierName?: string | null;
  targetId: string;
  targetType: PriceTargetType;
}

export interface ResolvedPrice {
  amount: number;
  currency: string;
  effectiveFrom: string;
  priceId: string;
  regionCode: string | null;
  sourceType: PriceSourceType;
  supplierId: string | null;
  supplierName: string | null;
}

export interface PriceHistoryEvent {
  amount: number;
  catalogPriceId: string;
  changeType: 'Created' | 'Updated' | 'Archived';
  changedAt: string;
  changedBy: string | null;
  currency: string;
  effectiveFrom: string;
  effectiveUntil: string | null;
  id: string;
  notes: string | null;
  priceStatus: PriceStatus;
  regionCode: string | null;
  sourceType: PriceSourceType;
  supplierId: string | null;
  supplierName: string | null;
}

export interface PriceHistoryResponse {
  events: PriceHistoryEvent[];
  prices: PriceSummary[];
}
