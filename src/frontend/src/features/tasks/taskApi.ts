import type { TaskFilters, TaskItem } from './taskTypes';

export async function fetchTasks(filters: TaskFilters): Promise<TaskItem[]> {
  void filters;
  return [];
}
