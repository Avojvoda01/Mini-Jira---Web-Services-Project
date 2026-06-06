import { useState } from 'react';
import { TriangleAlert } from 'lucide-react';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { Button } from '@/components/ui/button';
import { CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';

type DeleteProjectModalProps = {
  isOpen: boolean;
  projectName: string;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
};

export function DeleteProjectModal({ isOpen, projectName, onClose, onConfirm, isPending }: DeleteProjectModalProps) {
  const [confirmation, setConfirmation] = useState('');

  if (!isOpen) {
    return null;
  }

  const handleClose = () => {
    setConfirmation('');
    onClose();
  };

  const handleConfirm = () => {
    if (confirmation !== projectName) return;
    onConfirm();
    setConfirmation('');
  };

  return (
    <BacklogModal onClose={handleClose} cardClassName="w-full max-w-md border-destructive/40 bg-card shadow-2xl">
      <CardHeader className="space-y-3">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-destructive/10">
            <TriangleAlert className="h-5 w-5 text-destructive" />
          </div>
          <CardTitle className="text-destructive">Delete project</CardTitle>
        </div>
        <div className="rounded-lg border border-destructive/25 bg-destructive/5 px-4 py-3 text-sm text-muted-foreground">
          This will <span className="font-semibold text-foreground">permanently delete</span>{' '}
          <span className="font-semibold text-foreground">{projectName}</span> including all tasks, epics, and comments.
          This action cannot be undone.
        </div>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="space-y-1.5">
          <label className="text-sm text-muted-foreground" htmlFor="delete-confirmation">
            Type <span className="font-medium text-foreground">{projectName}</span> to confirm
          </label>
          <Input
            id="delete-confirmation"
            value={confirmation}
            onChange={(e) => setConfirmation(e.target.value)}
            placeholder={projectName}
            autoComplete="off"
            aria-invalid={confirmation.length > 0 && confirmation !== projectName}
          />
        </div>

        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={handleClose} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={confirmation !== projectName || isPending}
          >
            {isPending ? 'Deleting…' : 'Delete project'}
          </Button>
        </div>
      </CardContent>
    </BacklogModal>
  );
}
