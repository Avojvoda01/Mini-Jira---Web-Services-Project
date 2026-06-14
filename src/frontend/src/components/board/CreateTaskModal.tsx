import { useEffect, useMemo, useState } from 'react';
import { ChevronDown, Layers } from 'lucide-react';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { MemberAssigneePicker } from '@/components/board/MemberAssigneePicker';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { Button } from '@/components/ui/button';
import { CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { useChangeTaskPriorityMutation, useChangeTaskStatusMutation, useCreateTaskMutation, useAssignUserMutation, useAssignEpicMutation, useSetEstimateMutation } from '@/features/tasks';
import type { EpicDto } from '@/features/epics';
import type { UserDto } from '@/features/users';
import { isValidEstimate, parseEstimate } from '@/lib/estimate';
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
  epics: EpicDto[];
};

type CreateTaskState = {
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High';
  assigneeId: string;
  epicId: string;
  estimate: string;
};

const priorityToneClass: Record<CreateTaskState['priority'], string> = {
  High: 'bg-rose-500/10 text-rose-700',
  Medium: 'bg-amber-500/10 text-amber-700',
  Low: 'bg-slate-500/10 text-slate-700',
};

export function CreateTaskModal({ isOpen, onClose, projectId, defaultStatus, columnLabel, assignableUsers, epics }: CreateTaskModalProps) {
  const createTaskMutation = useCreateTaskMutation();
  const changeStatusMutation = useChangeTaskStatusMutation();
  const changePriorityMutation = useChangeTaskPriorityMutation();
  const assignUserMutation = useAssignUserMutation();
  const assignEpicMutation = useAssignEpicMutation();
  const setEstimateMutation = useSetEstimateMutation();
  const [form, setForm] = useState<CreateTaskState>({ title: '', description: '', priority: 'Medium', assigneeId: '', epicId: '', estimate: '' });
  const [errors, setErrors] = useState<Partial<Record<keyof CreateTaskState, string>>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setForm({ title: '', description: '', priority: 'Medium', assigneeId: '', epicId: '', estimate: '' });
      setErrors({});
      setSubmitError(null);
    }
  }, [isOpen]);

  const trimmedTitle = useMemo(() => form.title.trim(), [form.title]);
  const selectedEpic = useMemo(() => epics.find((e) => e.id === form.epicId) ?? null, [epics, form.epicId]);

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

    if (form.estimate.trim() && !isValidEstimate(form.estimate)) {
      nextErrors.estimate = 'Use a format like 10m, 2h, or 1d.';
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

      if (form.epicId) {
        await assignEpicMutation.mutateAsync({ taskId: created.id, epicId: form.epicId });
      }

      const estimateMinutes = form.estimate.trim() ? parseEstimate(form.estimate) : null;
      if (estimateMinutes !== null) {
        await setEstimateMutation.mutateAsync({ taskId: created.id, estimateMinutes });
      }

      onClose();
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Failed to create task.');
    }
  };

  const isBusy = createTaskMutation.isPending || changeStatusMutation.isPending || changePriorityMutation.isPending || assignUserMutation.isPending || assignEpicMutation.isPending || setEstimateMutation.isPending;

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-2xl border-border/70 bg-card shadow-2xl">
      <CardHeader>
        <CardTitle>New ticket</CardTitle>
        <CardDescription>Create a ticket in {columnLabel}. Status will start as {defaultStatus}.</CardDescription>
      </CardHeader>
      <CardContent className="overflow-y-auto space-y-5">
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

        <div className="grid gap-4 sm:grid-cols-2">
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
                  <DropdownMenuItem key={priority} className="py-1.5" onClick={() => updateField('priority', priority)}>
                    <span className={cn('rounded-full px-2.5 py-1 text-xs font-medium', priorityToneClass[priority])}>
                      {priority}
                    </span>
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium text-foreground" htmlFor="task-epic">
              Epic
            </label>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="h-11 w-full justify-between border-border/70 bg-background/80 shadow-sm">
                  <span className="flex items-center gap-1.5 truncate text-sm">
                    {selectedEpic ? (
                      <>
                        <Layers className="h-3.5 w-3.5 shrink-0 text-primary" />
                        <span className="truncate font-medium text-foreground">{selectedEpic.name}</span>
                      </>
                    ) : (
                      <span className="text-muted-foreground">No epic</span>
                    )}
                  </span>
                  <ChevronDown className="h-4 w-4 shrink-0 text-muted-foreground" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="start" className="min-w-[14rem]">
                <DropdownMenuItem className="py-1.5 text-muted-foreground" onClick={() => updateField('epicId', '')}>
                  No epic
                </DropdownMenuItem>
                {epics.map((epic) => (
                  <DropdownMenuItem key={epic.id} className="py-1.5" onClick={() => updateField('epicId', epic.id)}>
                    <Layers className="mr-2 h-3.5 w-3.5 text-primary" />
                    {epic.name}
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
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

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="task-estimate">
            Estimate
          </label>
          <Input
            id="task-estimate"
            value={form.estimate}
            onChange={(event) => updateField('estimate', event.target.value)}
            placeholder="e.g. 10m, 2h, 1d"
            aria-invalid={Boolean(errors.estimate)}
          />
          <p className="text-xs text-muted-foreground">Use m (minutes), h (hours), or d (days — 8h each).</p>
          {errors.estimate ? <p className="text-xs text-rose-700">{errors.estimate}</p> : null}
        </div>

        {submitError ? <p className="text-sm text-rose-700">{submitError}</p> : null}
      </CardContent>
      <CardFooter className="justify-end">
        <FormActionButtons
          onCancel={onClose}
          confirmLabel={isBusy ? 'Creating...' : 'Create ticket'}
          onConfirm={handleCreate}
          confirmDisabled={isBusy}
        />
      </CardFooter>
    </BacklogModal>
  );
}
