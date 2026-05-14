import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Bot, Pencil, Plus, SendHorizontal, X } from 'lucide-react';
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
import { useDeleteTaskMutation, useTasksQuery, type TaskItem, type TaskPriority } from '@/features/tasks';
import { cn } from '@/lib/utils';

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
  id: 'backlog' | 'in-progress' | 'done';
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
  done: 'Done',
  unknown: 'Open',
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
    id: 'backlog',
    title: 'Backlog',
    description: 'Ready for triage and sizing.',
  },
  {
    id: 'in-progress',
    title: 'In progress',
    description: 'Actively being implemented.',
  },
  {
    id: 'done',
    title: 'Done',
    description: 'Shipped and ready to verify.',
  },
];

const columnStatusMap: Record<BoardColumn['id'], string> = {
  backlog: 'Open',
  'in-progress': 'In Progress',
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
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  const { data: tasks = [], isLoading, isError, error, refetch } = useTasksQuery({
    projectId: projectId ?? null,
  });
  const createCommentMutation = useCreateCommentMutation();
  const updateCommentMutation = useUpdateCommentMutation();
  const deleteCommentMutation = useDeleteCommentMutation();
  const deleteTaskMutation = useDeleteTaskMutation();
  const [createColumnId, setCreateColumnId] = useState<BoardColumn['id'] | null>(null);
  const [editTaskId, setEditTaskId] = useState<string | null>(null);
  const [detailTaskId, setDetailTaskId] = useState<string | null>(null);
  const [deleteTaskId, setDeleteTaskId] = useState<string | null>(null);
  const [commentDraftByTask, setCommentDraftByTask] = useState<Record<string, string>>({});
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [commentEditDrafts, setCommentEditDrafts] = useState<Record<string, string>>({});
  const [deleteCommentId, setDeleteCommentId] = useState<string | null>(null);
  const [isAssistantOpen, setIsAssistantOpen] = useState(false);
  const [input, setInput] = useState('');
  const nextMessageIdRef = useRef(2);
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 1,
      role: 'assistant',
      text: 'I can help sort tasks, spot blockers, or summarize this board once the data is connected.',
    },
  ]);

  const sendMessage = () => {
    const text = input.trim();
    if (!text) {
      return;
    }

    const userMessageId = nextMessageIdRef.current;
    nextMessageIdRef.current += 1;
    const assistantMessageId = nextMessageIdRef.current;
    nextMessageIdRef.current += 1;

    setMessages((current) => [
      ...current,
      { id: userMessageId, role: 'user', text },
      {
        id: assistantMessageId,
        role: 'assistant',
        text: 'Noted. This board assistant will later connect to a real workflow endpoint.',
      },
    ]);
    setInput('');
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

  const boardColumns = useMemo<BoardColumn[]>(() => {
    const toCard = (task: TaskItem): TaskCard => ({
      taskId: task.id,
      ticket: taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`,
      title: task.title,
      description: truncateText(task.description ?? '', MAX_TASK_DESCRIPTION_LENGTH),
      owner: task.assigneeId ? `User ${task.assigneeId.slice(0, 6)}` : 'Unassigned',
      priority: priorityLabelMap[task.priority],
      estimate: 'n/a',
    });

    const byColumn = new Map<BoardColumn['id'], TaskCard[]>([
      ['backlog', []],
      ['in-progress', []],
      ['done', []],
    ]);

    tasks.forEach((task) => {
      const target = task.status === 'done' ? 'done' : task.status === 'in-progress' ? 'in-progress' : 'backlog';
      byColumn.get(target)?.push(toCard(task));
    });

    return columnConfig.map((column) => ({
      ...column,
      tasks: byColumn.get(column.id) ?? [],
    }));
  }, [taskDisplayIds, tasks]);

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
            onClick={() => setCreateColumnId('backlog')}
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
        columnLabel={createColumnId ? columnConfig.find((column) => column.id === createColumnId)?.title ?? 'Backlog' : 'Backlog'}
      />

      <EditTaskModal
        isOpen={editTaskId !== null}
        onClose={() => setEditTaskId(null)}
        onSave={() => {
          setEditTaskId(null);
          setDetailTaskId(null);
        }}
        task={activeEditTask}
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

      {activeDetailTask ? (
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
                  TASK-{activeDetailTask.id.slice(0, 6).toUpperCase()}
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
                  <span>Owner</span>
                  <span className="text-foreground">
                    {activeDetailTask.assigneeId ? `User ${activeDetailTask.assigneeId.slice(0, 6)}` : 'Unassigned'}
                  </span>
                </div>
                <div className="flex items-center justify-between gap-2">
                  <span>Estimate</span>
                  <span className="text-foreground">n/a</span>
                </div>
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
                          <p className="text-xs text-muted-foreground">{comment.userId ? `User ${comment.userId.slice(0, 6)}` : 'Unassigned'}</p>
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
                          userId: null,
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
        </>
      ) : null}

      {isError ? (
        <ErrorState
          title="Unable to load tasks"
          description={error instanceof Error ? error.message : 'Check your connection and try again.'}
          onRetry={() => refetch()}
        />
      ) : null}

      <div className="grid gap-4 lg:grid-cols-3">
        {boardColumns.map((column) => (
          <Card key={column.title} className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
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
                column.tasks.map((task, index) => (
                  <div key={task.taskId}>
                    {index > 0 ? <Separator className="mb-3" /> : null}
                    <button
                      type="button"
                      className="w-full cursor-pointer rounded-2xl border border-border/70 bg-background/80 p-4 text-left shadow-sm transition-shadow hover:shadow-md"
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
                  </div>
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
