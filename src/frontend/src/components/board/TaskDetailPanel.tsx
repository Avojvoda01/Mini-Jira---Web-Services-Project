import { ArrowLeftRight, Check, Minus, Plus, UserPlus, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { MemberAssigneePicker } from '@/components/board/MemberAssigneePicker';
import { TaskComments } from '@/components/board/TaskComments';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import type { EpicDto } from '@/features/epics';
import {
  type TaskItem,
  useAssignUserMutation,
  useSetEstimateMutation,
} from '@/features/tasks';
import type { UserDto } from '@/features/users';
import {
  formatEstimate,
  minutesToEditValue,
  parseEstimate,
} from '@/lib/estimate';
import { cn } from '@/lib/utils';
import type { AuthSession } from '@/store/authAtoms';
import {
  priorityBadgeClass,
  priorityLabelMap,
  statusLabelMap,
} from './boardModel';

type TaskDetailPanelProps = {
  task: TaskItem;
  ticketId: string;
  assignableUsers: UserDto[];
  epics: EpicDto[];
  session: AuthSession | null;
  isEditModalOpen: boolean;
  resolveUserDisplayName: (userId: string | null) => string;
  onClose: () => void;
  onEdit: () => void;
  onRequestDelete: () => void;
};

export function TaskDetailPanel({
  task,
  ticketId,
  assignableUsers,
  epics,
  session,
  isEditModalOpen,
  resolveUserDisplayName,
  onClose,
  onEdit,
  onRequestDelete,
}: TaskDetailPanelProps) {
  const assignUserMutation = useAssignUserMutation();
  const setEstimateMutation = useSetEstimateMutation();

  const [isAssigneePickerOpen, setIsAssigneePickerOpen] = useState(false);
  const [assigneeError, setAssigneeError] = useState<string | null>(null);
  const [isEstimateEditing, setIsEstimateEditing] = useState(false);
  const [estimateDraft, setEstimateDraft] = useState('');
  const [estimateError, setEstimateError] = useState<string | null>(null);

  useEffect(() => {
    setIsAssigneePickerOpen(false);
    setAssigneeError(null);
    setIsEstimateEditing(false);
    setEstimateDraft('');
    setEstimateError(null);
  }, [task.id]);

  const handleAssignUser = async (userId: string) => {
    setAssigneeError(null);

    try {
      await assignUserMutation.mutateAsync({ taskId: task.id, userId });
      setIsAssigneePickerOpen(false);
    } catch (error) {
      setAssigneeError(
        error instanceof Error ? error.message : 'Unable to update assignee.',
      );
    }
  };

  const handleRemoveUser = async () => {
    setAssigneeError(null);

    try {
      await assignUserMutation.mutateAsync({ taskId: task.id, userId: '' });
      setIsAssigneePickerOpen(false);
    } catch (error) {
      setAssigneeError(
        error instanceof Error ? error.message : 'Unable to update assignee.',
      );
    }
  };

  const saveEstimate = async () => {
    const minutes = estimateDraft.trim() ? parseEstimate(estimateDraft) : null;
    if (estimateDraft.trim() && minutes === null) {
      setEstimateError('Use 10m, 2h, or 1d.');
      return;
    }
    await setEstimateMutation.mutateAsync({
      taskId: task.id,
      estimateMinutes: minutes,
    });
    setIsEstimateEditing(false);
  };

  const requestClose = () => {
    if (isEditModalOpen) {
      return;
    }

    onClose();
  };

  return createPortal(
    <>
      <button
        type="button"
        className="fixed inset-0 z-30 bg-black/30"
        aria-label="Close task details"
        onClick={requestClose}
      />
      <aside className="fixed right-0 top-0 z-40 flex h-full w-full max-w-[34rem] flex-col border-l border-border/70 bg-background/95 shadow-2xl backdrop-blur-sm">
        <div className="flex items-start justify-between gap-4 border-b border-border/70 p-5">
          <div className="space-y-2">
            <Badge
              variant="outline"
              className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground"
            >
              {ticketId}
            </Badge>
            <h2 className="text-lg font-semibold leading-7 text-foreground">
              {task.title}
            </h2>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={onEdit}>
              Edit
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={requestClose}
              aria-label="Close task details"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <div className="flex-1 space-y-6 overflow-y-auto p-5">
          <div className="flex flex-wrap items-center gap-2">
            <Badge
              variant="outline"
              className="border-border/70 bg-background/70 text-muted-foreground"
            >
              {statusLabelMap[task.status]}
            </Badge>
            <Badge
              className={priorityBadgeClass(priorityLabelMap[task.priority])}
            >
              {priorityLabelMap[task.priority]}
            </Badge>
          </div>

          <div className="grid gap-3 text-sm text-muted-foreground">
            <div className="flex items-center justify-between gap-2">
              <span>Assignee</span>
              <div className="flex items-center gap-2">
                <span className="text-foreground">
                  {resolveUserDisplayName(task.assigneeId)}
                </span>
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  className={cn(
                    'h-8 w-8',
                    isAssigneePickerOpen
                      ? 'border-border/70 text-foreground hover:bg-background/80'
                      : '',
                  )}
                  onClick={() => setIsAssigneePickerOpen((current) => !current)}
                  aria-label={
                    isAssigneePickerOpen
                      ? 'Close assignee picker'
                      : 'Change assignee'
                  }
                >
                  {isAssigneePickerOpen ? (
                    <Minus className="h-4 w-4" />
                  ) : task.assigneeId ? (
                    <ArrowLeftRight className="h-4 w-4" />
                  ) : (
                    <UserPlus className="h-4 w-4" />
                  )}
                </Button>
              </div>
            </div>
            {isAssigneePickerOpen ? (
              <div className="rounded-2xl border border-border/70 bg-background/80 p-3">
                <MemberAssigneePicker
                  members={assignableUsers}
                  selectedAssigneeId={task.assigneeId ?? ''}
                  onAssign={handleAssignUser}
                  onRemove={handleRemoveUser}
                  searchInputId="detail-task-assignee"
                  isBusy={assignUserMutation.isPending}
                />
                <p className="mt-2 text-xs text-muted-foreground">
                  Only project members can be assigned.
                </p>
                {assigneeError ? (
                  <p className="mt-2 text-xs text-rose-700">{assigneeError}</p>
                ) : null}
              </div>
            ) : null}
            {task.epicId && (
              <div className="flex items-center justify-between gap-2">
                <span>Epic</span>
                <span className="text-foreground">
                  {epics.find((e) => e.id === task.epicId)?.name ?? 'Unknown'}
                </span>
              </div>
            )}
            <div className="flex items-center justify-between gap-2">
              <span>Estimate</span>
              <div className="flex items-center gap-2">
                {isEstimateEditing ? (
                  <div className="flex items-center gap-1.5">
                    <Input
                      className="h-7 w-24 text-xs"
                      value={estimateDraft}
                      onChange={(e) => {
                        setEstimateDraft(e.target.value);
                        setEstimateError(null);
                      }}
                      onKeyDown={async (e) => {
                        if (e.key === 'Enter') {
                          await saveEstimate();
                        } else if (e.key === 'Escape') {
                          setIsEstimateEditing(false);
                          setEstimateError(null);
                        }
                      }}
                      placeholder="10m, 2h, 1d"
                      autoFocus
                    />
                    <Button
                      type="button"
                      size="icon"
                      variant="ghost"
                      className="h-7 w-7"
                      onClick={saveEstimate}
                      disabled={setEstimateMutation.isPending}
                      aria-label="Save estimate"
                    >
                      <Check className="h-3.5 w-3.5" />
                    </Button>
                    <Button
                      type="button"
                      size="icon"
                      variant="ghost"
                      className="h-7 w-7"
                      onClick={() => {
                        setIsEstimateEditing(false);
                        setEstimateError(null);
                      }}
                      aria-label="Cancel estimate"
                    >
                      <Minus className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                ) : (
                  <>
                    <span
                      className={cn(
                        'text-foreground',
                        !task.estimateMinutes && 'text-muted-foreground',
                      )}
                    >
                      {formatEstimate(task.estimateMinutes)}
                    </span>
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => {
                        setEstimateDraft(
                          minutesToEditValue(task.estimateMinutes),
                        );
                        setEstimateError(null);
                        setIsEstimateEditing(true);
                      }}
                      aria-label={
                        task.estimateMinutes
                          ? 'Change estimate'
                          : 'Add estimate'
                      }
                    >
                      {task.estimateMinutes ? (
                        <ArrowLeftRight className="h-4 w-4" />
                      ) : (
                        <Plus className="h-4 w-4" />
                      )}
                    </Button>
                  </>
                )}
              </div>
            </div>
            {isEstimateEditing && estimateError ? (
              <p className="text-xs text-rose-700">{estimateError}</p>
            ) : null}
            <div className="flex items-center justify-between gap-2">
              <span>Created by</span>
              <span className="text-foreground">
                {resolveUserDisplayName(task.createdById)}
              </span>
            </div>
            {task.updatedById && (
              <div className="flex items-center justify-between gap-2">
                <span>Last edited by</span>
                <span className="text-foreground">
                  {resolveUserDisplayName(task.updatedById)}
                </span>
              </div>
            )}
          </div>

          <div className="space-y-2">
            <p className="text-sm font-medium text-foreground">Description</p>
            <p className="whitespace-pre-wrap text-sm leading-6 text-muted-foreground break-words">
              {task.description?.trim()
                ? task.description
                : 'No description yet.'}
            </p>
          </div>

          <TaskComments
            taskId={task.id}
            session={session}
            resolveUserDisplayName={resolveUserDisplayName}
          />
        </div>

        <div className="flex items-center justify-end border-t border-border/70 p-4">
          <Button variant="destructive" onClick={onRequestDelete}>
            Delete
          </Button>
        </div>
      </aside>
    </>,
    document.body,
  );
}
