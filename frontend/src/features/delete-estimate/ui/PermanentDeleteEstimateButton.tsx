import { LoaderCircle, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { useDeleteEstimatePermanently } from '@/features/delete-estimate/model/use-delete-estimate-permanently';
import { getApiErrorMessage } from '@/shared/api/api-client';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

interface PermanentDeleteEstimateButtonProps {
  estimateId: string;
  onDeleted?: () => void;
}

export function PermanentDeleteEstimateButton({
  estimateId,
  onDeleted,
}: PermanentDeleteEstimateButtonProps) {
  const { t } = useTranslation();
  const deleteMutation = useDeleteEstimatePermanently();
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);

  const handleDelete = () => {
    deleteMutation.mutate(estimateId, {
      onError: (error: unknown) => {
        toast.error(getApiErrorMessage(error, t, 'estimates.messages.permanentDeleteError'));
      },
      onSuccess: () => {
        toast.success(t('estimates.messages.permanentlyDeleted'));
        setIsConfirmOpen(false);
        onDeleted?.();
      },
    });
  };

  return (
    <>
      <Button
        aria-label={t('estimates.permanentDelete.action')}
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
        <span className="sr-only sm:not-sr-only">{t('estimates.permanentDelete.action')}</span>
      </Button>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('estimates.permanentDelete.action')}
        description={t('estimates.permanentDelete.description')}
        isLoading={deleteMutation.isPending}
        isOpen={isConfirmOpen}
        onCancel={() => setIsConfirmOpen(false)}
        onConfirm={handleDelete}
        title={t('estimates.permanentDelete.confirmation')}
        variant="destructive"
      />
    </>
  );
}
