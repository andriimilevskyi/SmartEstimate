import { zodResolver } from '@hookform/resolvers/zod';
import { Boxes, LoaderCircle, PackagePlus, Plus, Search, Wrench } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import { calculateLineItemTotal } from '@/entities/estimate/model/calculations';
import type { EstimateItemKind, EstimateZone } from '@/entities/estimate/model/types';
import {
  useConstructionWorksQuery,
  useKnowledgeCategoriesQuery,
  useKnowledgeMaterialsQuery,
  useKnowledgeUnitsQuery,
} from '@/entities/knowledge/api/knowledge-queries';
import { getLocalizedText } from '@/entities/knowledge/model/localization';
import type { ConstructionWork, KnowledgeMaterial } from '@/entities/knowledge/model/types';
import {
  addEstimateItemSchema,
  type AddEstimateItemFormValues,
} from '@/features/add-estimate-item/model/add-estimate-item-schema';
import { useAddEstimateItem } from '@/features/estimate-item-mutations/model/use-estimate-item-mutations';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

interface AddEstimateItemFormProps {
  currency: string;
  estimateId: string;
  onZoneChange: (zoneId: string) => void;
  selectedZoneId: string;
  zones: EstimateZone[];
}

const fieldClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

export function AddEstimateItemForm({
  currency,
  estimateId,
  onZoneChange,
  selectedZoneId,
  zones,
}: AddEstimateItemFormProps) {
  const { locale, t } = useTranslation();
  const [kind, setKind] = useState<EstimateItemKind>('work');
  const [categoryId, setCategoryId] = useState('');
  const [search, setSearch] = useState('');
  const categoriesQuery = useKnowledgeCategoriesQuery(kind === 'work');
  const worksQuery = useConstructionWorksQuery(categoryId || undefined, kind === 'work', search);
  const materialsQuery = useKnowledgeMaterialsQuery(undefined, kind === 'material', search);
  const unitsQuery = useKnowledgeUnitsQuery();
  const addMutation = useAddEstimateItem(estimateId, kind);
  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
    setValue,
    watch,
  } = useForm<AddEstimateItemFormValues>({
    defaultValues: {
      catalogItemId: '',
      notes: '',
      quantity: 1,
      unitPrice: 0,
    },
    resolver: zodResolver(addEstimateItemSchema),
  });
  const selectedItemId = watch('catalogItemId');
  const quantity = watch('quantity');
  const unitPrice = watch('unitPrice');
  const notes = watch('notes');
  const catalogItems = useMemo<Array<ConstructionWork | KnowledgeMaterial>>(
    () => (kind === 'work' ? (worksQuery.data?.items ?? []) : (materialsQuery.data?.items ?? [])),
    [kind, materialsQuery.data?.items, worksQuery.data?.items],
  );
  const selectedItem = catalogItems.find((item) => item.id === selectedItemId);
  const selectedUnit = unitsQuery.data?.items.find((unit) => unit.id === selectedItem?.unitId);
  const catalogIsPending =
    unitsQuery.isPending || (kind === 'work' ? worksQuery.isPending : materialsQuery.isPending);
  const catalogHasError =
    unitsQuery.isError || (kind === 'work' ? worksQuery.isError : materialsQuery.isError);

  useEffect(() => {
    setValue('catalogItemId', '');
  }, [categoryId, kind, search, setValue]);

  const changeKind = (nextKind: EstimateItemKind) => {
    setKind(nextKind);
    setCategoryId('');
    setSearch('');
  };

  const onSubmit = (values: AddEstimateItemFormValues) => {
    if (!selectedItem) {
      return;
    }

    addMutation.mutate(
      {
        knowledgeItemId: selectedItem.id,
        notes: values.notes.trim() || null,
        quantity: values.quantity,
        unitPrice: values.unitPrice,
        zoneId: selectedZoneId,
      },
      {
        onError: () => {
          toast.error(t('estimateEditor.messages.addError'));
        },
        onSuccess: () => {
          toast.success(t('estimateEditor.messages.added'));
          reset({
            catalogItemId: '',
            notes: '',
            quantity: 1,
            unitPrice: 0,
          });
        },
      },
    );
  };

  const unitValue = selectedUnit
    ? `${selectedUnit.symbol} - ${getLocalizedText(selectedUnit.name, locale)}`
    : t('estimateEditor.catalog.unitPlaceholder');
  const lineTotal = calculateLineItemTotal(quantity, unitPrice);

  return (
    <section
      aria-labelledby="knowledge-explorer-title"
      className="rounded-xl border border-border bg-card p-5 shadow-sm 2xl:sticky 2xl:top-6"
    >
      <div className="flex items-start gap-3">
        <div className="rounded-lg bg-primary/10 p-2 text-primary">
          <Boxes aria-hidden="true" className="size-5" />
        </div>
        <div>
          <h2 className="font-semibold" id="knowledge-explorer-title">
            {t('estimateEditor.catalog.title')}
          </h2>
          <p className="mt-1 text-sm leading-6 text-muted-foreground">
            {zones.find((zone) => zone.id === selectedZoneId)?.name}
          </p>
        </div>
      </div>

      <div className="mt-5 grid grid-cols-2 gap-2" role="group">
        <Button
          aria-pressed={kind === 'work'}
          onClick={() => changeKind('work')}
          size="sm"
          type="button"
          variant={kind === 'work' ? 'secondary' : 'outline'}
        >
          <Wrench aria-hidden="true" className="size-4" />
          {t('estimateEditor.catalog.work')}
        </Button>
        <Button
          aria-pressed={kind === 'material'}
          onClick={() => changeKind('material')}
          size="sm"
          type="button"
          variant={kind === 'material' ? 'secondary' : 'outline'}
        >
          <PackagePlus aria-hidden="true" className="size-4" />
          {t('estimateEditor.catalog.material')}
        </Button>
      </div>

      <form
        aria-busy={catalogIsPending || addMutation.isPending}
        className="mt-5 space-y-4"
        noValidate
        onSubmit={handleSubmit(onSubmit)}
      >
        <div className="space-y-2">
          <label className="text-sm font-medium" htmlFor="estimate-zone">
            {t('estimateEditor.zone.current')}
          </label>
          <select
            className={fieldClassName}
            id="estimate-zone"
            onChange={(event) => onZoneChange(event.target.value)}
            value={selectedZoneId}
          >
            {zones.map((zone) => (
              <option key={zone.id} value={zone.id}>
                {zone.name}
              </option>
            ))}
          </select>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium" htmlFor="catalog-search">
            {t('estimateEditor.catalog.search')}
          </label>
          <div className="relative">
            <Search
              aria-hidden="true"
              className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
            />
            <input
              className={`${fieldClassName} pl-9`}
              id="catalog-search"
              onChange={(event) => setSearch(event.target.value)}
              placeholder={t('estimateEditor.catalog.searchPlaceholder')}
              value={search}
            />
          </div>
        </div>

        {kind === 'work' ? (
          <div className="space-y-2">
            <label className="text-sm font-medium" htmlFor="knowledge-category">
              {t('estimateEditor.catalog.category')}
            </label>
            <select
              className={fieldClassName}
              disabled={catalogIsPending}
              id="knowledge-category"
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
        ) : null}

        <div className="space-y-2">
          <label className="text-sm font-medium" htmlFor="knowledge-item">
            {kind === 'work'
              ? t('estimateEditor.catalog.constructionWork')
              : t('estimateEditor.catalog.material')}
          </label>
          <select
            aria-invalid={Boolean(errors.catalogItemId)}
            className={fieldClassName}
            disabled={catalogIsPending || catalogHasError}
            id="knowledge-item"
            {...register('catalogItemId')}
          >
            <option value="">{t('estimateEditor.catalog.selectItem')}</option>
            {catalogItems.map((item) => (
              <option key={item.id} value={item.id}>
                {getLocalizedText(item.name, locale)}
              </option>
            ))}
          </select>
          {errors.catalogItemId ? (
            <p className="text-sm text-destructive" role="alert">
              {t('estimateEditor.validation.catalogItem')}
            </p>
          ) : null}
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium" htmlFor="knowledge-unit">
            {t('estimateEditor.catalog.unit')}
          </label>
          <input className={fieldClassName} id="knowledge-unit" readOnly value={unitValue} />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-2">
            <label className="text-sm font-medium" htmlFor="item-quantity">
              {t('estimateEditor.item.quantity')}
            </label>
            <input
              aria-invalid={Boolean(errors.quantity)}
              className={fieldClassName}
              id="item-quantity"
              inputMode="decimal"
              min="0.001"
              step="any"
              type="number"
              {...register('quantity')}
            />
          </div>
          <div className="space-y-2">
            <label className="text-sm font-medium" htmlFor="item-unit-price">
              {t('estimateEditor.item.unitPrice')}
            </label>
            <input
              aria-invalid={Boolean(errors.unitPrice)}
              className={fieldClassName}
              id="item-unit-price"
              inputMode="decimal"
              min="0"
              step="any"
              type="number"
              {...register('unitPrice')}
            />
          </div>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium" htmlFor="item-notes">
            {t('estimateEditor.item.notes')}
          </label>
          <textarea
            className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring"
            id="item-notes"
            placeholder={t('estimateEditor.item.notesPlaceholder')}
            {...register('notes')}
          />
        </div>

        <div className="rounded-lg bg-muted px-3 py-2 text-sm">
          <span className="text-muted-foreground">{t('estimateEditor.item.lineTotal')}: </span>
          <span className="font-semibold tabular-nums">
            {formatMoney(lineTotal, currency, locale)}
          </span>
          {notes ? <p className="mt-1 truncate text-xs text-muted-foreground">{notes}</p> : null}
        </div>

        <Button className="w-full" disabled={catalogIsPending || addMutation.isPending} type="submit">
          {addMutation.isPending ? (
            <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
          ) : (
            <Plus aria-hidden="true" className="size-4" />
          )}
          {t('estimateEditor.catalog.add')}
        </Button>
      </form>
    </section>
  );
}
