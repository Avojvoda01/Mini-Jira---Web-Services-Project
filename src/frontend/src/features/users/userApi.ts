import { apiClient } from '@/lib/apiClient';
import type { CreateAdminUserInput, UserDto } from './userTypes';

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
