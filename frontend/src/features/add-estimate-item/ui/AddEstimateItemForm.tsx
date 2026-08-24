import {
  Check,
  ChevronsUpDown,
  LoaderCircle,
  PackagePlus,
  Plus,
  Search,
  Sparkles,
  Wrench,
  X,
} from 'lucide-react';
import type { KeyboardEvent } from 'react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'sonner';

import { calculateLineItemTotal } from '@/entities/estimate/model/calculations';
import type { EstimateItemKind } from '@/entities/estimate/model/types';
import {
  useConstructionWorksQuery,
  useKnowledgeCategoriesQuery,
  useKnowledgeMaterialQuery,
  useKnowledgeMaterialsQuery,
  useKnowledgeUnitsQuery,
} from '@/entities/knowledge/api/knowledge-queries';
import { getLocalizedText } from '@/entities/knowledge/model/localization';
import type { ConstructionWork, KnowledgeMaterial } from '@/entities/knowledge/model/types';
import { usePricingCatalogQuery } from '@/entities/pricing/api/pricing-queries';
import type { PriceSummary, PricingCatalogItem } from '@/entities/pricing/model/types';
import { useAddEstimateItem } from '@/features/estimate-item-mutations/model/use-estimate-item-mutations';
import { useTranslation } from '@/shared/i18n/use-translation';
import type { Locale } from '@/shared/i18n/types';
import { formatMoney } from '@/shared/lib/formatters';
import { cn } from '@/shared/lib/utils';
import { Button } from '@/shared/ui/button';

interface AddEstimateItemFormProps {
  currency: string;
  estimateId: string;
  kind: EstimateItemKind;
  zoneId: string;
  zoneName: string;
}

type CatalogRecord = ConstructionWork | KnowledgeMaterial;

const pageSize = 100;

const fieldClassName =
  'h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const numberFieldClassName =
  'h-9 w-full rounded-md border border-input bg-background px-2 text-sm tabular-nums outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const getSearchableText = (item: CatalogRecord) =>
  [
    item.name.uk,
    item.name.en,
    item.name.de,
    item.description,
    ...item.tags,
  ]
    .filter(Boolean)
    .join(' ')
    .toLocaleLowerCase();

const zoneRecommendationKeywords = (zoneName: string) => {
  const normalized = zoneName.toLocaleLowerCase();

  if (/(ван|bath|bad|toilet|туал|wc|сан)/i.test(normalized)) {
    return [
      'гідро',
      'ізоляц',
      'плит',
      'клей',
      'затир',
      'ґрунт',
      'грунт',
      'waterproof',
      'tile',
      'adhesive',
      'grout',
      'primer',
    ];
  }

  if (/(кух|kitchen|küche)/i.test(normalized)) {
    return ['плит', 'клей', 'затир', 'ґрунт', 'грунт', 'фарб', 'розет', 'tile', 'grout', 'paint', 'socket'];
  }

  if (/(спаль|віталь|living|bedroom|zimmer|hall|передп)/i.test(normalized)) {
    return ['шпак', 'фарб', 'стеля', 'підлог', 'ламін', 'paint', 'putty', 'ceiling', 'floor', 'laminate'];
  }

  return [];
};

const getPriceLabel = (
  price: PriceSummary | null | undefined,
  currency: string,
  locale: Locale,
) => (price ? formatMoney(price.amount, price.currency, locale) : '—');

export function AddEstimateItemForm({
  currency,
  estimateId,
  kind,
  zoneId,
  zoneName,
}: AddEstimateItemFormProps) {
  const { locale, t } = useTranslation();
  const [searchParams] = useSearchParams();
  const preselectedMaterialId = searchParams.get('materialId') ?? '';
  const [isOpen, setIsOpen] = useState(false);
  const [categoryId, setCategoryId] = useState('');
  const [search, setSearch] = useState('');
  const [selectedItemId, setSelectedItemId] = useState('');
  const [activeIndex, setActiveIndex] = useState(0);
  const [quantity, setQuantity] = useState('1');
  const [manualUnitPrice, setManualUnitPrice] = useState('');
  const [isBatchMode, setIsBatchMode] = useState(false);
  const [selectedBatchIds, setSelectedBatchIds] = useState<string[]>([]);
  const searchRef = useRef<HTMLInputElement>(null);
  const quantityRef = useRef<HTMLInputElement>(null);
  const categoriesQuery = useKnowledgeCategoriesQuery(kind === 'work' && isOpen);
  const worksQuery = useConstructionWorksQuery(
    categoryId || undefined,
    kind === 'work' && isOpen,
    search,
  );
  const materialsQuery = useKnowledgeMaterialsQuery(
    categoryId || undefined,
    kind === 'material' && isOpen,
    search,
  );
  const preselectedMaterialQuery = useKnowledgeMaterialQuery(
    preselectedMaterialId,
    kind === 'material' && preselectedMaterialId.length > 0,
  );
  const unitsQuery = useKnowledgeUnitsQuery();
  const pricingQuery = usePricingCatalogQuery({
    categoryId: categoryId || undefined,
    currency,
    enabled: isOpen,
    pageSize,
    search: search || undefined,
    targetType: kind === 'work' ? 'ConstructionWork' : 'Material',
  });
  const addMutation = useAddEstimateItem(estimateId, kind);

  const catalogItems = useMemo<CatalogRecord[]>(() => {
    if (kind === 'work') {
      return worksQuery.data?.items ?? [];
    }

    const items = materialsQuery.data?.items ?? [];
    if (
      preselectedMaterialQuery.data &&
      !items.some((item) => item.id === preselectedMaterialQuery.data.id)
    ) {
      return [preselectedMaterialQuery.data, ...items];
    }

    return items;
  }, [kind, materialsQuery.data?.items, preselectedMaterialQuery.data, worksQuery.data?.items]);

  const pricesByTarget = useMemo(
    () =>
      new Map(
        (pricingQuery.data?.items ?? []).map((item: PricingCatalogItem) => [
          item.targetId,
          item.currentPrice,
        ]),
      ),
    [pricingQuery.data?.items],
  );
  const unitsById = useMemo(
    () => new Map((unitsQuery.data?.items ?? []).map((unit) => [unit.id, unit])),
    [unitsQuery.data?.items],
  );
  const selectedItem = catalogItems.find((item) => item.id === selectedItemId);
  const selectedPrice = selectedItem ? pricesByTarget.get(selectedItem.id) : null;
  const selectedUnit = selectedItem ? unitsById.get(selectedItem.unitId) : undefined;
  const visibleItems = catalogItems.slice(0, 12);
  const recommendedItems = useMemo(() => {
    const keywords = zoneRecommendationKeywords(zoneName);
    if (keywords.length === 0) {
      return [];
    }

    return catalogItems
      .filter((item) => {
        const text = getSearchableText(item);
        return keywords.some((keyword) => text.includes(keyword));
      })
      .slice(0, 5);
  }, [catalogItems, zoneName]);
  const hasManualPrice = manualUnitPrice.trim().length > 0;
  const manualPriceNumber = Number(manualUnitPrice);
  const effectiveUnitPrice =
    hasManualPrice && Number.isFinite(manualPriceNumber)
      ? manualPriceNumber
      : selectedPrice?.amount ?? 0;
  const quantityNumber = Number(quantity);
  const canAdd =
    Boolean(selectedItem) &&
    Number.isFinite(quantityNumber) &&
    quantityNumber > 0 &&
    (!hasManualPrice || (Number.isFinite(manualPriceNumber) && manualPriceNumber >= 0));
  const catalogIsPending =
    unitsQuery.isPending || pricingQuery.isPending || (kind === 'work' ? worksQuery.isPending : materialsQuery.isPending);
  const catalogHasError =
    unitsQuery.isError || pricingQuery.isError || (kind === 'work' ? worksQuery.isError : materialsQuery.isError);
  const title = kind === 'work'
    ? t('estimateEditor.catalog.addWork')
    : t('estimateEditor.catalog.addMaterial');

  useEffect(() => {
    if (kind !== 'material' || preselectedMaterialId.length === 0) {
      return;
    }

    setIsOpen(true);
    setSelectedItemId(preselectedMaterialId);
  }, [kind, preselectedMaterialId]);

  useEffect(() => {
    setActiveIndex(0);
  }, [catalogItems]);

  const resetAfterAdd = () => {
    setSelectedItemId('');
    setManualUnitPrice('');
    setQuantity('1');
    setSelectedBatchIds([]);
    requestAnimationFrame(() => searchRef.current?.focus());
  };

  const addItem = async (itemId = selectedItemId, overrideQuantity = quantity) => {
    const item = catalogItems.find((catalogItem) => catalogItem.id === itemId);
    const nextQuantity = Number(overrideQuantity);
    const nextManualPrice = Number(manualUnitPrice);
    const nextHasManualPrice = itemId === selectedItemId && manualUnitPrice.trim().length > 0;

    if (
      !item ||
      !Number.isFinite(nextQuantity) ||
      nextQuantity <= 0 ||
      (nextHasManualPrice && (!Number.isFinite(nextManualPrice) || nextManualPrice < 0))
    ) {
      toast.error(t('estimateEditor.messages.addValidationError'));
      return;
    }

    await addMutation.mutateAsync({
      knowledgeItemId: item.id,
      notes: null,
      quantity: nextQuantity,
      unitPrice: nextHasManualPrice ? nextManualPrice : null,
      zoneId,
    });
    toast.success(t('estimateEditor.messages.added'));
    resetAfterAdd();
  };

  const addBatch = async () => {
    if (selectedBatchIds.length === 0) {
      return;
    }

    try {
      for (const itemId of selectedBatchIds) {
        await addMutation.mutateAsync({
          knowledgeItemId: itemId,
          notes: null,
          quantity: 1,
          unitPrice: null,
          zoneId,
        });
      }
      toast.success(t('estimateEditor.messages.batchAdded', { count: selectedBatchIds.length }));
      resetAfterAdd();
      setIsBatchMode(false);
    } catch {
      toast.error(t('estimateEditor.messages.addError'));
    }
  };

  const selectItem = (itemId: string) => {
    setSelectedItemId(itemId);
    setManualUnitPrice('');
    requestAnimationFrame(() => quantityRef.current?.focus());
  };

  const toggleBatchItem = (itemId: string) => {
    setSelectedBatchIds((current) =>
      current.includes(itemId)
        ? current.filter((currentId) => currentId !== itemId)
        : [...current, itemId],
    );
  };

  const handleSearchKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (visibleItems.length === 0) {
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      setActiveIndex((current) => Math.min(current + 1, visibleItems.length - 1));
    }

    if (event.key === 'ArrowUp') {
      event.preventDefault();
      setActiveIndex((current) => Math.max(current - 1, 0));
    }

    if (event.key === 'Enter') {
      event.preventDefault();
      selectItem(visibleItems[activeIndex]?.id ?? visibleItems[0].id);
    }
  };

  if (!isOpen) {
    return (
      <Button onClick={() => setIsOpen(true)} size="sm" type="button" variant="outline">
        <Plus aria-hidden="true" className="size-4" />
        {title}
      </Button>
    );
  }

  return (
    <div className="space-y-3 rounded-lg border border-border bg-background p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2 text-sm font-semibold">
          {kind === 'work' ? (
            <Wrench aria-hidden="true" className="size-4 text-muted-foreground" />
          ) : (
            <PackagePlus aria-hidden="true" className="size-4 text-muted-foreground" />
          )}
          {title}
        </div>
        <div className="flex items-center gap-1">
          <Button
            aria-pressed={isBatchMode}
            onClick={() => {
              setIsBatchMode((current) => !current);
              setSelectedBatchIds([]);
            }}
            size="sm"
            type="button"
            variant={isBatchMode ? 'secondary' : 'ghost'}
          >
            <Check aria-hidden="true" className="size-4" />
            {t('estimateEditor.catalog.addMultiple')}
          </Button>
          <Button onClick={() => setIsOpen(false)} size="sm" type="button" variant="ghost">
            <X aria-hidden="true" className="size-4" />
            {t('estimateEditor.catalog.hide')}
          </Button>
        </div>
      </div>

      <div className="grid gap-2 lg:grid-cols-[minmax(13rem,1fr)_minmax(10rem,0.55fr)]">
        <label className="relative">
          <Search
            aria-hidden="true"
            className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
          />
          <span className="sr-only">{t('estimateEditor.catalog.search')}</span>
          <input
            className={`${fieldClassName} pl-9`}
            onChange={(event) => setSearch(event.target.value)}
            onKeyDown={handleSearchKeyDown}
            placeholder={t('estimateEditor.catalog.searchPlaceholder')}
            ref={searchRef}
            value={search}
          />
        </label>
        <select
          aria-label={t('estimateEditor.catalog.category')}
          className={fieldClassName}
          disabled={catalogIsPending}
          onChange={(event) => setCategoryId(event.target.value)}
          value={categoryId}
        >
          <option value="">{t('estimateEditor.catalog.allCategories')}</option>
          {(categoriesQuery.data?.items ?? []).map((category) => (
            <option key={category.id} value={category.id}>
              {getLocalizedText(category.name, locale)}
            </option>
          ))}
        </select>
      </div>

      {recommendedItems.length > 0 ? (
        <div className="flex flex-wrap items-center gap-2">
          <span className="inline-flex items-center gap-1 text-xs font-medium text-muted-foreground">
            <Sparkles aria-hidden="true" className="size-3.5" />
            {t('estimateEditor.catalog.recommended')}
          </span>
          {recommendedItems.map((item) => (
            <button
              className="rounded-md border border-border px-2 py-1 text-xs transition-colors hover:border-primary/50 hover:bg-accent"
              key={item.id}
              onClick={() => selectItem(item.id)}
              type="button"
            >
              {getLocalizedText(item.name, locale)}
            </button>
          ))}
        </div>
      ) : null}

      <div className="overflow-hidden rounded-md border border-border">
        <div className="grid grid-cols-[minmax(11rem,1fr)_5rem_7rem_5.5rem] gap-2 border-b border-border bg-muted/60 px-3 py-2 text-xs font-medium text-muted-foreground">
          <span>{kind === 'work' ? t('estimateEditor.catalog.constructionWork') : t('estimateEditor.catalog.material')}</span>
          <span>{t('estimateEditor.catalog.unit')}</span>
          <span>{t('estimateEditor.item.unitPrice')}</span>
          <span className="text-right">{t('estimateEditor.catalog.add')}</span>
        </div>

        {catalogIsPending ? (
          <div className="flex min-h-28 items-center justify-center gap-2 text-sm text-muted-foreground">
            <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
            {t('estimateEditor.catalog.loading')}
          </div>
        ) : null}

        {catalogHasError ? (
          <p className="px-3 py-6 text-sm text-destructive">{t('estimateEditor.catalog.error')}</p>
        ) : null}

        {!catalogIsPending && !catalogHasError && visibleItems.length === 0 ? (
          <p className="px-3 py-6 text-sm text-muted-foreground">{t('estimateEditor.catalog.empty')}</p>
        ) : null}

        {!catalogIsPending && !catalogHasError && visibleItems.length > 0 ? (
          <ul className="max-h-72 overflow-auto divide-y divide-border">
            {visibleItems.map((item, index) => {
              const price = pricesByTarget.get(item.id);
              const unit = unitsById.get(item.unitId);
              const isSelected = selectedItemId === item.id;
              const isBatchSelected = selectedBatchIds.includes(item.id);

              return (
                <li key={item.id}>
                  <button
                    className={cn(
                      'grid w-full grid-cols-[minmax(11rem,1fr)_5rem_7rem_5.5rem] gap-2 px-3 py-2 text-left text-sm transition-colors hover:bg-accent/70',
                      (isSelected || index === activeIndex) && 'bg-accent/70',
                    )}
                    onClick={() => (isBatchMode ? toggleBatchItem(item.id) : selectItem(item.id))}
                    type="button"
                  >
                    <span className="flex min-w-0 items-center gap-2">
                      {isBatchMode ? (
                        <span
                          aria-hidden="true"
                          className={cn(
                            'flex size-4 shrink-0 items-center justify-center rounded border border-input',
                            isBatchSelected && 'border-primary bg-primary text-primary-foreground',
                          )}
                        >
                          {isBatchSelected ? <Check className="size-3" /> : null}
                        </span>
                      ) : null}
                      <span className="truncate" title={getLocalizedText(item.name, locale)}>
                        {getLocalizedText(item.name, locale)}
                      </span>
                    </span>
                    <span className="truncate text-muted-foreground" title={unit?.symbol}>
                      {unit?.symbol ?? '—'}
                    </span>
                    <span className="truncate tabular-nums text-muted-foreground">
                      {getPriceLabel(price, currency, locale)}
                    </span>
                    <span className="flex justify-end">
                      <span className="inline-flex size-7 items-center justify-center rounded-md border border-border">
                        {isBatchMode ? (
                          <Check aria-hidden="true" className="size-4" />
                        ) : (
                          <Plus aria-hidden="true" className="size-4" />
                        )}
                      </span>
                    </span>
                  </button>
                </li>
              );
            })}
          </ul>
        ) : null}
      </div>

      {isBatchMode ? (
        <div className="flex flex-wrap items-center justify-between gap-2">
          <p className="text-sm text-muted-foreground">
            {t('estimateEditor.catalog.selectedCount', { count: selectedBatchIds.length })}
          </p>
          <Button
            disabled={selectedBatchIds.length === 0 || addMutation.isPending}
            onClick={() => void addBatch()}
            size="sm"
            type="button"
          >
            {addMutation.isPending ? (
              <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
            ) : (
              <Plus aria-hidden="true" className="size-4" />
            )}
            {t('estimateEditor.catalog.addSelected')}
          </Button>
        </div>
      ) : (
        <div className="grid gap-3 lg:grid-cols-[minmax(8rem,0.7fr)_minmax(8rem,0.7fr)_minmax(8rem,0.7fr)_auto] lg:items-end">
          <div className="space-y-1">
            <label className="text-xs font-medium text-muted-foreground" htmlFor={`${kind}-${zoneId}-quantity`}>
              {t('estimateEditor.item.quantity')}
            </label>
            <input
              className={numberFieldClassName}
              id={`${kind}-${zoneId}-quantity`}
              inputMode="decimal"
              min="0.001"
              onChange={(event) => setQuantity(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' && canAdd) {
                  event.preventDefault();
                  void addItem();
                }
              }}
              ref={quantityRef}
              step="any"
              type="number"
              value={quantity}
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-muted-foreground" htmlFor={`${kind}-${zoneId}-unit`}>
              {t('estimateEditor.catalog.unit')}
            </label>
            <input
              className={fieldClassName}
              id={`${kind}-${zoneId}-unit`}
              readOnly
              value={selectedUnit?.symbol ?? '—'}
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-muted-foreground" htmlFor={`${kind}-${zoneId}-price`}>
              {t('estimateEditor.item.unitPrice')}
            </label>
            <input
              className={numberFieldClassName}
              id={`${kind}-${zoneId}-price`}
              inputMode="decimal"
              min="0"
              onChange={(event) => setManualUnitPrice(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' && canAdd) {
                  event.preventDefault();
                  void addItem();
                }
              }}
              placeholder={selectedPrice ? String(selectedPrice.amount) : '—'}
              step="any"
              type="number"
              value={manualUnitPrice}
            />
          </div>
          <Button
            className="lg:min-w-32"
            disabled={!canAdd || addMutation.isPending}
            onClick={() => void addItem()}
            type="button"
          >
            {addMutation.isPending ? (
              <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
            ) : (
              <Plus aria-hidden="true" className="size-4" />
            )}
            {formatMoney(calculateLineItemTotal(quantity, effectiveUnitPrice), currency, locale)}
          </Button>
        </div>
      )}

      <p className="flex items-center gap-1 text-xs text-muted-foreground">
        <ChevronsUpDown aria-hidden="true" className="size-3.5" />
        {selectedPrice
          ? t('estimateEditor.catalog.priceFromPricing')
          : t('estimateEditor.catalog.priceMissing')}
      </p>
    </div>
  );
}
