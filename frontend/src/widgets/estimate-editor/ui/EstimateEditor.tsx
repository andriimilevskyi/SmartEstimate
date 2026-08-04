import { ArrowDown, ArrowUp, Calculator, Plus, Trash2, Wrench, PackagePlus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';

import {
  applyEstimateLineItemDrafts,
  type EstimateLineItemDraft,
  type EstimateLineItemDrafts,
} from '@/entities/estimate/model/calculations';
import type { Estimate, EstimateItemKind, EstimateLineItem, EstimateZone } from '@/entities/estimate/model/types';
import { AddEstimateItemForm } from '@/features/add-estimate-item/ui/AddEstimateItemForm';
import { EstimateLineItemRow } from '@/features/edit-estimate-item/ui/EstimateLineItemRow';
import {
  useAddEstimateZone,
  useDeleteEstimateZone,
  useReorderEstimateZones,
  useUpdateEstimateZone,
} from '@/features/estimate-item-mutations/model/use-estimate-item-mutations';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

interface EstimateItemsSectionProps {
  currency: string;
  drafts: EstimateLineItemDrafts;
  estimateId: string;
  items: EstimateLineItem[];
  kind: EstimateItemKind;
  onDraftChange: (itemId: string, draft: EstimateLineItemDraft) => void;
  onDraftClear: (itemId: string) => void;
}

function EstimateItemsSection({
  currency,
  drafts,
  estimateId,
  items,
  kind,
  onDraftChange,
  onDraftClear,
}: EstimateItemsSectionProps) {
  const { t } = useTranslation();
  const isWork = kind === 'work';

  return (
    <div>
      <div className="mb-2 flex items-center gap-2 text-sm font-semibold">
        {isWork ? (
          <Wrench aria-hidden="true" className="size-4 text-muted-foreground" />
        ) : (
          <PackagePlus aria-hidden="true" className="size-4 text-muted-foreground" />
        )}
        {isWork ? t('estimateEditor.workItems.title') : t('estimateEditor.materialItems.title')}
        <span className="text-xs font-normal text-muted-foreground">
          {t('estimateEditor.itemCount', { count: items.length })}
        </span>
      </div>

      {items.length === 0 ? (
        <div className="rounded-md border border-dashed border-border px-4 py-5 text-sm text-muted-foreground">
          {isWork ? t('estimateEditor.workItems.empty') : t('estimateEditor.materialItems.empty')}
        </div>
      ) : (
        <ul className="overflow-hidden rounded-md border border-border">
          {items.map((item) => (
            <EstimateLineItemRow
              currency={currency}
              draft={drafts[item.id]}
              estimateId={estimateId}
              item={item}
              key={item.id}
              kind={kind}
              onDraftChange={(draft) => onDraftChange(item.id, draft)}
              onDraftClear={() => onDraftClear(item.id)}
            />
          ))}
        </ul>
      )}
    </div>
  );
}

interface ZoneSectionProps {
  currency: string;
  drafts: EstimateLineItemDrafts;
  estimate: Estimate;
  isSelected: boolean;
  onDraftChange: (itemId: string, draft: EstimateLineItemDraft) => void;
  onDraftClear: (itemId: string) => void;
  onMoveZone: (zoneId: string, direction: 'up' | 'down') => void;
  onRenameZone: (zoneId: string, name: string) => void;
  onRemoveZone: (zoneId: string) => void;
  onSelect: (zoneId: string) => void;
  zone: EstimateZone;
}

function ZoneSection({
  currency,
  drafts,
  estimate,
  isSelected,
  onDraftChange,
  onDraftClear,
  onMoveZone,
  onRenameZone,
  onRemoveZone,
  onSelect,
  zone,
}: ZoneSectionProps) {
  const { locale, t } = useTranslation();
  const [nameDraft, setNameDraft] = useState(zone.name);
  const workItems = estimate.workItems.filter((item) => item.zoneId === zone.id);
  const materialItems = estimate.materialItems.filter((item) => item.zoneId === zone.id);

  useEffect(() => {
    setNameDraft(zone.name);
  }, [zone.name]);

  const persistName = () => {
    const nextName = nameDraft.trim();
    if (nextName && nextName !== zone.name) {
      onRenameZone(zone.id, nextName);
    } else {
      setNameDraft(zone.name);
    }
  };

  return (
    <section
      className={`rounded-xl border bg-card shadow-sm ${
        isSelected ? 'border-primary/60 ring-2 ring-primary/10' : 'border-border'
      }`}
      onFocus={() => onSelect(zone.id)}
    >
      <div className="flex flex-col gap-4 border-b border-border p-4 xl:flex-row xl:items-center xl:justify-between">
        <div className="min-w-0 flex-1">
          <input
            className="h-10 w-full rounded-md border border-transparent bg-transparent px-2 text-lg font-semibold outline-none transition-colors focus-visible:border-input focus-visible:bg-background focus-visible:ring-2 focus-visible:ring-ring"
            onBlur={persistName}
            onChange={(event) => setNameDraft(event.target.value)}
            value={nameDraft}
          />
        </div>
        <div className="grid gap-3 text-sm sm:grid-cols-3 xl:min-w-[27rem]">
          <div>
            <p className="text-xs text-muted-foreground">{t('estimateDetails.laborTotal')}</p>
            <p className="font-semibold tabular-nums">{formatMoney(zone.totalLabor, currency, locale)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">{t('estimateDetails.materialsTotal')}</p>
            <p className="font-semibold tabular-nums">{formatMoney(zone.totalMaterials, currency, locale)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">{t('estimateDetails.grandTotal')}</p>
            <p className="font-semibold tabular-nums text-primary">
              {formatMoney(zone.grandTotal, currency, locale)}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <Button aria-label={t('estimateEditor.zone.moveUp')} onClick={() => onMoveZone(zone.id, 'up')} size="icon" type="button" variant="ghost">
            <ArrowUp aria-hidden="true" className="size-4" />
          </Button>
          <Button aria-label={t('estimateEditor.zone.moveDown')} onClick={() => onMoveZone(zone.id, 'down')} size="icon" type="button" variant="ghost">
            <ArrowDown aria-hidden="true" className="size-4" />
          </Button>
          <Button aria-label={t('estimateEditor.zone.delete')} onClick={() => onRemoveZone(zone.id)} size="icon" type="button" variant="ghost">
            <Trash2 aria-hidden="true" className="size-4" />
          </Button>
        </div>
      </div>

      <div className="space-y-5 p-4">
        <EstimateItemsSection
          currency={currency}
          drafts={drafts}
          estimateId={estimate.id}
          items={workItems}
          kind="work"
          onDraftChange={onDraftChange}
          onDraftClear={onDraftClear}
        />
        <EstimateItemsSection
          currency={currency}
          drafts={drafts}
          estimateId={estimate.id}
          items={materialItems}
          kind="material"
          onDraftChange={onDraftChange}
          onDraftClear={onDraftClear}
        />
      </div>
    </section>
  );
}

interface EstimateTotalsProps {
  estimate: Estimate;
}

function EstimateTotals({ estimate }: EstimateTotalsProps) {
  const { locale, t } = useTranslation();

  return (
    <aside aria-labelledby="estimate-totals-title" className="rounded-xl border border-border bg-card p-5 shadow-sm">
      <div className="flex items-center gap-3">
        <div className="rounded-lg bg-primary/10 p-2 text-primary">
          <Calculator aria-hidden="true" className="size-5" />
        </div>
        <div>
          <h2 className="font-semibold" id="estimate-totals-title">
            {t('estimateEditor.totals.title')}
          </h2>
          <p className="text-sm text-muted-foreground">{t('estimateEditor.totals.description')}</p>
        </div>
      </div>

      <dl aria-live="polite" className="mt-5 space-y-4 text-sm">
        <div className="flex items-center justify-between gap-4">
          <dt className="text-muted-foreground">{t('estimateDetails.laborTotal')}</dt>
          <dd className="font-medium tabular-nums">
            {formatMoney(estimate.totalLabor, estimate.currency, locale)}
          </dd>
        </div>
        <div className="flex items-center justify-between gap-4">
          <dt className="text-muted-foreground">{t('estimateDetails.materialsTotal')}</dt>
          <dd className="font-medium tabular-nums">
            {formatMoney(estimate.totalMaterials, estimate.currency, locale)}
          </dd>
        </div>
        <div className="border-t border-border pt-4">
          <div className="flex items-end justify-between gap-4">
            <dt className="font-semibold">{t('estimateDetails.grandTotal')}</dt>
            <dd className="text-xl font-semibold tabular-nums text-primary">
              {formatMoney(estimate.grandTotal, estimate.currency, locale)}
            </dd>
          </div>
        </div>
      </dl>
    </aside>
  );
}

interface EstimateEditorProps {
  estimate: Estimate;
}

export function EstimateEditor({ estimate }: EstimateEditorProps) {
  const { t } = useTranslation();
  const [drafts, setDrafts] = useState<EstimateLineItemDrafts>({});
  const zones = [...estimate.zones].sort((left, right) => left.sortOrder - right.sortOrder);
  const [selectedZoneId, setSelectedZoneId] = useState(zones[0]?.id ?? '');
  const displayedEstimate = useMemo(
    () => applyEstimateLineItemDrafts(estimate, drafts),
    [drafts, estimate],
  );
  const addZoneMutation = useAddEstimateZone(estimate.id);
  const updateZoneMutation = useUpdateEstimateZone(estimate.id);
  const reorderZonesMutation = useReorderEstimateZones(estimate.id);
  const deleteZoneMutation = useDeleteEstimateZone(estimate.id);

  useEffect(() => {
    if (!zones.some((zone) => zone.id === selectedZoneId)) {
      setSelectedZoneId(zones[0]?.id ?? '');
    }
  }, [selectedZoneId, zones]);

  const updateDraft = (itemId: string, draft: EstimateLineItemDraft) => {
    setDrafts((currentDrafts) => ({
      ...currentDrafts,
      [itemId]: draft,
    }));
  };

  const clearDraft = (itemId: string) => {
    setDrafts((currentDrafts) => {
      const remainingDrafts = { ...currentDrafts };

      delete remainingDrafts[itemId];

      return remainingDrafts;
    });
  };

  const addZone = () => {
    const name = window.prompt(t('estimateEditor.zone.newName'));
    if (!name?.trim()) {
      return;
    }

    addZoneMutation.mutate(name.trim(), {
      onError: () => toast.error(t('estimateEditor.messages.zoneError')),
      onSuccess: (updatedEstimate) => {
        const createdZone = [...updatedEstimate.zones].sort((left, right) => right.sortOrder - left.sortOrder)[0];
        setSelectedZoneId(createdZone.id);
      },
    });
  };

  const renameZone = (zoneId: string, name: string) => {
    updateZoneMutation.mutate(
      { name, zoneId },
      { onError: () => toast.error(t('estimateEditor.messages.zoneError')) },
    );
  };

  const moveZone = (zoneId: string, direction: 'up' | 'down') => {
    const currentIndex = zones.findIndex((zone) => zone.id === zoneId);
    const nextIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1;
    if (currentIndex < 0 || nextIndex < 0 || nextIndex >= zones.length) {
      return;
    }

    const nextZones = [...zones];
    const [zone] = nextZones.splice(currentIndex, 1);
    nextZones.splice(nextIndex, 0, zone);
    reorderZonesMutation.mutate(nextZones.map((nextZone) => nextZone.id), {
      onError: () => toast.error(t('estimateEditor.messages.zoneError')),
    });
  };

  const removeZone = (zoneId: string) => {
    if (!window.confirm(t('estimateEditor.zone.deleteConfirmation'))) {
      return;
    }

    deleteZoneMutation.mutate(zoneId, {
      onError: () => toast.error(t('estimateEditor.messages.zoneError')),
    });
  };

  return (
    <div className="grid gap-6 2xl:grid-cols-[minmax(18rem,0.72fr)_minmax(0,1.8fr)_minmax(16rem,0.62fr)]">
      <AddEstimateItemForm
        currency={estimate.currency}
        estimateId={estimate.id}
        onZoneChange={setSelectedZoneId}
        selectedZoneId={selectedZoneId}
        zones={zones}
      />

      <div className="min-w-0 space-y-4">
        <div className="flex flex-wrap items-center gap-2">
          {zones.map((zone) => (
            <Button
              aria-pressed={zone.id === selectedZoneId}
              key={zone.id}
              onClick={() => setSelectedZoneId(zone.id)}
              size="sm"
              type="button"
              variant={zone.id === selectedZoneId ? 'secondary' : 'outline'}
            >
              {zone.name}
            </Button>
          ))}
          <Button onClick={addZone} size="sm" type="button" variant="outline">
            <Plus aria-hidden="true" className="size-4" />
            {t('estimateEditor.zone.add')}
          </Button>
        </div>

        {displayedEstimate.zones
          .sort((left, right) => left.sortOrder - right.sortOrder)
          .map((zone) => (
            <ZoneSection
              currency={estimate.currency}
              drafts={drafts}
              estimate={displayedEstimate}
              isSelected={zone.id === selectedZoneId}
              key={zone.id}
              onDraftChange={updateDraft}
              onDraftClear={clearDraft}
              onMoveZone={moveZone}
              onRemoveZone={removeZone}
              onRenameZone={renameZone}
              onSelect={setSelectedZoneId}
              zone={zone}
            />
          ))}
      </div>

      <EstimateTotals estimate={displayedEstimate} />
    </div>
  );
}
