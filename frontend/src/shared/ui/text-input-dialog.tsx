import { useEffect, useId, useState } from 'react';
import { LoaderCircle } from 'lucide-react';

import { Button } from '@/shared/ui/button';

interface TextInputDialogProps {
  cancelLabel: string;
  confirmLabel: string;
  description: string;
  initialValue?: string;
  inputLabel: string;
  isLoading?: boolean;
  isOpen: boolean;
  onCancel: () => void;
  onConfirm: (value: string) => void;
  title: string;
}

const inputClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

export function TextInputDialog({
  cancelLabel,
  confirmLabel,
  description,
  initialValue = '',
  inputLabel,
  isLoading = false,
  isOpen,
  onCancel,
  onConfirm,
  title,
}: TextInputDialogProps) {
  const titleId = useId();
  const descriptionId = useId();
  const inputId = useId();
  const [value, setValue] = useState(initialValue);

  useEffect(() => {
    if (isOpen) {
      setValue(initialValue);
    }
  }, [initialValue, isOpen]);

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

  const submit = () => {
    const nextValue = value.trim();
    if (nextValue) {
      onConfirm(nextValue);
    }
  };

  return (
    <div
      aria-labelledby={titleId}
      aria-describedby={descriptionId}
      aria-modal="true"
      className="fixed inset-0 z-50 grid place-items-center bg-background/80 p-4 backdrop-blur-sm"
      role="dialog"
    >
      <form
        className="w-full max-w-md rounded-xl border border-border bg-card p-5 shadow-lg"
        onSubmit={(event) => {
          event.preventDefault();
          submit();
        }}
      >
        <div className="space-y-2">
          <h2 className="text-lg font-semibold" id={titleId}>
            {title}
          </h2>
          <p className="text-sm leading-6 text-muted-foreground" id={descriptionId}>
            {description}
          </p>
        </div>
        <label className="mt-4 block space-y-2 text-sm font-medium" htmlFor={inputId}>
          <span>{inputLabel}</span>
          <input
            autoFocus
            className={inputClassName}
            disabled={isLoading}
            id={inputId}
            onChange={(event) => setValue(event.target.value)}
            value={value}
          />
        </label>
        <div className="mt-5 flex justify-end gap-2">
          <Button disabled={isLoading} onClick={onCancel} type="button" variant="ghost">
            {cancelLabel}
          </Button>
          <Button disabled={isLoading || value.trim().length === 0} type="submit">
            {isLoading ? <LoaderCircle aria-hidden="true" className="size-4 animate-spin" /> : null}
            {confirmLabel}
          </Button>
        </div>
      </form>
    </div>
  );
}
