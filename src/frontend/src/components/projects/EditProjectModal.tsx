import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';

const MAX_PROJECT_NAME_LENGTH = 20;
const MAX_DESCRIPTION_LENGTH = 1000;

type EditProjectModalProps = {
  isOpen: boolean;
  projectName: string;
  projectDescription: string;
  onClose: () => void;
  onChangeName: (value: string) => void;
  onChangeDescription: (value: string) => void;
  onSave: () => void;
  isPending: boolean;
};

export function EditProjectModal({
  isOpen,
  projectName,
  projectDescription,
  onClose,
  onChangeName,
  onChangeDescription,
  onSave,
  isPending,
}: EditProjectModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-2xl border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Edit project</CardTitle>
        <CardDescription>Update the project details below.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="edit-project-name">
            Project name
          </label>
          <Input
            id="edit-project-name"
            value={projectName}
            onChange={(event) => onChangeName(event.target.value)}
            placeholder="Enter project name"
            maxLength={MAX_PROJECT_NAME_LENGTH}
          />
          <p className="text-xs text-muted-foreground">Up to {MAX_PROJECT_NAME_LENGTH} characters.</p>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="edit-project-description">
            Description
          </label>
          <textarea
            id="edit-project-description"
            value={projectDescription}
            onChange={(event) => onChangeDescription(event.target.value)}
            placeholder="Describe the project goal and scope."
            maxLength={MAX_DESCRIPTION_LENGTH}
            className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
          <p className="text-xs text-muted-foreground">Up to {MAX_DESCRIPTION_LENGTH} characters.</p>
        </div>

        <FormActionButtons
          onCancel={onClose}
          confirmLabel="Save changes"
          onConfirm={onSave}
          confirmDisabled={projectName.trim().length < 3 || !projectDescription.trim() || isPending}
        />
      </CardContent>
    </BacklogModal>
  );
}
