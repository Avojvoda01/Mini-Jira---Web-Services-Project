export type TaskStatus = 'todo' | 'in-progress' | 'done' | 'unknown';

export type TaskPriority = 'low' | 'medium' | 'high' | 'unknown';

export type TaskItem = {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  assigneeId: string | null;
  projectId: string;
  epicId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type CreateTaskInput = {
  title: string;
  description: string | null;
  projectId: string;
};

export type UpdateTaskInput = {
  taskId: string;
  title: string;
  description: string | null;
};

export type DeleteTaskInput = {
  taskId: string;
};

export type ChangeTaskStatusInput = {
  taskId: string;
  status: string;
};

export type ChangeTaskPriorityInput = {
  taskId: string;
  priority: string;
};

export type TaskFilters = {
  search: string;
  status: string | null;
  priority: string | null;
  assigneeId: string | null;
  epicId: string | null;
  projectId: string | null;
};
