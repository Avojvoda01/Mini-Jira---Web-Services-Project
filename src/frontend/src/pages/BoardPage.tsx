import { useEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useAtomValue } from 'jotai';
import { useParams } from 'react-router-dom';
import { ArrowLeftRight, Bot, Check, Minus, Pencil, Plus, SendHorizontal, UserPlus, X } from 'lucide-react';
import { CreateTaskModal } from '@/components/board/CreateTaskModal';
import { DeleteCommentModal } from '@/components/board/DeleteCommentModal';
import { DeleteTaskModal } from '@/components/board/DeleteTaskModal';
import { EditTaskModal } from '@/components/board/EditTaskModal';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorState } from '@/components/ui/ErrorState';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { useCommentsQuery, useCreateCommentMutation, useDeleteCommentMutation, useUpdateCommentMutation } from '@/features/comments';
import { useEpicsQuery } from '@/features/epics';
import { useProjectQuery } from '@/features/projects';
import { useAssignUserMutation, useDeleteTaskMutation, useSetEstimateMutation, useTasksQuery, type TaskItem, type TaskPriority } from '@/features/tasks';
import { useUsersQuery } from '@/features/users';
import { MemberAssigneePicker } from '@/components/board/MemberAssigneePicker';
import { formatEstimate, minutesToEditValue, parseEstimate } from '@/lib/estimate';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';

type ChatMessage = {
  id: number;
  role: 'user' | 'assistant';
  text: string;
};

type TaskCard = {
  taskId: string;
  ticket: string;
  title: string;
  description: string;
  owner: string;
  priority: 'High' | 'Medium' | 'Low' | 'Unknown';
  estimate: string;
};

type BoardColumn = {
  id: 'ready' | 'in-progress' | 'review' | 'done';
  title: string;
  description: string;
  tasks: TaskCard[];
};

const priorityLabelMap: Record<TaskPriority, TaskCard['priority']> = {
  high: 'High',
  medium: 'Medium',
  low: 'Low',
  unknown: 'Unknown',
};

const statusLabelMap: Record<TaskItem['status'], string> = {
  todo: 'Open',
  'in-progress': 'In Progress',
  review: 'Review',
  done: 'Done',
  unknown: 'Open',
};

const priorityBorderClass = (priority: TaskCard['priority']) => {
  if (priority === 'High') return 'border-l-rose-500';
  if (priority === 'Medium') return 'border-l-amber-500';
  if (priority === 'Low') return 'border-l-slate-400';
  return 'border-l-border/60';
};

const priorityBadgeClass = (priority: TaskCard['priority']) => {
  if (priority === 'High') {
    return 'bg-rose-500/10 text-rose-700 hover:bg-rose-500/10';
  }
  if (priority === 'Medium') {
    return 'bg-amber-500/10 text-amber-700 hover:bg-amber-500/10';
  }
  if (priority === 'Low') {
    return 'bg-slate-500/10 text-slate-700 hover:bg-slate-500/10';
  }

  return 'bg-muted text-muted-foreground hover:bg-muted';
};

const columnConfig: Array<Omit<BoardColumn, 'tasks'>> = [
  {
    id: 'ready',
    title: 'Ready',
    description: 'Ready for triage and sizing.',
  },
  {
    id: 'in-progress',
    title: 'In progress',
    description: 'Actively being implemented.',
  },
  {
    id: 'review',
    title: 'Review',
    description: 'Awaiting review before closing.',
  },
  {
    id: 'done',
    title: 'Done',
    description: 'Shipped and ready to verify.',
  },
];

const columnStatusMap: Record<BoardColumn['id'], string> = {
  ready: 'Open',
  'in-progress': 'In Progress',
  review: 'Review',
  done: 'Done',
};

const MAX_TASK_DESCRIPTION_LENGTH = 145;

const truncateText = (value: string, maxLength: number) => {
  const trimmed = value.trim();
  if (!trimmed) {
    return '';
  }

  if (trimmed.length <= maxLength) {
    return trimmed;
  }

  return `${trimmed.slice(0, maxLength).trim()}...`;
};

export function BoardPage() {
  const session = useAtomValue(authSessionAtom);
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  const { data: project } = useProjectQuery(projectId ?? null);
  const { data: users = [] } = useUsersQuery();
  const { data: tasks = [], isLoading, isError, error, refetch } = useTasksQuery({
    projectId: projectId ?? null,
  });
  const { data: epics = [] } = useEpicsQuery({ projectId: projectId ?? null });
  const createCommentMutation = useCreateCommentMutation();
  const updateCommentMutation = useUpdateCommentMutation();
  const deleteCommentMutation = useDeleteCommentMutation();
  const deleteTaskMutation = useDeleteTaskMutation();
  const assignUserMutation = useAssignUserMutation();
  const setEstimateMutation = useSetEstimateMutation();
  const [createColumnId, setCreateColumnId] = useState<BoardColumn['id'] | null>(null);
  const [editTaskId, setEditTaskId] = useState<string | null>(null);
  const [detailTaskId, setDetailTaskId] = useState<string | null>(null);
  const [deleteTaskId, setDeleteTaskId] = useState<string | null>(null);
  const [commentDraftByTask, setCommentDraftByTask] = useState<Record<string, string>>({});
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [commentEditDrafts, setCommentEditDrafts] = useState<Record<string, string>>({});
  const [deleteCommentId, setDeleteCommentId] = useState<string | null>(null);
  const [isAssistantOpen, setIsAssistantOpen] = useState(false);
  const [isAssigneePickerOpen, setIsAssigneePickerOpen] = useState(false);
  const [assigneeError, setAssigneeError] = useState<string | null>(null);
  const [isEstimateEditing, setIsEstimateEditing] = useState(false);
  const [estimateDraft, setEstimateDraft] = useState('');
  const [estimateError, setEstimateError] = useState<string | null>(null);
  const [input, setInput] = useState('');
  const nextMessageIdRef = useRef(2);
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 1,
      role: 'assistant',
      text: 'I can help sort tasks, spot blockers, or summarize this board once the data is connected.',
    },
  ]);

    const sendMessage = async () => {
        const text = input.trim();
        if (!text) return;

        setMessages((current) => [
            ...current,
            {
                id: nextMessageIdRef.current++,
                role: "user",
                text,
            },
        ]);

        setInput("");

        const response = await fetch("/api/chats", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${session?.token}`, // change token name if different
            },
            body: JSON.stringify({
                message: text,
            }),
        });

        const data = await response.json();

        console.log("chat response:", data);

        setMessages((current) => [
            ...current,
            {
                id: nextMessageIdRef.current++,
                role: "assistant",
                text: data.answer ?? data.message ?? "No response text found.", 
            },
        ]);
    };

  const taskDisplayIds = useMemo(() => {
    const sorted = [...tasks].sort((left, right) => {
      const leftDate = Date.parse(left.createdAtUtc);
      const rightDate = Date.parse(right.createdAtUtc);
      return leftDate - rightDate;
    });

    const map = new Map<string, string>();
    sorted.forEach((task, index) => {
      map.set(task.id, `TASK-${String(index + 1).padStart(3, '0')}`);
    });

    return map;
  }, [tasks]);

  const taskById = useMemo(() => new Map(tasks.map((task) => [task.id, task])), [tasks]);

  const usersById = useMemo(
    () => new Map(users.map((user) => [user.id.toLowerCase(), user.displayName])),
    [users],
  );

  const assignableUsers = useMemo(() => {
    if (!projectId) {
      return [];
    }

    const participantIds = new Set([
      ...(project?.memberIds ?? []).map((id) => id.toLowerCase()),
      ...(project?.createdById ? [project.createdById.toLowerCase()] : []),
    ]);

    return users.filter((user) => participantIds.has(user.id.toLowerCase()));
  }, [project?.memberIds, project?.createdById, projectId, users]);

  const resolveUserDisplayName = (userId: string | null) => {
    if (!userId) {
      return 'Unassigned';
    }

    return usersById.get(userId.toLowerCase()) ?? `User ${userId.slice(0, 6)}`;
  };

  const boardColumns = useMemo<BoardColumn[]>(() => {
    const toCard = (task: TaskItem): TaskCard => ({
      taskId: task.id,
      ticket: taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`,
      title: task.title,
      description: truncateText(task.description ?? '', MAX_TASK_DESCRIPTION_LENGTH),
      owner: resolveUserDisplayName(task.assigneeId),
      priority: priorityLabelMap[task.priority],
      estimate: formatEstimate(task.estimateMinutes),
    });

    const byColumn = new Map<BoardColumn['id'], TaskCard[]>([
      ['ready', []],
      ['in-progress', []],
      ['review', []],
      ['done', []],
    ]);

    tasks.forEach((task) => {
      const target =
        task.status === 'done' ? 'done'
        : task.status === 'in-progress' ? 'in-progress'
        : task.status === 'review' ? 'review'
        : 'ready';
      byColumn.get(target)?.push(toCard(task));
    });

    return columnConfig.map((column) => ({
      ...column,
      tasks: byColumn.get(column.id) ?? [],
    }));
  }, [taskDisplayIds, tasks, usersById]);

  const activeEditTask = editTaskId ? taskById.get(editTaskId) ?? null : null;
  const activeDetailTask = detailTaskId ? taskById.get(detailTaskId) ?? null : null;
  const activeDeleteTask = deleteTaskId ? taskById.get(deleteTaskId) ?? null : null;
  const {
    data: activeComments = [],
    isLoading: isCommentsLoading,
    isError: isCommentsError,
    error: commentsError,
  } = useCommentsQuery(activeDetailTask?.id ?? null);
  const activeCommentDraft = activeDetailTask ? commentDraftByTask[activeDetailTask.id] ?? '' : '';
  const activeDeleteComment = deleteCommentId ? activeComments.find((comment) => comment.id === deleteCommentId) ?? null : null;

  const handleAssignUser = async (userId: string) => {
    if (!activeDetailTask) {
      return;
    }

    setAssigneeError(null);

    try {
      await assignUserMutation.mutateAsync({
        taskId: activeDetailTask.id,
        userId,
      });
      setIsAssigneePickerOpen(false);
    } catch (error) {
      setAssigneeError(error instanceof Error ? error.message : 'Unable to update assignee.');
    }
  };

  const handleRemoveUser = async () => {
    if (!activeDetailTask) {
      return;
    }

    setAssigneeError(null);

    try {
      await assignUserMutation.mutateAsync({
        taskId: activeDetailTask.id,
        userId: '',
      });
      setIsAssigneePickerOpen(false);
    } catch (error) {
      setAssigneeError(error instanceof Error ? error.message : 'Unable to update assignee.');
    }
  };

  const totalTasks = boardColumns.reduce((total, column) => total + column.tasks.length, 0);
  const inProgressCount = boardColumns.find((column) => column.id === 'in-progress')?.tasks.length ?? 0;

  useEffect(() => {
    setContent({
      title: 'Board',
      description: 'A structured Kanban surface for prioritization, flow management, and delivery review.',
      meta: (
        <>
          <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
            {totalTasks} tickets active
          </Badge>
          <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
            {inProgressCount} in progress
          </Badge>
          <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
            {boardColumns.length} columns
          </Badge>
        </>
      ),
      actions: (
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            className="border-border/70 bg-background/80 shadow-sm"
            onClick={() => setCreateColumnId('ready')}
          >
            <Plus className="mr-2 h-4 w-4" />
            Add ticket
          </Button>
        </div>
      ),
    });

    return () => setContent({});
  }, [boardColumns.length, inProgressCount, setContent, totalTasks]);

  useEffect(() => {
    setEditingCommentId(null);
    setDeleteCommentId(null);
    setIsAssigneePickerOpen(false);
    setAssigneeError(null);
    setIsEstimateEditing(false);
    setEstimateDraft('');
    setEstimateError(null);

    if (!detailTaskId) {
      setCommentDraftByTask({});
      setCommentEditDrafts({});
    }
  }, [detailTaskId]);

  return (
    <section className="relative space-y-6">
      <CreateTaskModal
        isOpen={createColumnId !== null}
        onClose={() => setCreateColumnId(null)}
        projectId={projectId ?? null}
        defaultStatus={createColumnId ? columnStatusMap[createColumnId] : 'Open'}
        columnLabel={createColumnId ? columnConfig.find((column) => column.id === createColumnId)?.title ?? 'Ready' : 'Ready'}
        assignableUsers={assignableUsers}
        epics={epics}
      />

      <EditTaskModal
        isOpen={editTaskId !== null}
        onClose={() => setEditTaskId(null)}
        onSave={() => {
          setEditTaskId(null);
          setDetailTaskId(null);
        }}
        task={activeEditTask}
        assignableUsers={assignableUsers}
        epics={epics}
      />

      <DeleteTaskModal
        isOpen={deleteTaskId !== null}
        taskTitle={activeDeleteTask?.title ?? 'this ticket'}
        onClose={() => setDeleteTaskId(null)}
        onConfirm={async () => {
          if (!activeDeleteTask) {
            return;
          }

          await deleteTaskMutation.mutateAsync({ taskId: activeDeleteTask.id });
          setDeleteTaskId(null);
          setDetailTaskId(null);
        }}
        isPending={deleteTaskMutation.isPending}
      />

      <DeleteCommentModal
        isOpen={deleteCommentId !== null}
        onClose={() => setDeleteCommentId(null)}
        onConfirm={async () => {
          if (!activeDeleteComment) {
            return;
          }

          await deleteCommentMutation.mutateAsync({
            taskId: activeDeleteComment.taskId,
            commentId: activeDeleteComment.id,
          });
          setDeleteCommentId(null);
        }}
        isPending={deleteCommentMutation.isPending}
      />

      {activeDetailTask ? createPortal(
        <>
          <button
            type="button"
            className="fixed inset-0 z-30 bg-black/30"
            aria-label="Close task details"
            onClick={() => {
              if (editTaskId) {
                return;
              }

              setDetailTaskId(null);
            }}
          />
          <aside className="fixed right-0 top-0 z-40 flex h-full w-full max-w-[34rem] flex-col border-l border-border/70 bg-background/95 shadow-2xl backdrop-blur-sm">
            <div className="flex items-start justify-between gap-4 border-b border-border/70 p-5">
              <div className="space-y-2">
                <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                  {taskDisplayIds.get(activeDetailTask.id) ?? `TASK-${activeDetailTask.id.slice(0, 6).toUpperCase()}`}
                </Badge>
                <h2 className="text-lg font-semibold leading-7 text-foreground">{activeDetailTask.title}</h2>
              </div>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => setEditTaskId(activeDetailTask.id)}
                >
                  Edit
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => {
                    if (editTaskId) {
                      return;
                    }

                    setDetailTaskId(null);
                  }}
                  aria-label="Close task details"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>

            <div className="flex-1 space-y-6 overflow-y-auto p-5">
              <div className="flex flex-wrap items-center gap-2">
                <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                  {statusLabelMap[activeDetailTask.status]}
                </Badge>
                <Badge className={priorityBadgeClass(priorityLabelMap[activeDetailTask.priority])}>
                  {priorityLabelMap[activeDetailTask.priority]}
                </Badge>
              </div>

              <div className="grid gap-3 text-sm text-muted-foreground">
                <div className="flex items-center justify-between gap-2">
                  <span>Assignee</span>
                  <div className="flex items-center gap-2">
                    <span className="text-foreground">
                      {resolveUserDisplayName(activeDetailTask.assigneeId)}
                    </span>
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      className={cn(
                        'h-8 w-8',
                        isAssigneePickerOpen ? 'border-border/70 text-foreground hover:bg-background/80' : '',
                      )}
                      onClick={() => setIsAssigneePickerOpen((current) => !current)}
                      aria-label={isAssigneePickerOpen ? 'Close assignee picker' : 'Change assignee'}
                    >
                      {isAssigneePickerOpen
                        ? <Minus className="h-4 w-4" />
                        : activeDetailTask.assigneeId
                          ? <ArrowLeftRight className="h-4 w-4" />
                          : <UserPlus className="h-4 w-4" />}
                    </Button>
                  </div>
                </div>
                {isAssigneePickerOpen ? (
                  <div className="rounded-2xl border border-border/70 bg-background/80 p-3">
                    <MemberAssigneePicker
                      members={assignableUsers}
                      selectedAssigneeId={activeDetailTask.assigneeId ?? ''}
                      onAssign={handleAssignUser}
                      onRemove={handleRemoveUser}
                      searchInputId="detail-task-assignee"
                      isBusy={assignUserMutation.isPending}
                    />
                    <p className="mt-2 text-xs text-muted-foreground">
                      Only project members can be assigned.
                    </p>
                    {assigneeError ? <p className="mt-2 text-xs text-rose-700">{assigneeError}</p> : null}
                  </div>
                ) : null}
                {activeDetailTask.epicId && (
                  <div className="flex items-center justify-between gap-2">
                    <span>Epic</span>
                    <span className="text-foreground">
                      {epics.find((e) => e.id === activeDetailTask.epicId)?.name ?? 'Unknown'}
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
                              const minutes = estimateDraft.trim() ? parseEstimate(estimateDraft) : null;
                              if (estimateDraft.trim() && minutes === null) {
                                setEstimateError('Use 10m, 2h, or 1d.');
                                return;
                              }
                              await setEstimateMutation.mutateAsync({ taskId: activeDetailTask.id, estimateMinutes: minutes });
                              setIsEstimateEditing(false);
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
                          onClick={async () => {
                            const minutes = estimateDraft.trim() ? parseEstimate(estimateDraft) : null;
                            if (estimateDraft.trim() && minutes === null) {
                              setEstimateError('Use 10m, 2h, or 1d.');
                              return;
                            }
                            await setEstimateMutation.mutateAsync({ taskId: activeDetailTask.id, estimateMinutes: minutes });
                            setIsEstimateEditing(false);
                          }}
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
                          onClick={() => { setIsEstimateEditing(false); setEstimateError(null); }}
                          aria-label="Cancel estimate"
                        >
                          <Minus className="h-3.5 w-3.5" />
                        </Button>
                      </div>
                    ) : (
                      <>
                        <span className={cn('text-foreground', !activeDetailTask.estimateMinutes && 'text-muted-foreground')}>
                          {formatEstimate(activeDetailTask.estimateMinutes)}
                        </span>
                        <Button
                          type="button"
                          variant="outline"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => {
                            setEstimateDraft(minutesToEditValue(activeDetailTask.estimateMinutes));
                            setEstimateError(null);
                            setIsEstimateEditing(true);
                          }}
                          aria-label={activeDetailTask.estimateMinutes ? 'Change estimate' : 'Add estimate'}
                        >
                          {activeDetailTask.estimateMinutes ? <ArrowLeftRight className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
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
                    {resolveUserDisplayName(activeDetailTask.createdById)}
                  </span>
                </div>
                {activeDetailTask.updatedById && (
                  <div className="flex items-center justify-between gap-2">
                    <span>Last edited by</span>
                    <span className="text-foreground">
                      {resolveUserDisplayName(activeDetailTask.updatedById)}
                    </span>
                  </div>
                )}
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-foreground">Description</p>
                <p className="whitespace-pre-wrap text-sm leading-6 text-muted-foreground break-words">
                  {activeDetailTask.description?.trim() ? activeDetailTask.description : 'No description yet.'}
                </p>
              </div>

              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-medium text-foreground">Comments</p>
                  <span className="text-xs text-muted-foreground">{activeComments.length}</span>
                </div>
                <div className="space-y-3">
                  {isCommentsLoading ? (
                    <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
                      Loading comments...
                    </div>
                  ) : isCommentsError ? (
                    <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
                      {commentsError instanceof Error ? commentsError.message : 'Unable to load comments.'}
                    </div>
                  ) : activeComments.length === 0 ? (
                    <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
                      No comments yet.
                    </div>
                  ) : (
                    activeComments.map((comment) => (
                      <div key={comment.id} className="rounded-2xl border border-border/70 bg-background/80 p-3">
                        <div className="flex items-start justify-between gap-2">
                          <div className="flex items-center gap-2">
                            <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-[10px] font-semibold text-primary">
                              {resolveUserDisplayName(comment.userId).slice(0, 2).toUpperCase()}
                            </div>
                            <span className="text-xs font-medium text-foreground">
                              {resolveUserDisplayName(comment.userId)}
                            </span>
                          </div>
                          {(session?.role === 'Admin' || comment.userId?.toLowerCase() === session?.userId?.toLowerCase()) && (
                            <div className="flex items-center gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-7 w-7"
                                onClick={() => {
                                  setEditingCommentId(comment.id);
                                  setCommentEditDrafts((current) => ({
                                    ...current,
                                    [comment.id]: comment.content,
                                  }));
                                }}
                                aria-label="Edit comment"
                              >
                                <Pencil className="h-3.5 w-3.5" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-7 w-7 text-rose-600 hover:text-rose-700"
                                onClick={() => setDeleteCommentId(comment.id)}
                                aria-label="Delete comment"
                              >
                                <X className="h-3.5 w-3.5" />
                              </Button>
                            </div>
                          )}
                        </div>
                        {editingCommentId === comment.id ? (
                          <div className="mt-2 space-y-2">
                            <textarea
                              value={commentEditDrafts[comment.id] ?? comment.content}
                              onChange={(event) =>
                                setCommentEditDrafts((current) => ({
                                  ...current,
                                  [comment.id]: event.target.value,
                                }))
                              }
                              className="min-h-20 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                            />
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => setEditingCommentId(null)}
                              >
                                Cancel
                              </Button>
                              <Button
                                size="sm"
                                onClick={async () => {
                                  const content = (commentEditDrafts[comment.id] ?? '').trim();
                                  if (!content) {
                                    return;
                                  }

                                  await updateCommentMutation.mutateAsync({
                                    taskId: comment.taskId,
                                    commentId: comment.id,
                                    content,
                                  });
                                  setEditingCommentId(null);
                                }}
                                disabled={updateCommentMutation.isPending}
                              >
                                {updateCommentMutation.isPending ? 'Saving...' : 'Save'}
                              </Button>
                            </div>
                          </div>
                        ) : (
                          <>
                            <p className="mt-1 whitespace-pre-wrap text-sm text-foreground break-words">{comment.content}</p>
                            <p className="mt-2 text-[0.7rem] text-muted-foreground">
                              {new Date(comment.createdAtUtc).toLocaleString()}
                            </p>
                          </>
                        )}
                      </div>
                    ))
                  )}
                </div>
                <div className="space-y-2">
                  <label className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground" htmlFor="comment-input">
                    Add comment
                  </label>
                  <textarea
                    id="comment-input"
                    value={activeCommentDraft}
                    onChange={(event) => {
                      if (!activeDetailTask) {
                        return;
                      }

                      setCommentDraftByTask((current) => ({
                        ...current,
                        [activeDetailTask.id]: event.target.value,
                      }));
                    }}
                    placeholder="Write a comment..."
                    maxLength={2000}
                    className="min-h-20 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                  />
                  <div className="flex justify-end">
                    <Button
                      variant="outline"
                      className="border-border/70 bg-background/80 shadow-sm"
                      disabled={!activeCommentDraft.trim() || createCommentMutation.isPending}
                      onClick={async () => {
                        if (!activeDetailTask) {
                          return;
                        }

                        const content = activeCommentDraft.trim();
                        if (!content) {
                          return;
                        }

                        await createCommentMutation.mutateAsync({
                          taskId: activeDetailTask.id,
                          content,
                        });
                        setCommentDraftByTask((current) => ({
                          ...current,
                          [activeDetailTask.id]: '',
                        }));
                      }}
                    >
                      {createCommentMutation.isPending ? 'Posting...' : 'Post comment'}
                    </Button>
                  </div>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-end border-t border-border/70 p-4">
              <Button
                variant="destructive"
                onClick={() => setDeleteTaskId(activeDetailTask.id)}
              >
                Delete
              </Button>
            </div>
          </aside>
        </>,
        document.body,
      ) : null}

      {isError ? (
        <ErrorState
          title="Unable to load tasks"
          description={error instanceof Error ? error.message : 'Check your connection and try again.'}
          onRetry={() => refetch()}
        />
      ) : null}

      <div className="grid gap-4 lg:grid-cols-2 xl:grid-cols-4">
        {boardColumns.map((column) => (
          <Card key={column.title} className="border-border/70 bg-muted/20 shadow-sm">
            <CardHeader className="space-y-3 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <CardTitle>{column.title}</CardTitle>
                  <CardDescription className="mt-1">{column.description}</CardDescription>
                </div>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8 border-border/70 bg-background/70 text-muted-foreground"
                    onClick={() => setCreateColumnId(column.id)}
                    aria-label={`Add ticket to ${column.title}`}
                  >
                    <Plus className="h-4 w-4" />
                  </Button>
                  <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                    {column.tasks.length}
                  </Badge>
                </div>
              </div>
              <Separator />
            </CardHeader>

            <CardContent className="space-y-3">
              {isLoading ? (
                <div
                  className="flex items-center justify-center rounded-2xl border border-dashed border-border/70 bg-background/60 p-6"
                  role="status"
                  aria-live="polite"
                >
                  <div className="h-9 w-9 animate-spin rounded-full border-2 border-border/60 border-t-foreground/70" aria-hidden="true" />
                  <span className="sr-only">Loading tasks</span>
                </div>
              ) : isError ? (
                <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-4 text-sm text-muted-foreground">
                  Unable to load tasks.
                </div>
              ) : column.tasks.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-4 text-sm text-muted-foreground">
                  No tasks here yet.
                </div>
              ) : (
                column.tasks.map((task) => (
                  <button
                    key={task.taskId}
                    type="button"
                    className={cn(
                      'w-full cursor-pointer rounded-xl border border-l-4 border-border/40 p-4 text-left',
                      'bg-white dark:bg-card',
                      'shadow-md transition-all duration-150',
                      'hover:-translate-y-0.5 hover:shadow-lg',
                      priorityBorderClass(task.priority),
                    )}
                    onClick={() => setDetailTaskId(task.taskId)}
                  >
                      <div className="flex items-start justify-between gap-3">
                        <div className="min-w-0 flex-1 space-y-2">
                          <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                            {task.ticket}
                          </Badge>
                          <h3 className="text-sm font-medium leading-6 text-foreground">{task.title}</h3>
                          {task.description ? (
                            <p className="max-w-full text-xs leading-5 text-muted-foreground break-all whitespace-normal">
                              {task.description}
                            </p>
                          ) : null}
                        </div>
                        <Badge className={priorityBadgeClass(task.priority)}>
                          {task.priority}
                        </Badge>
                      </div>

                      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 text-xs text-muted-foreground">
                        <span>{task.owner}</span>
                        <span>{task.estimate}</span>
                      </div>

                  </button>
                ))
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="fixed bottom-5 right-5 z-30 sm:bottom-6 sm:right-6">
        {isAssistantOpen ? (
          <Card id="board-ai-chat" className="mb-3 w-[min(92vw,360px)] border-border/70 bg-card/95 shadow-xl backdrop-blur-sm">
            <CardHeader className="space-y-3 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <CardTitle className="flex items-center gap-2">
                    <Bot className="h-4 w-4 text-muted-foreground" />
                    AI Assistant
                  </CardTitle>
                  <CardDescription className="mt-1">Useful for summaries, grouping, and quick board questions.</CardDescription>
                </div>
                <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                  Ready
                </Badge>
              </div>
              <Separator />
            </CardHeader>

            <CardContent className="space-y-4">
              <div className="max-h-56 space-y-3 overflow-y-auto pr-1">
                {messages.map((message) => (
                  <div
                    key={message.id}
                    className={cn(
                      'max-w-[92%] rounded-2xl border px-3 py-2.5 text-sm leading-6 shadow-sm',
                      message.role === 'user'
                        ? 'ml-auto border-primary/20 bg-primary/8 text-foreground'
                        : 'border-border/70 bg-muted/40 text-foreground',
                    )}
                  >
                    {message.text}
                  </div>
                ))}
              </div>

              <Separator />

              <div className="space-y-3">
                <Input
                  value={input}
                  placeholder="Ask about blockers, priorities, or grouping..."
                  onChange={(event) => setInput(event.target.value)}
                  onKeyDown={(event) => {
                    if (event.key === 'Enter') {
                      sendMessage();
                    }
                  }}
                />
                <Button className="w-full shadow-sm" onClick={sendMessage}>
                  <SendHorizontal className="mr-2 h-4 w-4" />
                  Send
                </Button>
              </div>
            </CardContent>
          </Card>
        ) : null}

        <Button
          type="button"
          aria-expanded={isAssistantOpen}
          aria-controls="board-ai-chat"
          className="rounded-full px-4 shadow-lg"
          onClick={() => setIsAssistantOpen((current) => !current)}
        >
          <Bot className="mr-2 h-4 w-4" />
          {isAssistantOpen ? 'Close assistant' : 'AI Assistant'}
        </Button>
      </div>
    </section>
  );
}
