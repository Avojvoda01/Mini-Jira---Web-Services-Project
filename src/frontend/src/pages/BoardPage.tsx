import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Bot, Plus, SendHorizontal } from 'lucide-react';
import { CreateTaskModal } from '@/components/board/CreateTaskModal';
import { EditTaskModal } from '@/components/board/EditTaskModal';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorState } from '@/components/ui/ErrorState';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { useTasksQuery, type TaskItem, type TaskPriority } from '@/features/tasks';
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
  const [createColumnId, setCreateColumnId] = useState<BoardColumn['id'] | null>(null);
  const [editTaskId, setEditTaskId] = useState<string | null>(null);
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

  const taskById = useMemo(() => new Map(tasks.map((task) => [task.id, task])), [tasks]);

  const boardColumns = useMemo<BoardColumn[]>(() => {
    const visibleTasks = projectId ? tasks.filter((task) => task.projectId === projectId) : tasks;

    const toCard = (task: TaskItem): TaskCard => ({
      taskId: task.id,
      ticket: `TASK-${task.id.slice(0, 6).toUpperCase()}`,
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

    visibleTasks.forEach((task) => {
      const target = task.status === 'done' ? 'done' : task.status === 'in-progress' ? 'in-progress' : 'backlog';
      byColumn.get(target)?.push(toCard(task));
    });

    return columnConfig.map((column) => ({
      ...column,
      tasks: byColumn.get(column.id) ?? [],
    }));
  }, [projectId, tasks]);

  const activeEditTask = editTaskId ? taskById.get(editTaskId) ?? null : null;

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

  return (
    <section className="relative space-y-6">
      <CreateTaskModal
        isOpen={createColumnId !== null}
        onClose={() => setCreateColumnId(null)}
        projectId={projectId ?? null}
        defaultStatus={createColumnId ? columnStatusMap[createColumnId] : 'Open'}
        columnLabel={createColumnId ? columnConfig.find((column) => column.id === createColumnId)?.title ?? 'Backlog' : 'Backlog'}
      />

      <EditTaskModal isOpen={editTaskId !== null} onClose={() => setEditTaskId(null)} task={activeEditTask} />

      {isError ? (
        <ErrorState
          title="Unable to load tasks"
          description={error instanceof Error ? error.message : 'Check your connection and try again.'}
          onRetry={() => refetch()}
        />
      ) : null}

      {isLoading ? (
        <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-6 text-sm text-muted-foreground">
          Loading tasks...
        </div>
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
              {column.tasks.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-4 text-sm text-muted-foreground">
                  No tasks here yet.
                </div>
              ) : (
                column.tasks.map((task, index) => (
                  <div key={task.ticket}>
                    {index > 0 ? <Separator className="mb-3" /> : null}
                    <article className="rounded-2xl border border-border/70 bg-background/80 p-4 shadow-sm transition-shadow hover:shadow-md">
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
                        <Badge
                          className={
                            task.priority === 'High'
                              ? 'bg-rose-500/10 text-rose-700 hover:bg-rose-500/10'
                              : task.priority === 'Medium'
                                ? 'bg-amber-500/10 text-amber-700 hover:bg-amber-500/10'
                                : task.priority === 'Low'
                                  ? 'bg-slate-500/10 text-slate-700 hover:bg-slate-500/10'
                                  : 'bg-muted text-muted-foreground hover:bg-muted'
                          }
                        >
                          {task.priority}
                        </Badge>
                      </div>

                      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 text-xs text-muted-foreground">
                        <span>{task.owner}</span>
                        <span>{task.estimate}</span>
                      </div>

                      <div className="mt-3 flex justify-end">
                        <Button variant="outline" size="sm" onClick={() => setEditTaskId(task.taskId)}>
                          Edit
                        </Button>
                      </div>
                    </article>
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
