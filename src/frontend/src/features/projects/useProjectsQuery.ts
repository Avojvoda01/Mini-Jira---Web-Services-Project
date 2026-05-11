import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createProject, deleteProject, fetchProjects, projectQueryKeys, updateProject } from './projectApi';

export function useProjectsQuery() {
  return useQuery({
    queryKey: projectQueryKeys.list(),
    queryFn: fetchProjects,
  });
}

export function useCreateProjectMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createProject,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: projectQueryKeys.all });
    },
  });
}

export function useUpdateProjectMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateProject,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: projectQueryKeys.all });
    },
  });
}

export function useDeleteProjectMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteProject,
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: projectQueryKeys.all });
    },
  });
}
