import { useQuery } from '@tanstack/react-query';
import { fetchTasks } from './taskApi';

export const taskQueryKeys = {
  all: ['tasks'] as const,
};

export function useTasksQuery() {
  return useQuery({
    queryKey: taskQueryKeys.all,
    queryFn: () => fetchTasks({ search: '', projectId: null }),
  });
}
