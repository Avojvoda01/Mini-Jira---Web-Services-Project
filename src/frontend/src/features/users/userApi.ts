import { apiClient } from '@/lib/apiClient';
import type { ChangePasswordInput, CreateAdminUserInput, UpdateProfileInput, UserDto } from './userTypes';

export const userQueryKeys = {
  all: ['users'] as const,
  adminList: () => [...userQueryKeys.all, 'admin-list'] as const,
};

export async function fetchAdminUsers(): Promise<UserDto[]> {
  return apiClient.get<UserDto[]>('/users');
}

export async function createAdminUser(input: CreateAdminUserInput): Promise<UserDto> {
  return apiClient.post<UserDto>('/users', input);
}

export async function deleteAdminUser(userId: string): Promise<void> {
  return apiClient.delete<void>(`/users/${userId}`);
}

export async function updateUserProfile(userId: string, input: UpdateProfileInput): Promise<UserDto> {
  return apiClient.patch<UserDto>(`/users/${userId}/profile`, input);
}

export async function changeUserPassword(userId: string, input: ChangePasswordInput): Promise<void> {
  return apiClient.patch<void>(`/users/${userId}/password`, input);
}

export async function changeAdminUserRole(userId: string, role: string): Promise<UserDto> {
  return apiClient.patch<UserDto>(`/users/${userId}/role`, { role });
}
