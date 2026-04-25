import { Button } from '@/components/ui/button';

type FormActionButtonsProps = {
  onCancel: () => void;
  cancelLabel?: string;
  confirmLabel?: string;
  onConfirm?: () => void;
  confirmType?: 'button' | 'submit';
  confirmDisabled?: boolean;
};

export function FormActionButtons({
  onCancel,
  cancelLabel = 'Cancel',
  confirmLabel = 'Create',
  onConfirm,
  confirmType = 'button',
  confirmDisabled = false,
}: FormActionButtonsProps) {
  return (
    <div className="flex justify-end gap-2">
      <Button
        variant="outline"
        className="border-rose-300 bg-rose-50 text-rose-700 hover:bg-rose-100 hover:text-rose-800"
        onClick={onCancel}
        type="button"
      >
        {cancelLabel}
      </Button>
      <Button
        className="border-0 bg-sky-500 text-white hover:bg-sky-600"
        onClick={onConfirm}
        type={confirmType}
        disabled={confirmDisabled}
      >
        {confirmLabel}
      </Button>
    </div>
  );
}
