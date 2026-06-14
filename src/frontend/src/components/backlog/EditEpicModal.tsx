import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';

type EditEpicModalProps = {
  isOpen: boolean;
  epicName: string;
  epicDescription: string;
  onClose: () => void;
  onChangeName: (value: string) => void;
  onChangeDescription: (value: string) => void;
  onSave: () => void;
  isPending: boolean;
};

export function EditEpicModal({
  isOpen,
  epicName,
  epicDescription,
  onClose,
  onChangeName,
  onChangeDescription,
  onSave,
  isPending,
}: EditEpicModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-2xl border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>Edit epic</CardTitle>
        <CardDescription>Update the epic details below.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5 overflow-y-auto">
        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="edit-epic-name">
            Epic name
          </label>
          <Input
            id="edit-epic-name"
            value={epicName}
            onChange={(event) => onChangeName(event.target.value)}
            placeholder="Enter epic name"
            maxLength={100}
          />
          <p className="text-xs text-muted-foreground">Minimum 3, maximum 100 characters.</p>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="edit-epic-description">
            Epic description
          </label>
          <textarea
            id="edit-epic-description"
            value={epicDescription}
            onChange={(event) => onChangeDescription(event.target.value)}
            placeholder="Describe the scope and expected outcome."
            maxLength={2000}
            className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
        </div>

      </CardContent>
      <CardFooter className="justify-end">
        <FormActionButtons
          onCancel={onClose}
          confirmLabel="Save changes"
          onConfirm={onSave}
          confirmDisabled={epicName.trim().length < 3 || isPending}
        />
      </CardFooter>
    </BacklogModal>
  );
}