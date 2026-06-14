import type { TaskItem, TaskPriority } from '@/features/tasks';
import { cn } from '@/lib/utils';

export type TaskCard = {
  taskId: string;
  ticket: string;
  title: string;
  description: string;
  owner: string;
  priority: 'High' | 'Medium' | 'Low' | 'Unknown';
  estimate: string;
};

export type BoardColumn = {
  id: 'ready' | 'in-progress' | 'review' | 'done';
  title: string;
  description: string;
  tasks: TaskCard[];
};

export const priorityLabelMap: Record<TaskPriority, TaskCard['priority']> = {
  high: 'High',
  medium: 'Medium',
  low: 'Low',
  unknown: 'Unknown',
};

export const statusLabelMap: Record<TaskItem['status'], string> = {
  todo: 'Ready',
  'in-progress': 'In Progress',
  review: 'Review',
  done: 'Done',
  unknown: 'Ready',
};

export const columnConfig: Array<Omit<BoardColumn, 'tasks'>> = [
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

export const columnStatusMap: Record<BoardColumn['id'], string> = {
  ready: 'Ready',
  'in-progress': 'In Progress',
  review: 'Review',
  done: 'Done',
};

export const columnTaskStatusMap: Record<
  BoardColumn['id'],
  TaskItem['status']
> = {
  ready: 'todo',
  'in-progress': 'in-progress',
  review: 'review',
  done: 'done',
};

export const statusColumnId = (
  status: TaskItem['status'],
): BoardColumn['id'] =>
  status === 'done'
    ? 'done'
    : status === 'in-progress'
      ? 'in-progress'
      : status === 'review'
        ? 'review'
        : 'ready';

export const priorityBorderClass = (priority: TaskCard['priority']) => {
  if (priority === 'High') return 'border-l-rose-500';
  if (priority === 'Medium') return 'border-l-amber-500';
  if (priority === 'Low') return 'border-l-slate-400';
  return 'border-l-border/60';
};

export const priorityBadgeClass = (priority: TaskCard['priority']) => {
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

export const taskCardClass = (priority: TaskCard['priority']) =>
  cn(
    'w-full cursor-pointer rounded-xl border border-l-4 border-border/40 p-4 text-left',
    'bg-white dark:bg-card',
    'shadow-md transition-all duration-150',
    'hover:-translate-y-0.5 hover:shadow-lg',
    priorityBorderClass(priority),
  );

export const MAX_TASK_DESCRIPTION_LENGTH = 145;

export const truncateText = (value: string, maxLength: number) => {
  const trimmed = value.trim();
  if (!trimmed || trimmed.length <= maxLength) return trimmed;

  const cut = trimmed.slice(0, maxLength);
  const lastSpace = cut.lastIndexOf(' ');
  return `${(lastSpace > 0 ? cut.slice(0, lastSpace) : cut).trimEnd()}...`;
};
