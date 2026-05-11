import { apiClient } from '@/lib/apiClient';
import type {
  ChangeTaskPriorityInput,
  ChangeTaskStatusInput,
  CreateTaskInput,
  TaskFilters,
  TaskItem,
  TaskPriority,
  TaskStatus,
  UpdateTaskInput,
} from './taskTypes';

type TaskItemResponse = {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priority: string;
  projectId: string;
  assigneeId: string | null;
  epicId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

const normalizeStatus = (value: string | null | undefined): TaskStatus => {
  if (!value) {
    return 'unknown';
  }

  const normalized = value.trim().toLowerCase();
  if (normalized === 'open' || normalized === 'todo' || normalized === 'backlog') {
    return 'todo';
  }
  if (normalized === 'in progress' || normalized === 'in-progress' || normalized === 'progress') {
    return 'in-progress';
  }
  if (normalized === 'done' || normalized === 'closed' || normalized === 'complete') {
    return 'done';
  }

  return 'unknown';
};

const normalizePriority = (value: string | null | undefined): TaskPriority => {
  if (!value) {
    return 'unknown';
  }

  const normalized = value.trim().toLowerCase();
  if (normalized === 'high') {
    return 'high';
  }
  if (normalized === 'medium') {
    return 'medium';
  }
  if (normalized === 'low') {
    return 'low';
  }

  return 'unknown';
};

const mapTaskItem = (task: TaskItemResponse): TaskItem => ({
  ...task,
  status: normalizeStatus(task.status),
  priority: normalizePriority(task.priority),
});

export async function fetchTasks(filters: TaskFilters): Promise<TaskItem[]> {
  const params = new URLSearchParams();

  if (filters.search) {
    params.set('search', filters.search);
  }
  if (filters.status) {
    params.set('status', filters.status);
  }
  if (filters.priority) {
    params.set('priority', filters.priority);
  }
  if (filters.assigneeId) {
    params.set('assigneeId', filters.assigneeId);
  }
  if (filters.epicId) {
    params.set('epicId', filters.epicId);
  }

  const query = params.toString();
  const result = await apiClient.get<TaskItemResponse[]>(`/tasks${query ? `?${query}` : ''}`);
  return result.map(mapTaskItem);
}

export async function createTask(input: CreateTaskInput): Promise<TaskItem> {
  const result = await apiClient.post<TaskItemResponse>('/tasks', input);
  return mapTaskItem(result);
}

export async function updateTask(input: UpdateTaskInput): Promise<void> {
  const { taskId, ...payload } = input;
  await apiClient.put<void>(`/tasks/${taskId}`, payload);
}

export async function changeTaskStatus(input: ChangeTaskStatusInput): Promise<void> {
  const { taskId, status } = input;
  await apiClient.patch<void>(`/tasks/${taskId}/status`, { status });
}

export async function changeTaskPriority(input: ChangeTaskPriorityInput): Promise<void> {
  const { taskId, priority } = input;
  await apiClient.patch<void>(`/tasks/${taskId}/priority`, { priority });
}
