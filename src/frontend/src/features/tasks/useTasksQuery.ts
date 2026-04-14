import { useQuery } from '@tanstack/react-query';
import { fetchTasks } from './taskApi';

export const taskQueryKeys = {
  all: ['tasks'] as const,
  list: (filters: { search: string; projectId: null }) =>
    [...taskQueryKeys.all, 'list', filters] as const,
};

export function useTasksQuery() {
  const filters = { search: '', projectId: null } as const;

  return useQuery({
    queryKey: taskQueryKeys.list(filters),
    queryFn: () => fetchTasks(filters),
  });
}
