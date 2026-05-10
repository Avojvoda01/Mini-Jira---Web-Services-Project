import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type DeleteProjectModalProps = {
  isOpen: boolean;
  projectName: string;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
};

export function DeleteProjectModal({ isOpen, projectName, onClose, onConfirm, isPending }: DeleteProjectModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-md border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Delete project?</CardTitle>
        <CardDescription>
          This will remove <span className="font-medium text-foreground">{projectName}</span> and its workspace data.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <FormActionButtons onCancel={onClose} confirmLabel="Delete project" onConfirm={onConfirm} confirmDisabled={isPending} />
      </CardContent>
    </BacklogModal>
  );
}
