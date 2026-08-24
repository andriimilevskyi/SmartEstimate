import { useEffect, useId } from 'react';
import { LoaderCircle } from 'lucide-react';

import { Button } from '@/shared/ui/button';

interface ConfirmationDialogProps {
  cancelLabel: string;
  confirmLabel: string;
  description: string;
  isLoading?: boolean;
  isOpen: boolean;
  onCancel: () => void;
  onConfirm: () => void;
  title: string;
  variant?: 'default' | 'destructive';
}

export function ConfirmationDialog({
  cancelLabel,
  confirmLabel,
  description,
  isLoading = false,
  isOpen,
  onCancel,
  onConfirm,
  title,
  variant = 'default',
}: ConfirmationDialogProps) {
  const titleId = useId();
  const descriptionId = useId();

  useEffect(() => {
    if (!isOpen) {
      return undefined;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !isLoading) {
        onCancel();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isLoading, isOpen, onCancel]);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      aria-labelledby={titleId}
      aria-describedby={descriptionId}
      aria-modal="true"
      className="fixed inset-0 z-50 grid place-items-center bg-background/80 p-4 backdrop-blur-sm"
      role="alertdialog"
    >
      <div className="w-full max-w-md rounded-xl border border-border bg-card p-5 shadow-lg">
        <div className="space-y-2">
          <h2 className="text-lg font-semibold" id={titleId}>
            {title}
          </h2>
          <p className="text-sm leading-6 text-muted-foreground" id={descriptionId}>
            {description}
          </p>
        </div>
        <div className="mt-5 flex justify-end gap-2">
          <Button disabled={isLoading} onClick={onCancel} type="button" variant="ghost">
            {cancelLabel}
          </Button>
          <Button
            autoFocus
            disabled={isLoading}
            onClick={onConfirm}
            type="button"
            variant={variant === 'destructive' ? 'destructive' : 'default'}
          >
            {isLoading ? <LoaderCircle aria-hidden="true" className="size-4 animate-spin" /> : null}
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
