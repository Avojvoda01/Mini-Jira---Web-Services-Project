import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createEpic, deleteEpic, epicQueryKeys, fetchEpics, updateEpic } from './epicApi';

export function useEpicsQuery() {
  return useQuery({
    queryKey: epicQueryKeys.list(),
    queryFn: fetchEpics,
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
