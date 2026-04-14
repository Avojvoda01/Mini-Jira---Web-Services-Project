export type TaskStatus = 'todo' | 'in-progress' | 'done' | 'unknown';

export type TaskPriority = 'low' | 'medium' | 'high' | 'unknown';

export type TaskItem = {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  assigneeId: string | null;
  projectId: string | null;
};

export type TaskFilters = {
  search: string;
  projectId: string | null;
};
