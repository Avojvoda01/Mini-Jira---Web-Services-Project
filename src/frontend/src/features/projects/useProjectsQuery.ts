import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { changeProjectOwner, createProject, deleteProject, fetchProjectById, fetchProjects, projectQueryKeys, updateProject } from './projectApi';
import { addProjectMember, removeProjectMember } from './projectMemberApi';

export function useProjectsQuery() {
  return useQuery({
    queryKey: projectQueryKeys.list(),
    queryFn: fetchProjects,
  });
}

export function useProjectQuery(projectId: string | null) {
  return useQuery({
    queryKey: projectId ? projectQueryKeys.detail(projectId) : [...projectQueryKeys.all, 'detail', 'none'],
    queryFn: () => fetchProjectById(projectId as string),
    enabled: Boolean(projectId),
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

export function useAddProjectMemberMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: addProjectMember,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: projectQueryKeys.all }),
  });
}

export function useRemoveProjectMemberMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: removeProjectMember,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: projectQueryKeys.all }),
  });
}

export function useChangeProjectOwnerMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ projectId, newOwnerId }: { projectId: string; newOwnerId: string }) =>
      changeProjectOwner(projectId, newOwnerId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: projectQueryKeys.all }),
  });
}
