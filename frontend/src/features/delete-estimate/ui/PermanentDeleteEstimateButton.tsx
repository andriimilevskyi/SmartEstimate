import { LoaderCircle, Trash2 } from 'lucide-react';
import { toast } from 'sonner';

import { useDeleteEstimatePermanently } from '@/features/delete-estimate/model/use-delete-estimate-permanently';
import { ApiClientError } from '@/shared/api/api-client';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';

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

  const handleDelete = () => {
    const confirmation = `${t('estimates.permanentDelete.confirmation')}\n\n${t('estimates.permanentDelete.warning')}`;
    if (!window.confirm(confirmation)) {
      return;
    }

    deleteMutation.mutate(estimateId, {
      onError: (error: unknown) => {
        if (error instanceof ApiClientError) {
          toast.error(error.message);
          return;
        }

        toast.error(t('estimates.messages.permanentDeleteError'));
      },
      onSuccess: () => {
        toast.success(t('estimates.messages.permanentlyDeleted'));
        onDeleted?.();
      },
    });
  };

  return (
    <Button
      aria-label={t('estimates.permanentDelete.action')}
      disabled={deleteMutation.isPending}
      onClick={handleDelete}
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
  );
}
