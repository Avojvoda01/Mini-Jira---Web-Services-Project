import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type DeleteTaskModalProps = {
  isOpen: boolean;
  taskTitle: string;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
};

export function DeleteTaskModal({ isOpen, taskTitle, onClose, onConfirm, isPending }: DeleteTaskModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-md border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Delete ticket?</CardTitle>
        <CardDescription>
          This will remove <span className="font-medium text-foreground">{taskTitle}</span> and its activity history.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <FormActionButtons onCancel={onClose} confirmLabel="Delete ticket" onConfirm={onConfirm} confirmDisabled={isPending} />
      </CardContent>
    </BacklogModal>
  );
}
