import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createAdminUser, deleteAdminUser, fetchAdminUsers, userQueryKeys } from './userApi';
import type { CreateAdminUserInput, DeleteAdminUserInput } from './userTypes';

export function useAdminUsersQuery() {
  return useQuery({
    queryKey: userQueryKeys.adminList(),
    queryFn: fetchAdminUsers,
  });
}

export function useCreateAdminUserMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateAdminUserInput) => createAdminUser(input),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: userQueryKeys.all });
    },
  });
}

export function useDeleteAdminUserMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: DeleteAdminUserInput) => deleteAdminUser(input.userId),
    onSuccess: () => {
      return queryClient.invalidateQueries({ queryKey: userQueryKeys.all });
    },
  });
}
