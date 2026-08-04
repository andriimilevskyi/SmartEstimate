import {
  Archive,
  BookOpenText,
  ChevronLeft,
  ChevronRight,
  Plus,
  Search,
  Sparkles,
} from 'lucide-react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import {
  archiveKnowledge,
  createConstructionWork,
  createKnowledgeCategory,
  createKnowledgeMaterial,
  createKnowledgeUnit,
  getConstructionWorks,
  getKnowledgeCategories,
  getKnowledgeMaterials,
  getKnowledgeUnits,
  updateConstructionWork,
  updateKnowledgeCategory,
  updateKnowledgeMaterial,
  updateKnowledgeUnit,
} from '@/entities/knowledge/api/knowledge-api';
import { getLocalizedText } from '@/entities/knowledge/model/localization';
import type {
  ConstructionWork,
  KnowledgeCategory,
  KnowledgeMaterial,
  KnowledgeStatus,
  KnowledgeUnit,
} from '@/entities/knowledge/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

type StudioResource = 'categories' | 'construction-works' | 'materials' | 'units';
type StudioRecord = KnowledgeCategory | ConstructionWork | KnowledgeMaterial | KnowledgeUnit;

interface FormState {
  categoryId: string;
  description: string;
  id?: string;
  nameDe: string;
  nameEn: string;
  nameUk: string;
  parentCategoryId: string;
  status: KnowledgeStatus;
  symbol: string;
  tags: string;
  unitId: string;
}

const emptyForm = (): FormState => ({
  categoryId: '',
  description: '',
  nameDe: '',
  nameEn: '',
  nameUk: '',
  parentCategoryId: '',
  status: 'Draft',
  symbol: '',
  tags: '',
  unitId: '',
});

const fieldClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring';

const hasTags = (record: StudioRecord): record is ConstructionWork | KnowledgeMaterial =>
  'tags' in record;
const hasUnit = (record: StudioRecord): record is ConstructionWork | KnowledgeMaterial =>
  'unitId' in record;
export function KnowledgeStudioPage() {
  const { locale, t } = useTranslation();
  const client = useQueryClient();
  const [resource, setResource] = useState<StudioResource>('construction-works');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<KnowledgeStatus | ''>('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [sort, setSort] = useState('name');
  const [page, setPage] = useState(1);
  const [form, setForm] = useState<FormState>(emptyForm);

  const categoriesQuery = useQuery({
    queryFn: ({ signal }) => getKnowledgeCategories(signal, { activeOnly: false, pageSize: 200 }),
    queryKey: ['knowledge', 'studio', 'categories-options'],
  });
  const unitsQuery = useQuery({
    queryFn: ({ signal }) => getKnowledgeUnits(signal, { activeOnly: false, pageSize: 200 }),
    queryKey: ['knowledge', 'studio', 'units-options'],
  });
  const recordsQuery = useQuery({
    queryFn: ({ signal }) => {
      const options = {
        activeOnly: false,
        categoryId: categoryFilter || undefined,
        page,
        pageSize: 10,
        search,
        sort,
        status: status || undefined,
      };
      switch (resource) {
        case 'categories':
          return getKnowledgeCategories(signal, options);
        case 'construction-works':
          return getConstructionWorks(undefined, signal, options);
        case 'materials':
          return getKnowledgeMaterials(undefined, signal, options);
        case 'units':
          return getKnowledgeUnits(signal, options);
      }
    },
    queryKey: ['knowledge', 'studio', resource, page, search, sort, status, categoryFilter],
  });

  const records = (recordsQuery.data?.items ?? []) as StudioRecord[];
  const totalPages = Math.max(1, Math.ceil((recordsQuery.data?.totalCount ?? 0) / 10));
  const categoryOptions = categoriesQuery.data?.items ?? [];
  const unitOptions = unitsQuery.data?.items ?? [];

  const mutation = useMutation({
    mutationFn: async (nextForm: FormState) => {
      const name = {
        de: nextForm.nameDe || undefined,
        en: nextForm.nameEn || undefined,
        uk: nextForm.nameUk,
      };
      const tags = nextForm.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter(Boolean);
      if (resource === 'categories') {
        const request = {
          description: nextForm.description || undefined,
          name,
          parentCategoryId: nextForm.parentCategoryId || null,
          status: nextForm.status,
        };
        return nextForm.id
          ? updateKnowledgeCategory(nextForm.id, request)
          : createKnowledgeCategory(request);
      }
      if (resource === 'construction-works') {
        const request = {
          categoryId: nextForm.categoryId,
          description: nextForm.description || undefined,
          name,
          status: nextForm.status,
          tags,
          unitId: nextForm.unitId,
        };
        return nextForm.id
          ? updateConstructionWork(nextForm.id, request)
          : createConstructionWork(request);
      }
      if (resource === 'materials') {
        const request = {
          categoryId: nextForm.categoryId || null,
          description: nextForm.description || undefined,
          name,
          status: nextForm.status,
          tags,
          unitId: nextForm.unitId,
        };
        return nextForm.id
          ? updateKnowledgeMaterial(nextForm.id, request)
          : createKnowledgeMaterial(request);
      }
      const request = { name, status: nextForm.status, symbol: nextForm.symbol };
      return nextForm.id ? updateKnowledgeUnit(nextForm.id, request) : createKnowledgeUnit(request);
    },
    onError: () => toast.error(t('knowledgeStudio.messages.saveError')),
    onSuccess: () => {
      toast.success(t('knowledgeStudio.messages.saved'));
      setForm(emptyForm());
      void client.invalidateQueries({ queryKey: ['knowledge'] });
    },
  });

  const archiveMutation = useMutation({
    mutationFn: (id: string) => archiveKnowledge(resource, id),
    onError: () => toast.error(t('knowledgeStudio.messages.archiveError')),
    onSuccess: () => {
      toast.success(t('knowledgeStudio.messages.archived'));
      setForm(emptyForm());
      void client.invalidateQueries({ queryKey: ['knowledge'] });
    },
  });

  const title = useMemo(() => {
    const titles: Record<StudioResource, string> = {
      categories: t('knowledgeStudio.resources.categories'),
      'construction-works': t('knowledgeStudio.resources.works'),
      materials: t('knowledgeStudio.resources.materials'),
      units: t('knowledgeStudio.resources.units'),
    };
    return titles[resource];
  }, [resource, t]);

  const selectResource = (next: StudioResource) => {
    setResource(next);
    setPage(1);
    setCategoryFilter('');
    setForm(emptyForm());
  };

  const startEdit = (record: StudioRecord) => {
    setForm({
      categoryId: 'categoryId' in record ? (record.categoryId ?? '') : '',
      description: 'description' in record ? (record.description ?? '') : '',
      id: record.id,
      nameDe: record.name.de,
      nameEn: record.name.en,
      nameUk: record.name.uk,
      parentCategoryId: 'parentCategoryId' in record ? (record.parentCategoryId ?? '') : '',
      status: record.status,
      symbol: 'symbol' in record ? record.symbol : '',
      tags: hasTags(record) ? record.tags.join(', ') : '',
      unitId: hasUnit(record) ? record.unitId : '',
    });
  };

  const statusText = (value: KnowledgeStatus) => {
    if (value === 'Active') return t('knowledgeStudio.status.active');
    if (value === 'Archived') return t('knowledgeStudio.status.archived');
    return t('knowledgeStudio.status.draft');
  };

  const updateForm = <TKey extends keyof FormState>(key: TKey, value: FormState[TKey]) =>
    setForm((current) => ({ ...current, [key]: value }));

  return (
    <div className="space-y-6">
      <section className="flex flex-col justify-between gap-4 rounded-xl border border-border bg-card p-6 shadow-sm lg:flex-row lg:items-center">
        <div className="flex items-start gap-3">
          <div className="rounded-lg bg-primary/10 p-2 text-primary">
            <BookOpenText aria-hidden="true" className="size-5" />
          </div>
          <div>
            <h1 className="text-xl font-semibold">{t('knowledgeStudio.title')}</h1>
            <p className="mt-1 text-sm text-muted-foreground">{t('knowledgeStudio.description')}</p>
          </div>
        </div>
        <Button onClick={() => setForm(emptyForm())} type="button">
          <Plus aria-hidden="true" className="size-4" />
          {t('knowledgeStudio.actions.new')}
        </Button>
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.45fr)_minmax(20rem,0.8fr)]">
        <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
          <div className="border-b border-border p-4">
            <div
              className="flex flex-wrap gap-2"
              role="tablist"
              aria-label={t('knowledgeStudio.resourceTabs')}
            >
              {(['categories', 'construction-works', 'materials', 'units'] as const).map((item) => (
                <Button
                  aria-selected={resource === item}
                  key={item}
                  onClick={() => selectResource(item)}
                  size="sm"
                  type="button"
                  variant={resource === item ? 'secondary' : 'ghost'}
                >
                  {item === 'categories'
                    ? t('knowledgeStudio.resources.categories')
                    : item === 'construction-works'
                      ? t('knowledgeStudio.resources.works')
                      : item === 'materials'
                        ? t('knowledgeStudio.resources.materials')
                        : t('knowledgeStudio.resources.units')}
                </Button>
              ))}
            </div>
            <div className="mt-4 grid gap-2 md:grid-cols-4">
              <label className="relative">
                <Search
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 top-3 size-4 text-muted-foreground"
                />
                <span className="sr-only">{t('knowledgeStudio.filters.search')}</span>
                <input
                  className={`${fieldClassName} pl-9`}
                  onChange={(event) => {
                    setSearch(event.target.value);
                    setPage(1);
                  }}
                  placeholder={t('knowledgeStudio.filters.search')}
                  value={search}
                />
              </label>
              <select
                aria-label={t('knowledgeStudio.filters.status')}
                className={fieldClassName}
                onChange={(event) => {
                  setStatus(event.target.value as KnowledgeStatus | '');
                  setPage(1);
                }}
                value={status}
              >
                <option value="">{t('knowledgeStudio.filters.allStatuses')}</option>
                <option value="Draft">{t('knowledgeStudio.status.draft')}</option>
                <option value="Active">{t('knowledgeStudio.status.active')}</option>
                <option value="Archived">{t('knowledgeStudio.status.archived')}</option>
              </select>
              <select
                aria-label={t('knowledgeStudio.filters.sort')}
                className={fieldClassName}
                onChange={(event) => setSort(event.target.value)}
                value={sort}
              >
                <option value="name">{t('knowledgeStudio.sort.name')}</option>
                <option value="-name">{t('knowledgeStudio.sort.nameDescending')}</option>
                <option value="-createdAt">{t('knowledgeStudio.sort.newest')}</option>
                <option value="createdAt">{t('knowledgeStudio.sort.oldest')}</option>
              </select>
              {resource === 'construction-works' || resource === 'materials' ? (
                <select
                  aria-label={t('knowledgeStudio.filters.category')}
                  className={fieldClassName}
                  onChange={(event) => {
                    setCategoryFilter(event.target.value);
                    setPage(1);
                  }}
                  value={categoryFilter}
                >
                  <option value="">{t('knowledgeStudio.filters.allCategories')}</option>
                  {categoryOptions.map((category) => (
                    <option key={category.id} value={category.id}>
                      {getLocalizedText(category.name, locale)}
                    </option>
                  ))}
                </select>
              ) : null}
            </div>
          </div>
          {recordsQuery.isPending ? (
            <p className="p-6 text-sm text-muted-foreground">{t('knowledgeStudio.loading')}</p>
          ) : null}
          {recordsQuery.isError ? (
            <p className="p-6 text-sm text-destructive">{t('knowledgeStudio.error')}</p>
          ) : null}
          {!recordsQuery.isPending && !recordsQuery.isError && records.length === 0 ? (
            <div className="p-8 text-center">
              <p className="font-medium">{t('knowledgeStudio.empty.title')}</p>
              <p className="mt-1 text-sm text-muted-foreground">
                {t('knowledgeStudio.empty.description')}
              </p>
            </div>
          ) : null}
          <ul className="divide-y divide-border">
            {records.map((record) => (
              <li className="flex items-center justify-between gap-3 p-4" key={record.id}>
                <button
                  className="min-w-0 text-left"
                  onClick={() => startEdit(record)}
                  type="button"
                >
                  <p className="truncate font-medium">{getLocalizedText(record.name, locale)}</p>
                  <p className="mt-1 truncate text-xs text-muted-foreground">{record.id}</p>
                </button>
                <div className="flex shrink-0 items-center gap-2">
                  <span className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">
                    {statusText(record.status)}
                  </span>
                  <Button
                    aria-label={t('knowledgeStudio.actions.archive')}
                    disabled={record.status === 'Archived' || archiveMutation.isPending}
                    onClick={() => archiveMutation.mutate(record.id)}
                    size="icon"
                    type="button"
                    variant="ghost"
                  >
                    <Archive aria-hidden="true" className="size-4" />
                  </Button>
                </div>
              </li>
            ))}
          </ul>
          <div className="flex items-center justify-between border-t border-border p-3 text-sm text-muted-foreground">
            <span>{t('knowledgeStudio.pagination', { page, total: totalPages })}</span>
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

        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between gap-3">
            <div>
              <h2 className="font-semibold">
                {form.id
                  ? t('knowledgeStudio.form.editTitle')
                  : t('knowledgeStudio.form.createTitle', { resource: title })}
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                {t('knowledgeStudio.form.description')}
              </p>
            </div>
            {resource === 'construction-works' ? (
              <Button disabled size="sm" type="button" variant="outline">
                <Sparkles aria-hidden="true" className="size-4" />
                {t('knowledgeStudio.ai.comingSoon')}
              </Button>
            ) : null}
          </div>
          <form
            className="mt-5 space-y-4"
            noValidate
            onSubmit={(event) => {
              event.preventDefault();
              mutation.mutate(form);
            }}
          >
            <div className="grid gap-3 sm:grid-cols-3">
              <label className="space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.nameUk')}</span>
                <input
                  className={fieldClassName}
                  onChange={(event) => updateForm('nameUk', event.target.value)}
                  required
                  value={form.nameUk}
                />
              </label>
              <label className="space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.nameEn')}</span>
                <input
                  className={fieldClassName}
                  onChange={(event) => updateForm('nameEn', event.target.value)}
                  value={form.nameEn}
                />
              </label>
              <label className="space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.nameDe')}</span>
                <input
                  className={fieldClassName}
                  onChange={(event) => updateForm('nameDe', event.target.value)}
                  value={form.nameDe}
                />
              </label>
            </div>
            {resource !== 'units' ? (
              <label className="block space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.descriptionLabel')}</span>
                <textarea
                  className="min-h-20 w-full rounded-md border border-input bg-background p-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  onChange={(event) => updateForm('description', event.target.value)}
                  value={form.description}
                />
              </label>
            ) : null}
            {resource === 'categories' ? (
              <label className="block space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.parentCategory')}</span>
                <select
                  className={fieldClassName}
                  onChange={(event) => updateForm('parentCategoryId', event.target.value)}
                  value={form.parentCategoryId}
                >
                  <option value="">{t('knowledgeStudio.form.none')}</option>
                  {categoryOptions
                    .filter((category) => category.id !== form.id)
                    .map((category) => (
                      <option key={category.id} value={category.id}>
                        {getLocalizedText(category.name, locale)}
                      </option>
                    ))}
                </select>
              </label>
            ) : null}
            {resource === 'construction-works' || resource === 'materials' ? (
              <>
                <label className="block space-y-1 text-sm font-medium">
                  <span>{t('knowledgeStudio.form.category')}</span>
                  <select
                    className={fieldClassName}
                    onChange={(event) => updateForm('categoryId', event.target.value)}
                    required={resource === 'construction-works'}
                    value={form.categoryId}
                  >
                    <option value="">
                      {resource === 'construction-works'
                        ? t('knowledgeStudio.form.selectCategory')
                        : t('knowledgeStudio.form.none')}
                    </option>
                    {categoryOptions.map((category) => (
                      <option key={category.id} value={category.id}>
                        {getLocalizedText(category.name, locale)}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="block space-y-1 text-sm font-medium">
                  <span>{t('knowledgeStudio.form.unit')}</span>
                  <select
                    className={fieldClassName}
                    onChange={(event) => updateForm('unitId', event.target.value)}
                    required
                    value={form.unitId}
                  >
                    <option value="">{t('knowledgeStudio.form.selectUnit')}</option>
                    {unitOptions.map((unit) => (
                      <option key={unit.id} value={unit.id}>
                        {unit.symbol} — {getLocalizedText(unit.name, locale)}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="block space-y-1 text-sm font-medium">
                  <span>{t('knowledgeStudio.form.tags')}</span>
                  <input
                    className={fieldClassName}
                    onChange={(event) => updateForm('tags', event.target.value)}
                    placeholder={t('knowledgeStudio.form.tagsPlaceholder')}
                    value={form.tags}
                  />
                </label>
              </>
            ) : null}
            {resource === 'units' ? (
              <label className="block space-y-1 text-sm font-medium">
                <span>{t('knowledgeStudio.form.symbol')}</span>
                <input
                  className={fieldClassName}
                  onChange={(event) => updateForm('symbol', event.target.value)}
                  required
                  value={form.symbol}
                />
              </label>
            ) : null}
            <label className="block space-y-1 text-sm font-medium">
              <span>{t('knowledgeStudio.form.status')}</span>
              <select
                className={fieldClassName}
                onChange={(event) => updateForm('status', event.target.value as KnowledgeStatus)}
                value={form.status}
              >
                <option value="Draft">{t('knowledgeStudio.status.draft')}</option>
                <option value="Active">{t('knowledgeStudio.status.active')}</option>
                <option value="Archived">{t('knowledgeStudio.status.archived')}</option>
              </select>
            </label>
            {form.id ? (
              <dl className="grid grid-cols-2 gap-3 rounded-lg bg-muted/50 p-3 text-xs text-muted-foreground">
                <div>
                  <dt>{t('knowledgeStudio.card.id')}</dt>
                  <dd className="mt-1 break-all text-foreground">{form.id}</dd>
                </div>
                <div>
                  <dt>{t('knowledgeStudio.card.version')}</dt>
                  <dd className="mt-1 text-foreground">
                    {records.find((record) => record.id === form.id)?.version}
                  </dd>
                </div>
                <div>
                  <dt>{t('knowledgeStudio.card.createdAt')}</dt>
                  <dd className="mt-1 text-foreground">
                    {formatDate(
                      records.find((record) => record.id === form.id)?.createdAt ?? '',
                      locale,
                    )}
                  </dd>
                </div>
                <div>
                  <dt>{t('knowledgeStudio.card.updatedAt')}</dt>
                  <dd className="mt-1 text-foreground">
                    {formatDate(
                      records.find((record) => record.id === form.id)?.updatedAt ?? '',
                      locale,
                    )}
                  </dd>
                </div>
              </dl>
            ) : null}
            <div className="flex justify-end gap-2">
              <Button onClick={() => setForm(emptyForm())} type="button" variant="ghost">
                {t('knowledgeStudio.actions.cancel')}
              </Button>
              <Button disabled={mutation.isPending} type="submit">
                {t('knowledgeStudio.actions.save')}
              </Button>
            </div>
          </form>
        </section>
      </div>
    </div>
  );
}
