import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createEpic, deleteEpic, epicQueryKeys, fetchEpics, updateEpic } from './epicApi';
import type { EpicFilters } from './epicTypes';

const normalizeFilters = (filters?: Partial<EpicFilters>): EpicFilters => ({
  projectId: filters?.projectId ?? null,
});

export function useEpicsQuery(filters?: Partial<EpicFilters>) {
  const normalizedFilters = normalizeFilters(filters);

  return useQuery({
    queryKey: epicQueryKeys.list(normalizedFilters),
    queryFn: () => fetchEpics(normalizedFilters),
  });
}

export function useCreateEpicMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createEpic,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: epicQueryKeys.all });
    },
  });
}

export function useUpdateEpicMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateEpic,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: epicQueryKeys.all });
    },
  });
}

export function useDeleteEpicMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteEpic,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: epicQueryKeys.all });
    },
  });
}
