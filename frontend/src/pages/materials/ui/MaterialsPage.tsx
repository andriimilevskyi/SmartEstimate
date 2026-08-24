import {
  ArrowUpDown,
  ChevronLeft,
  ChevronRight,
  FileText,
  LoaderCircle,
  Package,
  Search,
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';

import { useEstimatesQuery } from '@/entities/estimate/api/estimate-queries';
import {
  useKnowledgeCategoriesQuery,
  useKnowledgeMaterialsQuery,
  useKnowledgeUnitsQuery,
} from '@/entities/knowledge/api/knowledge-queries';
import { getLocalizedText } from '@/entities/knowledge/model/localization';
import type { KnowledgeMaterial, KnowledgeStatus } from '@/entities/knowledge/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

const pageSize = 25;

const fieldClassName =
  'h-10 rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring';

const statusClassName: Record<KnowledgeStatus, string> = {
  Active: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  Archived: 'border-slate-200 bg-slate-50 text-slate-600',
  Draft: 'border-amber-200 bg-amber-50 text-amber-700',
};

export function MaterialsPage() {
  const { locale, t } = useTranslation();
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [status, setStatus] = useState<KnowledgeStatus | ''>('Active');
  const [sort, setSort] = useState('name');
  const [page, setPage] = useState(1);
  const [selectedMaterialId, setSelectedMaterialId] = useState('');

  const materialOptions = {
    activeOnly: false,
    page,
    pageSize,
    search: search || undefined,
    sort,
    status: status || undefined,
  };
  const materialsQuery = useKnowledgeMaterialsQuery(
    categoryId || undefined,
    true,
    search || undefined,
    materialOptions,
  );
  const categoriesQuery = useKnowledgeCategoriesQuery(true, { activeOnly: false, pageSize: 500 });
  const unitsQuery = useKnowledgeUnitsQuery({ activeOnly: false, pageSize: 500 });
  const estimatesQuery = useEstimatesQuery();

  const materials = useMemo(() => materialsQuery.data?.items ?? [], [materialsQuery.data?.items]);
  const totalCount = materialsQuery.data?.totalCount ?? 0;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const categoriesById = useMemo(
    () => new Map((categoriesQuery.data?.items ?? []).map((category) => [category.id, category])),
    [categoriesQuery.data?.items],
  );
  const unitsById = useMemo(
    () => new Map((unitsQuery.data?.items ?? []).map((unit) => [unit.id, unit])),
    [unitsQuery.data?.items],
  );
  const selectedMaterial =
    materials.find((material) => material.id === selectedMaterialId) ?? materials[0];

  useEffect(() => {
    if (materials.length === 0) {
      setSelectedMaterialId('');
      return;
    }

    if (!materials.some((material) => material.id === selectedMaterialId)) {
      setSelectedMaterialId(materials[0].id);
    }
  }, [materials, selectedMaterialId]);

  const resetPage = () => setPage(1);
  const statusText = (value: KnowledgeStatus) => t(`knowledgeStudio.status.${value.toLowerCase()}`);
  const categoryName = (material: KnowledgeMaterial) =>
    material.categoryId && categoriesById.has(material.categoryId)
      ? getLocalizedText(categoriesById.get(material.categoryId)!.name, locale)
      : t('materials.catalog.noCategory');
  const unitLabel = (material: KnowledgeMaterial) => {
    const unit = unitsById.get(material.unitId);
    return unit ? `${unit.symbol} · ${getLocalizedText(unit.name, locale)}` : t('materials.catalog.unitUnknown');
  };

  return (
    <section aria-labelledby="materials-title" className="space-y-6">
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('materials.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="materials-title">
            {t('materials.title')}
          </h1>
          <p className="max-w-3xl text-base leading-7 text-muted-foreground">
            {t('materials.description')}
          </p>
        </div>
        <Button asChild variant="outline">
          <Link to="/knowledge-studio">{t('materials.actions.manageKnowledge')}</Link>
        </Button>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.6fr)_minmax(20rem,0.75fr)]">
        <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <div className="border-b border-border p-4">
            <div className="flex items-center justify-between gap-4">
              <div>
                <h2 className="font-semibold">{t('materials.catalog.title')}</h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  {t('materials.catalog.count', { count: totalCount })}
                </p>
              </div>
              <ArrowUpDown aria-hidden="true" className="size-5 text-muted-foreground" />
            </div>

            <div className="mt-4 grid gap-2 lg:grid-cols-[minmax(12rem,1.2fr)_minmax(10rem,1fr)_9rem_11rem]">
              <label className="relative">
                <Search
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                />
                <span className="sr-only">{t('materials.filters.search')}</span>
                <input
                  className={`${fieldClassName} w-full pl-9`}
                  onChange={(event) => {
                    setSearch(event.target.value);
                    resetPage();
                  }}
                  placeholder={t('materials.filters.search')}
                  value={search}
                />
              </label>
              <select
                aria-label={t('materials.filters.category')}
                className={`${fieldClassName} w-full`}
                disabled={categoriesQuery.isPending}
                onChange={(event) => {
                  setCategoryId(event.target.value);
                  resetPage();
                }}
                value={categoryId}
              >
                <option value="">{t('materials.filters.allCategories')}</option>
                {(categoriesQuery.data?.items ?? []).map((category) => (
                  <option key={category.id} value={category.id}>
                    {getLocalizedText(category.name, locale)}
                  </option>
                ))}
              </select>
              <select
                aria-label={t('materials.filters.status')}
                className={`${fieldClassName} w-full`}
                onChange={(event) => {
                  setStatus(event.target.value as KnowledgeStatus | '');
                  resetPage();
                }}
                value={status}
              >
                <option value="">{t('materials.filters.allStatuses')}</option>
                <option value="Active">{t('knowledgeStudio.status.active')}</option>
                <option value="Draft">{t('knowledgeStudio.status.draft')}</option>
                <option value="Archived">{t('knowledgeStudio.status.archived')}</option>
              </select>
              <select
                aria-label={t('materials.filters.sort')}
                className={`${fieldClassName} w-full`}
                onChange={(event) => {
                  setSort(event.target.value);
                  resetPage();
                }}
                value={sort}
              >
                <option value="name">{t('knowledgeStudio.sort.name')}</option>
                <option value="-name">{t('knowledgeStudio.sort.nameDescending')}</option>
                <option value="-createdAt">{t('knowledgeStudio.sort.newest')}</option>
                <option value="createdAt">{t('knowledgeStudio.sort.oldest')}</option>
              </select>
            </div>
          </div>

          {materialsQuery.isPending ? (
            <div
              aria-live="polite"
              className="flex min-h-64 items-center justify-center gap-3 text-sm text-muted-foreground"
            >
              <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
              {t('materials.states.loading')}
            </div>
          ) : null}

          {materialsQuery.isError ? (
            <div className="flex min-h-64 flex-col items-center justify-center gap-4 px-5 text-center">
              <p className="max-w-md text-sm leading-6 text-muted-foreground">
                {t('materials.states.error')}
              </p>
              <Button onClick={() => void materialsQuery.refetch()} variant="outline">
                {t('actions.retry')}
              </Button>
            </div>
          ) : null}

          {!materialsQuery.isPending && !materialsQuery.isError && materials.length === 0 ? (
            <div className="flex min-h-64 flex-col items-center justify-center gap-3 px-5 text-center">
              <div className="rounded-full bg-muted p-3 text-muted-foreground">
                <Package aria-hidden="true" className="size-6" />
              </div>
              <div>
                <p className="font-medium">{t('materials.states.emptyTitle')}</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  {t('materials.states.emptyDescription')}
                </p>
              </div>
            </div>
          ) : null}

          {!materialsQuery.isPending && !materialsQuery.isError && materials.length > 0 ? (
            <ul className="divide-y divide-border">
              {materials.map((material) => {
                const isSelected = selectedMaterial?.id === material.id;

                return (
                  <li key={material.id}>
                    <button
                      className={`grid w-full gap-3 px-5 py-4 text-left transition-colors hover:bg-accent/60 md:grid-cols-[minmax(0,1.2fr)_minmax(10rem,0.65fr)_minmax(8rem,0.45fr)_auto] md:items-center ${
                        isSelected ? 'bg-accent/70' : ''
                      }`}
                      onClick={() => setSelectedMaterialId(material.id)}
                      type="button"
                    >
                      <span className="min-w-0">
                        <span className="block truncate font-medium">
                          {getLocalizedText(material.name, locale)}
                        </span>
                        <span className="mt-1 block truncate text-sm text-muted-foreground">
                          {material.description ?? t('materials.catalog.noDescription')}
                        </span>
                      </span>
                      <span className="text-sm text-muted-foreground">{categoryName(material)}</span>
                      <span className="text-sm font-medium">{unitLabel(material)}</span>
                      <span
                        className={`w-fit rounded-full border px-2 py-1 text-xs font-medium ${
                          statusClassName[material.status]
                        }`}
                      >
                        {statusText(material.status)}
                      </span>
                    </button>
                  </li>
                );
              })}
            </ul>
          ) : null}

          <div className="flex items-center justify-between border-t border-border px-4 py-3 text-sm text-muted-foreground">
            <span>{t('materials.pagination', { page, total: totalPages })}</span>
            <div className="flex gap-1">
              <Button
                aria-label={t('knowledgeStudio.actions.previous')}
                disabled={page <= 1}
                onClick={() => setPage((current) => current - 1)}
                size="icon"
                type="button"
                variant="ghost"
              >
                <ChevronLeft aria-hidden="true" className="size-4" />
              </Button>
              <Button
                aria-label={t('knowledgeStudio.actions.next')}
                disabled={page >= totalPages}
                onClick={() => setPage((current) => current + 1)}
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
            {selectedMaterial ? (
              <div className="space-y-5">
                <div className="space-y-2">
                  <span
                    className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${
                      statusClassName[selectedMaterial.status]
                    }`}
                  >
                    {statusText(selectedMaterial.status)}
                  </span>
                  <h2 className="text-xl font-semibold">
                    {getLocalizedText(selectedMaterial.name, locale)}
                  </h2>
                  <p className="text-sm leading-6 text-muted-foreground">
                    {selectedMaterial.description ?? t('materials.catalog.noDescription')}
                  </p>
                </div>

                <dl className="grid gap-3 text-sm">
                  <div className="rounded-lg bg-muted/60 p-3">
                    <dt className="text-xs text-muted-foreground">{t('materials.catalog.category')}</dt>
                    <dd className="mt-1 font-medium">{categoryName(selectedMaterial)}</dd>
                  </div>
                  <div className="rounded-lg bg-muted/60 p-3">
                    <dt className="text-xs text-muted-foreground">{t('materials.catalog.unit')}</dt>
                    <dd className="mt-1 font-medium">{unitLabel(selectedMaterial)}</dd>
                  </div>
                  <div className="rounded-lg bg-muted/60 p-3">
                    <dt className="text-xs text-muted-foreground">{t('materials.catalog.updatedAt')}</dt>
                    <dd className="mt-1 font-medium">
                      {formatDate(selectedMaterial.updatedAt, locale)}
                    </dd>
                  </div>
                </dl>

                {selectedMaterial.tags.length > 0 ? (
                  <div className="flex flex-wrap gap-2">
                    {selectedMaterial.tags.slice(0, 8).map((tag) => (
                      <span
                        className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground"
                        key={tag}
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                ) : null}
              </div>
            ) : (
              <div className="text-sm text-muted-foreground">{t('materials.states.selectMaterial')}</div>
            )}
          </section>

          <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
            <div className="flex items-start gap-3">
              <div className="rounded-lg bg-primary/10 p-2 text-primary">
                <FileText aria-hidden="true" className="size-5" />
              </div>
              <div>
                <h2 className="font-semibold">{t('materials.use.title')}</h2>
                <p className="mt-1 text-sm leading-6 text-muted-foreground">
                  {t('materials.use.description')}
                </p>
              </div>
            </div>

            {estimatesQuery.isPending ? (
              <p className="mt-4 text-sm text-muted-foreground">{t('materials.use.loading')}</p>
            ) : null}

            {estimatesQuery.data?.items.length ? (
              <div className="mt-4 space-y-2">
                {estimatesQuery.data.items.slice(0, 5).map((estimate) => (
                  <Link
                    className="block rounded-lg border border-border px-3 py-2 text-sm transition-colors hover:border-primary/50 hover:bg-accent"
                    key={estimate.id}
                    to={`/estimates/${estimate.id}${
                      selectedMaterial ? `?materialId=${selectedMaterial.id}` : ''
                    }`}
                  >
                    <span className="block font-medium">{estimate.estimateNumber}</span>
                    <span className="mt-1 block truncate text-xs text-muted-foreground">
                      {estimate.object.customerName} · {estimate.object.name}
                    </span>
                  </Link>
                ))}
              </div>
            ) : null}

            {!estimatesQuery.isPending && estimatesQuery.data?.items.length === 0 ? (
              <div className="mt-4">
                <Button asChild className="w-full" variant="outline">
                  <Link to="/estimates?create=estimate">{t('materials.use.createEstimate')}</Link>
                </Button>
              </div>
            ) : null}
          </section>
        </aside>
      </div>
    </section>
  );
}
