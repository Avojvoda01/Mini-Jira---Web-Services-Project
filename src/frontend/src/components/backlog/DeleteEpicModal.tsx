import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type DeleteEpicModalProps = {
  isOpen: boolean;
  epicName: string;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
};

export function DeleteEpicModal({ isOpen, epicName, onClose, onConfirm, isPending }: DeleteEpicModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-md border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Delete epic?</CardTitle>
        <CardDescription>
          This will remove <span className="font-medium text-foreground">{epicName}</span>. Assigned tickets will remain in backlog as unassigned.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <FormActionButtons onCancel={onClose} confirmLabel="Delete epic" onConfirm={onConfirm} confirmDisabled={isPending} />
      </CardContent>
    </BacklogModal>
  );
}