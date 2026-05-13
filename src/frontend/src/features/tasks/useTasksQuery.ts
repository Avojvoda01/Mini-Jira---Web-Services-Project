import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { changeTaskPriority, changeTaskStatus, createTask, deleteTask, fetchTasks, updateTask } from './taskApi';
import type {
  ChangeTaskPriorityInput,
  ChangeTaskStatusInput,
  CreateTaskInput,
  DeleteTaskInput,
  TaskFilters,
  UpdateTaskInput,
} from './taskTypes';

export const taskQueryKeys = {
  all: ['tasks'] as const,
  list: (filters: TaskFilters) => [...taskQueryKeys.all, 'list', filters] as const,
};

const normalizeFilters = (filters?: Partial<TaskFilters>): TaskFilters => ({
  search: filters?.search ?? '',
  status: filters?.status ?? null,
  priority: filters?.priority ?? null,
  assigneeId: filters?.assigneeId ?? null,
  epicId: filters?.epicId ?? null,
  projectId: filters?.projectId ?? null,
});

export function useTasksQuery(filters?: Partial<TaskFilters>) {
  const normalizedFilters = normalizeFilters(filters);

  return useQuery({
    queryKey: taskQueryKeys.list(normalizedFilters),
    queryFn: () => fetchTasks(normalizedFilters),
  });
}

export function useCreateTaskMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateTaskInput) => createTask(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: taskQueryKeys.all });
    },
  });
}

export function useChangeTaskStatusMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ChangeTaskStatusInput) => changeTaskStatus(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: taskQueryKeys.all });
    },
  });
}

export function useChangeTaskPriorityMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ChangeTaskPriorityInput) => changeTaskPriority(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: taskQueryKeys.all });
    },
  });
}

export function useUpdateTaskMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateTaskInput) => updateTask(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: taskQueryKeys.all });
    },
  });
}

export function useDeleteTaskMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: DeleteTaskInput) => deleteTask(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: taskQueryKeys.all });
    },
  });
}
