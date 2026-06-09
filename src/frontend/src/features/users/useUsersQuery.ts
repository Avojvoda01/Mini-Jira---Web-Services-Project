import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { changeAdminUserRole, changeUserPassword, createAdminUser, deleteAdminUser, deleteOwnAccount, fetchAdminUsers, updateUserProfile, userQueryKeys } from './userApi';
import type { ChangePasswordInput, CreateAdminUserInput, DeleteAdminUserInput, UpdateProfileInput } from './userTypes';

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

export function useUpdateProfileMutation() {
  return useMutation({
    mutationFn: ({ userId, input }: { userId: string; input: UpdateProfileInput }) =>
      updateUserProfile(userId, input),
  });
}

export function useChangePasswordMutation() {
  return useMutation({
    mutationFn: ({ userId, input }: { userId: string; input: ChangePasswordInput }) =>
      changeUserPassword(userId, input),
  });
}

export function useChangeUserRoleMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) =>
      changeAdminUserRole(userId, role),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userQueryKeys.all }),
  });
}

export function useDeleteOwnAccountMutation() {
  return useMutation({
    mutationFn: (userId: string) => deleteOwnAccount(userId),
  });
}
