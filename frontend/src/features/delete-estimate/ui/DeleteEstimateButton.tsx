import { LoaderCircle, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { useDeleteEstimate } from '@/features/delete-estimate/model/use-delete-estimate';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

interface DeleteEstimateButtonProps {
  estimateId: string;
  onDeleted?: () => void;
}

export function DeleteEstimateButton({ estimateId, onDeleted }: DeleteEstimateButtonProps) {
  const { t } = useTranslation();
  const deleteMutation = useDeleteEstimate();
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);

  const handleDelete = () => {
    deleteMutation.mutate(estimateId, {
      onError: () => {
        toast.error(t('estimates.messages.deleteError'));
      },
      onSuccess: () => {
        toast.success(t('estimates.messages.deleted'));
        setIsConfirmOpen(false);
        onDeleted?.();
      },
    });
  };

  return (
    <>
      <Button
        aria-label={t('estimates.delete.action')}
        disabled={deleteMutation.isPending}
        onClick={() => setIsConfirmOpen(true)}
        size="sm"
        type="button"
        variant="ghost"
      >
        {deleteMutation.isPending ? (
          <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
        ) : (
          <Trash2 aria-hidden="true" className="size-4" />
        )}
        <span className="sr-only sm:not-sr-only">{t('estimates.delete.action')}</span>
      </Button>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('estimates.delete.action')}
        description={t('estimates.delete.confirmation')}
        isLoading={deleteMutation.isPending}
        isOpen={isConfirmOpen}
        onCancel={() => setIsConfirmOpen(false)}
        onConfirm={handleDelete}
        title={t('estimates.delete.title')}
        variant="destructive"
      />
    </>
  );
}
