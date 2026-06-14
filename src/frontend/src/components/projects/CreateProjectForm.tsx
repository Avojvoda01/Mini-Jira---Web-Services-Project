import { useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from 'react';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { ProjectMemberPicker } from '@/components/projects/ProjectMemberPicker';
import { useAddProjectMemberMutation, useCreateProjectMutation } from '@/features/projects';
import type { UserDto } from '@/features/users';

type CreateProjectProps = {
  open: boolean;
  onClose: () => void;
  members: UserDto[];
  currentUserId?: string;
};

type CreateProjectState = {
  name: string;
  description: string;
};

const MAX_PROJECT_NAME_LENGTH = 100;
const MAX_DESCRIPTION_LENGTH = 2000;

export function CreateProjectForm({ open, onClose, members, currentUserId }: CreateProjectProps) {
  const pickableMembers = currentUserId
    ? members.filter((m) => m.id.toLowerCase() !== currentUserId.toLowerCase())
    : members;
  const createProjectMutation = useCreateProjectMutation();
  const addProjectMemberMutation = useAddProjectMemberMutation();
  const dialogRef = useRef<HTMLDivElement | null>(null);
  const nameInputRef = useRef<HTMLInputElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const [form, setForm] = useState<CreateProjectState>({
    name: '',
    description: '',
  });
  const [selectedMemberIds, setSelectedMemberIds] = useState<string[]>([]);
  const [errors, setErrors] = useState<Partial<Record<keyof CreateProjectState, string>>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      setForm({
        name: '',
        description: '',
      });
      setSelectedMemberIds([]);
      setErrors({});
      setSubmitError(null);
      return;
    }
  }, [open]);

  useEffect(() => {
    if (!open) {
      previousFocusRef.current?.focus();
      previousFocusRef.current = null;
      return;
    }

    previousFocusRef.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;

    const focusTimer = window.setTimeout(() => {
      nameInputRef.current?.focus();
    }, 0);

    return () => {
      window.clearTimeout(focusTimer);
    };
  }, [open]);

  if (!open) {
    return null;
  }

  const updateField = (field: keyof CreateProjectState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
  };

  const validate = () => {
    const nextErrors: Partial<Record<keyof CreateProjectState, string>> = {};

    const trimmedName = form.name.trim();

    if (!trimmedName) {
      nextErrors.name = 'Project name is required.';
    } else if (trimmedName.length < 3) {
      nextErrors.name = 'Project name must be at least 3 characters.';
    } else if (trimmedName.length > MAX_PROJECT_NAME_LENGTH) {
      nextErrors.name = `Project name must be ${MAX_PROJECT_NAME_LENGTH} characters or less.`;
    }

    if (!form.description.trim()) {
      nextErrors.description = 'Description is required.';
    } else if (form.description.trim().length > MAX_DESCRIPTION_LENGTH) {
      nextErrors.description = `Description must be ${MAX_DESCRIPTION_LENGTH} characters or less.`;
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validate()) {
      return;
    }
    setSubmitError(null);

    createProjectMutation
      .mutateAsync({
        name: form.name.trim(),
        description: form.description.trim(),
      })
      .then(async (project) => {
        if (selectedMemberIds.length > 0) {
          await Promise.all(
            selectedMemberIds.map((userId) =>
              addProjectMemberMutation.mutateAsync({
                projectId: project.id,
                userId,
                role: 'Member',
              }),
            ),
          );
        }

        onClose();
      })
      .catch((error) => {
        setSubmitError(error instanceof Error ? error.message : 'Failed to create project.');
      });
  };

  const handleDialogKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.preventDefault();
      onClose();
      return;
    }

    if (event.key !== 'Tab') {
      return;
    }

    const dialogElement = dialogRef.current;
    if (!dialogElement) {
      return;
    }

    const focusableElements = Array.from(
      dialogElement.querySelectorAll<HTMLElement>(
        'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
      ),
    );

    if (focusableElements.length === 0) {
      return;
    }

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];
    const activeElement = document.activeElement;

    if (event.shiftKey && activeElement === firstElement) {
      event.preventDefault();
      lastElement.focus();
      return;
    }

    if (!event.shiftKey && activeElement === lastElement) {
      event.preventDefault();
      firstElement.focus();
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6"
      role="dialog"
      aria-modal="true"
      aria-labelledby="create-project-title"
      onClick={onClose}
      onKeyDown={handleDialogKeyDown}
    >
      <Card ref={dialogRef} className="flex max-h-[90dvh] w-full max-w-2xl flex-col overflow-hidden border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
        <CardHeader>
          <CardTitle id="create-project-title" className="text-2xl tracking-tight">
            Create project
          </CardTitle>
          <CardDescription>Fill out the project details below to add a new workspace.</CardDescription>
        </CardHeader>

        <CardContent className="overflow-y-auto">
          <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="name">
                Project name
              </label>
              <Input
                id="name"
                ref={nameInputRef}
                value={form.name}
                onChange={(event) => updateField('name', event.target.value)}
                placeholder="Enter a project name"
                maxLength={MAX_PROJECT_NAME_LENGTH}
                aria-invalid={Boolean(errors.name)}
              />
              <p className="text-xs text-muted-foreground">Up to {MAX_PROJECT_NAME_LENGTH} characters.</p>
              {errors.name ? <p className="text-xs text-rose-700">{errors.name}</p> : null}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="description">
                Description
              </label>
              <textarea
                id="description"
                value={form.description}
                onChange={(event) => updateField('description', event.target.value)}
                placeholder="Describe the project goal and scope."
                maxLength={MAX_DESCRIPTION_LENGTH}
                aria-invalid={Boolean(errors.description)}
                className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
              />
              <p className="text-xs text-muted-foreground">Up to {MAX_DESCRIPTION_LENGTH} characters.</p>
              {errors.description ? <p className="text-xs text-rose-700">{errors.description}</p> : null}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="project-members">
                Members
              </label>
              <ProjectMemberPicker
                members={pickableMembers}
                selectedMemberIds={selectedMemberIds}
                onAdd={(userId) => setSelectedMemberIds((current) => [...current, userId])}
                onRemove={(userId) => setSelectedMemberIds((current) => current.filter((memberId) => memberId !== userId))}
                searchInputId="project-members"
                isBusy={createProjectMutation.isPending || addProjectMemberMutation.isPending}
              />
              <p className="text-xs text-muted-foreground">Choose project members now, or add them later from the project edit form.</p>
            </div>

            {submitError ? <p className="text-sm text-rose-700">{submitError}</p> : null}

            <FormActionButtons
              onCancel={onClose}
              confirmLabel={createProjectMutation.isPending || addProjectMemberMutation.isPending ? 'Creating...' : 'Create'}
              confirmType="submit"
              confirmDisabled={createProjectMutation.isPending || addProjectMemberMutation.isPending}
            />
          </form>
        </CardContent>
      </Card>
    </div>
  );
}