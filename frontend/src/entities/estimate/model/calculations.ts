import type { Estimate, EstimateLineItem } from '@/entities/estimate/model/types';

export interface EstimateLineItemDraft {
  notes: string;
  quantity: string;
  unitPrice: string;
}

export type EstimateLineItemDrafts = Record<string, EstimateLineItemDraft>;

const getFiniteNumber = (value: string | number) => {
  const number = typeof value === 'number' ? value : Number(value);

  return Number.isFinite(number) ? number : 0;
};

export function roundMonetaryAmount(value: number) {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

export function calculateLineItemTotal(quantity: string | number, unitPrice: string | number) {
  return roundMonetaryAmount(getFiniteNumber(quantity) * getFiniteNumber(unitPrice));
}

export function createEstimateLineItemDraft(item: EstimateLineItem): EstimateLineItemDraft {
  return {
    notes: item.notes ?? '',
    quantity: String(item.quantity),
    unitPrice: String(item.unitPrice),
  };
}

export function applyEstimateLineItemDrafts(
  estimate: Estimate,
  drafts: EstimateLineItemDrafts,
): Estimate {
  const applyDraft = (item: EstimateLineItem) => {
    const draft = drafts[item.id];

    if (!draft) {
      return item;
    }

    return {
      ...item,
      notes: draft.notes,
      quantity: getFiniteNumber(draft.quantity),
      total: calculateLineItemTotal(draft.quantity, draft.unitPrice),
      unitPrice: getFiniteNumber(draft.unitPrice),
    };
  };
  const workItems = estimate.workItems.map(applyDraft);
  const materialItems = estimate.materialItems.map(applyDraft);
  const totalLabor = roundMonetaryAmount(workItems.reduce((total, item) => total + item.total, 0));
  const totalMaterials = roundMonetaryAmount(
    materialItems.reduce((total, item) => total + item.total, 0),
  );
  const zones = estimate.zones.map((zone) => {
    const zoneLabor = roundMonetaryAmount(
      workItems.filter((item) => item.zoneId === zone.id).reduce((total, item) => total + item.total, 0),
    );
    const zoneMaterials = roundMonetaryAmount(
      materialItems
        .filter((item) => item.zoneId === zone.id)
        .reduce((total, item) => total + item.total, 0),
    );

    return {
      ...zone,
      grandTotal: roundMonetaryAmount(zoneLabor + zoneMaterials),
      totalLabor: zoneLabor,
      totalMaterials: zoneMaterials,
    };
  });

  return {
    ...estimate,
    grandTotal: roundMonetaryAmount(totalLabor + totalMaterials),
    materialItems,
    totalLabor,
    totalMaterials,
    workItems,
    zones,
  };
}
