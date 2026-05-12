import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type DeleteCommentModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
};

export function DeleteCommentModal({ isOpen, onClose, onConfirm, isPending }: DeleteCommentModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-md border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Delete comment?</CardTitle>
        <CardDescription>Are you sure you want to delete this comment?</CardDescription>
      </CardHeader>
      <CardContent>
        <FormActionButtons onCancel={onClose} confirmLabel="Delete comment" onConfirm={onConfirm} confirmDisabled={isPending} />
      </CardContent>
    </BacklogModal>
  );
}
