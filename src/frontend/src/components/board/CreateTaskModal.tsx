import { useEffect, useMemo, useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { MemberAssigneePicker } from '@/components/board/MemberAssigneePicker';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { Button } from '@/components/ui/button';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { useChangeTaskPriorityMutation, useChangeTaskStatusMutation, useCreateTaskMutation, useAssignUserMutation } from '@/features/tasks';
import type { UserDto } from '@/features/users';
import { cn } from '@/lib/utils';

const MAX_TITLE_LENGTH = 200;
const MAX_DESCRIPTION_LENGTH = 2000;

type CreateTaskModalProps = {
  isOpen: boolean;
  onClose: () => void;
  projectId: string | null;
  defaultStatus: string;
  columnLabel: string;
  assignableUsers: UserDto[];
};

type CreateTaskState = {
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High';
  assigneeId: string;
};

const priorityToneClass: Record<CreateTaskState['priority'], string> = {
  High: 'bg-rose-500/10 text-rose-700',
  Medium: 'bg-amber-500/10 text-amber-700',
  Low: 'bg-slate-500/10 text-slate-700',
};

export function CreateTaskModal({ isOpen, onClose, projectId, defaultStatus, columnLabel, assignableUsers }: CreateTaskModalProps) {
  const createTaskMutation = useCreateTaskMutation();
  const changeStatusMutation = useChangeTaskStatusMutation();
  const changePriorityMutation = useChangeTaskPriorityMutation();
  const assignUserMutation = useAssignUserMutation();
  const [form, setForm] = useState<CreateTaskState>({ title: '', description: '', priority: 'Medium', assigneeId: '' });
  const [errors, setErrors] = useState<Partial<Record<keyof CreateTaskState, string>>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setForm({ title: '', description: '', priority: 'Medium', assigneeId: '' });
      setErrors({});
      setSubmitError(null);
    }
  }, [isOpen]);

  const trimmedTitle = useMemo(() => form.title.trim(), [form.title]);

  if (!isOpen) {
    return null;
  }

  const updateField = (field: keyof CreateTaskState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
  };

  const validate = () => {
    const nextErrors: Partial<Record<keyof CreateTaskState, string>> = {};

    if (!trimmedTitle) {
      nextErrors.title = 'Title is required.';
    } else if (trimmedTitle.length < 3) {
      nextErrors.title = 'Title must be at least 3 characters.';
    } else if (trimmedTitle.length > MAX_TITLE_LENGTH) {
      nextErrors.title = `Title must be ${MAX_TITLE_LENGTH} characters or less.`;
    }

    if (form.description.trim().length > MAX_DESCRIPTION_LENGTH) {
      nextErrors.description = `Description must be ${MAX_DESCRIPTION_LENGTH} characters or less.`;
    }

    const projectError = !projectId ? 'Select a project before creating tasks.' : null;

    setErrors(nextErrors);
    setSubmitError(projectError);
    return Object.keys(nextErrors).length === 0 && !projectError;
  };

  const handleCreate = async () => {
    if (!validate() || !projectId) {
      return;
    }

    setSubmitError(null);

    try {
      const created = await createTaskMutation.mutateAsync({
        title: trimmedTitle,
        description: form.description.trim() ? form.description.trim() : null,
        projectId,
      });

      if (defaultStatus !== 'Open') {
        await changeStatusMutation.mutateAsync({ taskId: created.id, status: defaultStatus });
      }

      if (form.priority !== 'Medium') {
        await changePriorityMutation.mutateAsync({ taskId: created.id, priority: form.priority });
      }

      if (form.assigneeId) {
        await assignUserMutation.mutateAsync({ taskId: created.id, userId: form.assigneeId });
      }

      onClose();
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Failed to create task.');
    }
  };

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-2xl border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>New ticket</CardTitle>
        <CardDescription>Create a ticket in {columnLabel}. Status will start as {defaultStatus}.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="task-title">
            Title
          </label>
          <Input
            id="task-title"
            value={form.title}
            onChange={(event) => updateField('title', event.target.value)}
            placeholder="Summarize the task..."
            maxLength={MAX_TITLE_LENGTH}
            aria-invalid={Boolean(errors.title)}
          />
          <p className="text-xs text-muted-foreground">Minimum 3, maximum {MAX_TITLE_LENGTH} characters.</p>
          {errors.title ? <p className="text-xs text-rose-700">{errors.title}</p> : null}
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="task-description">
            Description
          </label>
          <textarea
            id="task-description"
            value={form.description}
            onChange={(event) => updateField('description', event.target.value)}
            placeholder="Optional details or acceptance notes."
            maxLength={MAX_DESCRIPTION_LENGTH}
            aria-invalid={Boolean(errors.description)}
            className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
          />
          <p className="text-xs text-muted-foreground">Up to {MAX_DESCRIPTION_LENGTH} characters.</p>
          {errors.description ? <p className="text-xs text-rose-700">{errors.description}</p> : null}
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="task-priority">
            Priority
          </label>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" className="h-11 w-full justify-between border-border/70 bg-background/80 shadow-sm">
                <span className={cn('rounded-full px-2.5 py-1 text-xs font-medium', priorityToneClass[form.priority])}>
                  {form.priority}
                </span>
                <ChevronDown className="h-4 w-4 text-muted-foreground" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" className="min-w-[12rem]">
              {(['High', 'Medium', 'Low'] as const).map((priority) => (
                <DropdownMenuItem
                  key={priority}
                  className="py-1.5"
                  onClick={() => updateField('priority', priority)}
                >
                  <span className={cn('rounded-full px-2.5 py-1 text-xs font-medium', priorityToneClass[priority])}>
                    {priority}
                  </span>
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="task-assignee">
            Assignee
          </label>
          <MemberAssigneePicker
            members={assignableUsers}
            selectedAssigneeId={form.assigneeId}
            onAssign={(userId) => updateField('assigneeId', userId)}
            onRemove={() => updateField('assigneeId', '')}
            searchInputId="task-assignee"
          />
          <p className="text-xs text-muted-foreground">
            Only project members can be assigned.
          </p>
        </div>

        {submitError ? <p className="text-sm text-rose-700">{submitError}</p> : null}

        <FormActionButtons
          onCancel={onClose}
          confirmLabel={
            createTaskMutation.isPending || changeStatusMutation.isPending || changePriorityMutation.isPending || assignUserMutation.isPending
              ? 'Creating...'
              : 'Create ticket'
          }
          onConfirm={handleCreate}
          confirmDisabled={createTaskMutation.isPending || changeStatusMutation.isPending || changePriorityMutation.isPending || assignUserMutation.isPending}
        />
      </CardContent>
    </BacklogModal>
  );
}
