import { Copy, LoaderCircle, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import {
  calculateLineItemTotal,
  createEstimateLineItemDraft,
} from '@/entities/estimate/model/calculations';
import type { EstimateItemKind, EstimateLineItem } from '@/entities/estimate/model/types';
import {
  useDeleteEstimateItem,
  useDuplicateEstimateItem,
  useUpdateEstimateItem,
} from '@/features/estimate-item-mutations/model/use-estimate-item-mutations';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

interface EstimateLineItemRowProps {
  currency: string;
  draft?: {
    notes: string;
    quantity: string;
    unitPrice: string;
  };
  estimateId: string;
  item: EstimateLineItem;
  kind: EstimateItemKind;
  onDraftChange: (draft: { notes: string; quantity: string; unitPrice: string }) => void;
  onDraftClear: () => void;
}

const inputClassName =
  'h-10 w-full rounded-md border border-input bg-background px-3 text-sm tabular-nums shadow-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

export function EstimateLineItemRow({
  currency,
  draft,
  estimateId,
  item,
  kind,
  onDraftChange,
  onDraftClear,
}: EstimateLineItemRowProps) {
  const { locale, t } = useTranslation();
  const updateMutation = useUpdateEstimateItem(estimateId, kind);
  const deleteMutation = useDeleteEstimateItem(estimateId, kind);
  const duplicateMutation = useDuplicateEstimateItem(estimateId, kind);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const values = draft ?? createEstimateLineItemDraft(item);
  const quantity = Number(values.quantity);
  const unitPrice = Number(values.unitPrice);
  const isValid =
    Number.isFinite(quantity) && quantity > 0 && Number.isFinite(unitPrice) && unitPrice >= 0;
  const isDirty =
    values.quantity !== String(item.quantity) ||
    values.unitPrice !== String(item.unitPrice) ||
    values.notes !== (item.notes ?? '');
  const calculatedTotal = isValid ? calculateLineItemTotal(quantity, unitPrice) : 0;
  const isSaving =
    updateMutation.isPending || deleteMutation.isPending || duplicateMutation.isPending;

  const persist = () => {
    if (!isDirty || updateMutation.isPending) {
      return;
    }

    if (!isValid) {
      toast.error(t('estimateEditor.messages.updateValidationError'));
      onDraftClear();
      return;
    }

    updateMutation.mutate(
      {
        itemId: item.id,
        payload: {
          notes: values.notes.trim() || null,
          quantity,
          unitPrice,
        },
      },
      {
        onError: () => {
          toast.error(t('estimateEditor.messages.updateError'));
          onDraftClear();
        },
        onSuccess: () => {
          onDraftClear();
        },
      },
    );
  };

  const remove = () => {
    deleteMutation.mutate(item.id, {
      onError: () => {
        toast.error(t('estimateEditor.messages.deleteError'));
      },
      onSuccess: () => {
        toast.success(t('estimateEditor.messages.deleted'));
        setIsDeleteOpen(false);
        onDraftClear();
      },
    });
  };

  const duplicate = () => {
    duplicateMutation.mutate(item.id, {
      onError: () => {
        toast.error(t('estimateEditor.messages.duplicateError'));
      },
      onSuccess: () => {
        toast.success(t('estimateEditor.messages.duplicated'));
      },
    });
  };

  return (
    <li className="grid gap-4 border-b border-border px-5 py-4 last:border-b-0 xl:grid-cols-[minmax(11rem,1.2fr)_7rem_8rem_8rem_minmax(12rem,0.9fr)_auto] xl:items-end">
      <div className="min-w-0 space-y-1">
        <p className="truncate font-medium" title={item.name}>
          {item.name}
        </p>
        <p className="text-sm text-muted-foreground">{item.measurementUnit}</p>
      </div>

      <div className="space-y-2">
        <label
          className="text-xs font-medium text-muted-foreground"
          htmlFor={`item-${item.id}-quantity`}
        >
          {t('estimateEditor.item.quantity')}
        </label>
        <input
          aria-invalid={!isValid}
          className={inputClassName}
          disabled={isSaving}
          id={`item-${item.id}-quantity`}
          inputMode="decimal"
          min="0.0001"
          onBlur={persist}
          onChange={(event) => onDraftChange({ ...values, quantity: event.target.value })}
          step="any"
          type="number"
          value={values.quantity}
        />
      </div>

      <div className="space-y-2">
        <label
          className="text-xs font-medium text-muted-foreground"
          htmlFor={`item-${item.id}-unit-price`}
        >
          {t('estimateEditor.item.unitPrice')}
        </label>
        <input
          aria-invalid={!isValid}
          className={inputClassName}
          disabled={isSaving}
          id={`item-${item.id}-unit-price`}
          inputMode="decimal"
          min="0"
          onBlur={persist}
          onChange={(event) => onDraftChange({ ...values, unitPrice: event.target.value })}
          step="any"
          type="number"
          value={values.unitPrice}
        />
      </div>

      <div className="space-y-2">
        <p className="text-xs font-medium text-muted-foreground">
          {t('estimateEditor.item.lineTotal')}
        </p>
        <p aria-live="polite" className="flex h-10 items-center font-semibold tabular-nums">
          {formatMoney(calculatedTotal, currency, locale)}
        </p>
      </div>

      <div className="space-y-2">
        <label
          className="text-xs font-medium text-muted-foreground"
          htmlFor={`item-${item.id}-notes`}
        >
          {t('estimateEditor.item.notes')}
        </label>
        <input
          className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSaving}
          id={`item-${item.id}-notes`}
          onBlur={persist}
          onChange={(event) => onDraftChange({ ...values, notes: event.target.value })}
          placeholder={t('estimateEditor.item.notesPlaceholder')}
          value={values.notes}
        />
      </div>

      <div className="flex items-center justify-between gap-2 lg:justify-end">
        <p aria-live="polite" className="text-xs text-muted-foreground">
          {updateMutation.isPending ? t('estimateEditor.item.saving') : null}
        </p>
        <Button
          aria-label={t('estimateEditor.item.duplicate')}
          disabled={isSaving}
          onClick={duplicate}
          size="icon"
          type="button"
          variant="ghost"
        >
          {duplicateMutation.isPending ? (
            <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
          ) : (
            <Copy aria-hidden="true" className="size-4" />
          )}
        </Button>
        <Button
          aria-label={t('estimateEditor.item.delete')}
          disabled={isSaving}
          onClick={() => setIsDeleteOpen(true)}
          size="sm"
          type="button"
          variant="ghost"
        >
          {deleteMutation.isPending ? (
            <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
          ) : (
            <Trash2 aria-hidden="true" className="size-4" />
          )}
          <span className="sr-only sm:not-sr-only">{t('estimateEditor.item.delete')}</span>
        </Button>
      </div>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('estimateEditor.item.delete')}
        description={t('estimateEditor.item.deleteConfirmation')}
        isLoading={deleteMutation.isPending}
        isOpen={isDeleteOpen}
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={remove}
        title={t('estimateEditor.item.deleteTitle')}
        variant="destructive"
      />
    </li>
  );
}
