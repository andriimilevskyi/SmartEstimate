import {
  Archive,
  ChevronLeft,
  ChevronRight,
  Clock3,
  LoaderCircle,
  Plus,
  Save,
  Search,
  Tags,
  Wrench,
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';

import { useKnowledgeCategoriesQuery } from '@/entities/knowledge/api/knowledge-queries';
import { getLocalizedText } from '@/entities/knowledge/model/localization';
import type { PriceSourceType, PriceTargetType } from '@/entities/pricing/model/types';
import {
  useArchivePriceMutation,
  useCreatePriceMutation,
  usePriceHistoryQuery,
  usePricingCatalogQuery,
  useUpdatePriceMutation,
} from '@/entities/pricing/api/pricing-queries';
import { getApiErrorMessage } from '@/shared/api/api-client';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate, formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

const pageSize = 25;
const fieldClassName =
  'h-10 rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const sourceTypes: PriceSourceType[] = [
  'Manual',
  'Import',
  'SupplierIntegration',
  'MarketReference',
  'AiRecommendation',
];

const todayInputValue = () => new Date().toISOString().slice(0, 10);
const toIsoDate = (value: string) => new Date(`${value}T00:00:00.000Z`).toISOString();

export function PricingPage() {
  const { locale, t } = useTranslation();
  const [targetType, setTargetType] = useState<PriceTargetType>('Material');
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [currency, setCurrency] = useState('UAH');
  const [supplier, setSupplier] = useState('');
  const [regionCode, setRegionCode] = useState('');
  const [missingOnly, setMissingOnly] = useState(false);
  const [page, setPage] = useState(1);
  const [selectedTargetId, setSelectedTargetId] = useState('');
  const [amount, setAmount] = useState('');
  const [effectiveFrom, setEffectiveFrom] = useState(todayInputValue());
  const [sourceType, setSourceType] = useState<PriceSourceType>('Manual');
  const [formSupplier, setFormSupplier] = useState('');
  const [formRegion, setFormRegion] = useState('');
  const [notes, setNotes] = useState('');
  const [isArchiveOpen, setIsArchiveOpen] = useState(false);

  const catalogOptions = {
    categoryId: categoryId || undefined,
    currency: currency || undefined,
    missingOnly,
    page,
    pageSize,
    regionCode: regionCode || undefined,
    search: search || undefined,
    supplier: supplier || undefined,
    targetType,
  };
  const catalogQuery = usePricingCatalogQuery(catalogOptions);
  const categoriesQuery = useKnowledgeCategoriesQuery(true, { activeOnly: true, pageSize: 500 });
  const createMutation = useCreatePriceMutation();
  const updateMutation = useUpdatePriceMutation();
  const archiveMutation = useArchivePriceMutation();

  const items = useMemo(() => catalogQuery.data?.items ?? [], [catalogQuery.data?.items]);
  const selectedItem = items.find((item) => item.targetId === selectedTargetId) ?? items[0];
  const historyQuery = usePriceHistoryQuery(
    selectedItem?.targetType ?? targetType,
    selectedItem?.targetId ?? '',
    Boolean(selectedItem),
  );
  const totalPages = Math.max(1, Math.ceil((catalogQuery.data?.totalCount ?? 0) / pageSize));

  useEffect(() => {
    if (items.length === 0) {
      setSelectedTargetId('');
      return;
    }

    if (!items.some((item) => item.targetId === selectedTargetId)) {
      setSelectedTargetId(items[0].targetId);
    }
  }, [items, selectedTargetId]);

  useEffect(() => {
    if (!selectedItem) {
      return;
    }

    const price = selectedItem.currentPrice;
    setAmount(price ? String(price.amount) : '');
    setEffectiveFrom(price ? price.effectiveFrom.slice(0, 10) : todayInputValue());
    setSourceType(price?.sourceType ?? 'Manual');
    setFormSupplier(price?.supplierName ?? '');
    setFormRegion(price?.regionCode ?? '');
    setNotes(price?.notes ?? '');
  }, [selectedItem]);

  const resetPage = () => setPage(1);
  const switchTarget = (nextTarget: PriceTargetType) => {
    setTargetType(nextTarget);
    setSelectedTargetId('');
    setPage(1);
  };

  const savePrice = () => {
    if (!selectedItem) {
      return;
    }

    const numericAmount = Number(amount);
    if (!Number.isFinite(numericAmount) || numericAmount < 0) {
      toast.error(t('pricing.messages.validationError'));
      return;
    }

    const request = {
      amount: numericAmount,
      currency,
      effectiveFrom: toIsoDate(effectiveFrom),
      notes: notes.trim() || null,
      regionCode: formRegion.trim() || null,
      sourceType,
      supplierId: null,
      supplierName: formSupplier.trim() || null,
      targetId: selectedItem.targetId,
      targetType: selectedItem.targetType,
    };
    if (selectedItem.currentPrice) {
      updateMutation.mutate(
        { priceId: selectedItem.currentPrice.id, request },
        {
          onError: (error: unknown) =>
            toast.error(getApiErrorMessage(error, t, 'pricing.messages.saveError')),
          onSuccess: () => toast.success(t('pricing.messages.saved')),
        },
      );
      return;
    }

    createMutation.mutate(request, {
      onError: (error: unknown) =>
        toast.error(getApiErrorMessage(error, t, 'pricing.messages.saveError')),
      onSuccess: () => toast.success(t('pricing.messages.saved')),
    });
  };

  const archivePrice = () => {
    if (!selectedItem?.currentPrice) {
      return;
    }

    archiveMutation.mutate(selectedItem.currentPrice.id, {
      onError: (error: unknown) =>
        toast.error(getApiErrorMessage(error, t, 'pricing.messages.archiveError')),
      onSuccess: () => {
        toast.success(t('pricing.messages.archived'));
        setIsArchiveOpen(false);
      },
    });
  };

  return (
    <section aria-labelledby="pricing-title" className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm font-medium text-primary">{t('pricing.eyebrow')}</p>
        <h1 className="text-3xl font-semibold tracking-tight" id="pricing-title">
          {t('pricing.title')}
        </h1>
        <p className="max-w-3xl text-base leading-7 text-muted-foreground">
          {t('pricing.description')}
        </p>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.55fr)_minmax(22rem,0.8fr)]">
        <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <div className="border-b border-border p-4">
            <div className="flex flex-wrap items-center gap-2">
              <Button
                aria-pressed={targetType === 'Material'}
                onClick={() => switchTarget('Material')}
                size="sm"
                type="button"
                variant={targetType === 'Material' ? 'secondary' : 'outline'}
              >
                <Tags aria-hidden="true" className="size-4" />
                {t('pricing.targets.Material')}
              </Button>
              <Button
                aria-pressed={targetType === 'ConstructionWork'}
                onClick={() => switchTarget('ConstructionWork')}
                size="sm"
                type="button"
                variant={targetType === 'ConstructionWork' ? 'secondary' : 'outline'}
              >
                <Wrench aria-hidden="true" className="size-4" />
                {t('pricing.targets.ConstructionWork')}
              </Button>
            </div>

            <div className="mt-4 grid gap-2 lg:grid-cols-[minmax(12rem,1.2fr)_minmax(10rem,0.9fr)_6rem_8rem_8rem_auto]">
              <label className="relative">
                <Search
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                />
                <span className="sr-only">{t('pricing.filters.search')}</span>
                <input
                  className={`${fieldClassName} w-full pl-9`}
                  onChange={(event) => {
                    setSearch(event.target.value);
                    resetPage();
                  }}
                  placeholder={t('pricing.filters.search')}
                  value={search}
                />
              </label>
              <select
                aria-label={t('pricing.filters.category')}
                className={fieldClassName}
                onChange={(event) => {
                  setCategoryId(event.target.value);
                  resetPage();
                }}
                value={categoryId}
              >
                <option value="">{t('pricing.filters.allCategories')}</option>
                {(categoriesQuery.data?.items ?? []).map((category) => (
                  <option key={category.id} value={category.id}>
                    {getLocalizedText(category.name, locale)}
                  </option>
                ))}
              </select>
              <input
                aria-label={t('pricing.filters.currency')}
                className={fieldClassName}
                maxLength={3}
                onChange={(event) => {
                  setCurrency(event.target.value.toUpperCase());
                  resetPage();
                }}
                value={currency}
              />
              <input
                aria-label={t('pricing.filters.supplier')}
                className={fieldClassName}
                onChange={(event) => {
                  setSupplier(event.target.value);
                  resetPage();
                }}
                placeholder={t('pricing.filters.supplierShort')}
                value={supplier}
              />
              <input
                aria-label={t('pricing.filters.region')}
                className={fieldClassName}
                onChange={(event) => {
                  setRegionCode(event.target.value.toUpperCase());
                  resetPage();
                }}
                placeholder={t('pricing.filters.regionPlaceholder')}
                value={regionCode}
              />
              <label className="inline-flex h-10 items-center gap-2 rounded-md border border-input px-3 text-sm">
                <input
                  checked={missingOnly}
                  onChange={(event) => {
                    setMissingOnly(event.target.checked);
                    resetPage();
                  }}
                  type="checkbox"
                />
                {t('pricing.filters.missingOnly')}
              </label>
            </div>
          </div>

          {catalogQuery.isPending ? (
            <div className="flex min-h-64 items-center justify-center gap-3 text-sm text-muted-foreground">
              <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
              {t('pricing.states.loading')}
            </div>
          ) : null}

          {catalogQuery.isError ? (
            <div className="flex min-h-64 items-center justify-center px-6 text-center text-sm text-muted-foreground">
              {t('pricing.states.error')}
            </div>
          ) : null}

          {!catalogQuery.isPending && !catalogQuery.isError && items.length === 0 ? (
            <div className="flex min-h-64 items-center justify-center text-sm text-muted-foreground">
              {t('pricing.states.empty')}
            </div>
          ) : null}

          {!catalogQuery.isPending && !catalogQuery.isError && items.length > 0 ? (
            <ul className="divide-y divide-border">
              {items.map((item) => (
                <li key={item.targetId}>
                  <button
                    className={`grid w-full gap-3 px-5 py-4 text-left transition-colors hover:bg-accent/60 xl:grid-cols-[minmax(0,1.2fr)_minmax(8rem,0.65fr)_5rem_8rem_8rem_7rem] xl:items-center ${
                      selectedItem?.targetId === item.targetId ? 'bg-accent/70' : ''
                    }`}
                    onClick={() => setSelectedTargetId(item.targetId)}
                    type="button"
                  >
                    <span className="min-w-0">
                      <span className="block truncate font-medium">{item.name}</span>
                      <span className="mt-1 block truncate text-sm text-muted-foreground">
                        {item.categoryName ?? t('pricing.fallback.noCategory')}
                      </span>
                    </span>
                    <span className="text-sm text-muted-foreground">{item.unitSymbol}</span>
                    <span className="font-medium tabular-nums">
                      {item.currentPrice
                        ? formatMoney(item.currentPrice.amount, item.currentPrice.currency, locale)
                        : '—'}
                    </span>
                    <span className="text-sm text-muted-foreground">
                      {item.currentPrice?.supplierName ?? t('pricing.fallback.defaultSupplier')}
                    </span>
                    <span className="text-sm text-muted-foreground">
                      {item.currentPrice?.regionCode ?? t('pricing.fallback.globalRegion')}
                    </span>
                    <span className="text-sm text-muted-foreground">
                      {item.currentPrice
                        ? formatDate(item.currentPrice.effectiveFrom, locale)
                        : '—'}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          ) : null}

          <div className="flex items-center justify-between border-t border-border px-4 py-3 text-sm text-muted-foreground">
            <span>{t('pricing.pagination', { page, total: totalPages })}</span>
            <div className="flex gap-1">
              <Button
                aria-label={t('pricing.actions.previous')}
                disabled={page <= 1}
                onClick={() => setPage((value) => value - 1)}
                size="icon"
                type="button"
                variant="ghost"
              >
                <ChevronLeft aria-hidden="true" className="size-4" />
              </Button>
              <Button
                aria-label={t('pricing.actions.next')}
                disabled={page >= totalPages}
                onClick={() => setPage((value) => value + 1)}
                size="icon"
                type="button"
                variant="ghost"
              >
                <ChevronRight aria-hidden="true" className="size-4" />
              </Button>
            </div>
          </div>
        </section>

        <aside className="space-y-4">
          <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h2 className="font-semibold">
                  {selectedItem?.name ?? t('pricing.editor.fallbackTitle')}
                </h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  {selectedItem
                    ? `${selectedItem.categoryName ?? t('pricing.fallback.noCategory')} · ${selectedItem.unitSymbol}`
                    : t('pricing.editor.selectRecord')}
                </p>
              </div>
              <Plus aria-hidden="true" className="size-5 text-muted-foreground" />
            </div>

            <div className="mt-5 grid gap-3">
              <label className="space-y-2">
                <span className="text-sm font-medium">{t('pricing.editor.price')}</span>
                <input
                  className={`${fieldClassName} w-full`}
                  inputMode="decimal"
                  min="0"
                  onChange={(event) => setAmount(event.target.value)}
                  type="number"
                  value={amount}
                />
              </label>
              <div className="grid grid-cols-2 gap-3">
                <label className="space-y-2">
                  <span className="text-sm font-medium">{t('pricing.editor.currency')}</span>
                  <input
                    className={`${fieldClassName} w-full`}
                    maxLength={3}
                    onChange={(event) => setCurrency(event.target.value.toUpperCase())}
                    value={currency}
                  />
                </label>
                <label className="space-y-2">
                  <span className="text-sm font-medium">{t('pricing.editor.date')}</span>
                  <input
                    className={`${fieldClassName} w-full`}
                    onChange={(event) => setEffectiveFrom(event.target.value)}
                    type="date"
                    value={effectiveFrom}
                  />
                </label>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <label className="space-y-2">
                  <span className="text-sm font-medium">{t('pricing.editor.supplier')}</span>
                  <input
                    className={`${fieldClassName} w-full`}
                    onChange={(event) => setFormSupplier(event.target.value)}
                    value={formSupplier}
                  />
                </label>
                <label className="space-y-2">
                  <span className="text-sm font-medium">{t('pricing.editor.region')}</span>
                  <input
                    className={`${fieldClassName} w-full`}
                    onChange={(event) => setFormRegion(event.target.value.toUpperCase())}
                    value={formRegion}
                  />
                </label>
              </div>
              <label className="space-y-2">
                <span className="text-sm font-medium">{t('pricing.editor.source')}</span>
                <select
                  className={`${fieldClassName} w-full`}
                  onChange={(event) => setSourceType(event.target.value as PriceSourceType)}
                  value={sourceType}
                >
                  {sourceTypes.map((item) => (
                    <option key={item} value={item}>
                      {t(`pricing.sources.${item}`)}
                    </option>
                  ))}
                </select>
              </label>
              <label className="space-y-2">
                <span className="text-sm font-medium">{t('pricing.editor.notes')}</span>
                <textarea
                  className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  onChange={(event) => setNotes(event.target.value)}
                  value={notes}
                />
              </label>
            </div>

            <div className="mt-5 flex gap-2">
              <Button
                disabled={!selectedItem || createMutation.isPending || updateMutation.isPending}
                onClick={savePrice}
                type="button"
              >
                {createMutation.isPending || updateMutation.isPending ? (
                  <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                ) : (
                  <Save aria-hidden="true" className="size-4" />
                )}
                {t('pricing.actions.save')}
              </Button>
              <Button
                disabled={!selectedItem?.currentPrice || archiveMutation.isPending}
                onClick={() => setIsArchiveOpen(true)}
                type="button"
                variant="outline"
              >
                <Archive aria-hidden="true" className="size-4" />
                {t('pricing.actions.archive')}
              </Button>
            </div>
          </section>

          <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
            <div className="flex items-center gap-2">
              <Clock3 aria-hidden="true" className="size-5 text-muted-foreground" />
              <h2 className="font-semibold">{t('pricing.history.title')}</h2>
            </div>
            <div className="mt-4 space-y-3">
              {historyQuery.isPending ? (
                <p className="text-sm text-muted-foreground">{t('pricing.history.loading')}</p>
              ) : null}
              {(historyQuery.data?.prices ?? []).slice(0, 6).map((price) => (
                <div className="rounded-lg border border-border px-3 py-2 text-sm" key={price.id}>
                  <div className="flex items-center justify-between gap-3">
                    <span className="font-medium tabular-nums">
                      {formatMoney(price.amount, price.currency, locale)}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {t(`pricing.status.${price.status}`)}
                    </span>
                  </div>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {formatDate(price.effectiveFrom, locale)} ·{' '}
                    {t(`pricing.sources.${price.sourceType}`)} ·{' '}
                    {price.supplierName ?? t('pricing.fallback.defaultSupplier')} ·{' '}
                    {price.regionCode ?? t('pricing.fallback.globalRegion')}
                  </p>
                </div>
              ))}
              {!historyQuery.isPending && (historyQuery.data?.prices.length ?? 0) === 0 ? (
                <p className="text-sm text-muted-foreground">{t('pricing.history.empty')}</p>
              ) : null}
            </div>
          </section>
        </aside>
      </div>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('pricing.actions.archive')}
        description={t('pricing.dialogs.archiveDescription')}
        isLoading={archiveMutation.isPending}
        isOpen={isArchiveOpen}
        onCancel={() => setIsArchiveOpen(false)}
        onConfirm={archivePrice}
        title={t('pricing.dialogs.archiveTitle')}
        variant="destructive"
      />
    </section>
  );
}
